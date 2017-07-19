using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Excel = Microsoft.Office.Interop.Excel;

namespace Plan2Ext.BlockToExcel
{
    internal class Excelizer : IDisposable
    {
        #region log4net Initialization
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Convert.ToString((typeof(Excelizer))));
        #endregion

        private string _FileName = string.Empty;
        private Excel.Application _MyApp = null;
        private Excel.Workbook _WorkBook = null;
        private Excel.Worksheet _Sheet = null;
        private Dictionary<string, int> _IndexPerColHeader = new Dictionary<string, int>();

        public int NrCols { get; set; }
        public int NrRows { get; set; }

        public enum Direction
        {
            Export,
            Import,
        }

        public Excelizer(string fileName,Direction direct)
        {
            _FileName = fileName;
            _MyApp = new Excel.Application();

            if (File.Exists(fileName))
            {
                bool readOnly = true;
                if (direct == Direction.Export) readOnly = false;

                _WorkBook = _MyApp.Workbooks.Open(fileName, Missing.Value, ReadOnly: readOnly); //, 5, "", "", true, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);
                _Sheet = _WorkBook.Worksheets.get_Item(1);

                int nrRows, nrCols;
                GetNrRowsCols(_Sheet, out nrRows, out nrCols);
                NrCols = nrCols;
                NrRows = nrRows;

                ReadExistingHeaders();
            }
            else
            {
                _WorkBook = _MyApp.Workbooks.Add(Missing.Value);
                _Sheet = _WorkBook.ActiveSheet;
            }
        }

        private void ReadExistingHeaders()
        {
            if (NrCols == 0) throw new InvalidOperationException("Es wurden keine Spalten gefunden!");
            var b1 = GetCellBez(0, 0);
            var b2 = GetCellBez(0, NrCols-1);
            var range = _Sheet.Range[b1, b2];
            object[,] impMatrix = range.get_Value(Excel.XlRangeValueDataType.xlRangeValueDefault);
            for (int i = 1; i <= NrCols; i++)
            {
                var headerBez = impMatrix[1, i].ToString();
                _IndexPerColHeader[headerBez] = i - 1;
            }
        }

        private void AppendHeaders(List<string> headers)
        {
            int nextIndex = 0;
            if (_IndexPerColHeader.Count > 0)
                nextIndex = _IndexPerColHeader.Values.Max()+1;

            foreach (var headerBez in headers)
            {
                if (!_IndexPerColHeader.ContainsKey(headerBez))
                {
                    _IndexPerColHeader.Add(headerBez, nextIndex);
                    _Sheet.Cells[1, nextIndex + 1] = headerBez;
                    nextIndex++;
                }
            }
        }

        private void GetNrRowsCols(Excel.Worksheet sheet, out int nrRows, out int nrCols)
        {
            GetNrCols(sheet, out nrCols);
            GetNrRows(sheet, out nrRows);
        }

        private const int MAXCOLS = 50;
        private const int MAXROWS = 1000000;
        private void GetNrRows(Excel.Worksheet sheet, out int nrRows)
        {
            nrRows = 0;

            var b1 = GetCellBez(0, 0);
            var b2 = GetCellBez(MAXROWS, 0);
            var range = sheet.Range[b1, b2];

            object[,] indexMatrix = range.get_Value(Excel.XlRangeValueDataType.xlRangeValueDefault);

            for (int i = 1; i <= MAXROWS; i++)
            {
                var v1 = indexMatrix[i, 1];
                //if (v1 == null) break;
                //nrRows++;
                if (v1 != null) nrRows = i;
            }

        }

