// ReSharper disable CommentTypo
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
using _AcIntCom = BricscadDb;
using _AcInt = BricscadApp;
#elif ARX_APP
using Autodesk.AutoCAD.ApplicationServices;
//using _AcBr = Autodesk.AutoCAD.BoundaryRepresentation;
//using _AcCm = Autodesk.AutoCAD.Colors;
using _AcDb = Autodesk.AutoCAD.DatabaseServices;
using _AcEd = Autodesk.AutoCAD.EditorInput;
//using _AcGe = Autodesk.AutoCAD.Geometry;
//using _AcGi = Autodesk.AutoCAD.GraphicsInterface;
//using _AcGs = Autodesk.AutoCAD.GraphicsSystem;
//using _AcPl = Autodesk.AutoCAD.PlottingServices;
using _AcBrx = Autodesk.AutoCAD.Runtime;
using _AcTrx = Autodesk.AutoCAD.Runtime;
//using _AcWnd = Autodesk.AutoCAD.Windows;
//using _AcIntCom = Autodesk.AutoCAD.Interop.Common;
//using _AcInt = Autodesk.AutoCAD.Interop;
#endif

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;
// ReSharper disable LocalizableElement
// ReSharper disable UnusedMember.Global
// ReSharper disable IdentifierTypo

// ReSharper disable StringLiteralTypo
// ReSharper disable AccessToStaticMemberViaDerivedType

namespace Plan2Ext.XrefToExcel
{
    public class Commands
    {
        #region log4net Initialization
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(Convert.ToString((typeof(Commands))));
        #endregion


        [_AcTrx.CommandMethod("Plan2XrefToExcel", _AcTrx.CommandFlags.Session)]
        // ReSharper disable once UnusedMember.Global
        public static void Plan2XrefToExcel()
        {
            try
            {
                string dirName = string.Empty;
                string[] dwgFileNames = null;
                Globs.GetMultipleFileNames("AutoCAD-Zeichnung", "Dwg",
                    "Verzeichnis mit Zeichnungen für XRef-Export", "Zeichnungen für Plan2XrefToExcel",
                    ref dwgFileNames, ref dirName);

                Engine engine = new Engine();
                var xrefInfos = new List<Engine.XrefInfo>();
                if (dwgFileNames != null)
                {
                    foreach (var fileName in dwgFileNames)
                    {
                        Application.DocumentManager.Open(fileName, false);
                        Document doc = Application.DocumentManager.MdiActiveDocument;
                        // ReSharper disable once UnusedVariable
                        using (DocumentLock doclock = doc.LockDocument())
                        {
                            engine.GetXrefInfos(xrefInfos);
                        }
                        doc.CloseAndDiscard();
                    }
                }
                else
                {
                    // current document
                    Document doc = Application.DocumentManager.MdiActiveDocument;
                    // ReSharper disable once UnusedVariable
                    using (DocumentLock doclock = doc.LockDocument())
                    {
                        engine.GetXrefInfos(xrefInfos);
                    }
                }
                engine.ExcelExport(xrefInfos);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2XrefToExcel aufgetreten! {0}", ex.Message));
            }
        }

