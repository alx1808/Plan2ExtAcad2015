using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Text.RegularExpressions;


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
#elif ARX_APP
using Autodesk.AutoCAD.ApplicationServices;
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
using _AcLm = Autodesk.AutoCAD.LayerManager;
#endif

namespace Plan2Ext.Massenbefehle
{
    public class SetAttValuesBulkClass
    {
        #region log4net Initialization
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Convert.ToString((typeof(SetAttValuesBulkClass))));
        #endregion

        private static string _BlockName = string.Empty;
        private static string _AttributeName = string.Empty;
        private static string _AttributeValue = string.Empty;
        private static List<string> _UsedXrefs = new List<string>();
        private static List<string> _ErrorWithDwgs = new List<string>();
        private static int _NrOfChangedAttributes = 0;

        /// <summary>
        /// Wie Plan2SetAttValues aber in aktueller Zeichnung
        /// </summary>
        [_AcTrx.CommandMethod("Plan2SetAttValues")]
        static public void Plan2SetAttValues()
        {
            try
            {
                log.Info("----------------------------------------------------------------------------------");
                log.Info("Plan2SetAttValues");

                string dirName = string.Empty;
                _UsedXrefs.Clear();
                _ErrorWithDwgs.Clear();
                _NrOfChangedAttributes = 0;
                _BlockName = string.Empty;
                _AttributeName = string.Empty;
                _AttributeValue = string.Empty;

                var doc = Application.DocumentManager.MdiActiveDocument;
                _AcEd.Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
                _AcDb.Database db = doc.Database;

                if (!GetAttributeInfos(ed, doc)) return;

                log.Info("----------------------------------------------------------------------------------");
                log.Info(string.Format(CultureInfo.CurrentCulture, "Zeichnung {0}", doc.Name));

                Plan2Ext.Globs.UnlockAllLayers();

                // main part
                bool ok = SetAttValues(db);

                string resultMsg = string.Format(CultureInfo.CurrentCulture, "Anzahl geänderter Attribute: {0}", _NrOfChangedAttributes.ToString());
                log.Info(resultMsg);
                System.Windows.Forms.MessageBox.Show(resultMsg, "Plan2SetAttValues");
            }
            catch (System.Exception ex)
            {
                string msg = string.Format(CultureInfo.CurrentCulture, "{0}", ex.Message);
                log.Error(msg, ex);
                System.Windows.Forms.MessageBox.Show(msg, "Plan2SetAttValues");
            }
        }

        [_AcTrx.CommandMethod("Plan2SetAttValuesBulk", _AcTrx.CommandFlags.Session)]
        static public void Plan2SetAttValuesBulk()
        {
            try
            {
                log.Info("----------------------------------------------------------------------------------");
                log.Info("Plan2SetAttValuesBulk");

                _UsedXrefs.Clear();
                _ErrorWithDwgs.Clear();
                _NrOfChangedAttributes = 0;
                _BlockName = string.Empty;
                _AttributeName = string.Empty;
                _AttributeValue = string.Empty;

                var doc = Application.DocumentManager.MdiActiveDocument;
                _AcEd.Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                if (!GetAttributeInfos(ed, doc)) return;

                string dwgName = doc.Name;
                var dwgProposal = System.IO.Path.GetDirectoryName(dwgName);

                string dirName = string.Empty;
                string[] dwgFileNames = null;
                if (!Plan2Ext.Globs.GetMultipleFileNames("AutoCAD-Zeichnung", "Dwg", "Verzeichnis mit Zeichnungen für die Umbenennung der Attributwerte", "Zeichnungen für die Umbenennung der Attributwerte", ref dwgFileNames, ref dirName, defaultPath: dwgProposal))
                {
                    return;
                }
                doc.CloseAndDiscard();

                List<string> saveFileNotPossible = new List<string>();
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
                    doc = Application.DocumentManager.MdiActiveDocument;
                    _AcDb.Database db = doc.Database;

                    // Lock the new document
                    using (DocumentLock acLckDoc = doc.LockDocument())
                    {
                        ed = Application.DocumentManager.MdiActiveDocument.Editor;
                        Plan2Ext.Globs.UnlockAllLayers();

                        // main part
                        ok = SetAttValues(db);
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
                        _ErrorWithDwgs.Add(fileName);
                        log.Info(string.Format(CultureInfo.CurrentCulture, "Zeichnung schließen ohne zu speichern: {0}", fileName));
                        doc.CloseAndDiscard();
                    }
                }

                if (saveFileNotPossible.Count > 0)
                {
                    var names = saveFileNotPossible.Select(x => System.IO.Path.GetFileName(x)).ToArray();
                    var allNames = string.Join(", ", names);

                    System.Windows.Forms.MessageBox.Show(string.Format("Folgende Zeichnungen konnen nicht geändert werden: {0}", allNames), "Plan2SetAttValuesBulk");
                }

                ShowResultMessage(dirName.ToUpperInvariant());
            }
            catch (System.Exception ex)
            {
                string msg = string.Format(CultureInfo.CurrentCulture, "{0}", ex.Message);
                System.Windows.Forms.MessageBox.Show(msg, "Plan2SetAttValuesBulk");
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
            string resultMsg = string.Format(CultureInfo.CurrentCulture, "Anzahl geänderter Attribute: {0}\n{2}\n{1}", _NrOfChangedAttributes.ToString(), xrefMsg, errorsWithDwgsMsg);
            log.Info(resultMsg);
            System.Windows.Forms.MessageBox.Show(resultMsg, "Plan2SetAttValuesBulk");
        }

