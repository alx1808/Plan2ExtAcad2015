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
using _AcLm = Autodesk.AutoCAD.LayerManager;
#endif

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Excel = Microsoft.Office.Interop.Excel;

// ReSharper disable AccessToStaticMemberViaDerivedType

namespace Plan2Ext.Kleinbefehle
{
    public class BlockFarben
    {
        #region log4net Initialization
        // ReSharper disable once InconsistentNaming
        // ReSharper disable once UnusedMember.Local
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(Convert.ToString((typeof(BlockFarben))));
        #endregion

        private IBlockFarbenHandler _curHandler;
        private _AcDb.Database _database;
        private _AcEd.Editor _editor;

        [_AcTrx.CommandMethod("Plan2BlockFarben")]
        public void Plan2BlockFarben()
        {
            var doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            _database = doc.Database;
            _editor = _AcAp.Application.DocumentManager.MdiActiveDocument.Editor;
            try
            {
                // Alle Layer tauen, ein, entsperren.
                Globs.UnlockAllLayers();

                var pKeyOpts = new _AcEd.PromptKeywordOptions("") {Message = "\nOption eingeben: "};
                pKeyOpts.Keywords.Add("Export");
                pKeyOpts.Keywords.Add("Import");
                pKeyOpts.Keywords.Add("Manuell");
                pKeyOpts.AllowNone = true;

                _AcEd.PromptResult pKeyRes = _editor.GetKeywords(pKeyOpts);
                if (pKeyRes.Status == _AcEd.PromptStatus.None || pKeyRes.StringResult == "Manuell")
                {
                    _curHandler = new ManualBlockFarbenHandler(_editor);
                }
                else if (pKeyRes.Status == _AcEd.PromptStatus.OK)
                {
                    if (pKeyRes.StringResult == "Export")
                    {
                        _curHandler = new ExcelExporter();
                    }
                    else
                    {
                        _curHandler = new ExcelImporter(_editor);
                    }
                }
                else
                    return;

                if (!_curHandler.CanRun) return;

                _AcDb.ObjectId[] idArray;
                if (!SelectEntities(_editor, out idArray)) return;

                Run(idArray);
            }
            catch (Exception ex)
            {
                string msg = string.Format(CultureInfo.CurrentCulture, "Fehler in (Plan2BlockFarben): {0}", ex.Message);
                _editor.WriteMessage("\n" + msg);
                // ReSharper disable once LocalizableElement
                MessageBox.Show(ex.Message, "Plan2BlockFarben");
            }
        }

        private void Run(_AcDb.ObjectId[] idArray)
        {
            using (_AcDb.Transaction myT = _database.TransactionManager.StartTransaction())
            {
                int level = 0;

                RecRun(idArray, myT, level);
                myT.Commit();
            }
            _curHandler.Finish();
        }

        private void RecRun(_AcDb.ObjectId[] idArray, _AcDb.Transaction myT, int level)
        {
            if (level >= 10) return;
            level++;

            foreach (var oid in idArray)
            {
                _AcDb.Entity ent = myT.GetObject(oid, _AcDb.OpenMode.ForRead) as _AcDb.Entity;
                if (ent == null) continue;
                _curHandler.Run(ent);

                _AcDb.BlockReference blockRef = ent as _AcDb.BlockReference;
                if (blockRef != null)
                {
                    var blockTableRecord = (_AcDb.BlockTableRecord)myT.GetObject(blockRef.BlockTableRecord, _AcDb.OpenMode.ForRead);
                    if (blockTableRecord.IsFromExternalReference)
                    {
                        continue;
                    }

                    var attributes = Globs.GetAttributEntities(blockRef, myT);
                    foreach (var attributeReference in attributes)
                    {
                        _curHandler.Run(attributeReference);
                    }
                    var innerIdArray = GetOidsAsArray(blockTableRecord);
                    RecRun(innerIdArray, myT, level);
                }
            }
        }

        private static _AcDb.ObjectId[] GetOidsAsArray(_AcDb.BlockTableRecord bd)
        {
            var innerIdList = new List<_AcDb.ObjectId>();
            foreach (_AcDb.ObjectId innerOid in bd)
            {
                innerIdList.Add(innerOid);
            }

            var innerIdArray = innerIdList.ToArray();
            return innerIdArray;
        }

