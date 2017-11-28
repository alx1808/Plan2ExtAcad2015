#if BRX_APP
using _AcAp = Bricscad.ApplicationServices;
//using _AcBr = Teigha.BoundaryRepresentation;
using _AcCm = Teigha.Colors;
using _AcDb = Teigha.DatabaseServices;
using _AcEd = Bricscad.EditorInput;
using _AcGe = Teigha.Geometry;
using _AcGi = Teigha.GraphicsInterface;
using _AcGs = Teigha.GraphicsSystem;
using _AcPl = Bricscad.PlottingServices;
using _AcBrx = Bricscad.Runtime;
using _AcTrx = Teigha.Runtime;
using _AcWnd = Bricscad.Windows;
using _AcIntCom = BricscadDb;
using _AcInt = BricscadApp;
#elif ARX_APP
using _AcAp = Autodesk.AutoCAD.ApplicationServices;
using _AcBr = Autodesk.AutoCAD.BoundaryRepresentation;
using _AcCm = Autodesk.AutoCAD.Colors;
using _AcDb = Autodesk.AutoCAD.DatabaseServices;
using _AcEd = Autodesk.AutoCAD.EditorInput;
using _AcGe = Autodesk.AutoCAD.Geometry;
using _AcGi = Autodesk.AutoCAD.GraphicsInterface;
using _AcGs = Autodesk.AutoCAD.GraphicsSystem;
using _AcPl = Autodesk.AutoCAD.PlottingServices;
using _AcBrx = Autodesk.AutoCAD.Runtime;
using _AcTrx = Autodesk.AutoCAD.Runtime;
using _AcWnd = Autodesk.AutoCAD.Windows;
using _AcIntCom = Autodesk.AutoCAD.Interop.Common;
using _AcInt = Autodesk.AutoCAD.Interop;
#endif

