using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Globalization;
// ReSharper disable IdentifierTypo
// ReSharper disable CommentTypo
#endif

namespace Plan2Ext.BlockToExcel
{
    public class BlockToExcel
    {
        #region log4net Initialization
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Convert.ToString((typeof(BlockToExcel))));
        #endregion

        #region Constants
        public const string BlockName = "BLOCKNAME";
        public const string Handle = "HANDLE";
        public const string Dwgpath = "DWGPATH";

        #endregion
        #region Member variables
        protected List<_AcDb.ObjectId> BlocksForExcelExport = new List<_AcDb.ObjectId>();
        protected string ExcelFileName = string.Empty;
        protected string DwgPath = string.Empty;
        private string _blockName = string.Empty;
        private bool _createChangeLines = true;

        private readonly Dictionary<string, List<string>> _colsForExcel = new Dictionary<string, List<string>>();
        private readonly List<string> _attributesForExcelExport = new List<string>();

        #endregion

        /// <summary>
        /// Aufruf: (Plan2GetExcelToBlockDwgs ExcelFileName)
        /// </summary>
        /// <param name="rb"></param>
        /// <returns></returns>
        [_AcTrx.LispFunction("Plan2GetExcelToBlockDwgs")]
        public _AcDb.ResultBuffer Plan2GetExcelToBlockDwgs(_AcDb.ResultBuffer rb)
        {
            _AcEd.Editor ed = _AcAp.Application.DocumentManager.MdiActiveDocument.Editor;
            try
            {
                if (rb == null)
                {
                    ShowCallInfoDwgs(ed);
                    return null;
                }
                _AcDb.TypedValue[] values = rb.AsArray();
                if (values == null || values.Length < 1)
                {
                    ShowCallInfoDwgs(ed);
                    return null;

                }
                // Get ExcelFileName from Args
                if (values[0].Value == null)
                {
                    ShowCallInfoDwgs(ed);
                    return null;
                }
                string excelFileName = values[0].Value.ToString();
                if (string.IsNullOrEmpty(excelFileName))
                {
                    ShowCallInfoDwgs(ed);
                    return null;
                }

                return GetDwgNames(excelFileName);
            }
            catch (System.Exception ex)
            {
                log.Error(ex.Message);
                string msg = string.Format(CultureInfo.CurrentCulture, "Fehler in (Plan2GetExcelToBlockDwgs): {0}", ex.Message);
                ed.WriteMessage("\n" + msg);
                System.Windows.Forms.MessageBox.Show(ex.Message, "Plan2GetExcelToBlockDwgs");
                return null;
            }
        }

        
        /// <summary>
        /// Aufruf: (Plan2ExcelToBlock ExcelFileName)
        /// </summary>
        /// <param name="rb"></param>
        /// <returns></returns>
        [_AcTrx.LispFunction("Plan2ExcelToBlock")]
        public bool Plan2ExcelToBlock(_AcDb.ResultBuffer rb)
        {
            _AcEd.Editor ed = _AcAp.Application.DocumentManager.MdiActiveDocument.Editor;
            try
            {
                if (rb == null)
                {
                    ShowCallInfoImport(ed);
                    return false;
                }
                _AcDb.TypedValue[] values = rb.AsArray();
                if (values == null || values.Length < 2)
                {
                    ShowCallInfoImport(ed);
                    return false;
                }
                // Get ExcelFileName from Args
                if (values[0].Value == null)
                {
                    ShowCallInfoImport(ed);
                    return false;
                }
                string excelFileName = values[0].Value.ToString();
                if (string.IsNullOrEmpty(excelFileName))
                {
                    ShowCallInfoImport(ed);
                    return false;
                }

                bool createChangeLines = true;
                switch (values[1].TypeCode)
                {
                    case 5019:
                        createChangeLines = false;
                        break;
                    case 5021:
                        createChangeLines = true;
                        break;
                    default:
                                            ShowCallInfoImport(ed);
                    return false;

                }

                // Start Import
                StartImport(excelFileName,createChangeLines );

                return true;
            }
            catch (System.Exception ex)
            {
                log.Error(ex.Message);
                string msg = string.Format(CultureInfo.CurrentCulture, "Fehler in (Plan2ExcelToBlock): {0}", ex.Message);
                ed.WriteMessage("\n" + msg);
                System.Windows.Forms.MessageBox.Show(ex.Message, "Plan2ExcelToBlock");
                return false;
            }
        }