        private static bool SelectEntities(_AcEd.Editor ed, out _AcDb.ObjectId[] idArray)
        {
            _AcEd.PromptSelectionOptions selOpts =
                new _AcEd.PromptSelectionOptions { MessageForAdding = "Elemente für Farbänderung wählen: " };
            var res = ed.GetSelection(selOpts);
            if (res.Status != _AcEd.PromptStatus.OK)
            {
                ed.WriteMessage("\nAuswahl wurde abgebrochen.");
                idArray = null;
                return false;
            }

#if BRX_APP
            _AcEd.SelectionSet ss = res.Value;
#else
            using (_AcEd.SelectionSet ss = res.Value)
#endif
            {
                idArray = ss.GetObjectIds();
            }

            return true;
        }

        //private static bool GetColorNumbers(_AcEd.Editor ed, out short fromColorIndex, out short toColorIndex)
        //{
        //    fromColorIndex = 1;
        //    toColorIndex = 1;

        //    var inputOptions = new _AcEd.PromptIntegerOptions("")
        //    {
        //        Message = "Von Farbe (Nummer): ",
        //        AllowZero = false,
        //        AllowNegative = false
        //    };

        //    _AcEd.PromptIntegerResult intRes;
        //    if (!GetColorValue(ed, inputOptions, out intRes)) return false;
        //    fromColorIndex = (short)intRes.Value;

        //    inputOptions.Message = "Nach Farbe (Nummer): ";
        //    if (!GetColorValue(ed, inputOptions, out intRes)) return false;
        //    toColorIndex = (short)intRes.Value;

        //    return true;
        //}

        //private static bool GetColorValue(_AcEd.Editor ed, _AcEd.PromptIntegerOptions inputOptions, out _AcEd.PromptIntegerResult intRes)
        //{
        //    intRes = ed.GetInteger(inputOptions);
        //    if (!CheckGetIntegerforColorResult(ed, intRes)) return false;
        //    return true;
        //}

        //private static bool CheckGetIntegerforColorResult(_AcEd.Editor ed, _AcEd.PromptIntegerResult intRes)
        //{
        //    if (intRes.Status != _AcEd.PromptStatus.OK) return false;
        //    if (!CheckColorValidity(ed, intRes)) return false;
        //    return true;
        //}

        //private static bool CheckColorValidity(_AcEd.Editor ed, _AcEd.PromptIntegerResult intRes)
        //{
        //    if (intRes.Value > 255)
        //    {
        //        ed.WriteMessage("\nUngültiger Farbwert.");
        //        return false;
        //    }

        //    return true;
        //}


        //private static void RecSetColor(short fromColorIndex, short toColorIndex, _AcDb.ObjectId[] idArray, _AcDb.Transaction myT, int level)
        //{
        //    if (level >= 10) return;
        //    level++;

        //    foreach (var oid in idArray)
        //    {
        //        _AcDb.Entity ent = myT.GetObject(oid, _AcDb.OpenMode.ForRead) as _AcDb.Entity;
        //        if (ent == null) continue;
        //        ChangeColor(fromColorIndex, toColorIndex, ent);

        //        _AcDb.BlockReference blockRef = ent as _AcDb.BlockReference;
        //        if (blockRef != null)
        //        {
        //            var blockTableRecord = (_AcDb.BlockTableRecord)myT.GetObject(blockRef.BlockTableRecord, _AcDb.OpenMode.ForRead);
        //            if (blockTableRecord.IsFromExternalReference)
        //            {
        //                continue;
        //            }

        //            var attributes = Globs.GetAttributEntities(blockRef, myT);
        //            foreach (var attributeReference in attributes)
        //            {
        //                ChangeColor(fromColorIndex, toColorIndex, attributeReference);
        //            }


        //            var innerIdArray = GetOidsAsArray(blockTableRecord);

        //            RecSetColor(fromColorIndex, toColorIndex, innerIdArray, myT, level);
        //        }
        //    }
        //}

