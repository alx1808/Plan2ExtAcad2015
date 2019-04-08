// ReSharper disable CommentTypo
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices.Core;
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

// ReSharper disable StringLiteralTypo
// ReSharper disable IdentifierTypo
#endif

namespace Plan2Ext.BlockToExcel
{
    // ReSharper disable once UnusedMember.Global
    public class BlockToExcel2 : BlockToExcel
    {
        #region log4net Initialization
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Convert.ToString((typeof(BlockToExcel2))));
        #endregion

        private string _exportPath;
        private readonly Dictionary<string, List<_AcDb.ObjectId>> _blocksForExcelExportDictionary =
            new Dictionary<string, List<_AcDb.ObjectId>>();
        private WildcardAcad _wildcardAcad;

        /// <summary>
        /// Aufruf: (Plan2BlockToExcel BlockName ExcelFileName)
        /// </summary>
        /// <param name="rb"></param>
        /// <returns></returns>
        [_AcTrx.LispFunction("Plan2BlockToExcel2")]
        // ReSharper disable once UnusedMember.Global
        public bool Plan2BlockToExcel2(_AcDb.ResultBuffer rb)
        {
            _AcEd.Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            try
            {
                if (rb == null)
                {
                    ShowCallInfo(ed);
                    return false;
                }
                _AcDb.TypedValue[] values = rb.AsArray();
                if (values == null || values.Length < 1)
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
                var blockNames = values[0].Value.ToString();
                if (string.IsNullOrEmpty(blockNames))
                {
                    ShowCallInfo(ed);
                    return false;
                }

                if (values.Length > 1 && values[1].Value != null)
                {
                    _exportPath = values[1].Value.ToString();
                }
                else
                {
                    _exportPath = Application.GetSystemVariable("DWGPREFIX").ToString();
                }

                Log.InfoFormat(CultureInfo.CurrentCulture, "\nZeichnung: '{0}'", Application.GetSystemVariable("DWGNAME"));

                _wildcardAcad = new WildcardAcad(blockNames);

                // Start Export 
                StartExport();

                return true;
            }
            catch (System.Exception ex)
            {
                Log.Error(ex.Message);
                string msg = string.Format(CultureInfo.CurrentCulture, "Fehler in (Plan2BlockToExcel): {0}", ex.Message);
                ed.WriteMessage("\n" + msg);
                System.Windows.Forms.MessageBox.Show(ex.Message, @"Plan2BlockToExcel2");
                return false;
            }
        }

        private void StartExport()
        {
            _blocksForExcelExportDictionary.Clear();
            DwgPath = DrawingPath;

            if (!SelectBlocks()) return;

            foreach (var kvp in _blocksForExcelExportDictionary)
            {
                var blockName = kvp.Key;
                BlocksForExcelExport = kvp.Value;
                ExcelFileName = System.IO.Path.Combine(_exportPath, Globs.RemoveInvalidCharacters(blockName) + ".xlsx");

                if (!GetExcelExportAtts()) return;
                if (!WriteColsForExcel()) return;
                if (!ExcelExport()) return;
            }
        }

        private static void ShowCallInfo(_AcEd.Editor ed)
        {
            ed.WriteMessage("\n Aufruf: (Plan2BlockToExcel BlockNames [Exportpath])");
        }

        private bool SelectBlocks()
        {
            var ed = Application.DocumentManager.MdiActiveDocument.Editor;
            _AcEd.SelectionFilter filter = new _AcEd.SelectionFilter(new[] { 
                new _AcDb.TypedValue((int)_AcDb.DxfCode.Start,"INSERT" ),
                //new _AcDb.TypedValue((int)_AcDb.DxfCode.BlockName,_blockNames ), // this variant doesn't support dynamic blocks
            });

            _AcEd.PromptSelectionResult res = ed.SelectAll(filter);
            if (res.Status != _AcEd.PromptStatus.OK) return false;

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

            var doc = Application.DocumentManager.MdiActiveDocument;
            using (var trans = doc.TransactionManager.StartTransaction())
            {
                foreach (var objectId in selectedBlocks)
                {
                    var blockReference = (_AcDb.BlockReference)trans.GetObject(objectId, _AcDb.OpenMode.ForRead);
                    var blockName = Globs.GetBlockname(blockReference, trans);
                    if (!_wildcardAcad.IsMatch(blockName)) continue;

                    List<_AcDb.ObjectId> objectIds;
                    if (!_blocksForExcelExportDictionary.TryGetValue(blockName, out objectIds))
                    {
                        objectIds = new List<_AcDb.ObjectId>();
                        _blocksForExcelExportDictionary.Add(blockName, objectIds);
                    }
                    objectIds.Add(objectId);
                }

                //_BlocksForExcelExport = selectedBlocks.Where(oid => IsBlockToUse(oid, trans, _BlockName)).ToList();
                trans.Commit();
            }
            Log.Info("Anzahl gefundener Blöcke: ");
            foreach (var kvp in _blocksForExcelExportDictionary)
            {

                Log.InfoFormat("'{0}': {1}.", kvp.Key, kvp.Value.Count);
            }

            return _blocksForExcelExportDictionary.Count > 0;
        }
    }
}
