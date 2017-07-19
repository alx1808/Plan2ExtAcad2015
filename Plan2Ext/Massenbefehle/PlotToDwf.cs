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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Text.RegularExpressions;
using System.IO;
using System.Collections.Specialized;


namespace Plan2Ext.Massenbefehle
{
    public class PlotToDwfClass
    {
        #region log4net Initialization
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Convert.ToString((typeof(PlotToDwfClass))));
        #endregion

        #region Member variables
        private static string _PlotterName = "DWF6 ePlot.pc3";
        private static _AcDb.Transaction _Tr = null;
        private static _AcAp.Document _Doc = null;
        private static _AcDb.Database _Db = null;
        private static _AcEd.Editor _Editor = null;
        private static int _NrPlots = 0;
        private static int _NrPlotsAll = 0;
        private static bool _MultipleLayoutNames = false;
        private static List<string> _LayoutNames = new List<string>();
        #endregion

        [_AcTrx.CommandMethod("Plan2PlotToDwf")]
        public static void Plan2PlotToDwf()
        {
            log.Info("----------------------------------------------------------------");
            log.Info("Plan2PlotToDwf");
            _Doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            _Db = _Doc.Database;
            _Editor = _Doc.Editor;
            _NrPlots = 0;
            _MultipleLayoutNames = false;
            _LayoutNames.Clear();

            try
            {
                if (!PlotInLayouts())
                {
                    string msg = string.Format(CultureInfo.CurrentCulture, "Fehler beim Plotten. Siehe Log-Datei!");
                    _Editor.WriteMessage("\n" + msg);
                    System.Windows.Forms.MessageBox.Show(msg, "Plan2PlotToDwf");
                }
                else
                {
                    string add = string.Empty;
                    if (_MultipleLayoutNames)
                    {
                        add = "\n" + "Mehrfache Layoutnamen! (siehe Log-Datei).";
                    }
                    string resultMsg = string.Format(CultureInfo.CurrentCulture, "Anzahl erzeugter Plots: {0}{1}", _NrPlots.ToString(), add);
                    log.Info(resultMsg);
                    System.Windows.Forms.MessageBox.Show(resultMsg, "Plan2PlotToDwf");
                }
            }
            catch (System.Exception ex)
            {
                string msg = string.Format(CultureInfo.CurrentCulture, "Fehler in (Plan2SetPlotterInLayouts): {0}", ex.Message);
                _Editor.WriteMessage("\n" + msg);
                System.Windows.Forms.MessageBox.Show(ex.Message, "Plan2PlotToDwf");
            }
        }