        //private static _AcDb.ObjectId[] GetOidsAsArray(_AcDb.BlockTableRecord bd)
        //{
        //    var innerIdList = new List<_AcDb.ObjectId>();
        //    foreach (_AcDb.ObjectId innerOid in bd)
        //    {
        //        innerIdList.Add(innerOid);
        //    }

        //    var innerIdArray = innerIdList.ToArray();
        //    return innerIdArray;
        //}

        //private static void ChangeColor(short fromColorIndex, short toColorIndex, _AcDb.Entity ent)
        //{
        //    var col = ent.Color;
        //    switch (col.ColorMethod)
        //    {
        //        case Autodesk.AutoCAD.Colors.ColorMethod.ByAci:
        //            if (col.ColorIndex == fromColorIndex)
        //            {
        //                ent.UpgradeOpen();
        //                var newColour = _AcCm.Color.FromColorIndex(_AcCm.ColorMethod.ByAci, toColorIndex);
        //                ent.Color = newColour;
        //                ent.DowngradeOpen();
        //            }

        //            break;
        //    }
        //}
    }

    internal interface IBlockFarbenHandler
    {
        bool CanRun { get; }
        void Run(_AcDb.Entity ent);
        void Finish();
    }

    internal class ExcelBase
    {
        protected static string GetCellBez(int rowIndex, int colIndex)
        {
            return TranslateColumnIndexToName(colIndex) + (rowIndex + 1).ToString(CultureInfo.InvariantCulture);
        }

        protected static String TranslateColumnIndexToName(int index)
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

        protected void GetNrRowsCols(Excel.Worksheet sheet, out int nrRows, out int nrCols)
        {
            GetNrCols(sheet, out nrCols);
            GetNrRows(sheet, out nrRows);
        }
        private const int Maxcols = 50;
        private const int Maxrows = 3000;
        protected void GetNrRows(Excel.Worksheet sheet, out int nrRows)
        {
            nrRows = 0;

            var b1 = GetCellBez(0, 0);
            var b2 = GetCellBez(Maxrows, 0);
            var range = sheet.Range[b1, b2];

            // ReSharper disable once UseIndexedProperty
            object[,] indexMatrix = range.get_Value(Excel.XlRangeValueDataType.xlRangeValueDefault);

            for (int i = 1; i <= Maxrows; i++)
            {
                var v1 = indexMatrix[i, 1];
                if (v1 == null) break;
                nrRows++;
            }
        }

        protected void GetNrCols(Excel.Worksheet sheet, out int nrCols)
        {
            nrCols = 0;

            var b1 = GetCellBez(0, 0);
            var b2 = GetCellBez(0, Maxcols);
            var range = sheet.Range[b1, b2];
            // ReSharper disable once UseIndexedProperty
            object[,] indexMatrix = range.get_Value(Excel.XlRangeValueDataType.xlRangeValueDefault);

            for (int i = 1; i <= Maxcols; i++)
            {
                var v1 = indexMatrix[1, i];
                if (v1 == null) break;
                nrCols++;
            }
        }
    }

    internal class ExcelImporter : ExcelBase, IBlockFarbenHandler
    {
        private readonly _AcEd.Editor _editor;
        private string _excelImportDefaultName;
        private readonly List<ColorTransformation> _fromToColors;

        private class ColorTransformation
        {
            public _AcCm.Color FromColor { get; private set; }
            public _AcCm.Color ToColor { get; private set; }

            public ColorTransformation(_AcCm.Color fromColor, _AcCm.Color toColor)
            {
                FromColor = fromColor;
                ToColor = toColor;
            }
        }

        public ExcelImporter(_AcEd.Editor editor)
        {
            _editor = editor;
            _excelImportDefaultName = string.Empty;
            _fromToColors = new List<ColorTransformation>();
            if (GetExcelImportFileName())
            {
                ReadExcelFile();
            }
        }

