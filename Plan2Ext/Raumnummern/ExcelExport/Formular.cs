using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Excel;
using Plan2Ext.Excelx;

namespace Plan2Ext.Raumnummern.ExcelExport
{
    internal class Formular
    {
        public string CellBez { get; set; }
        public string FormularText { get; set; }



        public static Formular Sum1(int rowIndex1, int colIndex1, int rowIndex2, int colIndex2, int sumFieldRowIndex, int sumFieldColIndex)
        {
            return new Formular()
            {
                FormularText = GetSumString1(rowIndex1, colIndex1, rowIndex2, colIndex2),
                CellBez = Helper.GetCellBez1(sumFieldRowIndex, sumFieldColIndex)
            };
        }

        public static string GetSumString1(int rowIndex1, int colIndex1, int rowIndex2, int colIndex2)
        {
            var c1 = Helper.GetCellBez1(rowIndex1, colIndex1);
            var c2 = Helper.GetCellBez1(rowIndex2, colIndex2);
            return $"=SUM({c1}:{c2})";
        }

        public void Write(Worksheet workSheet)
        {
            var rng = workSheet.Range[CellBez];
            rng.Formula = FormularText;
        }
    }
}
