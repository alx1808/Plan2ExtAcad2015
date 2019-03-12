using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;

// ReSharper disable StringLiteralTypo

// ReSharper disable IdentifierTypo

namespace Plan2Ext.Kleinbefehle
{
    public class HatchPolyBreite
    {
        [Autodesk.AutoCAD.Runtime.LispFunction("Plan2HatchPolyBreite")]
        public static bool Plan2HatchPolyBreite(ResultBuffer rb)
        {
            var ed = Application.DocumentManager.MdiActiveDocument.Editor;
            var objectIds = new List<ObjectId>();
            try
            {
                var filter = new Autodesk.AutoCAD.EditorInput.SelectionFilter(new[] { 
                    new TypedValue((int)DxfCode.Start,"LWPOLYLINE" ),
                });

                var res = ed.SelectAll(filter);
                if (res.Status != Autodesk.AutoCAD.EditorInput.PromptStatus.OK) return false;

                using (var ss = res.Value)
                {
                    if (ss == null) return false;
                    objectIds.AddRange(ss.GetObjectIds().ToList());
                }

                var db = Application.DocumentManager.MdiActiveDocument.Database;
                using (var transaction = db.TransactionManager.StartTransaction())
                {
                    var lwPolys = new List<Polyline>();
                    foreach (var objectId in objectIds)
                    {
                        var poly = (Polyline)transaction.GetObject(objectId, OpenMode.ForRead);
                        if (poly.ConstantWidth > 0.0001)
                        {
                            lwPolys.Add(poly);
                        }
                    }
                    foreach (var polyline in lwPolys)
                    {
                        try
                        {
                            CreateBoundedHatch(polyline, db);
                        }
                        catch (Exception e)
                        {
                            ed.WriteMessage("\n"+e.Message);
                        }
                    }

                    transaction.Commit();
                }
            }
            catch (Exception ex)
            {
                string msg = string.Format(CultureInfo.CurrentCulture, "Fehler in (Plan2HPB): {0}", ex.Message);
                ed.WriteMessage("\n" + msg);
                //System.Windows.Forms.MessageBox.Show(ex.Message, "Plan2HPB");
                return false;
            }

            return true;
        }

        internal static ObjectId CreateBoundedHatch(Polyline polyline, Database db)
        {
            bool errorOccured = false;
            var hatchOid = default(ObjectId);
            using (var transaction = db.TransactionManager.StartTransaction())
            {
                try
                {

                    var blockTableRecord = (BlockTableRecord)transaction.GetObject(polyline.BlockId, OpenMode.ForWrite);

                    var dbObjectCollection1 = polyline.GetOffsetCurves(polyline.ConstantWidth / 2.0);
                    var dbObjectCollection2 = polyline.GetOffsetCurves(polyline.ConstantWidth / -2.0);

                    var boundaryEntities = new List<Entity>();
                    var innerBoundaryEntities = new List<Entity>();

                    var lwPoly1 = GetLwPolyOffsetEntity(dbObjectCollection1);
                    if (lwPoly1 == null) return hatchOid;
                    var lwPoly2 = GetLwPolyOffsetEntity(dbObjectCollection2);
                    if (lwPoly2 == null) return hatchOid;


                    if (polyline.Closed || polyline.StartPoint.DistanceTo(polyline.EndPoint) < 0.0001)
                    {
                        if (lwPoly1.Area > lwPoly2.Area)
                        {
                            AddToBoundary(boundaryEntities, lwPoly1, blockTableRecord, transaction);
                            AddToBoundary(innerBoundaryEntities, lwPoly2, blockTableRecord, transaction);
                        }
                        else
                        { 
                            AddToBoundary(boundaryEntities, lwPoly2, blockTableRecord, transaction);
                            AddToBoundary(innerBoundaryEntities, lwPoly1, blockTableRecord, transaction);
                        }
                    }
                    else
                    {

                        AddToBoundary(boundaryEntities, lwPoly1, blockTableRecord, transaction);
                        AddToBoundary(boundaryEntities, lwPoly2, blockTableRecord, transaction);

                        if (lwPoly1.StartPoint.Distance2dTo(lwPoly2.StartPoint) <
                            lwPoly1.StartPoint.Distance2dTo(lwPoly2.EndPoint))
                        {
                            var line1 = new Line(lwPoly1.StartPoint, lwPoly2.StartPoint) { Layer = polyline.Layer };
                            AddToBoundary(boundaryEntities, line1, blockTableRecord, transaction);
                            var line2 = new Line(lwPoly1.EndPoint, lwPoly2.EndPoint) { Layer = polyline.Layer };
                            AddToBoundary(boundaryEntities, line2, blockTableRecord, transaction);
                        }
                        else
                        {
                            var line1 = new Line(lwPoly1.StartPoint, lwPoly2.EndPoint) { Layer = polyline.Layer };
                            AddToBoundary(boundaryEntities, line1, blockTableRecord, transaction);
                            var line2 = new Line(lwPoly1.EndPoint, lwPoly2.StartPoint) { Layer = polyline.Layer };
                            AddToBoundary(boundaryEntities, line2, blockTableRecord, transaction);
                        }

                    }

                    try
                    {
                        var boundaryOids = boundaryEntities.Select(x => x.ObjectId).ToList();
                        var innerBoundaryOids = innerBoundaryEntities.Select(x => x.ObjectId).ToList();
                        var hatch = Globs.CreateHatch(boundaryOids,innerBoundaryOids, polyline.Layer, blockTableRecord, transaction);
                        hatchOid = hatch.ObjectId;
                    }
                    catch (Exception)
                    {
                        errorOccured = true;
                        throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture,"Fehler beim erzeugen der Schraffur für Element {0}!", polyline.Handle.ToString()));
                    }


                    var dbObjCollection = new DBObjectCollection();
                    boundaryEntities.ForEach(x => dbObjCollection.Add(x));
                    var regionParts = Region.CreateFromCurves(dbObjCollection);

                    foreach (Entity regionPart in regionParts)
                    {
                        var exploded = new DBObjectCollection();
                        regionPart.Explode(exploded);

                        foreach (Entity entity in exploded)
                        {
                            entity.Layer = polyline.Layer;
                            blockTableRecord.AppendEntity(entity);
                            transaction.AddNewlyCreatedDBObject(entity, true);
                        }
                    }

                    foreach (var entity in boundaryEntities)
                    {
                        entity.Erase(true);
                    }
                    foreach (var entity in innerBoundaryEntities)
                    {
                        entity.Erase(true);
                    }
                    if (!polyline.IsWriteEnabled)
                    {
                        polyline.UpgradeOpen();
                    }
                    polyline.Erase(true);
                }
                finally
                {
                    if (!errorOccured) transaction.Commit();
                }

            }

            return hatchOid;
        }

        private static void AddToBoundary(List<Entity> entitiesToDelete, Entity entity, BlockTableRecord blockTableRecord,
            Transaction transaction)
        {
            entitiesToDelete.Add(entity);
            blockTableRecord.AppendEntity(entity);
            transaction.AddNewlyCreatedDBObject(entity, true);
        }

        private static Polyline GetLwPolyOffsetEntity(DBObjectCollection dbObjectCollection)
        {
            if (dbObjectCollection == null || dbObjectCollection.Count != 1) return null;
            foreach (DBObject dbObject in dbObjectCollection)
            {
                return dbObject as Polyline;
            }

            return null;
        }
    }
}