        private void ReadExcelFile()
        {
            Excel.Application myApp = new Excel.Application();
            Excel.Workbook workBook = null;
            Excel.Worksheet sheet = null;
            try
            {
                workBook = myApp.Workbooks.Open(_excelImportDefaultName, Missing.Value, true);
                // ReSharper disable once UseIndexedProperty
                sheet = workBook.Worksheets.get_Item(1);

                int nrRows, nrCols;
                GetNrRowsCols(sheet, out nrRows, out nrCols);
                if (nrCols < 2) throw new InvalidOperationException(string.Format("Ungültige Anzahl von Spalten! {0}", nrCols));
                nrCols = 2;
                var b1 = GetCellBez(0, 0);
                var b2 = GetCellBez(nrRows, nrCols);
                var range = sheet.Range[b1, b2];
                // ReSharper disable once UseIndexedProperty
                object[,] impMatrix = range.get_Value(Excel.XlRangeValueDataType.xlRangeValueDefault);

                for (int r = 2; r <= nrRows; r++)
                {
                    var fromVal = impMatrix[r, 1];
                    var toVal = impMatrix[r, 2];
                    if (fromVal == null) continue;
                    if (toVal == null) continue;

                    var sFromVal = fromVal.ToString().Trim();
                    var sToVal = toVal.ToString().Trim();

                    var fromColor = GetColorFromName(sFromVal);
                    if (fromColor == null) continue;
                    var toColor = GetColorFromName(sToVal);
                    if (toColor == null) continue;

                    if (fromColor.ColorNameForDisplay == toColor.ColorNameForDisplay) continue;

                    _fromToColors.Add(new ColorTransformation(fromColor, toColor));
                }

                workBook.Close(false, Missing.Value, Missing.Value);
                myApp.Quit();
            }
            finally
            {
                ReleaseObject(sheet);
                ReleaseObject(workBook);
                ReleaseObject(myApp);
            }
        }

        private _AcCm.Color GetColorFromName(string colorName)
        {
            try
            {
                if (string.IsNullOrEmpty(colorName)) return null;

                var colArr = colorName.Split(',');
                if (colArr.Length == 1)
                {
                    var oneVal = colArr[0].Trim();
                    if (oneVal == "ByLayer")
                    {
                        return _AcCm.Color.FromColorIndex(_AcCm.ColorMethod.ByLayer, 256);
                    }
                    else if (oneVal == "ByBlock")
                    {
                        return _AcCm.Color.FromColorIndex(_AcCm.ColorMethod.ByBlock, 0);
                    }
                    else
                    {
                        short colorIndex = short.Parse(oneVal);
                        return _AcCm.Color.FromColorIndex(_AcCm.ColorMethod.ByAci, colorIndex);
                    }
                }
                else if (colArr.Length == 2)
                {
                    var colName = colArr[0].Trim();
                    var bookName = colArr[1].Trim();
                    return _AcCm.Color.FromNames(colName, bookName);
                }
                else if (colArr.Length == 3)
                {
                    var red = byte.Parse(colArr[0].Trim());
                    var green = byte.Parse(colArr[1].Trim());
                    var blue = byte.Parse(colArr[2].Trim());
                    return _AcCm.Color.FromRgb(red, green, blue);
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }
            catch (Exception)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Konnte keine Farbe erstellen aus '{0}'!",colorName));
            }
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

        private bool GetExcelImportFileName()
        {
            _AcWnd.OpenFileDialog ofd = new _AcWnd.OpenFileDialog(
                "Excel-Datei für Import wählen", _excelImportDefaultName, "xls;xlsx", "ExcelFileToImport",
                _AcWnd.OpenFileDialog.OpenFileDialogFlags.AllowAnyExtension
            );
            var dr = ofd.ShowDialog();
            if (dr != DialogResult.OK) return false;
            _excelImportDefaultName = ofd.Filename;
            return true;
        }

        public bool CanRun
        {
            get { return _fromToColors.Count > 0; }
        }

        public void Run(_AcDb.Entity ent)
        {
            var colTrans =
                _fromToColors.FirstOrDefault(x => ent.Color.ColorNameForDisplay == x.FromColor.ColorNameForDisplay);
            if (colTrans != null)
            {
                ent.UpgradeOpen();
                var newColor = colTrans.ToColor.Clone() as _AcCm.Color;
                ent.Color = newColor;
                ent.DowngradeOpen();
            }
        }

        public void Finish()
        {
            _editor.Regen();
        }
    }

    internal class ExcelExporter : ExcelBase, IBlockFarbenHandler
    {
        private readonly Dictionary<string, List<string>> _colsForExcel;
        private const string FromColorField = "FromColor";
        private const string ToColorField = "ToColor";
        private readonly List<string> _colorNames;

