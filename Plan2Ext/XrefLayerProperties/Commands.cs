using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using log4net;
using Plan2Ext.Properties;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Exception = System.Exception;
// ReSharper disable IdentifierTypo
// ReSharper disable StringLiteralTypo

namespace Plan2Ext.XrefLayerProperties
{
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
                string dwgfilename;
                using (var openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.CheckFileExists = true;
                    openFileDialog.CheckPathExists = true;
                    openFileDialog.Multiselect = false;
                    openFileDialog.Title = Resources.Commands_Plan2ImportXrefLayerProperties_Datei_für_Xref_Layer_Importwerten;
                    var filter = "DWG" + "|*." + "dwg";
                    openFileDialog.Filter = filter;
                    if (openFileDialog.ShowDialog() != DialogResult.OK)
                    {
                        return;
                    }
                    else
                    {
                        dwgfilename = openFileDialog.FileName;
                    }
                }
                using (var db = new Database())
                {
                    db.ReadDwgFile(dwgfilename, FileShare.Read, false, null);

                    var names = XrefManager.GetAllXrefNames(db);
                    var layerStates = LayerManager.GetClonedLayerTableRecords(db).ToList();
                    var nameStates = names.SelectMany(x => layerStates.Where(s => s.Name.StartsWith(x + "|", StringComparison.OrdinalIgnoreCase)));
                    SetLayersState(nameStates);
                }

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
