using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using log4net;
using Plan2Ext.Properties;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Exception = System.Exception;
// ReSharper disable IdentifierTypo
// ReSharper disable StringLiteralTypo

namespace Plan2Ext.XrefLayerProperties
{
    // ReSharper disable once UnusedMember.Global
    public class Commands
    {
        #region log4net Initialization
        private static readonly ILog Log = LogManager.GetLogger(Convert.ToString((typeof(Commands))));
        #endregion

        [CommandMethod("Plan2ImportXrefLayerProperties")]
        // ReSharper disable once UnusedMember.Global
        public void Plan2ImportXrefLayerProperties()
        {
            Log.Info("Plan2ImportXrefLayerProperties");
            try
            {
                string xrefName;
                if (!GetXrefName(out xrefName)) return;

                string dwgfilename;
                if (!GetDwgfilename(out dwgfilename)) return;


                if (!ImportXrefLayerProperties(dwgfilename, xrefName)) return;

                Application.DocumentManager.MdiActiveDocument.Editor.Regen();
            }
            catch (OperationCanceledException)
            {
                Log.Info("Abbruch durch Benutzer.");
                // cancelled by user
            }
            catch (Exception ex)
            {
                var msg = string.Format(CultureInfo.CurrentCulture,
                    "Fehler in Plan2ImportXrefLayerProperties aufgetreten! {0}", ex.Message);
                Log.Error(msg);
                Application.ShowAlertDialog(msg);
            }
        }

        [CommandMethod("-Plan2ImportXrefLayerProperties")]
        // ReSharper disable once UnusedMember.Global
        public void Plan2ImportXrefLayerPropertiesCl()
        {
            Log.Info("-Plan2ImportXrefLayerProperties");
            try
            {
                string xrefName;
                if (!GetStringAllowSpaces("Xref-Name: ", out xrefName)) return;
                if (string.IsNullOrEmpty(xrefName))
                {
                    var msg = "Es wurde kein XREF-Name angegeben.";
                    EditorHelper.WriteLine(msg);
                    Log.Warn(msg);
                    return;
                }

                string dwgfilename;
                if (!GetStringAllowSpaces("Import Dwg-Name: ", out dwgfilename)) return;
                dwgfilename = dwgfilename.Trim(new char[] {'"'});
                if (!dwgfilename.EndsWith(".dwg", StringComparison.InvariantCultureIgnoreCase))
                {
                    dwgfilename += ".dwg";
                }

                if (!File.Exists(dwgfilename))
                {
                    var msg = string.Format(CultureInfo.CurrentCulture, "Die Datei '{0}' existiert nicht!",
                        dwgfilename);
                    EditorHelper.WriteLine(msg);
                    Log.Warn(msg);
                    return;
                }

                if (!ImportXrefLayerProperties(dwgfilename, xrefName, true)) return;

                Application.DocumentManager.MdiActiveDocument.Editor.Regen();

            }
            catch (OperationCanceledException)
            {
                Log.Info("Abbruch durch Benutzer.");
                // cancelled by user
            }
            catch (Exception ex)
            {
                var msg = string.Format(CultureInfo.CurrentCulture,
                    "Fehler in -Plan2ImportXrefLayerProperties aufgetreten! {0}", ex.Message);
                Log.Error(msg);
                EditorHelper.WriteLine(msg);
            }
        }

        private static bool ImportXrefLayerProperties(string dwgfilename, string xrefName, bool commandLine = false)
        {
            using (var db = new Database())
            {
                db.ReadDwgFile(dwgfilename, FileOpenMode.OpenForReadAndAllShare, false, null);

                var names = XrefManager.GetAllXrefNames(db)
                    .Where(x => string.Compare(xrefName, x, StringComparison.OrdinalIgnoreCase) == 0).ToArray();
                if (!names.Any())
                {
                    var msg = string.Format(CultureInfo.CurrentCulture,
                        "Es gibt kein Xref mit Namen {0} in der Zeichnung {1}.", xrefName, dwgfilename);
                    if (commandLine) EditorHelper.WriteLine(msg);
                    else Application.ShowAlertDialog(msg);
                    Log.Warn(msg);
                    return false;
                }

                var layerStates = LayerManager.GetClonedLayerTableRecords(db).ToList();
                var nameStates = names.SelectMany(x =>
                    layerStates.Where(s => s.Name.StartsWith(x + "|", StringComparison.OrdinalIgnoreCase)));
                SetLayersState(nameStates);
            }

            return true;
        }

