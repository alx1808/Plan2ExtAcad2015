// ReSharper disable StringLiteralTypo
// ReSharper disable CommentTypo
// ReSharper disable IdentifierTypo
#if BRX_APP
using  Bricscad.ApplicationServices;
using Teigha.DatabaseServices;
using Teigha.Geometry;
using Bricscad.EditorInput;
#elif ARX_APP
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;

#endif
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Plan2Ext.ObjectFilter;


namespace Plan2Ext
{
    internal static class BlockManager
    {

        /// <summary>
        /// Inserts block local or from proto
        /// </summary>
        /// <param name="blockName"></param>
        /// <param name="positionUcs"></param>
        /// <param name="dwgName"></param>
        /// <param name="explode"></param>
        /// <param name="transaction"></param>
        /// <param name="scaleFactor"></param>
        /// <param name="ucsPointList"></param>
        /// <param name="newPositionX"></param>
        /// <returns>X-Value of boundary on the right side in UCS</returns>
        public static bool InsertLocalOrFromProto(string blockName, Point3d positionUcs, string dwgName, bool explode, Transaction transaction, double scaleFactor, List<Point3d> ucsPointList, out double newPositionX)
        {
            newPositionX = 0.0;

            var doc = Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;

            if (!BlockExists(blockName) && !InsertFromPrototype(blockName, dwgName)) return false;
            var blockTable = (BlockTable)transaction.GetObject(db.BlockTableId, OpenMode.ForRead);
            var oid = blockTable[blockName];
            using (var bref = new BlockReference(Globs.TransUcsWcs(positionUcs), oid))
            {
                bref.ScaleFactors = new Scale3d(scaleFactor);
                bref.Rotation = Globs.GetUcsDirection();
                var acCurSpaceBlkTblRec = (BlockTableRecord)transaction.GetObject(db.CurrentSpaceId, OpenMode.ForWrite);
                acCurSpaceBlkTblRec.AppendEntity(bref);
                transaction.AddNewlyCreatedDBObject(bref, true);

                var boundingPointsWcs = Boundings.CollectPointsWcs(transaction, bref);
                var ptsUcs = boundingPointsWcs.ToList().Select(Globs.TransWcsUcs).ToList();
                var recPointsUcs = Boundings.GetRectanglePointsFromBounding(buffer: 0.0, pts: ptsUcs);
                ucsPointList.AddRange(recPointsUcs);
                newPositionX = recPointsUcs.Select(x => x.X).Max();

                if (explode)
                {
                    var objs = new DBObjectCollection();
                    bref.Explode(objs);
                    foreach (DBObject obj in objs)
                    {
                        var ent = (Entity)obj;
                        acCurSpaceBlkTblRec.AppendEntity(ent);
                        transaction.AddNewlyCreatedDBObject(ent, true);
                    }
                    bref.UpgradeOpen();
                    bref.Erase();
                }
            }

            return true;
        }

        public static bool InsertFromPrototype(string blockName, string protoDwgName)
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            string protoDwgFullPath;
            try
            {
                protoDwgFullPath = HostApplicationServices.Current.FindFile(protoDwgName, doc.Database, FindFileHint.Default);
            }
            catch (Exception)
            {
                doc.Editor.WriteMessage(string.Format(CultureInfo.CurrentCulture, "\nKonnte Prototypzeichnung '{0}' nicht finden!", protoDwgName));
                return false;
            }
            bool ret = false;
            using (var openDb = new Database(false, true))
            {
                openDb.ReadDwgFile(protoDwgFullPath, System.IO.FileShare.ReadWrite, true, "");
                var ids = new ObjectIdCollection();

                using (var tr = openDb.TransactionManager.StartTransaction())
                {
                    var bt = (BlockTable)tr.GetObject(openDb.BlockTableId, OpenMode.ForRead);
                    if (bt.Has(blockName))
                    {
                        ids.Add(bt[blockName]);
                    }
                    else
                    {
                        doc.Editor.WriteMessage(string.Format(CultureInfo.CurrentCulture, "\nDie Prototypzeichnung '{0}' hat keinen Block namens '{1}'!", protoDwgFullPath, blockName));
                    }
                    tr.Commit();
                }

                if (ids.Count > 0)
                {
                    //get the current drawing database
                    var destdb = doc.Database;
                    var iMap = new IdMapping();
                    destdb.WblockCloneObjects(ids, destdb.BlockTableId, iMap, DuplicateRecordCloning.Ignore, deferTranslation: false);
                    ret = true;
                }
            }
            return ret;
        }

