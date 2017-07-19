#if ARX_APP
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Excel = Microsoft.Office.Interop.Excel;

namespace Plan2Ext.AutoIdVergabe
{
    internal class ExcelExport
    {
        #region log4net Initialization
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Convert.ToString((typeof(ExcelExport))));
        #endregion
        internal void Export(Dictionary<string, List<string>> valuesPerColumn)
        {
            var myApp = new Excel.Application();
            try
            {
                var workBook = myApp.Workbooks.Add(Missing.Value);
                Excel.Worksheet sheet = workBook.ActiveSheet;

                // Pull in all the cells of the worksheet
                Excel.Range cells = sheet.Cells;
                // set each cell's format to Text
                cells.NumberFormat = "@";
                //cells.HorizontalAlignment = XlHAlign.xlHAlignRight;

                var colLists = valuesPerColumn.Values.ToList();

                var b1 = GetCellBez(0, 0);
                var b2 = GetCellBez(0, colLists.Count);
                var range = sheet.Range[b1, b2];
                range.Font.Bold = true;

                int rowCount = colLists[0].Count;
                int colCount = colLists.Count;
                b2 = GetCellBez(rowCount - 1, colCount  - 1);
                range = sheet.Range[b1, b2];
                range.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;

                string[,] indexMatrix = new string[rowCount,colCount];
                for (int colCnt = 0; colCnt < colLists.Count; colCnt++)
                {
                    var rows = colLists[colCnt];
                    for (int rowCnt = 0; rowCnt < rows.Count; rowCnt++)
                    {
                        indexMatrix[rowCnt, colCnt] = rows[rowCnt];
                    }
                }
                range.set_Value(Excel.XlRangeValueDataType.xlRangeValueDefault, indexMatrix);

                //for (int colCnt = 0; colCnt < colLists.Count; colCnt++)
                //{
                //    var rows = colLists[colCnt];
                //    for (int rowCnt = 0; rowCnt < rows.Count; rowCnt++)
                //    {
                //        sheet.Cells[rowCnt + 1, colCnt + 1] = rows[rowCnt];
                //    }
                //}


                range.Font.Name = "Arial";
                range.Columns.AutoFit();

            }
            finally 
            {
                myApp.Visible = true;
                myApp.ScreenUpdating = true;
            }


            
        }

        internal List<BlockImportInfo> Import(string fileName)
        {
            Excel.Application myApp = new Excel.Application();
            Excel.Workbook workBook = null;
            Excel.Worksheet sheet = null;
            try
            {
                workBook = myApp.Workbooks.Open(fileName, Missing.Value, true); //, 5, "", "", true, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);
                sheet = workBook.Worksheets.get_Item(1);

                var biis = GetBlockInfos(sheet);

                workBook.Close(false, Missing.Value , Missing.Value );
                myApp.Quit();

                return biis;

            }
            finally
            {
                releaseObject(sheet);
                releaseObject(workBook);
                releaseObject(myApp);
            }
        }

        private void releaseObject(object obj)
        {
            try
            {
                if (obj != null)                System.Runtime.InteropServices.Marshal.ReleaseComObject(obj);
                obj = null;
            }
            catch (Exception ex)
            {
                obj = null;
                //MessageBox.Show("Unable to release the Object " + ex.ToString());
            }
            finally
            {
                GC.Collect();
            }
        } 

        private List<BlockImportInfo> GetBlockInfos(Excel.Worksheet sheet)
        {
            string b1, b2;
            Excel.Range range;
            // test import
            int nrRows, nrCols;
            GetNrRowsCols(sheet, out nrRows, out nrCols);
            if (nrCols == 0) throw new InvalidOperationException("Es wurden keine Spalten gefunden!");
            b1 = GetCellBez(0, 0);
            b2 = GetCellBez(nrRows, nrCols);
            range = sheet.Range[b1, b2];
            object[,] impMatrix = range.get_Value(Excel.XlRangeValueDataType.xlRangeValueDefault);
            string[] headerNames = new string[nrCols];
            int handleIndex = -1, blockNameIndex = -1;
            for (int i = 1; i <= nrCols; i++)
            {
                var headerBez = impMatrix[1, i].ToString();
                headerNames[i - 1] = headerBez;

                if (string.Compare(headerBez.ToString(), Engine.HANDLE, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    handleIndex = i;
                }
                else if (string.Compare(headerBez.ToString(), Engine.BLOCK_NAME, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    blockNameIndex = i;
                }
            }
            if (handleIndex == -1) throw new InvalidOperationException("Es wurde kein HANDLE-Feld gefunden!");
            if (blockNameIndex == -1) throw new InvalidOperationException("Es wurde kein BLOCKNAME-Feld gefunden!");

            List<BlockImportInfo> biis = new List<BlockImportInfo>();
            for (int r = 2; r <= nrRows; r++)
            {
                string handle = string.Empty;
                BlockImportInfo bii = new BlockImportInfo();
                for (int i = 1; i <= nrCols; i++)
                {
                    var val = impMatrix[r, i];
                    if (val != null)
                    {
                        if (i == handleIndex)
                        {
                            bii.Handle = val.ToString();
                        }
                        else if (i != blockNameIndex)
                        {
                            bii.Attributes[headerNames[i - 1]] = val.ToString();
                        }
                    }
                }
                if (string.IsNullOrEmpty(bii.Handle))
                {
                    log.WarnFormat(CultureInfo.InvariantCulture, "Kein Handle in Zeile {0}!", r.ToString());
                    // todo: msgbox am schluss
                }
                else
                {
                    biis.Add(bii);
                }
            }
            return biis;
        }

        internal  class BlockImportInfo
        {
            public string Handle { get; set; }
            private Dictionary<string, string> _Attributes = new Dictionary<string, string>();
            public Dictionary<string, string> Attributes { get { return _Attributes; } }
        }

        private void GetNrRowsCols(Excel.Worksheet sheet, out int nrRows, out int nrCols)
        {
            GetNrCols(sheet, out nrCols);
            GetNrRows(sheet, out nrRows);
            
        }

        private const int MAXCOLS = 50;
        private const int MAXROWS = 3000;
        private void GetNrRows(Excel.Worksheet sheet, out int nrRows)
        {
            nrRows = 0;

            var b1 = GetCellBez(0, 0);
            var b2 = GetCellBez(MAXROWS,0);
            log.DebugFormat("B1 '{0}' und B2 '{1}", b1, b2);
            var range = sheet.Range[b1, b2];
            log.DebugFormat("Nach getrange!");

            object[,] indexMatrix = range.get_Value(Excel.XlRangeValueDataType.xlRangeValueDefault);

            for (int i = 1; i <= MAXROWS; i++)
            {
                var v1 = indexMatrix[i,1];
                if (v1 == null) break;
                nrRows++;
            }

        }

        private void GetNrCols(Excel.Worksheet sheet, out int nrCols)
        {
            nrCols = 0;

            var b1 = GetCellBez(0, 0);
            var b2 = GetCellBez(0, MAXCOLS);
            //b2 = "IT1";
            log.DebugFormat("B1 '{0}' und B2 '{1}", b1, b2);
            var range = sheet.Range[b1, b2];
            log.DebugFormat("Nach getrange!");

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
            return TranslateColumnIndexToName(colIndex) + (rowIndex+1).ToString(CultureInfo.InvariantCulture);
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


    }
}
#endif