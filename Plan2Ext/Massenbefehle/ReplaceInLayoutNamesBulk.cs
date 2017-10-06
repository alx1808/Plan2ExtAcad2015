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
    public class ReplaceInLayoutNamesBulk
    {
        #region log4net Initialization
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Convert.ToString((typeof(Plan2ReplaceTextsClass))));
        #endregion

        #region Member variables
        private static string _OldText = string.Empty;
        private static string _NewText = string.Empty;
        private static _AcDb.Transaction _Tr = null;
        private static _AcDb.Database _Db = null;
        private static int _NrOfReplacedTexts = 0;

        #endregion

        [_AcTrx.CommandMethod("Plan2ReplaceInLayoutNamesBulk", _AcTrx.CommandFlags.Session)]
        static public void Plan2ReplaceInLayoutNamesBulk()
        {
            try
            {
                log.Info("----------------------------------------------------------------------------------");
                log.Info("Plan2ReplaceInLayoutNamesBulk");

                _NrOfReplacedTexts = 0;
                _OldText = "";
                _NewText = "";
                List<string> saveNotPossible = new List<string>();

                _AcEd.Editor ed  = _AcAp.Application.DocumentManager.MdiActiveDocument.Editor;

                if (!GetOldText(ed)) return;
                if (!GetNewText(ed)) return;
                log.Info(string.Format(CultureInfo.CurrentCulture, "Ersetzung: '{0}' -> '{1}'.", _OldText, _NewText));

                string dirName = string.Empty;
                string[] dwgFileNames = null;
                if (!Plan2Ext.Globs.GetMultipleFileNames("AutoCAD-Zeichnung", "Dwg", "Verzeichnis mit Zeichnungen für die Umbenennung der Layouts", "Zeichnungen für die Umbenennung der Layouts", ref dwgFileNames, ref dirName))
                {
                    return;
                }
                foreach (var fileName in dwgFileNames)
                {
                    SetReadOnlyAttribute(fileName, false);

                    bool ok = false;

                    log.Info("----------------------------------------------------------------------------------");
                    log.Info(string.Format(CultureInfo.CurrentCulture, "Öffne Zeichnung {0}", fileName));

                    _AcAp.Application.DocumentManager.Open(fileName, false);
                    _AcAp.Document doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
                    _AcDb.Database db = doc.Database;

                     //Lock the new document
                    using (DocumentLock acLckDoc = doc.LockDocument())
                    {
                        // main part
                        var layoutNames = Plan2Ext.Layouts.GetLayoutNames();
                        layoutNames = layoutNames.Where(x => string.Compare(x, "Model", StringComparison.OrdinalIgnoreCase) != 0).ToList();

                        _Tr = db.TransactionManager.StartTransaction();
                        using (_Tr)
                        {
                            _AcDb.LayoutManager layoutMgr = _AcDb.LayoutManager.Current;

                            foreach (var name in layoutNames)
                            {
                                bool changed;
                                var newT = ReplaceTexts(name, out changed);
                                if (changed)
                                {
                                    layoutMgr.RenameLayout(name, newT);
                                    _NrOfReplacedTexts++;
                                    ok = true;
                                }
                            }

                            _Tr.Commit();
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
                            saveNotPossible.Add(fileName);
                            doc.CloseAndDiscard();
                        }
                    }
                    else
                    {
                        log.Info(string.Format(CultureInfo.CurrentCulture, "Zeichnung schließen ohne zu speichern da keine Layouts geändert wurden: {0}", fileName));
                        doc.CloseAndDiscard();
                    }
                }

                if (saveNotPossible.Count > 0)
                {
                    var names = saveNotPossible.Select(x => System.IO.Path.GetFileName(x)).ToArray();
                    var allNames = string.Join(", ", names);

                    System.Windows.Forms.MessageBox.Show(string.Format("Folgende Zeichnungen konnen nicht gespeichert werden: {0}", allNames), "Plan2ReplaceInLayoutNamesBulk");
                }

                ShowResultMessage(dirName.ToUpperInvariant());
            }
            catch (System.Exception ex)
            {
                string msg = string.Format(CultureInfo.CurrentCulture, "{0}", ex.Message);
                System.Windows.Forms.MessageBox.Show(msg, "Plan2ReplaceInLayoutNamesBulk");
            }
        }

        private static void ShowResultMessage(string dirNameU)
        {
            string resultMsg = string.Format(CultureInfo.CurrentCulture, "Anzahl ersetzter Texte: {0}", _NrOfReplacedTexts.ToString());
            log.Info(resultMsg);
            System.Windows.Forms.MessageBox.Show(resultMsg, "Plan2ReplaceInLayoutNamesBulk");
        }

        private static string[] UpperCaseIt(string[] paths)
        {
            return paths.Select(x => x.ToUpperInvariant()).ToArray();
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

        private static string ReplaceTexts(string txt, out bool changed)
        {
            var newT = Regex.Replace(txt, _OldText, _NewText, RegexOptions.IgnoreCase);
            //var newT = txt.Replace(_OldText, _NewText);
            if (string.Compare(newT, txt, StringComparison.OrdinalIgnoreCase) == 0) changed = false;
            else changed = true;
            return newT;
        }

        private static bool GetNewText(_AcEd.Editor ed)
        {
            var prompt = new _AcEd.PromptStringOptions("\nNeuer Text: ");
            prompt.AllowSpaces = true;
            var prefixUserRes = ed.GetString(prompt);
            if (prefixUserRes.Status != _AcEd.PromptStatus.OK)
            {
                return false;
            }
            _NewText = prefixUserRes.StringResult;
            return true;
        }

        private static bool GetOldText(_AcEd.Editor ed)
        {
            var prompt = new _AcEd.PromptStringOptions("\nZu ersetzender Text: ");
            prompt.AllowSpaces = true;
            while (string.IsNullOrEmpty(_OldText))
            {
                var prefixUserRes = ed.GetString(prompt);
                if (prefixUserRes.Status != _AcEd.PromptStatus.OK)
                {
                    return false;
                }
                _OldText = prefixUserRes.StringResult;
            }
            return true;
        }
    }
}
