using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Globalization;


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



namespace Plan2Ext.LayTrans
{
    internal class Engine
    {
        #region log4net Initialization
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Convert.ToString((typeof(Engine))));
        #endregion


        private List<string> HEADER = new List<string>() { "Alter Layer", "Neuer Layer", "Farbe", "Linientyp", "Linienstärke", "Transparenz", "Plot", "Beschreibung" };
        private List<string> _Errors = new List<string>();

        internal bool LayTrans(string fileName)
        {
            _Errors.Clear();
            var linfos = ExcelImport(fileName);

            foreach (var err in _Errors)
            {
                log.Warn(err);
            }

            var doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            using (_AcDb.Transaction trans = doc.TransactionManager.StartTransaction())
            {
                _AcDb.LayerTable layTb = trans.GetObject(db.LayerTableId, _AcDb.OpenMode.ForWrite) as _AcDb.LayerTable;

                foreach (var linfo in linfos)
                {
                    CreateOrModifyLayer(linfo, doc, db, trans, layTb);
                }

                trans.Commit();
            }

            Dictionary<string, string> substDict = new Dictionary<string, string>();
            foreach (var linfo in linfos)
            {
                substDict[linfo.OldLayer.ToUpperInvariant()] = linfo.NewLayer;
            }

            using (_AcDb.Transaction trans = doc.TransactionManager.StartTransaction())
            {
                _AcDb.LayerTable layTb = trans.GetObject(db.LayerTableId, _AcDb.OpenMode.ForWrite) as _AcDb.LayerTable;

                //foreach (var linfo in linfos)
                //{
                ReplaceLayerInEntities(substDict, trans, db);
                //}

                trans.Commit();
            }

            Plan2Ext.Globs.SetLayerCurrent("0");

            List<string> oldLayerNames = linfos.Select(x => x.OldLayer).ToList();
            Plan2Ext.Globs.PurgeLayer(oldLayerNames);


            return true;
        }

        private void ReplaceLayerInEntities(Dictionary<string, string> substDict, _AcDb.Transaction trans, _AcDb.Database db)
        {
            //string oldLayer = linfo.OldLayer;
            //string newLayer = linfo.NewLayer;

            _AcDb.BlockTable blkTable = (_AcDb.BlockTable)trans.GetObject(db.BlockTableId, _AcDb.OpenMode.ForRead);
            foreach (var id in blkTable)
            {
                _AcDb.BlockTableRecord btRecord = (_AcDb.BlockTableRecord)trans.GetObject(id, _AcDb.OpenMode.ForRead);
                //System.Diagnostics.Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Block: {0}", btRecord.Name));

                //btRecord.UpgradeOpen();

                foreach (var entId in btRecord)
                {
                    _AcDb.Entity entity = (_AcDb.Entity)trans.GetObject(entId, _AcDb.OpenMode.ForWrite);

                    CheckSetLayer(substDict, entity);
                    //if (string.Compare(entity.Layer, oldLayer, StringComparison.OrdinalIgnoreCase) == 0)
                    //{
                    //    entity.Layer = newLayer;
                    //}

                    _AcDb.BlockReference block = entity as _AcDb.BlockReference;
                    if (block != null)
                    {
                        // sequend correction
                        string saveLay = block.Layer;
                        block.Layer = "0";
                        block.Layer = saveLay;

                        foreach (var att in block.AttributeCollection)
                        {
                            _AcDb.ObjectId attOid = (_AcDb.ObjectId)att;
                            _AcDb.AttributeReference attrib = trans.GetObject(attOid, _AcDb.OpenMode.ForWrite) as _AcDb.AttributeReference;
                            if (attrib != null)
                            {
                                CheckSetLayer(substDict, attrib);
                                //if (string.Compare(attrib.Layer, oldLayer, StringComparison.OrdinalIgnoreCase) == 0)
                                //{
                                //    attrib.Layer = newLayer;
                                //}
                            }
                        }

                    }
                }

                //}

            }

            // Layouts iterieren
            _AcDb.DBDictionary layoutDict = (_AcDb.DBDictionary)trans.GetObject(db.LayoutDictionaryId, _AcDb.OpenMode.ForRead);
            foreach (var loEntry in layoutDict)
            {
                if (loEntry.Key.ToUpperInvariant() == "MODEL") continue;
                _AcDb.Layout lo = (_AcDb.Layout)trans.GetObject(loEntry.Value, _AcDb.OpenMode.ForRead, false);
                if (lo == null) continue;
                //System.Diagnostics.Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Layout: {0}", lo.LayoutName));

                _AcDb.BlockTableRecord btRecord = (_AcDb.BlockTableRecord)trans.GetObject(lo.BlockTableRecordId, _AcDb.OpenMode.ForRead);
                foreach (var entId in btRecord)
                {
                    _AcDb.Entity entity = (_AcDb.Entity)trans.GetObject(entId, _AcDb.OpenMode.ForWrite);

                    CheckSetLayer(substDict, entity);

                    //if (string.Compare(entity.Layer, oldLayer, StringComparison.OrdinalIgnoreCase) == 0)
                    //{
                    //    entity.Layer = newLayer;
                    //}

                    _AcDb.BlockReference block = entity as _AcDb.BlockReference;
                    if (block != null)
                    {

                        string saveLay = block.Layer;
                        block.Layer = "0";
                        block.Layer = saveLay;

                        foreach (var att in block.AttributeCollection)
                        {
                            _AcDb.ObjectId attOid = (_AcDb.ObjectId)att;
                            _AcDb.AttributeReference attrib = trans.GetObject(attOid, _AcDb.OpenMode.ForWrite) as _AcDb.AttributeReference;
                            if (attrib != null)
                            {
                                CheckSetLayer(substDict, attrib);
                                //if (string.Compare(attrib.Layer, oldLayer, StringComparison.OrdinalIgnoreCase) == 0)
                                //{

                                //    //attrib.UpgradeOpen();
                                //    attrib.Layer = newLayer;
                                //}
                            }
                        }

                    }

                }

            }



        }

