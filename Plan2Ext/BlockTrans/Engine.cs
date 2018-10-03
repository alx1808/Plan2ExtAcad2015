using System;
using System.Collections.Generic;
using System.Reflection;
using System.Globalization;
using Autodesk.AutoCAD.ApplicationServices.Core;
#if BRX_APP
using _AcAp = Bricscad.ApplicationServices;
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
using _AcIntCom = Autodesk.AutoCAD.Interop.Common;
using _AcInt = Autodesk.AutoCAD.Interop;
using Excel = Microsoft.Office.Interop.Excel;


namespace Plan2Ext.BlockTrans
{
    internal class Engine
    {
        #region log4net Initialization
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(Convert.ToString((typeof(LayTrans.Engine))));
        #endregion

        // ReSharper disable once StringLiteralTypo
        private readonly List<string> _header = new List<string>() { "Alter Name", "Neuer Name" };

        internal bool ExcelExport()
        {
            Excel.Application myApp = null;
            Excel.Workbook workBook = null;
            Excel.Worksheet sheet = null;

            try
            {
                myApp = new Excel.Application();

                var blockInfos = GetBlockInfos();

                workBook = myApp.Workbooks.Add(Missing.Value);
                sheet = workBook.ActiveSheet;

                Excel.Range cells = sheet.Cells;
                cells.NumberFormat = "@";
                var b1 = GetCellBez(0, 0);
                var b2 = GetCellBez(0, _header.Count);
                var range = sheet.Range[b1, b2];
                range.Font.Bold = true;
                range.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;

                int rowCount = 1 + blockInfos.Count;
                int colCount = _header.Count;
                b2 = GetCellBez(rowCount - 1, colCount - 1);
                range = sheet.Range[b1, b2];

                string[,] indexMatrix = new string[rowCount, colCount];
                for (int i = 0; i < _header.Count; i++)
                {
                    indexMatrix[0, i] = _header[i];
                }
                for (int r = 1; r <= blockInfos.Count; r++)
                {
                    var blockInfo = blockInfos[r - 1];
                    List<string> values = blockInfo.RowAsList();
                    for (int i = 0; i < values.Count; i++)
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

                ReleaseObject(sheet);
                ReleaseObject(workBook);
                ReleaseObject(myApp);
            }

            return true;
        }

        private List<BlockInfo> GetBlockInfos()
        {
            List<BlockInfo> blockInfos = new List<BlockInfo>();
            var doc = Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;

            using (_AcDb.Transaction trans = doc.TransactionManager.StartTransaction())
            {
                try
                {
                    _AcDb.BlockTable blockTable = trans.GetObject(db.BlockTableId, _AcDb.OpenMode.ForRead) as _AcDb.BlockTable;
                    if (blockTable != null)
                    {
                        foreach (var ltrOid in blockTable)
                        {
                            _AcDb.BlockTableRecord ltr = (_AcDb.BlockTableRecord)trans.GetObject(ltrOid, _AcDb.OpenMode.ForRead);
                            if (ltr.IsAnonymous || ltr.IsFromExternalReference || ltr.IsDependent || ltr.IsLayout) continue;
                            blockInfos.Add(new BlockInfo(ltr, trans));
                        }
                    }
                }
                finally
                {
                    trans.Commit();
                }
            }

            return blockInfos;
        }

        private static void ReleaseObject(object obj)
        {
            try
            {
                if (obj != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(obj);
            }
            catch (Exception)
            {
                // ignored
            }
            finally
            {
                GC.Collect();
            }
        }

        public static string GetCellBez(int rowIndex, int colIndex)
        {
            return TranslateColumnIndexToName(colIndex) + (rowIndex + 1).ToString(CultureInfo.InvariantCulture);
        }

        public static String TranslateColumnIndexToName(int index)
        {
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

        private sealed class BlockInfo
        {
            #region Lifecycle

            public BlockInfo(_AcDb.BlockTableRecord blockTableRecord, _AcDb.Transaction trans)
            {
                OldLayer = blockTableRecord.Name;
                NewLayer = "";
            }
            #endregion

            #region Internal
            internal List<string> RowAsList()
            {
                return new List<string>() { OldLayer, NewLayer };
            }

            #endregion

            #region Properties
            private string _errors = string.Empty;
            public string Errors { get { return _errors; } }

            public string OldLayer { get; set; }
            private string _newLayer = string.Empty;
            public string NewLayer
            {
                get { return _newLayer; }
                set
                {
                    _newLayer = value;
                    if (!string.IsNullOrEmpty(OldLayer) && !String.IsNullOrEmpty(_newLayer)) Ok = true;
                    else
                    {
                        // ReSharper disable once StringLiteralTypo
                        _errors = _errors + string.Format(CultureInfo.CurrentCulture, "\nKein neuer Block für Block '{0}'", OldLayer);
                    }
                }
            }

            public bool Ok { get; private set; }

            #endregion
        }
    }
}
#endif
