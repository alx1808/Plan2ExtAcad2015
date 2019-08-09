using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;

// ReSharper disable IdentifierTypo
// ReSharper disable StringLiteralTypo

namespace Plan2Ext.BlockInfo
{
    // ReSharper disable once UnusedMember.Global
    public class BsLegende
    {
        #region log4net Initialization
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Convert.ToString((typeof(BsLegende))));
        #endregion

        private static readonly List<string> BlocksIgnored = new List<string>()
        {
            "BS_RA_RAUMBLOCk",
            "BS_AL_MASSSTABLEISTE_*",
            "BS_AL_RASTER_*",
            "BS_AL_SIEHE_PLAN",
            "BS_AL_NORDPFEIL",
            "BS_AL_PLANKOPF",
            "BS_AL_PLANKOPF_*",
            "BS_RA_RAUMBLOCK",
            "BS_AL_LEGENDE_BS_MELDERGRUPPE",
            "BS_AL_LEGENDE_BS_MELDER",
            "BS_AL_LOGO_*",
        };
        private static readonly List<string> BlocksAlwaysInLegend = new List<string>()
        {
            "PLK_BS_BA_FWKL_PROTO",
            "PLK_BS_AL_NORDPFEIL",
            "PLK_BS_AL_MASSSTABLEISTE_PROTO",
            "PLK_BS_BA_BRANDABSCHNITTSGRENZE",
            "PLK_BS_BA_RAUCHSCHÜRZE"
        };
        private const string LEGEND_BLOCK_PREFIX = "PLK_";
        private const double VERTICAL_DISTANCE = 7.8104;
        private const double HORIZONTAL_DISTANCE = 10.0;
        private const double FRAME_OFFSET = 3.0;
        private static int _NrOfVerticalBlockElements = 5;
        private static double _ScaleFactor = 1.0;
        private static IGetsFromUser _GetsFromUser;
        private static IProtoDwgInfo _ProtoDwgInfo;
        private static ILegendInserter _LegendInserter;
        private static WildcardAcad[] _BlocksIgnoredWildcards;


        [CommandMethod("Plan2BsLegende")]
        // ReSharper disable once UnusedMember.Global
        public static void Plan2BsLegende()
        {
            try
            {
                var doc = Application.DocumentManager.MdiActiveDocument;
                var ed = doc.Editor;
                _GetsFromUser = new GetsFromUser();
                _ProtoDwgInfo = new ProtoDwgInfo();
                _LegendInserter = new LegendInserter(VERTICAL_DISTANCE, HORIZONTAL_DISTANCE, FRAME_OFFSET);
                _BlocksIgnoredWildcards = BlocksIgnored.Select(x => new WildcardAcad(x)).ToArray();

                var prototypedwgName = GetPrototypedwgName();
                if (prototypedwgName == null) return;

                _GetsFromUser.GetNrOfVerticalBlockElements(ed, ref _NrOfVerticalBlockElements);
                _GetsFromUser.GetScaleFactor(ed, ref _ScaleFactor);

                Plan2BsLegendeModell(prototypedwgName);
            }
            catch (System.Exception ex)
            {
                Log.Error(ex.Message, ex);
                Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2BsLegende aufgetreten! {0}", ex.Message));
            }
        }

        private static string GetPrototypedwgName()
        {
            var fileNames = new[] { "BS_LEGENDE_KAV.dwg", "BS_LEGENDE_NORM.dwg", "BS_LEGENDE_CARLO.dwg" };
            var keywords = new[] { "Kav", "Norm", "Carlo" };

            var keyWord = Globs.AskKeywordFromUser("Prototyp für Legende eingeben", keywords, 2);
            return keyWord == null ? null : fileNames[keywords.ToList().IndexOf(keyWord)];
        }

        private static void Plan2BsLegendeModell(string prototypedwgName)
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
                            _LegendInserter.InsertLegend(orderedBlocksInProtodwg, legendBlockNames, prototypedwgName, positionWcs, transaction,_ScaleFactor, _NrOfVerticalBlockElements);
                            Globs.PurgeBlocks(legendBlockNames.ToList());
                        }
                        transaction.Commit();
                    }
                }
            }
            catch (System.Exception ex)
            {
                Log.Error(ex.Message, ex);
                Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2BsLegendeModell aufgetreten! {0}", ex.Message));
            }
        }
        private static HashSet<string> GetLegendBlockNames(List<string> blockNames)
        {
            var blToLegendBlockNames =
                blockNames.Where(x => !IsIgnoredBlock(x)).Select(x => LEGEND_BLOCK_PREFIX + x).ToList();
            var legendBlockNames = new HashSet<string>(blToLegendBlockNames);
            legendBlockNames.UnionWith(BlocksAlwaysInLegend);
            return legendBlockNames;
        }

        private static bool IsIgnoredBlock(string name)
        {
            return _BlocksIgnoredWildcards.Any(x => x.IsMatch(name));
        }

    }
}
