//using Autodesk.AutoCAD.ApplicationServices;
//using Autodesk.AutoCAD.DatabaseServices;
//using Autodesk.AutoCAD.EditorInput;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
#endif


namespace Plan2Ext.AutoIdVergabe
{
    internal class Engine
    {
        #region log4net Initialization
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Convert.ToString((typeof(Engine))));
        #endregion

        #region Constants
        private const string ID_NOT_UNIQUE = "ID_NICHT_EINDEUTIG";
        private const string ZU_RAUMBLOCK_OUTSIDE = "BLOCK_MIT_ID_IS_AUSSERHALB";
        private const string ZU_RAUMBLOCK_NORB = "KEIN_RAUMBLOCK";
        private const string ZU_RAUMBLOCK_MORERB = "MEHRERE_RAUMBLOECKE";
        private const string NO_RAUMNR = "Keine_Raumnummer";

        // Excel Export
        public const string BLOCK_NAME = "BLOCKNAME";
        public const string HANDLE = "HANDLE";

        #endregion

        #region Members
        private _AcDb.TransactionManager _TransMan = null;
        private AutoIdOptions _AutoIdOptions = null;

        List<_AcDb.ObjectId> _Raumblocks = new List<_AcDb.ObjectId>();
        List<_AcDb.ObjectId> _ZuRaumIdBlocksOrAttribs = new List<_AcDb.ObjectId>();
        List<_AcDb.ObjectId> _RaumPolygons = new List<_AcDb.ObjectId>();

        List<_AcDb.ObjectId> _ExplodedXrefEntities = new List<_AcDb.ObjectId>();

        // Excel Export
        List<_AcDb.ObjectId> _BlocksForExcelExport = new List<_AcDb.ObjectId>();
        private Dictionary<string, List<string>> _ColsForExcel = new Dictionary<string, List<string>>();
        private List<string> _InvalidAdditionsAttsForExport = new List<string>();
        private List<string> _AttributesForExcelExport = new List<string>();

        private static string _ExcelImportDefaultName = null;

        #endregion

        #region Lifecycle
        public Engine(AutoIdOptions options)
        {
            _AutoIdOptions = options;
            _TransMan = _AcAp.Application.DocumentManager.MdiActiveDocument.Database.TransactionManager;
        }
        #endregion

        #region Internal

        internal void ZuRaumIdVergabe()
        {

            try
            {
                log.Info("ZuRaumIdVergabe");

                CreateLayerIfNotExists(_AutoIdOptions.PolygonLayer);

                ExplodeAllXrefs();

                DeleteErrorSymbols();

                if (!SelectZuRaumIdEntities()) return;

                List<RaumInfo> raumInfos = new List<RaumInfo>();
                foreach (var oid in _RaumPolygons)
                {
                    raumInfos.Add(new RaumInfo(oid, _Raumblocks, _ZuRaumIdBlocksOrAttribs, _TransMan));
                }

                bool noRaumblockOccured = false;
                bool moreRaumblocksOccured = false;

                using (var myTrans = _TransMan.StartTransaction())
                {

                    AssignZuRaumId(raumInfos.Where(x => x.TheStatus == RaumInfo.Status.Ok));

                    ShowZuRaumblockOutside();

                    ShowNoRaumblockHatches(raumInfos, ref noRaumblockOccured);

                    ShowMoreRaumblocksHatches(raumInfos, ref moreRaumblocksOccured);

                    myTrans.Commit();
                }

                StringBuilder sb = new StringBuilder();
                if (_ZuRaumIdBlocksOrAttribs.Count > 0)
                {
                    sb.Append(string.Format(CultureInfo.CurrentCulture, "Anzahl der außerhalb liegenden Blöcke: {0} ", _ZuRaumIdBlocksOrAttribs.Count.ToString()));
                }

                if (noRaumblockOccured)
                {
                    sb.Append(string.Format(CultureInfo.CurrentCulture, "Es gibt Polygone ohne Raumblöcke. "));
                }

                if (moreRaumblocksOccured)
                {
                    sb.Append(string.Format(CultureInfo.CurrentCulture, "Es gibt Polygone mit mehr als einem Raumblock. "));
                }

                string msg = sb.ToString();
                if (!string.IsNullOrEmpty(msg))
                {
                    _AcAp.Application.ShowAlertDialog(msg);
                }
            }
            finally
            {
                DeleteExplodedXrefEntities();
            }
        }

        /// <summary>
        /// Für Attribute
        /// </summary>
        internal void ZuRaumIdVergabeAttribut()
        {
            try
            {
                log.Info("ZuRaumIdVergabe");

                CreateLayerIfNotExists(_AutoIdOptions.PolygonLayer);

                ExplodeAllXrefs();

                DeleteErrorSymbols();

                if (!SelectZuRaumIdEntities(useAttribs: true)) return;

                List<RaumInfo> raumInfos = new List<RaumInfo>();
                foreach (var oid in _RaumPolygons)
                {
                    raumInfos.Add(new RaumInfoForAttributes(oid, _Raumblocks, _ZuRaumIdBlocksOrAttribs, _TransMan));
                }

                bool noRaumblockOccured = false;
                bool moreRaumblocksOccured = false;

                using (var myTrans = _TransMan.StartTransaction())
                {

                    AssignZuRaumId(raumInfos.Where(x => x.TheStatus == RaumInfo.Status.Ok), useAttribs: true);

                    ShowZuRaumblockOutside();

                    ShowNoRaumblockHatches(raumInfos, ref noRaumblockOccured);

                    ShowMoreRaumblocksHatches(raumInfos, ref moreRaumblocksOccured);

                    myTrans.Commit();
                }

                StringBuilder sb = new StringBuilder();
                if (_ZuRaumIdBlocksOrAttribs.Count > 0)
                {
                    sb.Append(string.Format(CultureInfo.CurrentCulture, "Anzahl der außerhalb liegenden Blöcke: {0} ", _ZuRaumIdBlocksOrAttribs.Count.ToString()));
                }

                if (noRaumblockOccured)
                {
                    sb.Append(string.Format(CultureInfo.CurrentCulture, "Es gibt Polygone ohne Raumblöcke. "));
                }

                if (moreRaumblocksOccured)
                {
                    sb.Append(string.Format(CultureInfo.CurrentCulture, "Es gibt Polygone mit mehr als einem Raumblock. "));
                }

                string msg = sb.ToString();
                if (!string.IsNullOrEmpty(msg))
                {
                    _AcAp.Application.ShowAlertDialog(msg);
                }
            }
            finally
            {
                DeleteExplodedXrefEntities();
            }
        }
        
