using System.Globalization;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

// ReSharper disable StringLiteralTypo


namespace Plan2Ext.LayTrans
{
    // ReSharper disable once UnusedMember.Global
    public class Commands
    {
        #region log4net Initialization
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Convert.ToString((typeof(Commands))));
        #endregion

        [CommandMethod("Plan2LayTransExport")]
        // ReSharper disable once UnusedMember.Global
        public static void Plan2LayTransExport()
        {
            try
            {
                var doc = Application.DocumentManager.MdiActiveDocument;
                using (doc.LockDocument())
                {


                    var engine = new Engine();
                    var ok = engine.ExcelExport();
                    if (!ok)
                    {
                        Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler beim Export!"));
                    }
                    else
                    {
                        var msg = string.Format(CultureInfo.CurrentCulture, "Der Excel-Export wurde erfolgreich beendet.");
                        Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage(msg);
                        Log.Info(msg);
                    }
                }
            }
            catch (System.Exception ex)
            {
                Log.Error(ex.Message, ex);
                Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2LayTransExport aufgetreten! {0}", ex.Message));
            }
        }

        [CommandMethod("Plan2LayTransExportBulk", CommandFlags.Session)]
        // ReSharper disable once UnusedMember.Global
        public static void Plan2LayTransExportBulk()
        {
            try
            {
                Log.Info("----------------------------------------------------------------------------------");
                Log.Info("Plan2LayTransExportBulk");

                var doc = Application.DocumentManager.MdiActiveDocument;
                using (doc.LockDocument())
                {
                    var dirName = string.Empty;
                    string[] dwgFileNames = null;
                    if (!Globs.GetMultipleFileNames("AutoCAD-Zeichnung", "Dwg", "Verzeichnis mit Zeichnungen für den Layerexport", "Zeichnungen für Layerexport", ref dwgFileNames, ref dirName))
                    {
                        return;
                    }

                    var engine = new Engine();
                    var ok = engine.ExcelExport(dwgFileNames);
                    if (!ok)
                    {
                        Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler beim Plan2LayTransExportBulk!"));
                    }
                    else
                    {
                        var msg = string.Format(CultureInfo.CurrentCulture, "Der Excel-Export wurde erfolgreich beendet.");
                        Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage(msg);
                        Log.Info(msg);
                    }
                }
            }
            catch (System.Exception ex)
            {
                Log.Error(ex.Message, ex);
                Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2LayTransExportBulk aufgetreten! {0}", ex.Message));
            }
        }

        [CommandMethod("Plan2LayTransExportWithNrElements")]
        // ReSharper disable once UnusedMember.Global
        public static void Plan2LayTransExportWithNrElements()
        {
            try
            {
                Document doc = Application.DocumentManager.MdiActiveDocument;
                using (doc.LockDocument())
                {
                    var engine = new Engine();
                    var ok = engine.ExcelExportWithNrElements();
                    if (!ok)
                    {
                        Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler beim Export!"));
                    }
                    else
                    {
                        var msg = string.Format(CultureInfo.CurrentCulture, "Der Excel-Export wurde erfolgreich beendet.");
                        Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage(msg);
                        Log.Info(msg);
                    }
                }
            }
            catch (System.Exception ex)
            {
                Log.Error(ex.Message, ex);
                Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2LayTransExport aufgetreten! {0}", ex.Message));
            }
        }

        [CommandMethod("Plan2LayTrans")]
        // ReSharper disable once UnusedMember.Global
        public static void Plan2LayTrans()
        {
            try
            {
                var doc = Application.DocumentManager.MdiActiveDocument;
                using (doc.LockDocument())
                {

                    var ed = Application.DocumentManager.MdiActiveDocument.Editor;
                    // First let's use the editor method, GetFileNameForOpen()
                    var opts = new PromptOpenFileOptions("Excel-Datei für Layer-Infos")
                    {
                        Filter = "Excel (*.xlsx)|*.xlsx|Excel alt (*.xls)|*.xls"
                    };
                    var pr = ed.GetFileNameForOpen(opts);
                    if (pr.Status != PromptStatus.OK) return;
                    var fileName = pr.StringResult;

                    var engine = new Engine();
                    var ok = engine.LayTrans(fileName);
                    if (!ok)
                    {
                        Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in LayTrans!"));
                    }
                    else
                    {
                        var msg = string.Format(CultureInfo.CurrentCulture, "LayTrans wurde erfolgreich beendet.");
                        Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage(msg);
                        Log.Info(msg);
                    }
                }
            }
            catch (System.Exception ex)
            {
                Log.Error(ex.Message, ex);
                Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2LayTrans aufgetreten! {0}", ex.Message));
            }
        }
    }
}
