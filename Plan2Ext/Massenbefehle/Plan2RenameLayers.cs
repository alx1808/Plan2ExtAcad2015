#if ARX_APP
using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Interop;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using System.Globalization;
// ReSharper disable StringLiteralTypo
// ReSharper disable IdentifierTypo
// ReSharper disable LocalizableElement

// ReSharper disable once IdentifierTypo
namespace Plan2Ext.Massenbefehle
{
    // ReSharper disable once UnusedMember.Global
    public class Plan2RenameLayersClass
    {
        #region log4net Initialization
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(Convert.ToString((typeof(Plan2RenameLayersClass))));
        #endregion

        [CommandMethod("Plan2RenameLayers", CommandFlags.Session)]
        public static void Plan2RenameLayers()
        {
            try
            {
                string dirName;
                using (var folderBrowser = new System.Windows.Forms.FolderBrowserDialog())
                {
                    // ReSharper disable once LocalizableElement
                    folderBrowser.Description = "Verzeichnis mit Zeichnungen für die Umbenennung der Layer";
                    folderBrowser.RootFolder = Environment.SpecialFolder.MyComputer;
                    if (folderBrowser.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                    {
                        return;
                    }
                    dirName = folderBrowser.SelectedPath;
                }

                var ed = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.Editor;
                var prompt = new PromptStringOptions("\nEigener Prefix <Return für keinen>: ") { AllowSpaces = true };
                var prefixUserRes = ed.GetString(prompt);
                if (prefixUserRes.Status != PromptStatus.OK)
                {
                    return;
                }
                var prefixUser = prefixUserRes.StringResult;

                var renameNotPossible = new List<string>();

                var files = System.IO.Directory.GetFiles(dirName, "*.dwg", System.IO.SearchOption.AllDirectories);
                foreach (var fileName in files)
                {
                    Globs.SetReadOnlyAttribute(fileName, false);

                    var acadApp = (AcadApplication)Application.AcadApplication;
                    var prefix = System.IO.Path.GetFileNameWithoutExtension(fileName) + "_"; // "ALX_".ToUpperInvariant();
                    if (!string.IsNullOrEmpty(prefixUser))
                    {
                        prefix = prefixUser + "_" + prefix;
                    }
                    bool ok;

                    Log.Info("----------------------------------------------------------------------------------");
                    Log.Info(string.Format(CultureInfo.CurrentCulture, "Öffne Zeichnung {0}", fileName));

                    Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.Open(fileName, false);
                    var doc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
                    var db = doc.Database;

                    // Lock the new document
                    using (doc.LockDocument())
                    {
                        ok = RenameLayers(renameNotPossible, fileName, acadApp, prefix, db);
                    }

                    if (ok)
                    {
                        Log.Info(string.Format(CultureInfo.CurrentCulture, "Zeichnung speichern und schließen: {0}", fileName));
                        try
                        {
                            doc.CloseAndSave(fileName);
                        }
                        catch (System.Exception ex)
                        {
                            Log.Warn(string.Format(CultureInfo.CurrentCulture, "Zeichnung konnte nicht gespeichert werden! {0}. {1}", fileName, ex.Message));
                            renameNotPossible.Add(fileName);
                            doc.CloseAndDiscard();
                        }
                    }
                    else
                    {
                        Log.Info(string.Format(CultureInfo.CurrentCulture, "Zeichnung schließen ohne zu speichern: {0}", fileName));
                        doc.CloseAndDiscard();
                    }
                }

                if (renameNotPossible.Count > 0)
                {
                    var names = renameNotPossible.Select(System.IO.Path.GetFileName).ToArray();
                    var allNames = string.Join(", ", names);

                    System.Windows.Forms.MessageBox.Show(string.Format("Folgende Zeichnungen konnen nicht geändert werden: {0}", allNames), "Plan2RenameLayer");
                }
            }
            catch (System.Exception ex)
            {
                string msg = string.Format(CultureInfo.CurrentCulture, "{0}", ex.Message);
                System.Windows.Forms.MessageBox.Show(msg, "Plan2RenameLayers");
            }
        }

        private static bool RenameLayers(List<string> renameNotPossible, string fileName, AcadApplication acadApp, string prefix, Database db)
        {
            bool ok = true;
            try
            {
                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    var layerNames = new List<string>();
                    var layerTable = (LayerTable)tr.GetObject(db.LayerTableId, OpenMode.ForRead);
                    foreach (var id in layerTable)
                    {
                        var layerTableRecord = (LayerTableRecord)tr.GetObject(id, OpenMode.ForRead);
                        if (IsFromXref(layerTableRecord)) continue;
                        layerNames.Add(layerTableRecord.Name.ToUpperInvariant());
                    }

                    foreach (var layerName in layerNames)
                    {
                        if (layerName.Equals("0")) continue;
                        if (string.IsNullOrEmpty(layerName.Trim())) continue;
                        var newName = prefix + layerName;
                        if (layerNames.Contains(newName))
                        {
                            Log.Error(string.Format(CultureInfo.CurrentCulture, "Fehler beim Umbenennen von Layer {0}, da ein Layer mit dem neuen Namen '{1} schon existiert!", layerName, newName));
                            ok = false;
                            renameNotPossible.Add(fileName);
                            break;
                        }

                        Log.Info(string.Format(CultureInfo.CurrentCulture, "Layer {0} -> {1}", layerName, newName));
                        // also supports acad2013
                        acadApp.ActiveDocument.SendCommand("_.-RENAME _LA " + "\"" + layerName + "\"\n\"" + newName + "\"\n");
                    }

                    tr.Commit();
                }
            }
            catch (System.Exception ex)
            {
                Log.Error(string.Format(CultureInfo.CurrentCulture, "Fehler beim Umbenennen der Layer: {0}", ex.Message), ex);
                ok = false;

            }
            return ok;
        }

        private static bool IsFromXref(LayerTableRecord layerTableRecord)
        {
            return layerTableRecord.Name.Contains("|");
        }
    }
}
#endif