        [_AcTrx.CommandMethod("Plan2PlotToDwfBulk", _AcTrx.CommandFlags.Session)]
        static public void Plan2PlotToDwfBulk()
        {
            _Doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            _Db = _Doc.Database;
            _Editor = _Doc.Editor;
            _NrPlots = 0;
            _NrPlotsAll = 0;
            _MultipleLayoutNames = false;
            _LayoutNames.Clear();

            try
            {
                log.Info("----------------------------------------------------------------------------------");
                log.Info("Plan2PlotToDwfBulk");

                string dirName = string.Empty;
                using (var folderBrowser = new System.Windows.Forms.FolderBrowserDialog())
                {
                    folderBrowser.Description = "Verzeichnis mit Zeichnungen für den DWF-Plot";
                    folderBrowser.RootFolder = Environment.SpecialFolder.MyComputer;
                    if (folderBrowser.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                    {
                        return;
                    }
                    dirName = folderBrowser.SelectedPath;
                }
                //dirName = @"D:\Plan2\Data\Plan2PlotToDwf";
                log.Info(string.Format(CultureInfo.CurrentCulture, "Zeichnungen unter '{0}' werden als DWF geplottet.", dirName));

                var files = System.IO.Directory.GetFiles(dirName, "*.dwg", System.IO.SearchOption.AllDirectories);
                foreach (var fileName in files)
                {
                    log.Info("----------------------------------------------------------------------------------");
                    log.Info(string.Format(CultureInfo.CurrentCulture, "Öffne Zeichnung {0}", fileName));

                    _AcAp.Application.DocumentManager.Open(fileName, false);
                    _Doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
                    _Db = _Doc.Database;
                    _Editor = _Doc.Editor;

                    using (DocumentLock acLckDoc = _Doc.LockDocument())
                    {
                        _NrPlots = 0;
                        // main part
                        if (!PlotInLayouts())
                        {
                            string msg = string.Format(CultureInfo.CurrentCulture, "Fehler beim Plotten in Zeichnung '{0}'!", fileName);
                            log.Warn(msg);
                        }
                        else
                        {
                            string msg = string.Format(CultureInfo.CurrentCulture, "Anzahl erzeugter Plots: {0} in Zeichnung '{1}'.", _NrPlots.ToString(), fileName);
                            log.Info(msg);
                        }
                        _NrPlotsAll += _NrPlots;
                    }
                    _Doc.CloseAndDiscard();
                }

                string add = string.Empty;
                if (_MultipleLayoutNames)
                {
                    add = "\n" + "Mehrfache Layoutnamen! (siehe Log-Datei).";
                }
                string resultMsg = string.Format(CultureInfo.CurrentCulture, "Anzahl erzeugter Plots: {0}{1}", _NrPlotsAll.ToString(), add);
                System.Windows.Forms.MessageBox.Show(resultMsg, "Plan2PlotToDwf");

            }
            catch (System.Exception ex)
            {
                string msg = string.Format(CultureInfo.CurrentCulture, "{0}", ex.Message);
                System.Windows.Forms.MessageBox.Show(msg, "Plan2PlotToDwfBulk");
            }
        }

        /// <summary>
        /// Sets the read only attribute.
        /// </summary>
        /// <param name="fullName">The full name.</param>
        /// <param name="readOnly">if set to <c>true</c> [read only].</param>
        private static void SetReadOnlyAttribute(string fullName, bool readOnly)
        {
            System.IO.FileInfo filePath = new System.IO.FileInfo(fullName);
            System.IO.FileAttributes attribute;
            if (readOnly)
                attribute = filePath.Attributes | System.IO.FileAttributes.ReadOnly;
            else
            {
                attribute = filePath.Attributes;
                attribute &= ~System.IO.FileAttributes.ReadOnly;
                //attribute = (System.IO.FileAttributes)(filePath.Attributes - System.IO.FileAttributes.ReadOnly);
            }

            System.IO.File.SetAttributes(filePath.FullName, attribute);
        }

        private static bool PlotInLayouts()
        {
            var ok = true;
            _AcAp.Application.SetSystemVariable("BACKGROUNDPLOT", 0);
            using (_Tr = _Db.TransactionManager.StartTransaction())
            {
                var layouts = _Tr.GetObject(_Db.LayoutDictionaryId, _AcDb.OpenMode.ForRead) as _AcDb.DBDictionary;
                foreach (var layoutDe in layouts)
                {
                    //_AcDb.ObjectId layoutId = layManager.GetLayoutId(layManager.CurrentLayout);
                    var layoutId = layoutDe.Value;
                    var layoutObj = (_AcDb.Layout)_Tr.GetObject(layoutId, _AcDb.OpenMode.ForWrite);

                    var loNameUC = layoutObj.LayoutName.ToUpperInvariant();
                    if (string.Compare(loNameUC, "MODEL", StringComparison.OrdinalIgnoreCase) == 0 || loNameUC.StartsWith("X_", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    var fileName = _Doc.Name;
                    var dwgName = Path.GetFileNameWithoutExtension(fileName);
                    var dirName = Path.GetDirectoryName(fileName);
                    //var dwfName = Path.Combine(dirName, dwgName + "_" + RemoveInvalidCharacters(lo.LayoutName) + ".dwf");
                    var dwfName = Path.Combine(dirName, RemoveInvalidCharacters(layoutObj.LayoutName) + ".dwf");
                    log.InfoFormat("Ausgabedateiname: {0}", dwfName);
                    var dwfNameUC = dwfName.ToUpperInvariant();
                    if (_LayoutNames.Contains(dwfNameUC))
                    {
                        var msg = string.Format("Dwf kann nicht exportiert werden, da dieses schon aus einer Zeichnung exportiert wurde! {0}", dwfName);
                        log.Warn(msg);
                        _Editor.WriteMessage("\n" + msg);
                        _MultipleLayoutNames = true;
                        ok = false;
                        continue;
                    }
                    else
                    {
                        _LayoutNames.Add(dwfNameUC);
                    }

                    if (layoutObj.TabSelected == false)
                    {
                        _AcDb.LayoutManager acLayoutMgr = _AcDb.LayoutManager.Current;
                        acLayoutMgr.CurrentLayout = layoutObj.LayoutName;
                    }

                    if (!PlotDwf(layoutObj, dwfName)) ok = false;
                }
                _Tr.Commit();
            }
            return ok;
        }

        private static string RemoveInvalidCharacters(string fileName)
        {
            //string illegal = "\"M\"\\a/ry/ h**ad:>> a\\/:*?\"| li*tt|le|| la\"mb.?";
            string regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            Regex r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
            fileName = r.Replace(fileName, "_");
            return fileName;
        }

        private static bool PlotDwf(_AcDb.Layout lo, string dwfName)
        {
            try
            {
                var logMsg = string.Format("Erzeuge DWF-Datei für Layout '{0}'.", lo.LayoutName);
                log.Info(logMsg);
                _Editor.WriteMessage("\n" + logMsg);

                if (File.Exists(dwfName))
                {
                    File.Delete(dwfName);
                }

                var pi = new _AcPl.PlotInfo();
                pi.Layout = lo.Id;
                var ps = new _AcDb.PlotSettings(lo.ModelType);
                ps.CopyFrom(lo);
                var psv = _AcDb.PlotSettingsValidator.Current;
                psv.SetPlotConfigurationName(ps, _PlotterName, null);
                SetClosestMediaName(psv, _PlotterName, ps, lo.PlotPaperSize.X, lo.PlotPaperSize.Y, _AcDb.PlotPaperUnit.Millimeters, true);
                psv.SetPlotCentered(ps, true);

                pi.OverrideSettings = ps;
                var piv = new _AcPl.PlotInfoValidator();
                piv.MediaMatchingPolicy = _AcPl.MatchingPolicy.MatchEnabled;
                piv.Validate(pi);
                // You probably need to make sure background plotting is disabled. If the BACKGROUNDPLOT sysvar is set to 1 or 3, then PLOT will background plot (which is not what you want).
                // A PlotEngine does the actual plotting
                // (can also create one for Preview)
                if (_AcPl.PlotFactory.ProcessPlotState == _AcPl.ProcessPlotState.NotPlotting)
                {
                    var pe = _AcPl.PlotFactory.CreatePublishEngine();
                    using (pe)
                    {
                        // Create a Progress Dialog to provide info
                        // and allow the user to cancel
                        var ppd = new _AcPl.PlotProgressDialog(false, 1, true);
                        using (ppd)
                        {
                            ppd.set_PlotMsgString(_AcPl.PlotMessageIndex.DialogTitle, "Plot Fortschritt");
                            ppd.set_PlotMsgString(_AcPl.PlotMessageIndex.CancelJobButtonMessage, "Plot Abbrechen");
                            ppd.set_PlotMsgString(_AcPl.PlotMessageIndex.CancelSheetButtonMessage, "Sheet Abbrechen");
                            ppd.set_PlotMsgString(_AcPl.PlotMessageIndex.SheetSetProgressCaption, "Sheet Set Fortschritt");
                            ppd.set_PlotMsgString(_AcPl.PlotMessageIndex.SheetProgressCaption, "Sheet Fortschritt");
                            ppd.LowerPlotProgressRange = 0;
                            ppd.UpperPlotProgressRange = 100;
                            ppd.PlotProgressPos = 0;

                            // Let's start the plot, at last
                            ppd.OnBeginPlot();
                            ppd.IsVisible = true;
                            pe.BeginPlot(ppd, null);
                            // We'll be plotting a single document
                            pe.BeginDocument(pi, _Doc.Name, null, 1, true, dwfName);
                            // Which contains a single sheet
                            ppd.OnBeginSheet();
                            ppd.LowerSheetProgressRange = 0;
                            ppd.UpperSheetProgressRange = 100;
                            ppd.SheetProgressPos = 0;
                            var ppi = new _AcPl.PlotPageInfo();
                            pe.BeginPage(ppi, pi, true, null);
                            pe.BeginGenerateGraphics(null);
                            pe.EndGenerateGraphics(null);

                            // Finish the sheet
                            pe.EndPage(null);
                            ppd.SheetProgressPos = 100;
                            ppd.OnEndSheet();

                            // Finish the document
                            pe.EndDocument(null);

                            // And finish the plot
                            ppd.PlotProgressPos = 100;
                            ppd.OnEndPlot();
                            pe.EndPlot(null);
                        }
                    }
                }
                else
                {
                    var msg = string.Format("Fehler beim Plot von Layout '{0}' in Zeichnung '{1}'! Es wird gerade geplottet!", lo.LayoutName, _Doc.Name);
                    log.Warn(msg);
                    _Editor.WriteMessage("\n" + msg);
                    return false;
                }
            }
            catch (Exception ex)
            {
                var msg = string.Format("Fehler beim Plot von Layout '{0}' in Zeichnung '{1}'! {2}", lo.LayoutName, _Doc.Name, ex.Message);
                log.Warn(msg);
                _Editor.WriteMessage("\n" + msg);
                return false;
            }
            _NrPlots++;
            return true;
        }

        private static void SetClosestMediaName(_AcDb.PlotSettingsValidator psv, string device, _AcDb.PlotSettings ps, double pageWidth, double pageHeight, _AcDb.PlotPaperUnit units, bool matchPrintableArea)
        {
            psv.SetPlotType(ps, Autodesk.AutoCAD.DatabaseServices.PlotType.Extents);
            psv.SetPlotPaperUnits(ps, units);
            psv.SetUseStandardScale(ps, false);
            psv.SetStdScaleType(ps, _AcDb.StdScaleType.ScaleToFit);
            psv.SetPlotConfigurationName(ps, device, null);
            psv.RefreshLists(ps);
            StringCollection mediaList = psv.GetCanonicalMediaNameList(ps);
            double smallestOffset = 0.0;
            string selectedMedia = string.Empty;
            _AcDb.PlotRotation selectedRot = _AcDb.PlotRotation.Degrees000;
            foreach (string media in mediaList)
            {
                psv.SetCanonicalMediaName(ps, media);
                psv.SetPlotPaperUnits(ps, units);
                double mediaPageWidth = ps.PlotPaperSize.X;
                double mediaPageHeight = ps.PlotPaperSize.Y;
                if (matchPrintableArea)
                {
                    mediaPageWidth -= (ps.PlotPaperMargins.MinPoint.X + ps.PlotPaperMargins.MaxPoint.X);
                    mediaPageHeight -= (ps.PlotPaperMargins.MinPoint.Y + ps.PlotPaperMargins.MaxPoint.Y);
                }

                _AcDb.PlotRotation rotationType = _AcDb.PlotRotation.Degrees090;
                //Check that we are not outside the media print area
                if (mediaPageWidth < pageWidth || mediaPageHeight < pageHeight)
                {
                    //Check if 90°Rot will fit, otherwise check next media
                    if (mediaPageHeight < pageWidth || mediaPageWidth >= pageHeight)
                    {
                        //Too small, let's check next media
                        continue;
                    }
                    //That's ok 90°Rot will fit
                    rotationType = _AcDb.PlotRotation.Degrees090;
                }

                double offset = Math.Abs(mediaPageWidth * mediaPageHeight - pageWidth * pageHeight);
                if (selectedMedia == string.Empty || offset < smallestOffset)
                {
                    selectedMedia = media;
                    smallestOffset = offset;
                    selectedRot = rotationType;
                    //Found perfect match so we can quit early
                    if (smallestOffset == 0)
                        break;
                }
            }

            psv.SetCanonicalMediaName(ps, selectedMedia);
            psv.SetPlotRotation(ps, selectedRot);
            string localMedia = psv.GetLocaleMediaName(ps, selectedMedia);
            _Editor.WriteMessage("\n - Closest Media: " + localMedia);
            _Editor.WriteMessage("\n - Offset: " + smallestOffset.ToString());
            _Editor.WriteMessage("\n - Rotation: " + selectedRot.ToString());
        }
    }
}
