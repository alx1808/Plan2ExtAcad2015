using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
// ReSharper disable LocalizableElement
// ReSharper disable IdentifierTypo

// ReSharper disable StringLiteralTypo

namespace Plan2Ext.ETransmit
{
    // ReSharper disable once UnusedMember.Global
    public class Commands
    {
        #region log4net Initialization
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(Convert.ToString((typeof(Commands))));
        #endregion
        [CommandMethod("Plan2ETransmit", CommandFlags.Session)]
        // ReSharper disable once UnusedMember.Global
        public void Plan2ETransmit()
        {
            try
            {
                var ctbDir = GetPrinterStyleSheetDir();

                var doc = Application.DocumentManager.MdiActiveDocument;
                var db = doc.Database;
                var ed = doc.Editor;
                var dirName = string.Empty;
                string[] dwgFileNames = null;
                var targetDir = GetTargetDir();
                var insertBind = GetUserInputForInsertBind();
                if (!Globs.GetMultipleFileNames(
                    "AutoCAD-Zeichnung",
                    "Dwg",
                    "Verzeichnis mit Zeichnungen für ETransmit",
                    "Zeichnungen für ETransmit",
                    ref dwgFileNames,
                    ref dirName,
                    Application.GetSystemVariable("DWGPREFIX").ToString()))
                {
                    var dwgFileName = Globs.GetCurrentDwgName();
                    var targetFileName = Path.Combine(targetDir, Application.GetSystemVariable("DWGNAME").ToString());
                    ed.WriteMessage(string.Format(CultureInfo.CurrentCulture, "\nFile: {0} to {1}", "this", targetFileName));
                    using (doc.LockDocument())
                    {
                        CheckXRefBinding(insertBind, db);
                        CopyCtbs(targetDir, ctbDir);
                        SetNoPlotterToAllLayouts(db);
                    }
                    db.SaveAs(targetFileName, true, DwgVersion.Current, doc.Database.SecurityParameters);
                    doc.CloseAndSave(targetFileName);
                    Application.DocumentManager.Open(dwgFileName, false);

                }
                else
                {
                    if (dwgFileNames.Length == 0) return;
                    var commonParent = GetCommonParent(dwgFileNames);
                    foreach (var dwgFileName in dwgFileNames)
                    {
                        var targetFileName = GetTargetFileName(dwgFileName, commonParent, targetDir);
                        var exportDirForFile = Path.GetDirectoryName(targetFileName);
                        if (exportDirForFile != null && !Directory.Exists(exportDirForFile)) Directory.CreateDirectory(exportDirForFile);
                        Log.Info(string.Format(CultureInfo.CurrentCulture, "\nFile: {0} to {1}", dwgFileName, targetFileName));

                        Log.Info("----------------------------------------------------------------------------------");
                        Log.Info(string.Format(CultureInfo.CurrentCulture, "Öffne Zeichnung {0}", targetFileName));

                        Application.DocumentManager.Open(dwgFileName, false);
                        doc = Application.DocumentManager.MdiActiveDocument;
                        db = doc.Database;

                        using (doc.LockDocument())
                        {
                            CheckXRefBinding(insertBind, db);
                            CopyCtbs(Path.GetDirectoryName(targetFileName), ctbDir);
                            SetNoPlotterToAllLayouts(db);
                        }
                        Globs.CreateBakFile(targetFileName);
                        db.SaveAs(targetFileName, true, DwgVersion.Current, doc.Database.SecurityParameters);
                        doc.CloseAndSave(targetFileName);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // cancelled by user
            }
            catch (System.Exception ex)
            {
                Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture,
                    "Fehler in Plan2ETransmit aufgetreten! {0}", ex.Message));
            }
        }

        private void SetNoPlotterToAllLayouts(Database db)
        {
            var noPlotterName = GetNoPlotterName(db);
            SetPlotterInLayouts(noPlotterName);
        }

        private static void SetPlotterInLayouts(string plotterName)
        {
            var db = Application.DocumentManager.MdiActiveDocument.Database;
            using (var transaction = db.TransactionManager.StartTransaction())
            {
                var plotSetVal = PlotSettingsValidator.Current;
                var layouts = (DBDictionary)transaction.GetObject(db.LayoutDictionaryId, OpenMode.ForRead);
                foreach (var layoutDe in layouts)
                {
                    var layoutId = layoutDe.Value;
                    var layoutObj = (Layout)transaction.GetObject(layoutId, OpenMode.ForWrite);
                    plotSetVal.RefreshLists(layoutObj);
                    using (var ps = new PlotSettings(layoutObj.ModelType))
                    {
                        ps.CopyFrom(layoutObj);
                        plotSetVal.SetPlotConfigurationName(ps, plotterName, null);
                        SetCanonicalMediaToSunHires(plotSetVal, ps);
                        layoutObj.CopyFrom(ps);
                    }
                }
                transaction.Commit();
            }
        }

        private static void SetCanonicalMediaToSunHires(PlotSettingsValidator plotSetVal, PlotSettings ps)
        {
            var mediaNames = plotSetVal.GetCanonicalMediaNameList(ps);
            var mediaName = "";
            foreach (var mn in mediaNames)
            {
                if (mn.StartsWith("Sun_Hi", StringComparison.OrdinalIgnoreCase))
                {
                    mediaName = mn;
                }
            }

            if (!string.IsNullOrEmpty(mediaName)) plotSetVal.SetCanonicalMediaName(ps, mediaName);
        }


