using System;
using System.Globalization;

namespace Plan2Ext.Excelx
{
    internal static class Helper
    {
        public static string GetCellBez0(int rowIndex, int colIndex)
        {
            return TranslateColumnIndexToName(colIndex) + (rowIndex + 1).ToString(CultureInfo.InvariantCulture);
        }
        public static string GetCellBez1(int rowIndex, int colIndex)
        {
            return TranslateColumnIndexToName(colIndex-1) + rowIndex.ToString(CultureInfo.InvariantCulture);
        }

        public static String TranslateColumnIndexToName(int index)
        {
            var quotient = (index) / 26;

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
