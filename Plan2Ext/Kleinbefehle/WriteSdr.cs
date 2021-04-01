using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Windows;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Exception = Autodesk.AutoCAD.Runtime.Exception;

namespace Plan2Ext.Kleinbefehle
{
    // ReSharper disable once UnusedMember.Global
    public class WriteSdr
    {
        [CommandMethod("Plan2WriteSdr")]
        // ReSharper disable once UnusedMember.Global
        public void Plan2WriteSdr()
        {
            try
            {
                var blockInfoDict = new Dictionary<string, BlockInfo>();
                var objectIds = new HashSet<ObjectId>();
                var doc = Application.DocumentManager.MdiActiveDocument;

                if (!GetBlockInfos(doc, blockInfoDict, objectIds)) return;
                SelectAdditionalBlocks(doc, blockInfoDict, objectIds);


                var sdrInfos = GetSdrInfos(doc, objectIds, blockInfoDict).ToArray();
                if (sdrInfos.Length <= 0) return;
                var fileName = GetSdrFileName();
                if (fileName == null) return;

                WriteIt(doc, sdrInfos, fileName);
            }
            catch (Exception e)
            {
                Application.ShowAlertDialog(e.Message);
            }
        }

        private class BlockInfo
        {
            public string PktNrAttName { get; set; }
            public string ZAttname { get; set; }
        }

        private void WriteIt(Document doc, IEnumerable<SdrInfo> sdrInfos, string fileName)
        {
            var lines = new List<string>();
            lines.AddRange(GetHeaderLines(doc));
            lines.AddRange(sdrInfos.Select(x => x.ToString()));
            System.IO.File.WriteAllLines(fileName, lines);
        }

        private IEnumerable<SdrInfo> GetSdrInfos(Document doc, HashSet<ObjectId> objectIds, Dictionary<string, BlockInfo> blockInfoDict)
        {
            var sdrInfos = new List<SdrInfo>();
            using (var transaction = doc.TransactionManager.StartTransaction())
            {
                foreach (var objectId in objectIds)
                {
                    var blockRef = (BlockReference)transaction.GetObject(objectId, OpenMode.ForRead);
                    var blockName = Globs.GetBlockname(blockRef, transaction);
                    var blockInfo = blockInfoDict[blockName];
                    var attributes = Globs.GetAttributEntities(blockRef, transaction);
                    var attribute = attributes.FirstOrDefault(x => x.Tag.Equals(blockInfo.PktNrAttName));
                    var attValue = attribute == null
                        ? ""
                        : attribute.TextString;

                    double zValue;
                    if (string.IsNullOrEmpty(blockInfo.ZAttname)) zValue = blockRef.Position.Z;
                    else
                    {
                        var zAtt = attributes.FirstOrDefault(x => x.Tag.Equals(blockInfo.ZAttname));
                        if (zAtt == null)
                        {
                            doc.Editor.WriteMessage($"\nBlock {blockRef.Handle.ToString()} hat kein Attriut für Höhe: {blockInfo.ZAttname}");
                            zValue = 0.0;
                        }
                        else
                        {
                            var zAttValue = zAtt.TextString.Replace(',', '.');
                            if (!double.TryParse(zAttValue, NumberStyles.Any, CultureInfo.InvariantCulture, out zValue))
                            {
                                doc.Editor.WriteMessage($"\nBlock {blockRef.Handle.ToString()} hat ungültigen Wert für Höhe: {zAtt.TextString}");
                                zValue = 0.0;
                            }
                        }
                    }

                    sdrInfos.Add(new SdrInfo()
                    {
                        PktNr = attValue,
                        X = blockRef.Position.X,
                        Y = blockRef.Position.Y,
                        Z = zValue
                    });
                }
                transaction.Commit();
            }

            return sdrInfos;
        }

        private IEnumerable<string> GetHeaderLines(Document doc)
        {
            var date = System.DateTime.Now;
            var dateStr = date.ToString("yy-MMM-dd hh:mm", CultureInfo.InvariantCulture);

            var poo = new PromptStringOptions("Projektname: ") { AllowSpaces = true };
            var result = doc.Editor.GetString(poo);
            var project = result.Status == PromptStatus.OK
                ? result.StringResult
                : "";

            var l = 16;
            if (project.Length > l) project = project.Substring(0, l);
            project = project.PadRight(l, ' ');

            return new[]
            {
                // ReSharper disable once StringLiteralTypo
                $"00NMSDR33  V04-05.56    {dateStr} 211111",
                $"10NM{project}121111"
            };
        }

        private class SdrInfo
        {
            public string PktNr { get; set; }
            public double X { get; set; }
            public double Y { get; set; }
            public double Z { get; set; }