        private static string[] UpperCaseIt(string[] paths)
        {
            return paths.Select(x => x.ToUpperInvariant()).ToArray();
        }

        private static void GetAllXRefFullPaths(string[] files, List<string> usedXRefFullPaths)
        {
            foreach (var fileName in files)
            {
                _AcDb.Database db = new _AcDb.Database(false, true);
                using (db)
                {
                    db.ReadDwgFile(fileName, System.IO.FileShare.Read, allowCPConversion: false, password: "");

                    using (_AcDb.Transaction tr = db.TransactionManager.StartTransaction())
                    {
                        _AcDb.BlockTable bt = (_AcDb.BlockTable)tr.GetObject(db.BlockTableId, _AcDb.OpenMode.ForRead);
                        foreach (var id in bt)
                        {
                            _AcDb.BlockTableRecord btr = (_AcDb.BlockTableRecord)tr.GetObject(id, _AcDb.OpenMode.ForRead);
                            if (btr.IsLayout)
                            {
                                foreach (var oid in btr)
                                {
                                    var ent = tr.GetObject(oid, _AcDb.OpenMode.ForRead);
                                    _AcDb.BlockReference bref = ent as _AcDb.BlockReference;
                                    if (bref != null)
                                    {
                                        if (bref.BlockTableRecord == default(_AcDb.ObjectId))
                                        {
                                            try
                                            {
                                                log.WarnFormat(CultureInfo.CurrentCulture, "Block in '{0}' hat keinen Blocktable-Record. Handle = {1}", bref.BlockName, bref.Handle);
                                            }
                                            catch (Exception ex)
                                            {
                                                log.Error(ex.Message, ex);
                                            }
                                        }
                                        else
                                        {
                                            var bd = (_AcDb.BlockTableRecord)tr.GetObject(bref.BlockTableRecord, _AcDb.OpenMode.ForRead);
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

        private static bool SetAttValues(_AcDb.Database db)
        {
            bool ok = true;
            try
            {
                using (_AcDb.Transaction tr = db.TransactionManager.StartTransaction())
                {
                    _AcDb.BlockTable bt = (_AcDb.BlockTable)tr.GetObject(db.BlockTableId, _AcDb.OpenMode.ForRead);
                    foreach (var id in bt)
                    {
                        _AcDb.BlockTableRecord btr = (_AcDb.BlockTableRecord)tr.GetObject(id, _AcDb.OpenMode.ForRead);
                        if (btr.IsLayout)
                        {
                            foreach (var oid in btr)
                            {
                                var ent = tr.GetObject(oid, _AcDb.OpenMode.ForRead);
                                _AcDb.BlockReference bref = ent as _AcDb.BlockReference;
                                if (bref != null)
                                {
                                    bool sameName = false;
                                    try
                                    {
                                        sameName = (bref.Name == _BlockName);
                                    }
                                    catch (Exception)
                                    {
                                        log.WarnFormat("Invalid block with handle {0}!", bref.Handle);
                                    }
                                    if (sameName)
                                    {
                                        SetAttValues(bref, tr);
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
                log.Error(string.Format(CultureInfo.CurrentCulture, "Fehler beim Ändern der Attribute: {0}", ex.Message), ex);
                ok = false;

            }
            return ok;
        }

        private static void SetAttValues(_AcDb.BlockReference bref, _AcDb.Transaction tr)
        {
            if (Globs.IsXref(bref, tr)) return;

            var atts = GetAttributEntities(bref, tr);
            foreach (var att in atts)
            {
                if (att.Tag == _AttributeName)
                {
                    if (att.TextString == _AttributeValue) break;
                    att.UpgradeOpen();
                    att.TextString = _AttributeValue;
                    _NrOfChangedAttributes++;
                }
            }
        }

        public static List<_AcDb.AttributeReference> GetAttributEntities(_AcDb.BlockReference blockRef, _AcDb.Transaction tr)
        {
            var atts = new List<_AcDb.AttributeReference>();

            foreach (_AcDb.ObjectId attId in blockRef.AttributeCollection)
            {
                var anyAttRef = tr.GetObject(attId, _AcDb.OpenMode.ForRead) as _AcDb.AttributeReference;
                if (anyAttRef != null)
                {
                    atts.Add(anyAttRef);
                }
            }
            return atts;
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

        private static bool GetAttributeInfos(_AcEd.Editor ed, _AcAp.Document doc)
        {
            bool ok = false;
            _BlockName = string.Empty;
            _AttributeName = string.Empty;
            _AttributeValue = string.Empty;

            _AcEd.PromptNestedEntityResult per = ed.GetNestedEntity("\nZu änderndes Attribut wählen: ");

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
                            ed.WriteMessage(string.Format(CultureInfo.CurrentCulture, "\nBlockname: {0}, Attributname: {1}.", Plan2Ext.Globs.GetBlockname(br, tr), ar.Tag));
                            var prompt = new _AcEd.PromptStringOptions("\nText in Attribut: ");
                            prompt.AllowSpaces = true;
                            var prefixUserRes = ed.GetString(prompt);
                            if (prefixUserRes.Status == _AcEd.PromptStatus.OK)
                            {
                                _AttributeValue = prefixUserRes.StringResult;
                                _AttributeName = ar.Tag;
                                _BlockName = Plan2Ext.Globs.GetBlockname(br, tr);

                                log.InfoFormat(CultureInfo.CurrentCulture, "Block: {0}, Attribut: {1}, Wert: '{2}'", _BlockName, _AttributeName, _AttributeValue);

                                ok = true;
                            }
                        }
                    }

                    tr.Commit();
                }
            }
            return ok;
        }
    }
}