using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace Plan2Ext.Massenbefehle
{
    public class AttAustauschBulk
    {
        #region log4net Initialization
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Convert.ToString((typeof(AttAustauschBulk))));
        static AttAustauschBulk()
        {
            if (log4net.LogManager.GetRepository(System.Reflection.Assembly.GetExecutingAssembly()).Configured == false)
            {
                log4net.Config.XmlConfigurator.ConfigureAndWatch(new System.IO.FileInfo(System.IO.Path.Combine(new System.IO.FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).DirectoryName, "_log4net.config")));
            }

        }
        #endregion

        [_AcTrx.CommandMethod("Plan2AttAustausch", _AcTrx.CommandFlags.Session)]
        static public void Plan2AttAustausch()
        {
            try
            {
#if BRX_APP
                return;
#else
                log.Info("----------------------------------------------------------------------------------");
                log.Info("Plan2AttAustausch");


                _AcAp.Document doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
                _AcEd.Editor ed = doc.Editor;

                var blockName = string.Empty;
                var attributeName1 = string.Empty;
                var attributeName2 = string.Empty;
                if (!GetAttributeInfos(ed, doc, out blockName, out attributeName1, out attributeName2)) return;

                string dwgName = doc.Name;
                var dwgProposal = System.IO.Path.GetDirectoryName(dwgName);

                _CurInfo = new Info() { DwgName = dwgName };
                var infos = new List<Info>();
                infos.Add(_CurInfo);

                using (_AcAp.DocumentLock m_doclock = doc.LockDocument())
                {
                    SwitchAttributes(blockName, attributeName1, attributeName2);
                }

                var nrWithoutBlock = infos.Where(x => x.HasBlock == false).Count();
                var nrWithMissingAttsInRef = infos.Where(x => x.NrOfAttNotFoundInReferences > 0).Count();
                var nrWithMissingAttsInDef = infos.Where(x => x.Att1Found == false || x.Att2Found == false).Count();

                var messages = new List<string>();
                if (nrWithoutBlock > 0)
                {
                    messages.Add("Keine passende Blockdefinition.");
                }
                if (nrWithMissingAttsInDef > 0)
                {
                    messages.Add("Fehlenden Attributdefinitionen.");
                }
                if (nrWithMissingAttsInRef > 0)
                {
                    messages.Add("Fehlenden Attribute in den Blockreferenzen.");
                }
                if (messages.Count > 0)
                {
                    messages.Add("Siehe Logdatei.");
                    var msg = string.Join("\n", messages);
                    _AcAp.Application.ShowAlertDialog(msg);
                }
                else
                {
                    _AcAp.Application.ShowAlertDialog("Attributaustausch war erfolgreich.");
                }

#endif
            }
            catch (System.Exception ex)
            {
                log.Error(ex.Message, ex);
                _AcAp.Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2AttAustausch aufgetreten! {0}", ex.Message));
            }
        }

        [_AcTrx.CommandMethod("Plan2AttAustauschBulk", _AcTrx.CommandFlags.Session)]
        static public void Plan2AttAustauschBulk()
        {
            try
            {
#if BRX_APP
                return;
#else
                log.Info("----------------------------------------------------------------------------------");
                log.Info("Plan2AttAustauschBulk");


                _AcAp.Document doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
                _AcEd.Editor ed = doc.Editor;

                var blockName = string.Empty;
                var attributeName1 = string.Empty;
                var attributeName2 = string.Empty;
                if (!GetAttributeInfos(ed, doc, out blockName, out attributeName1, out attributeName2)) return;

                string dwgName = doc.Name;
                var dwgProposal = System.IO.Path.GetDirectoryName(dwgName);
                string dirName = string.Empty;
                string[] dwgFileNames = null;
                if (!Plan2Ext.Globs.GetMultipleFileNames("AutoCAD-Zeichnung", "Dwg", "Verzeichnis mit Zeichnungen für den Attributaustausch", "Zeichnungen für Attributaustausch", ref dwgFileNames, ref dirName, defaultPath: dwgProposal))
                {
                    return;
                }
                doc.CloseAndDiscard();

                List<string> saveFileNotPossible = new List<string>();
                List<Info> infos = new List<Info>();

                foreach (var fileName in dwgFileNames)
                {
                    _CurInfo = new Info() { DwgName = fileName };
                    log.InfoFormat(CultureInfo.CurrentCulture, "Tausche Attribute '{0}'-'{1}' in Block '{2}' in Zeichnung '{3}'.", attributeName1, attributeName2, blockName, fileName);
                    infos.Add(_CurInfo);
                    SetReadOnlyAttribute(fileName, false);

                    bool ok = true;

                    log.Info("----------------------------------------------------------------------------------");
                    log.Info(string.Format(CultureInfo.CurrentCulture, "Öffne Zeichnung {0}", fileName));

                    Application.DocumentManager.Open(fileName, false);
                    doc = Application.DocumentManager.MdiActiveDocument;
                    var db = doc.Database;

                    // Lock the new document
                    using (DocumentLock acLckDoc = doc.LockDocument())
                    {
                        ed = Application.DocumentManager.MdiActiveDocument.Editor;
                        ok = SwitchAttributes(blockName, attributeName1, attributeName2);
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
                            saveFileNotPossible.Add(fileName);
                            doc.CloseAndDiscard();
                        }
                    }
                    else
                    {
                        log.Info(string.Format(CultureInfo.CurrentCulture, "Zeichnung schließen ohne zu speichern: {0}", fileName));
                        doc.CloseAndDiscard();
                    }
                }
                if (saveFileNotPossible.Count > 0)
                {
                    var names = saveFileNotPossible.Select(x => System.IO.Path.GetFileName(x)).ToArray();
                    var allNames = string.Join(", ", names);

                    System.Windows.Forms.MessageBox.Show(string.Format("Folgende Zeichnungen konnen nicht geändert werden: {0}", allNames), "Plan2CalcAreaBulk");
                }
                var nrWithoutBlock = infos.Where(x => x.HasBlock == false).Count();
                var nrWithMissingAttsInRef = infos.Where(x => x.NrOfAttNotFoundInReferences > 0).Count();
                var nrWithMissingAttsInDef = infos.Where(x => x.Att1Found == false || x.Att2Found == false).Count();

                var messages = new List<string>();
                if (nrWithoutBlock > 0)
                {
                    messages.Add("Es gibt Zeichnungen ohne passende Blockdefinition.");
                }
                if (nrWithMissingAttsInDef > 0)
                {
                    messages.Add("Es gibt Zeichnungen mit fehlenden Attributdefinitionen.");
                }
                if (nrWithMissingAttsInRef > 0)
                {
                    messages.Add("Es gibt Zeichnungen mit fehlenden Attributen in den Blockreferenzen.");
                }
                if (messages.Count > 0)
                {
                    var logFileName = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "Plan2.log");
                    messages.Add("Siehe Logdatei.");
                    var msg = string.Join("\n", messages);
                    _AcAp.Application.ShowAlertDialog(msg);

                    if (System.IO.File.Exists(logFileName))
                    {
                        System.Diagnostics.Process.Start(logFileName);
                    }
                }
                else
                {
                    _AcAp.Application.ShowAlertDialog("Attributaustausch war erfolgreich.");
                }
