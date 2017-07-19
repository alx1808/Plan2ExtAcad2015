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


namespace Plan2Ext.RaumHoePruefung
{
    internal class Engine
    {
        #region log4net Initialization
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Convert.ToString((typeof(Engine))));
        #endregion

        #region Constants
        public const string FEHLER_LINE_LAYER = "_HÖHENPRÜFUNG_FEHLER";
        public const string HOEPR_OK_DICT_NAME = "Plan2HoePrOk";
        private const string DH_PUNKT_LAYER = "X_DH_SYMB";
        #endregion

        #region Members
        private TransactionManager _TransMan = null;
        private HoePrOptions _HoePrOptions = null;

        List<ObjectId> _RaumBlocks = new List<ObjectId>();
        List<ObjectId> _BodenPunkte = new List<ObjectId>();
        List<ObjectId> _DeckenPunkte = new List<ObjectId>();
        List<ObjectId> _RaumPolygons = new List<ObjectId>();

        private int _NrOfErrors = 0;

        #endregion

        #region Lifecycle
        public Engine(HoePrOptions options)
        {
            _HoePrOptions = options;
            RaumHoehenInfo.HoePrOptions = options;
            _TransMan = _AcAp.Application.DocumentManager.MdiActiveDocument.Database.TransactionManager;
        }
        #endregion

        #region Raumhöhenprüfung

        internal void CheckRaumHoehen(bool ignoreFirstCancel = false)
        {
            log.Info("CheckRaumHoehen");

            if (!CheckBlockAndAtt()) return;

            _NrOfErrors = 0;

            Plan2Ext.Globs.DeleteFehlerLines(FEHLER_LINE_LAYER);

            if (!SelectZuRaumIdEntities(ignoreFirstCancel)) return;

            List<RaumHoehenInfo> raumInfos = new List<RaumHoehenInfo>();
            foreach (var oid in _RaumPolygons)
            {
                raumInfos.Add(new RaumHoehenInfo(oid, _RaumBlocks, _BodenPunkte, _DeckenPunkte, _TransMan));
            }
            log.InfoFormat(CultureInfo.CurrentCulture, "Anzahl der gefundenen Räume: {0}", raumInfos.Count);

            using (var myTrans = _TransMan.StartTransaction())
            {
                try
                {
                    CheckRaumHoehen(raumInfos.Where(x => x.TheStatus == RaumHoehenInfo.Status.Ok), myTrans);
                    myTrans.Commit();
                }
                catch (Exception ex)
                {
                    log.Error(ex.Message, ex);
                    throw;
                }

            }

            var errors = raumInfos.SelectMany(x => x.ErrorPositions).ToList();
            Plan2Ext.Globs.InsertFehlerLines(errors, FEHLER_LINE_LAYER);

            ShowSingleRaumBlocks();

            string msg = string.Empty;
            if (errors.Count > 0)
            {
                msg = string.Format(CultureInfo.CurrentCulture, "Anzahl der Fehler: {0}.", errors.Count);
            }
            if (_RaumBlocks.Count > 0)
            {
                if (!string.IsNullOrEmpty(msg)) msg += "\n";

                msg += string.Format(CultureInfo.CurrentCulture, "Alleinstehende Raumblöcke oder RH-Bezeichnungen: {0}.", _RaumBlocks.Count);
            }

            if (!string.IsNullOrEmpty(msg))
            {
                _AcAp.Application.ShowAlertDialog(msg);
                log.Warn(msg);
            }
            else
            {
                msg = string.Format(CultureInfo.CurrentCulture, "Die Fußboden-Höhenprüfung wurde erfolgreich beendet.");
                _AcAp.Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage(msg);
                log.Info(msg);
            }

        }

        private void ShowSingleRaumBlocks()
        {
            if (_RaumBlocks.Count == 0) return;

            using (Transaction myT = _TransMan.StartTransaction())
            {
                foreach (var oid in _RaumBlocks)
                {
                    Entity ent = _TransMan.GetObject(oid, OpenMode.ForRead) as Entity;
                    if (ent == null) continue;
                    BlockReference blockRef = ent as BlockReference;
                    if (blockRef != null)
                    {
                        Plan2Ext.Globs.Stern(blockRef.Position, 1.0, 32, color: 1, highLighted: false);
                    }
                    else
                    {
                        DBText txt = ent as DBText;
                        if (txt != null)
                        {
                            Plan2Ext.Globs.Stern(txt.Position, 1.0, 32, color: 1, highLighted: false);
                        }
                    }

                }
                myT.Commit();
            }            
        }

        private void CheckRaumHoehen(IEnumerable<RaumHoehenInfo> rauminfos, Transaction trans)
        {
            foreach (var ri in rauminfos)
            {
                ri.CheckRaumHoehe();
            }
        }