        private static bool GetDwgfilename(out string dwgfilename)
        {
            dwgfilename = string.Empty;
            while (true)
            {
                using (var openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.CheckFileExists = true;
                    openFileDialog.CheckPathExists = true;
                    openFileDialog.Multiselect = false;
                    openFileDialog.Title = Resources.Commands_Plan2ImportXrefLayerProperties_Datei_für_Xref_Layer_Importwerten;
                    const string filter = "DWG" + "|*." + "dwg";
                    openFileDialog.Filter = filter;
                    if (openFileDialog.ShowDialog() != DialogResult.OK)
                    {
                        return false;
                    }
                    if (string.Compare(Path.GetFullPath(Globs.GetCurrentDwgName()), openFileDialog.FileName,
                            StringComparison.OrdinalIgnoreCase) != 0)
                    {
                        dwgfilename = openFileDialog.FileName;
                        return true;
                    }
                    Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nImportzeichnung kann nicht die aktuelle Zeichnung sein.");
                }
            }
        }

        private static bool GetStringAllowSpaces(string message, out string str)
        {
            str = string.Empty;
            var editor = Application.DocumentManager.MdiActiveDocument.Editor;
            var result = editor.GetString(new PromptStringOptions(message)
            {
                AllowSpaces = true,
            });

            if (result.Status != PromptStatus.OK) return false;
            str = result.StringResult;
            return true;
        }

        private static bool GetXrefName(out string xrefName)
        {
            xrefName = string.Empty;
            while (true)
            {
                var oid = Globs.GetEntity(typeof(BlockReference),
                    "Xref, dessen Layereigenschaften importiert werden sollen, auswählen: ");
                if (oid == ObjectId.Null) return false;
                xrefName = GetXrefName(oid);
                if (string.IsNullOrEmpty(xrefName))
                {
                    Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage(
                        "\nDas gewählte Element ist kein XRef.");
                }
                else break;
            }

            return true;
        }

        private static string GetXrefName(ObjectId oid)
        {
            var name = string.Empty;
            using (var transaction = Application.DocumentManager.MdiActiveDocument.TransactionManager.StartTransaction())
            {
                var blockReference = (BlockReference)transaction.GetObject(oid, OpenMode.ForRead);
                var blockTableRecord = (BlockTableRecord)transaction.GetObject(blockReference.BlockTableRecord, OpenMode.ForRead);
                if (blockTableRecord.IsFromExternalReference) name = blockTableRecord.Name;
                transaction.Commit();
            }

            return name;
        }

        private static void SetLayersState(IEnumerable<LayerManager.LayerTableRecordExt> layersStates)
        {
            // Get the current document and database
            var acDoc = Application.DocumentManager.MdiActiveDocument;
            var db = acDoc.Database;

            var curLayer = Application.GetSystemVariable("CLAYER").ToString();
            var layerStates = layersStates.ToArray();

            var clonedLineTypeRecords = Globs.GetAllClonedSymbolTablesTableRecords(db, db.LinetypeTableId)
                .Cast<LinetypeTableRecord>().ToArray();

            using (var transaction = db.TransactionManager.StartTransaction())
            {
                var layTb = (LayerTable)transaction.GetObject(db.LayerTableId, OpenMode.ForRead);
                foreach (var ltrOid in layTb)
                {
                    var ltr = (LayerTableRecord)transaction.GetObject(ltrOid, OpenMode.ForRead);


                    var ls = layerStates.FirstOrDefault(x => String.Compare(ltr.Name, x.Name, StringComparison.OrdinalIgnoreCase) == 0);
                    if (ls == null) continue;

                    ltr.UpgradeOpen();

                    ls.AssignValues(ltr, curLayer, clonedLineTypeRecords);

                }
                transaction.Commit();
            }
        }
    }
}