        /// <summary>
        /// Aufruf: (Plan2BlockToExcel BlockName ExcelFileName)
        /// </summary>
        /// <param name="rb"></param>
        /// <returns></returns>
        [_AcTrx.LispFunction("Plan2BlockToExcel")]
        public bool Plan2BlockToExcel(_AcDb.ResultBuffer rb)
        {
            _AcEd.Editor ed = _AcAp.Application.DocumentManager.MdiActiveDocument.Editor;
            try
            {
                if (rb == null)
                {
                    ShowCallInfo(ed);
                    return false;
                }
                _AcDb.TypedValue[] values = rb.AsArray();
                if (values == null || values.Length < 2)
                {
                    ShowCallInfo(ed);
                    return false;
                }
                // Get Blockname from Args
                if (values[0].Value == null)
                {
                    ShowCallInfo(ed);
                    return false;
                }
                string blockName = values[0].Value.ToString();
                if (string.IsNullOrEmpty(blockName))
                {
                    ShowCallInfo(ed);
                    return false;
                }

                // Get ExcelFileName from Args
                if (values[1].Value == null)
                {
                    ShowCallInfo(ed);
                    return false;
                }
                string excelFileName = values[1].Value.ToString();
                if (string.IsNullOrEmpty(excelFileName))
                {
                    ShowCallInfo(ed);
                    return false;
                }

                // Start Export 
                StartExport(blockName, excelFileName);

                return true;
            }
            catch (System.Exception ex)
            {
                log.Error(ex.Message);
                string msg = string.Format(CultureInfo.CurrentCulture, "Fehler in (Plan2BlockToExcel): {0}", ex.Message);
                ed.WriteMessage("\n" + msg);
                System.Windows.Forms.MessageBox.Show(ex.Message, "Plan2BlockToExcel");
                return false;
            }
        }


        private _AcDb.ResultBuffer GetDwgNames(string excelFileName)
        {
            List<string> dwgNames = new List<string>();
            using (var excelizer = new Excelizer(excelFileName, Excelizer.Direction.Import))
            {
                var importedRows = excelizer.Import();
                var perDwgName = importedRows.GroupBy(x => x.DwgPath);
                _AcDb.ResultBuffer rbRet = new _AcDb.ResultBuffer();
                foreach (var pdn in perDwgName)
                {
                    string dwgName = pdn.Key;
                    dwgNames.Add(dwgName);
                    rbRet.Add(new _AcDb.TypedValue((int)_AcBrx.LispDataType.Text, dwgName));
                }

                var dwgString = string.Join(", ", dwgNames.Select(x => System.IO.Path.GetFileName(x)).ToArray());
                log.InfoFormat("\nDatei '{0}' enthält Informationen folgende dwgs: {1}", excelFileName, dwgString);

                return rbRet;
            }
        }

        private void StartImport(string excelFileName, bool createChangeLines)
        {
            DwgPath = DrawingPath;
            ExcelFileName = excelFileName;
            _createChangeLines = createChangeLines;

            log.InfoFormat("Starte Blockimport für '{0}'.", DwgPath);

            if (!ExcelImport()) return;
        }

        private bool ExcelImport()
        {
            using (var excelizer = new Excelizer(ExcelFileName, Excelizer.Direction.Import))
            {
                var importedRows = excelizer.Import();
                importedRows = importedRows.Where(x => string.Compare(DwgPath, x.DwgPath, StringComparison.OrdinalIgnoreCase) == 0).ToList();

                Import(importedRows);
            }

            return true;
        }

