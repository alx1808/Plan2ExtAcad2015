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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Excel = Microsoft.Office.Interop.Excel;
using System.Reflection;
using System.Globalization;

namespace Plan2Ext.CalcArea
{
    internal class Excelizer
    {
        //public enum AktFlaecheErrorType
        //{
        //    NoError,
        //    NoRaumBlock,
        //    MoreThanOneRaumBlock,
        //    InvalidGeometry,
        //    RaumblocksWithoutFlaechengrenze,
        //    BlockHasNotThisM2Attribute,
        //    NoFlaechengrenzen,
        //    WrongM2,
        //}

        private List<string> HEADER = new List<string>() { "DWG-Name", "Fehler"};

        internal bool ExcelExport(Dictionary<string, List<Plan2Ext.Flaeche.AktFlaecheErrorType>> dwgErrors)
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

                var b1 = GetCellBez(0, 0);
                var b2 = GetCellBez(0, HEADER.Count);
                var range = sheet.Range[b1, b2];
                range.Font.Bold = true;
                range.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;


                int rowCount = 1 + dwgErrors.Count; // colLists[0].Count;
                int colCount = HEADER.Count; // colLists.Count;
                b2 = GetCellBez(rowCount - 1, colCount - 1);
                range = sheet.Range[b1, b2];
                //range.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;

                string[,] indexMatrix = new string[rowCount, colCount];
                for (int i = 0; i < HEADER.Count; i++)
                {
                    indexMatrix[0, i] = HEADER[i];
                }

                int r = 1;
                foreach (var kvp in dwgErrors)
                {
                    indexMatrix[r, 0] = kvp.Key; // dwgname
                    if (kvp.Value.Count > 0) indexMatrix[r, 1] = "x"; // dwgname

                    r++;
                }

                range.set_Value(Excel.XlRangeValueDataType.xlRangeValueDefault, indexMatrix);

                range.Font.Name = "Arial";
                range.Columns.AutoFit();
            }
            finally
            {
                myApp.Visible = true;
                myApp.ScreenUpdating = true;

                releaseObject(sheet);
                releaseObject(workBook);
                releaseObject(myApp);
            }

            return true;
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

    }
}
