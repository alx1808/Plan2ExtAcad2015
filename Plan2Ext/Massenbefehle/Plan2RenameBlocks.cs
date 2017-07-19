#if ARX_APP
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Interop;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using System.Globalization;

namespace Plan2Ext.Massenbefehle
{
    public class Plan2RenameBlocksClass
    {
        #region log4net Initialization
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Convert.ToString((typeof(Plan2RenameBlocksClass))));
        #endregion

        [CommandMethod("Plan2RenameBlocks", CommandFlags.Session)]
        static public void Plan2RenameBlocks()
        {
            try
            {
                string dirName = null;
                using (var folderBrowser = new System.Windows.Forms.FolderBrowserDialog())
                {
                    folderBrowser.Description = "Verzeichnis mit Zeichnungen für die Umbenennung der Blöcke";
                    folderBrowser.RootFolder = Environment.SpecialFolder.MyComputer;
                    //folderBrowser.ShowNewFolderButton = false;
                    if (folderBrowser.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                    {
                        return;
                    }
                    dirName = folderBrowser.SelectedPath;
                }

                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
                var prompt = new PromptStringOptions("\nEigener Prefix <Return für keinen>: ");
                prompt.AllowSpaces = true;
                var prefixUserRes =  ed.GetString(prompt);
                if (prefixUserRes.Status != PromptStatus.OK)
                {
                    return;
                }
                var prefixUser = prefixUserRes.StringResult;

                List<string> renameNotPossible = new List<string>();

                var files = System.IO.Directory.GetFiles(dirName, "*.dwg", System.IO.SearchOption.AllDirectories);
                foreach (var fileName in files)
                {
                    SetReadOnlyAttribute(fileName, false);
                    
                    AcadApplication acadApp = (AcadApplication)Application.AcadApplication;
                    string prefix = System.IO.Path.GetFileNameWithoutExtension(fileName) + "_"; // "ALX_".ToUpperInvariant();
                    if (!string.IsNullOrEmpty(prefixUser))
                    {
                        prefix += (prefixUser + "_");
                    }
                    bool ok = true;

                    log.Info("----------------------------------------------------------------------------------");
                    log.Info(string.Format(CultureInfo.CurrentCulture, "Öffne Zeichnung {0}", fileName));

                    Application.DocumentManager.Open(fileName, false);
                    Document doc = Application.DocumentManager.MdiActiveDocument;
                    Database db = doc.Database;

                    // Lock the new document
                    using (DocumentLock acLckDoc = doc.LockDocument())
                    {
                        ed = Application.DocumentManager.MdiActiveDocument.Editor;

                        // Dyn Blocks are handled in RenameBlocks
                        //ok = RenameDynamicBlocks(prefix, db);
                        
                        if (!ok)
                        {
                            renameNotPossible.Add(fileName);
                        }
                        else
                        {
                            ok = RenameBlocks(renameNotPossible, fileName, acadApp, prefix, db);
                        }
                    }

                    if (ok)
                    {
                        log.Info(string.Format(CultureInfo.CurrentCulture, "Zeichnung speichern und schließen: {0}", fileName));
                        try
                        {
                            doc.CloseAndSave(fileName);
                        }
                        catch (System.Exception ex)
                        {
                            log.Warn(string.Format(CultureInfo.CurrentCulture, "Zeichnung konnte nicht gespeichert werden! {0}. {1}", fileName, ex.Message));
                            renameNotPossible.Add(fileName);
                            doc.CloseAndDiscard();
                        }
                    }
                    else
                    {
                        log.Info(string.Format(CultureInfo.CurrentCulture, "Zeichnung schließen ohne zu speichern: {0}", fileName));
                        doc.CloseAndDiscard();
                    }
                }

                if (renameNotPossible.Count > 0)
                {
                    var names = renameNotPossible.Select(x => System.IO.Path.GetFileName(x)).ToArray();
                    var allNames = string.Join(", ", names);

                    System.Windows.Forms.MessageBox.Show(string.Format("Folgende Zeichnungen konnen nicht geändert werden: {0}", allNames), "Plan2RenameBlock");
                }
            }
            catch (System.Exception ex)
            {
                string msg = string.Format(CultureInfo.CurrentCulture, "{0}", ex.Message);
                System.Windows.Forms.MessageBox.Show(msg, "Plan2RenameBlock");
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


        private static bool RenameBlocks(List<string> renameNotPossible, string fileName, AcadApplication acadApp, string prefix, Database db)
        {
            bool ok = true;
            try
            {
                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    List<string> blockNames = new List<string>();
                    // Open the blocktable, get the modelspace
                    BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                    foreach (var id in bt)
                    {
                        BlockTableRecord btr = (BlockTableRecord)tr.GetObject(id, OpenMode.ForRead);
                        if (!btr.IsLayout)
                        {
                            blockNames.Add(btr.Name.ToUpperInvariant());
                        }
                    }

                    foreach (var bname in blockNames)
                    {
                        if (string.IsNullOrEmpty(bname.Trim())) continue;
                        string newName = prefix + bname;
                        if (blockNames.Contains(newName))
                        {
                            log.Error(string.Format(CultureInfo.CurrentCulture, "Fehler beim Umbenennen der von Block {0}, da ein Block mit dem neuen Namen '{1} schon existiert!", bname, newName));
                            ok = false;
                            renameNotPossible.Add(fileName);
                            break;
                        }

                        if (bname.StartsWith("*"))
                        {
                            // anonymous or dynamic block
                            continue;
                        }

                        log.Info(string.Format(CultureInfo.CurrentCulture, "Block {0} -> {1}", bname, newName));
                        // also supports acad2013
                        acadApp.ActiveDocument.SendCommand("_.-RENAME _BL " + "\"" + bname + "\"\n\"" + newName + "\"\n");

                        //ed.Command("_.RENAME", "_BL", bname, newName);
                        // async needs await (since 2015)
                        //doc.SendStringToExecute("_.-RENAME _BL " + "\"" + bname + "\"\n\"" + newName + "\"\n", false, false, true);
                        //ed.WriteMessage("\n" + bname);
                    }

                    tr.Commit();
                }
            }
            catch (System.Exception ex)
            {
                log.Error(string.Format(CultureInfo.CurrentCulture, "Fehler beim Umbenennen der Blöcke: {0}", ex.Message), ex);
                ok = false;

            }
            return ok;

        }

        private static bool RenameDynamicBlocks(string prefix, Database db)
        {
            bool ok = true;
            try
            {

                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    // Open the blocktable, get the modelspace
                    BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                    BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);


                    List<BlockTableRecord> dynBtrs = new List<BlockTableRecord>();
                    List<string> names = new List<string>();
                    // Iterate through it, dumping objects
                    foreach (ObjectId objId in btr)
                    {
                        BlockReference br = tr.GetObject(objId, OpenMode.ForRead) as BlockReference;
                        if (br != null)
                        {
                            if (br.IsDynamicBlock)
                            {
                                BlockTableRecord dynBtr = (BlockTableRecord)tr.GetObject(br.DynamicBlockTableRecord, OpenMode.ForWrite);
                                if (!names.Contains(dynBtr.Name))
                                {
                                    dynBtrs.Add(dynBtr);
                                    names.Add(dynBtr.Name);
                                }
                            }
                            else
                            {
                                var bd = (BlockTableRecord)tr.GetObject(br.BlockTableRecord, OpenMode.ForRead);
                                if (bd.IsFromExternalReference)
                                {
                                    // todo?
                                }
                            }
                        }
                    }

                    foreach (var dynBtr in dynBtrs)
                    {
                        log.Info(string.Format(CultureInfo.CurrentCulture, "Dynamischer Block {0} -> {1}", dynBtr.Name, prefix + dynBtr.Name));
                        dynBtr.Name = prefix + dynBtr.Name;
                    }

                    tr.Commit();
                }
            }
            catch (System.Exception ex)
            {
                log.Error(string.Format(CultureInfo.CurrentCulture, "Fehler beim Umbenennen der dynamischen Blöcke: {0}", ex.Message), ex);
                ok = false;
            }

            return ok;
        }
    }
}
#endif