        private bool Import(List<Excelizer.ImportedRow> importedRows)
        {
            bool errorOccured = false;

            var doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            using (_AcDb.Transaction trans = doc.TransactionManager.StartTransaction())
            {
                var perBlockName = importedRows.GroupBy(x => x.BlockName);
                foreach (var pbn in perBlockName)
                {
                    int nrOfChangedBlocks = 0;

                    string blockName = pbn.Key;
                    if (blockName == null || string.IsNullOrEmpty(blockName.Trim()))
                    {
                        log.WarnFormat("Es gibt Zeilen ohne Blocknamen!");
                        continue;
                    }

                    var attNames = Plan2Ext.Globs.GetBlockAttributeNames(blockName);
                    log.InfoFormat("Import für Block '{0}' mit {1} Attribut(en): ", blockName, attNames.Count.ToString());
                    int cnt = 0;
                    foreach (var record in pbn)
                    {
                        try
                        {
                            int changedAttributes = 0;

                            long ln = Convert.ToInt64(record.Handle, 16);
                            // Not create a Handle from the long integer
                            _AcDb.Handle hn = new _AcDb.Handle(ln);
                            // And attempt to get an _AcDb.ObjectId for the Handle
                            _AcDb.ObjectId id = db.GetObjectId(false, hn, 0);

                            _AcDb.BlockReference br = (_AcDb.BlockReference)trans.GetObject(id, _AcDb.OpenMode.ForWrite);

                            cnt++;

                            foreach (var attName in attNames)
                            {
                                _AcDb.AttributeReference attRef = null;
                                attRef = GetBlockAttribute(attName, br,trans);
                                if (attRef != null)
                                {
                                    attRef.UpgradeOpen();
                                    string attVal = record.GetAttribute(attName);
                                    if (attVal != null)
                                    {
                                        if (string.Compare(attRef.TextString,attVal) != 0)
                                        {
                                            if (_createChangeLines )
                                            { 
                                            Plan2Ext.Globs.InsertFehlerLines(new List<_AcGe.Point3d> { attRef.Position }, 
                                                "Plan2BlockAttribChanged",
                                                50.0,
                                                Math.PI * 1.25,
                                                _AcCm.Color.FromRgb((byte)255, (byte)0, (byte)0));
                                            }

                                            changedAttributes++;
                                        }
                                        attRef.TextString = attVal;
                                    }
                                }
                            }
                            if (changedAttributes > 0)
                            {
                                log.DebugFormat("Block mit Referenz '{0}': Anzahl geänderter Attribute: {1}", record.Handle, changedAttributes.ToString());
                                nrOfChangedBlocks++;
                            }
                        }
                        catch (Exception ex)
                        {
                            log.Warn(string.Format("Fehler beim Import von Block mit Handle '{0}'! {1}", record.Handle, ex.Message));
                            errorOccured = true;
                        }

                    }

                    log.InfoFormat("Anzahl geänderter Blöcke namens '{0}': {1}/{2}", blockName, nrOfChangedBlocks, cnt);
                }
                trans.Commit();
            }
            return !errorOccured;
        }


        private void StartExport(string blockName, string excelFileName)
        {
            DwgPath = DrawingPath;
            _blockName = blockName;
            ExcelFileName = excelFileName;

            if (!SelectBlocks()) return;

            if (!GetExcelExportAtts()) return;

            if (!WriteColsForExcel()) return;

            if (!ExcelExport()) return;
        }

        protected bool ExcelExport()
        {
            if (_colsForExcel.Count == 0) return true;
            using (var excelizer = new Excelizer(ExcelFileName, Excelizer.Direction.Export))
            {
                excelizer.Export(_colsForExcel);
            }

            return true;
        }

        protected bool WriteColsForExcel()
        {
            var doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            using (var trans = doc.TransactionManager.StartTransaction())
            {
                var blockRefs = BlocksForExcelExport.Select(oid => (_AcDb.BlockReference)trans.GetObject(oid, _AcDb.OpenMode.ForRead));
                foreach (var br in blockRefs)
                {
                    _colsForExcel[BlockName].Add(Plan2Ext.Globs.GetBlockname(br, trans));
                    _colsForExcel[Handle].Add(br.Handle.ToString());
                    _colsForExcel[Dwgpath].Add(DwgPath);
                    foreach (var attName in _attributesForExcelExport)
                    {
                        var att = GetBlockAttribute(attName, br,trans);
                        if (att == null)
                        {
                            _colsForExcel[attName].Add("");
                        }
                        else
                        {
                            _colsForExcel[attName].Add(att.TextString);
                        }
                    }
                }
                trans.Commit();
            }
            return true;
        }

