//using Autodesk.AutoCAD.ApplicationServices;
//using Autodesk.AutoCAD.DatabaseServices;
//using Autodesk.AutoCAD.EditorInput;
//using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

#if BRX_APP
using _AcAp = Bricscad.ApplicationServices;
using Teigha.DatabaseServices;
using Bricscad.EditorInput;
using Teigha.Geometry;
#elif ARX_APP
using _AcAp = Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
#endif

namespace Plan2Ext.HoehenPruefung
{
    internal class Engine
    {
        #region log4net Initialization
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Convert.ToString((typeof(Engine))));
        #endregion

        #region Constants
        public const string FEHLER_LINE_LAYER = "_HÖHENPRÜFUNG_FEHLER";
        public const string HOEPR_OK_DICT_NAME = "Plan2HoePrOk";
        public const string HOEHENKOTEN_BLOCK_2 = "HÖHENK_GROSS";
        #endregion

        #region Members
        private TransactionManager _TransMan = null;
        private HoePrOptions _HoePrOptions = null;

        List<ObjectId> _HKBlocks = new List<ObjectId>();
        List<ObjectId> _VermPunkte = new List<ObjectId>();
        List<ObjectId> _RaumPolygons = new List<ObjectId>();

        private int _NrOfErrors = 0;

        #endregion

        #region Lifecycle
        public Engine(HoePrOptions options)
        {
            _HoePrOptions = options;
            FbRaumInfo.HoePrOptions = options;
            _TransMan = _AcAp.Application.DocumentManager.MdiActiveDocument.Database.TransactionManager;
        }
        #endregion

        #region Fußbodenprüfung

        internal void CheckFb(bool ignoreFirstCancel = false)
        {
            log.Info("CheckFb");

            if (!CheckBlockAndAtt()) return;

            _NrOfErrors = 0;

            Plan2Ext.Globs.DeleteFehlerLines(FEHLER_LINE_LAYER);

            if (!SelectZuRaumIdEntities(ignoreFirstCancel)) return;

            List<FbRaumInfo> raumInfos = new List<FbRaumInfo>();
            foreach (var oid in _RaumPolygons)
            {
                raumInfos.Add(new FbRaumInfo(oid, _HKBlocks, _VermPunkte, _TransMan));
            }
            log.InfoFormat(CultureInfo.CurrentCulture, "Anzahl der gefundenen Räume: {0}", raumInfos.Count);

            using (var myTrans = _TransMan.StartTransaction())
            {
                try
                {
                    CheckFb(raumInfos.Where(x => x.TheStatus == FbRaumInfo.Status.Ok), myTrans);
                }
                catch (Exception ex)
                {
                    log.Error(ex.Message, ex);
                }

                myTrans.Commit();
            }

            var errors = raumInfos.SelectMany(x => x.ErrorPositions).ToList();
            Plan2Ext.Globs.InsertFehlerLines(errors, FEHLER_LINE_LAYER);


            if (errors.Count > 0)
            {
                string msg = string.Format(CultureInfo.CurrentCulture, "Anzahl der Fehler: {0}.", errors.Count);
                _AcAp.Application.ShowAlertDialog(msg);
                log.Warn(msg);
            }
            else
            {
                string msg = string.Format(CultureInfo.CurrentCulture, "Die Fußboden-Höhenprüfung wurde erfolgreich beendet.");
                _AcAp.Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage(msg);
                log.Info(msg);
            }

        }

        private void CheckFb(IEnumerable<FbRaumInfo> rauminfos, Transaction trans)
        {
            foreach (var ri in rauminfos)
            {
                ri.CheckFb();
            }
        }

