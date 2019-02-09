using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Excel = Microsoft.Office.Interop.Excel;
// ReSharper disable IdentifierTypo

namespace Plan2Ext.BlockInfo
{
    internal class Excelizer
    {

        internal void ExcelExport(string[] header, IEnumerable<IRowProvider> rowProviders)
        {
            if (rowProviders == null) return;
            var rows = rowProviders.ToArray();
            Excel.Application myApp = null;
            Excel.Workbook workBook = null;
            Excel.Worksheet sheet = null;

            try
            {
                myApp = new Excel.Application();

                workBook = myApp.Workbooks.Add(Missing.Value);
                sheet = workBook.ActiveSheet;

                Excel.Range cells = sheet.Cells;
                cells.NumberFormat = "@";
                var b1 = Globs.GetCellBez(0, 0);
                var b2 = Globs.GetCellBez(0, header.Length);
                var range = sheet.Range[b1, b2];
                range.Font.Bold = true;
                range.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;

                var rowCount = 1 + rows.Length;
                var colCount = header.Length;
                b2 = Globs.GetCellBez(rowCount - 1, colCount - 1);
                range = sheet.Range[b1, b2];

                string[,] indexMatrix = new string[rowCount, colCount];
                for (var i = 0; i < header.Length; i++)
                {
                    indexMatrix[0, i] = header[i];
                }
                for (var r = 1; r <= rows.Length; r++)
                {
                    var blockInfo = rows[r - 1];
                    var values = blockInfo.RowValues().ToArray();
                    for (var i = 0; i < values.Length; i++)
                    {
                        indexMatrix[r, i] = values[i];
                    }
                }

                // ReSharper disable once UseIndexedProperty
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

                Globs.FinalReleaseComObject(sheet);
                Globs.FinalReleaseComObject(workBook);
                Globs.FinalReleaseComObject(myApp);
            }
        }
    }
}
