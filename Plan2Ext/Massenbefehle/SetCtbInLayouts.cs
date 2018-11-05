using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Text.RegularExpressions;


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
using Autodesk.AutoCAD.ApplicationServices;
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
#endif

namespace Plan2Ext.Massenbefehle
{
    public class SetCtbInLayoutsClass
    {
        #region log4net Initialization
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Convert.ToString((typeof(SetCtbInLayoutsClass))));
        #endregion

        #region Member variables
        private static string _CtbName = string.Empty;
        private static _AcDb.Transaction _Tr = null;
        private static _AcDb.Database _Db = null;
        private static List<string> _DwgsWrongCtb = new List<string>();
        private static int _NrCtbs = 0;
        private static bool _NoCtbInModelSpace = false;
        private static string _NoCtbName = string.Empty;
        #endregion

        [_AcTrx.CommandMethod("Plan2SetCtbInLayouts")]
        public static void Plan2SetCtbInLayouts()
        {
            _AcAp.Document doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            _Db = doc.Database;
            _AcEd.Editor ed = doc.Editor;
            _CtbName = string.Empty;
            _DwgsWrongCtb.Clear();
            _NrCtbs = 0;

            try
            {
                if (!GetCtbName(ed)) return;
                if (!SetCtbInLayouts())
                {
                    string msg = string.Format(CultureInfo.CurrentCulture, "Ctb '{0}' existiert nicht!", _CtbName);
                    ed.WriteMessage("\n" + msg);
                    System.Windows.Forms.MessageBox.Show(msg, "Plan2SetCtbInLayouts");
                }
                else
                {
                    string resultMsg = string.Format(CultureInfo.CurrentCulture, "Anzahl gesetzter CTBs: {0}", _NrCtbs.ToString());
                    log.Info(resultMsg);
                    System.Windows.Forms.MessageBox.Show(resultMsg, "Plan2SetCtb");
                }
            }
            catch (System.Exception ex)
            {
                string msg = string.Format(CultureInfo.CurrentCulture, "Fehler in (Plan2SetCtbInLayouts): {0}", ex.Message);
                ed.WriteMessage("\n" + msg);
                System.Windows.Forms.MessageBox.Show(ex.Message, "Plan2SetCtbInLayouts");
            }
        }

