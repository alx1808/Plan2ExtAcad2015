using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

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
using _AcIntCom = Autodesk.AutoCAD.Interop.Common;
using _AcInt = Autodesk.AutoCAD.Interop;
#endif

namespace Plan2Ext.LayTrans
{
    public class Commands
    {
        #region log4net Initialization
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Convert.ToString((typeof(Commands))));
        static Commands()
        {
            if (log4net.LogManager.GetRepository(System.Reflection.Assembly.GetExecutingAssembly()).Configured == false)
            {
                log4net.Config.XmlConfigurator.ConfigureAndWatch(new System.IO.FileInfo(System.IO.Path.Combine(new System.IO.FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).DirectoryName, "_log4net.config")));
            }

        }
        #endregion

        [_AcTrx.CommandMethod("Plan2LayTransExport")]
        static public void Plan2LayTransExport()
        {
            try
            {
#if BRX_APP
                return;
#else

                _AcAp.Document doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
                using (_AcAp.DocumentLock m_doclock = doc.LockDocument())
                {

                    //_AcWnd.SaveFileDialog sfd = new _AcWnd.SaveFileDialog("Excel-Datei", "", "xlsx","LayTransSave", _AcWnd.SaveFileDialog.SaveFileDialogFlags.AllowAnyExtension);
                    //System.Windows.Forms.DialogResult dr = sfd.ShowDialog();
                    //if (dr != System.Windows.Forms.DialogResult.OK)  return;

                    Engine engine = new Engine();
                    var ok = engine.ExcelExport();
                    if (!ok)
                    {
                        _AcAp.Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler beim Export!"));
                    }
                    else
                    {
                        string msg = string.Format(CultureInfo.CurrentCulture, "Der Excel-Export wurde erfolgreich beendet.");
                        _AcAp.Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage(msg);
                        log.Info(msg);
                    }
                }
#endif

            }
            catch (System.Exception ex)
            {
                log.Error(ex.Message, ex);
                _AcAp.Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2LayTransExport aufgetreten! {0}", ex.Message));
            }
        }

        [_AcTrx.CommandMethod("Plan2LayTransExportBulk", _AcTrx.CommandFlags.Session)]
        static public void Plan2LayTransExportBulk()
        {
            try
            {
#if BRX_APP
                return;
#else
                log.Info("----------------------------------------------------------------------------------");
                log.Info("Plan2LayTransExportBulk");

                _AcAp.Document doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
                using (_AcAp.DocumentLock m_doclock = doc.LockDocument())
                {
                    string dirName = string.Empty;
                    string[] dwgFileNames = null;
                    using (var folderBrowser = new System.Windows.Forms.FolderBrowserDialog())
                    {
                        folderBrowser.Description = "Verzeichnis mit Zeichnungen für den Layerexport";
                        folderBrowser.RootFolder = Environment.SpecialFolder.MyComputer;
                        if (folderBrowser.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                        {
                            var openFileDialog = new System.Windows.Forms.OpenFileDialog();
                            openFileDialog.CheckFileExists = true;
                            openFileDialog.CheckPathExists = true;
                            openFileDialog.Multiselect = true;
                            openFileDialog.Title = "Zeichnungen für Layerexport";
                            openFileDialog.Filter = "Dwg" + "|*." + "Dwg";
                            if (openFileDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                            {
                                return;
                            }
                            else
                            {
                                dwgFileNames = openFileDialog.FileNames;
                            }
                        }
                        else
                        {
                            dirName = folderBrowser.SelectedPath;
                            dwgFileNames = System.IO.Directory.GetFiles(dirName, "*.dwg", System.IO.SearchOption.AllDirectories);
                        }
                    }
                    //dirName = @"D:\Plan2\Data\Plan2PlotToDwf";

                    Engine engine = new Engine();
                    var ok = engine.ExcelExport(dwgFileNames);
                    if (!ok)
                    {
                        _AcAp.Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler beim Plan2LayTransExportBulk!"));
                    }
                    else
                    {
                        string msg = string.Format(CultureInfo.CurrentCulture, "Der Excel-Export wurde erfolgreich beendet.");
                        _AcAp.Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage(msg);
                        log.Info(msg);
                    }
                }
#endif
            }
            catch (System.Exception ex)
            {
                log.Error(ex.Message, ex);
                _AcAp.Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2LayTransExportBulk aufgetreten! {0}", ex.Message));
            }
        }

        [_AcTrx.CommandMethod("Plan2LayTrans")]
        static public void Plan2LayTrans()
        {
            try
            {
#if BRX_APP
                return;
#else

                _AcAp.Document doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
                using (_AcAp.DocumentLock m_doclock = doc.LockDocument())
                {

                    _AcEd.Editor ed = _AcAp.Application.DocumentManager.MdiActiveDocument.Editor;
                    // First let's use the editor method, GetFileNameForOpen()
                    _AcEd.PromptOpenFileOptions opts = new _AcEd.PromptOpenFileOptions("Excel-Datei für Layer-Infos");
                    opts.Filter = "Excel (*.xlsx)|*.xlsx|Excel alt (*.xls)|*.xls";
                    _AcEd.PromptFileNameResult pr = ed.GetFileNameForOpen(opts);
                    if (pr.Status != _AcEd.PromptStatus.OK) return;

                    //_AcWnd.OpenFileDialog ofd = new _AcWnd.OpenFileDialog("Excel-Datei", "", "xlsx", "LayTrans", _AcWnd.OpenFileDialog.OpenFileDialogFlags.AllowAnyExtension);
                    //System.Windows.Forms.DialogResult dr = ofd.ShowDialog();
                    //if (dr != System.Windows.Forms.DialogResult.OK)  return;

                    string fileName = pr.StringResult;

                    Engine engine = new Engine();
                    var ok = engine.LayTrans(fileName);
                    if (!ok)
                    {
                        _AcAp.Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in LayTrans!"));
                    }
                    else
                    {
                        string msg = string.Format(CultureInfo.CurrentCulture, "LayTrans wurde erfolgreich beendet.");
                        _AcAp.Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage(msg);
                        log.Info(msg);
                    }
                }
#endif
            }
            catch (System.Exception ex)
            {
                log.Error(ex.Message, ex);
                _AcAp.Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2LayTrans aufgetreten! {0}", ex.Message));
            }
        }
    }
}
