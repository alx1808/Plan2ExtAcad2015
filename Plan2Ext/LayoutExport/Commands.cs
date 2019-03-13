using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Plan2Ext.Plot;
using Plan2Ext.Properties;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
// ReSharper disable IdentifierTypo
// ReSharper disable StringLiteralTypo
// ReSharper disable CommentTypo

namespace Plan2Ext.LayoutExport
{
    // ReSharper disable once UnusedMember.Global
    public class Commands
    {
        #region log4net Initialization
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(Convert.ToString((typeof(Commands))));
        #endregion

        private static string CurrentOutDwg { get; set; }
        private static string CurrentExportDwg { get; set; }
        private static Point3d CurrentOrigin { get; set; }
        private static double CurrentScale { get; set; }
        private static double _ViewportTwistAngle;
        private static readonly List<string> ExportDwgNames = new List<string>();


        [CommandMethod("Plan2LayoutExport")]
        // ReSharper disable once UnusedMember.Global
        public static async void Plan2LayoutExport()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            dynamic acDoc = doc.GetAcadDocument();
            ExportDwgNames.Clear();
            try
            {
                var selectedLayoutNames = Layouts.GetOrderedLayoutNames(true);
                foreach (var selectedLayoutName in selectedLayoutNames)
                {
                    acDoc.StartUndoMark();

                    if (selectedLayoutName == "Model") continue;
                    Layouts.SetLayoutActive(selectedLayoutName);
                    if (Globs.IsModelspace)
                    {
                        continue;
                    }

                    SetCurrentExportDwgName();

                    Globs.CreateBakFile(CurrentExportDwg);

                    // Export Wipeout and Blockreference entities inside first view to external dwg and delete entities.
                    if (!SaveAndDeleteNonExportableEntities()) return;

                    // Export layout to <dwgprefix><layoutname>.dwg
                    await ExportLayout();

                    // Import saved entities to exported layout
                    ImportSavedEntitiesToExportedLayout();

                    // export drawing correction: Draworder correction, Purge blocks, Plotsettings, layer name correction
                    ExportDrawingCorrection();

                    Cleanup();

                    // Undo
                    acDoc.EndUndoMark();
                    await Globs.CallCommandAsync("_.U");

                    WriteHatchPolyBreiteScript();

                }
            }
            catch (System.Exception ex)
            {
                Log.Error(ex.Message, ex);
                Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, Resources.LayoutExportError,
                    ex.Message));
            }

#pragma warning disable 4014
            Globs.CallCommandAsync("_.Script", GetScriptName());