        private _AcDb.AttributeReference GetBlockAttribute(string name, _AcDb.BlockReference blockEnt, _AcDb.Transaction trans)
        {
            foreach (_AcDb.ObjectId attId in blockEnt.AttributeCollection)
            {
                if (attId.IsErased) continue;
                var anyAttRef = trans.GetObject(attId, _AcDb.OpenMode.ForRead) as _AcDb.AttributeReference;
                if (anyAttRef != null)
                {
                    if (string.Compare(anyAttRef.Tag, name, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return anyAttRef;
                    }
                }
            }
            return null;
        }

        protected bool GetExcelExportAtts()
        {
            _attributesForExcelExport.Clear();
            _colsForExcel.Clear();

            var doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            using (var trans = doc.TransactionManager.StartTransaction())
            {

                //var blockRefs = _BlocksForExcelExport.Select(oid => (_AcDb.BlockReference)trans.GetObject(oid, _AcDb.OpenMode.ForRead)).ToList();
                //List<_AcDb.ObjectId> btrOids = new List<_AcDb.ObjectId>();

                var br = (_AcDb.BlockReference)trans.GetObject(BlocksForExcelExport[0], _AcDb.OpenMode.ForRead);
                var bd = (_AcDb.BlockTableRecord)trans.GetObject(br.BlockTableRecord, _AcDb.OpenMode.ForRead);
                List<string> atts = new List<string>();
                foreach (_AcDb.ObjectId oid in bd)
                {
                    _AcDb.AttributeDefinition adef = trans.GetObject(oid, _AcDb.OpenMode.ForRead) as _AcDb.AttributeDefinition;
                    if (adef != null)
                    {
                        string tagUC = adef.Tag.ToUpperInvariant();
                        _attributesForExcelExport.Add(tagUC);
                    }
                }

                _colsForExcel.Add(BlockName, new List<string> {  });
                _colsForExcel.Add(Handle, new List<string> {  });
                _colsForExcel.Add(Dwgpath, new List<string> {  });
                foreach (var att in _attributesForExcelExport)
                {
                    if (!_colsForExcel.ContainsKey(att))
                    {
                        _colsForExcel.Add(att, new List<string> {  });
                    }
                }

                //_ColsForExcel.Add(BLOCK_NAME, new List<string> { BLOCK_NAME });
                //_ColsForExcel.Add(HANDLE, new List<string> { HANDLE });
                //_ColsForExcel.Add(DWGPATH, new List<string> { DWGPATH });
                //foreach (var att in _AttributesForExcelExport)
                //{
                //    if (!_ColsForExcel.ContainsKey(att))
                //    {
                //        _ColsForExcel.Add(att, new List<string> { att });
                //    }
                //}

                trans.Commit();
            }
            return true;
        }


        public string DrawingPath
        {
            get
            {
                _AcAp.Document doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
                _AcDb.HostApplicationServices hs = _AcDb.HostApplicationServices.Current;

                return hs.FindFile(doc.Name, doc.Database, _AcDb.FindFileHint.Default);
            }
        }

        private bool SelectBlocks()
        {
            var ed = _AcAp.Application.DocumentManager.MdiActiveDocument.Editor;
            _AcEd.SelectionFilter filter = new _AcEd.SelectionFilter(new _AcDb.TypedValue[] { 
                new _AcDb.TypedValue((int)_AcDb.DxfCode.Start,"INSERT" ),
            });

            _AcEd.PromptSelectionResult res = ed.SelectAll(filter);
            //if (res.Status != _AcEd.PromptStatus.OK) return false;

            List<_AcDb.ObjectId> selectedBlocks = new List<_AcDb.ObjectId>();
#if BRX_APP
            _AcEd.SelectionSet ss = res.Value;
#else
            using (_AcEd.SelectionSet ss = res.Value)
#endif
            {
                if (ss == null) return false;
                selectedBlocks.AddRange(ss.GetObjectIds().ToList());
            }

            var doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            using (var trans = doc.TransactionManager.StartTransaction())
            {
                BlocksForExcelExport = selectedBlocks.Where(oid => IsBlockToUse(oid, trans, _blockName)).ToList();
                trans.Commit();
            }

            log.InfoFormat("Anzahl gefundener Blöcke namens '{0}': {1}.",_blockName, BlocksForExcelExport.Count.ToString());

            if (BlocksForExcelExport.Count > 0) return true;
            else return false;
        }

        private bool IsBlockToUse(_AcDb.ObjectId oid, _AcDb.Transaction tr, string blockName)
        {
            var br = tr.GetObject(oid, _AcDb.OpenMode.ForRead) as _AcDb.BlockReference;
            if (br == null) return false;
            string bn = Plan2Ext.Globs.GetBlockname(br, tr);
            if (string.Compare(bn, blockName, StringComparison.OrdinalIgnoreCase) != 0) return false;
            var bd = (_AcDb.BlockTableRecord)tr.GetObject(br.BlockTableRecord, _AcDb.OpenMode.ForRead);
            if (bd.IsFromExternalReference) return false;
            return true;
        }

        private static void ShowCallInfo(_AcEd.Editor ed)
        {
            ed.WriteMessage("\n Aufruf: (Plan2BlockToExcel BlockName ExcelFileName)");
        }
        private static void ShowCallInfoImport(_AcEd.Editor ed)
        {
            ed.WriteMessage("\n Aufruf: (Plan2ExcelToBlock ExcelFileName ShowChangeLines)");
        }
        private static void ShowCallInfoDwgs(_AcEd.Editor ed)
        {
            ed.WriteMessage("\n Aufruf: (Plan2GetExcelToBlockDwgs ExcelFileName)");
        }
        
    }
}