#endif
            }
            catch (System.Exception ex)
            {
                log.Error(ex.Message, ex);
                _AcAp.Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2AttAustauschBulk aufgetreten! {0}", ex.Message));
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

        private class Info
        {
            public string DwgName { get; set; }
            public bool HasBlock { get; set; }
            public bool Att1Found { get; set; }
            public bool Att2Found { get; set; }
            public int NrOfAttNotFoundInReferences { get; set; }
        }
        private static Info _CurInfo;

        private static bool SwitchAttributes(string blockName, string attributeName1, string attributeName2)
        {
            try
            {

                _AcAp.Document doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
                _AcEd.Editor ed = doc.Editor;
                var db = doc.Database;

                if (!SwitchInBlockTableRecord(blockName, attributeName1, attributeName2, db)) return false;

                return SwitchInBlockReferences(blockName, attributeName1, attributeName2, db);

            }
            catch (System.Exception ex)
            {
                log.Error(ex.Message);
                return false;
            }
        }

        private static bool SwitchInBlockReferences(string blockName, string attributeName1, string attributeName2, _AcDb.Database db)
        {
            bool ok = true;
            using (var trans = db.TransactionManager.StartTransaction())
            {
                _AcDb.BlockTable bt = (_AcDb.BlockTable)trans.GetObject(db.BlockTableId, _AcDb.OpenMode.ForRead);
                _AcDb.BlockTableRecord btr = (_AcDb.BlockTableRecord)trans.GetObject(bt[_AcDb.BlockTableRecord.ModelSpace], _AcDb.OpenMode.ForRead);

                foreach (_AcDb.ObjectId objId in btr)
                {
                    _AcDb.BlockReference br = trans.GetObject(objId, _AcDb.OpenMode.ForRead) as _AcDb.BlockReference;
                    if (br != null)
                    {
                        var bn = Plan2Ext.Globs.GetBlockname(br, trans);
                        if (bn == blockName)
                        {
                            _AcDb.AttributeReference att1 = null;
                            _AcDb.AttributeReference att2 = null;
                            foreach (_AcDb.ObjectId attId in br.AttributeCollection)
                            {
                                var anyAttRef = trans.GetObject(attId, _AcDb.OpenMode.ForRead) as _AcDb.AttributeReference;
                                if (anyAttRef != null)
                                {
                                    if (anyAttRef.Tag == attributeName1)
                                    {
                                        att1 = anyAttRef;
                                    }
                                    else if (anyAttRef.Tag == attributeName2)
                                    {
                                        att2 = anyAttRef;
                                    }
                                }
                            }
                            if (att1 == null)
                            {
                                _CurInfo.NrOfAttNotFoundInReferences++;
                                log.WarnFormat(CultureInfo.CurrentCulture, "Attribut '{0}' nicht gefunden in Block {1}.", attributeName1, br.Handle.ToString());
                            }
                            else if (att2 == null)
                            {
                                _CurInfo.NrOfAttNotFoundInReferences++;
                                log.WarnFormat(CultureInfo.CurrentCulture, "Attribut '{0}' nicht gefunden in Block {1}.", attributeName2, br.Handle.ToString());
                            }
                            else
                            {
                                _CurInfo.Att1Found = true;
                                _CurInfo.Att2Found = true;
                                btr.UpgradeOpen();
                                att1.UpgradeOpen();
                                att2.UpgradeOpen();

                                att1.Tag = attributeName2;
                                att2.Tag = attributeName1;

                                att1.DowngradeOpen();
                                att2.DowngradeOpen();
                                btr.DowngradeOpen();

                                ok = true;
                            }
                        }
                    }
                }
                trans.Commit();
            }
            return ok;
        }

        private static bool SwitchInBlockTableRecord(string blockName, string attributeName1, string attributeName2, _AcDb.Database db)
        {
            bool ok = false;
            using (var trans = db.TransactionManager.StartTransaction())
            {
                _AcDb.BlockTable bt = db.BlockTableId.GetObject(_AcDb.OpenMode.ForRead) as _AcDb.BlockTable;
                if (!bt.Has(blockName))
                {
                    _CurInfo.HasBlock = false;
                    log.WarnFormat(CultureInfo.CurrentCulture, "Es existiert kein Block namens '{0}'!", blockName);
                }
                else
                {
                    _CurInfo.HasBlock = true;

                    var oid = bt[blockName];
                    _AcDb.BlockTableRecord btr = (_AcDb.BlockTableRecord)trans.GetObject(oid, _AcDb.OpenMode.ForRead);
                    if (btr.HasAttributeDefinitions)
                    {
                        _AcDb.AttributeDefinition att1 = null;
                        _AcDb.AttributeDefinition att2 = null;
                        foreach (var attOid in btr)
                        {

                            _AcDb.AttributeDefinition attDef = attOid.GetObject(_AcDb.OpenMode.ForRead) as _AcDb.AttributeDefinition;
                            if (attDef != null)
                            {
                                if (string.Compare(attributeName1, attDef.Tag, StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    att1 = attDef;
                                }
                                else if (string.Compare(attributeName2, attDef.Tag, StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    att2 = attDef;
                                }
                            }
                        }
                        if (att1 == null)
                        {
                            _CurInfo.Att1Found = false;
                            log.WarnFormat(CultureInfo.CurrentCulture, "Attribut '{0}' nicht gefunden in Blockdefinition.", attributeName1);

                        }
                        if (att2 == null)
                        {
                            _CurInfo.Att2Found = false;
                            log.WarnFormat(CultureInfo.CurrentCulture, "Attribut '{0}' nicht gefunden in Blockdefinition.", attributeName2);
                        }
                        if (att1 != null && att2 != null)
                        {
                            _CurInfo.Att1Found = true;
                            _CurInfo.Att2Found = true;
                            btr.UpgradeOpen();
                            att1.UpgradeOpen();
                            att2.UpgradeOpen();

                            var att1Prompt = att1.Prompt;
                            var att2Prompt = att2.Prompt;

                            att1.Prompt = att2Prompt;
                            att2.Prompt = att1Prompt;
                            att1.Tag = attributeName2;
                            att2.Tag = attributeName1;

                            att1.DowngradeOpen();
                            att2.DowngradeOpen();
                            btr.DowngradeOpen();

                            ok = true;
                        }
                    }
                }
                trans.Commit();
            }
            return ok;
        }

        private static bool GetAttributeInfos(_AcEd.Editor ed, _AcAp.Document doc, out string blockName, out string attributeName1, out string attributeName2)
        {
            string bname1;
            attributeName1 = string.Empty;
            attributeName2 = string.Empty;
            blockName = string.Empty;
            if (!GetAttribute(ed, doc, out bname1, out attributeName1, "Attribut1 wählen: ")) return false;
            string bname2;
            if (!GetAttribute(ed, doc, out bname2, out attributeName2, "Attribut2 wählen: ")) return false;
            if (bname1 != bname2)
            {
                _AcAp.Application.ShowAlertDialog("Die Attribute gehören zu unterschiedlichen Blöcken.");
                return false;
            }
            if (attributeName1 == attributeName2)
            {
                _AcAp.Application.ShowAlertDialog("Die Attribute sind identisch.");
                return false;
            }
            blockName = bname1;
            return true;
        }

        private static bool GetAttribute(_AcEd.Editor ed, _AcAp.Document doc, out string bname, out string attributeName, string msg)
        {
            bool found = false;
            bname = string.Empty;
            attributeName = string.Empty;
            _AcEd.PromptNestedEntityResult per = ed.GetNestedEntity("\n" + msg);
            if (per.Status == _AcEd.PromptStatus.OK)
            {
                using (var tr = doc.TransactionManager.StartTransaction())
                {
                    _AcDb.DBObject obj = tr.GetObject(per.ObjectId, _AcDb.OpenMode.ForRead);
                    _AcDb.AttributeReference ar = obj as _AcDb.AttributeReference;
                    if (ar != null && !ar.IsConstant)
                    {
                        _AcDb.BlockReference br = Plan2Ext.Globs.GetBlockFromItsSubentity(tr, per);
                        if (br != null)
                        {
                            bname = Plan2Ext.Globs.GetBlockname(br, tr);
                            attributeName = ar.Tag;
                            ed.WriteMessage(string.Format(CultureInfo.CurrentCulture, "\nBlockname: {0}, Attributname: {1}.", bname, attributeName));
                            found = true;
                        }
                    }
                    tr.Commit();
                }
            }
            return found;
        }
    }
}