        private bool SelectZuRaumIdEntities(bool ignoreFirstCancel)
        {
            _HKBlocks.Clear();
            _VermPunkte.Clear();
            _RaumPolygons.Clear();

            //LayerOnAndThaw(new List<string> { _HoePrOptions.PolygonLayer, "X_FH_SYMB", "X_SPKT_SYMB", "X_GH_SYMB", "X_DR_SYMB", FEHLER_LINE_LAYER });
            LayerOnAndThawRegex(new List<string> { "^" + _HoePrOptions.PolygonLayer + "$", "^X", "^" + FEHLER_LINE_LAYER + "$" });

            var ed = _AcAp.Application.DocumentManager.MdiActiveDocument.Editor;
            SelectionFilter filter = new SelectionFilter(new TypedValue[] { 
                new TypedValue((int)DxfCode.Operator,"<OR" ),

                new TypedValue((int)DxfCode.Operator,"<AND" ),
                    new TypedValue((int)DxfCode.Start,"INSERT" ),
                    new TypedValue((int)DxfCode.BlockName, _HoePrOptions.HKBlockname ),
                new TypedValue((int)DxfCode.Operator,"AND>" ),

                // zusätzlicher höhenkotenblock
                new TypedValue((int)DxfCode.Operator,"<AND" ),
                    new TypedValue((int)DxfCode.Start,"INSERT" ),
                    new TypedValue((int)DxfCode.BlockName, HOEHENKOTEN_BLOCK_2 ),
                new TypedValue((int)DxfCode.Operator,"AND>" ),

                new TypedValue((int)DxfCode.Operator,"<AND" ),
                    new TypedValue((int)DxfCode.Start,"INSERT" ),
                    new TypedValue((int)DxfCode.BlockName, "GEOINOVA" ),
                    new TypedValue((int)DxfCode.Operator,"<OR" ),
                        new TypedValue((int)DxfCode.LayerName, "X_FH_SYMB"),
                        new TypedValue((int)DxfCode.LayerName, "X_SPKT_SYMB"),
                        new TypedValue((int)DxfCode.LayerName, "X_GH_SYMB"),
                        new TypedValue((int)DxfCode.LayerName, "X_DR_SYMB"),
                    new TypedValue((int)DxfCode.Operator,"OR>" ),
                new TypedValue((int)DxfCode.Operator,"AND>" ),

                new TypedValue((int)DxfCode.Operator,"<AND" ),
                    new TypedValue((int)DxfCode.Start,"*POLYLINE" ),
                    new TypedValue((int)DxfCode.LayerName, _HoePrOptions.PolygonLayer),
                new TypedValue((int)DxfCode.Operator,"AND>" ),
                new TypedValue((int)DxfCode.Operator,"OR>" ),

            });

            PromptSelectionResult res = ed.GetSelection(filter);
            if (res.Status == PromptStatus.Cancel && ignoreFirstCancel)
            {
                res = ed.GetSelection(filter);
            }
            if (res.Status != PromptStatus.OK) return false;

            List<ObjectId> allEntities = null;
#if BRX_APP
            SelectionSet ss = res.Value;
#else
            using (SelectionSet ss = res.Value)
#endif
            {
                allEntities = ss.GetObjectIds().ToList();
            }

            if (allEntities == null || allEntities.Count == 0) return false;

            bool someMarkedAsOk = false;

            using (Transaction myT = _TransMan.StartTransaction())
            {
                foreach (var oid in allEntities)
                {
                    Entity ent = _TransMan.GetObject(oid, OpenMode.ForRead) as Entity;
                    if (ent == null) continue;
                    BlockReference blockRef = ent as BlockReference;
                    if (blockRef != null)
                    {
                        var bd = (BlockTableRecord)_TransMan.GetObject(blockRef.BlockTableRecord, OpenMode.ForRead);

                        if (
                            (string.Compare(_HoePrOptions.HKBlockname, blockRef.Name, StringComparison.OrdinalIgnoreCase) == 0) ||
                            (string.Compare(HOEHENKOTEN_BLOCK_2, blockRef.Name, StringComparison.OrdinalIgnoreCase) == 0)
                            )
                        {
                            //HKBlocks
                            if (!MarkedAsOk(oid))
                            {
                                _HKBlocks.Add(oid);
                            }
                            else
                            {
                                Plan2Ext.Globs.Stern(blockRef.Position, 1.0, 32, color: 3, highLighted: false);
                                someMarkedAsOk = true;
                            }
                        }
                        else
                        {
                            _VermPunkte.Add(oid);
                        }
                    }
                    else
                    {
                        //Polylines
                        if (string.Compare(_HoePrOptions.PolygonLayer, ent.Layer, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            _RaumPolygons.Add(oid);
                        }
                    }
                }
                myT.Commit();
            }

            if (someMarkedAsOk) log.Warn("Einige Blöcke wurden ignoriert, da sie als OK markiert wurden.");

            if (_HKBlocks.Count == 0) log.Warn("Es wurden keine Höhenkoten gefunden!");
            else log.InfoFormat(CultureInfo.CurrentCulture, "Anzahl der Höhenkoten: {0}", _HKBlocks.Count);

            if (_VermPunkte.Count == 0) log.Warn("Es wurden keine passenden Vermessungspunkte gefunden!");
            else log.InfoFormat(CultureInfo.CurrentCulture, "Anzahl der Vermessungspunkte: {0}", _VermPunkte.Count);

            if (_RaumPolygons.Count == 0)
            {
                string msg = "Es wurden keine Raumpolylinien gewählt!";
                log.Warn(msg);
                _AcAp.Application.ShowAlertDialog(msg);
            }
            else log.InfoFormat(CultureInfo.CurrentCulture, "Anzahl der Raumpolylinien: {0}", _RaumPolygons.Count);

            if (_HKBlocks.Count > 0 && _VermPunkte.Count > 0 && _RaumPolygons.Count > 0) return true;
            else return false;
        }

        private bool MarkedAsOk(ObjectId oid)
        {
            ResultBuffer rb = Globs.GetXrecord(oid, HOEPR_OK_DICT_NAME);
            if (rb == null) return false;
            TypedValue[] values = rb.AsArray();

            return (Convert.ToInt16(values[0].Value) == 1);
        }

        private void LayerOnAndThawRegex(List<string> layerNames)
        {
            var doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            bool needsRegen = false;
            using (Transaction trans = doc.TransactionManager.StartTransaction())
            {
                try
                {
                    LayerTable layTb = trans.GetObject(db.LayerTableId, OpenMode.ForRead) as LayerTable;
                    List<LayerTableRecord> ltrs = new List<LayerTableRecord>();
                    foreach (var ltrOid in layTb)
                    {
                        LayerTableRecord ltr = (LayerTableRecord)trans.GetObject(ltrOid, OpenMode.ForRead);
                        string ok = layerNames.FirstOrDefault(x => Regex.IsMatch(ltr.Name, x, RegexOptions.IgnoreCase));
                        if (!string.IsNullOrEmpty(ok))
                        {
                            ltrs.Add(ltr);
                        }
                    }

                    foreach (var ltr in ltrs)
                    {
                        log.InfoFormat("Taue und schalte Layer {0} ein.", ltr.Name);
                        ltr.UpgradeOpen();
                        ltr.IsOff = false;
                        if (string.Compare(_AcAp.Application.GetSystemVariable("CLAYER").ToString(), ltr.Name, StringComparison.OrdinalIgnoreCase) != 0)
                        {
                            if (ltr.IsFrozen) needsRegen = true;
                            ltr.IsFrozen = false;
                        }
                    }
                }
                finally
                {
                    trans.Commit();
                    if (needsRegen)
                    {
                        doc.Editor.Regen();
                    }
                }
            }

        }

        private void LayerOnAndThaw(List<string> layerNames)
        {
            var doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            bool needsRegen = false;
            using (Transaction trans = doc.TransactionManager.StartTransaction())
            {
                try
                {
                    LayerTable layTb = trans.GetObject(db.LayerTableId, OpenMode.ForRead) as LayerTable;

                    foreach (var layerName in layerNames)
                    {
                        if (!layTb.Has(layerName)) continue;
                        var layId = layTb[layerName];
                        LayerTableRecord ltr = trans.GetObject(layId, OpenMode.ForRead) as LayerTableRecord;
                        log.InfoFormat("Taue und schalte Layer {0} ein.", ltr.Name);
                        ltr.UpgradeOpen();
                        ltr.IsOff = false;
                        if (string.Compare(_AcAp.Application.GetSystemVariable("CLAYER").ToString(), ltr.Name, StringComparison.OrdinalIgnoreCase) != 0)
                        {
                            if (ltr.IsFrozen) needsRegen = true;
                            ltr.IsFrozen = false;
                        }
                    }
                }
                finally
                {
                    trans.Commit();
                    if (needsRegen)
                    {
                        doc.Editor.Regen();
                    }
                }
            }

        }

        private class FbRaumInfo
        {
            private TransactionManager _TransMan = null;
            public enum Status
            {
                None,
                Ok,
                ErrorGetPolyHeight
            }

            public Status TheStatus { get; set; }
            public ObjectId Polygon { get; set; }
            public List<ObjectId> HKBlocks { get; set; }
            public List<ObjectId> VermPunkte { get; set; }
            public List<Point3d> ErrorPositions { get; set; }

            private double _Height = 0.0;
            public static HoePrOptions HoePrOptions { get; set; }

            public FbRaumInfo(ObjectId polygon, List<ObjectId> raumblocks, List<ObjectId> vermPunkte, TransactionManager tm)
            {
                Init();

                _TransMan = tm;
                Polygon = polygon;

                if (!GetPolyHeight())
                {
                    TheStatus = Status.ErrorGetPolyHeight;
                }

                FindHKBlock(raumblocks);

                FindVermPunkte(vermPunkte);

            }

            private bool GetPolyHeight()
            {
                try
                {
                    using (var tr = _TransMan.StartTransaction())
                    {
                        Entity ent = (Entity)tr.GetObject(Polygon, OpenMode.ForRead);
                        Polyline pl = ent as Polyline;
                        if (pl != null)
                        {
                            _Height = pl.Elevation;
                            return true;
                        }
                        Polyline2d pl2 = ent as Polyline2d;
                        if (pl2 != null)
                        {
                            _Height = pl2.GetElevation();
                            return true;
                        }

                        Polyline3d pl3 = ent as Polyline3d;
                        if (pl3 != null)
                        {
                            _Height = pl3.GetElevation();
                            return true;
                        }

                        tr.Commit();
                    }


                }
                catch (Exception ex)
                {
                    log.Error(ex.Message, ex);
                }

                return false;

            }

            private void Init()
            {
                TheStatus = Status.None;
                VermPunkte = new List<ObjectId>();
                HKBlocks = new List<ObjectId>();
                ErrorPositions = new List<Point3d>();

            }

            private void FindHKBlock(List<ObjectId> raumBlocks)
            {
                using (var myTrans = _TransMan.StartTransaction())
                {
                    Entity poly = _TransMan.GetObject(Polygon, OpenMode.ForRead) as Entity;
                    if (poly != null)
                    {
                        List<ObjectId> toRemove = new List<ObjectId>();
                        foreach (var oid in raumBlocks)
                        {
                            BlockReference block = _TransMan.GetObject(oid, OpenMode.ForRead) as BlockReference;
                            if (block != null)
                            {
                                if (AreaEngine.InPoly(ClonePointSetZ(block.Position, _Height), poly))
                                {
                                    toRemove.Add(oid);
                                }
                            }
                        }

                        foreach (var oid in toRemove)
                        {
                            raumBlocks.Remove(oid);
                        }

                        HKBlocks.AddRange(toRemove);
                        TheStatus = Status.Ok;
                    }

                    myTrans.Commit();
                }
            }

            private void FindVermPunkte(List<ObjectId> zuRaumIdBlocks)
            {

                using (var myTrans = _TransMan.StartTransaction())
                {
                    Entity poly = _TransMan.GetObject(Polygon, OpenMode.ForRead) as Entity;
                    if (poly != null)
                    {
                        List<ObjectId> toRemove = new List<ObjectId>();
                        foreach (var oid in zuRaumIdBlocks)
                        {
                            BlockReference block = _TransMan.GetObject(oid, OpenMode.ForRead) as BlockReference;
                            if (block != null)
                            {

                                if (AreaEngine.InPoly(ClonePointSetZ(block.Position, _Height), poly))
                                {
                                    VermPunkte.Add(oid);
                                    toRemove.Add(oid);
                                }
                            }
                        }
                        foreach (var oid in toRemove)
                        {
                            zuRaumIdBlocks.Remove(oid);
                        }
                    }

                    myTrans.Commit();
                }
            }

            private Point3d ClonePointSetZ(Point3d point3d, double z)
            {
                return new Point3d(point3d.X, point3d.Y, z);
            }

            /// <summary>
            /// Vergleich Fußbodenhöhe mit Höhenkotenwert
            /// </summary>
            /// <remarks>Schreibt ErrorPoistions</remarks>
            /// 
            internal void CheckFb()
            {
                ErrorPositions.Clear();

                foreach (var hkOid in HKBlocks)
                {
                    CheckFb(hkOid);
                }

            }

            private void CheckFb(ObjectId hkOid)
            {
                ObjectId vp = GetNearestVp(hkOid);
                if (vp != default(ObjectId))
                {
                    CompareHeights(vp, hkOid);
                }
            }

            private void CompareHeights(ObjectId vpOid, ObjectId hkOid)
            {
                using (var trans = _AcAp.Application.DocumentManager.MdiActiveDocument.TransactionManager.StartTransaction())
                {
                    BlockReference vpBlock = (BlockReference)trans.GetObject(vpOid, OpenMode.ForRead);
                    BlockReference hkBlock = (BlockReference)trans.GetObject(hkOid, OpenMode.ForRead);

                    double hkHeight = GetHkHeight(hkBlock);

                    int diff = (int)Math.Round((Math.Abs(vpBlock.Position.Z - hkHeight) * 100.0));
                    if (diff > HoePrOptions.FbToleranz)
                    {
                        ErrorPositions.Add(hkBlock.Position);
                    }

                    trans.Commit();
                }
            }

            private double GetHkHeight(BlockReference hkBlock)
            {
                var att = GetBlockAttribute(HoePrOptions.AttHoehe, hkBlock);
                if (att == null)
                {
                    log.WarnFormat("Attribut '{0}' nicht gefunden!", HoePrOptions.AttHoehe);
                    return 0.0;
                }

                string hText = att.TextString;
                if (hText.StartsWith("%%P")) hText = hText.Remove(0, 3);

                double height;
                if (double.TryParse(hText, NumberStyles.Any, CultureInfo.InvariantCulture, out height))
                {
                    return height;
                }
                else
                {
                    log.WarnFormat("Konnte Höhe nicht ermitteln aus '{0}'! Verwende 0.0", att.TextString);
                    return 0.0;
                }

            }

            private AttributeReference GetBlockAttribute(string name, BlockReference blockEnt)
            {
                foreach (ObjectId attId in blockEnt.AttributeCollection)
                {
                    var anyAttRef = _TransMan.GetObject(attId, OpenMode.ForRead) as AttributeReference;
                    if (anyAttRef != null)
                    {
                        if (string.Compare(anyAttRef.Tag, name, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            return anyAttRef;
                        }
                    }
                }
                return null;
            }

            private ObjectId GetNearestVp(ObjectId hkOid)
            {
                ObjectId theVp = default(ObjectId);

                using (var trans = _AcAp.Application.DocumentManager.MdiActiveDocument.TransactionManager.StartTransaction())
                {
                    BlockReference hkBlock = (BlockReference)trans.GetObject(hkOid, OpenMode.ForRead);

                    double? minDist = null;
                    foreach (var vpOid in VermPunkte)
                    {
                        BlockReference vpBlock = (BlockReference)trans.GetObject(vpOid, OpenMode.ForRead);

                        double dist = Get2dDist(hkBlock.Position, vpBlock.Position);
                        if (minDist.HasValue)
                        {
                            if (dist < minDist)
                            {
                                minDist = dist;
                                theVp = vpOid;
                            }
                        }
                        else
                        {
                            minDist = dist;
                            theVp = vpOid;
                        }
                    }

                    trans.Commit();
                }

                return theVp;
            }

            private double Get2dDist(Point3d p1, Point3d p2)
            {
                return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
            }
        }

        private bool CheckBlockAndAtt()
        {
            try
            {
                if (!Plan2Ext.Globs.BlockHasAttribute(_HoePrOptions.HKBlockname, _HoePrOptions.AttHoehe))
                {
                    string msg = string.Format(CultureInfo.CurrentCulture, "Der Block '{0}' hat kein Attribut '{1}'!", _HoePrOptions.HKBlockname, _HoePrOptions.AttHoehe);
                    log.Debug(msg);
                    _AcAp.Application.ShowAlertDialog(msg);
                    return false;
                }

            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
                _AcAp.Application.ShowAlertDialog(ex.Message);
                return false;
            }
            return true;
        }
        #endregion


    }
}
