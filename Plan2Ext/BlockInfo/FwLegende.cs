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
        private const double VerticalDistance = 7.8104;
        private const double HorizontalDistance = 10.0;
        private static int _nrOfVerticalBlockElements = 5;
        private static double _scaleFactor = 1.0;
        private static double _frameOffset = 3.0;

        [CommandMethod("Plan2FwLegende")]
        // ReSharper disable once UnusedMember.Global
        public static void Plan2FwLegende()
        {
            try
            {
                var doc = Application.DocumentManager.MdiActiveDocument;
                var ed = doc.Editor;

                Application.SetSystemVariable("OSMODE", 0);

                var prototypedwgName = GetPrototypedwgName(ed);
                if (prototypedwgName == null) return;

                GetNrOfVerticalBlockElements(ed);
                GetScaleFactor(ed);

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

        private static void GetNrOfVerticalBlockElements(Editor ed)
        {
            var resultInteger = ed.GetInteger(new PromptIntegerOptions(string.Format(CultureInfo.CurrentCulture,
                "\nAnzahl vertikaler Blockelemente in Legende <{0}>:", _nrOfVerticalBlockElements))
            {
                AllowNegative = false,
                AllowArbitraryInput = false,
                AllowZero = false,
                AllowNone = true,
            });
            if (resultInteger.Status == PromptStatus.OK) _nrOfVerticalBlockElements = resultInteger.Value;
        }

        private static void GetScaleFactor(Editor ed)
        {
            var resultDouble = ed.GetDouble(new PromptDoubleOptions(string.Format(CultureInfo.CurrentCulture,
                "\nSkalierfaktor <{0}>:", _scaleFactor))
            {
                AllowNegative = false,
                AllowZero = false,
                AllowArbitraryInput = false,
                AllowNone = true,
            });
            if (resultDouble.Status == PromptStatus.OK) _scaleFactor = resultDouble.Value;
        }

        private static string GetPrototypedwgName(Editor ed)
        {
            var pKeyOpts = new PromptKeywordOptions("") { Message = "\nPrototyp für Legende eingeben Kav/Norm/<Carlo>: " };
            pKeyOpts.Keywords.Add("Kav");
            pKeyOpts.Keywords.Add("Norm");
            pKeyOpts.Keywords.Add("Carlo");
            pKeyOpts.AllowNone = true;

            PromptResult pKeyRes = ed.GetKeywords(pKeyOpts);
            if (pKeyRes.Status == PromptStatus.None || pKeyRes.StringResult == "Carlo")
            {
                return "FW_LEGENDE_CARLO.dwg";
            }
            if (pKeyRes.Status == PromptStatus.OK)
            {
                if (pKeyRes.StringResult == "Norm")
                {
                    return "FW_LEGENDE_NORM.dwg";
                }
                else
                {
                    return "FW_LEGENDE_KAV.dwg";
                }
            }

            return null;
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
                    var orderedBlocksInProtodwg = GetOrderedBlocknames(prototypedwgName);
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
                            InsertLegend(orderedBlocksInProtodwg, legendBlockNames, prototypedwgName, positionWcs, transaction);
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

                    var orderedBlocksInProtodwg = GetOrderedBlocknames(prototypedwgName);
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
                            InsertLegend(orderedBlocksInProtodwg, legendBlockNames, prototypedwgName, positionWcs, transaction);
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

                    var orderedBlocksInProtodwg = GetOrderedBlocknames(prototypedwgName);
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
                            InsertLegend(orderedBlocksInProtodwg, legendBlockNames, prototypedwgName, positionWcs, transaction);
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

        private static void InsertLegend(List<string> blocksInProtodwg, HashSet<string> legendBlockNames,
            string prototypedwgName, Point3d positionWcs, Transaction transaction)
        {

            var scaleFactor = _scaleFactor;
            var positionUcs = Globs.TransWcsUcs(positionWcs);
            var origPositionY = positionUcs.Y;

            var verticalIncrement = VerticalDistance * -1.0 * scaleFactor;
            var horizontalAddition = HorizontalDistance * scaleFactor;

            var positionUcsX = positionUcs.X;
            var verticalNr = 0;

            var ucsPointList = new List<Point3d>();

            foreach (var legendBlockname in blocksInProtodwg)
            {
                if (legendBlockNames.Contains(legendBlockname))
                {
                    double newOPositionUcsX;
                    if (!InsertLocalOrFromProto(legendBlockname, positionUcs, prototypedwgName, explode: true,
                        transaction: transaction, scaleFactor: scaleFactor, ucsPointList: ucsPointList,  newPositionX: out newOPositionUcsX)) continue;
                    if (newOPositionUcsX > positionUcsX) positionUcsX = newOPositionUcsX;
                    verticalNr += 1;
                    // insertpoint up and right
                    if (verticalNr >= _nrOfVerticalBlockElements)
                    {
                        verticalNr = 0;
                        positionUcs = new Point3d(positionUcsX + horizontalAddition, origPositionY, 0);
                    }
                    // insertpoint down
                    else positionUcs += new Vector3d(0, verticalIncrement, 0);
                }
            }

            // frame
            var framePointsUcs = Boundings.GetRectanglePointsFromBounding(buffer: _frameOffset * scaleFactor, pts: ucsPointList);
            var wcsPointList = framePointsUcs.Select(Globs.TransUcsWcs).ToList();
            var wcs2DPointList = wcsPointList.ToList2D();
            var bnd = Boundings.CreatePolyline(wcs2DPointList, closed: true);
            bnd.Layer = "0";
            var btr = (BlockTableRecord)transaction.GetObject(Application.DocumentManager.MdiActiveDocument.Database.CurrentSpaceId, OpenMode.ForWrite);
            btr.AppendEntity(bnd);
            transaction.AddNewlyCreatedDBObject(bnd, true);

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

        /// <summary>
        /// Inserts block local or from proto
        /// </summary>
        /// <param name="blockName"></param>
        /// <param name="positionUcs"></param>
        /// <param name="dwgName"></param>
        /// <param name="explode"></param>
        /// <param name="transaction"></param>
        /// <param name="scaleFactor"></param>
        /// <param name="ucsPointList"></param>
        /// <param name="newPositionX"></param>
        /// <returns>X-Value of boundary on the right side in UCS</returns>
        private static bool InsertLocalOrFromProto(string blockName, Point3d positionUcs, string dwgName, bool explode, Transaction transaction, double scaleFactor, List<Point3d> ucsPointList,  out double newPositionX)
        {
            newPositionX = 0.0;

            Document doc = Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;

            if (!Globs.BlockExists(blockName) && !Globs.InsertFromPrototype(blockName, dwgName)) return false;
            var blockTable = (BlockTable)transaction.GetObject(db.BlockTableId, OpenMode.ForRead);
            var oid = blockTable[blockName];
            using (var bref = new BlockReference(Globs.TransUcsWcs(positionUcs), oid))
            {
                bref.ScaleFactors = new Scale3d(scaleFactor);
                bref.Rotation = Globs.GetUcsDirection();
                var acCurSpaceBlkTblRec = (BlockTableRecord)transaction.GetObject(db.CurrentSpaceId, OpenMode.ForWrite);
                acCurSpaceBlkTblRec.AppendEntity(bref);
                transaction.AddNewlyCreatedDBObject(bref, true);

                var boundingPointsWcs = Boundings.CollectPointsWcs(transaction, bref);
                var ptsUcs = boundingPointsWcs.ToList().Select(Globs.TransWcsUcs).ToList();
                var recPointsUcs = Boundings.GetRectanglePointsFromBounding(buffer: 0.0, pts: ptsUcs);
                ucsPointList.AddRange(recPointsUcs);
                newPositionX = recPointsUcs.Select(x => x.X).Max();

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

            return true;
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