        private static void CheckSetLayer(Dictionary<string, string> substDict, _AcDb.Entity entity)
        {
            string newLayer;
            if (substDict.TryGetValue(entity.Layer.ToUpperInvariant(), out newLayer))
            {
                entity.Layer = newLayer;
            }
        }

        private void CreateOrModifyLayer(LayerInfo layerInfo, _AcAp.Document doc, _AcDb.Database db, _AcDb.Transaction trans, _AcDb.LayerTable layTb)
        {

            if (layTb.Has(layerInfo.NewLayer))
            {
                var oid = layTb[layerInfo.NewLayer];
                _AcDb.LayerTableRecord ltr = (_AcDb.LayerTableRecord)trans.GetObject(oid, _AcDb.OpenMode.ForWrite);
                layerInfo.ModifyLayer(ltr, trans, db);
            }
            else
            {
                using (_AcDb.LayerTableRecord ltr = new _AcDb.LayerTableRecord())
                {
                    // Assign the layer a name
                    ltr.Name = layerInfo.NewLayer;

                    // Upgrade the Layer table for write
                    //layTb.UpgradeOpen();


                    // Append the new layer to the Layer table and the transaction
                    layTb.Add(ltr);
                    trans.AddNewlyCreatedDBObject(ltr, true);

                    layerInfo.ModifyLayer(ltr, trans, db);

                }
            }


        }

        private List<LayerInfo> ExcelImport(string fileName)
        {
            Excel.Application myApp = new Excel.Application();
            Excel.Workbook workBook = null;
            Excel.Worksheet sheet = null;
            try
            {
                workBook = myApp.Workbooks.Open(fileName, Missing.Value, true); //, 5, "", "", true, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);
                sheet = workBook.Worksheets.get_Item(1);

                var biis = GetLayerInfos(sheet);

                workBook.Close(false, Missing.Value, Missing.Value);
                myApp.Quit();

                return biis;

            }
            finally
            {
                releaseObject(sheet);
                releaseObject(workBook);
                releaseObject(myApp);
            }
        }

