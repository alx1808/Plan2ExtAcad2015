using System.Globalization;
#if BRX_APP
using Teigha.DatabaseServices;
using Teigha.Runtime;
using Bricscad.ApplicationServices;
#elif ARX_APP
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
#endif
// ReSharper disable IdentifierTypo
// ReSharper disable StringLiteralTypo


namespace Plan2Ext.Kleinbefehle
{
    public class AllBlocksExplodable
    {
        [CommandMethod("Plan2AllBlocksExplodable")]
        public void Plan2AllBlocksExplodable()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            var editor = doc.Editor;
            var nrsChanged = 0;
            try
            {
                using (var transaction = doc.TransactionManager.StartTransaction())
                {
                    var blockTable = (BlockTable) transaction.GetObject(db.BlockTableId, OpenMode.ForRead);
                    foreach (var btrOid in blockTable)
                    {
                        var blockTableRecord = (BlockTableRecord) transaction.GetObject(btrOid, OpenMode.ForRead);
                        if (!blockTableRecord.Explodable)
                        {
                            blockTableRecord.UpgradeOpen();
                            blockTableRecord.Explodable = true;
                            blockTableRecord.DowngradeOpen();
                            nrsChanged++;
                        }
                    }

                    transaction.Commit();
                }
				editor.WriteMessage("\nAnzahl geänderter Blockdefinitionen: " + nrsChanged);
            }
            catch (Exception ex)
            {
                string msg = string.Format(CultureInfo.CurrentCulture, "Fehler in (Plan2AllBlocksExplodable): {0}", ex.Message);
                editor.WriteMessage("\n" + msg);
            }
        }

    }
}