        public ExcelExporter()
        {
            CanRun = true;
            _colsForExcel = new Dictionary<string, List<string>>
            {
                {FromColorField, new List<string> {FromColorField}},
                {ToColorField, new List<string> {ToColorField}}
            };
            _colorNames = new List<string>();

        }

        public bool CanRun { get; private set; }

        public void Run(_AcDb.Entity ent)
        {
            var color = ent.Color;
            var colorName = GetColorName(color);
            if (!_colorNames.Contains(colorName))
            {
                _colorNames.Add(colorName);
            }
        }

        private static string GetColorName(_AcCm.Color color)
        {
            if (color.HasColorName && color.HasBookName)
            {
                return color.ColorName + "," + color.BookName;
            }

            switch (color.ColorMethod)
            {
                case Autodesk.AutoCAD.Colors.ColorMethod.ByAci:
                    return color.ColorIndex.ToString();
                case Autodesk.AutoCAD.Colors.ColorMethod.ByBlock:
                    return "ByBlock";
                case Autodesk.AutoCAD.Colors.ColorMethod.ByColor:
                    return color.ColorNameForDisplay;
                case Autodesk.AutoCAD.Colors.ColorMethod.ByLayer:
                    return "ByLayer";
            }

            return color.ColorNameForDisplay + " not supported!";
        }

        public void Finish()
        {
            foreach (var colorName in _colorNames)
            {
                _colsForExcel[FromColorField].Add(colorName);
                _colsForExcel[ToColorField].Add(colorName);
            }

            ExcelExport();
        }

        private void ExcelExport()
        {
            var myApp = new Excel.Application();
            try
            {
                var workBook = myApp.Workbooks.Add(Missing.Value);
                Excel.Worksheet sheet = workBook.ActiveSheet;
                Excel.Range cells = sheet.Cells;
                cells.NumberFormat = "@";

                var colLists = _colsForExcel.Values.ToList();

                var b1 = GetCellBez(0, 0);
                var b2 = GetCellBez(0, colLists.Count);
                var range = sheet.Range[b1, b2];
                range.Font.Bold = true;

                int rowCount = colLists[0].Count;
                int colCount = colLists.Count;
                b2 = GetCellBez(rowCount - 1, colCount - 1);
                range = sheet.Range[b1, b2];
                range.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;

                string[,] indexMatrix = new string[rowCount, colCount];
                for (int colCnt = 0; colCnt < colLists.Count; colCnt++)
                {
                    var rows = colLists[colCnt];
                    for (int rowCnt = 0; rowCnt < rows.Count; rowCnt++)
                    {
                        indexMatrix[rowCnt, colCnt] = rows[rowCnt];
                    }
                }

                range.Value[Excel.XlRangeValueDataType.xlRangeValueDefault] = indexMatrix;
                range.Font.Name = "Arial";
                range.Columns.AutoFit();
            }
            finally
            {
                myApp.Visible = true;
                myApp.ScreenUpdating = true;
            }
        }
    }

    internal class ManualBlockFarbenHandler : IBlockFarbenHandler
    {
        private readonly _AcEd.Editor _editor;
        private readonly _AcCm.Color _fromColor;
        private readonly _AcCm.Color _toColor;

        public ManualBlockFarbenHandler(_AcEd.Editor editor)
        {
            CanRun = false;
            _editor = editor;

            var dlg = new _AcWnd.ColorDialog();
            var cres = dlg.ShowDialog();
            if (cres != DialogResult.OK) return;
            _fromColor = dlg.Color;

            cres = dlg.ShowDialog();
            if (cres != DialogResult.OK) return;
            _toColor = dlg.Color;

            CanRun = true;
        }

        public bool CanRun { get; private set; }

        public void Run(_AcDb.Entity ent)
        {
            var col = ent.Color;
            if (col.ColorNameForDisplay == _fromColor.ColorNameForDisplay)
            {
                ent.UpgradeOpen();
                var newColor = _toColor.Clone() as _AcCm.Color;
                ent.Color = newColor;
                ent.DowngradeOpen();
            }
        }

        public void Finish()
        {
            _editor.Regen();
        }
    }
}
