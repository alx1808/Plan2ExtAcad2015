using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
// ReSharper disable StringLiteralTypo
// ReSharper disable IdentifierTypo

namespace Plan2Ext.BlockInfo
{
    public class Commands
    {
        #region log4net Initialization
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Convert.ToString((typeof(Commands))));
        #endregion

        [CommandMethod("Plan2BlockinfoLayout")]
        public static void Plan2BlockinfoLayout()
        {
            try
            {
                var doc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
                var ed = doc.Editor;

                Application.SetSystemVariable("OSMODE",0);

                using (doc.LockDocument())
                {
                    if (!Globs.IsPaperspace)
                    {
                        ed.WriteMessage("\nDieser Befehl kann nur im Papierbereich ausgeführt werden");
                        return;
                    }

                    using (var transaction = doc.TransactionManager.StartTransaction())
                    {
                        var viewport = SelectViewport(transaction);
                        if (viewport == null) return;

                        Point3dCollection point3dCollectionWcs = GetWcsViewportFrame(viewport, transaction);

                        Globs.SwitchToModelSpace();
                        Globs.ZoomExtents();

                        Point3dCollection point3dCollectionUcs = WcsToUcs(point3dCollectionWcs);

                        SelectionFilter filter = new SelectionFilter(new[]
                        {
                            new TypedValue((int)DxfCode.Start,"INSERT" ),
                        });
                        var promptSelectionResult = ed.SelectCrossingPolygon(point3dCollectionUcs, filter);
                        List<ObjectId> selectedBlocks = new List<ObjectId>();
                        using (SelectionSet ss = promptSelectionResult.Value)
                        {
                            if (ss != null)
                                selectedBlocks.AddRange(ss.GetObjectIds().ToList());
                        }

                        var blockNames = selectedBlocks.Where(x => !Globs.IsXref(x, transaction))
                            .Select(x => Globs.GetBlockname(x, transaction)).Distinct().ToList();

                        //DrawPolyline(transaction, doc, point3dCollectionWcs);

                        Globs.SwitchToPaperSpace();

                        transaction.Commit();

                        if (blockNames.Count == 0)
                        {
                            ed.WriteMessage("\nKeine Blöcke gefunden.");
                        }
                        else
                        {
                            var rows = blockNames.Select(x => new SingleBlockNameRowProvider() { Blockname = x });
                            var excelizer = new Excelizer();
                            excelizer.ExcelExport(new[] { "Blöcke" }, rows);
                        }
                    }
                    // todo: export to excel
                }
            }
            catch (System.Exception ex)
            {
                log.Error(ex.Message, ex);
                Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2BlockinfoLayout aufgetreten! {0}", ex.Message));
            }
        }

        private static Point3dCollection WcsToUcs(Point3dCollection point3DCollectionWcs)
        {
            var point3DCollectionUcs = new Point3dCollection();
            foreach (Point3d point3D in point3DCollectionWcs)
            {
                point3DCollectionUcs.Add(Globs.TransWcsUcs(point3D));

            }

            return point3DCollectionUcs;
        }

        private static void DrawPolyline(Transaction transaction, Document doc, Point3dCollection point3dCollection)
        {
            var blockTable = transaction.GetObject(doc.Database.BlockTableId, OpenMode.ForRead) as BlockTable;
            var blockTableRecord =
                transaction.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
            using (var polyline = new Polyline())
            {
                for (int i = 0; i < point3dCollection.Count; i++)
                {
                    polyline.AddVertexAt(i, To2D(point3dCollection[i]), 0, 0, 0);
                }

                blockTableRecord.AppendEntity(polyline);
                transaction.AddNewlyCreatedDBObject(polyline, true);
            }
        }

        private static Point2d To2D(Point3d point3D)
        {
            return new Point2d(point3D.X, point3D.Y);
        }

        private static Point3dCollection GetWcsViewportFrame(Viewport viewport, Transaction transaction)
        {
            var cp = viewport.CenterPoint;
            var halfHeight = viewport.Height / 2.0;
            var halfWidth = viewport.Width / 2.0;

            var lu = new Point3d(cp.X - halfWidth, cp.Y - halfHeight, cp.Z);
            var lo = new Point3d(cp.X - halfWidth, cp.Y + halfHeight, cp.Z);
            var ro = new Point3d(cp.X + halfWidth, cp.Y + halfHeight, cp.Z);
            var ru = new Point3d(cp.X + halfWidth, cp.Y - halfHeight, cp.Z);

            var points = new List<Point3d>() { lu, lo, ro, ru };
            var wcsPoints = new List<Point3d>();
            PaperSpaceHelper.ConvertPaperSpaceCoordinatesToModelSpaceWcs(viewport.ObjectId, points, wcsPoints);

            return new Point3dCollection(wcsPoints.ToArray());
        }

        private static Viewport SelectViewport(Transaction transaction)
        {
            var doc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
            var ed = doc.Editor;
            ed.SwitchToPaperSpace();

            var promptEntityResult = ed.GetEntity("\nViewport auswählen: ");
            if (promptEntityResult.Status != PromptStatus.OK) return null;
            return transaction.GetObject(promptEntityResult.ObjectId, OpenMode.ForRead) as Viewport;
        }
    }
}
