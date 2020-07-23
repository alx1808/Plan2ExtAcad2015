using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Plan2Ext.Excelx;
using Excel = Microsoft.Office.Interop.Excel;


#if BRX_APP
using Bricscad.ApplicationServices;

#elif ARX_APP
using Autodesk.AutoCAD.ApplicationServices;
#endif

namespace Plan2Ext.Raumnummern.ExcelExport
{
    internal class ExcelExporter
    {
        private static readonly string TemplateFileName = "aufstellung_template.xlsx";
        private static readonly string StylesWorksheetName = "Styles";
        private static readonly string TopWorksheeetName = "Top";

        private const int TOP_NAME_FORMAT_ROW_INDEX = 7;
        private const int TOP_NAME_FORMAT_COL_INDEX = 1;
        private const int TOP_NR_FORMAT_ROW_INDEX = 8;
        private const int TOP_NR_SUM_FORMAT_ROW_INDEX = 31;
        private const int MAX_COL_INDEX = 7;
        private const int SUM_INDEX_START = 5;
        private const int SUM_INDEX_GESAMT = 4;

        public void Export(IExcelExportModel model, IGeschossnameHelper geschossnameHelper, Document doc)
        {
            var projekt = model.ProjectName;
            var app = new Excel.Application();
            try
            {
                if (!Plan2Ext.Globs.FindFile(TemplateFileName, doc.Database, out var template))
                {
                    throw new InvalidOperationException($"Template {TemplateFileName} nicht gefunden1");
                }

                var templateWorkbook = app.Workbooks.Open(template, Missing.Value, true);
                var stylesSheet = GetWorksheet(templateWorkbook, StylesWorksheetName);
                var topTemplateSheet = GetWorksheet(templateWorkbook, TopWorksheeetName);
                var topComparer = new TopComparer();
                var topNrComparer = new TopNrComparer();

                var workBook = app.Workbooks.Add(Missing.Value);
                var sheet = workBook.ActiveSheet;

                var geschosse = model.BlockInfos.GroupBy(x => x.Geschoss);

                var sheetIndex = 1;
                foreach (var geschoss in geschosse)
                {
                    var geschossKurz = geschoss.Key;
                    var geschossLang = geschossnameHelper.Langbez(geschossKurz);

                    topTemplateSheet.Copy(sheet);
                    var targetSheet = GetWorksheet(workBook, sheetIndex);
                    sheetIndex++;
                    targetSheet.Name = geschossKurz.ToUpper();

                    //var nrOfSheets = workBook.Worksheets.Count;

                    var matrix = new ExcelMatrix(1, 7);
                    matrix.Add(2, 1, "Flächenaufstellung " + projekt);
                    CopyCells(stylesSheet, targetSheet, 2, 1, 2, MAX_COL_INDEX, copyColumnWidth: true);
                    matrix.Add(4, 1, geschossLang);
                    matrix.Add(4, 7, "m2");
                    CopyCells(stylesSheet, targetSheet, 4, 1, 4, MAX_COL_INDEX);

                    var currentIndex = 7;
                    var formulars = new List<Formular>();
                    var orderedTops = geschoss.OrderBy(x => x.Top, topComparer);
                    var tops = orderedTops.GroupBy(x => x.Top);
                    foreach (var top in tops)
                    {
                        var topName = top.Key;
                        matrix.Add(currentIndex,1,topName);
                        CopyCells(stylesSheet, targetSheet, TOP_NAME_FORMAT_ROW_INDEX, TOP_NAME_FORMAT_COL_INDEX, TOP_NAME_FORMAT_ROW_INDEX, MAX_COL_INDEX,false, currentIndex);
                        currentIndex++;

                        var firstSumRowIndex = currentIndex;
                        var raueme = top.OrderBy(x => x.Topnr, topNrComparer).ToArray();
                        var unterlinePos = raueme.Length - 1;
                        for (var i = 0; i < raueme.Length; i++)
                        {
                            var raum = raueme[i];
                            matrix.Add(currentIndex,1,raum.Topnr);
                            matrix.Add(currentIndex, 2, raum.Zimmer);
                            matrix.Add(currentIndex, 3, raum.Area);
                            matrix.Add(currentIndex, 4, "m2");
                            if (i == unterlinePos)
                            {
                                CopyCells(stylesSheet, targetSheet, TOP_NR_SUM_FORMAT_ROW_INDEX, 1, TOP_NR_SUM_FORMAT_ROW_INDEX, MAX_COL_INDEX, false, currentIndex);
                                formulars.Add(Formular.Sum1(firstSumRowIndex, 3, currentIndex, 3, currentIndex, 6));
                                matrix.Add(currentIndex, 7, "m2");
                            }
                            else
                            {
                                CopyCells(stylesSheet, targetSheet, TOP_NR_FORMAT_ROW_INDEX, 1, TOP_NR_FORMAT_ROW_INDEX, MAX_COL_INDEX,false, currentIndex);

                            }
                            currentIndex++;
                        }

                        currentIndex += 2;
                    }

                    matrix.Write(targetSheet);
                    foreach (var formular in formulars)
                    {
                        formular.Write(targetSheet);
                    }

                    Formular.Sum1(SUM_INDEX_START, 6, currentIndex, 6, SUM_INDEX_GESAMT, 6).Write(targetSheet);
                }

                sheet.Delete();
                templateWorkbook.Close();
            }
            finally
            {
                app.Visible = true;
                app.ScreenUpdating = true;
                ReleaseObject(app);
            }
        }

