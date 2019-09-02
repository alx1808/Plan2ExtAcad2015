using System.Globalization;
using System.Linq;


#if BRX_APP
using Bricscad.ApplicationServices;
using Teigha.DatabaseServices;
using Bricscad.EditorInput;
using Teigha.Runtime;
#elif ARX_APP
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
#endif



// ReSharper disable CommentTypo

// ReSharper disable StringLiteralTypo


namespace Plan2Ext.LayTrans
{
    // ReSharper disable once UnusedMember.Global
    public class Commands
    {
        #region log4net Initialization
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Convert.ToString((typeof(Commands))));
		#endregion

#if ARX_APP
        [CommandMethod("Plan2LayTransExport")]
#endif
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

#if ARX_APP
        [CommandMethod("Plan2LayTransExportBulk", CommandFlags.Session)]
#endif
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

#if ARX_APP
        [CommandMethod("Plan2LayTransExportWithNrElements")]
#endif
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

#if ARX_APP
        [CommandMethod("Plan2LayTrans")]
#endif
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

		/// <summary>
		/// Laytrans with commandline options instead of fileselection via dialog
		/// </summary>
#if ARX_APP
        [CommandMethod("Plan2LayTrans2")]
#endif
        // ReSharper disable once UnusedMember.Global
        public static void Plan2LayTrans2()
        {
            try
            {
                var doc = Application.DocumentManager.MdiActiveDocument;
                using (doc.LockDocument())
                {

                    var fileNames = new[] { "CARLO_AUF_PLAN2", "NORM_AUF_PLAN2", "PLAN2_AUF_CARLO", "PLAN2_AUF_NORM" };
                    var keywords = new[] {"Carloplan2", "Normplan2", "Plan2carlo", "pLan2norm"};

                    var keyword = Globs.AskKeywordFromUser("Layerkonfiguration", keywords);
                    if (keyword== null) return;

                    var fn = fileNames[keywords.ToList().IndexOf(keyword)];
                    fn += ".xlsx";
                    string fileName;
                    try
                    {
                        fileName = HostApplicationServices.Current.FindFile(fn, doc.Database, FindFileHint.Default);
                    }
                    catch (Exception)
                    {
                        Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Datei {0} wurde nicht gefunden!", keyword));
                        return;
                    }

                    var engine = new Engine();
                    var ok = engine.LayTrans(fileName);
                    if (!ok)
                    {
                        Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in LayTrans2!"));
                    }
                    else
                    {
                        var msg = string.Format(CultureInfo.CurrentCulture, "LayTrans2 für {0} wurde erfolgreich beendet.", fn);
                        Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage(msg);
                        Log.Info(msg);
                    }
                }
            }
            catch (System.Exception ex)
            {
                Log.Error(ex.Message, ex);
                Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2LayTrans2 aufgetreten! {0}", ex.Message));
            }
        }
    }
}
