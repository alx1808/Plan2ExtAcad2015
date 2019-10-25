using System.Collections.Generic;
using Plan2Ext.ObjectFilter;
#if BRX_APP
using  Bricscad.ApplicationServices;
using Teigha.DatabaseServices;
#elif ARX_APP
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.ApplicationServices.Core;
#endif

namespace Plan2Ext
{
    internal static class BlockManager
    {
        public  static IEnumerable<ObjectId> InsertXrefsAsBlocks(Database db, IEnumerable<ObjectId> xrefTableRecordIds)
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
    }
}
