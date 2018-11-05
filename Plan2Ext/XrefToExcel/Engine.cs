using System;
using System.Collections.Generic;
using System.Reflection;
using System.Globalization;
using System.Linq;
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

namespace Plan2Ext.XrefToExcel
{
    internal class Engine
    {
        #region log4net Initialization
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(Convert.ToString((typeof(LayTrans.Engine))));
        #endregion

        public readonly List<string> Errors = new List<string>();

        // ReSharper disable once StringLiteralTypo
        private readonly List<string> _header = new List<string>() { "DwgName", "XRef-Name alt", "XRef-Name neu", "Pfad", "Typ" };

        private readonly List<XrefInfo> _xrefInfos;

        public Engine() { }

        public Engine(string fileName)
        {
            _xrefInfos = ExcelImport(fileName).ToList();
            if (_xrefInfos == null) throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Fehler beim Auslesen der Exceldatei!"));
        }

        internal void GetXrefInfos(List<XrefInfo> xrefInfos)
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;

            using (var trans = doc.TransactionManager.StartTransaction())
            {
                var blockTable = trans.GetObject(db.BlockTableId, _AcDb.OpenMode.ForRead) as _AcDb.BlockTable;
                // ReSharper disable once PossibleNullReferenceException
                foreach (var blockOid in blockTable)
                {
                    _AcDb.BlockTableRecord blockTableRecord = (_AcDb.BlockTableRecord)trans.GetObject(blockOid, _AcDb.OpenMode.ForRead);
                    if (blockTableRecord.IsFromExternalReference)
                    {
                        xrefInfos.Add(new XrefInfo(doc.Name, blockTableRecord));
                    }
                }

                trans.Commit();
            }
        }

        internal void ExcelExport(List<XrefInfo> xrefInfos)
        {
            if (xrefInfos == null) return;
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
                var b2 = Globs.GetCellBez(0, _header.Count);
                var range = sheet.Range[b1, b2];
                range.Font.Bold = true;
                range.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;

                int rowCount = 1 + xrefInfos.Count;
                int colCount = _header.Count;
                b2 = Globs.GetCellBez(rowCount - 1, colCount - 1);
                range = sheet.Range[b1, b2];

                string[,] indexMatrix = new string[rowCount, colCount];
                for (int i = 0; i < _header.Count; i++)
                {
                    indexMatrix[0, i] = _header[i];
                }
                for (int r = 1; r <= xrefInfos.Count; r++)
                {
                    var blockInfo = xrefInfos[r - 1];
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

                Globs.FinalReleaseComObject(sheet);
                Globs.FinalReleaseComObject(workBook);
                Globs.FinalReleaseComObject(myApp);
            }
        }

        private List<XrefInfo> ExcelImport(string fileName)
        {
            Excel.Application myApp = new Excel.Application();
            Excel.Workbook workBook = null;
            Excel.Worksheet sheet = null;
            try
            {
                workBook = myApp.Workbooks.Open(fileName, Missing.Value, true); //, 5, "", "", true, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);
                // ReSharper disable once UseIndexedProperty
                sheet = workBook.Worksheets.get_Item(1);

                var biis = GetXrefInfos(sheet);

                workBook.Close(false, Missing.Value, Missing.Value);
                myApp.Quit();

                return biis;

            }
            finally
            {
                Globs.FinalReleaseComObject(sheet);
                Globs.FinalReleaseComObject(workBook);
                Globs.FinalReleaseComObject(myApp);
            }
        }

        private List<XrefInfo> GetXrefInfos(Excel.Worksheet sheet)
        {
            // test import
            int nrRows;
            var nrCols = _header.Count;
            GetNrRows(sheet, out nrRows);
            var b1 = Globs.GetCellBez(0, 0);
            var b2 = Globs.GetCellBez(nrRows, nrCols);
            var range = sheet.Range[b1, b2];
            // ReSharper disable once UseIndexedProperty
            object[,] impMatrix = range.get_Value(Excel.XlRangeValueDataType.xlRangeValueDefault);

            var biis = new List<XrefInfo>();
            for (int r = 2; r <= nrRows; r++)
            {
                for (int c = 1; c <= nrCols; c++)
                {
                    if (impMatrix[r, c] == null) impMatrix[r, c] = "";
                }
            }
            for (int r = 2; r <= nrRows; r++)
            {
                var attInfo = new XrefInfo
                {
                    DwgName = impMatrix[r, 1].ToString(),
                    OldName = impMatrix[r, 2].ToString(),
                    NewName = impMatrix[r, 3].ToString(),
                    PathName = impMatrix[r, 4].ToString(),
                    Typ = impMatrix[r, 5].ToString()
                };

                if (attInfo.Ok)
                {
                    biis.Add(attInfo);
                }
                if (!string.IsNullOrEmpty(attInfo.Errors))
                {
                    Errors.Add(attInfo.Errors);
                }
            }
            return biis;
        }

