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

using System;
using System.Collections.Generic;
using System.Globalization;
using Autodesk.AutoCAD.ApplicationServices.Core;
// ReSharper disable StringLiteralTypo

namespace Plan2Ext.BlockTrans
{
    public class Commands
    {
        #region log4net Initialization
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(Convert.ToString((typeof(Commands))));
        static Commands()
        {
            if (log4net.LogManager.GetRepository(System.Reflection.Assembly.GetExecutingAssembly()).Configured == false)
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                log4net.Config.XmlConfigurator.ConfigureAndWatch(new System.IO.FileInfo(System.IO.Path.Combine(new System.IO.FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).DirectoryName, "_log4net.config")));
            }

        }
        #endregion

        [_AcTrx.CommandMethod("Plan2BlockTransExport")]
        // ReSharper disable once UnusedMember.Global
        public static void Plan2BlockTransExport()
        {
            try
            {
#if BRX_APP
                return;
#else

                _AcAp.Document doc = Application.DocumentManager.MdiActiveDocument;
                using (doc.LockDocument())
                {
                    Engine engine = new Engine();
                    var ok = engine.ExcelExport();
                    if (!ok)
                    {
                        Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler beim Export!"));
                    }
                    else
                    {
                        string msg = string.Format(CultureInfo.CurrentCulture, "Der Excel-Export wurde erfolgreich beendet.");
                        Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage(msg);
                        Log.Info(msg);
                    }
                }
#endif
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2BlockTransExport aufgetreten! {0}", ex.Message));
            }
        }

        [_AcTrx.CommandMethod("Plan2BlockTrans")]
        // ReSharper disable once UnusedMember.Global
        public static void Plan2BlockTrans()
        {
            try
            {
#if BRX_APP
                return;
#else

                _AcAp.Document doc = Application.DocumentManager.MdiActiveDocument;
                // ReSharper disable once UnusedVariable
                using (_AcAp.DocumentLock doclock = doc.LockDocument())
                {

                    _AcEd.Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
                    // First let's use the editor method, GetFileNameForOpen()
                    _AcEd.PromptOpenFileOptions opts = new _AcEd.PromptOpenFileOptions("Excel-Datei für Block-Infos")
                    {
                        Filter = "Excel (*.xlsx)|*.xlsx|Excel alt (*.xls)|*.xls"
                    };
                    _AcEd.PromptFileNameResult pr = ed.GetFileNameForOpen(opts);
                    if (pr.Status != _AcEd.PromptStatus.OK) return;

                    string fileName = pr.StringResult;

                    Engine engine = new Engine();
                    var ok = engine.BlockTrans(fileName);
                    if (!ok)
                    {
                        var errors = GetFirstErrors(engine, maximumNrOfErrors: 10);
                        var msg = string.Join("\n", errors);
                        Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2BlockTrans!\n" + msg));
                    }
                    else
                    {
                        string msg = string.Format(CultureInfo.CurrentCulture, "Plan2BlockTrans wurde erfolgreich beendet.");
                        Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage(msg);
                        Log.Info(msg);
                    }
                }
#endif
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2BlockTrans aufgetreten! {0}", ex.Message));
            }
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
