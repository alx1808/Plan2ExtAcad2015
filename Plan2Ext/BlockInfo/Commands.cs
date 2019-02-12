using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Convert.ToString((typeof(Commands))));
        #endregion

        [CommandMethod("Plan2Blockinfo")]
        public static void Plan2Blockinfo()
        {
            try
            {
                var doc = Application.DocumentManager.MdiActiveDocument;
                var ed = doc.Editor;

                Application.SetSystemVariable("OSMODE", 0);

                var pKeyOpts = new PromptKeywordOptions("") { Message = "\nOption eingeben Model/Layout/<All>: " };
                if (Globs.IsModelspace)
                {
                    pKeyOpts.Keywords.Add("Model");
                }
                else
                {
                    pKeyOpts.Keywords.Add("Layout");
                }
                pKeyOpts.Keywords.Add("All");
                pKeyOpts.AllowNone = true;

                PromptResult pKeyRes = ed.GetKeywords(pKeyOpts);
                if (pKeyRes.Status == PromptStatus.None || pKeyRes.StringResult == "All")
                {
                    Plan2BlockinfoLayoutAll();
                }
                else if (pKeyRes.Status == PromptStatus.OK)
                {
                    if (pKeyRes.StringResult == "Layout")
                    {
                        Plan2BlockinfoLayout();
                    }
                    else
                    {
                        Plan2BlockinfoModell();
                    }
                }
            }
            catch (System.Exception ex)
            {
                Log.Error(ex.Message, ex);
                Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2Blockinfo aufgetreten! {0}", ex.Message));
            }
        }

        //[CommandMethod("Plan2BlockinfoLayoutAll")]
        public static void Plan2BlockinfoLayoutAll()
        {
            try
            {
                var doc = Application.DocumentManager.MdiActiveDocument;
                var ed = doc.Editor;

                Application.SetSystemVariable("OSMODE", 0);

                using (doc.LockDocument())
                {
                    Globs.SwitchToModelSpace();
                    Globs.ZoomExtents();

                    using (var transaction = doc.TransactionManager.StartTransaction())
                    {
                        var layoutNames = Layouts.GetOrderedLayoutNames();

                        var rowProviders = new List<IRowProvider>();

                        foreach (var layoutName in layoutNames)
                        {
                            if (layoutName == "Model") continue;
                            var viewport = SetLayoutActiveAndGetViewport(layoutName, transaction);
                            if (viewport == null) continue;

                            Point3dCollection point3DCollectionWcs = GetWcsViewportFrame(viewport);
                            Globs.SwitchToModelSpace();
                            Point3dCollection point3DCollectionUcs = WcsToUcs(point3DCollectionWcs);
                            SelectionFilter filter = new SelectionFilter(new[]
                            {
                                new TypedValue((int)DxfCode.Start,"INSERT" ),
                            });
                            var promptSelectionResult = ed.SelectCrossingPolygon(point3DCollectionUcs, filter);
                            List<ObjectId> selectedBlocks = new List<ObjectId>();
                            using (SelectionSet ss = promptSelectionResult.Value)
                            {
                                if (ss != null)
                                    selectedBlocks.AddRange(ss.GetObjectIds().ToList());
                            }

                            var blockNames = selectedBlocks.Where(x => !Globs.IsXref(x, transaction))
                                .Select(x => Globs.GetBlockname(x, transaction)).Distinct().ToList();

                            rowProviders.AddRange(LayoutBlockNameFactory.CreateRowProviders(layoutName, blockNames));

                        }

                        transaction.Commit();

                        var excelizer = new Excelizer();
                        excelizer.ExcelExport(new[] { "LAYOUTNAME", "BLÖCKE" }, rowProviders);

                    }
                }
            }
            catch (System.Exception ex)
            {
                Log.Error(ex.Message, ex);
                Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2BlockinfoLayoutAll aufgetreten! {0}", ex.Message));
            }
        }

        //[CommandMethod("Plan2BlockinfoModell")]
        public static void Plan2BlockinfoModell()
        {
            try
            {
                var doc = Application.DocumentManager.MdiActiveDocument;
                var ed = doc.Editor;

                Application.SetSystemVariable("OSMODE", 0);

                using (doc.LockDocument())
                {
                    if (!Globs.IsModelspace)
                    {
                        ed.WriteMessage("\nDieser Befehl kann nur im Modellbereich ausgeführt werden");
                        return;
                    }

                    using (var transaction = doc.TransactionManager.StartTransaction())
                    {
                        SelectionFilter filter = new SelectionFilter(new[]
                        {
                            new TypedValue((int)DxfCode.Start,"INSERT" ),
                        });
                        var promptSelectionOptions = new PromptSelectionOptions();
                        promptSelectionOptions.RejectObjectsFromNonCurrentSpace = true;
                        promptSelectionOptions.AllowDuplicates = false;


                        var promptSelectionResult = ed.GetSelection(promptSelectionOptions, filter);
                        var selectedBlocks = new List<ObjectId>();
                        using (SelectionSet ss = promptSelectionResult.Value)
                        {
                            if (ss != null)
                                selectedBlocks.AddRange(ss.GetObjectIds().ToList());
                        }

                        var blockNames = selectedBlocks.Where(x => !Globs.IsXref(x, transaction))
                            .Select(x => Globs.GetBlockname(x, transaction)).Distinct().ToList();

                        transaction.Commit();

                        if (blockNames.Count == 0)
                        {
                            ed.WriteMessage("\nKeine Blöcke gefunden.");
                        }
                        else
                        {
                            var rows = blockNames.Select(x => new SingleBlockNameRowProvider() { Blockname = x });
                            var excelizer = new Excelizer();
                            excelizer.ExcelExport(new[] { "BLÖCKE" }, rows);
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                Log.Error(ex.Message, ex);
                Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2BlockinfoModell aufgetreten! {0}", ex.Message));
            }
        }

        //[CommandMethod("Plan2BlockinfoLayout")]
        public static void Plan2BlockinfoLayout()
        {
            try
            {
                var doc = Application.DocumentManager.MdiActiveDocument;
                var ed = doc.Editor;

                Application.SetSystemVariable("OSMODE", 0);

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

                        var blockNames = GetBlocknames(viewport, ed, transaction);

                        transaction.Commit();

                        if (blockNames.Count == 0)
                        {
                            ed.WriteMessage("\nKeine Blöcke gefunden.");
                        }
                        else
                        {
                            var rows = blockNames.Select(x => new SingleBlockNameRowProvider() { Blockname = x });
                            var excelizer = new Excelizer();
                            excelizer.ExcelExport(new[] { "BLÖCKE" }, rows);
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                Log.Error(ex.Message, ex);
                Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2BlockinfoLayout aufgetreten! {0}", ex.Message));
            }
        }

        internal static List<string> GetBlocknames(Viewport viewport, Editor ed, Transaction transaction)
        {
            Point3dCollection point3DCollectionWcs = GetWcsViewportFrame(viewport);

            Globs.SwitchToModelSpace();
            Globs.ZoomExtents();

            Point3dCollection point3DCollectionUcs = WcsToUcs(point3DCollectionWcs);

            SelectionFilter filter = new SelectionFilter(new[]
            {
                new TypedValue((int) DxfCode.Start, "INSERT"),
            });
            var promptSelectionResult = ed.SelectCrossingPolygon(point3DCollectionUcs, filter);
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
            return blockNames;
        }

        internal static Viewport SelectViewport(Transaction transaction)
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            var ed = doc.Editor;
            ed.SwitchToPaperSpace();

            var promptEntityResult = ed.GetEntity("\nViewport auswählen: ");
            if (promptEntityResult.Status != PromptStatus.OK) return null;
            return transaction.GetObject(promptEntityResult.ObjectId, OpenMode.ForRead) as Viewport;
        }

        internal static Viewport SetLayoutActiveAndGetViewport(string name, Transaction transaction)
        {
            // Reference the Layout Manager
            LayoutManager acLayoutMgr = LayoutManager.Current;

            // Open the layout
            Layout layout = transaction.GetObject(acLayoutMgr.GetLayoutId(name), OpenMode.ForRead) as Layout;
            // Set the layout current if it is not already
            // ReSharper disable once PossibleNullReferenceException
            if (layout.TabSelected == false)
            {
                acLayoutMgr.CurrentLayout = layout.LayoutName;
            }
            var viewPorts = layout.GetViewports();
            if (viewPorts.Count <= 1) return null;


            return transaction.GetObject(viewPorts[1], OpenMode.ForRead) as Viewport;
        }

        internal static Point3dCollection GetWcsViewportFrame(Viewport viewport)
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

        internal static Point3dCollection WcsToUcs(Point3dCollection point3DCollectionWcs)
        {
            var point3DCollectionUcs = new Point3dCollection();
            foreach (Point3d point3D in point3DCollectionWcs)
            {
                point3DCollectionUcs.Add(Globs.TransWcsUcs(point3D));

            }

            return point3DCollectionUcs;
        }

        // ReSharper disable once UnusedMember.Local
        private static void DrawPolyline(Transaction transaction, Document doc, Point3dCollection point3DCollection)
        {
            var blockTable = transaction.GetObject(doc.Database.BlockTableId, OpenMode.ForRead) as BlockTable;
            var blockTableRecord =
                // ReSharper disable once PossibleNullReferenceException
                transaction.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
            using (var polyline = new Polyline())
            {
                for (int i = 0; i < point3DCollection.Count; i++)
                {
                    polyline.AddVertexAt(i, To2D(point3DCollection[i]), 0, 0, 0);
                }

                // ReSharper disable once PossibleNullReferenceException
                blockTableRecord.AppendEntity(polyline);
                transaction.AddNewlyCreatedDBObject(polyline, true);
            }
        }

        private static Point2d To2D(Point3d point3D)
        {
            return new Point2d(point3D.X, point3D.Y);
        }

    }
}
