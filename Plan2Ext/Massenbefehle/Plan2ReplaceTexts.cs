#if ARX_APP
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
//using Autodesk.AutoCAD.Interop;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Plan2Ext.Massenbefehle
{
    public class Plan2ReplaceTextsClass
    {
        #region log4net Initialization
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Convert.ToString((typeof(Plan2ReplaceTextsClass))));
        #endregion

        private static string _OldText = string.Empty;
        private static string _NewText = string.Empty;
        private static List<string> _UsedXrefs = new List<string>();
        private static List<string> _ErrorWithDwgs = new List<string>();
        private static int _NrOfReplacedTexts = 0;

        [CommandMethod("Plan2ReplaceTexts", CommandFlags.Session)]
        static public void Plan2ReplaceTexts()
        {
            try
            {
                log.Info("----------------------------------------------------------------------------------");
                log.Info("Plan2ReplaceTexts");

                _UsedXrefs.Clear();
                _ErrorWithDwgs.Clear();
                _NrOfReplacedTexts = 0;
                _OldText = "";
                _NewText = "";

                string dirName = string.Empty;
                string[] dwgFileNames = null;
                if (!Plan2Ext.Globs.GetMultipleFileNames("AutoCAD-Zeichnung", "Dwg", "Verzeichnis mit Zeichnungen für die Umbenennung der Texte", "Zeichnungen für die Umbenennung der Texte", ref dwgFileNames, ref dirName))
                {
                    return;
                }
                //dirName = @"D:\Plan2\Data\Plan2ReplaceTexts\work";

                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
                if (!GetOldText(ed)) return;
                if (!GetNewText(ed)) return;
                log.Info(string.Format(CultureInfo.CurrentCulture, "Ersetzung: '{0}' -> '{1}'.", _OldText, _NewText));
                List<string> renameNotPossible = new List<string>();

                _UsedXrefs = new List<string>();
                GetAllXRefFullPaths(dwgFileNames, _UsedXrefs);
                foreach (var fileName in dwgFileNames)
                {
                    if (_UsedXrefs.Contains(fileName.ToUpperInvariant())) continue;
                    SetReadOnlyAttribute(fileName, false);

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
                        Plan2Ext.Globs.UnlockAllLayers();

                        // main part
                        ok = ReplaceTexts(renameNotPossible, fileName, db);
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
                        _ErrorWithDwgs.Add(fileName);
                        log.Info(string.Format(CultureInfo.CurrentCulture, "Zeichnung schließen ohne zu speichern: {0}", fileName));
                        doc.CloseAndDiscard();
                    }
                }

                if (renameNotPossible.Count > 0)
                {
                    var names = renameNotPossible.Select(x => System.IO.Path.GetFileName(x)).ToArray();
                    var allNames = string.Join(", ", names);

                    System.Windows.Forms.MessageBox.Show(string.Format("Folgende Zeichnungen konnen nicht geändert werden: {0}", allNames), "Plan2ReplaceTexts");
                }

                ShowResultMessage(dirName.ToUpperInvariant());
            }
            catch (System.Exception ex)
            {
                string msg = string.Format(CultureInfo.CurrentCulture, "{0}", ex.Message);
                System.Windows.Forms.MessageBox.Show(msg, "Plan2ReplaceTexts");
            }
        }

        private static void ShowResultMessage(string dirNameU)
        {
            string errorsWithDwgsMsg = string.Empty;
            if (_ErrorWithDwgs.Count > 0)
            {
                var errDwgs = _ErrorWithDwgs.Select(x =>
                {
                    string newName = string.Empty;
                    if (x.StartsWith(dirNameU))
                    {
                        newName = "." + x.Remove(0, dirNameU.Length);
                    }
                    else
                    {
                        newName = x;
                    }
                    return newName;
                });
                errorsWithDwgsMsg = string.Join("\n", errDwgs.ToArray());
                if (!string.IsNullOrEmpty(errorsWithDwgsMsg))
                    errorsWithDwgsMsg = string.Format(CultureInfo.CurrentCulture, "\nFehler in folgenden Dwgs aufgetreten (Info in Logdatei):\n{0}", errorsWithDwgsMsg);
            }

            string xrefMsg = string.Empty;
            if (_UsedXrefs.Count > 0)
            {
                var usedXrefs = _UsedXrefs.Select(x =>
                {
                    string newName = string.Empty;
                    if (x.StartsWith(dirNameU))
                    {
                        newName = "." + x.Remove(0, dirNameU.Length);
                    }
                    else
                    {
                        newName = x;
                    }
                    return newName;
                });
                xrefMsg = string.Join("\n", usedXrefs.ToArray());
                if (!string.IsNullOrEmpty(xrefMsg))
                    xrefMsg = string.Format(CultureInfo.CurrentCulture, "\nIgnorierte XREFs:\n{0}", xrefMsg);
            }
            string resultMsg = string.Format(CultureInfo.CurrentCulture, "Anzahl ersetzter Texte: {0}\n{2}\n{1}", _NrOfReplacedTexts.ToString(), xrefMsg, errorsWithDwgsMsg);
            log.Info(resultMsg);
            System.Windows.Forms.MessageBox.Show(resultMsg, "Plan2ReplaceTexts");
        }

        private static string[] UpperCaseIt(string[] paths)
        {
            return paths.Select(x => x.ToUpperInvariant()).ToArray();
        }

        private static void GetAllXRefFullPaths(string[] files, List<string> usedXRefFullPaths)
        {
            foreach (var fileName in files)
            {
                Database db = new Database(false, true);
                using (db)
                {
                    db.ReadDwgFile(fileName, System.IO.FileShare.Read, allowCPConversion: false, password: "");

                    using (Transaction tr = db.TransactionManager.StartTransaction())
                    {
                        BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                        foreach (var id in bt)
                        {
                            BlockTableRecord btr = (BlockTableRecord)tr.GetObject(id, OpenMode.ForRead);
                            if (btr.IsLayout)
                            {
                                foreach (var oid in btr)
                                {
                                    var ent = tr.GetObject(oid, OpenMode.ForRead);
                                    BlockReference bref = ent as BlockReference;
                                    if (bref != null)
                                    {
                                        if (bref.BlockTableRecord == default(ObjectId))
                                        {
                                            try
                                            {
                                                log.WarnFormat(CultureInfo.CurrentCulture, "Block in '{0}' hat keinen Blocktable-Record. Handle = {1}", bref.BlockName, bref.Handle);
                                            }
                                            catch (System.Exception ex)
                                            {
                                                log.Error(ex.Message, ex);
                                            }
                                        }
                                        else
                                        {

                                            var bd = (BlockTableRecord)tr.GetObject(bref.BlockTableRecord, OpenMode.ForRead);
                                            if (bd.IsFromExternalReference)
                                            {
                                                string xpath = bd.PathName;
                                                string completePath = string.Empty;
                                                if (!System.IO.Path.IsPathRooted(xpath))
                                                {
                                                    // relativer pfad
                                                    string cpath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(fileName), xpath);
                                                    completePath = System.IO.Path.GetFullPath(cpath);
                                                }
                                                else
                                                {
                                                    // absoluter pfad
                                                    completePath = System.IO.Path.GetFullPath(xpath);
                                                }
                                                completePath = completePath.ToUpperInvariant();
                                                if (!usedXRefFullPaths.Contains(completePath)) usedXRefFullPaths.Add(completePath);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        tr.Commit();
                    }
                }
            }
        }

        private static bool GetNewText(Editor ed)
        {
            var prompt = new PromptStringOptions("\nNeuer Text: ");
            prompt.AllowSpaces = true;
            var prefixUserRes = ed.GetString(prompt);
            if (prefixUserRes.Status != PromptStatus.OK)
            {
                return false;
            }
            _NewText = prefixUserRes.StringResult;
            return true;
        }

        private static bool GetOldText(Editor ed)
        {
            var prompt = new PromptStringOptions("\nZu ersetzender Text: ");
            prompt.AllowSpaces = true;
            while (string.IsNullOrEmpty(_OldText))
            {
                var prefixUserRes = ed.GetString(prompt);
                if (prefixUserRes.Status != PromptStatus.OK)
                {
                    return false;
                }
                _OldText = prefixUserRes.StringResult;
            }
            return true;
        }

        private static bool ReplaceTexts(List<string> renameNotPossible, string fileName, Database db)
        {
            bool ok = true;
            try
            {
                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                    foreach (var id in bt)
                    {
                        BlockTableRecord btr = (BlockTableRecord)tr.GetObject(id, OpenMode.ForRead);
                        if (btr.IsLayout)
                        {
                            foreach (var oid in btr)
                            {
                                var ent = tr.GetObject(oid, OpenMode.ForRead);
                                DBText text = ent as DBText;
                                if (text != null)
                                {
                                    ReplaceTexts(text, tr);
                                }
                                else
                                {
                                    BlockReference bref = ent as BlockReference;
                                    if (bref != null)
                                    {
                                        ReplaceTexts(bref, tr);
                                    }
                                    else
                                    {
                                        MText mtext = ent as MText;
                                        if (mtext != null)
                                        {
                                            ReplaceTexts(mtext, tr);
                                        }
                                    }
                                }
                            }
                        }
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

        private static void ReplaceTexts(BlockReference bref, Transaction tr)
        {
            if (Globs.IsXref(bref, tr)) return;

            var atts = GetAttributEntities(bref, tr);
            foreach (var att in atts)
            {
                if (!att.IsConstant)
                {
                    bool changed;
                    string newT = ReplaceTexts(att.TextString, out changed);
                    if (changed)
                    {
                        att.UpgradeOpen();
                        att.TextString = newT;
                        _NrOfReplacedTexts++;
                    }
                }
            }
        }

        public static List<AttributeReference> GetAttributEntities(BlockReference blockRef, Transaction tr)
        {
            var atts = new List<AttributeReference>();

            foreach (ObjectId attId in blockRef.AttributeCollection)
            {
                var anyAttRef = tr.GetObject(attId, OpenMode.ForRead) as AttributeReference;
                if (anyAttRef != null)
                {
                    atts.Add(anyAttRef);
                }
            }
            return atts;
        }

        private static void ReplaceTexts(DBText text, Transaction tr)
        {
            bool changed;
            var newT = ReplaceTexts(text.TextString, out changed);
            if (changed)
            {
                text.UpgradeOpen();
                text.TextString = newT;
                _NrOfReplacedTexts++;
            }
        }

        private static void ReplaceTexts(MText mtext, Transaction tr)
        {
            bool changed;
            var newT = ReplaceTexts(mtext.Contents, out changed);
            if (changed)
            {
                mtext.UpgradeOpen();
                mtext.Contents = newT;
                _NrOfReplacedTexts++;
            }
        }

        private static string ReplaceTexts(string txt, out bool changed)
        {
            var newT = Regex.Replace(txt, _OldText, _NewText, RegexOptions.IgnoreCase);
            //var newT = txt.Replace(_OldText, _NewText);
            if (string.Compare(newT, txt, StringComparison.OrdinalIgnoreCase) == 0) changed = false;
            else changed = true;
            return newT;
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
    }
}

#endif