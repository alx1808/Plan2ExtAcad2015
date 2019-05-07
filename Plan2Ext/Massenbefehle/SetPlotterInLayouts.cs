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
    public class SetPlotterInLayoutsClass
    {
        #region log4net Initialization
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Convert.ToString((typeof(SetPlotterInLayoutsClass))));
        #endregion

        #region Member variables
        private static string _PlotterName = string.Empty;
        private static _AcDb.Transaction _Tr = null;
        private static _AcDb.Database _Db = null;
        private static List<string> _DwgsWrongPlotter = new List<string>();
        private static int _NrPlotters = 0;
        private static bool _NoPlotterInModelSpace = false;
        private static string _NoPlotterName = string.Empty;
        #endregion

        [_AcTrx.CommandMethod("Plan2SetPlotterInLayouts")]
        public static void Plan2SetPlotterInLayouts()
        {
            _AcAp.Document doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            _Db = doc.Database;
            _AcEd.Editor ed = doc.Editor;
            _PlotterName = string.Empty;
            _DwgsWrongPlotter.Clear();
            _NrPlotters = 0;

            try
            {
                if (!GetPlotter(ed)) return;
                if (!SetPlotterInLayouts())
                {
                    string msg = string.Format(CultureInfo.CurrentCulture, "Plotter '{0}' existiert nicht!", _PlotterName);
                    ed.WriteMessage("\n" + msg);
                    System.Windows.Forms.MessageBox.Show(msg, "Plan2SetPlotterInLayouts");
                }
                else
                {
                    string resultMsg = string.Format(CultureInfo.CurrentCulture, "Anzahl gesetzter Plotter: {0}", _NrPlotters.ToString());
                    log.Info(resultMsg);
                    System.Windows.Forms.MessageBox.Show(resultMsg, "Plan2SetPlotter");
                }
            }
            catch (System.Exception ex)
            {
                string msg = string.Format(CultureInfo.CurrentCulture, "Fehler in (Plan2SetPlotterInLayouts): {0}", ex.Message);
                ed.WriteMessage("\n" + msg);
                System.Windows.Forms.MessageBox.Show(ex.Message, "Plan2SetPlotterInLayouts");
            }
        }

        [_AcTrx.CommandMethod("Plan2SetPlotterInLayoutsBulk", _AcTrx.CommandFlags.Session)]
        static public void Plan2SetPlotterInLayoutsBulk()
        {
            _AcAp.Document doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            _Db = doc.Database;
            _AcEd.Editor ed = doc.Editor;
            _PlotterName = string.Empty;
            _DwgsWrongPlotter.Clear();
            List<string> saveNotPossible = new List<string>();
            try
            {
                log.Info("----------------------------------------------------------------------------------");
                log.Info("Plan2SetPlotterInLayoutsBulk");

                using (DocumentLock acLckDoc = doc.LockDocument())
                {
                    if (!GetPlotter(ed)) return;
                }

                string dirName = string.Empty;
                string[] dwgFileNames = null;
                if (!Plan2Ext.Globs.GetMultipleFileNames("AutoCAD-Zeichnung", "Dwg", "Verzeichnis mit Zeichnungen für das Setzen des Plotters", "Zeichnungen für das Setzen des Plotters", ref dwgFileNames, ref dirName))
                {
                    return;
                }
                if (!string.IsNullOrEmpty(dirName))
                {
                    log.Info(string.Format(CultureInfo.CurrentCulture, "Plotter '{0}' wird gesetzt in Zeichnungen unter '{1}'.", _PlotterName, dirName));
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
                        ok = SetPlotterInLayouts();
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
                        _DwgsWrongPlotter.Add(fileName);
                        log.Info(string.Format(CultureInfo.CurrentCulture, "Zeichnung schließen ohne zu speichern da keine Plotters nicht geändert wurden: {0}", fileName));
                        doc.CloseAndDiscard();
                    }
                }

                if (saveNotPossible.Count > 0)
                {
                    var names = saveNotPossible.Select(x => System.IO.Path.GetFileName(x)).ToArray();
                    var allNames = string.Join(", ", names);

                    System.Windows.Forms.MessageBox.Show(string.Format("Folgende Zeichnungen konnen nicht gespeichert werden: {0}", allNames), "Plan2SetPlotterInLayoutsBulk");
                }

                ShowResultMessage(dirName.ToUpperInvariant());
            }
            catch (System.Exception ex)
            {
                string msg = string.Format(CultureInfo.CurrentCulture, "{0}", ex.Message);
                System.Windows.Forms.MessageBox.Show(msg, "Plan2SetPlotterInLayoutsBulk");
            }
        }

        private static bool SetPlotterInLayouts()
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
                    plotSetVal.RefreshLists(layoutObj);
                    System.Collections.Specialized.StringCollection deviceList = plotSetVal.GetPlotDeviceList();
                    bool plotterExists = PlotterExists(deviceList);
                    if (!plotterExists)
                    {
                        ok = false;
                        break;
                    }
                    else
                    {
                        using (var ps = new _AcDb.PlotSettings(layoutObj.ModelType))
                        {
                            ps.CopyFrom(layoutObj);
                            if (_NoPlotterInModelSpace && layoutObj.LayoutName == "Model")
                            {
                                plotSetVal.SetPlotConfigurationName(ps, _NoPlotterName, null);
                            }
                            else
                            {
                                plotSetVal.SetPlotConfigurationName(ps, _PlotterName, null);
                            }
                            layoutObj.CopyFrom(ps);
                        }
                        _NrPlotters++;
                    }
                }
                _Tr.Commit();
            }
            return ok;
        }

        private static bool PlotterExists(System.Collections.Specialized.StringCollection sheetList)
        {
            foreach (var PlotterName in sheetList)
            {
                if (string.Compare(PlotterName, _PlotterName, StringComparison.OrdinalIgnoreCase) == 0) return true;
            }
            return false;
        }

        private static void ShowResultMessage(string dirNameU)
        {
            string errorsWithDwgsMsg = string.Empty;
            if (_DwgsWrongPlotter.Count > 0)
            {
                var errDwgs = _DwgsWrongPlotter.Select(x =>
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
            string resultMsg = string.Format(CultureInfo.CurrentCulture, "Anzahl gesetzter Plotters: {0}\n{1}", _NrPlotters.ToString(), errorsWithDwgsMsg);
            log.Info(resultMsg);
            System.Windows.Forms.MessageBox.Show(resultMsg, "Plan2SetPlotter");
        }

        private static bool GetPlotter(_AcEd.Editor ed)
        {
            var devList = Plotter.GetDeviceList(_Db);
            if (devList.Count == 0) throw new InvalidOperationException("Es wurden keine Plotter gefunden!");
            _NoPlotterName = devList[0];
            if (!
                (
                (string.Compare(_NoPlotterName, "Kein", StringComparison.OrdinalIgnoreCase) == 0) ||
                (string.Compare(_NoPlotterName, "None", StringComparison.OrdinalIgnoreCase) == 0) ||
                (string.Compare(_NoPlotterName, "No", StringComparison.OrdinalIgnoreCase) == 0)
                ))
            {
                _NoPlotterName = "Kein";
            }

            using (var frm = new GetPlotterName(devList))
            {
                var res = _AcAp.Application.ShowModalDialog(frm);
                if (res == System.Windows.Forms.DialogResult.Cancel) return false;
                _PlotterName = frm.CurrentPlotterName;
                _NoPlotterInModelSpace = frm.NoPlotterInModelspace;
            }
            //var prompt = new _AcEd.PromptStringOptions("\nPlotter-Name, der allen Layouts zugewiesen werden soll: ");
            //prompt.AllowSpaces = true;
            //while (string.IsNullOrEmpty(_PlotterName))
            //{
            //    var res = ed.GetString(prompt);
            //    if (res.Status != _AcEd.PromptStatus.OK)
            //    {
            //        return false;
            //    }
            //    _PlotterName = res.StringResult;
            //}
            return true;
        }
    }
}
