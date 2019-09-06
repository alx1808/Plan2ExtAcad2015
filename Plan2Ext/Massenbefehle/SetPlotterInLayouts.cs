using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
#if BRX_APP
using _AcAp = Bricscad.ApplicationServices;
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
// ReSharper disable IdentifierTypo
// ReSharper disable StringLiteralTypo
// ReSharper disable LocalizableElement
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
	// ReSharper disable once UnusedMember.Global
	public class SetPlotterInLayoutsClass
    {
        #region log4net Initialization
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(Convert.ToString((typeof(SetPlotterInLayoutsClass))));
        #endregion

        #region Member variables
        private static string _plotterName = string.Empty;
        private static _AcDb.Transaction _tr;
        private static _AcDb.Database _db;
        private static readonly List<string> DwgsWrongPlotter = new List<string>();
        private static int _nrPlotters;
        private static bool _noPlotterInModelSpace;
        private static string _noPlotterName = string.Empty;
		#endregion

        [_AcTrx.CommandMethod("Plan2SetPlotterInLayouts")]
        // ReSharper disable once UnusedMember.Global
        public static void Plan2SetPlotterInLayouts()
        {
            _AcAp.Document doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            _db = doc.Database;
            _AcEd.Editor ed = doc.Editor;
            _plotterName = string.Empty;
            DwgsWrongPlotter.Clear();
            _nrPlotters = 0;

            try
            {
                if (!GetPlotter()) return;
                if (!SetPlotterInLayouts())
                {
                    string msg = string.Format(CultureInfo.CurrentCulture, "Plotter '{0}' existiert nicht!", _plotterName);
                    ed.WriteMessage("\n" + msg);
                    System.Windows.Forms.MessageBox.Show(msg, "Plan2SetPlotterInLayouts");
                }
                else
                {
                    string resultMsg = string.Format(CultureInfo.CurrentCulture, "Anzahl gesetzter Plotter: {0}", _nrPlotters.ToString());
                    Log.Info(resultMsg);
                    System.Windows.Forms.MessageBox.Show(resultMsg, "Plan2SetPlotter");
                }
            }
            catch (Exception ex)
            {
                string msg = string.Format(CultureInfo.CurrentCulture, "Fehler in (Plan2SetPlotterInLayouts): {0}", ex.Message);
                ed.WriteMessage("\n" + msg);
                System.Windows.Forms.MessageBox.Show(ex.Message, "Plan2SetPlotterInLayouts");
            }
        }
		[_AcTrx.CommandMethod("Plan2SetPlotterInLayoutsBulk", _AcTrx.CommandFlags.Session)]
		// ReSharper disable once UnusedMember.Global
		public static void Plan2SetPlotterInLayoutsBulk()
        {
            _AcAp.Document doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            _db = doc.Database;
            _plotterName = string.Empty;
            DwgsWrongPlotter.Clear();
            List<string> saveNotPossible = new List<string>();
            try
            {
                Log.Info("----------------------------------------------------------------------------------");
                Log.Info("Plan2SetPlotterInLayoutsBulk");

                using (doc.LockDocument())
                {
                    if (!GetPlotter()) return;
                }

                string dirName = string.Empty;
                string[] dwgFileNames = null;
                if (!Globs.GetMultipleFileNames("AutoCAD-Zeichnung", "Dwg", "Verzeichnis mit Zeichnungen für das Setzen des Plotters", "Zeichnungen für das Setzen des Plotters", ref dwgFileNames, ref dirName))
                {
                    return;
                }
                if (!string.IsNullOrEmpty(dirName))
                {
                    Log.Info(string.Format(CultureInfo.CurrentCulture, "Plotter '{0}' wird gesetzt in Zeichnungen unter '{1}'.", _plotterName, dirName));
                }
                foreach (var fileName in dwgFileNames)
                {
                    Globs.SetReadOnlyAttribute(fileName, false);

                    bool ok;

                    Log.Info("----------------------------------------------------------------------------------");
                    Log.Info(string.Format(CultureInfo.CurrentCulture, "Öffne Zeichnung {0}", fileName));

                    _AcAp.Application.DocumentManager.Open(fileName, false);
                    doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
                    _db = doc.Database;

                    //Lock the new document
                    using (doc.LockDocument())
                    {
                        // main part
                        ok = SetPlotterInLayouts();
                    }

                    if (ok)
                    {
                        Log.Info(string.Format(CultureInfo.CurrentCulture, "Zeichnung speichern und schließen: {0}", fileName));
                        try
                        {
                            doc.CloseAndSave(fileName);
                        }
                        catch (Exception ex)
                        {
                            Log.Warn(string.Format(CultureInfo.CurrentCulture, "Zeichnung konnte nicht gespeichert werden! {0}. {1}", fileName, ex.Message));
                            saveNotPossible.Add(fileName);
                            doc.CloseAndDiscard();
                        }
                    }
                    else
                    {
                        DwgsWrongPlotter.Add(fileName);
                        Log.Info(string.Format(CultureInfo.CurrentCulture, "Zeichnung schließen ohne zu speichern da keine Plotters nicht geändert wurden: {0}", fileName));
                        doc.CloseAndDiscard();
                    }
                }

                if (saveNotPossible.Count > 0)
                {
                    var names = saveNotPossible.Select(System.IO.Path.GetFileName).ToArray();
                    var allNames = string.Join(", ", names);

                    System.Windows.Forms.MessageBox.Show(string.Format("Folgende Zeichnungen konnen nicht gespeichert werden: {0}", allNames), "Plan2SetPlotterInLayoutsBulk");
                }

                // ReSharper disable once PossibleNullReferenceException
                ShowResultMessage(dirName.ToUpperInvariant());
            }
            catch (Exception ex)
            {
                string msg = string.Format(CultureInfo.CurrentCulture, "{0}", ex.Message);
                System.Windows.Forms.MessageBox.Show(msg, "Plan2SetPlotterInLayoutsBulk");
            }
        }

        private static bool SetPlotterInLayouts()
        {
            var ok = true;
            using (_tr = _db.TransactionManager.StartTransaction())
            {
                //var layManager = _AcDb.LayoutManager.Current;
                var plotSetVal = _AcDb.PlotSettingsValidator.Current;

                var layouts = (_AcDb.DBDictionary)_tr.GetObject(_db.LayoutDictionaryId, _AcDb.OpenMode.ForRead);
                foreach (var layoutDe in layouts)
                {
                    //_AcDb.ObjectId layoutId = layManager.GetLayoutId(layManager.CurrentLayout);
                    var layoutId = layoutDe.Value;
                    var layoutObj = (_AcDb.Layout)_tr.GetObject(layoutId, _AcDb.OpenMode.ForWrite);
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
                            if (_noPlotterInModelSpace && layoutObj.LayoutName == "Model")
                            {
                                plotSetVal.SetPlotConfigurationName(ps, _noPlotterName, null);
                            }
                            else
                            {
                                plotSetVal.SetPlotConfigurationName(ps, _plotterName, null);
                            }
                            layoutObj.CopyFrom(ps);
                        }
                        _nrPlotters++;
                    }
                }
                _tr.Commit();
            }
            return ok;
        }

        private static bool PlotterExists(System.Collections.Specialized.StringCollection sheetList)
        {
            foreach (var plotterName in sheetList)
            {
                if (string.Compare(plotterName, _plotterName, StringComparison.OrdinalIgnoreCase) == 0) return true;
            }
            return false;
        }

        private static void ShowResultMessage(string dirNameU)
        {
            string errorsWithDwgsMsg = string.Empty;
            if (DwgsWrongPlotter.Count > 0)
            {
                var errDwgs = DwgsWrongPlotter.Select(x =>
                {
                    string newName;
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
            string resultMsg = string.Format(CultureInfo.CurrentCulture, "Anzahl gesetzter Plotters: {0}\n{1}", _nrPlotters.ToString(), errorsWithDwgsMsg);
            Log.Info(resultMsg);
            System.Windows.Forms.MessageBox.Show(resultMsg, "Plan2SetPlotter");
        }

        private static bool GetPlotter()
        {
            var devList = Plotter.GetDeviceList(_db);
            if (devList.Count == 0) throw new InvalidOperationException("Es wurden keine Plotter gefunden!");
            _noPlotterName = devList[0];
            if (!
                (
                (string.Compare(_noPlotterName, "Kein", StringComparison.OrdinalIgnoreCase) == 0) ||
                (string.Compare(_noPlotterName, "None", StringComparison.OrdinalIgnoreCase) == 0) ||
                (string.Compare(_noPlotterName, "No", StringComparison.OrdinalIgnoreCase) == 0)
                ))
            {
                _noPlotterName = "Kein";
            }

            using (var frm = new GetPlotterName(devList))
            {
                var res = _AcAp.Application.ShowModalDialog(frm);
                if (res == System.Windows.Forms.DialogResult.Cancel) return false;
                _plotterName = frm.CurrentPlotterName;
                _noPlotterInModelSpace = frm.NoPlotterInModelspace;
            }
            return true;
        }
    }
}
