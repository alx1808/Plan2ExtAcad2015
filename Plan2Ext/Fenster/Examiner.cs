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
#elif ARX_APP
using _AcAp = Autodesk.AutoCAD.ApplicationServices;
using _AcBr = Autodesk.AutoCAD.BoundaryRepresentation;
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
using _AcLm = Autodesk.AutoCAD.LayerManager;
using System.Globalization;
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plan2Ext.Fenster
{
    internal class Examiner
    {
        #region log4net Initialization
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Convert.ToString((typeof(Examiner))));
        static Examiner()
        {
            if (log4net.LogManager.GetRepository(System.Reflection.Assembly.GetExecutingAssembly()).Configured == false)
            {
                log4net.Config.XmlConfigurator.ConfigureAndWatch(new System.IO.FileInfo(System.IO.Path.Combine(new System.IO.FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).DirectoryName, "_log4net.config")));
            }

        }
        #endregion

        #region Constants
        const string WEITE_ATTRIBUTE_NAME = "WEITE";

        const string WEITE_DIFF_LAYER = "_Abweichung_Weite";
        const string MISSING_FENLINE_LAYER = "_Keine_Fensterlinie";
        const string MISSING_STPARALINE_LAYER = "_Keine_SturzPara_Linie";
        const string NO_WEITE_ATTRIBUTE_LAYER = "_Kein_Weite_Attribut";
        const string NO_INT_IN_WEITE_ATTRIBUTE_LAYER = "_Keine_Zahl_In_Weite_Attribut";
        #endregion

        #region Quasi constants
        private double _SearchTol = 1.5;
        public double SearchTol
        {
            get { return _SearchTol; }
            set { _SearchTol = value; }
        }

        private double _OrthoToleranceRad = 5 * Math.PI / 180.0;
        public double OrthoToleranceRad
        {
            get { return _OrthoToleranceRad; }
            set { _OrthoToleranceRad = value; }
        }
        #endregion

        #region Member variables
        private static double _Weite_Eps = 0.01;
        public static double Weite_Eps
        {
            get { return _Weite_Eps; }
            set { _Weite_Eps = value; }
        }
        #endregion

        private class FensterWidthInfo
        {
            public FensterBlockInfo FbInfo { get; set; }
            public FensterLineInfo FlInfo { get; set; }
            public SturzParaLineInfo StParaInfo { get; set; }

            public bool CheckWidth()
            {
                bool ok = true;
                var doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
                var db = doc.Database;
                using (var tr = db.TransactionManager.StartTransaction())
                {
                    var fbRef = tr.GetObject(FbInfo.Oid, _AcDb.OpenMode.ForRead) as _AcDb.BlockReference;
                    var attribs = Plan2Ext.Globs.GetAttributes(fbRef);
                    string widthTxt;
                    if (attribs.TryGetValue(WEITE_ATTRIBUTE_NAME, out widthTxt))
                    {
                        int weite;
                        if (int.TryParse(widthTxt, out weite))
                        {
                            var stParaLine = tr.GetObject(StParaInfo.Oid, _AcDb.OpenMode.ForRead) as _AcDb.Line;
                            double dWeite = weite / 100.0;
                            double diffWeite = Math.Abs(stParaLine.Length - dWeite);
                            if (diffWeite > Weite_Eps)
                            {
                                Plan2Ext.Globs.InsertFehlerLines(new List<_AcGe.Point3d>() { fbRef.Position }, WEITE_DIFF_LAYER);
                                ok = false;
                            }
                        }
                        else
                        {
                            Plan2Ext.Globs.InsertFehlerLines(new List<_AcGe.Point3d>() { fbRef.Position }, NO_INT_IN_WEITE_ATTRIBUTE_LAYER);
                            ok = false;
                        }
                    }
                    else
                    {
                        Plan2Ext.Globs.InsertFehlerLines(new List<_AcGe.Point3d>() { fbRef.Position }, NO_WEITE_ATTRIBUTE_LAYER);
                        ok = false;
                    }

                    tr.Commit();
                }
                return ok;
            }
        }

        public int CheckWindowWidth()
        {
            DeleteFehlerSymbols();

            GetFensterBlockNamesFromConfig();

            var missingSpLinesPos = new List<_AcGe.Point3d>();
            var missingFbLinesPos = new List<_AcGe.Point3d>();
            var fwInfos = new List<FensterWidthInfo>();

            GetFensterWidthInfos(missingSpLinesPos, missingFbLinesPos, fwInfos);

            if (missingSpLinesPos.Count > 0)
            {
                Plan2Ext.Globs.InsertFehlerLines(missingSpLinesPos, MISSING_STPARALINE_LAYER);
            }
            if (missingFbLinesPos.Count > 0)
            {
                Plan2Ext.Globs.InsertFehlerLines(missingFbLinesPos, MISSING_FENLINE_LAYER);
            }
            var fbsWithErrors = fwInfos.Where(x => !x.CheckWidth()).ToList();
            return fbsWithErrors.Count;
        }


        private List<string> _ConfiguredFensterBlockVarNames = new List<string>()
        {
            "alx_V:ino_fenster_Block_Oben",
            "alx_V:ino_fenster_Block_Unten",
            "alx_V:ino_fenster_Block_Links",
            "alx_V:ino_fenster_Block_Rechts",
        };
        private void GetFensterBlockNamesFromConfig()
        {

            var fbNames = new List<string>();
            string val;
            foreach (var fbVarName in _ConfiguredFensterBlockVarNames)
            {
                if (GetFromConfig(out val, fbVarName))
                {
                    fbNames.Add(val);
                }
                else
                {
                    log.WarnFormat(CultureInfo.CurrentCulture, "Fensterblock '{0}' ist nicht konfiguriert!", fbVarName);
                }
            }
            if (fbNames.Count > 0)
            {
                FensterBlockInfo.FensterBlockNames = fbNames;
            }
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

        private void GetFensterWidthInfos(List<_AcGe.Point3d> missingSpLinesPos, List<_AcGe.Point3d> missingFbLinesPos, List<FensterWidthInfo> fwInfos)
        {
            var searchers = new List<ObjectSearcher> {
                new Plan2Ext.Fenster.FensterBlockInfo(),
                new Plan2Ext.Fenster.FensterLineInfo(),
                new Plan2Ext.Fenster.SturzParaLineInfo()
            };
            var foundObjects = Plan2Ext.Searcher.Search(searchers);
            var fenBlocks = foundObjects.Select(x => x as Plan2Ext.Fenster.FensterBlockInfo).Where(x => x != null).ToList();
            var fenBLines = foundObjects.Select(x => x as Plan2Ext.Fenster.FensterLineInfo).Where(x => x != null).ToList();
            var sturzParaLines = foundObjects.Select(x => x as Plan2Ext.Fenster.SturzParaLineInfo).Where(x => x != null).ToList();

            foreach (var fenBlock in fenBlocks)
            {
                var p = fenBlock.InsertPoint;

                var orderedDistsToFenLines = fenBLines.Select(x => new { fbLine = x, dist = Dist2d(p, x.MiddelPoint) }).Where(x => x.dist < SearchTol).OrderBy(x => x.dist);
                var fenLineInfo = orderedDistsToFenLines.FirstOrDefault(x => true);
                Plan2Ext.Fenster.FensterLineInfo fbLine = null;
                Plan2Ext.Fenster.SturzParaLineInfo spLine = null;
                if (fenLineInfo != null)
                {
                    fbLine = fenLineInfo.fbLine;
                    fenBLines.Remove(fbLine);

                    var pm = fbLine.MiddelPoint;

                    var orderedDistsFbToSPLines = sturzParaLines.Select(x => new { spLine = x, dist = Dist2d(pm, x.MiddelPoint) }).Where(x => x.dist < SearchTol).OrderBy(x => x.dist);
                    var orthoSpLines = orderedDistsFbToSPLines.Where(x => IsOrthogonal2D(x.spLine, fbLine));
                    var spLineInfo = orthoSpLines.FirstOrDefault(x => true);
                    if (spLineInfo != null)
                    {
                        spLine = spLineInfo.spLine;
                        sturzParaLines.Remove(spLine);
                    }
                    else
                    {
                        missingSpLinesPos.Add(p);
                    }
                }
                else
                {
                    missingFbLinesPos.Add(p);
                }

                if (spLine != null && fbLine != null)
                {
                    fwInfos.Add(new FensterWidthInfo() { FbInfo = fenBlock, FlInfo = fbLine, StParaInfo = spLine });
                }
            }
        }

        private static void DeleteFehlerSymbols()
        {
            var layerNames = new List<string>
            {
                WEITE_DIFF_LAYER,
                MISSING_FENLINE_LAYER,
                MISSING_STPARALINE_LAYER,
                NO_WEITE_ATTRIBUTE_LAYER,
                NO_INT_IN_WEITE_ATTRIBUTE_LAYER,
            };
            foreach (var layerName in layerNames)
            {
                Plan2Ext.Globs.DeleteFehlerLines(layerName);
            }
        }

        private bool IsOrthogonal2D(SturzParaLineInfo st, FensterLineInfo fl)
        {

            _AcGe.Vector2d v1 = new _AcGe.Vector2d(st.EndPoint.X - st.StartPoint.X, st.EndPoint.Y - st.StartPoint.Y);
            _AcGe.Vector2d v2 = new _AcGe.Vector2d(fl.EndPoint.X - fl.StartPoint.X, fl.EndPoint.Y - fl.StartPoint.Y);
            var dotProduct = v1.DotProduct(v2);

            var lenProd = v1.Length * v2.Length;
            if (lenProd == 0.0) return false;

            var cosy = dotProduct / lenProd;
            var ang = Math.Acos(cosy);
            ang = Math.Min(ang, (2 * Math.PI) - ang);

            var deviation = Math.Abs((Math.PI / 2.0) - ang);
            return (deviation < _OrthoToleranceRad);
        }

        private static double Dist2d(_AcGe.Point3d p1, _AcGe.Point3d p2)
        {
            var distX = p2.X - p1.X;
            var distY = p2.Y - p1.Y;
            return Math.Sqrt((distX * distX) + (distY * distY));
        }
    }
}
