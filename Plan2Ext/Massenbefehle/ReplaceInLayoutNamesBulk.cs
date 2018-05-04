using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using Plan2Ext.AutoIdVergabe;
using Excel = Microsoft.Office.Interop.Excel;

namespace Plan2Ext.Massenbefehle
{
    public class ReplaceInLayoutNamesBulk
    {
        #region log4net Initialization
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(Convert.ToString((typeof(Plan2ReplaceTextsClass))));
        #endregion

        #region Member variables
        private static string _OldText = string.Empty;
        private static string _NewText = string.Empty;
        private static _AcDb.Transaction _Tr = null;
        private static _AcDb.Database _Db = null;
        private static int _NrOfReplacedTexts = 0;

        #endregion

        [_AcTrx.CommandMethod("Plan2ImportLayoutNamesToExcelBulk", _AcTrx.CommandFlags.Session)]
        static public void Plan2ImportLayoutNamesToExcelBulk()
        {
            try
            {
                log.Info("----------------------------------------------------------------------------------");
                log.Info("Plan2ImportLayoutNamesToExcelBulk");
                List<string> saveNotPossible = new List<string>();
                _NrOfReplacedTexts = 0;
                bool isWarn = false;

                _AcWnd.OpenFileDialog ofd = new _AcWnd.OpenFileDialog("Excel-Import-Datei", "", "xlsx", "Plan2ImportLayoutNamesToExcelBulk", _AcWnd.OpenFileDialog.OpenFileDialogFlags.AllowAnyExtension);
                System.Windows.Forms.DialogResult dr = ofd.ShowDialog();
                if (dr != System.Windows.Forms.DialogResult.OK) return;
                var excelFileName = ofd.Filename;

                var colsForExcel = Import(excelFileName, 3);
                if (!colsForExcel.ContainsKey(dwgHeaderName)) throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Es gibt keine Spalte '{0}'!", dwgHeaderName));
                if (!colsForExcel.ContainsKey(layoutHeaderName)) throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Es gibt keine Spalte '{0}'!", layoutHeaderName));
                if (!colsForExcel.ContainsKey(layoutNewHeaderName)) throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Es gibt keine Spalte '{0}'!", layoutNewHeaderName));

                var layoutsPerDwg = new Dictionary<string, Dictionary<string, string>>();
                GetLayoutsPerDwg(colsForExcel, layoutsPerDwg);

                _AcEd.Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                foreach (var fileName in layoutsPerDwg.Keys)
                {
                    bool ok = false;
                    if (!System.IO.File.Exists(fileName))
                    {
                        log.WarnFormat(CultureInfo.CurrentCulture, "Dwg '{0}' existiert nicht!", fileName);
                        isWarn = true;
                        continue;
                    }

                    var loNameDict = layoutsPerDwg[fileName];

                    // ignore dwgs where nothing is to do
                    bool allSame = true;
                    foreach (var kvp in loNameDict)
                    {
                        if (kvp.Key != kvp.Value)
                        {
                            allSame = false;
                            break;
                        }
                    }

                    if (allSame) continue;

                    SetReadOnlyAttribute(fileName, false);

                    log.Info("----------------------------------------------------------------------------------");
                    log.Info(string.Format(CultureInfo.CurrentCulture, "Öffne Zeichnung {0}", fileName));

                    Application.DocumentManager.Open(fileName, false);
                    Document doc = Application.DocumentManager.MdiActiveDocument;
                    _AcDb.Database db = doc.Database;

                    //Lock the new document
                    using (DocumentLock acLckDoc = doc.LockDocument())
                    {
                        // main part
                        var layoutNames = Layouts.GetLayoutNames();
                        layoutNames = layoutNames.Where(x => string.Compare(x, "Model", StringComparison.OrdinalIgnoreCase) != 0).ToList();

                        _Tr = db.TransactionManager.StartTransaction();
                        using (_Tr)
                        {
                            _AcDb.LayoutManager layoutMgr = _AcDb.LayoutManager.Current;

                            foreach (var name in layoutNames)
                            {
                                string newLoName;
                                if (!loNameDict.TryGetValue(name, out newLoName))
                                {
                                    //log.WarnFormat(CultureInfo.CurrentCulture, "Layout '{0}' nicht gefunden in '{1}'!", name, fileName);
                                    //isWarn = true;
                                    continue;
                                }

                                try
                                {
                                    if (String.CompareOrdinal(name, newLoName) != 0)
                                    {
                                        layoutMgr.RenameLayout(name, newLoName);
                                        log.InfoFormat(CultureInfo.CurrentCulture, "Layout '{0}' umbenannt in '{1}'.",name,newLoName);
                                        ok = true;
                                        _NrOfReplacedTexts++;
                                    }
                                }
                                catch (Exception e)
                                {
                                    log.WarnFormat(CultureInfo.CurrentCulture, "Layout '{0}' konnte nicht umbenannt werden in '{2}' in '{1}'!", name, fileName, newLoName);
                                    isWarn = true;
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
                        catch (Exception ex)
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

                    System.Windows.Forms.MessageBox.Show(string.Format("Folgende Zeichnungen konnen nicht gespeichert werden: {0}", allNames), "Plan2ImportLayoutNamesToExcelBulk");
                }

                ShowResultMessageForExcelImport(isWarn);

            }
            catch (Exception ex)
            {
                string msg = string.Format(CultureInfo.CurrentCulture, "{0}", ex.Message);
                System.Windows.Forms.MessageBox.Show(msg, "Plan2ImportLayoutNamesToExcelBulk");
            }
        }

        private static void ShowResultMessageForExcelImport(bool isWarn)
        {
            string resultMsg = string.Format(CultureInfo.CurrentCulture, "Anzahl ersetzter Layoutnamen: {0}", _NrOfReplacedTexts.ToString());
            if (isWarn) resultMsg += "\n" + "Es sind Fehler aufgetreten. Siehe Logdatei.";
            log.Info(resultMsg);
            System.Windows.Forms.MessageBox.Show(resultMsg, "Plan2ImportLayoutNamesToExcelBulk");
            if (isWarn)
            {
                var logFileName = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "Plan2.log");
                if (System.IO.File.Exists(logFileName))
                {
                    System.Diagnostics.Process.Start(logFileName);
                }
            }
        }

        private static void GetLayoutsPerDwg(Dictionary<string, List<string>> colsForExcel, Dictionary<string, Dictionary<string, string>> layoutsPerDwg)
        {
            var lstDwgNames = colsForExcel[dwgHeaderName];
            var lstLayoutNames = colsForExcel[layoutHeaderName];
            var lstNewLayoutNames = colsForExcel[layoutNewHeaderName];

            for (int i = 0; i < lstDwgNames.Count; i++)
            {
                var dwgName = lstDwgNames[i];
                Dictionary<string, string> dic;
                if (!layoutsPerDwg.TryGetValue(dwgName, out dic))
                {
                    dic = new Dictionary<string, string>();
                    layoutsPerDwg.Add(dwgName, dic);
                }

                dic[lstLayoutNames[i]] = lstNewLayoutNames[i];
            }
        }


        [_AcTrx.CommandMethod("Plan2ExportLayoutNamesToExcelBulk", _AcTrx.CommandFlags.Session)]
        static public void Plan2ExportLayoutNamesToExcelBulk()
        {
            try
            {
                log.Info("----------------------------------------------------------------------------------");
                log.Info("Plan2ExportLayoutNamesToExcelBulk");


                var layoutsPerDwg = new Dictionary<string, List<string>>();

                _AcEd.Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;


                string dirName = string.Empty;
                string[] dwgFileNames = null;
                if (!Globs.GetMultipleFileNames("AutoCAD-Zeichnung", "Dwg", "Verzeichnis mit Zeichnungen für die Umbenennung der Layouts", "Zeichnungen für die Umbenennung der Layouts", ref dwgFileNames, ref dirName))
                {
                    return;
                }
                foreach (var fileName in dwgFileNames)
                {
                    bool ok = false;

                    log.Info("----------------------------------------------------------------------------------");
                    log.Info(string.Format(CultureInfo.CurrentCulture, "Öffne Zeichnung {0}", fileName));

                    Application.DocumentManager.Open(fileName, true);
                    Document doc = Application.DocumentManager.MdiActiveDocument;
                    _AcDb.Database db = doc.Database;

                    //Lock the new document
                    using (DocumentLock acLckDoc = doc.LockDocument())
                    {
                        // main part
                        var layoutNames = Layouts.GetLayoutNames();
                        layoutNames = layoutNames.Where(x => string.Compare(x, "Model", StringComparison.OrdinalIgnoreCase) != 0).ToList();

                        layoutsPerDwg[fileName] = layoutNames;
                        log.Info(string.Format(CultureInfo.CurrentCulture, "Anzahl von Layout ins '{0}': {1}", fileName, layoutNames.Count()));
                    }

                    doc.CloseAndDiscard();
                }

                string excelFileName = "";
                var colsForExcel = CreateColsForExcel(layoutsPerDwg);
                ExcelExport(excelFileName, colsForExcel);

            }
            catch (Exception ex)
            {
                string msg = string.Format(CultureInfo.CurrentCulture, "{0}", ex.Message);
                System.Windows.Forms.MessageBox.Show(msg, "Plan2ExportLayoutNamesToExcelBulk");
            }
        }

        private static bool ExcelExport(string excelFileName, Dictionary<string, List<string>> colsForExcel)
        {
            if (colsForExcel.Count == 0) return true;
            using (var excelizer = new BlockToExcel.Excelizer(excelFileName, BlockToExcel.Excelizer.Direction.Export))
            {
                Export(colsForExcel);
            }

            return true;
        }

        #region Excelizer

        const string dwgHeaderName = "Dwgname";
        const string layoutHeaderName = "Layout";
        const string layoutNewHeaderName = "Layout New";

        private static Dictionary<string, List<string>> Import(string fileName, int minNrOfCols)
        {
            var colsForExcel = new Dictionary<string, List<string>>();

            Excel.Application myApp = new Excel.Application();
            Excel.Workbook workBook = null;
            Excel.Worksheet sheet = null;
            try
            {
                workBook = myApp.Workbooks.Open(fileName, Missing.Value, true);
                // ReSharper disable once UseIndexedProperty
                sheet = workBook.Worksheets.get_Item(1);

                int nrRows, nrCols;
                GetNrRowsCols(sheet, out nrRows, out nrCols);
                if (nrCols < minNrOfCols) throw new InvalidOperationException(string.Format("Ungültige Anzahl von Spalten! {0}", nrCols));
                nrCols = minNrOfCols;
                var b1 = GetCellBez(0, 0);
                var b2 = GetCellBez(nrRows, nrCols);
                var range = sheet.Range[b1, b2];
                // ReSharper disable once UseIndexedProperty
                object[,] impMatrix = range.get_Value(Excel.XlRangeValueDataType.xlRangeValueDefault);

                // get headers
                var lists = new List<List<string>>();
                for (int i = 1; i <= minNrOfCols; i++)
                {
                    colsForExcel[impMatrix[1, i].ToString()] = new List<string>();
                    lists.Add(new List<string>());
                }

                for (int r = 2; r <= nrRows; r++)
                {
                    for (int i = 0; i < minNrOfCols; i++)
                    {
                        var val = impMatrix[r, i + 1];
                        if (val == null)
                        {
                            throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Ungültiger Wert in Exceldatei in Spalte {0}, Reihe {1}!", i+1,r));
                        }
                        lists[i].Add(val.ToString());
                    }
                }

                int j = 0;
                var headerNames = colsForExcel.Keys.ToList();
                foreach (var headerName in headerNames)
                {
                    colsForExcel[headerName] = lists[j];
                    j++;
                }

                workBook.Close(false, Missing.Value, Missing.Value);
                myApp.Quit();
            }
            finally
            {
                ReleaseObject(sheet);
                ReleaseObject(workBook);
                ReleaseObject(myApp);
            }

            return colsForExcel;
        }


        internal static void Export(Dictionary<string, List<string>> valuesPerColumn)
        {
            Excel.Application myApp = null;
            Excel.Workbook workBook = null;
            Excel.Worksheet sheet = null;
            try
            {
                myApp = new Excel.Application();
                workBook = myApp.Workbooks.Add(Missing.Value);
                sheet = workBook.ActiveSheet;

                // Pull in all the cells of the worksheet
                Excel.Range cells = sheet.Cells;
                // set each cell's format to Text
                cells.NumberFormat = "@";

                var colLists = valuesPerColumn.Values.ToList();

                var b1 = GetCellBez(0, 0);

                var indexPerColHeader = new Dictionary<string, int>();

                AppendHeaders(valuesPerColumn.Keys.ToList(), indexPerColHeader, sheet);
                var b2 = GetCellBez(0, indexPerColHeader.Count - 1);
                var range = sheet.Range[b1, b2];
                range.Font.Bold = true;

                // header added (new excel file)
                int NrRows = 1;

                int rowCount = colLists[0].Count;
                int colCount = indexPerColHeader.Count;
                b1 = GetCellBez(NrRows, 0);
                b2 = GetCellBez(NrRows + rowCount - 1, colCount - 1);
                range = sheet.Range[b1, b2];
                range.HorizontalAlignment = Excel.XlHAlign.xlHAlignLeft;

                string[,] indexMatrix = new string[rowCount, colCount];
                foreach (var kvp in valuesPerColumn)
                {
                    string headerBez = kvp.Key;
                    var rows = kvp.Value;
                    int colIndex = indexPerColHeader[headerBez];
                    for (int rowCnt = 0; rowCnt < rows.Count; rowCnt++)
                    {
                        indexMatrix[rowCnt, colIndex] = rows[rowCnt];
                    }
                }

                range.set_Value(Excel.XlRangeValueDataType.xlRangeValueDefault, indexMatrix);
                range.Font.Name = "Arial";
                range.Columns.AutoFit();
            }
            finally
            {
                if (myApp != null)
                {
                    myApp.Visible = true;
                    myApp.ScreenUpdating = true;
                }
                ReleaseObject(sheet);
                ReleaseObject(workBook);
                ReleaseObject(myApp);

            }
        }

        protected static void GetNrRowsCols(Excel.Worksheet sheet, out int nrRows, out int nrCols)
        {
            GetNrCols(sheet, out nrCols);
            GetNrRows(sheet, out nrRows);
        }
        private const int Maxcols = 50;
        private const int Maxrows = 3000;
        protected static void GetNrRows(Excel.Worksheet sheet, out int nrRows)
        {
            nrRows = 0;

            var b1 = GetCellBez(0, 0);
            var b2 = GetCellBez(Maxrows, 0);
            var range = sheet.Range[b1, b2];

            // ReSharper disable once UseIndexedProperty
            object[,] indexMatrix = range.get_Value(Excel.XlRangeValueDataType.xlRangeValueDefault);

            for (int i = 1; i <= Maxrows; i++)
            {
                var v1 = indexMatrix[i, 1];
                if (v1 == null) break;
                nrRows++;
            }
        }

        protected static void GetNrCols(Excel.Worksheet sheet, out int nrCols)
        {
            nrCols = 0;

            var b1 = GetCellBez(0, 0);
            var b2 = GetCellBez(0, Maxcols);
            var range = sheet.Range[b1, b2];
            // ReSharper disable once UseIndexedProperty
            object[,] indexMatrix = range.get_Value(Excel.XlRangeValueDataType.xlRangeValueDefault);

            for (int i = 1; i <= Maxcols; i++)
            {
                var v1 = indexMatrix[1, i];
                if (v1 == null) break;
                nrCols++;
            }
        }

        private static void ReleaseObject(object obj)
        {
            try
            {
                if (obj != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(obj);
                obj = null;
            }
            catch (Exception ex)
            {
                obj = null;
            }
            finally
            {
                GC.Collect();
            }
        }

        private static void AppendHeaders(List<string> headers, Dictionary<string, int> indexPerColHeader, Excel.Worksheet sheet)
        {
            int nextIndex = 0;
            if (indexPerColHeader.Count > 0)
                nextIndex = indexPerColHeader.Values.Max() + 1;

            foreach (var headerBez in headers)
            {
                if (!indexPerColHeader.ContainsKey(headerBez))
                {
                    indexPerColHeader.Add(headerBez, nextIndex);
                    sheet.Cells[1, nextIndex + 1] = headerBez;
                    nextIndex++;
                }
            }
        }

        public static string GetCellBez(int rowIndex, int colIndex)
        {
            return TranslateColumnIndexToName(colIndex) + (rowIndex + 1).ToString(CultureInfo.InvariantCulture);
        }

        public static String TranslateColumnIndexToName(int index)
        {
            //assert (index >= 0);

            int quotient = (index) / 26;

            if (quotient > 0)
            {
                return TranslateColumnIndexToName(quotient - 1) + (char)((index % 26) + 65);
            }
            else
            {
                return "" + (char)((index % 26) + 65);
            }
        }
        #endregion

        private static Dictionary<string, List<string>> CreateColsForExcel(Dictionary<string, List<string>> layoutsPerDwg)
        {
            var colsForExcel = new Dictionary<string, List<string>>();
            colsForExcel[dwgHeaderName] = new List<string>();
            colsForExcel[layoutHeaderName] = new List<string>();
            colsForExcel[layoutNewHeaderName] = new List<string>();

            foreach (var kvp in layoutsPerDwg)
            {
                var dwgName = kvp.Key;
                var layoutNames = kvp.Value;
                foreach (var layoutName in layoutNames)
                {
                    colsForExcel[dwgHeaderName].Add(dwgName);
                    colsForExcel[layoutHeaderName].Add(layoutName);
                    colsForExcel[layoutNewHeaderName].Add(layoutName);
                }
            }

            return colsForExcel;
        }


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

                _AcEd.Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                if (!GetOldText(ed)) return;
                if (!GetNewText(ed)) return;
                log.Info(string.Format(CultureInfo.CurrentCulture, "Ersetzung: '{0}' -> '{1}'.", _OldText, _NewText));

                string dirName = string.Empty;
                string[] dwgFileNames = null;
                if (!Globs.GetMultipleFileNames("AutoCAD-Zeichnung", "Dwg", "Verzeichnis mit Zeichnungen für die Umbenennung der Layouts", "Zeichnungen für die Umbenennung der Layouts", ref dwgFileNames, ref dirName))
                {
                    return;
                }
                foreach (var fileName in dwgFileNames)
                {
                    SetReadOnlyAttribute(fileName, false);

                    bool ok = false;

                    log.Info("----------------------------------------------------------------------------------");
                    log.Info(string.Format(CultureInfo.CurrentCulture, "Öffne Zeichnung {0}", fileName));

                    Application.DocumentManager.Open(fileName, false);
                    Document doc = Application.DocumentManager.MdiActiveDocument;
                    _AcDb.Database db = doc.Database;

                    //Lock the new document
                    using (DocumentLock acLckDoc = doc.LockDocument())
                    {
                        // main part
                        var layoutNames = Layouts.GetLayoutNames();
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
                        catch (Exception ex)
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

                ShowResultMessage();
            }
            catch (Exception ex)
            {
                string msg = string.Format(CultureInfo.CurrentCulture, "{0}", ex.Message);
                System.Windows.Forms.MessageBox.Show(msg, "Plan2ReplaceInLayoutNamesBulk");
            }
        }

        private static void ShowResultMessage()
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
