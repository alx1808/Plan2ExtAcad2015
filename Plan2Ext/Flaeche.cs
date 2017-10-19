using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Globalization;
using Plan2Ext.CalcArea;


//using Autodesk.AutoCAD.Runtime;
//using Autodesk.AutoCAD.DatabaseServices;
//using Autodesk.AutoCAD.EditorInput;
//using Autodesk.AutoCAD.ApplicationServices;
//using Autodesk.AutoCAD.Geometry;
//using Autodesk.AutoCAD.Interop;
//using Autodesk.AutoCAD.Interop.Common;

#if BRX_APP
using _AcAp = Bricscad.ApplicationServices;
//using _AcBr = Teigha.BoundaryRepresentation;
using _AcCm = Teigha.Colors;
using _AcDb = Teigha.DatabaseServices;
using _AcEd = Bricscad.EditorInput;
using _AcGe = Teigha.Geometry;
using _AcGi = Teigha.GraphicsInterface;
using _AcGs = Teigha.GraphicsSystem;
using _AcPl = Bricscad.PlottingServices;
using _AcBrx = Bricscad.Runtime;
using _AcTrx = Teigha.Runtime;
using _AcWnd = Bricscad.Windows;
using _AcIntCom = BricscadDb;
using _AcInt = BricscadApp;
#elif ARX_APP
using _AcAp = Autodesk.AutoCAD.ApplicationServices;
using _AcCm = Autodesk.AutoCAD.Colors;
using _AcDb = Autodesk.AutoCAD.DatabaseServices;
using _AcEd = Autodesk.AutoCAD.EditorInput;
using _AcGe = Autodesk.AutoCAD.Geometry;
using _AcGi = Autodesk.AutoCAD.GraphicsInterface;
using _AcGs = Autodesk.AutoCAD.GraphicsSystem;
using _AcPl = Autodesk.AutoCAD.PlottingServices;
using _AcBrx = Autodesk.AutoCAD.Runtime;
using _AcTrx = Autodesk.AutoCAD.Runtime;
using _AcWnd = Autodesk.AutoCAD.Windows;
using _AcIntCom = Autodesk.AutoCAD.Interop.Common;
using _AcInt = Autodesk.AutoCAD.Interop;
#endif


