using System.Collections.Generic;
using System.Globalization;
using System.Linq;
// ReSharper disable IdentifierTypo

// ReSharper disable StringLiteralTypo

#if BRX_APP
using _AcAp = Bricscad.ApplicationServices;
using Teigha.Colors;
using Teigha.DatabaseServices;
using Teigha.Runtime;
using Bricscad.ApplicationServices;
#elif ARX_APP
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
#endif



namespace Plan2Ext.Kleinbefehle
{
    public class Battman
    {
        [CommandMethod("-Battman")]
        public void BattmanCommandLine()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            var editor = doc.Editor;
            try
            {
                var blockNamesWildCards = Globs.GetWildcards("Blocknamen: ", true).ToArray();
                if (blockNamesWildCards.Length == 0) return;

                using (var transaction = db.TransactionManager.StartTransaction())
                {
                    var blockRefs = new List<BlockReference>();
                    var currentSpace = (BlockTableRecord)transaction.GetObject(db.CurrentSpaceId, OpenMode.ForWrite);
                    foreach (var oid in currentSpace)
                    {
                        var blockReference = transaction.GetObject(oid, OpenMode.ForRead) as BlockReference;
                        if (blockReference == null) continue;
                        var blockName = Globs.GetBlockname(blockReference, transaction);
                        if (!blockNamesWildCards.Any(x => x.IsMatch(blockName))) continue;
                        blockRefs.Add(blockReference);
                    }

                    foreach (var blockReference in blockRefs)
                    {
                        var attributes = Globs.GetAttributEntities(blockReference, transaction);
                        var tagValueDict = new Dictionary<string, string>();
                        foreach (var attributeReference in attributes)
                        {
                            tagValueDict[attributeReference.Tag] = attributeReference.TextString;
                        }
                        foreach (var attributeReference in attributes)
                        {
                            attributeReference.UpgradeOpen();
                            attributeReference.Erase(true);
                        }
                        blockReference.UpgradeOpen();
                        var newAttributes = BlockManager.AddAllAttributesFromDefinition(blockReference, transaction);
                        foreach (var attributeReference in newAttributes)
                        {
                            var tag = attributeReference.Tag;
                            string value;
                            if (tagValueDict.TryGetValue(tag, out value))
                            {
                                attributeReference.TextString = value;
                            }
                            else
                            {
                                attributeReference.TextString = "";
                            }
                        }
                        blockReference.DowngradeOpen();
                    }

                    transaction.Commit();
                }
            }
            catch (Exception ex)
            {
                string msg = string.Format(CultureInfo.CurrentCulture, "Fehler in (-Battman): {0}", ex.Message);
                editor.WriteMessage("\n" + msg);
            }
        }

        public void BattmanCommandLineVariante2()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            var editor = doc.Editor;
            try
            {
                var blockNamesWildCards = Globs.GetWildcards("Blocknamen: ", true).ToArray();
                if (blockNamesWildCards.Length == 0) return;

                using (var transaction = db.TransactionManager.StartTransaction())
                {
                    var blockRefs = new List<BlockReference>();
                    var currentSpace = (BlockTableRecord)transaction.GetObject(db.CurrentSpaceId, OpenMode.ForWrite);
                    foreach (var oid in currentSpace)
                    {
                        var blockReference = transaction.GetObject(oid, OpenMode.ForRead) as BlockReference;
                        if (blockReference == null) continue;
                        var blockName = Globs.GetBlockname(blockReference, transaction);
                        if (!blockNamesWildCards.Any(x => x.IsMatch(blockName))) continue;
                        blockRefs.Add(blockReference);
                    }

                    foreach (var blockReference in blockRefs)
                    {
                        var attributes = Globs.GetAttributes(blockReference);
                        using (var substitute = new BlockReference(blockReference.Position, blockReference.BlockTableRecord))
                        {
                            substitute.Layer = blockReference.Layer;
                            substitute.Linetype = blockReference.Linetype;
                            substitute.Color = (Color)blockReference.Color.Clone();
                            substitute.Rotation = blockReference.Rotation;
                            substitute.ScaleFactors = blockReference.ScaleFactors;
                            currentSpace.AppendEntity(substitute);
                            transaction.AddNewlyCreatedDBObject(substitute, true);
                            BlockManager.SetAttributes(substitute, attributes, transaction);
                        }
                    }

                    foreach (var blockReference in blockRefs)
                    {
                        blockReference.UpgradeOpen();
                        blockReference.Erase(true);
                    }

                    transaction.Commit();
                }
            }
            catch (Exception ex)
            {
                string msg = string.Format(CultureInfo.CurrentCulture, "Fehler in (-Battman): {0}", ex.Message);
                editor.WriteMessage("\n" + msg);
            }
        }


    }
}