        private bool SelectZuRaumIdEntities(bool ignoreFirstCancel)
        {
            _RaumBlocks.Clear();
            _BodenPunkte.Clear();
            _DeckenPunkte.Clear();
            _RaumPolygons.Clear();

            //LayerOnAndThaw(new List<string> { _HoePrOptions.PolygonLayer, "X_FH_SYMB", "X_SPKT_SYMB", "X_DR_SYMB", DH_PUNKT_LAYER, FEHLER_LINE_LAYER });
            LayerOnAndThawRegex(new List<string> { "^" + _HoePrOptions.PolygonLayer + "$", "^X", "^" + DH_PUNKT_LAYER + "$", "^" + FEHLER_LINE_LAYER + "$" });

            var ed = _AcAp.Application.DocumentManager.MdiActiveDocument.Editor;
            SelectionFilter filter = new SelectionFilter(new TypedValue[] { 
                new TypedValue((int)DxfCode.Operator,"<OR" ),

                new TypedValue((int)DxfCode.Operator,"<AND" ),
                    new TypedValue((int)DxfCode.Start,"INSERT" ),
                    new TypedValue((int)DxfCode.BlockName, _HoePrOptions.RaumBlockname ),
                new TypedValue((int)DxfCode.Operator,"AND>" ),

                new TypedValue((int)DxfCode.Start,"TEXT" ),

                new TypedValue((int)DxfCode.Operator,"<AND" ),
                    new TypedValue((int)DxfCode.Start,"INSERT" ),
                    new TypedValue((int)DxfCode.BlockName, "GEOINOVA" ),
                    new TypedValue((int)DxfCode.Operator,"<OR" ),
                        new TypedValue((int)DxfCode.LayerName, "X_FH_SYMB"),
                        new TypedValue((int)DxfCode.LayerName, "X_SPKT_SYMB"),
                        //new TypedValue((int)DxfCode.LayerName, "X_GH_SYMB"), Rücksprache GH 25.7.2016
                        new TypedValue((int)DxfCode.LayerName, "X_DR_SYMB"),
                        new TypedValue((int)DxfCode.LayerName, DH_PUNKT_LAYER), // -> Deckenhöhe
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


                        if (string.Compare(_HoePrOptions.RaumBlockname, blockRef.Name, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            //Raumblocks
                            if (!MarkedAsOk(oid))
                            {
                                var valuePerTag = Plan2Ext.Globs.GetAttributes(blockRef);
                                //var gewoelbe = valuePerTag.Values.FirstOrDefault(x => Regex.IsMatch(x, "gew", RegexOptions.IgnoreCase));
                                var gewoelbe = valuePerTag.Values.FirstOrDefault(x => IsIgnoreRbAttribute(x));
                                if (gewoelbe == null)
                                {
                                    _RaumBlocks.Add(oid);
                                }
                            }
                            else
                            {
                                Plan2Ext.Globs.Stern(blockRef.Position, 1.0, 32, color: 3, highLighted: false);
                                someMarkedAsOk = true;
                            }
                        }
                        else if (string.Compare(blockRef.Layer, DH_PUNKT_LAYER, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            _DeckenPunkte.Add(oid);
                        }
                        else
                        {
                            _BodenPunkte.Add(oid);
                        }
                    }
                    else
                    {
                        DBText text = ent as DBText;
                        if (text != null && text.TextString.StartsWith("RH", StringComparison.OrdinalIgnoreCase))
                        {
                            // Raumtext
                            if (!MarkedAsOk(oid)) _RaumBlocks.Add(oid);
                        }
                        else
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

            if (_RaumBlocks.Count == 0) log.Warn("Es wurden keine Raumblöcke gefunden!");
            else log.InfoFormat(CultureInfo.CurrentCulture, "Anzahl der Raumblöcke: {0}", _RaumBlocks.Count);

            if (_BodenPunkte.Count == 0) log.Warn("Es wurden keine passenden Boden-Vermessungspunkte gefunden!");
            else log.InfoFormat(CultureInfo.CurrentCulture, "Anzahl der Boden-Vermessungspunkte: {0}", _BodenPunkte.Count);

            if (_DeckenPunkte.Count == 0) log.Warn("Es wurden keine passenden Decken-Vermessungspunkte gefunden!");
            else log.InfoFormat(CultureInfo.CurrentCulture, "Anzahl der Decken-Vermessungspunkte: {0}", _DeckenPunkte.Count);

            if (_RaumPolygons.Count == 0)
            {
                string msg = "Es wurden keine Raumpolylinien gewählt!";
                log.Warn(msg);
                _AcAp.Application.ShowAlertDialog(msg);
            }
            else log.InfoFormat(CultureInfo.CurrentCulture, "Anzahl der Raumpolylinien: {0}", _RaumPolygons.Count);

            if (_RaumBlocks.Count > 0 && _BodenPunkte.Count > 0 && _DeckenPunkte.Count > 0 && _RaumPolygons.Count > 0) return true;
            else return false;
        }

        private bool IsIgnoreRbAttribute(string att)
        {
            if (Regex.IsMatch(att, "gew", RegexOptions.IgnoreCase)) return true;
            if (Regex.IsMatch(att, "schr", RegexOptions.IgnoreCase) && !Regex.IsMatch(att, "schrank", RegexOptions.IgnoreCase)) return true;

            return false;
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

        private bool LayerOnAndThaw(string layerName)
        {
            var doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            bool needsRegen = false;
            using (Transaction trans = doc.TransactionManager.StartTransaction())
            {
                try
                {
                    LayerTable layTb = trans.GetObject(db.LayerTableId, OpenMode.ForRead) as LayerTable;
                    if (!layTb.Has(layerName)) return false;
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
                    return true;
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

        private class RaumHoehenInfo
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
            public List<ObjectId> RaumBlocks { get; set; }
            public List<ObjectId> BodenPunkte { get; set; }
            public List<ObjectId> DeckenPunkte { get; set; }
            public List<Point3d> ErrorPositions { get; set; }

            private double _Height = 0.0;
            public static HoePrOptions HoePrOptions { get; set; }

            public RaumHoehenInfo(ObjectId polygon, List<ObjectId> raumblocks, List<ObjectId> bodenPunkte, List<ObjectId> deckenPunkte, TransactionManager tm)
            {
                Init();

                _TransMan = tm;
                Polygon = polygon;

                if (!GetPolyHeight())
                {
                    TheStatus = Status.ErrorGetPolyHeight;
                }

                FindRaumBlocks(raumblocks);

                FindBodenPunkte(bodenPunkte);

                FindDeckenPunkte(deckenPunkte);

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
                BodenPunkte = new List<ObjectId>();
                DeckenPunkte = new List<ObjectId>();
                RaumBlocks = new List<ObjectId>();
                ErrorPositions = new List<Point3d>();

            }

            private void FindRaumBlocks(List<ObjectId> raumBlocks)
            {
                using (var myTrans = _TransMan.StartTransaction())
                {
                    Entity poly = _TransMan.GetObject(Polygon, OpenMode.ForRead) as Entity;
                    if (poly != null)
                    {
                        List<ObjectId> toRemove = new List<ObjectId>();
                        foreach (var oid in raumBlocks)
                        {
                            Entity ent = _TransMan.GetObject(oid, OpenMode.ForRead) as Entity;
                            Point3d pos = GetRbPosition(ent);

                            if (!pos.Equals(default(Point3d)))
                            {
                                if (AreaEngine.InPoly(ClonePointSetZ(pos, _Height), poly))
                                {
                                    toRemove.Add(oid);
                                }
                            }
                        }

                        foreach (var oid in toRemove)
                        {
                            raumBlocks.Remove(oid);
                        }

                        RaumBlocks.AddRange(toRemove);
                        TheStatus = Status.Ok;
                    }

                    myTrans.Commit();
                }
            }

            private static Point3d GetRbPosition(Entity ent)
            {
                BlockReference block = ent as BlockReference;
                Point3d pos = default(Point3d);
                if (block != null)
                {
                    pos = block.Position;
                }
                else
                {
                    // Text
                    pos = Globs.GetCenter(ent);
                }
                return pos;
            }

            private void FindDeckenPunkte(List<ObjectId> deckenPunkte)
            {
                using (var myTrans = _TransMan.StartTransaction())
                {
                    Entity poly = _TransMan.GetObject(Polygon, OpenMode.ForRead) as Entity;
                    if (poly != null)
                    {
                        List<ObjectId> toRemove = new List<ObjectId>();
                        foreach (var oid in deckenPunkte)
                        {
                            BlockReference block = _TransMan.GetObject(oid, OpenMode.ForRead) as BlockReference;
                            if (block != null)
                            {

                                if (AreaEngine.InPoly(ClonePointSetZ(block.Position, _Height), poly))
                                {
                                    DeckenPunkte.Add(oid);
                                    toRemove.Add(oid);
                                }
                            }
                        }
                        foreach (var oid in toRemove)
                        {
                            deckenPunkte.Remove(oid);
                        }
                    }

                    myTrans.Commit();
                }
            }


            private void FindBodenPunkte(List<ObjectId> bodenPunkte)
            {

                using (var myTrans = _TransMan.StartTransaction())
                {
                    Entity poly = _TransMan.GetObject(Polygon, OpenMode.ForRead) as Entity;
                    if (poly != null)
                    {
                        List<ObjectId> toRemove = new List<ObjectId>();
                        foreach (var oid in bodenPunkte)
                        {
                            BlockReference block = _TransMan.GetObject(oid, OpenMode.ForRead) as BlockReference;
                            if (block != null)
                            {

                                if (AreaEngine.InPoly(ClonePointSetZ(block.Position, _Height), poly))
                                {
                                    BodenPunkte.Add(oid);
                                    toRemove.Add(oid);
                                }
                            }
                        }
                        foreach (var oid in toRemove)
                        {
                            bodenPunkte.Remove(oid);
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
            internal void CheckRaumHoehe()
            {
                ErrorPositions.Clear();

                foreach (var raumOid in RaumBlocks)
                {
                    CheckRaumHoehe(raumOid);
                }

            }

            private void CheckRaumHoehe(ObjectId rbOid)
            {
                ObjectId fp = GetNearestBodenPunkt(rbOid);
                ObjectId dp = GetNearestDeckenPunkt(rbOid);
                if (fp != default(ObjectId) && dp != default(ObjectId))
                {
                    CompareHeights(fp, dp, rbOid);
                }
            }

            private void CompareHeights(ObjectId fpOid, ObjectId dpOid, ObjectId rbOid)
            {
                using (var trans = _AcAp.Application.DocumentManager.MdiActiveDocument.TransactionManager.StartTransaction())
                {
                    BlockReference fbBlock = (BlockReference)trans.GetObject(fpOid, OpenMode.ForRead);
                    BlockReference dpBlock = (BlockReference)trans.GetObject(dpOid, OpenMode.ForRead);
                    Entity raumEntity = (Entity)trans.GetObject(rbOid, OpenMode.ForRead);
                    var rbpos = GetRbPosition(raumEntity);

                    double raumHoehe;
                    if (GetRaumHoehe(raumEntity, out raumHoehe))
                    {
                        double vpDiff = dpBlock.Position.Z - fbBlock.Position.Z;

                        int diff = (int)Math.Round((Math.Abs(vpDiff - raumHoehe) * 100.0));
                        if (diff > HoePrOptions.RhToleranz)
                        {
                            ErrorPositions.Add(rbpos);
                        }
                    }

                    trans.Commit();
                }
            }

            private bool GetRaumHoehe(Entity ent, out double height)
            {
                height = 0.0;
                string hText = string.Empty;

                BlockReference raumBlock = ent as BlockReference;
                if (raumBlock != null)
                {

                    var att = GetBlockAttribute(HoePrOptions.AttHoehe, raumBlock);
                    if (att == null)
                    {
                        log.WarnFormat("Attribut '{0}' nicht gefunden!", HoePrOptions.AttHoehe);
                        return true;
                    }

                    hText = att.TextString;
                }
                else
                {
                    DBText text = (DBText)ent;
                    hText = text.TextString;
                }

                if (string.IsNullOrEmpty(hText.Trim())) return false;

                if (Globs.GetFirstDoubleInText(hText, out height))
                {
                    return true;
                }
                else
                {
                    log.WarnFormat("Konnte Höhe nicht ermitteln aus '{0}'! Verwende 0.0", hText);
                    return true;
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

            private ObjectId GetNearestDeckenPunkt(ObjectId raumOid)
            {
                ObjectId theVp = default(ObjectId);

                using (var trans = _AcAp.Application.DocumentManager.MdiActiveDocument.TransactionManager.StartTransaction())
                {
                    Entity raumBlock = (Entity)trans.GetObject(raumOid, OpenMode.ForRead);
                    var pos = GetRbPosition(raumBlock);

                    double? minDist = null;
                    foreach (var vpOid in DeckenPunkte)
                    {
                        BlockReference vpBlock = (BlockReference)trans.GetObject(vpOid, OpenMode.ForRead);

                        double dist = Get2dDist(pos, vpBlock.Position);
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

            private ObjectId GetNearestBodenPunkt(ObjectId raumOid)
            {
                ObjectId theVp = default(ObjectId);

                using (var trans = _AcAp.Application.DocumentManager.MdiActiveDocument.TransactionManager.StartTransaction())
                {
                    Entity raumBlock = (Entity)trans.GetObject(raumOid, OpenMode.ForRead);
                    var pos = GetRbPosition(raumBlock);

                    double? minDist = null;
                    foreach (var vpOid in BodenPunkte)
                    {
                        BlockReference vpBlock = (BlockReference)trans.GetObject(vpOid, OpenMode.ForRead);

                        double dist = Get2dDist(pos, vpBlock.Position);
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
                if (!Plan2Ext.Globs.BlockHasAttribute(_HoePrOptions.RaumBlockname, _HoePrOptions.AttHoehe))
                {
                    string msg = string.Format(CultureInfo.CurrentCulture, "Der Block '{0}' hat kein Attribut '{1}'!", _HoePrOptions.RaumBlockname, _HoePrOptions.AttHoehe);
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