        [_AcTrx.CommandMethod("Plan2XrefFromExcel", _AcTrx.CommandFlags.Session)]
        // ReSharper disable once UnusedMember.Global
        public static void Plan2XrefFromExcel()
        {
            try
            {
                string dirName = string.Empty;
                string[] dwgFileNames = null;
                Globs.GetMultipleFileNames("AutoCAD-Zeichnung", "Dwg",
                    "Verzeichnis mit Zeichnungen für den XRef-Import", "Zeichnungen für Plan2XrefFromExcel",
                    ref dwgFileNames, ref dirName);

                _AcEd.Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
                // First let's use the editor method, GetFileNameForOpen()
                _AcEd.PromptOpenFileOptions opts = new _AcEd.PromptOpenFileOptions("Excel-Datei für XRef-Import")
                {
                    Filter = "Excel (*.xlsx)|*.xlsx|Excel alt (*.xls)|*.xls"
                };
                _AcEd.PromptFileNameResult pr = ed.GetFileNameForOpen(opts);
                if (pr.Status != _AcEd.PromptStatus.OK) return;

                string excelFileName = pr.StringResult;
                Engine engine = new Engine(excelFileName);

                if (engine.Errors.Count > 0)
                {
                    var errors = GetFirstErrors(engine, maximumNrOfErrors: 10);
                    var msg = string.Join("\n", errors);
                    // ReSharper disable once LocalizableElement
                    var res = MessageBox.Show(msg + "\nBefehl fortsetzen?", "Plan2XrefFromExcel", MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);
                    if (res != DialogResult.Yes) return;
                }

                if (dwgFileNames != null)
                {
                    var errDwgs = new List<string>();
                    foreach (var fileName in dwgFileNames)
                    {
                        Globs.SetReadOnlyAttribute(fileName, false);

                        Application.DocumentManager.Open(fileName, false);
                        Document doc = Application.DocumentManager.MdiActiveDocument;
                        // ReSharper disable once UnusedVariable
                        using (DocumentLock doclock = doc.LockDocument())
                        {
                            engine.Errors.Clear();
                            engine.XrefTrans();
                        }
                        if (engine.Errors.Count <= 0)
                        {
                            try
                            {
                                doc.CloseAndSave(fileName);
                            }
                            catch (Exception ex)
                            {
                                errDwgs.Add(fileName);
                                LogErrorToDwg(fileName,
                                    string.Format(CultureInfo.CurrentCulture,
                                        "Zeichnung konnte nicht gespeichert werden! {0}. {1}", fileName,
                                        ex.Message));
                                doc.CloseAndDiscard();
                            }
                        }
                        else
                        {
                            errDwgs.Add(fileName);
                            var msg = string.Join("\n", engine.Errors);
                            LogErrorToDwg(fileName, msg);
                            doc.CloseAndDiscard();
                        }
                    }

                    if (errDwgs.Count > 0)
                    {
                        var msg = "Es sind bei folgenden Dateien Fehler aufgetreten. Diese Zeichnungen wurden nicht gespeichert. Siehe log-Dateien!\n" +
                                  string.Join("\n", errDwgs);
                        Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2XrefFromExcel!\n" + msg));
                    }
                    else
                    {
                        MessageBox.Show(string.Format(CultureInfo.CurrentCulture,
                            // ReSharper disable once LocalizableElement
                            "Vorgang für {0} Zeichnung(en) abgeschlossen.", dwgFileNames.Length - errDwgs.Count), "Plan2AttTrans", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    // current document
                    Document doc = Application.DocumentManager.MdiActiveDocument;
                    // ReSharper disable once UnusedVariable
                    using (DocumentLock doclock = doc.LockDocument())
                    {
                        var ok = engine.XrefTrans();
                        if (!ok)
                        {
                            var errors = GetFirstErrors(engine, maximumNrOfErrors: 10);
                            var msg = string.Join("\n", errors);
                            Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2XrefFromExcel!\n" + msg));
                        }
                        else
                        {
                            string msg = string.Format(CultureInfo.CurrentCulture, "Plan2XrefFromExcel wurde erfolgreich beendet.");
                            Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage(msg);
                            Log.Info(msg);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2BlockTrans aufgetreten! {0}", ex.Message));
            }
        }

        private static void LogErrorToDwg(string fileName, string msg)
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            var logFileName = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(fileName),
                System.IO.Path.GetFileNameWithoutExtension(fileName) + "_error.log");

            System.IO.File.WriteAllText(logFileName, msg);
        }

        private static List<string> GetFirstErrors(Engine engine, int maximumNrOfErrors)
        {
            var errors = new List<string>();
            foreach (var engineError in engine.Errors)
            {
                errors.Add(engineError);
                if (errors.Count > maximumNrOfErrors)
                {
                    errors.Add("...");
                    break;
                }
            }
            return errors;
        }

    }
}