        public static ObjectId InsertDwg(string fname, Point3d insertPt, double rotation, double scale, string blockName)
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            var ret = InsertDwgToDb(fname, insertPt, rotation, scale, blockName, db);
            return ret;
        }

        public static void InsertDwgToDwg(string targetFileName, string sourceFileName, Point3d insertPt,
            double rotation, double scale, bool createBak)
        {
	        var dirName = System.IO.Path.GetDirectoryName(targetFileName);
			if (dirName == null) return;
			var newFileName = System.IO.Path.Combine(dirName ,
                System.IO.Path.GetFileNameWithoutExtension(targetFileName) + "_X.dwg");
            using (var dbTarget = new Database(false, true))
            {
                dbTarget.ReadDwgFile(targetFileName, System.IO.FileShare.Read, true, "");
                string newBlockName = GetNewBlockname(dbTarget, "Dummy");
                var blockOid = InsertDwgToDb(sourceFileName, insertPt, rotation, scale, newBlockName, dbTarget);
                Explode(blockOid, dbTarget, true, true);
                dbTarget.SaveAs(newFileName, DwgVersion.Newest);
            }

            if (createBak)
                BakAndMove(newFileName, targetFileName);
            else Move(newFileName, targetFileName);
        }

        private static ObjectId InsertDwgToDb(string fname, Point3d insertPt, double rotation, double scale, string blockName, Database db)
        {
            ObjectId ret;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTableRecord btrMs = (BlockTableRecord)db.CurrentSpaceId.GetObject(OpenMode.ForWrite);
                ObjectId objId;
                using (Database dbInsert = new Database(false, true))
                {
                    dbInsert.ReadDwgFile(fname, FileOpenMode.OpenForReadAndAllShare, false, null);
                    objId = db.Insert(blockName, dbInsert, true);
                }

                BlockReference bref = new BlockReference(insertPt, objId)
                {
                    Rotation = rotation, ScaleFactors = new Scale3d(scale)
                };
                btrMs.AppendEntity(bref);
                tr.AddNewlyCreatedDBObject(bref, true);

                ret = bref.ObjectId;
                tr.Commit();
            }

            return ret;
        }

        public static void Wblock(string fileName, IEnumerable<ObjectId> objectIds, Point3d origin)
        {
            if (System.IO.File.Exists(fileName)) throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "File already exists: {0}!", fileName));

            var currentDatabase = Application.DocumentManager.MdiActiveDocument.Database;
            var objectIdCollection = new ObjectIdCollection(objectIds.ToArray());
            using (var database = new Database(true, false))
            {
                currentDatabase.Wblock(database, objectIdCollection, origin,
                    DuplicateRecordCloning.Ignore);
                database.SaveAs(fileName, DwgVersion.Newest);
            }
        }

        /// <summary>
        /// Explode block in current document
        /// </summary>
        /// <param name="blockOid"></param>
        /// <param name="deleteRef"></param>
        /// <param name="purge"></param>
        public static List<ObjectId> Explode(ObjectId blockOid, bool deleteRef = false, bool purge = false)
        {
            var db = Application.DocumentManager.MdiActiveDocument.Database;
            return Explode(blockOid, db, deleteRef, purge);
        }

        internal static List<ObjectId> Explode(ObjectId blockOid, Database db, bool deleteRef, bool purge)
        {
            var newlyCreatedObjects = new List<ObjectId>();
            using (var tr = db.TransactionManager.StartTransaction())
            {
                //BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockReference block = (BlockReference)tr.GetObject(blockOid, OpenMode.ForRead);
                ObjectId blockRefTableId = block.BlockTableRecord;
                BlockTableRecord targetSpace =
                    (BlockTableRecord)tr.GetObject(block.BlockId, OpenMode.ForWrite);
                //BlockTableRecord targetSpace = (BlockTableRecord)tr.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(db), OpenMode.ForWrite);
                DBObjectCollection objs = new DBObjectCollection();
                block.Explode(objs);
                foreach (DBObject obj in objs)
                {
                    Entity ent = (Entity)obj;
                    targetSpace.AppendEntity(ent);
                    tr.AddNewlyCreatedDBObject(ent, true);
                    newlyCreatedObjects.Add(ent.ObjectId);
                }

                if (deleteRef)
                {
                    block.UpgradeOpen();
                    block.Erase();
                }

                if (purge)
                {
                    var bd = (BlockTableRecord)tr.GetObject(blockRefTableId, OpenMode.ForWrite);
                    bd.Erase();
                }

                tr.Commit();
            }

            return newlyCreatedObjects;
        }


        public static BlockReference GetBlockFromItsSubentity(Transaction tr, PromptNestedEntityResult nres)
        {
            ObjectId selId = nres.ObjectId;

            List<ObjectId> objIds = new List<ObjectId>(nres.GetContainers());

            objIds.Add(selId);

            objIds.Reverse();

            // following lines needed?

            // Retrieve the sub-entity path for this entity

            //SubentityId subEnt = new SubentityId(SubentityType.Null, 0);

            //FullSubentityPath path = new FullSubentityPath(objIds.ToArray(), subEnt);

            // Open the outermost container, relying on the open

            // transaction...

            Entity subent = (Entity)tr.GetObject(objIds[0], OpenMode.ForRead, false);

            ObjectId eid = subent.OwnerId;

            var bowner = tr.GetObject(eid, OpenMode.ForRead, false);

            return bowner as BlockReference;

        }

        internal static bool InsertWblock(Database db, string blockName, string newBlockName = null)
        {
            if (string.IsNullOrEmpty(newBlockName)) newBlockName = blockName;
            var dwgFullPath = HostApplicationServices.Current.FindFile(blockName + ".dwg", db, FindFileHint.Default);
            if (!System.IO.File.Exists(dwgFullPath)) return false;

            using (var transaction = db.TransactionManager.StartTransaction())
            {
                using (var wblockDatabase = new Database(false, true))
                {
                    wblockDatabase.ReadDwgFile(dwgFullPath, System.IO.FileShare.Read, true, null);
                    db.Insert(newBlockName, wblockDatabase, true);
                }
                transaction.Commit();
            }
            return true;
        }

        private static void BakAndMove(string newFileName, string targetFileName)
        {
            var bakFileName = GetBakFileName(targetFileName);
            if (System.IO.File.Exists(bakFileName)) System.IO.File.Delete(bakFileName);
            System.IO.File.Move(targetFileName, bakFileName);
            System.IO.File.Move(newFileName, targetFileName);
        }

        internal static string GetBakFileName(string dwgFileName)
        {
            return dwgFileName.Remove(dwgFileName.Length - 3, 3) + "Bak";
        }

        internal static void CreateBakFile(string dwgFileName)
        {
            if (System.IO.File.Exists(dwgFileName))
            {
                System.IO.File.Copy(dwgFileName, GetBakFileName(dwgFileName), true);
            }
        }

        internal static void Move(string newFileName, string targetFileName)
        {
            if (System.IO.File.Exists(targetFileName)) System.IO.File.Delete(targetFileName);
            System.IO.File.Move(newFileName, targetFileName);
        }

        internal static bool BlockExists(string blockName)
        {
            var db = Application.DocumentManager.MdiActiveDocument.Database;
            return BlockExists(blockName, db);
        }

        private static bool BlockExists(string blockName, Database db)
        {
            bool exists;
            var tm = db.TransactionManager;
            using (var  transaction = tm.StartTransaction())
            {
                using (var  bt = (BlockTable)tm.GetObject(db.BlockTableId, OpenMode.ForRead, false))
                {
                    exists = bt.Has(blockName);
                }
                transaction.Commit();
            }

            return exists;
        }

        internal static string GetNewBlockname(Database dbTarget, string prefix)
        {
            var inc = 1;
            while (BlockExists(prefix + inc, dbTarget))
            {
                inc++;
            }

            return prefix + inc;
        }

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
                            var blockOid = InsertDwg(bd.PathName, br.Position, br.Rotation, 1.0, br.Name + "_AS_BLOCK");
                            blockRefIds.Add(blockOid);
                        }
                        else
                        {
                            var blockOid = InsertDwg(System.IO.Path.GetFullPath(dwgPath + bd.PathName), br.Position, br.Rotation, 1.0, br.Name + "_AS_BLOCK");
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