namespace Plan2Ext
{
    public class Flaeche : _AcTrx.IExtensionApplication
    {
        #region log4net Initialization
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Convert.ToString((typeof(Flaeche))));
        static Flaeche()
        {
            if (log4net.LogManager.GetRepository(System.Reflection.Assembly.GetExecutingAssembly()).Configured == false)
            {
                log4net.Config.XmlConfigurator.ConfigureAndWatch(new System.IO.FileInfo(System.IO.Path.Combine(new System.IO.FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).DirectoryName, "_log4net.config")));
            }

        }
        #endregion

        #region Constants
        private const int AcHatchObject = 0;
        private const int AcGradientObject = 1;
        private const int AcPatternType = 0;
        private const int AcGradientPatternType = 1;
        private const string COLOROBJECTPROGID = "AutoCAD.AcCmColor.18";
        private const double ABSTANDTEXT = 4.0;

        #endregion

        #region Member Variables

        static CalcAreaPalette _CalcAreaPalette;
        internal static CalcAreaPalette TheCalcAreaPalette { get { return _CalcAreaPalette; } }

        private static string _RaumblockName = "";
        private static string _FlAttrib = "";
        private static string _FgLayer = "";
        private static string _AfLayer = "";
        private static string _PeriAttrib = "";
        private static string _PeriString = "";
        private static string _GeomIncorrectLayer = "";
        private static string _AbzGeomIncorrectLayer = "";
        private static string _InvalidNrRb = "";
        private static string _RegionLayer = "";
        private static string _DiffersLayer = "";
        private static string _LooseBlockLayer = "";
        private static string _M2Bez = "";
        private static bool _Modify = false;
        public static bool Modify
        {
            get { return Flaeche._Modify; }
            set { Flaeche._Modify = value; }
        }

        private static List<_AcDb.ObjectId> _FlaechenGrenzen = new List<_AcDb.ObjectId>();
        private static List<_AcDb.ObjectId> _Raumbloecke = new List<_AcDb.ObjectId>();

        private static AreaEngine _AreaEngine = null;
        #endregion


        public delegate List<AktFlaecheErrorType> AktFlaecheDelegate(_AcAp.Document doc, string rbName, string flAttrib, string periAtt, string fgLayer, string afLayer, bool selectAll = false, bool layerSchalt = true, bool automated = false);

        [_AcTrx.LispFunction("DotNetCalcFlaeche")]
        public static _AcDb.ResultBuffer DotNetCalcFlaeche(_AcDb.ResultBuffer rb)
        {
            log.Debug("--------------------------------------------------------------------------------");
            log.Debug("DotNetAktFlaeche");
            try
            {
                _AcAp.Document doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
                log.DebugFormat("Dokumentname: {0}.", doc.Name);

                GetArgs(rb);

                _CalcAreaPalette.Show(_RaumblockName, _FlAttrib, _FgLayer, _AfLayer, new AktFlaecheDelegate(AktFlaeche));

                return null; // returns nil

            }
            catch (System.Exception ex)
            {
                _AcAp.Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in DotNetCalcFlaeche aufgetreten! {0}", ex.Message));

            }
            finally
            {
                Free();
            }
            return null;
        }

        [_AcTrx.LispFunction("DotNetAktFlaeche")]
        public static _AcDb.ResultBuffer DotNetAktFlaeche(_AcDb.ResultBuffer rb)
        {
            log.Debug("--------------------------------------------------------------------------------");
            log.Debug("DotNetAktFlaeche");
            try
            {
                _AcAp.Document doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
                log.DebugFormat("Dokumentname: {0}.", doc.Name);

                GetArgs(rb);

                Plan2Ext.Kleinbefehle.Layers.Plan2SaveLayerStatus();

                _CalcAreaPalette.Show(_RaumblockName, _FlAttrib, _FgLayer, _AfLayer, new AktFlaecheDelegate(AktFlaeche));

                AktFlaeche(doc, null, null, null, null, null);

                return null; // returns nil

            }
            catch (System.Exception ex)
            {
                _AcAp.Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in DotNetAktFlaeche aufgetreten! {0}", ex.Message));

            }
            finally
            {
                Free();
            }
            return null;
        }

        [_AcTrx.LispFunction("DotNetUpdateFlaecheValues")]
        public static _AcDb.ResultBuffer DotNetUpdateFlaecheValues(_AcDb.ResultBuffer rb)
        {
            log.Debug("--------------------------------------------------------------------------------");
            log.Debug("DotNetUpdateFlaecheValues");
            try
            {
                _AcAp.Document doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
                log.DebugFormat("Dokumentname: {0}.", doc.Name);

                GetArgs(rb);

                _CalcAreaPalette.UpdateValues(_RaumblockName, _FlAttrib, _FgLayer, _AfLayer);

                return null; // returns nil

            }
            catch (System.Exception ex)
            {
                _AcAp.Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in DotNetCalcFlaeche aufgetreten! {0}", ex.Message));

            }
            finally
            {
                Free();
            }
            return null;
        }

        public enum AktFlaecheErrorType
        {
            NoError,
            NoRaumBlock,
            MoreThanOneRaumBlock,
            InvalidGeometry,
            RaumblocksWithoutFlaechengrenze,
            BlockHasNotThisM2Attribute,
            NoFlaechengrenzen,
            WrongM2,
        }

        internal static List<AktFlaecheErrorType> AktFlaeche(_AcAp.Document doc, string rbName, string flAttrib, string periAttrib, string fgLayer, string afLayer, bool selectAll = false, bool layerSchalt = true, bool automated = false)
        {
            log.Debug("--------------------------");

            var errorList = new List<AktFlaecheErrorType>();
            _AreaEngine = new AreaEngine();
            try
            {
                if (!automated) InitVariablesFromConfig();

                if (!string.IsNullOrEmpty(rbName)) _RaumblockName = rbName;
                if (!string.IsNullOrEmpty(flAttrib)) _FlAttrib = flAttrib;
                if (!string.IsNullOrEmpty(periAttrib)) _PeriAttrib = periAttrib;
                if (!string.IsNullOrEmpty(fgLayer)) _FgLayer = fgLayer;
                if (!string.IsNullOrEmpty(afLayer)) _AfLayer = afLayer;

                if (!CheckBlockAndAtt(automated))
                {
                    errorList.Add(AktFlaecheErrorType.BlockHasNotThisM2Attribute);
                    return errorList;
                }

                if (layerSchalt)
                {
                    Plan2Ext.Globs.SetLayerCurrent("0");
                    Plan2Ext.Globs.LayersOnRestOffAllThawIC(GetLayerNamesToTurnOn());
                }

                DeleteRegions();
                DeleteFehlerSymbols();

                if (!_AreaEngine.SelectFgAndRb(_FlaechenGrenzen, _Raumbloecke, _FgLayer, _RaumblockName, selectAll)) return errorList;

                var fgRbStructs = AreaEngine.GetFgRbStructs(_RaumblockName, _FgLayer, _AfLayer, doc.Database);
                var nrOfOverlaps = AreaEngine.NrOfOverlaps;
                var keinRb = fgRbStructs.Values.Where(x => x.Raumbloecke.Count == 0).ToList();
                var mehrRb = fgRbStructs.Values.Where(x => x.Raumbloecke.Count > 1).ToList();
                var orphans = AreaEngine.OrphanRaumblocks;

                if (_FlaechenGrenzen.Count == 0)
                {
                    string msg = "Es wurden keine Raumpolylinien gewählt!";
                    log.Warn(msg);
                    if (!automated) _AcAp.Application.ShowAlertDialog(msg);
                    errorList.Add(AktFlaecheErrorType.NoFlaechengrenzen);
                    return errorList;
                }

                // init div
                int fehlerKeinRb = 0;
                int fehlerMehrRb = 0;
                int fehlerWertFalsch = 0;

                _AcDb.Database db = doc.Database;
                _AcEd.Editor ed = doc.Editor;
                _AcDb.TransactionManager tm = db.TransactionManager;
                using (_AcDb.Transaction myT = tm.StartTransaction())
                {
                    int attsNotFound = 0;
                    for (int i = 0; i < _FlaechenGrenzen.Count; i++)
                    {
                        log.Debug("--------------------------");
                        _AcDb.ObjectId elFG = _FlaechenGrenzen[i];
                        log.DebugFormat("Flächengrenze {0}", elFG.Handle.ToString());
                        var info = fgRbStructs[elFG];

                        var ssAF = new List<_AcDb.ObjectId>();
                        ssAF.AddRange(info.Inseln);
                        ssAF.AddRange(info.Abzugsflaechen);
                        if (ssAF.Count > 0)
                        {
                            string handles = string.Join(",", ssAF.Select(x => x.Handle.ToString()).ToArray());
                            log.DebugFormat("Abzugpolylinien und abzuziehende Flächengrenzen: {0}", handles);
                        }
                        var ssRB = new List<_AcDb.ObjectId>();
                        ssRB.AddRange(info.Raumbloecke);
                        if (ssRB.Count > 0)
                        {
                            string handles = string.Join(",", ssRB.Select(x => x.Handle.ToString()).ToArray());
                            log.DebugFormat("Raumblöcke: {0}", handles);
                        }
                        double dM2, dPeri;
                        bool errorSymbolSet;
                        if (!GetRegionArea(elFG, ssAF, out dM2, out dPeri, tm, out errorSymbolSet))
                        {
                            if (!errorSymbolSet)
                            {
                                log.WarnFormat("Ungültige Geometrie in Flächengrenze {0}!", elFG.Handle.ToString());
                                FehlerLineOrHatchPoly(elFG, _GeomIncorrectLayer, 255, 255, 0, tm, Globs.GetMiddlePoint(elFG));
                            }
                            if (!errorList.Contains(AktFlaecheErrorType.InvalidGeometry)) errorList.Add(AktFlaecheErrorType.InvalidGeometry);
                            continue;
                        }

                        foreach (var rb in info.Raumbloecke)
                        {
                            if (_Raumbloecke.Contains(rb)) _Raumbloecke.Remove(rb);
                        }
                        var rbAnz = ssRB.Count;
                        if (rbAnz < 1)
                        {
                            log.WarnFormat("Kein Raumblock in Flächengrenze {0}!", elFG.Handle.ToString());
                            FehlerLineOrHatchPoly(elFG, _InvalidNrRb, 255, 0, 0, tm, Globs.GetMiddlePoint(elFG));
                            if (!errorList.Contains(AktFlaecheErrorType.NoRaumBlock)) errorList.Add(AktFlaecheErrorType.NoRaumBlock);
                            fehlerKeinRb++;

                        }
                        else if (rbAnz > 1)
                        {
                            log.WarnFormat("Mehr als ein Raumblock in Flächengrenze {0}!", elFG.Handle.ToString());
                            FehlerLineOrHatchPoly(elFG, _InvalidNrRb, 255, 0, 0, tm, Globs.GetMiddlePoint(elFG));
                            if (!errorList.Contains(AktFlaecheErrorType.MoreThanOneRaumBlock)) errorList.Add(AktFlaecheErrorType.MoreThanOneRaumBlock);
                            fehlerMehrRb++;
                        }
                        else
                        {
                            bool Differs = false;
                            var blockRef = myT.GetObject(ssRB[0], _AcDb.OpenMode.ForRead) as _AcDb.BlockReference;
                            var attRefs = Globs.GetAttributEntities(blockRef, myT);
                            bool m2AttFound = false;
                            foreach (var attRef in attRefs)
                            {
                                if (string.Compare(attRef.Tag, _FlAttrib, StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    m2AttFound = true;
                                    string sAtt = string.Format(CultureInfo.InvariantCulture, "{0:F2}{1}", dM2, _M2Bez);
                                    if (string.Compare(sAtt, attRef.TextString, StringComparison.OrdinalIgnoreCase) != 0)
                                    {
                                        log.DebugFormat("Unterschied in Raumblock {0}: Wert in Attribut = {1}, tatsächlicher Wert = {2}.", blockRef.Handle.ToString(), attRef.TextString, sAtt);
                                        Differs = true;
                                        if (_Modify)
                                        {
                                            attRef.UpgradeOpen();
                                            attRef.TextString = sAtt;
                                            attRef.DowngradeOpen();
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }
                                }
                                else if (string.Compare(attRef.Tag, _PeriAttrib, StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    string sAtt = string.Format(CultureInfo.InvariantCulture, "{0:F2}{1}", dPeri, _PeriString);
                                    if (string.Compare(sAtt, attRef.TextString, StringComparison.OrdinalIgnoreCase) != 0)
                                    {
                                        log.DebugFormat("Unterschied in Raumblock {0}: Wert in Attribut = {1}, tatsächlicher Wert = {2}.", blockRef.Handle.ToString(), attRef.TextString, sAtt);
                                        Differs = true;
                                        if (_Modify)
                                        {
                                            attRef.UpgradeOpen();
                                            attRef.TextString = sAtt;
                                            attRef.DowngradeOpen();
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }
                                }
                            }

                            if (!m2AttFound) attsNotFound++;

                            if (Differs && (!_Modify))
                            {
                                FehlerLineOrHatchPoly(elFG, _DiffersLayer, 0, 0, 255, tm, Globs.GetLabelOrStartPoint(elFG));
                                if (!errorList.Contains(AktFlaecheErrorType.WrongM2)) errorList.Add(AktFlaecheErrorType.WrongM2);
                                fehlerWertFalsch++;
                            }
                        }
                    }

                    if (_Raumbloecke.Count > 0)
                    {
                        List<object> insPoints = new List<object>();
                        for (int i = 0; i < _Raumbloecke.Count; i++)
                        {
                            _AcIntCom.AcadBlockReference rbBlock = (_AcIntCom.AcadBlockReference)Globs.ObjectIdToAcadEntity(_Raumbloecke[i], tm);
                            insPoints.Add(rbBlock.InsertionPoint);
                        }
                        if (!errorList.Contains(AktFlaecheErrorType.RaumblocksWithoutFlaechengrenze)) errorList.Add(AktFlaecheErrorType.RaumblocksWithoutFlaechengrenze);
                        _AcCm.Color col = _AcCm.Color.FromRgb((byte)0, (byte)255, (byte)0);
                        Plan2Ext.Globs.InsertFehlerLines(insPoints, _LooseBlockLayer, 50, Math.PI * 1.25, col);
                    }

                    if (fehlerKeinRb > 0 || fehlerMehrRb > 0 || fehlerWertFalsch > 0 || _Raumbloecke.Count > 0)
                    {
                        string msg = string.Format(CultureInfo.CurrentCulture, "Räume ohne Raumblock: {0}\nRäume mit mehr als einem Raumblock: {1}\nRäume mit falschem Wert in Raumblock: {2}\nRaumblöcke ohne entsprechende Flächengrenzen: {3}", fehlerKeinRb, fehlerMehrRb, fehlerWertFalsch, _Raumbloecke.Count);
                        if (attsNotFound > 0)
                        {
                            msg += string.Format(CultureInfo.CurrentCulture, "\nBlöcke ohne Flächenattribut: {0}!", attsNotFound.ToString());
                        }
                        log.Debug(msg);
                        if (!automated) _AcAp.Application.ShowAlertDialog(msg);
                    }

                    myT.Commit();
                }
            }
            finally
            {
            }

            return errorList;
        }

        internal static void BereinigFehlerlinienAndRegions(bool automated)
        {
            if (!automated) InitVariablesFromConfig();
            DeleteRegions();
            DeleteFehlerSymbols();
        }

        internal static void BereinigRegions(bool automated)
        {
            if (!automated) InitVariablesFromConfig();
            DeleteRegions();
        }

        internal static void AktFlaecheOld(_AcAp.Document doc, string rbName, string flAttrib, string periAttrib, string fgLayer, string afLayer)
        {
            log.Debug("--------------------------");

            _AreaEngine = new AreaEngine();

            const string VIEWNAME = "Temp_AktFlache_View";

            Globs.SaveView(VIEWNAME);
            //_AcGe.Matrix3d ucs = _AcGe.Matrix3d.Identity;
            try
            {
                //Globs.SetWorldView();
                //Globs.ZoomExtents();

                //ucs = doc.Editor.CurrentUserCoordinateSystem;
                //doc.Editor.CurrentUserCoordinateSystem = _AcGe.Matrix3d.Identity;

                InitVariablesFromConfig();

                if (!string.IsNullOrEmpty(rbName)) _RaumblockName = rbName;
                if (!string.IsNullOrEmpty(flAttrib)) _FlAttrib = flAttrib;
                if (!string.IsNullOrEmpty(periAttrib)) _PeriAttrib = periAttrib;
                if (!string.IsNullOrEmpty(fgLayer)) _FgLayer = fgLayer;
                if (!string.IsNullOrEmpty(afLayer)) _AfLayer = afLayer;

                if (!CheckBlockAndAtt(false)) return;

                Plan2Ext.Globs.SetLayerCurrent("0");
                Plan2Ext.Globs.LayersOnRestOffAllThawIC(GetLayerNamesToTurnOn());

                DeleteRegions();
                DeleteFehlerSymbols();

                //var fgRbStructs = AreaEngine.GetFgRbStructs(_RaumblockName, _FgLayer, _AfLayer, doc.Database);
                //var nrOfOverlaps = AreaEngine.NrOfOverlaps;
                //var keinRb = fgRbStructs.Where(x => x.Raumbloecke.Count == 0).ToList();
                //var mehrRb = fgRbStructs.Where(x => x.Raumbloecke.Count > 1).ToList();
                //var orphans = AreaEngine.OrphanRaumblocks;


                if (!_AreaEngine.SelectFgAndRb(_FlaechenGrenzen, _Raumbloecke, _FgLayer, _RaumblockName)) return;

                Globs.SetWorldView();
                Globs.ZoomExtents();

                if (_FlaechenGrenzen.Count == 0)
                {
                    string msg = "Es wurden keine Raumpolylinien gewählt!";
                    log.Warn(msg);
                    _AcAp.Application.ShowAlertDialog(msg);
                    return;
                }

                // todo: läuft nicht synchron - wird dzt in lisp ausgeführt
                //Globs.SetWorldUCS();

                ZoomToFlaechenGrenzen();

                // init div
                int fehlerKeinRb = 0;
                int fehlerMehrRb = 0;
                int fehlerWertFalsch = 0;

                _AcDb.Database db = doc.Database;
                _AcEd.Editor ed = doc.Editor;
                _AcDb.TransactionManager tm = db.TransactionManager;
                _AcDb.Transaction myT = tm.StartTransaction();
                try
                {
                    _AcGe.Point2d lu = new _AcGe.Point2d();
                    _AcGe.Point2d ro = new _AcGe.Point2d();

                    int attsNotFound = 0;

                    for (int i = 0; i < _FlaechenGrenzen.Count; i++)
                    {
                        log.Debug("--------------------------");

                        double sumAF = 0;
                        int rbInd = -1;
                        _AcDb.ObjectId elFG = _FlaechenGrenzen[i];
                        log.DebugFormat("Flächengrenze {0}", elFG.Handle.ToString());

                        _AcDb.Extents3d ext = GetExtents(tm, elFG);
                        _AcGe.Point3d minExt = new _AcGe.Point3d(ext.MinPoint.X - ABSTANDTEXT, ext.MinPoint.Y - ABSTANDTEXT, ext.MinPoint.Z);
                        _AcGe.Point3d maxExt = new _AcGe.Point3d(ext.MaxPoint.X + ABSTANDTEXT, ext.MaxPoint.Y + ABSTANDTEXT, ext.MaxPoint.Z);

                        List<_AcDb.ObjectId> rbsToIgnoreCol = FindRbsForInnerPolyline(minExt, maxExt, elFG);
                        if (rbsToIgnoreCol.Count > 0)
                        {
                            string handles = string.Join(",", rbsToIgnoreCol.Select(x => x.Handle.ToString()).ToArray());
                            log.DebugFormat("Zu ignorierende Raumblöcke: {0}", handles);
                        }

                        //    'raumbloecke und abzugspolylinien holen
                        List<_AcDb.ObjectId> ssAF = SelAbzugAndInnerPolys(ext.MinPoint, ext.MaxPoint, elFG);
                        if (ssAF.Count > 0)
                        {
                            string handles = string.Join(",", ssAF.Select(x => x.Handle.ToString()).ToArray());
                            log.DebugFormat("Abzugpolylinien und abzuziehende Flächengrenzen: {0}", handles);

                        }
                        List<_AcDb.ObjectId> ssRB = SelectRaumblocks(minExt, maxExt);
                        if (ssRB.Count > 0)
                        {
                            string handles = string.Join(",", ssRB.Select(x => x.Handle.ToString()).ToArray());
                            log.DebugFormat("Raumblöcke: {0}", handles);

                        }


                        int rbAnz = 0;
                        //    'raumbloecke pruefen
                        for (int rbCnt = 0; rbCnt < ssRB.Count; rbCnt++)
                        {
                            _AcDb.ObjectId rbBlock2 = ssRB[rbCnt];

                            //      ' ignore rbs
                            _AcDb.ObjectId found = rbsToIgnoreCol.FirstOrDefault(x => x.Equals(rbBlock2));
                            if (found != default(_AcDb.ObjectId)) continue;

                            using (_AcDb.DBObject dbObj = tm.GetObject(rbBlock2, _AcDb.OpenMode.ForRead, false))
                            {
                                _AcGe.Point3d rbEp = ((_AcDb.BlockReference)dbObj).Position;

                                using (_AcDb.Entity elFGEnt = (_AcDb.Entity)tm.GetObject(elFG, _AcDb.OpenMode.ForRead, false))
                                {
                                    if (AreaEngine.InPoly(rbEp, elFGEnt))
                                    {
                                        log.DebugFormat("Raumblock {0} ist innerhalb der Flächengrenze.", rbBlock2.Handle.ToString());

                                        if (_Raumbloecke.Contains(rbBlock2)) _Raumbloecke.Remove(rbBlock2);
                                        rbAnz++;
                                        rbInd = rbCnt;
                                    }
                                    else
                                    {
                                        log.DebugFormat("Außen liegender Raumblock {0} wird ignoriert.", rbBlock2.Handle.ToString());
                                    }

                                }
                            }
                        }


                        double dM2, dPeri;
                        bool errorSymbolSet;
                        if (!GetRegionArea(elFG, ssAF, out dM2, out dPeri, tm, out errorSymbolSet))
                        {
                            if (!errorSymbolSet)
                            {
                                log.WarnFormat("Ungültige Geometrie in Flächengrenze {0}!", elFG.Handle.ToString());
                                FehlerLineOrHatchPoly(elFG, _GeomIncorrectLayer, 255, 255, 0, tm, Globs.GetMiddlePoint(elFG));
                            }
                            continue;
                        }

                        if (rbAnz < 1)
                        {
                            log.WarnFormat("Kein Raumblock in Flächengrenze {0}!", elFG.Handle.ToString());
                            //HatchPoly(elFG, _InvalidNrRb, 255, 0, 0, tm);
                            FehlerLineOrHatchPoly(elFG, _InvalidNrRb, 255, 0, 0, tm, Globs.GetMiddlePoint(elFG));
                            fehlerKeinRb++;

                        }
                        else if (rbAnz > 1)
                        {
                            log.WarnFormat("Mehr als ein Raumblock in Flächengrenze {0}!", elFG.Handle.ToString());
                            //HatchPoly(elFG, _InvalidNrRb, 255, 0, 0, tm);
                            FehlerLineOrHatchPoly(elFG, _InvalidNrRb, 255, 0, 0, tm, Globs.GetMiddlePoint(elFG));
                            fehlerMehrRb++;

                        }
                        else
                        {
                            bool Differs = false;

                            _AcIntCom.AcadBlockReference elRB = Globs.ObjectIdToAcadEntity(ssRB[rbInd], tm) as _AcIntCom.AcadBlockReference;
                            Object[] varAtt = (Object[])elRB.GetAttributes();
                            bool m2AttFound = false;
                            for (int j = 0; j < varAtt.Length; j++)
                            {
                                _AcIntCom.AcadAttributeReference att = varAtt[j] as _AcIntCom.AcadAttributeReference;

                                if (att == null) continue;
                                if (string.Compare(att.TagString, _FlAttrib, StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    m2AttFound = true;
                                    string sAtt = string.Format(CultureInfo.InvariantCulture, "{0:F2}{1}", dM2, _M2Bez);
                                    if (string.Compare(sAtt, att.TextString, StringComparison.OrdinalIgnoreCase) != 0)
                                    {
                                        log.DebugFormat("Unterschied in Raumblock {0}: Wert in Attribut = {1}, tatsächlicher Wert = {2}.", elRB.Handle.ToString(), att.TextString, sAtt);
                                        Differs = true;
                                        if (_Modify)
                                        {
                                            att.TextString = sAtt;
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }
                                }
                                else if (string.Compare(att.TagString, _PeriAttrib, StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    string sAtt = string.Format(CultureInfo.InvariantCulture, "{0:F2}{1}", dPeri, _PeriString);
                                    if (string.Compare(sAtt, att.TextString, StringComparison.OrdinalIgnoreCase) != 0)
                                    {
                                        log.DebugFormat("Unterschied in Raumblock {0}: Wert in Attribut = {1}, tatsächlicher Wert = {2}.", elRB.Handle.ToString(), att.TextString, sAtt);
                                        Differs = true;
                                        if (_Modify)
                                        {
                                            att.TextString = sAtt;
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }
                                }

                            }

                            if (!m2AttFound) attsNotFound++;

                            if (Differs && (!_Modify))
                            {
                                FehlerLineOrHatchPoly(elFG, _DiffersLayer, 0, 0, 255, tm, Globs.GetLabelOrStartPoint(elFG));
                                //HatchPoly(elFG, _DiffersLayer, 0, 0, 255, tm);
                                fehlerWertFalsch++;
                            }

                        }
                    }

                    if (_Raumbloecke.Count > 0)
                    {
                        List<object> insPoints = new List<object>();
                        for (int i = 0; i < _Raumbloecke.Count; i++)
                        {
                            _AcIntCom.AcadBlockReference rbBlock = (_AcIntCom.AcadBlockReference)Globs.ObjectIdToAcadEntity(_Raumbloecke[i], tm);
                            insPoints.Add(rbBlock.InsertionPoint);
                        }

                        //Plan2Ext.Globs.InsertFehlerBlocks(insPoints, _LooseBlockLayer);
                        _AcCm.Color col = _AcCm.Color.FromRgb((byte)0, (byte)255, (byte)0);

                        Plan2Ext.Globs.InsertFehlerLines(insPoints, _LooseBlockLayer, 50, Math.PI * 1.25, col);

                    }



                    //If wucs = 0 Then
                    //    ThisDrawing.SendCommand "(command ""_.UCS"" ""_P"") "
                    //End If

                    if (fehlerKeinRb > 0 || fehlerMehrRb > 0 || fehlerWertFalsch > 0 || _Raumbloecke.Count > 0)
                    {
                        string msg = string.Format(CultureInfo.CurrentCulture, "Räume ohne Raumblock: {0}\nRäume mit mehr als einem Raumblock: {1}\nRäume mit falschem Wert in Raumblock: {2}\nRaumblöcke ohne entsprechende Flächengrenzen: {3}", fehlerKeinRb, fehlerMehrRb, fehlerWertFalsch, _Raumbloecke.Count);
                        if (attsNotFound > 0)
                        {
                            msg += string.Format(CultureInfo.CurrentCulture, "\nBlöcke ohne Flächenattribut: {0}!", attsNotFound.ToString());
                        }
                        log.Debug(msg);
                        _AcAp.Application.ShowAlertDialog(msg);

                    }

                    //If wucs = 0 Then
                    //    ThisDrawing.SendCommand "(command ""_.UCS"" ""_P"") "
                    //End If

                    myT.Commit();

                }
                finally
                {
                    myT.Dispose();
                }

            }
            finally
            {
                //doc.Editor.CurrentUserCoordinateSystem = ucs;
                Globs.RestoreView(VIEWNAME);
                Globs.DeleteView(VIEWNAME);
            }
        }

        private static List<string> GetLayerNamesToTurnOn()
        {
            var lst = new List<string> { "0", _FgLayer, _AfLayer, _GeomIncorrectLayer, _AbzGeomIncorrectLayer, _InvalidNrRb, _RegionLayer, _DiffersLayer, _LooseBlockLayer };
            var rbLayers = GetRaumblockLayers();
            foreach (var rbLay in rbLayers)
            {
                if (!ContainsLambda(lst, rbLay)) lst.Add(rbLay);
            }
            return lst.Where(x => !string.IsNullOrEmpty(x.Trim())).ToList();
        }

        private static bool ContainsLambda(List<string> lst, string elem)
        {
            var ok = lst.FirstOrDefault(x => x == elem);
            return (!string.IsNullOrEmpty(ok));
        }

        private static List<string> GetRaumblockLayers()
        {
            var lst = new List<string>();
            if (string.IsNullOrEmpty(_RaumblockName.Trim())) return lst;
            return Plan2Ext.Globs.GetBlockAttributeLayers(_RaumblockName);
        }

        public static void InitVariablesFromConfig()
        {
            string val;
            if (GetFromConfig(out val, "alx_V:ino_rbName")) _RaumblockName = val;
            if (GetFromConfig(out val, "alx_V:ino_flattrib")) _FlAttrib = val;
            if (GetFromConfig(out val, "alx_V:ino_fglayer")) _FgLayer = val;
            if (GetFromConfig(out val, "alx_V:ino_aflayer")) _AfLayer = val;
            if (GetFromConfig(out val, "alx_V:ino_PeriAttrib")) _PeriAttrib = val;
            if (GetFromConfig(out val, "alx_V:ino_PeriString")) _PeriString = val;
            if (GetFromConfig(out val, "alx_V:ino_GeomIncorrectLayer")) _GeomIncorrectLayer = val;
            if (GetFromConfig(out val, "alx_V:ino_AbzGeomIncorrectLayer")) _AbzGeomIncorrectLayer = val;
            if (GetFromConfig(out val, "alx_V:ino_InvalidNrRbLayer")) _InvalidNrRb = val;
            if (GetFromConfig(out val, "alx_V:ino_RegionLayer")) _RegionLayer = val;
            if (GetFromConfig(out val, "alx_V:ino_UpdFlaDiffersLayer")) _DiffersLayer = val;
            if (GetFromConfig(out val, "alx_V:ino_UpdFlaLooseRbLayer")) _LooseBlockLayer = val;
            if (GetFromConfig(out val, "alx_V:ino_flString")) _M2Bez = val;
        }

        private static bool GetFromConfig(out string val, string varName)
        {
            val = null;
            try
            {
                val = TheConfiguration.GetValueString(varName);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static bool CheckBlockAndAtt(bool automated)
        {
            try
            {
                if (!Globs.BlockHasAttribute(_RaumblockName, _FlAttrib))
                {
                    string msg = string.Format(CultureInfo.CurrentCulture, "Der Block '{0}' hat kein Attribut '{1}'!", _RaumblockName, _FlAttrib);
                    log.Debug(msg);
                    if (!automated) _AcAp.Application.ShowAlertDialog(msg);
                    return false;
                }

            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
                if (!automated) _AcAp.Application.ShowAlertDialog(ex.Message);
                return false;
            }
            return true;
        }

        private static void DeleteFehlerSymbols()
        {
            Globs.DeleteFehlerLines(_GeomIncorrectLayer);
            Globs.DeleteHatches(_GeomIncorrectLayer);

            Globs.DeleteFehlerLines(_InvalidNrRb);
            Globs.DeleteHatches(_InvalidNrRb);

            Globs.DeleteFehlerLines(_DiffersLayer);
            Globs.DeleteHatches(_DiffersLayer);

            Globs.DeleteFehlerLines(_LooseBlockLayer);

        }

        private static double GetAreaFromPolyline(_AcIntCom.AcadEntity oPoly)
        {
            if (oPoly is _AcIntCom.AcadPolyline)
            {
                _AcIntCom.AcadPolyline poly1 = (_AcIntCom.AcadPolyline)oPoly;
                return poly1.Area;
            }
            else if (oPoly is _AcIntCom.AcadLWPolyline)
            {
                _AcIntCom.AcadLWPolyline poly2 = (_AcIntCom.AcadLWPolyline)oPoly;
                return poly2.Area;
            }

            throw new InvalidOperationException(string.Format("Element {0} hat Type {1} anstatt Polylinie!", oPoly.Handle, oPoly.GetType().Name));
        }

        private static bool IsClosed(_AcIntCom.AcadEntity oPoly)
        {
            const double eps = 0.00001;
            if (oPoly is _AcIntCom.AcadPolyline)
            {
                _AcIntCom.AcadPolyline poly1 = (_AcIntCom.AcadPolyline)oPoly;
                if (poly1.Closed) return true;

                double[] coords = (double[])poly1.Coordinates;
                var len = coords.Length;

                double[] fromPoint = new double[] { coords[0], coords[1] };
                double[] toPoint = new double[] { coords[len - 3], coords[len - 2] };
                return (dblEqual(fromPoint[0], toPoint[0], eps) && dblEqual(fromPoint[1], toPoint[1], eps));
            }
            else if (oPoly is _AcIntCom.AcadLWPolyline)
            {
                _AcIntCom.AcadLWPolyline poly2 = (_AcIntCom.AcadLWPolyline)oPoly;
                if (poly2.Closed) return true;


                double[] coords = (double[])poly2.Coordinates;
                var len = coords.Length;

                double[] fromPoint = new double[] { coords[0], coords[1] };
                double[] toPoint = new double[] { coords[len - 2], coords[len - 1] };

                return (dblEqual(fromPoint[0], toPoint[0], eps) && dblEqual(fromPoint[1], toPoint[1], eps));

            }

            throw new InvalidOperationException(string.Format("Element {0} hat Type {1} anstatt Polylinie!", oPoly.Handle, oPoly.GetType().Name));
        }

        private static double GetPerimeterFromPolyline(_AcIntCom.AcadEntity oPoly)
        {
            if (oPoly is _AcIntCom.AcadPolyline)
            {
                _AcIntCom.AcadPolyline poly1 = (_AcIntCom.AcadPolyline)oPoly;
                return poly1.Length;
            }
            else if (oPoly is _AcIntCom.AcadLWPolyline)
            {
                _AcIntCom.AcadLWPolyline poly2 = (_AcIntCom.AcadLWPolyline)oPoly;
                return poly2.Length;
            }

            throw new InvalidOperationException(string.Format("Element {0} hat Type {1} anstatt Polylinie!", oPoly.Handle, oPoly.GetType().Name));

        }

        private static void FehlerLineOrHatchPoly(_AcDb.ObjectId oid, string layer, int red, int green, int blue, _AcDb.TransactionManager tm, _AcGe.Point3d? label)
        {
            if (label.HasValue)
            {
                _AcCm.Color col = _AcCm.Color.FromRgb((byte)red, (byte)green, (byte)blue);
                Globs.InsertFehlerLines(new List<_AcGe.Point3d> { label.Value }, layer, 50, Math.PI * 1.25, col);
            }
            else
            {
                HatchPoly(oid, layer, red, green, blue, tm);
            }
        }
        private static void HatchPoly(_AcDb.ObjectId oid, string layer, int red, int green, int blue, _AcDb.TransactionManager tm)
        {
            string patternName = "_SOLID";
            bool bAssociativity = false;

            _AcIntCom.AcadEntity oPoly = Globs.ObjectIdToAcadEntity(oid, tm);
            _AcIntCom.AcadEntity oCopiedPoly = null;
            if (oPoly is _AcIntCom.AcadPolyline)
            {
                _AcIntCom.AcadPolyline poly1 = (_AcIntCom.AcadPolyline)oPoly;
                oCopiedPoly = (_AcIntCom.AcadEntity)poly1.Copy();
                ((_AcIntCom.AcadPolyline)oCopiedPoly).Closed = true;
            }
            else if (oPoly is _AcIntCom.AcadLWPolyline)
            {
                _AcIntCom.AcadLWPolyline poly2 = (_AcIntCom.AcadLWPolyline)oPoly;
                oCopiedPoly = (_AcIntCom.AcadEntity)poly2.Copy();
                ((_AcIntCom.AcadLWPolyline)oCopiedPoly).Closed = true;
            }
            else // 3dpoly
            {
                _AcIntCom.Acad3DPolyline poly2 = (_AcIntCom.Acad3DPolyline)oPoly;
                oCopiedPoly = (_AcIntCom.AcadEntity)poly2.Copy();
                ((_AcIntCom.Acad3DPolyline)oCopiedPoly).Closed = true;
            }

            //' Create the non associative Hatch object in model space
            _AcInt.AcadApplication app = (_AcInt.AcadApplication)_AcAp.Application.AcadApplication;
            //log.Info("1..");
            //Autodesk.AutoCAD.Interop.AcadApplication abc = (Autodesk.AutoCAD.Interop.AcadApplication)Autodesk.AutoCAD.ApplicationServices.Application.AcadApplication;
            //log.Info("2..");

            _AcIntCom.AcadHatch hatchObj = app.ActiveDocument.ModelSpace.AddHatch(AcPatternType, patternName, bAssociativity, AcHatchObject);
            _AcIntCom.AcadAcCmColor col1 = new _AcIntCom.AcadAcCmColor(); // app.GetInterfaceObject(COLOROBJECTPROGID) as AcadAcCmColor;
            //AcadAcCmColor col2 = app.GetInterfaceObject(COLOROBJECTPROGID) as AcadAcCmColor;
            col1.SetRGB(red, green, blue);
            hatchObj.TrueColor = col1;
            _AcIntCom.AcadEntity[] outerLoop = new _AcIntCom.AcadEntity[] { oCopiedPoly };
            hatchObj.AppendOuterLoop(outerLoop);
            SetLayer((_AcIntCom.AcadEntity)hatchObj, layer);
            if (oCopiedPoly != null) oCopiedPoly.Delete();

        }

        private static void SetLayer(_AcIntCom.AcadEntity oCopiedPoly, string layerName)
        {
            Globs.CreateLayer(layerName);
            oCopiedPoly.Layer = layerName;

        }

        private static bool GetRegionArea(_AcDb.ObjectId elFG, List<_AcDb.ObjectId> ssAF, out double dM2, out double dPeri, _AcDb.TransactionManager tm, out bool errorSymbolSet)
        {
            errorSymbolSet = false;

            _AcIntCom.AcadRegion region = default(_AcIntCom.AcadRegion);
            dM2 = 0.0; dPeri = 0.0;
            _AcInt.AcadApplication app = (_AcInt.AcadApplication)_AcAp.Application.AcadApplication;
            _AcIntCom.AcadEntity oldCurve = Globs.ObjectIdToAcadEntity(elFG, tm);

            _AcIntCom.AcadEntity[] curves = new _AcIntCom.AcadEntity[] { oldCurve };
            try
            {
#if BRX_APP
                System.Object[] objs = (System.Object[])app.ActiveDocument.database.ModelSpace.AddRegion(curves);
#else
                System.Object[] objs = (System.Object[])app.ActiveDocument.Database.ModelSpace.AddRegion(curves);
#endif
                if (objs == null || objs.Length == 0) return false;
                region = (_AcIntCom.AcadRegion)objs[0];
            }
            catch (System.Exception)
            {
                log.DebugFormat("Konnte keine Region aus der Flächengrenze {0} erzeugen. Verwende Polylinien-Algorithmus.", elFG.Handle.ToString());
                return GetPolylineArea(elFG, ssAF, out dM2, out dPeri, tm, out errorSymbolSet);
            }
            dPeri = region.Perimeter;
            dM2 = region.Area;
            SetLayer((_AcIntCom.AcadEntity)region, _RegionLayer);


            log.DebugFormat("Region aus Flächengrenze erzeugt. {0} m2", dM2.ToString());

            for (int i = 0; i < ssAF.Count; i++)
            {
                _AcIntCom.AcadRegion AbzRegion = null;
                _AcDb.ObjectId oid = ssAF[i];
                log.DebugFormat("Erzeuge Region aus Abzugsfläche {0}", oid.Handle.ToString());

                _AcIntCom.AcadEntity AbzCurve = Globs.ObjectIdToAcadEntity(ssAF[i], tm);
                curves[0] = AbzCurve;
                try
                {
#if BRX_APP
                    System.Object[] objs = (System.Object[])app.ActiveDocument.database.ModelSpace.AddRegion(curves);
#else
                    System.Object[] objs = (System.Object[])app.ActiveDocument.Database.ModelSpace.AddRegion(curves);
#endif
                    if (objs == null || objs.Length == 0) return false;
                    AbzRegion = (_AcIntCom.AcadRegion)objs[0];
                }
                catch (System.Exception)
                {
                    log.DebugFormat("Konnte keine Region aus der Abzugsfläche {0} erzeugen. Verwende Polylinien-Algorithmus.", oid.Handle.ToString());
                    return GetPolylineArea(elFG, ssAF, out dM2, out dPeri, tm, out errorSymbolSet);

                    //HatchPoly(ssAF[i], _AbzGeomIncorrectLayer, 255, 255, 0, tm);
                    //continue;
                }

                SetLayer((_AcIntCom.AcadEntity)AbzRegion, _RegionLayer);

                // ignore double flaechengrenze oder complete overlapping areas
                if (!CompleteOverlap(AbzRegion, region, dM2, tm))
                {
                    log.DebugFormat("Schneide Abzugsfläche {0} ab von Flächenregion", oid.Handle.ToString());
                    region.Boolean(_AcIntCom.AcBooleanType.acSubtraction, AbzRegion);
                }
                else
                {
                    log.DebugFormat("Abzugsfläche {0} ist identisch mit Flächenregion oder komplett überlappend und wird daher ignoriert.", oid.Handle.ToString());
                }
            }
            dM2 = region.Area;
            dPeri = region.Perimeter;
            return true;
        }

        /// <summary>
        /// Get Area via Polyline. Fallback, if Region doesn't work
        /// </summary>
        /// <param name="elFG"></param>
        /// <param name="ssAF"></param>
        /// <param name="dM2"></param>
        /// <param name="dPeri"></param>
        /// <param name="tm"></param>
        /// <param name="region"></param>
        /// <returns></returns>
        private static bool GetPolylineArea(_AcDb.ObjectId elFG, List<_AcDb.ObjectId> ssAF, out double dM2, out double dPeri, _AcDb.TransactionManager tm, out bool errorSymbolSet)
        {
            errorSymbolSet = false;

            dM2 = 0.0; dPeri = 0.0;
            _AcInt.AcadApplication app = (_AcInt.AcadApplication)_AcAp.Application.AcadApplication;

            _AcIntCom.AcadEntity flaechenGrenze = Globs.ObjectIdToAcadEntity(elFG, tm);

            if (!AssertClosedPolyline(flaechenGrenze, elFG, tm))
            {
                errorSymbolSet = true;
                return false;
            }

            dM2 = GetAreaFromPolyline(flaechenGrenze);
            dPeri = GetPerimeterFromPolyline(flaechenGrenze);

            log.DebugFormat("Flächengrenze als Polylinie.Vor Abzug der Abzugsflächen {0} m2", dM2.ToString());

            List<_AcDb.ObjectId> ssAF2 = new List<_AcDb.ObjectId>();
            for (int i = 0; i < ssAF.Count; i++)
            {
                _AcDb.ObjectId oid = ssAF[i];
                if (!PolyInPoly(tm, oid, elFG))
                {
                    log.DebugFormat("Vermeintliche Abzugsfläche {0} ist nicht innerhalb der Flächengrenze", oid.Handle.ToString());
                    continue;
                }

                ssAF2.Add(oid);
            }

            List<_AcDb.ObjectId> ssAF3 = new List<_AcDb.ObjectId>();
            for (int i = 0; i < ssAF2.Count; i++)
            {
                _AcDb.ObjectId oid = ssAF2[i];
                bool inOtherAf = false;
                for (int j = 0; j < ssAF2.Count; j++)
                {
                    _AcDb.ObjectId compOid = ssAF2[j];
                    if (i == j) continue;
                    if (PolyInPoly(tm, oid, compOid))
                    {
                        log.DebugFormat("Abzugsfläche {0} ist innerhalb anderer Abzugsfläche {1}.", oid.Handle.ToString(), compOid.Handle.ToString());
                        inOtherAf = true;
                        break;
                    }

                }
                if (!inOtherAf) ssAF3.Add(oid);
            }

            for (int i = 0; i < ssAF3.Count; i++)
            {
                _AcDb.ObjectId oid = ssAF3[i];
                _AcIntCom.AcadEntity ae = Globs.ObjectIdToAcadEntity(oid, tm);

                if (!AssertClosedPolyline(ae, oid, tm))
                {
                    errorSymbolSet = true;
                    return false;
                }

                double m2 = GetAreaFromPolyline(ae);

                dM2 -= m2;
                double per = GetPerimeterFromPolyline(ae);
                dPeri += per;

            }

            return true;
        }

        private static bool AssertClosedPolyline(_AcIntCom.AcadEntity poly, _AcDb.ObjectId elFG, _AcDb.TransactionManager tm)
        {
            if (!IsClosed(poly))
            {
                log.WarnFormat("Polylinie '{0}' ist nicht geschlossen!", poly.Handle);
                FehlerLineOrHatchPoly(elFG, _GeomIncorrectLayer, 255, 255, 0, tm, Globs.GetStartPoint(elFG));
                return false;
            }
            return true;
        }

        private static bool CompleteOverlap(_AcIntCom.AcadRegion abzRegion, _AcIntCom.AcadRegion region, double m2, _AcDb.TransactionManager tm)
        {
            const double eps = 0.000001;

            //if (dblEqual(abzRegion.Area, m2, eps))
            //{


            _AcIntCom.AcadRegion abz2 = abzRegion.Copy() as _AcIntCom.AcadRegion;
            _AcIntCom.AcadRegion region2 = region.Copy() as _AcIntCom.AcadRegion;
            region2.Boolean(_AcIntCom.AcBooleanType.acSubtraction, abz2);

            bool ret;
            if (dblEqual(region2.Area, 0.0, eps)) ret = true;
            else ret = false;

            ((_AcIntCom.IAcadObject)region2).Delete();

            return ret;


            //}
            //else return false;
        }

        private static bool dblEqual(double d1, double d2, double eps)
        {
            return Math.Abs(d1 - d2) <= eps;
        }

        private static double GetArea(_AcDb.ObjectId elFG, _AcDb.TransactionManager tm)
        {
            using (_AcDb.DBObject dbo = tm.GetObject(elFG, _AcDb.OpenMode.ForRead, false))
            {
                if (dbo is _AcDb.Curve)
                {
                    return ((_AcDb.Curve)dbo).Area;
                }
                else return 0.0;
            }
        }

        private static List<_AcDb.ObjectId> SelectRaumblocks(_AcGe.Point3d minExt, _AcGe.Point3d maxExt)
        {
            _AcEd.Editor ed = _AcAp.Application.DocumentManager.MdiActiveDocument.Editor;
            _AcEd.SelectionFilter filter = new _AcEd.SelectionFilter(new _AcDb.TypedValue[] { 
                new _AcDb.TypedValue((int)_AcDb.DxfCode.Start,"INSERT" ),
                //new _AcDb.TypedValue((int)_AcDb.DxfCode.BlockName,_RaumblockName)
            });
            _AcEd.PromptSelectionResult res = null;
            res = ed.SelectCrossingWindow(minExt, maxExt, filter);
            if (res.Status != _AcEd.PromptStatus.OK)
            {
                log.Warn("Fehler beim Auswählen der Raumblöcke!");
                return new List<_AcDb.ObjectId>();
            }

#if BRX_APP
            _AcEd.SelectionSet ss = res.Value;
#else
            using (_AcEd.SelectionSet ss = res.Value)
#endif
            {
                List<_AcDb.ObjectId> theBlockOids = new List<_AcDb.ObjectId>();

                _AcAp.Document doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
                using (_AcDb.Transaction myT = doc.TransactionManager.StartTransaction())
                {

                    var lstBlocks = ss.GetObjectIds();
                    foreach (var oid in lstBlocks)
                    {
                        var br = myT.GetObject(oid, _AcDb.OpenMode.ForRead) as _AcDb.BlockReference;
                        if (br != null && string.Compare(Plan2Ext.Globs.GetBlockname(br, myT), _RaumblockName, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            theBlockOids.Add(oid);
                        }
                    }
                    myT.Commit();
                }
                return theBlockOids;
            }
        }

        private static List<_AcDb.ObjectId> SelAbzugAndInnerPolys(_AcGe.Point3d minExt, _AcGe.Point3d maxExt, _AcDb.ObjectId elFG)
        {
            _AcEd.Editor ed = _AcAp.Application.DocumentManager.MdiActiveDocument.Editor;
            _AcEd.SelectionFilter filter = new _AcEd.SelectionFilter(new _AcDb.TypedValue[] { 
                new _AcDb.TypedValue((int)_AcDb.DxfCode.Start,"*POLYLINE" ),
                new _AcDb.TypedValue((int)_AcDb.DxfCode.Operator ,"<OR"),
                new _AcDb.TypedValue((int)_AcDb.DxfCode.LayerName,_AfLayer  ),
                new _AcDb.TypedValue((int)_AcDb.DxfCode.LayerName,_FgLayer  ),
                new _AcDb.TypedValue((int)_AcDb.DxfCode.Operator ,"OR>")

            });
            _AcEd.PromptSelectionResult res = null;
            res = ed.SelectCrossingWindow(minExt, maxExt, filter);
            if (res.Status != _AcEd.PromptStatus.OK)
            {
                log.Warn("Fehler beim Auswählen der Abzugsflächen und Flächengrenzen!");
                return new List<_AcDb.ObjectId>();
            }

#if BRX_APP
            _AcEd.SelectionSet ss = res.Value;
#else
            using (_AcEd.SelectionSet ss = res.Value)
#endif
            {
                // polylinepoly? 
                _AcDb.ObjectId[] idArray = ss.GetObjectIds();
                return idArray.Where(x => !x.Equals(elFG)).ToList();
            }

        }

        private static void ZoomToFlaechenGrenzen()
        {
            log.DebugFormat(CultureInfo.CurrentCulture, "Zoom auf Flächengrenzen");
            if (_FlaechenGrenzen.Count == 0) return;

            double MinX, MinY, MaxX, MaxY;

            _AcAp.Document doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            _AcDb.Database db = doc.Database;
            _AcEd.Editor ed = doc.Editor;
            _AcDb.TransactionManager tm = db.TransactionManager;
            _AcDb.Transaction myT = tm.StartTransaction();
            try
            {

                _AcDb.Extents3d ext = GetExtents(tm, _FlaechenGrenzen[0]);
                MinX = ext.MinPoint.X;
                MinY = ext.MinPoint.Y;
                MaxX = ext.MaxPoint.X;
                MaxY = ext.MaxPoint.Y;

                for (int i = 1; i < _FlaechenGrenzen.Count; i++)
                {
                    _AcDb.ObjectId oid = _FlaechenGrenzen[i];
                    ext = GetExtents(tm, oid);
                    if (ext.MinPoint.X < MinX) MinX = ext.MinPoint.X;
                    if (ext.MinPoint.Y < MinY) MinY = ext.MinPoint.Y;
                    if (ext.MaxPoint.X > MaxX) MaxX = ext.MaxPoint.X;
                    if (ext.MaxPoint.Y > MaxY) MaxY = ext.MaxPoint.Y;
                }


                //Globs.Zoom( new Point3d(MinX, MinY, 0.0), new Point3d(MaxX, MaxY, 0.0),new Point3d(), 1.0);
                //Globs.ZoomWin3(ed, new Point3d(MinX, MinY, 0.0), new Point3d(MaxX, MaxY, 0.0));
                //Globs.ZoomWin2(ed, new Point3d(MinX, MinY, 0.0), new Point3d(MaxX, MaxY, 0.0));

                myT.Commit();

            }
            finally
            {
                myT.Dispose();
            }

            // Rauszoomen, sonst werden Blöcken nicht gefunden, die außerhalb der Flächengrenzen liegen.
            MinX -= ABSTANDTEXT;
            MinY -= ABSTANDTEXT;
            MaxX += ABSTANDTEXT;
            MaxY += ABSTANDTEXT;

            Globs.Zoom(new _AcGe.Point3d(MinX, MinY, 0.0), new _AcGe.Point3d(MaxX, MaxY, 0.0), new _AcGe.Point3d(), 1.0);

        }

        private static _AcDb.Extents3d GetExtents(_AcDb.TransactionManager tm, _AcDb.ObjectId oid)
        {
            using (_AcDb.DBObject dbobj = tm.GetObject(oid, _AcDb.OpenMode.ForRead, false))
            {
                _AcDb.Entity ent = dbobj as _AcDb.Entity;
                return ent.GeometricExtents;
            }
        }

        private static void DeleteRegions()
        {
            log.DebugFormat(CultureInfo.CurrentCulture, "Lösche Regions auf Layer {0}", _RegionLayer);
            DeleteEntities("REGION", _RegionLayer);
        }

        private static List<_AcDb.ObjectId> FindRbsForInnerPolyline(_AcGe.Point3d minExt, _AcGe.Point3d maxExt, _AcDb.ObjectId elFG)
        {
            List<_AcDb.ObjectId> Ret = new List<_AcDb.ObjectId>();

            _AcEd.Editor ed = _AcAp.Application.DocumentManager.MdiActiveDocument.Editor;
            _AcEd.SelectionFilter filter = new _AcEd.SelectionFilter(new _AcDb.TypedValue[] { 
                new _AcDb.TypedValue((int)_AcDb.DxfCode.Start,"*POLYLINE" ),
                new _AcDb.TypedValue((int)_AcDb.DxfCode.LayerName,_FgLayer )
            });
            _AcEd.PromptSelectionResult res = null;
            res = ed.SelectCrossingWindow(minExt, maxExt, filter);
            //res = ed.SelectAll(filter);
            if (res.Status != _AcEd.PromptStatus.OK)
            {
                // todo: logging: lot4net?
                return Ret;
            }

#if BRX_APP
            _AcEd.SelectionSet ss = res.Value;
#else
            using (_AcEd.SelectionSet ss = res.Value)
#endif
            {

                _AcDb.ObjectId[] idArray = ss.GetObjectIds();
                _AcDb.Database db = _AcAp.Application.DocumentManager.MdiActiveDocument.Database;
                _AcDb.TransactionManager tm = db.TransactionManager;
                _AcDb.Transaction myT = tm.StartTransaction();
                try
                {
                    for (int i = 0; i < idArray.Length; i++)
                    {
                        _AcDb.ObjectId oid = idArray[i];
                        if (!oid.Equals(elFG))
                        {
                            if (PolyInPoly(tm, oid, elFG))
                                AddRbToRetCol(Ret, tm, oid);
                        }
                    }
                    myT.Commit();
                }
                finally
                {
                    myT.Dispose();
                }

            }

            return Ret;

        }

        private static void AddRbToRetCol(List<_AcDb.ObjectId> Ret, _AcDb.TransactionManager tm, _AcDb.ObjectId elFG)
        {

            _AcDb.Extents3d ext = GetExtents(tm, elFG);
            _AcGe.Point3d minExt = new _AcGe.Point3d(ext.MinPoint.X - ABSTANDTEXT, ext.MinPoint.Y - ABSTANDTEXT, ext.MinPoint.Z);
            _AcGe.Point3d maxExt = new _AcGe.Point3d(ext.MaxPoint.X + ABSTANDTEXT, ext.MaxPoint.Y + ABSTANDTEXT, ext.MaxPoint.Z);

            _AcEd.Editor ed = _AcAp.Application.DocumentManager.MdiActiveDocument.Editor;
            _AcEd.SelectionFilter filter = new _AcEd.SelectionFilter(new _AcDb.TypedValue[] { 
                new _AcDb.TypedValue((int)_AcDb.DxfCode.Start,"INSERT" ),
                new _AcDb.TypedValue((int)_AcDb.DxfCode.BlockName,_RaumblockName)
            });
            _AcEd.PromptSelectionResult res = null;
            res = ed.SelectCrossingWindow(minExt, maxExt, filter);
            if (res.Status != _AcEd.PromptStatus.OK)
            {
                // todo: logging: lot4net?
                return;
            }

#if BRX_APP
            _AcEd.SelectionSet ss = res.Value;
#else
            using (_AcEd.SelectionSet ss = res.Value)
#endif
            {


                _AcDb.ObjectId[] idArray = ss.GetObjectIds();
                for (int i = 0; i < idArray.Length; i++)
                {
                    _AcDb.ObjectId oid = idArray[i];
                    using (_AcDb.DBObject pEntity = tm.GetObject(oid, _AcDb.OpenMode.ForRead, false))
                    {
                        using (_AcDb.Entity entElFG = tm.GetObject(elFG, _AcDb.OpenMode.ForRead, false) as _AcDb.Entity)
                        {
                            if (pEntity is _AcDb.BlockReference)
                            {
                                _AcDb.BlockReference br = pEntity as _AcDb.BlockReference;
                                if (AreaEngine.InPoly(br.Position, entElFG))
                                {
                                    Ret.Add(oid);
                                }
                            }
                        }
                    }
                }
            }
        }

        private static bool PolyInPoly(_AcDb.TransactionManager tm, _AcDb.ObjectId oid, _AcDb.ObjectId elFG)
        {
            using (_AcDb.DBObject pEntity = tm.GetObject(oid, _AcDb.OpenMode.ForRead, false))
            {
                using (_AcDb.DBObject pElFG = tm.GetObject(elFG, _AcDb.OpenMode.ForRead, false))
                {

                    if (pEntity is _AcDb.Polyline2d)
                    {
                        _AcDb.Polyline2d oldPolyline = (_AcDb.Polyline2d)pEntity;
                        foreach (_AcDb.ObjectId Vertex2d in oldPolyline)
                        {
                            using (_AcDb.DBObject dbobj = tm.GetObject(Vertex2d, _AcDb.OpenMode.ForRead, false))
                            {
                                _AcDb.Vertex2d vertex = dbobj as _AcDb.Vertex2d;

                                if (vertex == null)
                                {
                                    string msg = string.Format(CultureInfo.CurrentCulture, "Polylinie {0} gibt falsches Objekt {1} als Vertex zurück.", oldPolyline.Handle.ToString(), dbobj.GetType().ToString());
                                    throw new InvalidOperationException(string.Format(msg));
                                }

                                _AcGe.Point3d vertexPoint = oldPolyline.VertexPosition(vertex);
                                if (!AreaEngine.InPoly(vertexPoint, (_AcDb.Entity)pElFG)) return false;

                            }
                        }
                        return true;
                    }
                    else if (pEntity is _AcDb.Polyline3d)
                    {
                        _AcDb.Polyline3d poly3d = (_AcDb.Polyline3d)pEntity;
                        foreach (_AcDb.ObjectId Vertex3d in poly3d)
                        {
                            using (_AcDb.DBObject dbobj = tm.GetObject(Vertex3d, _AcDb.OpenMode.ForRead, false))
                            {
                                _AcDb.PolylineVertex3d vertex = dbobj as _AcDb.PolylineVertex3d;

                                if (vertex == null)
                                {
                                    string msg = string.Format(CultureInfo.CurrentCulture, "3D-Polylinie {0} gibt falsches Objekt {1} als Vertex zurück.", poly3d.Handle.ToString(), dbobj.GetType().ToString());
                                    throw new InvalidOperationException(string.Format(msg));
                                }

                                _AcGe.Point3d vertexPoint = vertex.Position;
                                if (!AreaEngine.InPoly(vertexPoint, (_AcDb.Entity)pElFG)) return false;

                            }
                        }
                        return true;
                    }
                    else if (pEntity is _AcDb.Polyline)
                    {
                        _AcDb.Polyline poly = pEntity as _AcDb.Polyline;
                        for (int i = 0; i < poly.NumberOfVertices; i++)
                        {
                            _AcGe.Point3d vertexPoint = poly.GetPoint3dAt(i);
                            if (!AreaEngine.InPoly(vertexPoint, (_AcDb.Entity)pElFG)) return false;

                        }
                        return true;
                    }
                }
            }
            return false;
        }

        #region DeleteEntities

        private static void DeleteEntities(string entityType, string layer)
        {
            DeleteEntities(new _AcEd.SelectionFilter(new _AcDb.TypedValue[] { 
                new _AcDb.TypedValue((int)_AcDb.DxfCode.Start,entityType ),
                new _AcDb.TypedValue((int)_AcDb.DxfCode.LayerName,layer)
            }));
        }
        private static void DeleteEntities(_AcEd.SelectionFilter filter)
        {
            _AcEd.Editor ed = _AcAp.Application.DocumentManager.MdiActiveDocument.Editor;
            _AcEd.PromptSelectionResult res = null;
            res = ed.SelectAll(filter);
            if (res.Status != _AcEd.PromptStatus.OK) return;

#if BRX_APP
            _AcEd.SelectionSet ss = res.Value;
#else
            using (_AcEd.SelectionSet ss = res.Value)
#endif
            {

                _AcDb.ObjectId[] idArray = ss.GetObjectIds();
                _AcDb.Database db = _AcAp.Application.DocumentManager.MdiActiveDocument.Database;
                _AcDb.TransactionManager tm = db.TransactionManager;
                _AcDb.Transaction myT = tm.StartTransaction();
                try
                {

                    for (int i = 0; i < idArray.Length; i++)
                    {

                        try
                        {
                            using (_AcDb.DBObject obj = tm.GetObject(idArray[i], _AcDb.OpenMode.ForWrite, false))
                            {
                                obj.Erase();
                            }

                        }
                        catch (System.Exception ex)
                        {
                            log.Error(ex.Message, ex);
                        }

                    }
                    myT.Commit();
                }
                finally
                {
                    myT.Dispose();
                }

            }


        }

        #endregion

        #region Args
        private static void GetArgs(_AcDb.ResultBuffer rb)
        {
            _AcDb.TypedValue[] values = rb.AsArray();
            _RaumblockName = values[1].Value.ToString();
            _FlAttrib = values[2].Value.ToString();
            _FgLayer = values[3].Value.ToString();
            _AfLayer = values[4].Value.ToString();
            _PeriAttrib = values[5].Value.ToString();
            _PeriString = values[6].Value.ToString();
            _GeomIncorrectLayer = values[7].Value.ToString();
            _AbzGeomIncorrectLayer = values[8].Value.ToString();
            _InvalidNrRb = values[9].Value.ToString();
            _RegionLayer = values[10].Value.ToString();
            _DiffersLayer = values[11].Value.ToString();
            _LooseBlockLayer = values[12].Value.ToString();
            _M2Bez = values[13].Value.ToString();
            if (values[14].Value == null) _Modify = false;
            else _Modify = true;

            log.DebugFormat(CultureInfo.InvariantCulture, "Raumblock: {0}", _RaumblockName);
            log.DebugFormat(CultureInfo.InvariantCulture, "Area-Attrib: {0}", _FlAttrib);
            log.DebugFormat(CultureInfo.InvariantCulture, "Flächengrenze-Layer: {0}", _FgLayer);
            log.DebugFormat(CultureInfo.InvariantCulture, "Abzugsfläche-Layer: {0}", _AfLayer);



        }

        #endregion

        #region Free

        private static void Free()
        {
            _FlaechenGrenzen.Clear();
            _Raumbloecke.Clear();
            //FreeSelectionSet(ref _ssFG);
            //FreeSelectionSet(ref _ssRB);
        }
        private static void FreeSelectionSet(ref _AcEd.SelectionSet ss)
        {
            if (ss == null) return;
#if ARX_APP
            ss.Dispose();
            ss = null;
#endif
        }
        #endregion

        // funkt so nicht
        //private static void Hatch(AcadRegion region, string layer, int red, int green, int blue)
        //{
        //    string patternName = "_SOLID";
        //    bool bAssociativity = false;

        //    AcadEntity RegionEntity = region as AcadEntity;

        //    AcadRegion regionCopy = (AcadRegion)region.Copy();

        //    Object[] items = (Object[])regionCopy.Explode();


        //    List<AcadEntity> polylines = new List<AcadEntity>();
        //    foreach (object o in items)
        //    {
        //        if (o is AcadPolyline)
        //        {
        //            polylines.Add((AcadEntity)o);
        //        }
        //        else if (o is AcadLWPolyline)
        //        {
        //            polylines.Add((AcadEntity)o);
        //        }
        //        else if (o is Acad3DPolyline)
        //        {
        //            polylines.Add((AcadEntity)o);
        //        }


        //    }

        //    //SetLayer(RegionEntity, layer);

        //    ////' Create the non associative Hatch object in model space
        //    //AcadApplication app = (AcadApplication)Application.AcadApplication;
        //    //AcadHatch hatchObj = app.ActiveDocument.ModelSpace.AddHatch(AcPatternType, patternName, bAssociativity, AcHatchObject);
        //    //AcadAcCmColor col1 = app.GetInterfaceObject(COLOROBJECTPROGID) as AcadAcCmColor;
        //    //AcadAcCmColor col2 = app.GetInterfaceObject(COLOROBJECTPROGID) as AcadAcCmColor;
        //    //col1.SetRGB(red, green, blue);
        //    //hatchObj.TrueColor = col1;
        //    //AcadEntity[] outerLoop = new AcadEntity[] { RegionEntity };
        //    //hatchObj.AppendOuterLoop(outerLoop);
        //    //if (RegionEntity != null) RegionEntity.Delete();

        //}



        #region IExtensionApplication

        public void Initialize()
        {
            _CalcAreaPalette = new CalcAreaPalette();
        }

        public void Terminate()
        {
            ;
        }
        #endregion

        #region Obsolete
        [Obsolete("Use AreaEngine")]
        private static void SelectFgAndRb()
        {
            log.Debug("Auswahl Flächengrenzen und Raumblöcke");
            _AcEd.Editor ed = _AcAp.Application.DocumentManager.MdiActiveDocument.Editor;
            _AcEd.PromptSelectionResult res = null;



            _AcEd.SelectionFilter filter = new _AcEd.SelectionFilter(new _AcDb.TypedValue[] {

                new _AcDb.TypedValue((int)_AcDb.DxfCode.Operator ,"<OR"),

                new _AcDb.TypedValue((int)_AcDb.DxfCode.Operator ,"<AND"),
                new _AcDb.TypedValue((int)_AcDb.DxfCode.Start ,"*POLYLINE"),
                new _AcDb.TypedValue((int)_AcDb.DxfCode.LayerName  , _FgLayer  ),
                new _AcDb.TypedValue((int)_AcDb.DxfCode.Operator ,"AND>"),

                new _AcDb.TypedValue((int)_AcDb.DxfCode.Operator ,"<AND"),
                new _AcDb.TypedValue((int)_AcDb.DxfCode.Start ,"INSERT"),
                new _AcDb.TypedValue((int)_AcDb.DxfCode.BlockName ,_RaumblockName  ),
                new _AcDb.TypedValue((int)_AcDb.DxfCode.Operator ,"AND>"),

                new _AcDb.TypedValue((int)_AcDb.DxfCode.Operator ,"OR>")
            });

            _AcEd.PromptSelectionOptions SelOpts = new _AcEd.PromptSelectionOptions();
            SelOpts.MessageForAdding = "Raumblöcke und Flächengrenzen wählen: ";

            res = ed.GetSelection(SelOpts, filter);
            if (res.Status != _AcEd.PromptStatus.OK)
            {
                log.Debug("Auswahl wurde abgebrochen.");
                return;
            }

#if BRX_APP
            _AcEd.SelectionSet ss = res.Value;
#else
            using (_AcEd.SelectionSet ss = res.Value)
#endif
            {
                _FlaechenGrenzen = new List<_AcDb.ObjectId>();
                _Raumbloecke = new List<_AcDb.ObjectId>();
                _AcDb.ObjectId[] idArray = ss.GetObjectIds();

                _AcDb.Database db = _AcAp.Application.DocumentManager.MdiActiveDocument.Database;
                _AcDb.TransactionManager tm = db.TransactionManager;
                _AcDb.Transaction myT = tm.StartTransaction();
                try
                {
                    for (int i = 0; i < idArray.Length; i++)
                    {
                        _AcDb.ObjectId oid = idArray[i];
                        using (_AcDb.DBObject dbobj = tm.GetObject(oid, _AcDb.OpenMode.ForRead, false) as _AcDb.Entity)
                        {
                            _AcDb.Entity ent = dbobj as _AcDb.Entity;
                            if (ent != null)
                            {
                                if (ent is _AcDb.BlockReference) _Raumbloecke.Add(oid);
                                else _FlaechenGrenzen.Add(oid);
                            }
                        }
                    }
                    myT.Commit();
                }
                finally
                {
                    myT.Dispose();
                }
            }

            log.DebugFormat(CultureInfo.CurrentCulture, "Auswahl: Raumblöcke {0}, Flächengrenzen {1}", _Raumbloecke.Count, _FlaechenGrenzen.Count);

        }


        #endregion
    }
}