        internal void AssignIds()
        {
            if (!SelectRaumblocks()) return;

            DeleteErrorSymbols();

            using (_AcDb.Transaction myT = _TransMan.StartTransaction())
            {
                foreach (var oid in _Raumblocks)
                {
                    _AcDb.BlockReference blockRef = _TransMan.GetObject(oid, _AcDb.OpenMode.ForRead) as _AcDb.BlockReference;
                    if (blockRef == null) continue;

                    var tuerSchildAtt = GetBlockAttribute(_AutoIdOptions.AttTuerschildnummer, blockRef);
                    if (tuerSchildAtt == null) continue;

                    var idNummerAtt = GetBlockAttribute(_AutoIdOptions.AttIdNummer, blockRef);
                    if (idNummerAtt == null) continue;

                    string tuerSchild = tuerSchildAtt.TextString.Trim();
                    
                    string raumNrInfo;
                    if (!GetNrInfo(tuerSchild,out raumNrInfo))
                    {
                        _AcCm.Color col = _AcCm.Color.FromRgb((byte)255, (byte)10, (byte)40);
                        Plan2Ext.Globs.InsertFehlerLines(new List<_AcGe.Point3d> { tuerSchildAtt.Position }, layerName: NO_RAUMNR, length: 50, ang: Math.PI * 1.25, col: col);
                    }

                    idNummerAtt.UpgradeOpen();
                    string id = string.Format(CultureInfo.InvariantCulture, "{0}-{1}-{2}-{3}-{4}",
                        _AutoIdOptions.Liegenschaft,
                        _AutoIdOptions.Objekt,
                        _AutoIdOptions.Geschoss,
                        _AutoIdOptions.Arial,
                        raumNrInfo);

                    idNummerAtt.TextString = id.ToUpperInvariant();
                }
                myT.Commit();
            }
        }

        internal void CheckUniqueness()
        {
            log.Info("CheckUniqueness");
            if (!SelectRaumblocks()) return;

            //Plan2Ext.Globs.DeleteFehlerBlocks(ID_NOT_UNIQUE);
            Plan2Ext.Globs.DeleteFehlerLines(ID_NOT_UNIQUE);

            Dictionary<string, List<_AcDb.BlockReference>> blocksPerId = new Dictionary<string, List<_AcDb.BlockReference>>();

            List<object> insPoints = new List<object>();

            using (_AcDb.Transaction myT = _TransMan.StartTransaction())
            {
                foreach (var oid in _Raumblocks)
                {
                    _AcDb.BlockReference blockRef = _TransMan.GetObject(oid, _AcDb.OpenMode.ForRead) as _AcDb.BlockReference;
                    if (blockRef == null) continue;

                    var idNummerAtt = GetBlockAttribute(_AutoIdOptions.AttIdNummer, blockRef);
                    if (idNummerAtt == null) continue;

                    string id = idNummerAtt.TextString.Trim().ToUpperInvariant();

                    if (string.IsNullOrEmpty(id)) continue;


                    List<_AcDb.BlockReference> blocks;
                    if (!blocksPerId.TryGetValue(id, out blocks))
                    {
                        blocks = new List<_AcDb.BlockReference>();
                        blocksPerId[id] = blocks;
                    }
                    blocks.Add(blockRef);
                }

                foreach (var kvp in blocksPerId)
                {
                    if (kvp.Value.Count > 1)
                    {
                        foreach (var bref in kvp.Value)
                        {
                            insPoints.Add(new double[] { bref.Position.X, bref.Position.Y, bref.Position.Z });
                        }
                    }
                }

                myT.Commit();
            }

            _AcCm.Color col = _AcCm.Color.FromRgb((byte)255, (byte)10, (byte)40);
            Plan2Ext.Globs.InsertFehlerLines(insPoints, layerName: ID_NOT_UNIQUE, length: 50, ang: Math.PI * 1.25, col: col);
            //Plan2Ext.Globs.InsertFehlerBlocks(insPoints, layerName: ID_NOT_UNIQUE);
        }
#if ARX_APP
        internal bool ExcelExport()
        {

            log.Info("ExcelExport");

            if (!SelectBlocks()) return true;

            if (!GetExcelExportAtts()) return true;

            if (!WriteColsForExcel()) return true;

            WriteToExcel();

            return (_InvalidAdditionsAttsForExport.Count == 0);
        }

        internal bool ExcelImport()
        {
            string fileName = GetExcelImportFileName();
            if (string.IsNullOrEmpty(fileName)) return true;

            ExcelExport ex = new ExcelExport();
            var biis = ex.Import(fileName);

            if (!Import(biis)) return false;

            return true;
        }