        private static string GetNoPlotterName(Database db)
        {
            var possibleNoPlotterNames = new[] { "Kein", "None", "No" };
            var devList = Plotter.GetDeviceList(db);
            if (devList.Count == 0) throw new InvalidOperationException("Es wurden keine Plotter gefunden!");

            var noPlotterName = possibleNoPlotterNames.FirstOrDefault(x => devList.Contains(x));
            if (noPlotterName == default(string))
            {
                throw new InvalidOperationException("Es wurde kein Plotter mit Namen 'Kein' gefunden!");
            }

            return noPlotterName;
        }

        private void CopyCtbs(string targetDir, string ctbDir)
        {
            foreach (var stylesheetName in GetAllUsedStylesheetNames())
            {
                var sourceFile = Path.Combine(ctbDir, stylesheetName);
                if (!File.Exists(sourceFile)) continue;
                var targetFile = Path.Combine(targetDir, stylesheetName);
                File.Copy(sourceFile, targetFile, true);
            }
        }

        private static string GetPrinterStyleSheetDir()
        {
            UserConfigurationManager userConfigurationManager = Application.UserConfigurationManager;
            IConfigurationSection profile = userConfigurationManager.OpenCurrentProfile();
            using (IConfigurationSection general = profile.OpenSubsection("General"))
            {
                return (string)general.ReadProperty("PrinterStyleSheetDir", string.Empty);
            }
        }

        private static IEnumerable<string> GetAllUsedStylesheetNames()
        {
            var stylesheetNames = new HashSet<string>();
            var doc = Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            using (var transaction = db.TransactionManager.StartTransaction())
            {
                var layouts = (DBDictionary)transaction.GetObject(db.LayoutDictionaryId, OpenMode.ForRead);
                foreach (var layoutDe in layouts)
                {
                    var layoutId = layoutDe.Value;
                    var layoutObj = (Layout)transaction.GetObject(layoutId, OpenMode.ForRead);
                    var ps = (PlotSettings)layoutObj;
                    var stylesheetName = ps.CurrentStyleSheet;
                    if (!string.IsNullOrEmpty(stylesheetName)) stylesheetNames.Add(stylesheetName);
                }
                transaction.Commit();
            }

            return stylesheetNames;
        }


        private string GetTargetFileName(string dwgFileName, string commonParent, string targetDir)
        {
            var rest = dwgFileName.Remove(0, commonParent.Length);
            var dirName = Path.GetFileName(commonParent);
            if (!string.IsNullOrEmpty(dirName))
            {
                dirName = "\\" + dirName;
            }
            return targetDir + dirName + rest;
        }

        private string GetTargetDir()
        {
            var defaultPath = "c:\\exporttemp";
            using (var folderBrowser = new System.Windows.Forms.FolderBrowserDialog())
            {
                folderBrowser.Description = "Zielverzeichnis";
                folderBrowser.RootFolder = Environment.SpecialFolder.MyComputer;
                folderBrowser.SelectedPath = defaultPath;

                if (folderBrowser.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                {
                    throw new OperationCanceledException();
                }

                return folderBrowser.SelectedPath;
            }
        }

        private string GetCommonParent(string[] dwgFileNames)
        {
            var firstDwg = dwgFileNames[0];
            var path = Path.GetDirectoryName(firstDwg);
            while (path != null && dwgFileNames.Any(x => !x.StartsWith(path)))
            {
                path = Path.GetDirectoryName(path);
            }

            return path;
        }

        private bool GetUserInputForInsertBind()
        {
            var pKeyOpts = new PromptKeywordOptions("") { Message = "\nXRefs Binden/<Einfügen>: " };
            pKeyOpts.Keywords.Add("Binden");
            pKeyOpts.Keywords.Add("Einfügen");
            pKeyOpts.AllowNone = true;
            var doc = Application.DocumentManager.MdiActiveDocument;
            var ed = doc.Editor;
            PromptResult pKeyRes = ed.GetKeywords(pKeyOpts);
            if (pKeyRes.Status == PromptStatus.Cancel)
            {
                throw new OperationCanceledException();
            }
            if (pKeyRes.Status == PromptStatus.None || pKeyRes.StringResult == "Einfügen") return true;
            if (pKeyRes.Status == PromptStatus.OK) return false;
            throw new InvalidOperationException("Userinput Status: " + pKeyRes.Status);
        }

        private void CheckXRefBinding(bool insertBind, Database db)
        {
            //var xrefObjectIds = Globs.GetAllMsXrefIds(db);
            var xrefObjectIds = XrefManager.GetAllFirstLevelXrefIds(db);
            using (var acXrefIdCol = new ObjectIdCollection())
            {
                foreach (var xrefObjectId in xrefObjectIds)
                {
                    acXrefIdCol.Add(xrefObjectId);

                }
                if (acXrefIdCol.Count > 0)
                {
                    var method = insertBind ? "Einfügen" : "Binden";
                    Log.InfoFormat(CultureInfo.CurrentCulture, "{0} von XRefs, Anzahl = {1}", method, acXrefIdCol.Count);
                    db.BindXrefs(acXrefIdCol, insertBind);
                }
            }
        }
    }
}