        private void GetNrCols(Excel.Worksheet sheet, out int nrCols)
        {
            nrCols = 0;

            var b1 = GetCellBez(0, 0);
            var b2 = GetCellBez(0, MAXCOLS);
            var range = sheet.Range[b1, b2];

            object[,] indexMatrix = range.get_Value(Excel.XlRangeValueDataType.xlRangeValueDefault);

            for (int i = 1; i <= MAXCOLS; i++)
            {
                var v1 = indexMatrix[1, i];
                if (v1 == null) break;
                nrCols++;
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

        public void Dispose()
        {
            if (_WorkBook != null) _WorkBook.Close(false, Missing.Value, Missing.Value);
            _MyApp.Quit();

            releaseObject(_Sheet);
            releaseObject(_WorkBook);
            releaseObject(_MyApp);

        }

        private void releaseObject(object obj)
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

        internal class ImportedRow
        {
            public string DwgPath { get { return Values[IndexPerHeader[BlockToExcel.DWGPATH]]; } }
            public string BlockName { get { return Values[IndexPerHeader[BlockToExcel.BLOCK_NAME]]; } }
            public string Handle { get { return Values[IndexPerHeader[BlockToExcel.HANDLE]]; } }
            public static Dictionary<string, int> IndexPerHeader { get; set; }
            public List<string> Values { get; set; }

            internal string GetAttribute(string attName)
            {
                foreach (var kvp in IndexPerHeader)
                {
                    if (string.Compare(kvp.Key, attName, StringComparison.OrdinalIgnoreCase)== 0)
                    {
                        return Values[IndexPerHeader[kvp.Key]];
                    }
                }
                return null;
            }
        }

        internal List<ImportedRow> Import()
        {
            string b1, b2;
            Excel.Range range;
            // test import
            b1 = GetCellBez(1, 0);
            b2 = GetCellBez(NrRows-1, NrCols-1);
            range = _Sheet.Range[b1, b2];
            object[,] impMatrix = range.get_Value(Excel.XlRangeValueDataType.xlRangeValueDefault);

            ImportedRow.IndexPerHeader = _IndexPerColHeader;
            List<ImportedRow> importedRows = new List<ImportedRow>();
            int nrOfValRows = NrRows -1;
            for (int i = 1; i <= nrOfValRows;i++)
            {
                List<string> values = new List<string>();
                for (int j = 1; j <= NrCols; j++)
                {
                    var val = impMatrix[i, j] ?? "";
                    values.Add(val.ToString());
                }
                importedRows.Add(new ImportedRow() { Values = values });

            }

            return importedRows;
        }

        internal void Export(Dictionary<string, List<string>> valuesPerColumn)
        {
            // Pull in all the cells of the worksheet
            Excel.Range cells = _Sheet.Cells;
            // set each cell's format to Text
            cells.NumberFormat = "@";
            //cells.HorizontalAlignment = XlHAlign.xlHAlignRight;

            var colLists = valuesPerColumn.Values.ToList();

            var b1 = GetCellBez(0, 0);

            AppendHeaders(valuesPerColumn.Keys.ToList());
            var b2 = GetCellBez(0, _IndexPerColHeader.Count-1);
            var range = _Sheet.Range[b1, b2];
            range.Font.Bold = true;

            // header added (new excel file)
            if (NrRows == 0) NrRows = 1;

            int rowCount = colLists[0].Count;
            int colCount = _IndexPerColHeader.Count;
            b1 = GetCellBez(NrRows,0);
            b2 = GetCellBez(NrRows + rowCount - 1, colCount - 1);
            range = _Sheet.Range[b1, b2];
            range.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;

            string[,] indexMatrix = new string[rowCount, colCount];
            foreach (var kvp in valuesPerColumn)
            {
                string headerBez = kvp.Key;
                var rows = kvp.Value;
                int colIndex = _IndexPerColHeader[headerBez];
                for (int rowCnt = 0; rowCnt < rows.Count; rowCnt++)
                {
                    indexMatrix[rowCnt, colIndex] = rows[rowCnt];
                }
            }
            range.set_Value(Excel.XlRangeValueDataType.xlRangeValueDefault, indexMatrix);

            range.Font.Name = "Arial";
            range.Columns.AutoFit();

            if (File.Exists(_FileName))
                _WorkBook.Save();
            else
                _WorkBook.SaveAs(_FileName);

        }
    }
}