            public override string ToString()
            {
                var prefix = "08TP";
                var pktnr = PadLeft(PktNr, 16);
                var x = PadLeft($"{X:0.0000000000000}%", 15);
                var y = PadLeft($"{Y:0.0000000000000}%", 15);
                var z = PadLeft($"{Z:0.0000000000000}%", 15);

                return prefix + pktnr + x + " " + y + " " + z;
            }

            private string PadLeft(string txt, int i)
            {
                if (txt.Length > i)
                    txt = txt.Substring(0, i);
                return txt.PadLeft(i, ' ');
            }
        }

        private string GetSdrFileName()
        {
            var saveFileDialog = new SaveFileDialog("Ziel-Zeichnung", "", "sdr", "SDR-Datei",
                SaveFileDialog.SaveFileDialogFlags.NoFtpSites);
            var dialogResult = saveFileDialog.ShowDialog();
            return dialogResult != System.Windows.Forms.DialogResult.OK
                ? null
                : saveFileDialog.Filename;
        }

        private void SelectAdditionalBlocks(Document doc, Dictionary<string, BlockInfo> blockInfoDict, HashSet<ObjectId> objectIds)
        {

            var blockNames = blockInfoDict.Keys.Select(x => new TypedValue((int)DxfCode.BlockName, x));
            var typedValues = new List<TypedValue>
            {
                new TypedValue((int) DxfCode.Start, "INSERT"), new TypedValue((int) DxfCode.Operator, "<OR")
            };
            typedValues.AddRange(blockNames);
            typedValues.Add(new TypedValue((int)DxfCode.Operator, "OR>"));

            var filter = new SelectionFilter(typedValues.ToArray());
            var selOpts = new PromptSelectionOptions { MessageForAdding = "Zusätzliche Blöcke wählen: " };
            var res = doc.Editor.GetSelection(selOpts, filter);
            if (res.Status != PromptStatus.OK) return;
            using (var ss = res.Value)
            {

                var idArray = ss.GetObjectIds();
                foreach (var objectId in idArray)
                {
                    objectIds.Add(objectId);
                }
            }
        }

        private bool GetBlockInfos(Document doc, Dictionary<string, BlockInfo> blockInfoDict, HashSet<ObjectId> objectIds)
        {
            PromptNestedEntityResult result;
            using (var transaction = doc.TransactionManager.StartTransaction())
            {
                var promptOptions = new PromptNestedEntityOptions("Blöcke mit Attributen zeigen <Return für beenden>: ")
                { AllowNone = true };

                do
                {
                    result = doc.Editor.GetNestedEntity(promptOptions);
                    if (result.Status == PromptStatus.OK)
                    {
                        var ent = (Entity)transaction.GetObject(result.ObjectId, OpenMode.ForRead);
                        var attribute = ent as AttributeReference;
                        if (attribute != null)
                        {
                            var blockRef = (BlockReference)transaction.GetObject(attribute.OwnerId, OpenMode.ForRead);
                            var bName = Globs.GetBlockname(blockRef, transaction);
                            objectIds.Add(blockRef.ObjectId);
                            var zAttName = GetZAttname(doc, transaction);
                            blockInfoDict[bName] = new BlockInfo() { PktNrAttName = attribute.Tag, ZAttname = zAttName };
                        }
                        else
                        {
                            var containers = result.GetContainers();
                            if (containers.Length <= 0) continue;
                            var parentBlockRef = (BlockReference)transaction.GetObject(containers[containers.Length - 1], OpenMode.ForRead);
                            var blockName = Globs.GetBlockname(parentBlockRef, transaction);

                            var attRefs = Globs.GetAttributEntities(parentBlockRef, transaction);
                            if (attRefs.Count > 0)
                            {
                                var zAttName = GetZAttname(doc, transaction);

                                objectIds.Add(parentBlockRef.ObjectId);
                                blockInfoDict[blockName] = new BlockInfo() { PktNrAttName = attRefs[0].Tag, ZAttname = zAttName };
                            }
                        }
                    }
                } while (result.Status == PromptStatus.OK);

                transaction.Commit();
            }

            return result.Status == PromptStatus.None;
        }

        private string GetZAttname(Document doc, Transaction transaction)
        {
            string zAttNAme = null;
            var promptOptions = new PromptNestedEntityOptions("Höhen-Attribut zeigen <Return für Blockposition>: ")
            { AllowNone = true };

            do
            {
                var result = doc.Editor.GetNestedEntity(promptOptions);
                if (result.Status == PromptStatus.None) return "";
                if (result.Status == PromptStatus.OK)
                {
                    var attribute = (Entity)transaction.GetObject(result.ObjectId, OpenMode.ForRead) as AttributeReference;
                    if (attribute == null)
                    {
                        doc.Editor.WriteMessage("\nGewähltes Element ist kein Attribut.");
                        continue;
                    }

                    zAttNAme = attribute.Tag;
                }
            } while (zAttNAme == null);

            return zAttNAme;
        }
    }
}
