using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace Plan2Ext.Excelx
{
    /// <summary>
    /// Base1-Matrix
    /// </summary>
    internal class ExcelMatrix
    {
        private readonly int _nrOfCols;
        private readonly int _startRowIndex;
        private readonly List<ExRow> _rows = new List<ExRow>();
        public ExcelMatrix(int startRowIndex, int nrOfCols)
        {
            if (nrOfCols <= 0) throw new ArgumentNullException(string.Format(CultureInfo.InvariantCulture, "Invalid nrOrCols: {0}", nrOfCols), @"nrOfCols");
            if (startRowIndex <= 0) throw new ArgumentNullException(string.Format(CultureInfo.InvariantCulture, "Invalid startRowIndex: {0}", nrOfCols), @"startRowIndex");
            _nrOfCols = nrOfCols;
            _startRowIndex = startRowIndex-1;
        }
        public void Add(int rowIndex, int colIndex, object o)
        {
            AddMissingRows(rowIndex-1);
            _rows[(rowIndex-1) - _startRowIndex].Arr[colIndex-1] = o;
        }

        private void AddMissingRows(int rowIndex)
        {
            for (int i = _rows.Count + _startRowIndex; i <= rowIndex; i++)
            {
                _rows.Add(new ExRow(_nrOfCols));
            }
        }
        internal void Write(Microsoft.Office.Interop.Excel.Worksheet targetSheet)
        {
            object[,] indexMatrix = BuildMatrix();
            var b1 = Helper.GetCellBez0(_startRowIndex, 0);
            var b2 = Helper.GetCellBez0(_startRowIndex + _rows.Count - 1, _nrOfCols - 1);
            var range = targetSheet.Range[b1, b2];
            range.Value[Microsoft.Office.Interop.Excel.XlRangeValueDataType.xlRangeValueDefault] = indexMatrix;
        }

        private object[,] BuildMatrix()
        {
            object[,] indexMatrix = new object[_rows.Count, _nrOfCols];
            for (int rowCnt = 0; rowCnt < _rows.Count; rowCnt++)
            {
                var r = _rows[rowCnt];
                for (int colCnt = 0; colCnt < _nrOfCols; colCnt++)
                {
                    indexMatrix[rowCnt, colCnt] = r.Arr[colCnt];
                }
            }
            return indexMatrix;
        }

        private class ExRow
        {
            private object[] _arr;
            public object[] Arr
            {
                get { return _arr; }
                set { _arr = value; }
            }
            public ExRow(int cols)
            {
                _arr = new object[cols];
                for (int i = 0; i < cols; i++)
                {
                    _arr[i] = Missing.Value; // "";
                }
            }
        }
    }

}
