using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Plan2Ext.Configuration;
#if BRX_APP
using Bricscad.EditorInput;
using Teigha.DatabaseServices;
using Teigha.Runtime;
using Teigha.Geometry;
using Bricscad.ApplicationServices;
#elif ARX_APP
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
#endif
// ReSharper disable IdentifierTypo
// ReSharper disable StringLiteralTypo


namespace Plan2Ext.Kleinbefehle
{
    public class BaseMoveOeffnungen
    {

        private double _searchDistance = 0.1;
        private double _zoomWith = 1.0;
        private readonly string HatchOrPolyNotFound = "_Offnungsschraffur_nicht_gefunden";
        private readonly string CentroidNotFound = "_Konnte_keinen_Centroid_finden";
        private readonly string NoIntersectionFound = "_Konnte_Schnittpunkt_nicht_finden";

        [CommandMethod("Plan2BaseMoveOeffnungen")]
        public void Plan2BaseMoveOeffnungen()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            var editor = doc.Editor;
            try
            {
                _searchDistance = Convert.ToDouble(TheConfiguration.GetValue("alx_V:ino_zrids_BaseMoveDistFromHatch"));
                var blockIds = SearchOeffBlockIds();
                if (blockIds == null) return;
                blockIds.ToList().ForEach(BasePointCorrection);

            }
            catch (System.Exception ex)
            {
                string msg = string.Format(CultureInfo.CurrentCulture, "Fehler in (Plan2BaseMoveOeffnungen): {0}", ex.Message);
                editor.WriteMessage("\n" + msg);
            }
        }

        private void BasePointCorrection(ObjectId oid)
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            using (var transaction = doc.TransactionManager.StartTransaction())
            {
                var block = (BlockReference)oid.GetObject(OpenMode.ForRead);
                var hatchOrPolyOid = SearchHatchOrPoly(block.Position);
                if (hatchOrPolyOid.IsNull)
                {
                    Globs.InsertFehlerLines(new List<Point3d>() { block.Position }, HatchOrPolyNotFound);
                }
                else
                {
                    var centroid = GetCentroidUcs(hatchOrPolyOid);
                    if (centroid == null)
                    {
                        Globs.InsertFehlerLines(new List<Point3d>() { block.Position }, CentroidNotFound);
                    }
                    else
                    {
                        var intersPerpenticular = GetIntersPerpenticular(block, centroid.Value.ToWcs());
                        if (intersPerpenticular == null)
                        {
                            Globs.InsertFehlerLines(new List<Point3d>() { block.Position }, NoIntersectionFound);
                        }
                        else
                        {
                            MoveBasePoint(block, intersPerpenticular.Value);
                        }
                    }
                }

                transaction.Commit();
            }
        }

        private static void MoveBasePoint(BlockReference block, Point3d targetPoint)
        {
            block.UpgradeOpen();
            block.Position = targetPoint;
            block.DowngradeOpen();
        }

        private Point3d? GetIntersPerpenticular(BlockReference block, Point3d wcsCentroid)
        {
            var blockName = block.Name;
            var additionalRotation = FensterConfiguration.Pertains(blockName)
                ? FensterConfiguration.GetRotation(block.Name)
                : TuerConfiguration.GetRotation(blockName);
            var blockRotation = block.Rotation + additionalRotation;
            var perpRotation = blockRotation + Math.PI * 0.5;


            using (var xline = new Xline())
            {
                xline.BasePoint = block.Position;
                xline.SecondPoint = block.Position.Polar(blockRotation, 1.0);

                using (var xline2 = new Xline())
                {
                    xline2.BasePoint = wcsCentroid;
                    xline2.SecondPoint = wcsCentroid.Polar(perpRotation, 1.0);
                    var col = new Point3dCollection();
                    xline.IntersectWith(xline2, Intersect.ExtendBoth, col, IntPtr.Zero, IntPtr.Zero);
                    if (col.Count == 0) return null;
                    return col[0];
                }
            }
        }

        private Point3d? GetCentroidUcs(ObjectId hatchOrPolyOid)
        {
            var ent = hatchOrPolyOid.GetObject(OpenMode.ForRead);
            if (ent is Polyline) return Globs.GetCentroid(hatchOrPolyOid);

            var toDelete = Globs.GeneratePolylinesFromHatches(new[] { hatchOrPolyOid }).ToArray();
            var centroid = Globs.GetCentroid(toDelete.First());
            foreach (var objectId in toDelete)
            {
                var dbo = objectId.GetObject(OpenMode.ForWrite);
                dbo.Erase(true);
            }
            return centroid;
        }

        private ObjectId SearchHatchOrPoly(Point3d wcsBlockPosition)
        {
            var cps = Globs.GetSelectCrossingPoints(wcsBlockPosition, _searchDistance);
            Globs.ZoomToPoint(wcsBlockPosition, _zoomWith);
            var oeffHatchLayer = TheConfiguration.GetValueString("alx_V:ino_zrids_OeffnungsLayer");
            oeffHatchLayer = Globs.MatchCodeCorrection(oeffHatchLayer);
            var filter = new SelectionFilter(new[]
            {
                new TypedValue((int)DxfCode.Operator ,"<AND"),
                new TypedValue((int) DxfCode.LayerName, oeffHatchLayer),
                new TypedValue((int)DxfCode.Operator ,"<OR"),
                new TypedValue((int) DxfCode.Start, "HATCH"),
                new TypedValue((int) DxfCode.Start, "*POLYLINE"),
                new TypedValue((int)DxfCode.Operator ,"OR>"),
                new TypedValue((int)DxfCode.Operator ,"AND>"),
            });
            var result = Application.DocumentManager.MdiActiveDocument.Editor.SelectCrossingPolygon(cps, filter);
            if (result.Status != PromptStatus.OK) return ObjectId.Null;
#if BRX_APP
            SelectionSet ss = result.Value;
#else
            using (SelectionSet ss = result.Value)
#endif
            {
                // first
                return ss.GetObjectIds()[0];
            }
        }

        private IEnumerable<ObjectId> SearchOeffBlockIds()
        {
            var filter = new SelectionFilter(new[]
            {
                new TypedValue((int)DxfCode.Operator ,"<AND"),
                new TypedValue((int) DxfCode.Start, "INSERT"),
                new TypedValue((int)DxfCode.Operator ,"<OR"),
                new TypedValue((int) DxfCode.BlockName, FensterConfiguration.FensterblockOben),
                new TypedValue((int) DxfCode.BlockName, FensterConfiguration.FensterblockUnten),
                new TypedValue((int) DxfCode.BlockName, FensterConfiguration.FensterblockLinks),
                new TypedValue((int) DxfCode.BlockName, FensterConfiguration.FensterblockRechts),
                new TypedValue((int) DxfCode.BlockName, TuerConfiguration.TuerblockOben),
                new TypedValue((int) DxfCode.BlockName, TuerConfiguration.TuerblockUnten),
                new TypedValue((int) DxfCode.BlockName, TuerConfiguration.TuerblockLinks),
                new TypedValue((int) DxfCode.BlockName, TuerConfiguration.TuerblockRechts),
                new TypedValue((int)DxfCode.Operator ,"OR>"),
                new TypedValue((int)DxfCode.Operator ,"AND>"),
            });
            var result = Application.DocumentManager.MdiActiveDocument.Editor.SelectAll(filter);
            if (result.Status != PromptStatus.OK) return null;
#if BRX_APP
            SelectionSet ss = result.Value;
#else
            using (SelectionSet ss = result.Value)
#endif
            {
                return ss.GetObjectIds();
            }
        }
    }
}
