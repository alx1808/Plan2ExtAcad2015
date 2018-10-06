using System;
using System.Collections.Generic;
using System.Reflection;
using System.Globalization;
using Autodesk.AutoCAD.ApplicationServices.Core;
using StringComparison = System.StringComparison;
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
// ReSharper disable CommentTypo
// ReSharper disable StringLiteralTypo
// ReSharper disable IdentifierTypo


namespace Plan2Ext.BlockTrans
{
    internal class Engine
    {
        #region log4net Initialization
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(Convert.ToString((typeof(LayTrans.Engine))));
        #endregion

        public readonly List<string> Errors = new List<string>();

        // ReSharper disable once StringLiteralTypo
        private readonly List<string> _header = new List<string>() { "Alter Name", "Neuer Name", "Auflösen", "Einheit" };

        internal bool BlockTrans(string fileName)
        {

            Globs.UnlockAllLayers();

            Errors.Clear();
            var blockInfos = ExcelImport(fileName);

            foreach (var err in Errors)
            {
                Log.Warn(err);
            }

            var doc = Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            foreach (var blockInfo in blockInfos)
            {
                try
                {
                    using (_AcDb.Transaction trans = doc.TransactionManager.StartTransaction())
                    {
                        _AcDb.BlockTable blockTable = trans.GetObject(db.BlockTableId, _AcDb.OpenMode.ForRead) as _AcDb.BlockTable;

                        if (blockTable != null && blockTable.Has(blockInfo.OldBlockName))
                        {
                            var oid = blockTable[blockInfo.OldBlockName];
                            _AcDb.BlockTableRecord blockTableRecord = (_AcDb.BlockTableRecord)trans.GetObject(oid, _AcDb.OpenMode.ForWrite);

                            if (string.Compare(blockInfo.OldBlockName, blockInfo.NewBlockName, StringComparison.OrdinalIgnoreCase) != 0)
                            {
                                blockTableRecord.Name = blockInfo.NewBlockName;
                            }

                            blockTableRecord.Explodable = blockInfo.Explodable2;
                            blockTableRecord.Units = blockInfo.Units2;
                        }
                        trans.Commit();
                    }
                }
                catch (Exception ex)
                {
                    Log.WarnFormat(CultureInfo.CurrentCulture, "Fehler bei Block '{0}'! {1}",blockInfo.OldBlockName, ex.Message);
                }
            }

            return Errors.Count == 0;
        }

        private List<BlockInfo> ExcelImport(string fileName)
        {
            Excel.Application myApp = new Excel.Application();
            Excel.Workbook workBook = null;
            Excel.Worksheet sheet = null;
            try
            {
                workBook = myApp.Workbooks.Open(fileName, Missing.Value, true); //, 5, "", "", true, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);
                // ReSharper disable once UseIndexedProperty
                sheet = workBook.Worksheets.get_Item(1);

                var biis = GetBlockInfos(sheet);

                workBook.Close(false, Missing.Value, Missing.Value);
                myApp.Quit();

                return biis;

            }
            finally
            {
                ReleaseObject(sheet);
                ReleaseObject(workBook);
                ReleaseObject(myApp);
            }
        }

        private List<BlockInfo> GetBlockInfos(Excel.Worksheet sheet)
        {
            Excel.Range range;
            // test import
            int nrRows;
            var nrCols = _header.Count;
            GetNrRows(sheet, out nrRows);
            var b1 = GetCellBez(0, 0);
            var b2 = GetCellBez(nrRows, nrCols);
            range = sheet.Range[b1, b2];
            // ReSharper disable once UseIndexedProperty
            object[,] impMatrix = range.get_Value(Excel.XlRangeValueDataType.xlRangeValueDefault);

            var doc = Application.DocumentManager.MdiActiveDocument;

            List<BlockInfo> biis = new List<BlockInfo>();
            using (_AcDb.Transaction trans = doc.TransactionManager.StartTransaction())
            {
                try
                {
                    for (int r = 2; r <= nrRows; r++)
                    {
                        for (int c = 1; c <= nrCols; c++)
                        {
                            if (impMatrix[r, c] == null) impMatrix[r, c] = "";
                        }
                    }
                    for (int r = 2; r <= nrRows; r++)
                    {
                        BlockInfo blockInfo = new BlockInfo
                        {
                            OldBlockName = impMatrix[r, 1].ToString(),
                            NewBlockName = impMatrix[r, 2].ToString(),
                            Explodable = impMatrix[r, 3].ToString(),
                            Units = impMatrix[r, 4].ToString()
                        };

                        if (blockInfo.Ok)
                        {
                            biis.Add(blockInfo);
                        }
                        if (!string.IsNullOrEmpty(blockInfo.Errors))
                        {
                            Errors.Add(blockInfo.Errors);
                        }
                    }
                }
                finally
                {
                    trans.Commit();
                }
            }

            return biis;
        }

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