        private List<LayerInfo> GetLayerInfos(Excel.Worksheet sheet)
        {
            string b1, b2;
            Excel.Range range;
            // test import
            int nrRows, nrCols;
            nrCols = HEADER.Count;
            GetNrRows(sheet, out nrRows);
            b1 = GetCellBez(0, 0);
            b2 = GetCellBez(nrRows, nrCols);
            range = sheet.Range[b1, b2];
            object[,] impMatrix = range.get_Value(Excel.XlRangeValueDataType.xlRangeValueDefault);

            var doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;

            List<LayerInfo> biis = new List<LayerInfo>();
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

                        //List<string> HEADER = new List<string>() { "Alter Layer", "Neuer Layer", "Farbe", "Linientyp", "Linienstärke", "Transparenz", "Beschreibung" };

                        LayerInfo layerInfo = new LayerInfo();
                        layerInfo.OldLayer = impMatrix[r, 1].ToString();
                        layerInfo.NewLayer = impMatrix[r, 2].ToString();
                        layerInfo.Color = impMatrix[r, 3].ToString();
                        layerInfo.SetLineType(impMatrix[r, 4].ToString(), trans, db);
                        layerInfo.LineWeight = impMatrix[r, 5].ToString();
                        layerInfo.Transparency = impMatrix[r, 6].ToString();
                        layerInfo.Plot = impMatrix[r, 7].ToString();
                        layerInfo.Description = impMatrix[r, 8].ToString();

                        if (layerInfo.Ok)
                        {
                            biis.Add(layerInfo);
                        }
                        if (!string.IsNullOrEmpty(layerInfo.Errors))
                        {
                            _Errors.Add(layerInfo.Errors);
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

        private const int MAXROWS = 3000;
        private void GetNrRows(Excel.Worksheet sheet, out int nrRows)
        {
            nrRows = 0;

            var b1 = GetCellBez(0, 0);
            var b2 = GetCellBez(MAXROWS, 0);
            log.DebugFormat("B1 '{0}' und B2 '{1}", b1, b2);
            var range = sheet.Range[b1, b2];
            log.DebugFormat("Nach getrange!");

            object[,] indexMatrix = range.get_Value(Excel.XlRangeValueDataType.xlRangeValueDefault);

            for (int i = 1; i <= MAXROWS; i++)
            {
                var v1 = indexMatrix[i, 1];
                if (v1 == null) break;
                nrRows++;
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
                //MessageBox.Show("Unable to release the Object " + ex.ToString());
            }
            finally
            {
                GC.Collect();
            }
        }


        internal bool ExcelExport(string dwgDir = "")
        {
            Excel.Application myApp = null;
            Excel.Workbook workBook = null;
            Excel.Worksheet sheet = null;

            try
            {
                myApp = new Excel.Application();

                List<LayerInfo> layerInfos = null;
                if (string.IsNullOrEmpty(dwgDir))
                {
                    layerInfos = GetLayerInfos();
                }
                else
                {
                    var dwgFiles = System.IO.Directory.GetFiles(dwgDir, "*.dwg", System.IO.SearchOption.AllDirectories);
                    layerInfos = GetLayerInfos(dwgFiles.ToList());
                }

                workBook = myApp.Workbooks.Add(Missing.Value);
                sheet = workBook.ActiveSheet;

                // Pull in all the cells of the worksheet
                Excel.Range cells = sheet.Cells;
                // set each cell's format to Text
                cells.NumberFormat = "@";
                //cells.HorizontalAlignment = XlHAlign.xlHAlignRight;

                //var colLists = valuesPerColumn.Values.ToList();


                var b1 = GetCellBez(0, 0);
                var b2 = GetCellBez(0, HEADER.Count);
                var range = sheet.Range[b1, b2];
                range.Font.Bold = true;
                range.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;


                int rowCount = 1 + layerInfos.Count; // colLists[0].Count;
                int colCount = HEADER.Count; // colLists.Count;
                b2 = GetCellBez(rowCount - 1, colCount - 1);
                range = sheet.Range[b1, b2];
                //range.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;

                string[,] indexMatrix = new string[rowCount, colCount];
                for (int i = 0; i < HEADER.Count; i++)
                {
                    indexMatrix[0, i] = HEADER[i];
                }
                for (int r = 1; r <= layerInfos.Count; r++)
                {
                    var linfo = layerInfos[r - 1];
                    List<string> vals = linfo.RowAsList();
                    for (int i = 0; i < vals.Count; i++)
                    {
                        indexMatrix[r, i] = vals[i];
                    }
                }

                //for (int colCnt = 0; colCnt < colLists.Count; colCnt++)
                //{
                //    var rows = colLists[colCnt];
                //    for (int rowCnt = 0; rowCnt < rows.Count; rowCnt++)
                //    {
                //        indexMatrix[rowCnt, colCnt] = rows[rowCnt];
                //    }
                //}
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

        /// <summary>
        /// Bulk-Befehl
        /// </summary>
        /// <param name="dwgFiles"></param>
        /// <returns></returns>
        private List<LayerInfo> GetLayerInfos(List<string> dwgFiles)
        {
            var layDict = new Dictionary<string, LayerInfo>();
            foreach (var fileName in dwgFiles)
            {
                int layerCount = 0;
                int layerAddedCount = 0;
                var db = new _AcDb.Database(false, true);
                using (db)
                {
                    try
                    {
                        db.ReadDwgFile(fileName, System.IO.FileShare.Read, allowCPConversion: false, password: "");
                    }
                    catch (Exception ex)
                    {
                        log.WarnFormat("Fehler beim Öffnen der Zeichnung '{0}'!{1}", fileName, ex.Message);
                        continue;
                    }
                    using (_AcDb.Transaction trans = db.TransactionManager.StartTransaction())
                    {
                        try
                        {
                            _AcDb.LayerTable layTb = trans.GetObject(db.LayerTableId, _AcDb.OpenMode.ForRead) as _AcDb.LayerTable;
                            foreach (var ltrOid in layTb)
                            {
                                _AcDb.LayerTableRecord ltr = (_AcDb.LayerTableRecord)trans.GetObject(ltrOid, _AcDb.OpenMode.ForRead);
                                string layNameUC = ltr.Name.ToUpperInvariant();
                                layerCount++;
                                if (!layDict.ContainsKey(layNameUC))
                                {
                                    layDict.Add(layNameUC, new LayerInfo(ltr, trans));
                                    layerAddedCount++;
                                }
                            }
                        }
                        finally
                        {
                            trans.Commit();
                        }
                    }
                }
                log.InfoFormat("{0}: {1} von {2} Layer hinzugefügt.", fileName, layerAddedCount, layerCount);
            }

            var linfos = layDict.Values.ToList();
            return linfos;
        }

        private List<LayerInfo> GetLayerInfos()
        {
            List<LayerInfo> linfos = new List<LayerInfo>();
            var doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;

            using (_AcDb.Transaction trans = doc.TransactionManager.StartTransaction())
            {
                try
                {
                    _AcDb.LayerTable layTb = trans.GetObject(db.LayerTableId, _AcDb.OpenMode.ForRead) as _AcDb.LayerTable;
                    foreach (var ltrOid in layTb)
                    {
                        _AcDb.LayerTableRecord ltr = (_AcDb.LayerTableRecord)trans.GetObject(ltrOid, _AcDb.OpenMode.ForRead);
                        linfos.Add(new LayerInfo(ltr, trans));
                    }
                }
                finally
                {
                    trans.Commit();
                }
            }

            return linfos;
        }

        private class LayerInfo
        {
            //List<string> HEADER = new List<string>() { "Alter Layer", "Neuer Layer", "Farbe", "Linientyp", "Linienstärke", "Transparenz", "Beschreibung" };

            private string _Errors = string.Empty;
            public string Errors { get { return _Errors; } }

            public string OldLayer { get; set; }
            private string _NewLayer = string.Empty;
            public string NewLayer
            {
                get { return _NewLayer; }
                set
                {
                    _NewLayer = value;
                    if (!string.IsNullOrEmpty(OldLayer) && !String.IsNullOrEmpty(_NewLayer)) _Ok = true;
                    else
                    {
                        _Errors = _Errors + string.Format(CultureInfo.CurrentCulture, "\nKein neuer Layer für Layer '{0}'", OldLayer);
                    }
                }
            }
            private _AcCm.Color _ColorO = null;
            private string _Color = string.Empty;
            public string Color
            {
                get { return _Color; }
                set
                {
                    _Color = value;

                    StringToColor();
                }
            }

            private _AcDb.ObjectId _LineTypeO;
            private string _LineType = string.Empty;
            public void SetLineType(string lt, _AcDb.Transaction trans, _AcDb.Database db)
            {
                _LineType = lt;
                var lto = GetLinetypeFromName(_LineType, trans, db);
                if (lto == default(_AcDb.ObjectId))
                {
                    _Errors = _Errors + string.Format(CultureInfo.CurrentCulture, "\nUngültiger Linientyp '{0}' für Layer '{1}'", _LineType, OldLayer);
                    return;
                }

                _LineTypeO = lto;
            }
            public string LineType { get { return _LineType; } }
            private _AcDb.LineWeight _LineWeightO;
            private string _LineWeight = string.Empty;
            public string LineWeight
            {
                get { return _LineWeight; }
                set
                {
                    _LineWeight = value;
                    SetLineWeight();


                }
            }

            private void SetLineWeight()
            {
                _LineWeightO = _AcDb.LineWeight.ByLineWeightDefault;
                if (string.IsNullOrEmpty(_LineWeight))
                {
                    return;
                }
                double d;
                if (!double.TryParse(_LineWeight, NumberStyles.Any, CultureInfo.InvariantCulture, out d))
                {
                    _Errors = _Errors + string.Format(CultureInfo.CurrentCulture, "\nUngültiger Eintrag für Linienstärke für Layer '{0}': {1}", OldLayer, _LineWeight);
                    return;
                }

                d = d * 100.0;
                int val = (int)Math.Floor(d);
                string cmpVal = "LineWeight" + val.ToString().PadLeft(3, '0');

                foreach (var e in Enum.GetValues(typeof(_AcDb.LineWeight)))
                {
                    if (cmpVal == e.ToString())
                    {
                        _LineWeightO = (_AcDb.LineWeight)e;
                        return;
                    }
                }

                _Errors = _Errors + string.Format(CultureInfo.CurrentCulture, "\nUngültiger Eintrag für Linienstärke für Layer '{0}': {1}", OldLayer, _LineWeight);

            }
            private _AcCm.Transparency _TransparencyO;
            private string _Transparency = string.Empty;
            public string Transparency
            {
                get { return _Transparency; }
                set
                {
                    _Transparency = value;
                    SetTransparency();

                }

            }

            private void SetTransparency()
            {
                if (string.IsNullOrEmpty(_Transparency)) return;
                int t;
                if (!int.TryParse(_Transparency, out t))
                {
                    _Errors = _Errors + string.Format(CultureInfo.CurrentCulture, "\nUngültiger Eintrag für Transparenz für Layer '{0}': {1}", OldLayer, _Transparency);
                    return;
                }
                if (t < 0 || t > 90)
                {
                    _Errors = _Errors + string.Format(CultureInfo.CurrentCulture, "\nUngültiger Eintrag für Transparenz für Layer '{0}': {1}", OldLayer, _Transparency);
                    return;
                }
                Byte alpha = TransparenzToAlpha(t);
                _TransparencyO = new _AcCm.Transparency(alpha);

            }


            private bool _IsPlottable = false;
            private string _Plot = string.Empty;
            public string Plot
            {
                get { return _Plot; }
                set
                {
                    _Plot = value;
                    SetPlot();
                }
            }

            private void SetPlot()
            {
                _IsPlottable = false;
                if (string.IsNullOrEmpty(_Plot)) return;

                if (string.Compare(_Plot.Trim(), "Ja", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    _IsPlottable = true;
                }

            }

            public string Description { get; set; }

            private bool _Ok = false;
            public bool Ok { get { return _Ok; } }


            public LayerInfo()
            {
            }
            public LayerInfo(_AcDb.LayerTableRecord ltr, _AcDb.Transaction trans)
            {
                OldLayer = ltr.Name;
                NewLayer = "";
                _ColorO = ltr.Color;
                Color = ColorToString();
                _LineTypeO = ltr.LinetypeObjectId;
                _LineType = Engine.GetNameFromLinetypeOid(ltr.LinetypeObjectId, trans);
                _LineWeightO = ltr.LineWeight;
                LineWeight = LineWeightToString();
                _TransparencyO = ltr.Transparency;
                if (_TransparencyO != default(_AcCm.Transparency))
                {
                    Transparency = Engine.AlphaToTransparenz(_TransparencyO.Alpha).ToString(CultureInfo.InvariantCulture);
                }
                else
                {
                    Transparency = string.Empty;
                }
                if (ltr.IsPlottable) Plot = "Ja";
                else Plot = "Nein";

                Description = ltr.Description;
            }

            private string LineWeightToString()
            {
                int lw = (int)_LineWeightO;
                if (lw < 0) return "";

                double d = lw / 100.0;
                return d.ToString("F", CultureInfo.InvariantCulture);
            }

            private void StringToColor()
            {
                if (string.IsNullOrEmpty(_Color))
                {
                    _Errors = _Errors + string.Format(CultureInfo.CurrentCulture, "\nFehler in Eintrag für Layer '{0}': Es ist keine Farbe festgelegt!", OldLayer);
                    return;
                }

                var vals = _Color.Split(new char[] { '/' });
                if (vals.Length != 1 && vals.Length != 3)
                {
                    _Errors = _Errors + string.Format(CultureInfo.CurrentCulture, "\nUngültiger Eintrag für Farbe für Layer '{0}': {1}", OldLayer, _Color);
                    return;
                }

                if (vals.Length == 1)
                {
                    byte index;
                    if (!GetColorInt(vals[0], out index)) return;

                    _ColorO = _AcCm.Color.FromColorIndex(_AcCm.ColorMethod.ByColor, index);
                }
                else
                {
                    // rgb
                    byte rIndex, gIndex, bIndex;
                    if (!GetColorInt(vals[0], out rIndex)) return;
                    if (!GetColorInt(vals[1], out gIndex)) return;
                    if (!GetColorInt(vals[2], out bIndex)) return;

                    _ColorO = _AcCm.Color.FromRgb(rIndex, gIndex, bIndex);

                }


            }

            private bool GetColorInt(string val, out byte index)
            {
                if (!byte.TryParse(val, out index))
                {
                    _Errors = _Errors + string.Format(CultureInfo.CurrentCulture, "\nUngültiger Wert für Farbe für Layer '{0}': {1}", OldLayer, _Color);
                    return false;
                }
                if (index < 0 || index > 256)
                {
                    _Errors = _Errors + string.Format(CultureInfo.CurrentCulture, "\nUngültiger Wert für Farbe für Layer '{0}': {1}", OldLayer, _Color);
                    return false;
                }
                return true;
            }


            private string ColorToString()
            {
                if (_ColorO.IsByAci)
                {
                    return _ColorO.ColorIndex.ToString(CultureInfo.InvariantCulture);
                }
                else
                {
                    return string.Format(CultureInfo.InvariantCulture, "{0}/{1}/{2}",
                        _ColorO.Red.ToString(CultureInfo.InvariantCulture),
                        _ColorO.Green.ToString(CultureInfo.InvariantCulture),
                        _ColorO.Blue.ToString(CultureInfo.InvariantCulture));
                }
            }

            internal List<string> RowAsList()
            {
                return new List<string>() { OldLayer, NewLayer, Color, LineType, LineWeight, Transparency, Plot, Description };
            }


            internal void ModifyLayer(_AcDb.LayerTableRecord ltr, _AcDb.Transaction trans, _AcDb.Database db)
            {

                if (_ColorO != null)
                {
                    ltr.Color = _ColorO;
                }

                if (_LineTypeO != null && !_LineTypeO.IsNull)
                {
                    ltr.LinetypeObjectId = _LineTypeO;
                }

                ltr.LineWeight = _LineWeightO;

                if (_TransparencyO != null && _TransparencyO != default(_AcCm.Transparency))
                {
                    ltr.Transparency = _TransparencyO;
                }
                else
                {
                    ltr.Transparency = default(_AcCm.Transparency);
                }

                if (!string.IsNullOrEmpty(Description)) ltr.Description = Description;

                ltr.IsPlottable = _IsPlottable;

            }
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

        private static _AcDb.ObjectId GetLinetypeFromName(string name, _AcDb.Transaction trans, _AcDb.Database db)
        {
            _AcDb.LinetypeTable acLinTbl;
            acLinTbl = trans.GetObject(db.LinetypeTableId,
                                            _AcDb.OpenMode.ForRead) as _AcDb.LinetypeTable;

            if (acLinTbl.Has(name)) return acLinTbl[name];
            else return default(_AcDb.ObjectId);
        }
        private static string GetNameFromLinetypeOid(_AcDb.ObjectId oid, _AcDb.Transaction trans)
        {
            _AcDb.LinetypeTableRecord ltr = (_AcDb.LinetypeTableRecord)trans.GetObject(oid, _AcDb.OpenMode.ForRead);
            return ltr.Name;

        }
        private static byte TransparenzToAlpha(int transparenz)
        {
            return (Byte)(255 * (100 - transparenz) / 100);
        }
        private static int AlphaToTransparenz(byte alpha)
        {
            return 100 - (100 * alpha / 255);
        }


    }
}
#endif