        private bool Import(List<AutoIdVergabe.ExcelExport.BlockImportInfo> biis)
        {
            bool errorOccured = false;

            var doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            using (_AcDb.Transaction trans = doc.TransactionManager.StartTransaction())
            {
                foreach (var bii in biis)
                {
                    try
                    {
                        long ln = Convert.ToInt64(bii.Handle, 16);
                        // Not create a Handle from the long integer
                        _AcDb.Handle hn = new _AcDb.Handle(ln);
                        // And attempt to get an _AcDb.ObjectId for the Handle
                        _AcDb.ObjectId id = db.GetObjectId(false, hn, 0);

                        _AcDb.BlockReference br = (_AcDb.BlockReference)trans.GetObject(id, _AcDb.OpenMode.ForWrite);
                        foreach (var kvp in bii.Attributes)
                        {
                            string attName = kvp.Key;
                            string attVal = kvp.Value;
                            _AcDb.AttributeReference attRef = null;
                            attRef = GetBlockAttribute(attName, br);

                            if (attRef != null)
                            {
                                attRef.UpgradeOpen();
                                attRef.TextString = attVal;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Warn(string.Format("Fehler beim Import von Block mit Handle '{0}'! {1}", bii.Handle, ex.Message));
                        errorOccured = true;
                    }

                }
                trans.Commit();
            }

            return !errorOccured;
        }
        private string GetExcelImportFileName()
        {
            _AcAp.Document doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            _AcDb.Database db = doc.Database;
            _AcEd.Editor ed = doc.Editor;

            _AcWnd.OpenFileDialog ofd = new _AcWnd.OpenFileDialog(
                "Excel-Datei für Import wählen", _ExcelImportDefaultName, "xls;xlsx", "ExcelFileToImport",
                _AcWnd.OpenFileDialog.OpenFileDialogFlags.AllowAnyExtension

              );

            System.Windows.Forms.DialogResult dr = ofd.ShowDialog();

            if (dr != System.Windows.Forms.DialogResult.OK) return string.Empty;

            _ExcelImportDefaultName = ofd.Filename;

            return ofd.Filename;
        }
#endif


        #endregion

        #region Excel-Export


        private void WriteToExcel()
        {
#if ARX_APP
            var export = new ExcelExport();
            export.Export(_ColsForExcel);
#endif
        }

        private bool WriteColsForExcel()
        {
            var doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            using (var trans = doc.TransactionManager.StartTransaction())
            {
                var blockRefs = _BlocksForExcelExport.Select(oid => (_AcDb.BlockReference)trans.GetObject(oid, _AcDb.OpenMode.ForRead));


                foreach (var br in blockRefs)
                {
                    _ColsForExcel[BLOCK_NAME].Add(Plan2Ext.Globs.GetBlockname(br, trans));
                    _ColsForExcel[HANDLE].Add(br.Handle.ToString());
                    foreach (var attName in _AttributesForExcelExport)
                    {
                        var att = GetBlockAttribute(attName, br);
                        if (att == null)
                        {
                            _ColsForExcel[attName].Add("");
                        }
                        else
                        {
                            _ColsForExcel[attName].Add(att.TextString);
                        }
                    }
                }

                trans.Commit();
            }
            return true;
        }

        private struct BtrInfo
        {
            public _AcDb.ObjectId BlockTableRec { get; set; }
            public string BlockName { get; set; }
            public List<string> Attribs { get; set; }
        }

        private bool GetExcelExportAtts()
        {
            _AttributesForExcelExport.Clear();
            _InvalidAdditionsAttsForExport.Clear();
            _ColsForExcel.Clear();

            var doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            using (var trans = doc.TransactionManager.StartTransaction())
            {

                var blockRefs = _BlocksForExcelExport.Select(oid => (_AcDb.BlockReference)trans.GetObject(oid, _AcDb.OpenMode.ForRead));
                List<_AcDb.ObjectId> btrOids = new List<_AcDb.ObjectId>();

                List<BtrInfo> btrInfos = new List<BtrInfo>();
                foreach (var br in blockRefs)
                {
                    if (!btrOids.Contains(br.BlockTableRecord))
                    {
                        btrOids.Add(br.BlockTableRecord);
                        string blockName = Plan2Ext.Globs.GetBlockname(br, trans);

                        var bd = (_AcDb.BlockTableRecord)trans.GetObject(br.BlockTableRecord, _AcDb.OpenMode.ForRead);
                        List<string> atts = new List<string>();
                        foreach (_AcDb.ObjectId oid in bd)
                        {
                            _AcDb.AttributeDefinition adef = trans.GetObject(oid, _AcDb.OpenMode.ForRead) as _AcDb.AttributeDefinition;
                            if (adef != null)
                            {
                                string tagUC = adef.Tag.ToUpperInvariant();
                                atts.Add(tagUC);
                            }
                        }
                        btrInfos.Add(new BtrInfo() { BlockName = blockName, BlockTableRec = br.BlockTableRecord, Attribs = atts });
                    }
                }

                var blockInfos = btrInfos.Select(info => new { Name = info.BlockName.ToUpperInvariant(), Attributes = info.Attribs }).OrderBy(x => x.Attributes.Count).ToList();

                //var blockInfos = btrOids.Select((btrOid) =>
                //{
                //    var bd = (_AcDb.BlockTableRecord)trans.GetObject(btrOid, _AcDb.OpenMode.ForRead);
                //    List<string> atts = new List<string>();
                //    foreach (_AcDb.ObjectId oid in bd)
                //    {

                //        _AcDb.AttributeDefinition adef = trans.GetObject(oid, _AcDb.OpenMode.ForRead) as _AcDb.AttributeDefinition;
                //        if (adef != null)
                //        {
                //            string tagUC = adef.Tag.ToUpperInvariant();
                //            atts.Add(tagUC);
                //        }
                //    }
                //    return new { Name = bd.Name.ToUpperInvariant(), Attributes = atts };
                //}).OrderBy(x => x.Attributes.Count).ToList();

                if (blockInfos.Count == 0) return false;
                _AttributesForExcelExport.AddRange(blockInfos[0].Attributes);
                for (int i = 1; i < blockInfos.Count; i++)
                {

                    foreach (var att in blockInfos[i].Attributes)
                    {
                        if (!_AttributesForExcelExport.Contains(att))
                        {
                            _AttributesForExcelExport.Add(att);
                            log.WarnFormat("Der Block '{0}' hat ein zusätzliches Attribut '{1}'.", blockInfos[i].Name, att);
                            if (!_InvalidAdditionsAttsForExport.Contains(att))
                            {
                                _InvalidAdditionsAttsForExport.Add(att);
                            }
                        }
                    }

                }
                _ColsForExcel.Add(BLOCK_NAME, new List<string> { BLOCK_NAME });
                foreach (var att in _AttributesForExcelExport)
                {
                    if (!_ColsForExcel.ContainsKey(att))
                    {
                        _ColsForExcel.Add(att, new List<string> { att });
                    }
                }
                _ColsForExcel.Add(HANDLE, new List<string> { HANDLE });

                trans.Commit();
            }

            return true;

        }

        private List<string> GetAttributDefNames(_AcDb.BlockReference br, _AcDb.Transaction trans)
        {
            List<string> atts = new List<string>();
            var bd = (_AcDb.BlockTableRecord)trans.GetObject(br.BlockTableRecord, _AcDb.OpenMode.ForRead);
            foreach (_AcDb.ObjectId oid in bd)
            {

                _AcDb.AttributeDefinition adef = trans.GetObject(oid, _AcDb.OpenMode.ForRead) as _AcDb.AttributeDefinition;
                if (adef != null)
                {
                    string tagUC = adef.Tag.ToUpperInvariant();
                    atts.Add(tagUC);
                }
            }
            return atts;
        }

        private bool SelectBlocks()
        {
            var ed = _AcAp.Application.DocumentManager.MdiActiveDocument.Editor;
            _AcEd.SelectionFilter filter = new _AcEd.SelectionFilter(new _AcDb.TypedValue[] { 
                new _AcDb.TypedValue((int)_AcDb.DxfCode.Start,"INSERT" ),
            });

            _AcEd.PromptSelectionResult res = ed.GetSelection(filter); // ed.SelectAll(filter);
            if (res.Status != _AcEd.PromptStatus.OK) return false;

            List<_AcDb.ObjectId> selectedBlocks = new List<_AcDb.ObjectId>();
#if BRX_APP
            _AcEd.SelectionSet ss = res.Value;
#else
            using (_AcEd.SelectionSet ss = res.Value)
#endif
            {
                selectedBlocks.AddRange(ss.GetObjectIds().ToList());
            }

            var doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            using (var trans = doc.TransactionManager.StartTransaction())
            {
                _BlocksForExcelExport = selectedBlocks.Where(oid => !IsXRef(oid, trans)).ToList();
                trans.Commit();
            }

            if (_BlocksForExcelExport.Count > 0) return true;
            else return false;

        }

        private bool IsXRef(_AcDb.ObjectId oid, _AcDb.Transaction tr)
        {
            var br = tr.GetObject(oid, _AcDb.OpenMode.ForRead) as _AcDb.BlockReference;
            if (br != null)
            {
                var bd = (_AcDb.BlockTableRecord)tr.GetObject(br.BlockTableRecord, _AcDb.OpenMode.ForRead);
                if (bd.IsFromExternalReference) return true;
            }
            return false;
        }

        #endregion

        #region XREF-Handling

        private void ExplodeAllXrefs()
        {
            log.Debug("ExplodeAllXrefs");
            _ExplodedXrefEntities.Clear();
            // todo

            var doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            List<_AcDb.ObjectId> allXrefsInMs = new List<_AcDb.ObjectId>();
            GetAllMsXrefs(db, allXrefsInMs);

            // todo
            List<_AcDb.ObjectId> blockRefs = new List<_AcDb.ObjectId>();
            InsertBlocks(db, allXrefsInMs, blockRefs);

            List<_AcDb.ObjectId> newlyCreatedObjects = new List<_AcDb.ObjectId>();
            // todo
            ExplodeBlocks(db, blockRefs, newlyCreatedObjects, deleteRef: true, deleteBtr: true);

            _ExplodedXrefEntities.AddRange(newlyCreatedObjects);
        }

        private void InsertBlocks(_AcDb.Database db, List<_AcDb.ObjectId> allXrefsInMs, List<_AcDb.ObjectId> _BlockRefs)
        {
            using (_AcDb.Transaction tr = db.TransactionManager.StartTransaction())
            {
                foreach (var oid in allXrefsInMs)
                {
                    var br = tr.GetObject(oid, _AcDb.OpenMode.ForRead) as _AcDb.BlockReference;
                    if (br != null)
                    {
                        var bd = (_AcDb.BlockTableRecord)tr.GetObject(br.BlockTableRecord, _AcDb.OpenMode.ForRead);
                        if (bd.IsFromExternalReference)
                        {
                            string name = bd.PathName;
                            var dwgPath = _AcAp.Application.GetSystemVariable("DWGPREFIX").ToString();
                            if (System.IO.Path.IsPathRooted(bd.PathName))
                            {
                                var blockOid = Plan2Ext.Globs.InsertDwg(bd.PathName, br.Position, br.Rotation, br.Name + "_AS_BLOCK");
                                _BlockRefs.Add(blockOid);
                            }
                            else
                            {
                                var blockOid = Plan2Ext.Globs.InsertDwg(System.IO.Path.GetFullPath(dwgPath + bd.PathName), br.Position, br.Rotation, br.Name + "_AS_BLOCK");
                                _BlockRefs.Add(blockOid);
                            }
                        }
                    }

                }

                tr.Commit();
            }
        }

        private void ExplodeBlocks(_AcDb.Database db, List<_AcDb.ObjectId> allXrefsInMs, List<_AcDb.ObjectId> newlyCreatedObjects, bool deleteRef, bool deleteBtr)
        {
            log.Debug("ExplodeXRefs");
            using (_AcDb.Transaction tr = _TransMan.StartTransaction())
            {
                _AcDb.BlockTable bt = (_AcDb.BlockTable)tr.GetObject(db.BlockTableId, _AcDb.OpenMode.ForRead);

                _AcDb.BlockTableRecord btr = (_AcDb.BlockTableRecord)tr.GetObject(_AcDb.SymbolUtilityServices.GetBlockModelSpaceId(db), _AcDb.OpenMode.ForWrite);


                foreach (var oid in allXrefsInMs)
                {
                    _AcDb.DBObjectCollection objs = new _AcDb.DBObjectCollection();
                    _AcDb.BlockReference block = (_AcDb.BlockReference)tr.GetObject(oid, _AcDb.OpenMode.ForRead);
                    block.Explode(objs);

                    _AcDb.ObjectId blockRefTableId = block.BlockTableRecord;


                    foreach (_AcDb.DBObject obj in objs)
                    {
                        _AcDb.Entity ent = (_AcDb.Entity)obj;
                        btr.AppendEntity(ent);
                        tr.AddNewlyCreatedDBObject(ent, true);

                        newlyCreatedObjects.Add(ent.ObjectId);
                    }

                    if (deleteRef)
                    {
                        block.UpgradeOpen();
                        block.Erase();
                    }

                    if (deleteBtr)
                    {
                        // funkt nicht -> xref würde gelöscht
                        var bd = (_AcDb.BlockTableRecord)tr.GetObject(blockRefTableId, _AcDb.OpenMode.ForWrite);
                        bd.Erase();

                    }
                }
                tr.Commit();
            }
        }

        private void GetAllMsXrefs(_AcDb.Database db, List<_AcDb.ObjectId> allXrefsInMs)
        {
            log.Debug("GetAllMsXrefs");
            using (_AcDb.Transaction tr = _TransMan.StartTransaction())
            {
                _AcDb.BlockTable bt = (_AcDb.BlockTable)tr.GetObject(db.BlockTableId, _AcDb.OpenMode.ForRead);

                _AcDb.BlockTableRecord btr = (_AcDb.BlockTableRecord)tr.GetObject(_AcDb.SymbolUtilityServices.GetBlockModelSpaceId(db), _AcDb.OpenMode.ForRead);

                foreach (var oid in btr)
                {
                    var br = tr.GetObject(oid, _AcDb.OpenMode.ForRead) as _AcDb.BlockReference;
                    if (br != null)
                    {
                        var bd = (_AcDb.BlockTableRecord)tr.GetObject(br.BlockTableRecord, _AcDb.OpenMode.ForRead);
                        if (bd.IsFromExternalReference)
                        {
                            allXrefsInMs.Add(br.ObjectId);
                        }
                    }
                }

                tr.Commit();
            }
        }

        private void DeleteExplodedXrefEntities()
        {
            using (_AcDb.Transaction tr = _TransMan.StartTransaction())
            {
                foreach (var oid in _ExplodedXrefEntities)
                {
                    var ent = tr.GetObject(oid, _AcDb.OpenMode.ForWrite);
                    ent.Erase();
                }

                tr.Commit();
            }
        }

        public static int XrefCompare(string name, string xrefName, StringComparison sc)
        {
            var parts = xrefName.Split(new char[] { '|' });
            return string.Compare(name, parts[parts.Length - 1], sc);
        }

        public static string RemoveXRefPart(string name)
        {
            var parts = name.Split(new char[] { '|' });
            return parts[parts.Length - 1];
        }

        #endregion

        #region Private
        private bool GetNrInfo(string tuerSchild, out string raumNr)
        {
            bool ret = false;
            raumNr = "000";
            if (_AutoIdOptions.RaumnummerAbStelle > tuerSchild.Length) return ret;

            // vorlauf: ab gewissen zeichen restliche zeichen entfernen
            var specialChars = new char[] { ' ', '(', ')' };
            tuerSchild = RemoveEverythingAfterPosition(tuerSchild, specialChars);

            string rest = tuerSchild.Remove(0, _AutoIdOptions.RaumnummerAbStelle - 1);

            int inclStelle = _AutoIdOptions.RaumnummerBisStelle + 1;
            if (inclStelle > _AutoIdOptions.RaumnummerAbStelle)
            {
                int len = inclStelle - _AutoIdOptions.RaumnummerAbStelle;
                if (rest.Length > len)
                {
                    rest = rest.Substring(0, len);
                }
            }

            // take maximal first three digits - pad 0 on the left
            StringBuilder sb = new StringBuilder();
            var cnt = 0;
            foreach (var c in rest.ToCharArray())
            {
                if (cnt == 3) break;
                int i = 0;
                string s = c.ToString();
                if (!int.TryParse(s,out i)) break;
                sb.Append(i.ToString());
                cnt++;
            }
            raumNr  = sb.ToString();
            if (!string.IsNullOrEmpty(raumNr))
            {
                rest = rest.Remove(0,raumNr.Length);
                ret = true;
            }
            raumNr = raumNr.PadLeft(3, '0');

            // only letter and digits in rest
            rest = OnlyLetterAndDigits(rest);

            if (!string.IsNullOrEmpty(rest))
            {
                raumNr += "-" + rest;
            }

            return ret;
        }

        private string OnlyLetterAndDigits(string txt)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var c in txt.ToCharArray())
            {
                if (char.IsLetterOrDigit(c)) sb.Append(c);
            }
            return sb.ToString();
        }