        internal bool XrefTrans()
        {
            if (_xrefInfos == null) return false;

            Globs.UnlockAllLayers();

            Errors.Clear();

            var doc = Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            _AcDb.ObjectIdCollection collection = new _AcDb.ObjectIdCollection();
            using (var trans = doc.TransactionManager.StartTransaction())
            {
                var blockTable = trans.GetObject(db.BlockTableId, _AcDb.OpenMode.ForRead) as _AcDb.BlockTable;
                foreach (var xrefInfo in _xrefInfos)
                {
                    try
                    {
                        if (xrefInfo.DwgName != doc.Name) continue;
                        // ReSharper disable once PossibleNullReferenceException
                        if (!blockTable.Has(xrefInfo.OldName))
                        {
                            var msg = string.Format(CultureInfo.CurrentCulture, "Xref '{0}' nicht gefunden!", xrefInfo.OldName);
                            Errors.Add(msg);
                            Log.WarnFormat(msg);
                            continue;
                        }

                        var oid = blockTable[xrefInfo.OldName];
                        var btr = (_AcDb.BlockTableRecord)trans.GetObject(oid, _AcDb.OpenMode.ForRead);
                        btr.UpgradeOpen();
                        btr.PathName = xrefInfo.PathName;
                        btr.Name = xrefInfo.NewName;
                        btr.IsFromOverlayReference = xrefInfo.IsFromOverlayReference;
                        btr.DowngradeOpen();
                        collection.Add(oid);
                    }
                    catch (Exception ex)
                    {
                        var msg = string.Format(CultureInfo.CurrentCulture, "Fehler bei Xref '{0}'! {1}", xrefInfo.OldName, ex.Message);
                        Errors.Add(msg);
                        Log.WarnFormat(msg);
                    }
                }
                trans.Commit();
            }

            if (collection.Count > 0)
            {
                db.ReloadXrefs(collection);
            }

            return Errors.Count == 0;
        }


        private const int Maxrows = 3000;
        private void GetNrRows(Excel.Worksheet sheet, out int nrRows)
        {
            nrRows = 0;

            var b1 = Globs.GetCellBez(0, 0);
            var b2 = Globs.GetCellBez(Maxrows, 0);
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

        internal sealed class XrefInfo
        {
            #region Lifecycle

            public XrefInfo()
            {
                Ok = true;
            }
            public XrefInfo(string dwgName, _AcDb.BlockTableRecord blockTableRecord)
            {
                Ok = true;
                DwgName = dwgName;
                OldName = blockTableRecord.Name;
                NewName = blockTableRecord.Name;
                PathName = blockTableRecord.PathName;
                IsFromOverlayReference = blockTableRecord.IsFromOverlayReference;
            }

            #endregion

            #region Internal
            internal List<string> RowAsList()
            {
                return new List<string>() { DwgName, OldName, NewName, PathName, Typ };
            }

            #endregion

            #region Properties
            private string _errors = string.Empty;
            public string Errors { get { return _errors; } }


            public string DwgName { get; set; }

            private string _oldName = string.Empty;
            public string OldName
            {
                get { return _oldName; }
                set
                {
                    _oldName = value;
                    if (String.IsNullOrEmpty(_oldName))
                    {
                        Ok = false;
                        _errors = _errors + string.Format(CultureInfo.CurrentCulture, "\nKein bestehender Xref-Name '{0}'", _oldName);
                    }
                }
            }
            private string _newName = string.Empty;
            public string NewName
            {
                get { return _newName; }
                set
                {
                    _newName = value;
                    if (String.IsNullOrEmpty(_newName))
                    {
                        Ok = false;
                        _errors = _errors + string.Format(CultureInfo.CurrentCulture, "\nKein neuer Name für Xref '{0}'", OldName);
                    }
                }
            }

            private string _pathName = string.Empty;
            public string PathName
            {
                get { return _pathName; }
                set
                {
                    _pathName = value;
                }
            }

            public string Typ
            {
                private get { return IsFromOverlayReference ? "Überlagerung" : "Zugeordnet"; }
                set
                {
                    var val = value.Trim();
                    if (string.Compare(val, "Überlagerung", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        IsFromOverlayReference = true;
                    }
                    else if (string.Compare(val, "Zugeordnet", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        IsFromOverlayReference = false;
                    }
                    else
                    {
                        Ok = false;
                        _errors = _errors + string.Format(CultureInfo.CurrentCulture, "\nUngültige Bezeichnung für Typ: '{0}'", value);
                    }
                }
            }

            public bool Ok { get; private set; }
            public bool IsFromOverlayReference { get; private set; }

            #endregion
        }

    }
}
#endif
