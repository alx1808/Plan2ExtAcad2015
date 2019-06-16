using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

// ReSharper disable StringLiteralTypo

// ReSharper disable IdentifierTypo

namespace Plan2Ext.AutoIdVergabeOeff
{
    class FenSorter : Sorter
    {
        private readonly IConfigurationHandler _configurationHandler;
        private readonly IPalette _palette;
        private int _currentNr;

        public FenSorter(ConfigurationHandler configurationHandler, IPalette palette)
        {
            _configurationHandler = configurationHandler;
            _palette = palette;
        }

        public void Sort(IEnumerable<IFensterInfo> fensterInfos, ObjectId objectPolygonId)
        {
            _currentNr = _palette.FenNr;
            var arr = fensterInfos.ToArray();
            SortAlongObjectPolygon(arr, objectPolygonId);
            try
            {
                UcsToAnsicht();
                SortFromLeftToRight(arr);
            }
            finally
            {
                UcsRestore();
            }

            _palette.FenNr = _currentNr + 1;
        }

        private void SortFromLeftToRight(IFensterInfo[] fensterInfos)
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            var fensOnPoly = fensterInfos.Where(x => x.Kind == FensterInfo.KindEnum.InsidePolygon).ToArray();
            NrFromLeftToRight(doc, fensOnPoly);
            fensOnPoly = fensterInfos.Where(x => x.Kind == FensterInfo.KindEnum.OutsidePolygon).ToArray();
            NrFromLeftToRight(doc, fensOnPoly);
        }

        private void NrFromLeftToRight(Document doc, IFensterInfo[] fensOnPoly)
        {
            using (var transaction = doc.TransactionManager.StartTransaction())
            {
                var ordered = fensOnPoly.OrderBy(x => Globs.TransWcsUcs(x.InsertPoint).X).ToArray();
                Renumber(ordered, transaction);

                transaction.Commit();
            }
        }

        private void SortAlongObjectPolygon(IFensterInfo[] fensterInfos, ObjectId objectPolygonId)
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            var ed = doc.Editor;
            var result = ed.GetPoint("Startpunkt:");
            if (result.Status != PromptStatus.OK) return;
            var startPoint = result.Value;
            var fensOnPoly = fensterInfos.Where(x => x.Kind == FensterInfo.KindEnum.OnPolygon).ToArray();
            using (var transaction = doc.TransactionManager.StartTransaction())
            {
                var curve = (Curve)transaction.GetObject(objectPolygonId, OpenMode.ForRead);
                var polyline = (Polyline)curve;

                //var bounds = curve.Bounds;
                //var startPoint = new Point3d(bounds.Value.MinPoint.X, bounds.Value.MaxPoint.Y, 0.0);
                var ordered = fensOnPoly.OrderBy(x => GetDistanceToPoint(curve, x.InsertPoint, startPoint)).ToArray();
                if (!polyline.IsClockWise())
                {
                    ordered = ordered.Reverse().ToArray();
                }
                Renumber(ordered, transaction);

                transaction.Commit();
            }
        }

        private void Renumber(IFensterInfo[] ordered, Transaction transaction)
        {
            foreach (var fensterInfo in ordered)
            {
                var blockReference = (BlockReference) transaction.GetObject(fensterInfo.Oid, OpenMode.ForRead);
                var nrAtt = Globs.GetAttributEntities(blockReference, transaction).FirstOrDefault(x =>
                    string.Compare(x.Tag, _configurationHandler.FenNrAttName, StringComparison.OrdinalIgnoreCase) == 0);
                if (nrAtt == null)
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture,
                        "Fensterblock mit Handle {0} hat kein Attribut {1}!", blockReference.Handle.ToString(),
                        _configurationHandler.FenNrAttName));
                nrAtt.UpgradeOpen();
                nrAtt.TextString = _palette.FenPrefix + _currentNr.ToString().PadLeft(3, '0');
                nrAtt.DowngradeOpen();
                _currentNr++;
            }
        }

        // ReSharper disable once UnusedMember.Local
        private static double GetDistanceToPoint(Curve curve, Point3d pt)
        {
            var ptOnCurve = curve.GetClosestPointTo(pt, true);
            var ptparam = curve.GetParameterAtPoint(ptOnCurve);
            var a = curve.GetDistanceAtParameter(ptparam);
            var b = curve.GetDistanceAtParameter(curve.StartParam);
            return a - b;
        }

        private static double GetDistanceToPoint(Curve curve, Point3d pt, Point3d startPoint)
        {
            var ptOnCurve = curve.GetClosestPointTo(pt, true);
            var ptparam = curve.GetParameterAtPoint(ptOnCurve);

            var startPointOnCurve = curve.GetClosestPointTo(startPoint, true);
            var startPointParam = curve.GetParameterAtPoint(startPointOnCurve);
            var a = curve.GetDistanceAtParameter(ptparam);
            var b = curve.GetDistanceAtParameter(startPointParam);
            var dist = a - b;

            if (dist < 0.0)
            {
                var length = curve.GetDistanceAtParameter(curve.EndParam) - curve.GetDistanceAtParameter(curve.StartParam);
                dist += length;
            }

            return dist;
        }
    }
}