#pragma warning restore 4014

        }

        [LispFunction("Plan2LayoutExportDrawOrder")]
        // ReSharper disable once UnusedMember.Global
        // ReSharper disable once UnusedParameter.Global
        public static bool Plan2LayoutExportDrawOrder(ResultBuffer rb)
        {
            try
            {
                var db = Application.DocumentManager.MdiActiveDocument.Database;
                DraworderCorrection(db);
            }
            catch (System.Exception ex)
            {
                string msg = string.Format(CultureInfo.CurrentCulture, "Fehler in (Plan2LayoutExportDrawOrder): {0}", ex.Message);
                Log.Error(msg);
                return false;
            }

            return true;
        }

        // ReSharper disable once UnusedMember.Local
        private static void WriteHatchPolyBreiteScript()
        {
            if (ExportDwgNames.Count == 0) return;
            var sb = new StringBuilder();
            foreach (var exportDwgName in ExportDwgNames)
            {
                sb.AppendLine("_open");
                sb.AppendLine("\"" + exportDwgName +  "\"");
                sb.AppendLine("(Plan2HatchPolyBreite)");
                sb.AppendLine("(Plan2LayoutExportDrawOrder)");
                sb.AppendLine("(setvar \"LTSCALE\" 5.0)");
                sb.AppendLine("_Close");
                sb.AppendLine("_N");
            }

            var scriptName = GetScriptName();
            if (File.Exists(scriptName)) File.Delete(scriptName);
            File.WriteAllText(scriptName,sb.ToString());
        }

        private static string GetScriptName()
        {
            var curDwgName = Globs.GetCurrentDwgName();
            // ReSharper disable once AssignNullToNotNullAttribute
            var scriptName = Path.Combine(Path.GetDirectoryName(curDwgName),
                Path.GetFileNameWithoutExtension(curDwgName) + ".scr");
            return scriptName;
        }

        private static void SetCurrentExportDwgName()
        {
            var layoutName = GetCurrentLayoutName();
            var prefix = Application.GetSystemVariable("DWGPREFIX").ToString();
            var dwgName = Globs.RemoveInvalidCharacters(layoutName);

            CurrentExportDwg = Path.Combine(prefix, dwgName + ".dwg");
            ExportDwgNames.Add(CurrentExportDwg);
        }

        private static string GetCurrentLayoutName()
        {
            return LayoutManager.Current.CurrentLayout;
        }

        private static void Cleanup()
        {
            DeleteOutDwg();
        }

        private static void DeleteOutDwg()
        {
            try
            {
                if (string.IsNullOrEmpty(CurrentOutDwg)) return;
                if (File.Exists(CurrentOutDwg))
                {
                    File.Delete(CurrentOutDwg);
                }
            }
            catch (System.Exception e)
            {
                Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage(string.Format(CultureInfo.InvariantCulture, "Unable to delete file '{0}'! {1}", CurrentOutDwg, e.Message));
            }
        }

        private static void ExportDrawingCorrection()
        {
            var dirName = Path.GetDirectoryName(CurrentExportDwg);
            if (dirName == null) return;

            var newFileName = Path.Combine(dirName,
                Path.GetFileNameWithoutExtension(CurrentExportDwg) + "_X.dwg");
            using (var dbTarget = new Database(false, true))
            {
                dbTarget.ReadDwgFile(CurrentExportDwg, FileShare.Read, true, "");

                //HatchPolyBreite(dbTarget); -> via script

                //DraworderCorrection(dbTarget); -> via script

                RenameLayers(dbTarget);

                SetPlottersettings(dbTarget);

                SetLayerLineWeight(dbTarget,LineWeight.LineWeight000);

                Globs.PurgeAllSymbolTables(dbTarget);

                dbTarget.SaveAs(newFileName, DwgVersion.Newest);
            }

            Globs.Move(newFileName, CurrentExportDwg);
        }
        
        // ReSharper disable once UnusedMember.Local
        /// <summary>
        /// Creates Hatches from Polyline.
        /// </summary>
        /// <param name="db"></param>
        /// <remarks>
        /// Doesn't work yet. Creation of Entities in different Database. Maybe with wblockclone
        /// </remarks>
        private static void HatchPolyBreite(Database db)
        {
            using (var transaction = db.TransactionManager.StartTransaction())
            {
                var blockTable = (BlockTable) transaction.GetObject(db.BlockTableId, OpenMode.ForRead);
                var blockTableRecord = (BlockTableRecord)blockTable[BlockTableRecord.ModelSpace].GetObject(OpenMode.ForRead, false);
                var lwPolys = new List<Polyline>();
                foreach (var objectId in blockTableRecord)
                {
                    var poly = transaction.GetObject(objectId, OpenMode.ForRead) as Polyline;
                    if (poly == null) continue;
                    if (poly.ConstantWidth > 0.0001)
                    {
                        lwPolys.Add(poly);
                    }
                }

                foreach (var polyline in lwPolys)
                {
                    try
                    {
                        Kleinbefehle.HatchPolyBreite.CreateBoundedHatch(polyline, db);
                    }
                    catch (System.Exception e)
                    {
                        Log.Error(string.Format(CultureInfo.CurrentCulture, "Fehler in '{0}': {1}", CurrentExportDwg, e.Message));
                    }
                }

                transaction.Commit();
            }
        }

        private static void SetLayerLineWeight(Database db, LineWeight lineWeight)
        {
            using (var trans = db.TransactionManager.StartTransaction())
            {
                var layerTable = (LayerTable)trans.GetObject(db.LayerTableId, OpenMode.ForRead);
                foreach (var layerOid in layerTable)
                {
                    var layerTableRecord = (LayerTableRecord)trans.GetObject(layerOid, OpenMode.ForRead);
                    if (layerTableRecord.LineWeight != lineWeight)
                    {
                        layerTableRecord.UpgradeOpen();
                        layerTableRecord.LineWeight = lineWeight;
                        layerTableRecord.DowngradeOpen();
                    }
                }

                trans.Commit();
            }
        }


        private static void RenameLayers(Database db)
        {
            const string searchText = "$0$";
            const string replaceText = "_";
            using (var transaction = db.TransactionManager.StartTransaction())
            {
                var layerTable = (LayerTable)transaction.GetObject(db.LayerTableId, OpenMode.ForRead);

                foreach (var oid in layerTable)
                {
                    var ltr = (LayerTableRecord) transaction.GetObject(oid, OpenMode.ForRead);
                    if (!ltr.Name.Contains(searchText)) continue;

                    var layerName = ltr.Name;
                    ltr.UpgradeOpen();
                    ltr.Name = layerName.Replace(searchText, replaceText);
                    ltr.DowngradeOpen();
                }

                transaction.Commit();
            }
        }

        private static void DraworderCorrection(Database db)
        {
            var formerXrefs = new ObjectIdCollection();
            var otherEntities = new ObjectIdCollection();
            var wipeOuts = new ObjectIdCollection();
            var hatches = new ObjectIdCollection();

            using (var trans = db.TransactionManager.StartTransaction())
            {
                var bt = (BlockTable) trans.GetObject(db.BlockTableId, OpenMode.ForRead);
                var btr = (BlockTableRecord) trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);
                foreach (var oid in btr)
                {
                    var ent = trans.GetObject(oid, OpenMode.ForRead);
                    var blockRef = ent as BlockReference;
                    if (blockRef != null)
                    {
                        var blockName = Globs.GetBlockname(blockRef, trans);
                        if (blockName.StartsWith("*")) formerXrefs.Add(oid);
                    }
                    else
                    {
                        var wipeOut = ent as Wipeout;
                        if (wipeOut != null)
                        {
                            wipeOuts.Add(oid);
                        }
                        else
                        {
                            var txt = ent as DBText;
                            if (txt == null)
                            {
                                var hatch = ent as Hatch;
                                if (hatch != null &&
                                    hatch.PatternName.EndsWith("SOLID", StringComparison.OrdinalIgnoreCase))
                                {
                                    hatches.Add(oid);
                                }
                                else
                                {
                                    otherEntities.Add(oid);
                                }
                            }
                        }
                    }
                }

                trans.Commit();
            }

            if (wipeOuts.Count > 0) Globs.DrawOrderBottom(wipeOuts, db);
            if (otherEntities.Count > 0) Globs.DrawOrderBottom(otherEntities, db);
            if (hatches.Count > 0) Globs.DrawOrderBottom(hatches, db);
            if (formerXrefs.Count > 0) Globs.DrawOrderBottom(formerXrefs, db);
        }

        // ReSharper disable once UnusedMember.Local
        private static void PurgeAllBlocks(Database dbTarget)
        {
            for (var i = 0; i < 5; i++)
            {
                if (Globs.PurgeBlocks(dbTarget) == 0) break;
            }
        }

        // ReSharper disable once UnusedMember.Local
        private static void PurgeAllLayers(Database dbTarget)
        {
            for (var i = 0; i < 5; i++)
            {
                if (Globs.PurgeLayers(dbTarget) == 0) break;
            }
        }

        private static void SetPlottersettings(Database dbTarget)
        {
            var modelspaceLayoutName = "Model";
            var layoutId = Layouts.GetLayoutId("Model", dbTarget);
            if (!layoutId.IsValid)
            {
                Log.Error(string.Format(CultureInfo.CurrentCulture, Resources.LayoutDoesntExist,
                    modelspaceLayoutName));
            }

            using (var trans = dbTarget.TransactionManager.StartTransaction())
            {
                var layout = (Layout) trans.GetObject(layoutId, OpenMode.ForWrite);
                layout.SetPlotSettingsDevice("Kein");
                layout.SetPlotSettingsStyleSheet("");

                trans.Commit();
            }
        }


        private static void ImportSavedEntitiesToExportedLayout()
        {
            Globs.InsertDwgToDwg(CurrentExportDwg, CurrentOutDwg, Point3d.Origin, _ViewportTwistAngle, CurrentScale, false);
        }

        /// <summary>
        /// Can't be debugged!
        /// </summary>
        /// <returns></returns>