        [_AcTrx.CommandMethod("Plan2SetCtbInLayoutsBulk", _AcTrx.CommandFlags.Session)]
        static public void Plan2SetCtbInLayoutsBulk()
        {
            _AcAp.Document doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            _Db = doc.Database;
            _AcEd.Editor ed = doc.Editor;
            _CtbName = string.Empty;
            _DwgsWrongCtb.Clear();
            _NrCtbs = 0;
            List<string> saveNotPossible = new List<string>();
            try
            {
                log.Info("----------------------------------------------------------------------------------");
                log.Info("Plan2SetCtbInLayoutsBulk");

                using (DocumentLock acLckDoc = doc.LockDocument())
                {
                    if (!GetCtbName(ed)) return;
                }

                string dirName = string.Empty;
                string[] dwgFileNames = null;
                if (!Plan2Ext.Globs.GetMultipleFileNames("AutoCAD-Zeichnung", "Dwg", "Verzeichnis mit Zeichnungen für die Umbenennung der Layouts", "Zeichnungen für die Umbenennung der Layouts", ref dwgFileNames, ref dirName))
                {
                    return;
                }
                if (!string.IsNullOrEmpty(dirName))
                {
                    log.Info(string.Format(CultureInfo.CurrentCulture, "CTB '{0}' wird gesetzt in Zeichnungen unter '{1}'.", _CtbName, dirName));
                }
                foreach (var fileName in dwgFileNames)
                {
                    Globs.SetReadOnlyAttribute(fileName, false);

                    bool ok = false;

                    log.Info("----------------------------------------------------------------------------------");
                    log.Info(string.Format(CultureInfo.CurrentCulture, "Öffne Zeichnung {0}", fileName));

                    _AcAp.Application.DocumentManager.Open(fileName, false);
                    doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
                    _Db = doc.Database;

                    //Lock the new document
                    using (DocumentLock acLckDoc = doc.LockDocument())
                    {
                        // main part
                        ok = SetCtbInLayouts();
                    }

                    if (ok)
                    {
                        log.Info(string.Format(CultureInfo.CurrentCulture, "Zeichnung speichern und schließen: {0}", fileName));
                        try
                        {
                            doc.CloseAndSave(fileName);
                        }
                        catch (System.Exception ex)
                        {
                            log.Warn(string.Format(CultureInfo.CurrentCulture, "Zeichnung konnte nicht gespeichert werden! {0}. {1}", fileName, ex.Message));
                            saveNotPossible.Add(fileName);
                            doc.CloseAndDiscard();
                        }
                    }
                    else
                    {
                        _DwgsWrongCtb.Add(fileName);
                        log.Info(string.Format(CultureInfo.CurrentCulture, "Zeichnung schließen ohne zu speichern da keine CTBs nicht geändert wurden: {0}", fileName));
                        doc.CloseAndDiscard();
                    }
                }

                if (saveNotPossible.Count > 0)
                {
                    var names = saveNotPossible.Select(x => System.IO.Path.GetFileName(x)).ToArray();
                    var allNames = string.Join(", ", names);

                    System.Windows.Forms.MessageBox.Show(string.Format("Folgende Zeichnungen konnen nicht gespeichert werden: {0}", allNames), "Plan2SetCtbInLayoutsBulk");
                }

                ShowResultMessage(dirName.ToUpperInvariant());
            }
            catch (System.Exception ex)
            {
                string msg = string.Format(CultureInfo.CurrentCulture, "{0}", ex.Message);
                System.Windows.Forms.MessageBox.Show(msg, "Plan2SetCtbInLayoutsBulk");
            }
        }
        private static bool SetCtbInLayouts()
        {
            var ok = true;
            using (_Tr = _Db.TransactionManager.StartTransaction())
            {
                var layManager = _AcDb.LayoutManager.Current;
                var plotSetVal = _AcDb.PlotSettingsValidator.Current;

                //_AcDb.DBDictionary layoutsEx = acTransEx.GetObject(acExDb.LayoutDictionaryId, _AcDb.OpenMode.ForRead) as _AcDb.DBDictionary;
                var layouts = _Tr.GetObject(_Db.LayoutDictionaryId, _AcDb.OpenMode.ForRead) as _AcDb.DBDictionary;
                foreach (var layoutDe in layouts)
                {
                    //_AcDb.ObjectId layoutId = layManager.GetLayoutId(layManager.CurrentLayout);
                    var layoutId = layoutDe.Value;
                    var layoutObj = (_AcDb.Layout)_Tr.GetObject(layoutId, _AcDb.OpenMode.ForWrite);
                    //ed.WriteMessage("Style sheet of current layout: " + layoutObj.CurrentStyleSheet + "\n");
                    //plotSetVal.RefreshLists(layoutObj); // ändert zb auch paperunits
                    System.Collections.Specialized.StringCollection sheetList = plotSetVal.GetPlotStyleSheetList();
                    bool ctbExists = CtbExists(sheetList);
                    if (!ctbExists)
                    {
                        ok = false;
                        break;
                    }
                    else
                    {
                        if (_NoCtbInModelSpace && layoutObj.LayoutName == "Model")
                        {
                            plotSetVal.SetCurrentStyleSheet(layoutObj, _NoCtbName);
                        }
                        else
                        {
                            plotSetVal.SetCurrentStyleSheet(layoutObj, _CtbName);
                            //var ps = (_AcDb.PlotSettings)layoutObj;
                            //if (ps.PlotPaperUnits != _AcDb.PlotPaperUnit.Millimeters) -> sollt nicht notwendig sein
                            //{ 
                            //    plotSetVal.SetPlotPaperUnits(ps, _AcDb.PlotPaperUnit.Millimeters);
                            //}
                        }
                        _NrCtbs++;
                    }
                    //ed.WriteMessage("The list of available plot style sheets\n");
                    //foreach (String str in sheetList)
                    //{
                    //    ed.WriteMessage(str + "\n");
                    //    if (str.ToLower().Equals("acad.ctb"))
                    //    {
                    //        //find out if drawing is using ctb
                    //        System.Object test = Application.GetSystemVariable("PSTYLEMODE");
                    //        if (test.ToString().Equals("1"))
                    //        {
                    //            // drawing is using ctb so go ahead and
                    //            //assign acad.ctb to the layout
                    //            ed.WriteMessage("\nThe plot style sheet is" + " being set to acad.ctb\n\n");
                    //            plotSetVal.SetCurrentStyleSheet(layoutObj, str);
                    //        }
                    //        else
                    //        {
                    //            ed.WriteMessage("\nUnable to set plot style in" + " this example, drawing using stb\n\n");
                    //        }
                    //    }
                    //}
                }
                _Tr.Commit();
            }
            return ok;
        }

