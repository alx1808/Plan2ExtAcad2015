using System;
using System.Collections.Generic;
using System.Linq;
using Plan2Ext.ObjectFilter;
#if BRX_APP
using  Bricscad.ApplicationServices;
using Teigha.DatabaseServices;
#elif ARX_APP
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.ApplicationServices.Core;
// ReSharper disable CommentTypo
#endif

namespace Plan2Ext
{
    internal static class BlockManager
    {
        public static IEnumerable<ObjectId> InsertXrefsAsBlocks(Database db, IEnumerable<ObjectId> xrefTableRecordIds)
        {
            var blockRefIds = new List<ObjectId>();
            using (var tr = db.TransactionManager.StartTransaction())
            {
                foreach (var oid in xrefTableRecordIds)
                {
                    var br = tr.GetObject(oid, OpenMode.ForRead) as BlockReference;
                    if (br == null) continue;
                    var bd = (BlockTableRecord)tr.GetObject(br.BlockTableRecord, OpenMode.ForRead);
                    if (bd.IsFromExternalReference)
                    {
                        var dwgPath = Application.GetSystemVariable("DWGPREFIX").ToString();
                        if (System.IO.Path.IsPathRooted(bd.PathName))
                        {
                            var blockOid = Globs.InsertDwg(bd.PathName, br.Position, br.Rotation, 1.0, br.Name + "_AS_BLOCK");
                            blockRefIds.Add(blockOid);
                        }
                        else
                        {
                            var blockOid = Globs.InsertDwg(System.IO.Path.GetFullPath(dwgPath + bd.PathName), br.Position, br.Rotation, 1.0, br.Name + "_AS_BLOCK");
                            blockRefIds.Add(blockOid);
                        }
                    }
                }

                tr.Commit();
            }

            return blockRefIds;
        }

        public static IEnumerable<ObjectId> ExplodeBlocks(
            Database db,
            ObjectId spaceId,
            IEnumerable<ObjectId> blockRefIds,
            bool deleteRef,
            bool deleteBtr,
            IObjectFilter objectFilter
            )
        {
            var newlyCreatedObjects = new List<ObjectId>();
            using (var transaction = db.TransactionManager.StartTransaction())
            {
                BlockTableRecord btr = (BlockTableRecord)transaction.GetObject(spaceId, OpenMode.ForWrite);
                foreach (var oid in blockRefIds)
                {
                    var dbObjectCollection = new DBObjectCollection();
                    var block = (BlockReference)transaction.GetObject(oid, OpenMode.ForRead);
                    block.Explode(dbObjectCollection);
                    var blockRefTableId = block.BlockTableRecord;
                    var filteredObjects = GetFilteredObjects(objectFilter, dbObjectCollection, transaction);
                    foreach (var dbObject in filteredObjects)
                    {
                        var ent = (Entity)dbObject;
                        btr.AppendEntity(ent);
                        transaction.AddNewlyCreatedDBObject(ent, true);
                        newlyCreatedObjects.Add(ent.ObjectId);
                    }

                    if (deleteRef)
                    {
                        block.UpgradeOpen();
                        block.Erase();
                    }

                    if (deleteBtr)
                    {
                        var bd = (BlockTableRecord)transaction.GetObject(blockRefTableId, OpenMode.ForWrite);
                        bd.Erase();
                    }
                }
                transaction.Commit();
            }

            return newlyCreatedObjects;
        }

        private static List<DBObject> GetFilteredObjects(IObjectFilter objectFilter, DBObjectCollection dbObjectCollection,
            Transaction transaction)
        {
            var filteredObjects = new List<DBObject>();
            if (objectFilter != null)
            {
                foreach (DBObject obj in dbObjectCollection)
                {
                    if (!objectFilter.Matches(obj, transaction)) continue;
                    filteredObjects.Add(obj);
                }
            }
            else
            {
                foreach (DBObject obj in dbObjectCollection)
                {
                    filteredObjects.Add(obj);
                }
            }

            return filteredObjects;
        }

