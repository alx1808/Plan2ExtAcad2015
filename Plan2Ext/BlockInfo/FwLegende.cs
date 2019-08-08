using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
        private const string LEGEND_BLOCK_PREFIX = "PLK_";
        private const double VERTICAL_DISTANCE = 7.8104;
        private const double HORIZONTAL_DISTANCE = 10.0;
        private const double FRAME_OFFSET = 3.0;
        private static int _NrOfVerticalBlockElements = 5;
        private static double _ScaleFactor = 1.0;
        private static IGetsFromUser _GetsFromUser;
        private static IProtoDwgInfo _ProtoDwgInfo;
        private static ILegendInserter _LegendInserter;

        [CommandMethod("Plan2FwLegende")]
        // ReSharper disable once UnusedMember.Global
        public static void Plan2FwLegende()
        {
            try
            {
                var doc = Application.DocumentManager.MdiActiveDocument;
                var ed = doc.Editor;
                _GetsFromUser = new GetsFromUser();
                _ProtoDwgInfo = new ProtoDwgInfo();
                _LegendInserter = new LegendInserter(VERTICAL_DISTANCE, HORIZONTAL_DISTANCE, FRAME_OFFSET);

                Application.SetSystemVariable("OSMODE", 0);

                var prototypedwgName = GetPrototypedwgName();
                if (prototypedwgName == null) return;

                _GetsFromUser.GetNrOfVerticalBlockElements(ed, ref _NrOfVerticalBlockElements);
                _GetsFromUser.GetScaleFactor(ed, ref _ScaleFactor);

                var pKeyOpts = new PromptKeywordOptions("") { Message = "\nOption eingeben Model/Layout/<All>: " };
                pKeyOpts.Keywords.Add(Globs.IsModelspace ? "Model" : "Layout");
                pKeyOpts.Keywords.Add("All");
                pKeyOpts.AllowNone = true;

                PromptResult pKeyRes = ed.GetKeywords(pKeyOpts);
                if (pKeyRes.Status == PromptStatus.None || pKeyRes.StringResult == "All")
                {
                    Plan2FwLegendeAll(prototypedwgName);
                }
                else if (pKeyRes.Status == PromptStatus.OK)
                {
                    if (pKeyRes.StringResult == "Layout")
                    {
                        Plan2FwLegendeLayout(prototypedwgName);
                    }
                    else
                    {
                        Plan2FwLegendeModell(prototypedwgName);
                    }
                }
            }
            catch (System.Exception ex)
            {
                Log.Error(ex.Message, ex);
                Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2FwLegende aufgetreten! {0}", ex.Message));
            }
        }

        private static string GetPrototypedwgName()
        {
            var fileNames = new[] { "FW_LEGENDE_KAV.dwg", "FW_LEGENDE_NORM.dwg", "FW_LEGENDE_CARLO.dwg" };
            var keywords = new[] { "Kav", "Norm", "Carlo" };

            var keyWord = Globs.AskKeywordFromUser("Prototyp für Legende eingeben", keywords, 2);
            return keyWord == null ? null : fileNames[keywords.ToList().IndexOf(keyWord)];
        }

        private static void Plan2FwLegendeAll(string prototypedwgName)
        {
            try
            {
                var doc = Application.DocumentManager.MdiActiveDocument;
                var ed = doc.Editor;

                Application.SetSystemVariable("OSMODE", 0);

                using (doc.LockDocument())
                {
                    var orderedBlocksInProtodwg = _ProtoDwgInfo.GetOrderedBlocknames(prototypedwgName);
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
                            _LegendInserter.InsertLegend(orderedBlocksInProtodwg, legendBlockNames, prototypedwgName, positionWcs, transaction, _ScaleFactor, _NrOfVerticalBlockElements);
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

        private static void Plan2FwLegendeModell(string prototypedwgName)
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

                    var orderedBlocksInProtodwg = _ProtoDwgInfo.GetOrderedBlocknames(prototypedwgName);
                    if (orderedBlocksInProtodwg == null) return;

                    using (var transaction = doc.TransactionManager.StartTransaction())
                    {
                        SelectionFilter filter = new SelectionFilter(new[]
                        {
                            new TypedValue((int)DxfCode.Start,"INSERT" ),
                        });
                        var promptSelectionOptions = new PromptSelectionOptions
                        {
                            RejectObjectsFromNonCurrentSpace = true,
                            AllowDuplicates = false
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
                            _LegendInserter.InsertLegend(orderedBlocksInProtodwg, legendBlockNames, prototypedwgName, positionWcs, transaction, _ScaleFactor, _NrOfVerticalBlockElements);
                            Globs.PurgeBlocks(legendBlockNames.ToList());
                        }
                        transaction.Commit();
                    }
                }
            }
            catch (System.Exception ex)
            {
                Log.Error(ex.Message, ex);
                Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2FwLegendeModell aufgetreten! {0}", ex.Message));
            }
        }


        private static void Plan2FwLegendeLayout(string prototypedwgName)
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

                    var orderedBlocksInProtodwg = _ProtoDwgInfo.GetOrderedBlocknames(prototypedwgName);
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
                            _LegendInserter.InsertLegend(orderedBlocksInProtodwg, legendBlockNames, prototypedwgName, positionWcs, transaction, _ScaleFactor, _NrOfVerticalBlockElements);
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
                blockNames.Where(x => !BlocksIgnored.Contains(x)).Select(x => LEGEND_BLOCK_PREFIX + x).ToList();
            var legendBlockNames = new HashSet<string>(blToLegendBlockNames);
            legendBlockNames.UnionWith(BlocksAlwaysInLegend);
            return legendBlockNames;
        }
    }
}