        private static bool CtbExists(System.Collections.Specialized.StringCollection sheetList)
        {
            if (_CtbName == "") return true;
            foreach (var ctbName in sheetList)
            {
                if (string.Compare(ctbName, _CtbName, StringComparison.OrdinalIgnoreCase) == 0) return true;
            }
            return false;
        }

        private static void ShowResultMessage(string dirNameU)
        {
            string errorsWithDwgsMsg = string.Empty;
            if (_DwgsWrongCtb.Count > 0)
            {
                var errDwgs = _DwgsWrongCtb.Select(x =>
                {
                    string newName = string.Empty;
                    if (x.StartsWith(dirNameU))
                    {
                        newName = "." + x.Remove(0, dirNameU.Length);
                    }
                    else
                    {
                        newName = x;
                    }
                    return newName;
                });
                errorsWithDwgsMsg = string.Join("\n", errDwgs.ToArray());
                if (!string.IsNullOrEmpty(errorsWithDwgsMsg))
                    errorsWithDwgsMsg = string.Format(CultureInfo.CurrentCulture, "\nFehler in folgenden Dwgs aufgetreten (Info in Logdatei):\n{0}", errorsWithDwgsMsg);
            }
            string resultMsg = string.Format(CultureInfo.CurrentCulture, "Anzahl gesetzter CTBs: {0}\n{1}", _NrCtbs.ToString(), errorsWithDwgsMsg);
            log.Info(resultMsg);
            System.Windows.Forms.MessageBox.Show(resultMsg, "Plan2SetCtb");
        }

        private static bool GetCtbName(_AcEd.Editor ed)
        {
            var devList = GetCtbList();
            _NoCtbName = "";

            using (var frm = new GetPlotterName(devList))
            {
                frm.Text = "CTB";
                frm.chkModelToNone.Text = "Kein CTB im Modellbereich";
                var res = _AcAp.Application.ShowModalDialog(frm);
                if (res == System.Windows.Forms.DialogResult.Cancel) return false;
                _CtbName = frm.CurrentPlotterName;
                _NoCtbInModelSpace = frm.NoPlotterInModelspace;
            }

            if (string.Compare(_CtbName, "Keine", StringComparison.OrdinalIgnoreCase) == 0)
            {
                _CtbName = "";
            }
            //var prompt = new _AcEd.PromptStringOptions("\nCTB-Name, der allen Layouts zugewiesen werden soll: ");
            //prompt.AllowSpaces = true;
            //while (string.IsNullOrEmpty(_CtbName))
            //{
            //    var res = ed.GetString(prompt);
            //    if (res.Status != _AcEd.PromptStatus.OK)
            //    {
            //        return false;
            //    }
            //    _CtbName = res.StringResult;
            //    if (!_CtbName.EndsWith(".ctb", StringComparison.OrdinalIgnoreCase)) _CtbName += ".ctb";
            //}
            return true;
        }

        private static List<string> GetCtbList()
        {
            var ctbs = new List<string>() { "Keine" };
            using (var trans = _Db.TransactionManager.StartTransaction())
            {
                var plotSetVal = _AcDb.PlotSettingsValidator.Current;
                var layouts = trans.GetObject(_Db.LayoutDictionaryId, _AcDb.OpenMode.ForRead) as _AcDb.DBDictionary;
                foreach (var layoutDe in layouts)
                {
                    var layoutId = layoutDe.Value;
                    var layoutObj = (_AcDb.Layout)trans.GetObject(layoutId, _AcDb.OpenMode.ForRead);
                    if (layoutObj.LayoutName == "Model")
                    {
                        //layoutObj.UpgradeOpen();
                        //plotSetVal.RefreshLists(layoutObj);
                        //layoutObj.DowngradeOpen();
                        System.Collections.Specialized.StringCollection ctbList = plotSetVal.GetPlotStyleSheetList();
                        foreach (var dev in ctbList)
                        {
                            if (dev.EndsWith(".ctb", StringComparison.OrdinalIgnoreCase))
                            {
                                ctbs.Add(dev);
                            }
                        }
                        break;
                    }
                }
                trans.Commit();
            }

            return ctbs;
        }
    }
}