        private const int Maxrows = 3000;
        private void GetNrRows(Excel.Worksheet sheet, out int nrRows)
        {
            nrRows = 0;

            var b1 = GetCellBez(0, 0);
            var b2 = GetCellBez(Maxrows, 0);
            Log.DebugFormat("B1 '{0}' und B2 '{1}", b1, b2);
            var range = sheet.Range[b1, b2];
            Log.DebugFormat("Nach getrange!");

            // ReSharper disable once UseIndexedProperty
            object[,] indexMatrix = range.get_Value(Excel.XlRangeValueDataType.xlRangeValueDefault);

            for (int i = 1; i <= Maxrows; i++)
            {
                var v1 = indexMatrix[i, 1];
                if (v1 == null) break;
                nrRows++;
            }
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
                            _AcDb.BlockTableRecord blockTableRecord = (_AcDb.BlockTableRecord)trans.GetObject(ltrOid, _AcDb.OpenMode.ForRead);
                            if (blockTableRecord.IsAnonymous || blockTableRecord.IsFromExternalReference || blockTableRecord.IsDependent || blockTableRecord.IsLayout) continue;
                            blockInfos.Add(new BlockInfo(blockTableRecord));
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

        private static string GetCellBez(int rowIndex, int colIndex)
        {
            return TranslateColumnIndexToName(colIndex) + (rowIndex + 1).ToString(CultureInfo.InvariantCulture);
        }

        private static String TranslateColumnIndexToName(int index)
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

            public BlockInfo()
            {
                Ok = true;
            }
            public BlockInfo(_AcDb.BlockTableRecord blockTableRecord)
            {
                Ok = true;
                OldBlockName = blockTableRecord.Name;
                NewBlockName = blockTableRecord.Name;
                Explodable2 = blockTableRecord.Explodable;
                _explodable = Explodable2.ToString();
                Units2 = blockTableRecord.Units;
                _units = Units2.ToString();
            }
            #endregion

            #region Internal
            internal List<string> RowAsList()
            {
                return new List<string>() { OldBlockName, NewBlockName, Explodable, Units };
            }

            #endregion

            #region Properties
            private string _errors = string.Empty;
            public string Errors { get { return _errors; }}

            private string _oldBlockName = string.Empty;
            public string OldBlockName
            {
                get { return _oldBlockName; }
                set
                {
                    _oldBlockName = value;
                    if (String.IsNullOrEmpty(_oldBlockName))
                    {
                        Ok = false;
                        // ReSharper disable once StringLiteralTypo
                        _errors = _errors + string.Format(CultureInfo.CurrentCulture, "\nKein bestehender Blockname '{0}'", _oldBlockName);
                    }
                }
            }
            private string _newBlockName = string.Empty;
            public string NewBlockName
            {
                get { return _newBlockName; }
                set
                {
                    _newBlockName = value;
                    if (String.IsNullOrEmpty(_newBlockName))
                    {
                        Ok = false;
                        // ReSharper disable once StringLiteralTypo
                        _errors = _errors + string.Format(CultureInfo.CurrentCulture, "\nKein neuer Blockname für Block '{0}'", OldBlockName);
                    }
                }
            }

            public bool Explodable2 = true;
            private string _explodable = string.Empty;
            public string Explodable
            {
                private get { return _explodable; }
                set
                {
                    if (string.Compare(value, "false", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Explodable2 = false;
                        _explodable = Explodable2.ToString();
                    }
                    else if (string.Compare(value, "true", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Explodable2 = true;
                        _explodable = Explodable2.ToString();
                    }
                    else
                    {
                        Ok = false;
                        _errors = _errors + string.Format(CultureInfo.CurrentCulture, "\nUngültiger Wert '{0}' für auslösbar für Block '{1}'", value, OldBlockName);
                    }
                }
            }

            public _AcDb.UnitsValue Units2 = _AcDb.UnitsValue.Undefined;
            private string _units = string.Empty;
            public string Units
            {
                private get { return _units; }
                set
                {
                    if (!TryConvertToUnits(value))
                    {
                        Ok = false;
                        _errors = _errors + string.Format(CultureInfo.CurrentCulture, "\nUngültiger Wert '{0}' für Einheit für Block '{1}'", value, OldBlockName);
                    }
                }
            }

            private bool TryConvertToUnits(string value)
            {
                foreach (var enumValue in Enum.GetValues(typeof(_AcDb.UnitsValue)))
                {
                    var ev = (_AcDb.UnitsValue)enumValue;
                    if (string.Compare(ev.ToString(), value, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Units2 = ev;
                        _units = Units2.ToString();
                        return true;
                    }
                }
                return false;
            }

            public bool Ok { get; private set; }

            #endregion
        }
    }
}
#endif
