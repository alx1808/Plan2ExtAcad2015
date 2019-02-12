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
    // ReSharper disable once UnusedMember.Global
    public class FwLegende
    {
        #region log4net Initialization
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Convert.ToString((typeof(FwLegende))));
        #endregion

        private static readonly List<string> BlocksIgnored = new List<string>() { "FW_RA_RAUMBLOCK", "FW_BA_STIEGENÜBERSICHT", "FW_BA_AUFZUG" };
        private static readonly List<string> BlocksAlwaysInLegend = new List<string>() { "PLK_FW_BA_STANDORT", "PLK_FW_BA_SAMMELPLATZ" };
        private const string LegendBlockPrefix = "PLK_";
        private const string LegendBlockDwg = "FW_Legende.dwg";
        private const double VerticalDistance = -7.8104;

        [CommandMethod("Plan2FwLegende")]
        // ReSharper disable once UnusedMember.Global
        public static void Plan2FwLegende()
        {
            try
            {
                var doc = Application.DocumentManager.MdiActiveDocument;
                var ed = doc.Editor;

                Application.SetSystemVariable("OSMODE", 0);

                var pKeyOpts = new PromptKeywordOptions("") { Message = "\nOption eingeben Model/Layout/<All>: " };
                pKeyOpts.Keywords.Add(Globs.IsModelspace ? "Model" : "Layout");
                pKeyOpts.Keywords.Add("All");
                pKeyOpts.AllowNone = true;

                PromptResult pKeyRes = ed.GetKeywords(pKeyOpts);
                if (pKeyRes.Status == PromptStatus.None || pKeyRes.StringResult == "All")
                {
                    Plan2FwLegendeAll();
                }
                else if (pKeyRes.Status == PromptStatus.OK)
                {
                    if (pKeyRes.StringResult == "Layout")
                    {
                        Plan2FwLegendeLayout();
                    }
                    else
                    {
                        Plan2FwLegendeModell();
                    }
                }
            }
            catch (System.Exception ex)
            {
                Log.Error(ex.Message, ex);
                Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2FwLegende aufgetreten! {0}", ex.Message));
            }
        }

        private static void Plan2FwLegendeAll()
        {
            try
            {
                var doc = Application.DocumentManager.MdiActiveDocument;
                var ed = doc.Editor;

                Application.SetSystemVariable("OSMODE", 0);

                using (doc.LockDocument())
                {
                    var orderedBlocksInProtodwg = GetOrderedBlocknames(LegendBlockDwg);
                    if (orderedBlocksInProtodwg == null) return;


                    Globs.SwitchToModelSpace();
                    Globs.ZoomExtents();

                    using (var transaction = doc.TransactionManager.StartTransaction())
                    {
                        var layoutNames = Layouts.GetOrderedLayoutNames();

                        foreach (var layoutName in layoutNames)
                        {
                            if (layoutName == "Model") continue;
                            var viewport = Commands.SetLayoutActiveAndGetViewport(layoutName, transaction);
                            if (viewport == null) continue;

                            Point3dCollection point3DCollectionWcs = Commands.GetWcsViewportFrame(viewport);
                            Globs.SwitchToModelSpace();
                            Point3dCollection point3DCollectionUcs = Commands.WcsToUcs(point3DCollectionWcs);
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

                            Globs.SwitchToPaperSpace();
                            var blockNames = selectedBlocks.Where(x => !Globs.IsXref(x, transaction))
                                .Select(x => Globs.GetBlockname(x, transaction)).Distinct().ToList();

                            var positionWcs = Globs.TransUcsWcs(new Point3d(-200, 0, 0));
                            var legendBlockNames = GetLegendBlockNames(blockNames);
                            InsertLegend(orderedBlocksInProtodwg, legendBlockNames, positionWcs, transaction);
                            Globs.PurgeBlocks(legendBlockNames.ToList());

                        }

                        transaction.Commit();
                    }
                }
            }
            catch (System.Exception ex)
            {
                Log.Error(ex.Message, ex);
                Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2BlockinfoLayoutAll aufgetreten! {0}", ex.Message));
            }
        }

        private static void Plan2FwLegendeModell()
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

                    var orderedBlocksInProtodwg = GetOrderedBlocknames(LegendBlockDwg);
                    if (orderedBlocksInProtodwg == null) return;

                    using (var transaction = doc.TransactionManager.StartTransaction())
                    {
                        SelectionFilter filter = new SelectionFilter(new[]
                        {
                            new TypedValue((int)DxfCode.Start,"INSERT" ),
                        });
                        var promptSelectionOptions = new PromptSelectionOptions
                        {
                            RejectObjectsFromNonCurrentSpace = true, AllowDuplicates = false
                        };


                        var promptSelectionResult = ed.GetSelection(promptSelectionOptions, filter);
                        var selectedBlocks = new List<ObjectId>();
                        using (SelectionSet ss = promptSelectionResult.Value)
                        {
                            if (ss != null)
                                selectedBlocks.AddRange(ss.GetObjectIds().ToList());
                        }

                        var blockNames = selectedBlocks.Where(x => !Globs.IsXref(x, transaction))
                            .Select(x => Globs.GetBlockname(x, transaction)).Distinct().ToList();

                        var result = ed.GetPoint("\nEinfügepunkt der Legende: ");
                        if (result.Status == PromptStatus.OK)
                        {
                            var positionWcs = Globs.TransUcsWcs(result.Value);
                            var legendBlockNames = GetLegendBlockNames(blockNames);
                            InsertLegend(orderedBlocksInProtodwg, legendBlockNames, positionWcs, transaction);
                            Globs.PurgeBlocks(legendBlockNames.ToList());
                        }
                        transaction.Commit();
                    }
                }
            }
            catch (System.Exception ex)
            {
                Log.Error(ex.Message, ex);
                Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2BlockinfoModell aufgetreten! {0}", ex.Message));
            }
        }


        private static void Plan2FwLegendeLayout()
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

                    var orderedBlocksInProtodwg = GetOrderedBlocknames(LegendBlockDwg);
                    if (orderedBlocksInProtodwg == null) return;

                    using (var transaction = doc.TransactionManager.StartTransaction())
                    {
                        var viewport = Commands.SelectViewport(transaction);
                        if (viewport == null) return;

                        var blockNames = Commands.GetBlocknames(viewport, ed, transaction);

                        var result = ed.GetPoint("\nEinfügepunkt der Legende: ");
                        if (result.Status == PromptStatus.OK)
                        {
                            var positionWcs = Globs.TransUcsWcs(result.Value);
                            var legendBlockNames = GetLegendBlockNames(blockNames);
                            InsertLegend(orderedBlocksInProtodwg,legendBlockNames, positionWcs, transaction);
                            Globs.PurgeBlocks(legendBlockNames.ToList());
                        }

                        transaction.Commit();
                    }
                }
            }
            catch (System.Exception ex)
            {
                Log.Error(ex.Message, ex);
                Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2BlockinfoLayout aufgetreten! {0}", ex.Message));
            }
        }

        private static HashSet<string> GetLegendBlockNames(List<string> blockNames)
        {
            var blToLegendBlockNames =
                blockNames.Where(x => !BlocksIgnored.Contains(x)).Select(x => LegendBlockPrefix + x).ToList();
            var legendBlockNames = new HashSet<string>(blToLegendBlockNames);
            legendBlockNames.UnionWith(BlocksAlwaysInLegend);
            return legendBlockNames;
        }

        private static void InsertLegend(List<string> blocksInProtodwg, HashSet<string> legendBlockNames, Point3d positionWcs, Transaction transaction)
        {
            foreach (var legendBlockname in blocksInProtodwg)
            {
                if (legendBlockNames.Contains(legendBlockname))
                {
                    InsertLocalOrFromProto(legendBlockname, positionWcs, LegendBlockDwg, explode: true, transaction: transaction);
                    positionWcs += new Vector3d(0, VerticalDistance, 0);
                }
            }

            Document doc = Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;

            // missing blocks in proto as text
            foreach (var legendBlockName in legendBlockNames)
            {
                if (blocksInProtodwg.Contains(legendBlockName)) continue;
                using (var text = new DBText())
                {
                    text.Height = 3.0;
                    text.TextString = legendBlockName;
                    text.Position = positionWcs;
                    text.Layer = "0";
                    var acCurSpaceBlkTblRec = (BlockTableRecord)transaction.GetObject(db.CurrentSpaceId, OpenMode.ForWrite);
                    acCurSpaceBlkTblRec.AppendEntity(text);
                    transaction.AddNewlyCreatedDBObject(text, true);
                }

                positionWcs += new Vector3d(0, VerticalDistance, 0);
            }


        }

        private static void InsertLocalOrFromProto(string blockName, Point3d position, string dwgName, bool explode, Transaction transaction)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;

            if (!Globs.BlockExists(blockName) && !Globs.InsertFromPrototype(blockName, dwgName)) return;
            var blockTable = (BlockTable)transaction.GetObject(db.BlockTableId, OpenMode.ForRead);
            var oid = blockTable[blockName];
            using (var bref = new BlockReference(position, oid))
            {
                var acCurSpaceBlkTblRec = (BlockTableRecord)transaction.GetObject(db.CurrentSpaceId, OpenMode.ForWrite);
                acCurSpaceBlkTblRec.AppendEntity(bref);
                transaction.AddNewlyCreatedDBObject(bref, true);

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
        }


        public static List<string> GetOrderedBlocknames(string protoDwgName)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            string protoDwgFullPath;
            try
            {
                protoDwgFullPath = HostApplicationServices.Current.FindFile(protoDwgName, doc.Database, FindFileHint.Default);
            }
            catch (Exception)
            {
                doc.Editor.WriteMessage(string.Format(CultureInfo.CurrentCulture, "\nKonnte Prototypzeichnung '{0}' nicht finden!", protoDwgName));
                return null;
            }

            using (var openDb = new Database(buildDefaultDrawing: false, noDocument: true))
            {
                openDb.ReadDwgFile(protoDwgFullPath, System.IO.FileShare.ReadWrite, allowCPConversion: true, password: "");
                using (var tr = openDb.TransactionManager.StartTransaction())
                {
                    var btr = (BlockTableRecord)tr.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(openDb),
                        OpenMode.ForRead);
                    var blockList = new List<BlockReference>();
                    foreach (var oid in btr)
                    {
                        var blockReference = tr.GetObject(oid, OpenMode.ForRead) as BlockReference;
                        if (blockReference != null) blockList.Add(blockReference);
                    }

                    tr.Commit();

                    var orderedBlocks = blockList.OrderBy(x => x.Position.Y).Reverse();
                    return orderedBlocks.Select(x => Globs.GetBlockname(x, tr)).Distinct().ToList();
                }
            }
        }

    }
}