        private string RemoveEverythingAfterPosition(string txt, char[] specialChars)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var c in txt.ToCharArray())
            {
                if (specialChars.Contains(c)) break;
                sb.Append(c);
            }
            return sb.ToString();
        }

        private bool SelectZuRaumIdEntities(bool useAttribs = false)
        {
            _Raumblocks.Clear();
            _ZuRaumIdBlocksOrAttribs.Clear();
            _RaumPolygons.Clear();

            LayerOnAndThawThatEndsWith(_AutoIdOptions.PolygonLayer);

            var ed = _AcAp.Application.DocumentManager.MdiActiveDocument.Editor;
            _AcEd.SelectionFilter filter = new _AcEd.SelectionFilter(new _AcDb.TypedValue[] { 
                new _AcDb.TypedValue((int)_AcDb.DxfCode.Operator,"<OR" ),
                new _AcDb.TypedValue((int)_AcDb.DxfCode.Start,"INSERT" ),
                new _AcDb.TypedValue((int)_AcDb.DxfCode.Operator,"<AND" ),
                    new _AcDb.TypedValue((int)_AcDb.DxfCode.Start,"*POLYLINE" ),
                    new _AcDb.TypedValue((int)_AcDb.DxfCode.LayerName, "*" + _AutoIdOptions.PolygonLayer),
                new _AcDb.TypedValue((int)_AcDb.DxfCode.Operator,"AND>" ),
                new _AcDb.TypedValue((int)_AcDb.DxfCode.Operator,"OR>" ),

            });

            _AcEd.PromptSelectionResult res = ed.GetSelection(filter);
            if (res.Status != _AcEd.PromptStatus.OK) return false;


            List<_AcDb.ObjectId> allEntities = null;
#if BRX_APP
            _AcEd.SelectionSet ss = res.Value;
#else
            using (_AcEd.SelectionSet ss = res.Value)
#endif
            {
                allEntities = ss.GetObjectIds().ToList();
            }

            if (allEntities == null || allEntities.Count == 0) return false;


            using (_AcDb.Transaction myT = _TransMan.StartTransaction())
            {

                foreach (var oid in allEntities)
                {
                    _AcDb.Entity ent = _TransMan.GetObject(oid, _AcDb.OpenMode.ForRead) as _AcDb.Entity;
                    if (ent == null) continue;
                    _AcDb.BlockReference blockRef = ent as _AcDb.BlockReference;
                    if (blockRef != null)
                    {
                        var bd = (_AcDb.BlockTableRecord)_TransMan.GetObject(blockRef.BlockTableRecord, _AcDb.OpenMode.ForRead);
                        if (bd.IsFromExternalReference)
                        {
                            continue;
                        }

                        string blockName = Plan2Ext.Globs.GetBlockname(blockRef, myT);
                        if (XrefCompare(_AutoIdOptions.Blockname, blockName, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            //Raumblocks
                            _Raumblocks.Add(oid);
                        }
                        else
                        {
                            //ZumRaumIdBlocks
                            foreach (var zuw in _AutoIdOptions.Zuweisungen)
                            {
                                var att = GetBlockAttribute(zuw.ToAtt, blockRef);
                                if (att != null)
                                {
                                    if (useAttribs)
                                    {
                                        _ZuRaumIdBlocksOrAttribs.Add(att.ObjectId);
                                    }
                                    else
                                    {
                                        _ZuRaumIdBlocksOrAttribs.Add(oid);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        //Polylines
                        if (XrefCompare(_AutoIdOptions.PolygonLayer, ent.Layer, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            _RaumPolygons.Add(oid);
                        }
                    }
                }
                myT.Commit();
            }

            if (_Raumblocks.Count == 0) log.Warn("Es wurden keine Raumblöcke gefunden!");
            else log.InfoFormat(CultureInfo.CurrentCulture, "Anzahl der Raumblöcke: {0}", _Raumblocks.Count);

            if (useAttribs)
            {
                if (_ZuRaumIdBlocksOrAttribs.Count == 0) log.Warn("Es wurden keine Blöcke mit passenden Attributen gefunden!");
                else log.InfoFormat(CultureInfo.CurrentCulture, "Anzahl der passenden Attribute: {0}", _ZuRaumIdBlocksOrAttribs.Count);
            }
            else
            {
                if (_ZuRaumIdBlocksOrAttribs.Count == 0) log.Warn("Es wurden keine Blöcke mit passenden Attributen gefunden!");
                else log.InfoFormat(CultureInfo.CurrentCulture, "Anzahl der Blöcke mit passenden Attributen: {0}", _ZuRaumIdBlocksOrAttribs.Count);
            }

            if (_RaumPolygons.Count == 0) log.Warn("Es wurden keine Raumpolylinien gefunden!");
            else log.InfoFormat(CultureInfo.CurrentCulture, "Anzahl der Raumpolylinien: {0}", _RaumPolygons.Count);

            if (_Raumblocks.Count > 0 && _ZuRaumIdBlocksOrAttribs.Count > 0 && _RaumPolygons.Count > 0) return true;
            else return false;
        }

        private void CreateLayerIfNotExists(string layerName)
        {
            if (string.IsNullOrEmpty(layerName.Trim())) return;
            var doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            using (_AcDb.Transaction trans = doc.TransactionManager.StartTransaction())
            {
                try
                {
                    _AcDb.LayerTable layTb = trans.GetObject(db.LayerTableId, _AcDb.OpenMode.ForRead) as _AcDb.LayerTable;
                    if (!layTb.Has(layerName))
                    {
                        log.InfoFormat("Layer {0} wird erstellt.", layerName); ;
                        using (_AcDb.LayerTableRecord acLyrTblRec = new _AcDb.LayerTableRecord())
                        {
                            // Assign the layer a name
                            acLyrTblRec.Name = layerName;

                            // Upgrade the Layer table for write
                            layTb.UpgradeOpen();

                            // Append the new layer to the Layer table and the transaction
                            layTb.Add(acLyrTblRec);
                            trans.AddNewlyCreatedDBObject(acLyrTblRec, true);
                        }
                    }
                    else
                    {
                        log.InfoFormat("Layer {0} existiert bereits.", layerName); ;
                    }
                }
                finally
                {
                    trans.Commit();
                }
            }
        }

        private void LayerOnAndThawThatEndsWith(string layerName)
        {
            var doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            using (_AcDb.Transaction trans = doc.TransactionManager.StartTransaction())
            {
                try
                {
                    _AcDb.LayerTable layTb = trans.GetObject(db.LayerTableId, _AcDb.OpenMode.ForRead) as _AcDb.LayerTable;
                    foreach (var layId in layTb)
                    {
                        _AcDb.LayerTableRecord ltr = trans.GetObject(layId, _AcDb.OpenMode.ForRead) as _AcDb.LayerTableRecord;
                        if (ltr.Name.EndsWith(layerName, StringComparison.OrdinalIgnoreCase))
                        {
                            log.InfoFormat("Taue und schalte Layer {0} ein.", ltr.Name);
                            ltr.UpgradeOpen();
                            ltr.IsOff = false;
                            if (string.Compare(_AcAp.Application.GetSystemVariable("CLAYER").ToString(), ltr.Name, StringComparison.OrdinalIgnoreCase) != 0)
                            {
                                ltr.IsFrozen = false;
                            }

                        }
                    }
                }
                finally
                {
                    trans.Commit();
                }
            }
        }

        //private void SelectZuRaumIdEntitiesInXRef(_AcDb.BlockReference xRef)
        //{
        //    DBObjectCollection objs = new DBObjectCollection();
        //    xRef.Explode(objs);

        //    foreach (DBObject obj in objs)
        //    {

        //        _AcDb.Entity ent = (_AcDb.Entity)obj;
        //        _AcDb.BlockReference blockRef = ent as _AcDb.BlockReference;
        //        if (blockRef != null)
        //        {
        //            if (XrefCompare(_AutoIdOptions.Blockname, blockRef.Name, StringComparison.OrdinalIgnoreCase) == 0)
        //            {
        //                //Raumblocks
        //                _Raumblocks.Add(blockRef.ObjectId );
        //            }
        //            else
        //            {
        //                //ZumRaumIdBlocks
        //                var idNummerAtt = GetBlockAttribute(_AutoIdOptions.ZuRaumIdAtt, blockRef);
        //                if (idNummerAtt == null) continue;
        //                _ZuRaumIdBlocks.Add(blockRef.ObjectId );
        //            }
        //        }
        //        else
        //        {
        //            Polyline2d polyline = ent as Polyline2d;
        //            if (polyline != null)
        //            {
        //                if (XrefCompare(polyline.Layer, _AutoIdOptions.PolygonLayer, StringComparison.OrdinalIgnoreCase) == 0)
        //                {
        //                    //Polylines
        //                    _RaumPolygons.Add(polyline.ObjectId);
        //                }
        //            }
        //        }


        //    }
        //}


        private bool SelectRaumblocks()
        {
            _Raumblocks.Clear();

            string hkBlockName = _AutoIdOptions.Blockname; // TheConfiguration.GetValueString(HK_BLOCKNAME_KONFIG);
            var ed = _AcAp.Application.DocumentManager.MdiActiveDocument.Editor;
            _AcEd.SelectionFilter filter = new _AcEd.SelectionFilter(new _AcDb.TypedValue[] { 
                new _AcDb.TypedValue((int)_AcDb.DxfCode.Start,"INSERT" ),
                //new _AcDb.TypedValue((int)_AcDb.DxfCode.BlockName,hkBlockName) // doesn't work with dynamic blocks
            });

            _AcEd.PromptSelectionResult res = ed.GetSelection(filter); // ed.SelectAll(filter);
            if (res.Status != _AcEd.PromptStatus.OK) return false;


#if BRX_APP
            _AcEd.SelectionSet ss = res.Value;
#else
            using (_AcEd.SelectionSet ss = res.Value)
#endif
            {

                var allSelectedIds = ss.GetObjectIds().ToList();

                using (var myTrans = _TransMan.StartTransaction())
                {
                    foreach (var oid in allSelectedIds)
                    {
                        var bt = _TransMan.GetObject(oid, _AcDb.OpenMode.ForRead) as _AcDb.BlockReference;
                        if (string.Compare(hkBlockName, Plan2Ext.Globs.GetBlockname(bt, myTrans), StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            _Raumblocks.Add(oid);
                        }
                    }
                    myTrans.Commit();
                }
                //_Raumblocks.AddRange(ss.GetObjectIds().ToList());

                if (_Raumblocks.Count > 0) return true;
                else return false;
            }

        }

        private bool SelectAllRaumblocks()
        {
            _Raumblocks.Clear();

            string hkBlockName = _AutoIdOptions.Blockname;
            var ed = _AcAp.Application.DocumentManager.MdiActiveDocument.Editor;
            _AcEd.SelectionFilter filter = new _AcEd.SelectionFilter(new _AcDb.TypedValue[] { 
                new _AcDb.TypedValue((int)_AcDb.DxfCode.Start,"INSERT" ),
                new _AcDb.TypedValue((int)_AcDb.DxfCode.BlockName,hkBlockName)
            });

            _AcEd.PromptSelectionResult res = ed.SelectAll(filter);
            if (res.Status != _AcEd.PromptStatus.OK) return false;

#if BRX_APP
            _AcEd.SelectionSet ss = res.Value;
#else
            using (_AcEd.SelectionSet ss = res.Value)
#endif
            {
                _Raumblocks.AddRange(ss.GetObjectIds().ToList());
                if (_Raumblocks.Count > 0) return true;
                else return false;
            }

        }

        private bool SelectAllRaumPolygons()
        {
            _RaumPolygons.Clear();

            var ed = _AcAp.Application.DocumentManager.MdiActiveDocument.Editor;
            _AcEd.SelectionFilter filter = new _AcEd.SelectionFilter(new _AcDb.TypedValue[] { 
                new _AcDb.TypedValue((int)_AcDb.DxfCode.Start,"*POLYLINE" ),
                new _AcDb.TypedValue((int)_AcDb.DxfCode.LayerName, _AutoIdOptions.PolygonLayer)
            });

            _AcEd.PromptSelectionResult res = ed.SelectAll(filter);
            if (res.Status != _AcEd.PromptStatus.OK) return false;

#if BRX_APP
            _AcEd.SelectionSet ss = res.Value;
#else
            using (_AcEd.SelectionSet ss = res.Value)
#endif
            {
                _RaumPolygons.AddRange(ss.GetObjectIds().ToList());
                if (_RaumPolygons.Count > 0) return true;
                else return false;
            }

        }
        private _AcDb.AttributeReference GetBlockAttribute(string name, _AcDb.BlockReference blockEnt)
        {
            foreach (_AcDb.ObjectId attId in blockEnt.AttributeCollection)
            {
                var anyAttRef = _TransMan.GetObject(attId, _AcDb.OpenMode.ForRead) as _AcDb.AttributeReference;
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

        private string GetBlockAttribute(string attName, _AcDb.ObjectId block)
        {
            _AcDb.BlockReference blockEnt = _TransMan.GetObject(block, _AcDb.OpenMode.ForRead) as _AcDb.BlockReference;
            if (blockEnt != null)
            {
                _AcDb.AttributeReference attRef = null;

                attRef = GetBlockAttribute(attName, blockEnt);

                if (attRef != null)
                {
                    return attRef.TextString;
                }

            }
            return string.Empty;
        }

        private void SetAttribValue(_AcDb.ObjectId oid, string attName, string val)
        {
            if (oid == _AcDb.ObjectId.Null) return;

            _AcDb.AttributeReference attRef = _TransMan.GetObject(oid, _AcDb.OpenMode.ForRead) as _AcDb.AttributeReference;
            if (attRef != null)
            {
                if (attName == attRef.Tag)
                {
                    attRef.UpgradeOpen();
                    attRef.TextString = val;
                }
            }
        }
        private void SetBlockAttrib(_AcDb.ObjectId oid, string attName, string val, bool tolerant = false)
        {
            if (oid == _AcDb.ObjectId.Null) return;

            _AcDb.BlockReference blockEnt = _TransMan.GetObject(oid, _AcDb.OpenMode.ForRead) as _AcDb.BlockReference;
            if (blockEnt != null)
            {
                _AcDb.AttributeReference attRef = null;

                attRef = GetBlockAttribute(attName, blockEnt);

                if (attRef != null)
                {
                    attRef.UpgradeOpen();
                    attRef.TextString = val;
                }
                else
                {
                    if (!tolerant)
                    {
                        throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Attribut '{0}' nicht gefunden!", attName));
                    }
                }
            }
        }
        #endregion

        #region ZuRaumIdVergabe

        private static void ShowMoreRaumblocksHatches(List<RaumInfo> raumInfos, ref bool moreRaumblocksOccured)
        {
            var invalidRinfos = raumInfos.Where(x => x.TheStatus == RaumInfo.Status.MoreRaumblock);
            foreach (var ris in invalidRinfos)
            {
                moreRaumblocksOccured = true;
                if (ris.Polygon != null)
                {
                    Plan2Ext.Globs.HatchPoly(ris.Polygon, ZU_RAUMBLOCK_MORERB, _AcCm.Color.FromRgb(0, 0, 255));
                }
            }
        }

        private static void ShowNoRaumblockHatches(List<RaumInfo> raumInfos, ref bool noRaumblockOccured)
        {
            var invalidRinfos = raumInfos.Where(x => x.TheStatus == RaumInfo.Status.NoRaumblock);
            foreach (var ris in invalidRinfos)
            {
                noRaumblockOccured = true;
                if (ris.Polygon != null)
                {
                    Plan2Ext.Globs.HatchPoly(ris.Polygon, ZU_RAUMBLOCK_NORB, _AcCm.Color.FromRgb(255, 0, 0));
                }
            }
        }

        private void DeleteErrorSymbols()
        {
            Plan2Ext.Globs.DeleteFehlerLines(ZU_RAUMBLOCK_OUTSIDE);
            Plan2Ext.Globs.DeleteFehlerLines(NO_RAUMNR);
            Plan2Ext.Globs.DeleteFehlerBlocks(ZU_RAUMBLOCK_OUTSIDE);
            Plan2Ext.Globs.DeleteHatches(ZU_RAUMBLOCK_NORB);
            Plan2Ext.Globs.DeleteHatches(ZU_RAUMBLOCK_MORERB);
        }

        private void ShowZuRaumblockOutside()
        {
            List<_AcGe.Point3d> insPoints = new List<_AcGe.Point3d>();
            foreach (var oid in _ZuRaumIdBlocksOrAttribs)
            {
                var ent = _TransMan.GetObject(oid, _AcDb.OpenMode.ForRead);
                var  bref  = ent as _AcDb.BlockReference;
                if (bref != null)
                {
                    //insPoints.Add(new double[] { bref.Position.X, bref.Position.Y, bref.Position.Z });
                    insPoints.Add(bref.Position);
                }
                else
                {
                    var attRef = ent as _AcDb.AttributeReference;
                    if (attRef != null)
                    {
                        insPoints.Add(attRef.Position);
                    }
                }
            }
            if (insPoints.Count == 0) return;

            _AcCm.Color col = _AcCm.Color.FromRgb((byte)255, (byte)10, (byte)40);
            Plan2Ext.Globs.InsertFehlerLines(insPoints, layerName: ZU_RAUMBLOCK_OUTSIDE, length: 50, ang: Math.PI * 1.25, col: col);
        }

        private void AssignZuRaumId(IEnumerable<RaumInfo> okRaumInfos, bool useAttribs = false)
        {
            foreach (var rinfo in okRaumInfos)
            {
                foreach (var zuw in _AutoIdOptions.Zuweisungen)
                {
                    string attVal = GetBlockAttribute(zuw.FromAtt, rinfo.Raumblock);

                    foreach (var oid in rinfo.ZuRaumIdObjectIds)
                    {
                        if (useAttribs)
                        {
                            SetAttribValue(oid, zuw.ToAtt, attVal);
                        }
                        else
                        {
                            SetBlockAttrib(oid, zuw.ToAtt, attVal, tolerant: true);
                        }
                    }
                }

                //string id = GetBlockAttribute(_AutoIdOptions.AttIdNummer, rinfo.Raumblock);

                //foreach (var oid in rinfo.ZuRaumIdBlocks)
                //{
                //    SetBlockAttrib(oid, _AutoIdOptions.ZuRaumIdAtt, id);
                //}
            }

        }


        private class RaumInfo
        {

            protected _AcDb.TransactionManager _TransMan = null;
            public enum Status
            {
                None,
                Ok,
                NoRaumblock,
                MoreRaumblock
            }
            public Status TheStatus { get; set; }
            public _AcDb.ObjectId Polygon { get; set; }
            public _AcDb.ObjectId Raumblock { get; set; }
            public List<_AcDb.ObjectId> ZuRaumIdObjectIds { get; set; }

            public RaumInfo()
            {
                Init();
            }

            public RaumInfo(_AcDb.ObjectId polygon, List<_AcDb.ObjectId> raumblocks, List<_AcDb.ObjectId> zuRaumIdBlocks, _AcDb.TransactionManager tm)
            {
                Init();

                _TransMan = tm;
                Polygon = polygon;

                FindRaumBlock(raumblocks);

                FindZuRaumIdBlocks(zuRaumIdBlocks);

            }

            protected void Init()
            {
                TheStatus = Status.None;
                ZuRaumIdObjectIds = new List<_AcDb.ObjectId>();
            }

            protected void FindRaumBlock(List<_AcDb.ObjectId> raumBlocks)
            {
                using (var myTrans = _TransMan.StartTransaction())
                {
                    _AcDb.Entity poly = _TransMan.GetObject(Polygon, _AcDb.OpenMode.ForRead) as _AcDb.Entity;
                    if (poly != null)
                    {
                        List<_AcDb.ObjectId> toRemove = new List<_AcDb.ObjectId>();
                        foreach (var oid in raumBlocks)
                        {
                            _AcDb.BlockReference block = _TransMan.GetObject(oid, _AcDb.OpenMode.ForRead) as _AcDb.BlockReference;
                            if (block != null)
                            {
                                if (AreaEngine.InPoly(block.Position, poly))
                                {
                                    toRemove.Add(oid);
                                }
                            }
                        }

                        foreach (var oid in toRemove)
                        {
                            raumBlocks.Remove(oid);
                        }

                        if (toRemove.Count > 1)
                        {
                            TheStatus = Status.MoreRaumblock;
                        }
                        else if (toRemove.Count == 0)
                        {
                            TheStatus = Status.NoRaumblock;
                        }
                        else
                        {
                            Raumblock = toRemove[0];
                            TheStatus = Status.Ok;
                        }
                    }

                    myTrans.Commit();
                }
            }

            protected virtual void FindZuRaumIdBlocks(List<_AcDb.ObjectId> zuRaumIdBlocks)
            {
                using (var myTrans = _TransMan.StartTransaction())
                {
                    _AcDb.Entity poly = _TransMan.GetObject(Polygon, _AcDb.OpenMode.ForRead) as _AcDb.Entity;
                    if (poly != null)
                    {
                        List<_AcDb.ObjectId> toRemove = new List<_AcDb.ObjectId>();
                        foreach (var oid in zuRaumIdBlocks)
                        {
                            _AcDb.BlockReference block = _TransMan.GetObject(oid, _AcDb.OpenMode.ForRead) as _AcDb.BlockReference;
                            if (block != null)
                            {
                                if (AreaEngine.InPoly(block.Position, poly))
                                {
                                    ZuRaumIdObjectIds.Add(oid);
                                    toRemove.Add(oid);
                                }
                            }
                        }
                        foreach (var oid in toRemove)
                        {
                            zuRaumIdBlocks.Remove(oid);
                        }
                    }

                    myTrans.Commit();
                }
            }
        }

        private class RaumInfoForAttributes : RaumInfo
        {
            public RaumInfoForAttributes(_AcDb.ObjectId polygon, List<_AcDb.ObjectId> raumblocks, List<_AcDb.ObjectId> zuRaumIdAttributes, _AcDb.TransactionManager tm)
                : base()
            {
                _TransMan = tm;

                Polygon = polygon;

                FindRaumBlock(raumblocks);

                FindZuRaumIdBlocks(zuRaumIdAttributes);

            }

            protected override void FindZuRaumIdBlocks(List<_AcDb.ObjectId> zuRaumIdAttributes)
            {
                using (var myTrans = _TransMan.StartTransaction())
                {
                    _AcDb.Entity poly = _TransMan.GetObject(Polygon, _AcDb.OpenMode.ForRead) as _AcDb.Entity;
                    if (poly != null)
                    {
                        List<_AcDb.ObjectId> toRemove = new List<_AcDb.ObjectId>();
                        foreach (var oid in zuRaumIdAttributes)
                        {
                            _AcDb.AttributeReference block = _TransMan.GetObject(oid, _AcDb.OpenMode.ForRead) as _AcDb.AttributeReference;
                            if (block != null)
                            {
                                if (AreaEngine.InPoly(block.Position, poly))
                                {
                                    ZuRaumIdObjectIds.Add(oid);
                                    toRemove.Add(oid);
                                }
                            }
                        }
                        foreach (var oid in toRemove)
                        {
                            zuRaumIdAttributes.Remove(oid);
                        }
                    }
                    myTrans.Commit();
                }
            }
        }
        #endregion
    }
}
