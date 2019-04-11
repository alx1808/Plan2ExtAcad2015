using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

// ReSharper disable StringLiteralTypo

namespace Plan2Ext.ETransmit
{
    // ReSharper disable once UnusedMember.Global
    public class Commands
    {
        #region log4net Initialization
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Convert.ToString((typeof(Commands))));
        #endregion
        [CommandMethod("Plan2ETransmit", CommandFlags.Session)]
        // ReSharper disable once UnusedMember.Global
        public void Plan2ETransmit()
        {
            try
            {
                var doc = Application.DocumentManager.MdiActiveDocument;
                var db = doc.Database;
                var ed = doc.Editor;
                var dirName = string.Empty;
                string[] dwgFileNames = null;
                bool useCurrentDwg;
                string commonParent = null;
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
                    var targetFileName = Path.Combine(targetDir, Application.GetSystemVariable("DWGNAME").ToString());
                    ed.WriteMessage(string.Format(CultureInfo.CurrentCulture, "\nFile: {0} to {1}", "this", targetFileName));
                    db.SaveAs(targetFileName, true, DwgVersion.Current, doc.Database.SecurityParameters);
                    CheckXRefBinding(insertBind, db);
                    //DetachAllXrefs(db);
                    //db.Save();
                    useCurrentDwg = true;
                }
                else
                {
                    if (dwgFileNames.Length == 0) return;
                    commonParent = GetCommonParent(dwgFileNames);
                    foreach (var dwgFileName in dwgFileNames)
                    {
                        var targetFileName = GetTargetFileName(dwgFileName, commonParent, targetDir);
                        var exportDirForFile = Path.GetDirectoryName(targetFileName);
                        if (exportDirForFile != null && !Directory.Exists(exportDirForFile)) Directory.CreateDirectory(exportDirForFile);
                        log.Info(string.Format(CultureInfo.CurrentCulture, "\nFile: {0} to {1}", dwgFileName, targetFileName));
                        //File.Copy(dwgFileName,targetFileName, true);

                        log.Info("----------------------------------------------------------------------------------");
                        log.Info(string.Format(CultureInfo.CurrentCulture, "Öffne Zeichnung {0}", targetFileName));

                        Application.DocumentManager.Open(targetFileName, false);
                        doc = Application.DocumentManager.MdiActiveDocument;
                        db = doc.Database;

                        using (DocumentLock acLckDoc = doc.LockDocument())
                        {
                            CheckXRefBinding(insertBind, db);
                            //AddLine(db);
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

        //private void AddLine(Database acCurDb)
        //{
        //    using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
        //    {
        //        // Open the Block table for read
        //        BlockTable acBlkTbl;
        //        acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId,
        //            OpenMode.ForRead) as BlockTable;

        //        // Open the Block table record Model space for write
        //        BlockTableRecord acBlkTblRec;
        //        acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
        //            OpenMode.ForWrite) as BlockTableRecord;

        //        // Create a line that starts at 5,5 and ends at 12,3
        //        using (Line acLine = new Line(new Point3d(5, 5, 0),
        //            new Point3d(12, 3, 0)))
        //        {

        //            // Add the new object to the block table record and the transaction
        //            acBlkTblRec.AppendEntity(acLine);
        //            acTrans.AddNewlyCreatedDBObject(acLine, true);
        //        }

        //        // Save the new object to the database
        //        acTrans.Commit();
        //    }
        //}

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

        private IEnumerable<string> GetPathList(string firstDwg)
        {
            var path = System.IO.Path.GetDirectoryName(firstDwg);
            var lst = new List<string>();
            while (path != null)
            {
                lst.Add(path);
                path = Path.GetDirectoryName(path);
            }

            lst.Reverse();
            return lst;
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
            var xrefObjectIds = Globs.GetAllMsXrefIds(db);
            using (var acXrefIdCol = new ObjectIdCollection())
            {
                foreach (var xrefObjectId in xrefObjectIds)
                {
                    acXrefIdCol.Add(xrefObjectId);

                }
                if (acXrefIdCol.Count > 0)
                {
                    var method = insertBind ? "Einfügen" : "Binden";
                    log.InfoFormat(CultureInfo.CurrentCulture, "{0} von XRefs, Anzahl = {1}",method,acXrefIdCol.Count);
                    db.BindXrefs(acXrefIdCol, insertBind);
                }
            }
        }
    }
}