        private Excel.Worksheet GetWorksheet(Excel.Workbook woorkbook, object indexOrName)
        {
            try
            {
                return (Excel.Worksheet)woorkbook.Worksheets.Item[indexOrName];
            }
            catch (Exception)
            {
                throw new InvalidOperationException($"Arbeitsblatt {indexOrName} nicht gefunden!");
            }
        }
        private void CopyCells(Excel.Worksheet sourceSheet, Excel.Worksheet targetSheet, int rowBase1Index1, int colBase1Index1, int rowBase1Index2, int colBase1Index2, bool copyColumnWidth = false, int targetBase1RowIndex1 = 0)
        {
            var rowIndex1 = rowBase1Index1 - 1;
            var colIndex1 = colBase1Index1 - 1;
            var rowIndex2 = rowBase1Index2 - 1;
            var colIndex2 = colBase1Index2 - 1;
            var targetRowIndex1 = targetBase1RowIndex1 - 1;
            var cell1Bez = Helper.GetCellBez0(rowIndex1, colIndex1);
            var cell2Bez = Helper.GetCellBez0(rowIndex2, colIndex2);
            var range1 = sourceSheet.Range[cell1Bez, cell2Bez];

            Excel.Range range2;
            if (targetRowIndex1 == -1)
            {
                range2 = targetSheet.Range[cell1Bez, cell2Bez];
            }
            else
            {
                int targetRowIndex2 = targetRowIndex1 + (rowIndex2 - rowIndex1);
                cell1Bez = Helper.GetCellBez0(targetRowIndex1, colIndex1);
                cell2Bez = Helper.GetCellBez0(targetRowIndex2, colIndex2);
                range2 = targetSheet.Range[cell1Bez, cell2Bez];
            }

            range1.Copy(Type.Missing);
            //R2.PasteSpecial(Excel.XlPasteType.xlPasteFormats, Excel.XlPasteSpecialOperation.xlPasteSpecialOperationNone, false, false);
            range2.PasteSpecial(Excel.XlPasteType.xlPasteAll, Excel.XlPasteSpecialOperation.xlPasteSpecialOperationNone, false, false);
            if (copyColumnWidth)
                range2.PasteSpecial(Excel.XlPasteType.xlPasteColumnWidths, Excel.XlPasteSpecialOperation.xlPasteSpecialOperationNone, false, false);

            range2.RowHeight = range1.RowHeight;
        }

        private void ReleaseObject(object obj)
        {
            if (obj != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(obj);
        }
    }
}