#pragma warning disable 1998
        private static async Task ExportLayout()
#pragma warning restore 1998
        {
            Globs.SwitchToPaperSpace();
#if DEBUG
            if (File.Exists(CurrentExportDwg)) File.Delete(CurrentExportDwg);
            File.Copy(CurrentExportDwg + ".sic", CurrentExportDwg);
#else
            await Globs.CallCommandAsync("_.ExportLayout", CurrentExportDwg);
#endif
        }

        private static bool SaveAndDeleteNonExportableEntities()
        {
            var objectIds = SelectNonExportableEntities();
            if (objectIds == null) return false;

            GetCurrentOutDwg();

            Globs.Wblock(CurrentOutDwg, objectIds, CurrentOrigin);

            Delete(objectIds);

            return true;
        }

        private static void Delete(List<ObjectId> objectIds)
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            using (var trans = doc.TransactionManager.StartTransaction())
            {
                foreach (var objectId in objectIds)
                {
                    var ent = trans.GetObject(objectId, OpenMode.ForWrite);
                    ent.Erase(true);
                }
                trans.Commit();
            }
        }


        private static void GetCurrentOutDwg()
        {
            var tmpFileName = "Out";
            var inc = 1;
            while (File.Exists(Path.Combine(Path.GetTempPath(), tmpFileName + inc + ".dwg")))
            {
                inc++;
            }

            CurrentOutDwg = Path.Combine(Path.GetTempPath(), tmpFileName + inc + ".dwg");
        }


        private static List<ObjectId> SelectNonExportableEntities()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            Globs.SwitchToModelSpace();
            Globs.ZoomExtents();
            using (var transaction = doc.TransactionManager.StartTransaction())
            {
                try
                {
                    Globs.SwitchToPaperSpace();
                    Viewport viewport;
                    if (!GetFirstViewport(transaction, doc, out viewport)) return null;
                    _ViewportTwistAngle = viewport.TwistAngle;

                    Point3dCollection point3DCollectionWcs = GetWcsViewportFrame(viewport);
                    Globs.SwitchToModelSpace();
                    Point3dCollection point3DCollectionUcs = WcsToUcs(point3DCollectionWcs);

                    SelectionFilter filter = new SelectionFilter(new[]
                        {
                            new TypedValue((int)DxfCode.Operator,"<OR" ),
                            new TypedValue((int)DxfCode.Start,"INSERT" ),
                            new TypedValue((int)DxfCode.Start,"WIPEOUT" ),
                            new TypedValue((int)DxfCode.Operator,"OR>" ),
                        });
                    var promptSelectionResult = doc.Editor.SelectCrossingPolygon(point3DCollectionUcs, filter);
                    if (promptSelectionResult.Value == null) return null;
                    using (var ss = promptSelectionResult.Value)
                    {
                        return new List<ObjectId>(ss.GetObjectIds().Where(x => IsNotXref(x, transaction)));
                    }
                }
                finally
                {
                    transaction.Commit();
                }
            }
        }

        private static bool IsNotXref(ObjectId objectId, Transaction transaction)
        {
            var br = transaction.GetObject(objectId, OpenMode.ForRead) as BlockReference;
            if (br == null) return true;
            return !Globs.IsXref(br, transaction);
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

        private static Point3dCollection GetWcsViewportFrame(Viewport viewport)
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
            var luWcs = wcsPoints[0];
            var loWcs = wcsPoints[1];
            var heightWcs = luWcs.Distance2dTo(loWcs);
            CurrentScale = viewport.Height / heightWcs;
            var retPointCollection = new Point3dCollection(wcsPoints.ToArray());

            points.Clear();
            points.Add(Point3d.Origin);
            wcsPoints.Clear();
            PaperSpaceHelper.ConvertPaperSpaceCoordinatesToModelSpaceWcs(viewport.ObjectId, points, wcsPoints);
            CurrentOrigin = wcsPoints[0];

            return retPointCollection;
        }

        private static bool GetFirstViewport(Transaction transaction, Document doc, out Viewport viewport)
        {
            // Reference the Layout Manager
            LayoutManager acLayoutMgr = LayoutManager.Current;
            // Open the layout
            Layout layout = (Layout)transaction.GetObject(acLayoutMgr.GetLayoutId(acLayoutMgr.CurrentLayout), OpenMode.ForRead);
            var viewPorts = layout.GetViewports();
            if (viewPorts.Count <= 1)
            {
                doc.Editor.WriteMessage(string.Format(CultureInfo.CurrentCulture, Resources.LayoutExportNoView));
                viewport = null;
                return false;
            }

            viewport = transaction.GetObject(viewPorts[1], OpenMode.ForRead) as Viewport;
            return true;
        }
    }
}