        /// <summary>
        /// Adds all non constant attributes from blockdefinition
        /// </summary>
        /// <param name="blockReference"></param>
        /// <param name="transaction"></param>
        /// <returns>Added AttributeReferences</returns>
        public static IEnumerable<AttributeReference> AddAllAttributesFromDefinition(BlockReference blockReference, Transaction transaction)
        {
            var attributeReferences = new List<AttributeReference>();
            var blockDef = (BlockTableRecord)blockReference.BlockTableRecord.GetObject(OpenMode.ForRead);
            var nonConstantAttributeDefinitions = GetNonConstantAttributeDefinitions(blockDef).ToArray();
            foreach (var attDef in nonConstantAttributeDefinitions)
            {
                var attRef = new AttributeReference();
                attRef.SetAttributeFromBlock(attDef, blockReference.BlockTransform);
                blockReference.AttributeCollection.AppendAttribute(attRef);
                transaction.AddNewlyCreatedDBObject(attRef, true);
                attributeReferences.Add(attRef);
            }

            return attributeReferences;
        }

        /// <summary>
        /// Sets Attributvalues. If attribute doesn't exist, but exists in the blockdefinition, the attribute will be added.
        /// </summary>
        /// <param name="blockRef"></param>
        /// <param name="attributes"></param>
        /// <param name="transaction"></param>
        /// <returns>
        /// Returns true, if all values in attributes were found and set.
        /// </returns>
        public static bool SetAttributes(BlockReference blockRef, Dictionary<string, string> attributes, Transaction transaction)
        {
            var everyValueSet = true;
            var blockDef = (BlockTableRecord)transaction.GetObject(blockRef.BlockTableRecord, OpenMode.ForRead);
            var nonConstantAttributeDefinitions = GetNonConstantAttributeDefinitions(blockDef).ToArray();
            var attributeReferences = GetAttributeReferences(blockRef).ToArray();
            foreach (var kvpAttribute in attributes)
            {
                var attributeTag = kvpAttribute.Key;
                var attributeValue = kvpAttribute.Value;
                var existingAtt = attributeReferences.FirstOrDefault(x =>
                    x.Tag.Equals(attributeTag, StringComparison.InvariantCultureIgnoreCase));
                if (existingAtt != null)
                {
                    existingAtt.UpgradeOpen();
                    existingAtt.TextString = attributeValue;
                    existingAtt.DowngradeOpen();
                    continue;
                }

                var attDef = nonConstantAttributeDefinitions.FirstOrDefault(x =>
                    x.Tag.Equals(attributeTag, StringComparison.InvariantCultureIgnoreCase));
                if (attDef != null)
                {
                    using (var attRef = new AttributeReference())
                    {
                        attRef.SetAttributeFromBlock(attDef, blockRef.BlockTransform);
                        attRef.TextString = attributeValue;
                        blockRef.AttributeCollection.AppendAttribute(attRef);
                        transaction.AddNewlyCreatedDBObject(attRef, true);
                    }

                    continue;
                }

                everyValueSet = false;
            }

            return everyValueSet;
        }

        private static IEnumerable<AttributeReference> GetAttributeReferences(BlockReference blockReference, OpenMode openMode = OpenMode.ForRead)
        {
            var attributeReferences = new List<AttributeReference>();
            foreach (ObjectId attId in blockReference.AttributeCollection)
            {
                if (attId.IsErased) continue;
                var anyAttRef = attId.GetObject(openMode) as AttributeReference;
                if (anyAttRef == null) continue;
                attributeReferences.Add(anyAttRef);
            }

            return attributeReferences;
        }

        private static IEnumerable<AttributeDefinition> GetNonConstantAttributeDefinitions(
            BlockTableRecord blockTableRecord)
        {
            var attributeDefinitions = new List<AttributeDefinition>();
            foreach (var oid in blockTableRecord)
            {
                var attDef = oid.GetObject(OpenMode.ForRead) as AttributeDefinition;
                if (attDef != null && !attDef.Constant)
                {
                    attributeDefinitions.Add(attDef);
                }
            }
            return attributeDefinitions;
        }

    }
}
