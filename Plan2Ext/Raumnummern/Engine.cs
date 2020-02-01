//using Autodesk.AutoCAD.ApplicationServices;
//using Autodesk.AutoCAD.DatabaseServices;
//using Autodesk.AutoCAD.EditorInput;
//using Autodesk.AutoCAD.Geometry;
//using Autodesk.AutoCAD.Interop.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
#if BRX_APP
using _AcAp = Bricscad.ApplicationServices;
using _AcCm = Teigha.Colors;
using Teigha.DatabaseServices;
using Bricscad.EditorInput;
using Teigha.Geometry;
using _AcIntCom = BricscadDb;
#elif ARX_APP
using _AcAp = Autodesk.AutoCAD.ApplicationServices;
using _AcCm = Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using _AcIntCom = Autodesk.AutoCAD.Interop.Common;
// ReSharper disable IdentifierTypo
// ReSharper disable CommentTypo
#endif


namespace Plan2Ext.Raumnummern
{
    internal class Engine
    {
        private readonly IEngineParameter _engineParameter;

        #region log4net Initialization
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Convert.ToString((typeof(Engine))));
        #endregion

        #region Const
        private const string _NoRaumblockMessage = "\nDas gewählte Element ist kein Raumblock '{0}'.";
        private const string DIST_RB_TO_FB_KONFIG = "alx_V:ino_rb_rb2fbDist";
        private const string XREC_HATCH_OF_RAUM = "Plan2RoomHatch";
        private const string XREC_TOP_IN_FG = "Plan2TopInFg";

        //internal const string HIDDEN_NUMMER_ATT = "INFO";
        internal const string TOP_LAYER_PREFIX = "A_RA_TOP_";

        //      private readonly Dictionary<string, int> _ColorIndexDict = new Dictionary<string, int>() {
        //	{ "1", 141},
        //	{ "2", 21},
        //	{ "3", 72},
        //	{ "4", 50},
        //	{ "5", 24},
        //	{ "6", 111},
        //	{ "7", 11},
        //	{ "8", 53},
        //	{ "9", 101},
        //	{ "0", 31},
        //	{ "J", 141},
        //	{ "I", 21},
        //	{ "H", 72},
        //	{ "G", 50},
        //	{ "F", 24},
        //	{ "E", 111},
        //	{ "D", 11},
        //	{ "C", 53},
        //	{ "B", 101},
        //	{ "A", 31},
        //};



        #endregion

        #region Members
        private TransactionManager _TransMan;
        private HatchColorServer _hatchColorServer;
        private ObjectId _CurrentBlock = ObjectId.Null;
        private ObjectId _LastBlock = ObjectId.Null;
        private Editor _Editor;
        private RnOptions _RnOptions;
        private Dictionary<string, List<ObjectId>> _OidsPerTop = new Dictionary<string, List<ObjectId>>();
        private List<ObjectId> _AllRaumBlocks = new List<ObjectId>();
        public List<ObjectId> AllRaumBlocks
        {
            get { return _AllRaumBlocks; }
            set { _AllRaumBlocks = value; }
        }
        private double _MaxDist = 0.25;
        Dictionary<ObjectId, Plan2Ext.AreaEngine.FgRbStructure> _FgRbStructs = new Dictionary<ObjectId, AreaEngine.FgRbStructure>();

        public string Blockname { get; set; }
        public string NrAttribname { get; set; }
        public string HBlockname { get; set; }
        public string TopBlockName { get; set; }
        public string TopBlockTopNrAttName { get; set; }

        #endregion

        #region Lifecycle

        public Engine(IEngineParameter engineParameter)
        {
            _engineParameter = engineParameter;
            _RnOptions = null;

            _hatchColorServer = new HatchColorServer();
            _RnOptions = engineParameter.Options;
            Blockname = _RnOptions.Blockname;
            NrAttribname = _RnOptions.Attribname;
            HBlockname = _RnOptions.HBlockname;
            TopBlockName = Commands.TOPBLOCKNAME;
            TopBlockTopNrAttName = Commands.TOPBLOCK_TOPNR_ATTNAME;

            var sMaxDist = TheConfiguration.GetValueString(DIST_RB_TO_FB_KONFIG);
            _MaxDist = double.Parse(sMaxDist, CultureInfo.InvariantCulture);

            Database db = _AcAp.Application.DocumentManager.MdiActiveDocument.Database;
            _TransMan = db.TransactionManager;
            _Editor = _AcAp.Application.DocumentManager.MdiActiveDocument.Editor;

            _FgRbStructs = engineParameter.FgRbs;
            _AllRaumBlocks = engineParameter.AllRaumBlocks;
        }

        private void MarkRbs()
        {
            using (Transaction transaction = _TransMan.StartTransaction())
            {
                foreach (var oid in _AllRaumBlocks)
                {
                    BlockReference ent = transaction.GetObject(oid, OpenMode.ForRead) as BlockReference;
                    MarkRbIfNumber(ent, transaction);
                }
                transaction.Commit();
            }
        }

        private void MarkRbIfNumber(BlockReference ent, Transaction transaction)
        {
            if (ent == null) return;
            var attRef = GetBlockAttribute(NrAttribname, ent, transaction);
            if (attRef == null) return;
            if (string.IsNullOrEmpty(attRef.TextString))
            {
                ent.Unhighlight();
            }
            else
            {
                ent.Highlight();
            }
        }

        #endregion

        #region Internal
        internal void MoveFbh(double xDist, double yDist, bool ignoreIfNrExists)
        {
            List<ObjectId> allHkBlocks = SelectAllHkBlocks();

            if (allHkBlocks.Count == 0 || _AllRaumBlocks.Count == 0) return;

            using (Transaction myT = _TransMan.StartTransaction())
            {
                List<RbInfo> RbInsPoints = GetRbInfos();

                foreach (var oid in allHkBlocks)
                {
                    BlockReference blockRef = _TransMan.GetObject(oid, OpenMode.ForRead) as BlockReference;
                    if (blockRef == null) continue;

                    RbInfo par = GetNearest(blockRef.Position, RbInsPoints);
                    if (par == null) continue;

                    if (ignoreIfNrExists && NrAttWithValueExists(par))
                    {
                        continue;
                    }

                    Vector2d v = new Vector2d(xDist * par.Scale, yDist * par.Scale);
                    v = v.RotateBy(par.Rot);
                    Vector3d v3 = new Vector3d(v.X, v.Y, 0.0);
                    Point3d newP = par.Pos.Add(v3);

                    Vector3d vAtt = blockRef.Position.GetVectorTo(newP);
                    blockRef.UpgradeOpen();
                    blockRef.Position = newP;
                    foreach (ObjectId attId in blockRef.AttributeCollection)
                    {
                        if (attId.IsErased) continue;
                        var anyAttRef = _TransMan.GetObject(attId, OpenMode.ForWrite) as AttributeReference;
                        if (anyAttRef != null)
                        {
                            newP = anyAttRef.Position.Add(vAtt);
                            anyAttRef.Position = newP;

                        }
                    }
                }

                myT.Commit();
            }
        }

        private bool NrAttWithValueExists(RbInfo par)
        {
            bool exists = false;
            _AcAp.Document doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            using (var trans = db.TransactionManager.StartTransaction())
            {
                var blockRef = (BlockReference)trans.GetObject(par.Oid, OpenMode.ForRead);
                foreach (ObjectId attId in blockRef.AttributeCollection)
                {
                    if (attId.IsErased) continue;
                    var anyAttRef = trans.GetObject(attId, OpenMode.ForRead) as AttributeReference;
                    if (anyAttRef != null && string.Compare(anyAttRef.Tag, _RnOptions.Attribname, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        if (!string.IsNullOrEmpty(anyAttRef.TextString.Trim())) exists = true;
                        break;
                    }
                }
                trans.Commit();
            }
            return exists;
        }

        internal void DeleteNummerAttribute(List<ObjectId> blockOids, Transaction transaction)
        {
            if (blockOids == null || blockOids.Count == 0) return;
            using (Transaction myT = _TransMan.StartTransaction())
            {
                foreach (var oid in blockOids)
                {
                    var blockRef = (BlockReference)myT.GetObject(oid, OpenMode.ForRead);
                    var attRef = GetBlockAttribute(NrAttribname, blockRef, transaction);
                    if (attRef != null)
                    {
                        attRef.UpgradeOpen();
                        attRef.TextString = "";
                    }
                }
                myT.Commit();
            }
        }

        internal void CopyAttribute(List<ObjectId> blockOids, string fromAttname, string toAttname, Transaction transaction)
        {
            if (blockOids == null || blockOids.Count == 0) return;
            using (Transaction myT = _TransMan.StartTransaction())
            {
                foreach (var oid in blockOids)
                {
                    var blockRef = (BlockReference)myT.GetObject(oid, OpenMode.ForRead);
                    var attRef = GetBlockAttribute(fromAttname, blockRef, transaction);
                    if (attRef != null)
                    {
                        var nr = attRef.TextString;
                        var attRefInfo = GetBlockAttribute(toAttname, blockRef, transaction);
                        if (attRefInfo != null)
                        {
                            attRefInfo.UpgradeOpen();
                            attRefInfo.TextString = nr;
                        }
                    }
                }
                myT.Commit();
            }
        }

        //internal void MoveNrToInfoAttribute(List<ObjectId> blockOids)
        //{
        //    if (blockOids == null || blockOids.Count == 0) return;
        //    using (Transaction myT = _TransMan.StartTransaction())
        //    {
        //        foreach (var oid in blockOids)
        //        {
        //            var blockRef = (BlockReference)myT.GetObject(oid, OpenMode.ForRead);
        //            var attRef = GetBlockAttribute(NrAttribname, blockRef);
        //            if (attRef != null)
        //            {
        //                var nr = attRef.TextString;

        //                var attRefInfo = GetBlockAttribute(HIDDEN_NUMMER_ATT, blockRef);
        //                if (attRefInfo != null)
        //                {
        //                    attRefInfo.UpgradeOpen();
        //                    attRefInfo.TextString = nr;
        //                    attRef.UpgradeOpen();
        //                    attRef.TextString = "";
        //                }
        //            }
        //        }
        //        myT.Commit();
        //    }
        //}

        //internal void CopyNrToInfoAttribute(List<ObjectId> blockOids)
        //{
        //    if (blockOids == null || blockOids.Count == 0) return;
        //    using (Transaction myT = _TransMan.StartTransaction())
        //    {
        //        foreach (var oid in blockOids)
        //        {
        //            var blockRef = (BlockReference)myT.GetObject(oid, OpenMode.ForRead);
        //            var attRef = GetBlockAttribute(NrAttribname, blockRef);
        //            if (attRef != null)
        //            {
        //                var nr = attRef.TextString;

        //                var attRefInfo = GetBlockAttribute(HIDDEN_NUMMER_ATT, blockRef);
        //                if (attRefInfo != null)
        //                {
        //                    attRefInfo.UpgradeOpen();
        //                    attRefInfo.TextString = nr;
        //                }
        //            }
        //        }
        //        myT.Commit();
        //    }
        //}

        internal bool AddNumber(List<ObjectId> blockOids)
        {
            // DeleteAllFehlerLines();
            // MarkRbs();

            try
            {
                UndoHandler.StartUndoMark();
                using (Transaction transaction = _TransMan.StartTransaction())
                {
                    //if (!SelectBlock()) return false; // old variante

                    AreaEngine.FgRbStructure fgrb = null;
                    if (!SelectBlockViaPoint(transaction, out fgrb)) return false;
                    if (fgrb == null) return true;
                    if (_CurrentBlock == ObjectId.Null) return true;

                    var otherFgRbs = GetFgRbInSameTopWithExistingHatch(fgrb, transaction);

                    var oldCompleteNr = GetBlockAttribute(_RnOptions.Attribname, _CurrentBlock, transaction);

                    HatchItAndSetTopToFg(fgrb, transaction);

                    if (_RnOptions.AutoCorr)
                    {
                        CalcBlocksPerTop(transaction);
                        AutoIncrementHigherNumbers(_RnOptions.Number, transaction);
                    }

                    // verhindern: Inkrement bei zweitem Mal Klick in gleiche Fläche 
                    if (!CheckIsSecondClickInSameRoom())
                    {
                        var completeNum = GetCompleteNumber(_RnOptions.Number);
                        SetBlockAttrib(_CurrentBlock, _RnOptions.Attribname, completeNum, transaction);

                        //BlockReference br = transaction.GetObject(_CurrentBlock, OpenMode.ForRead) as BlockReference;
                        //MarkRbIfNumber(br, transaction);

                        if (_RnOptions.AutoCorr)
                        {

                            CalcBlocksPerTop(transaction);
                            string t, g, n;
                            if (GetTopGeschRaumnr(completeNum, out t, out g, out n)) ;
                            AutoCorrection(t, g, transaction);

                            // correction old top
                            if (!string.IsNullOrEmpty(oldCompleteNr))
                            {
                                if (GetTopGeschRaumnr(oldCompleteNr, out t, out g, out n)) ;
                                AutoCorrection(t, g, transaction);
                            }
                        }

                        string newNr = Increment(_RnOptions.Number);

                        // nummer in formular maximal auf derzeitige anzahl der blöcke
                        if (_RnOptions.AutoCorr)
                        {
                            string geschoss, nr;
                            Globs.GetGeschossAndNr(newNr, out geschoss, out nr);
                            var i = int.Parse(nr);
                            List<ObjectId> oids;
                            if (_OidsPerTop.TryGetValue(_RnOptions.Top, out oids))
                            {
                                var oidsInfos = GetOidsinfos(transaction, oids, geschoss);
                                if (i > (oidsInfos.Count + 1)) i = oidsInfos.Count + 1;
                                newNr = geschoss + i.ToString().PadLeft(2, '0');
                            }
                        }

                        _RnOptions.SetNumber(newNr);

                        if (!blockOids.Contains(_CurrentBlock)) blockOids.Add(_CurrentBlock);
                    }

                    HatchMany(otherFgRbs, transaction);

                    transaction.Commit();
                }
            }
            finally
            {
                UndoHandler.EndUndoMark();
            }


            return true;
        }

        internal void BereinigFehlerlinien()
        {
            foreach (var fi in _FehlerInfos)
            {
                var layer = fi.Value.Layername;
                Plan2Ext.Globs.DeleteFehlerLines(layer);
            }
        }

        private class TopStructure
        {
            public string TopNummer { get; set; }
            public ObjectId TopOid { get; set; }
            private List<Plan2Ext.AreaEngine.FgRbStructure> _FgRbs = new List<AreaEngine.FgRbStructure>();
            public List<Plan2Ext.AreaEngine.FgRbStructure> FgRbs
            {
                get { return _FgRbs; }
                set { _FgRbs = value; }
            }
        }

        internal bool RenameTop(string topNrNeu)
        {
            DeleteAllFehlerLines();
            using (Transaction transaction = _TransMan.StartTransaction())
            {
                AreaEngine.FgRbStructure theFgRb = null;
                if (!SelectBlockViaPoint(transaction, out theFgRb)) return false;
                if (theFgRb == null)
                {
                    _Editor.WriteMessage("\nEs wurde kein Raum gewählt.");
                    return true;
                }

                var topNr = GetTopNr(theFgRb, transaction);
                if (string.IsNullOrEmpty(topNr))
                {
                    _Editor.WriteMessage("\nDer Raum hat keine Topnummer!");
                    transaction.Commit();
                    return true;
                }

                if (topNr == topNrNeu)
                {
                    MessageBox.Show(string.Format(CultureInfo.CurrentCulture, "\nTop hat bereits die Topnummer '{0}'.", topNr),
                        "Top umbenennen", MessageBoxButtons.OK);
                    transaction.Commit();
                    return true;
                }

                var res = MessageBox.Show(
                    string.Format(CultureInfo.CurrentCulture, "Soll Top {0} umbenannt werden in Top {1}?", topNr,
                        topNrNeu),
                    "Top umbenennen", MessageBoxButtons.YesNoCancel);
                if (res != DialogResult.Yes)
                {
                    transaction.Commit();
                    return false;
                }

                var fgRbs = GetFgRbInSameTopWithExistingHatch(theFgRb, transaction);
                fgRbs.Add(theFgRb);

                var topBlockOid = FindTopBlockWithTopNr(transaction, topNr);
                if (topBlockOid == default(ObjectId))
                {
                    InsertFehlerLineAt(theFgRb.FlaechenGrenze, _NO_TOPBLOCK_FOR_ROOM);
                }
                else
                {

                    var pfeilBlockOid = FindPfeilBlockWithTopNr(transaction, topNr);
                    var topStruct = new TopStructure() { TopOid = topBlockOid, TopNummer = topNrNeu, FgRbs = fgRbs };

                    foreach (var fgrb in fgRbs)
                    {
                        SetTopForFg(topNrNeu, fgrb, transaction);

                        if (fgrb.Raumbloecke.Count != 1)
                        {
                            InsertFehlerLineAt(fgrb.FlaechenGrenze, _INVALID_NR_OF_RBS_IN_ROOM);
                            continue;
                        }
                        var rbOid = fgrb.Raumbloecke[0];
                        string rbTopNr, rbRaumNr;
                        if (!GetPartsFromRb(rbOid, transaction, out rbTopNr, out rbRaumNr))
                        {
                            InsertFehlerLineAt(rbOid, _INVALID_TOP_NR);
                        }
                        else
                        {

                            var completeNr = topNrNeu + _RnOptions.Separator + rbRaumNr;
                            //SetBlockAttrib(rbOid, HIDDEN_NUMMER_ATT, completeNr);
                            SetBlockAttrib(rbOid, _RnOptions.Attribname, completeNr, transaction);
                            //if (_RnOptions.UseHiddenAttribute)
                            //{
                            //    SetBlockAttrib(rbOid, NrAttribname, "");
                            //}
                            //else
                            //{
                            //    SetBlockAttrib(rbOid, NrAttribname, completeNr);
                            //}
                        }
                    }

                    // topblock
                    SetBlockAttrib(topBlockOid, TopBlockTopNrAttName, topNrNeu, transaction);
                    // pfeilblock
                    if (pfeilBlockOid != default(ObjectId))
                    {
                        SetBlockAttrib(pfeilBlockOid, Commands.PFEILBLOCK_TOPNR_ATTNAME, topNrNeu, transaction);
                    }

                    // schraffur
                    HatchIt(topStruct, transaction, true);
                }
                transaction.Commit();
            }
            return true;
        }


        private ObjectId FindPfeilBlockWithTopNr(Transaction myT, string tnr)
        {
            var pfeilBlock = SelectAllPfeilBlocks();
            return pfeilBlock.FirstOrDefault((oid) =>
            {
                string pbTopNr;
                if (GetTopNrFromPfeilBlock(oid, myT, out pbTopNr))
                {
                    if (pbTopNr == tnr) return true;
                }
                return false;
            }
                );
        }

        private ObjectId FindTopBlockWithTopNr(Transaction myT, string tnr)
        {
            var topBlocks = SelectAllTopBlocks();
            return topBlocks.FirstOrDefault((oid) =>
            {
                string tbTopNr;
                if (GetTopNrFromTopBlock(oid, myT, out tbTopNr))
                {
                    if (tbTopNr == tnr) return true;
                }
                return false;
            }
                );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Es werden nur Raumblöcke verwendet, die innerhalb einer FgRb-Struktur vorkommen, damit nicht irgendwo einsam außerhalb liegende Raumblöcke verwendet werden.
        /// </remarks>
        internal bool SumTops()
        {
            FehlerLinesOnOff(true);
            DeleteAllFehlerLines();

            var fgOids = new List<ObjectId>();
            var rbOids = new List<ObjectId>();
            var topOids = new List<ObjectId>();

            SelectFgsandRbsAndTops(fgOids, rbOids, topOids);
            if (fgOids.Count == 0 || rbOids.Count == 0) return true;
            var fgRbStructValues = _FgRbStructs.Values.Where(x => fgOids.Contains(x.FlaechenGrenze)).ToArray();

			// flaeche rechnen
            Plan2Ext.Flaeche.Modify = true;
            Plan2Ext.Flaeche.AktFlaeche(
	            _AcAp.Application.DocumentManager.MdiActiveDocument,
	            _RnOptions.Blockname,
	            _RnOptions.FlaechenAttributName,
	            _RnOptions.UmfangAttributName,
	            _RnOptions.FlaechenGrenzeLayerName,
	            _RnOptions.AbzFlaechenGrenzeLayerName,
	            fgRbStructValues
            );
            Plan2Ext.Flaeche.BereinigRegions(automated: false);

			using (Transaction transaction = _TransMan.StartTransaction())
            {
                var allTopNrs = new HashSet<string>();

                var fgrbPerTopNr = new Dictionary<string, List<AreaEngine.FgRbStructure>>();
                foreach (var fgrb in fgRbStructValues)
                {
                    var rb = Plan2Ext.Globs.GetXrecord(fgrb.FlaechenGrenze, XREC_TOP_IN_FG);
                    if (rb != null)
                    {
                        TypedValue[] values = rb.AsArray();
                        try
                        {
                            var topNr = values[0].Value.ToString().Trim();
                            if (string.IsNullOrEmpty(topNr)) continue;

                            if (fgrb.Raumbloecke.Count != 1)
                            {
                                InsertFehlerLineAt(fgrb.FlaechenGrenze, _INVALID_NR_OF_RBS_IN_ROOM);
                                continue;
                            }
                            var rbOid = fgrb.Raumbloecke[0];
                            string topName = GetTopNameFromRb(rbOid, transaction).Trim();
                            if (string.Compare(topName, topNr, StringComparison.CurrentCulture) != 0)
                            {
                                InsertFehlerLineAt(rbOid, _ROOM_HAS_WRONG_TOPNR);
                                continue;
                            }

                            allTopNrs.Add(topNr);
                            List<AreaEngine.FgRbStructure> fgRbList;
                            if (!fgrbPerTopNr.TryGetValue(topNr, out fgRbList))
                            {
                                fgRbList = new List<AreaEngine.FgRbStructure>();
                                fgrbPerTopNr.Add(topNr, fgRbList);
                            }
                            fgRbList.Add(fgrb);
                        }
                        catch (Exception)
                        {
                        }
                    }
                }

                var topOidPerTopNr = new Dictionary<string, ObjectId>();
                var topsWithSameNumber = new HashSet<ObjectId>();
                foreach (var topOid in topOids)
                {
                    // get topnr
                    string topNr;
                    if (!GetCompleteTopNrFromTopBlock(topOid, transaction, out topNr)) continue;
                    topNr = topNr.Trim();
                    if (string.IsNullOrEmpty(topNr)) continue;
                    if (!allTopNrs.Contains(topNr)) continue;

                    ObjectId oid;
                    if (topOidPerTopNr.TryGetValue(topNr, out oid))
                    {
                        topsWithSameNumber.Add(oid);
                        topsWithSameNumber.Add(topOid);
                        continue;
                    }
                    topOidPerTopNr.Add(topNr, topOid);
                }

                foreach (var oid in topsWithSameNumber)
                {
                    InsertFehlerLineAt(oid, _TOPS_HAVE_SAME_NR);
                }

                var topStructs = new List<TopStructure>();
                foreach (var topNr in allTopNrs)
                {
                    var topStruct = new TopStructure { TopNummer = topNr };
                    List<AreaEngine.FgRbStructure> fgrbs;
                    topStruct.FgRbs = fgrbPerTopNr.TryGetValue(topNr, out fgrbs) ? fgrbs : null;
                    ObjectId topOid;
                    topStruct.TopOid = topOidPerTopNr.TryGetValue(topNr, out topOid) ? topOid : ObjectId.Null;
                    topStructs.Add(topStruct);
                }


                // summieren
                foreach (var topStruct in topStructs)
                {
                    SumM2(topStruct, transaction);
                }
                // schraffieren
                var ids = new ObjectIdCollection();
                foreach (var topStruct in topStructs)
                {
                    var oid = HatchIt(topStruct, transaction, false);
                    ids.Add(oid);
                }
                Plan2Ext.Globs.DrawOrderBottom(ids);

                transaction.Commit();
            }

            CheckRblockConsistency(rbOids, fgRbStructValues);

            return true;
        }
        /// <summary>
        /// Auswahl von Flächengrenzen. Nur dieser werden summiert und schraffiert. Die Summe der Fläche der Raumblöcke wird zurückgegeben.
        /// </summary>
        /// <returns></returns>
        internal bool SumFgs(ref double m2)
        {
            FehlerLinesOnOff(true);
            DeleteAllFehlerLines();

            var fgOids = SelectFgs();
            if (fgOids.Count == 0) return false;
            var fgrbs = _FgRbStructs.Values.Where(x => fgOids.Contains(x.FlaechenGrenze)).ToList();

            var topNr = RandomTopNr.NextTopNr; // TOP_PREFIX + "01";
            using (Transaction transaction = _TransMan.StartTransaction())
            {

                var fgRbToRemove = new List<AreaEngine.FgRbStructure>();
                foreach (var fgrb in fgrbs)
                {
                    if (fgrb.Raumbloecke.Count != 1)
                    {
                        InsertFehlerLineAt(fgrb.FlaechenGrenze, _INVALID_NR_OF_RBS_IN_ROOM);
                        fgRbToRemove.Add(fgrb);
                        continue;
                    }
                }
                foreach (var fgRb in fgRbToRemove)
                {
                    fgrbs.Remove(fgRb);
                }

                var topStruct = new TopStructure() { TopNummer = topNr };
                topStruct.FgRbs = fgrbs;

                HatchIt(topStruct, transaction,true);
                m2 = GetSumM2FromBlocks(topStruct, transaction);

                transaction.Commit();
            }

            return true;
        }

        private static class RandomTopNr
        {
            private static char _CurrentTopNr = 'z';
            public static string NextTopNr
            {
                get
                {
                    if (_CurrentTopNr == 'z') _CurrentTopNr = 'a';
                    else _CurrentTopNr++;
                    return _CurrentTopNr.ToString();
                }
            }
        }

        internal void RemoveAllInfos()
        {
            var elems = SelectAllInfoElements();
            using (Transaction transaction = _TransMan.StartTransaction())
            {
                foreach (var oid in elems)
                {
                    var dbo = transaction.GetObject(oid, OpenMode.ForRead); // has to be for read  -> later upgradopen
                    var blockReference = dbo as BlockReference;
                    if (blockReference != null)
                    {
                        if (blockReference.Name == Blockname)
                        {
                            // raumblock
                            SetBlockAttrib(blockReference, _RnOptions.Attribname, "", transaction);
                            //SetBlockAttrib(blockReference, HIDDEN_NUMMER_ATT, "");
                        }
                        else if (blockReference.Name == TopBlockName)
                        {
                            // topblock
                            // SetBlockAttrib(blockReference , TopBlockTopNrAttName, "TOP");
                            SetBlockAttrib(blockReference, Commands.TOPBLOCK_M2_ATTNAME, "00", transaction);
                        }
                        else
                        {
                            log.WarnFormat(CultureInfo.InvariantCulture, "Invalid block in selection! '{0}'", blockReference.Name);
                        }
                    }
                    else
                    {
                        Polyline poly = dbo as Polyline;
                        if (poly != null)
                        {
                            // flächengrenze
                            Plan2Ext.Globs.DelXrecord(transaction, poly, XREC_TOP_IN_FG);
                            Plan2Ext.Globs.DelXrecord(transaction, poly, XREC_HATCH_OF_RAUM);
                        }
                        else
                        {
                            Hatch hatch = dbo as Hatch;
                            if (hatch != null)
                            {
                                // schraffur
                                hatch.UpgradeOpen();
                                if (!hatch.IsErased)
                                {
                                    hatch.Erase();
                                }
                                hatch.DowngradeOpen();
                            }
                            else
                            {
                                log.WarnFormat(CultureInfo.InvariantCulture, "Invalid element in selection! '{0}'", dbo.GetType().Name);
                            }
                        }
                    }
                }

                transaction.Commit();
            }


        }

        private void CheckRblockConsistency(List<ObjectId> oids, AreaEngine.FgRbStructure[] fgRbStructures)
        {
            var rbsInTop = fgRbStructures.SelectMany(x => x.Raumbloecke).ToList(); // _FgRbStructs.Values.SelectMany(x => x.Raumbloecke).ToList();
            var rbsOutOfTop = _AllRaumBlocks.Where(oids.Contains).Where(x => !rbsInTop.Contains(x)).ToList();

            using (var transaction = _TransMan.StartTransaction())
            {
                foreach (var fgrb in fgRbStructures)
                {
                    CheckRblockConsistency(fgrb, transaction);
                }

                CheckRblockConsistency("", rbsOutOfTop, transaction);

                transaction.Commit();
            }
        }

        private void CheckRblockConsistency(AreaEngine.FgRbStructure fgrb, Transaction transaction)
        {
            var topNr = GetTopNr(fgrb, transaction);
            if (topNr == null) return;
            var rbOids = fgrb.Raumbloecke;
            CheckRblockConsistency(topNr, rbOids, transaction);
        }

        private void CheckRblockConsistency(string topNr, List<ObjectId> rbOids, Transaction transaction)
        {
            foreach (var oid in rbOids)
            {
                var attVal = GetBlockAttribute(NrAttribname, oid, transaction);
                if (!string.IsNullOrEmpty(attVal) && !attVal.StartsWith(topNr))
                {
                    InsertFehlerLineAt(oid, _ROOM_HAS_WRONG_TOPNR);
                }
            }
        }

        //internal bool SumTopsOld()
        //{
        //    FehlerLinesOnOff(true);
        //    DeleteAllFehlerLines();
        //    CalcBlocksPerTop(); // berechnet _OidsPerTop
        //    var rbsPerTopNr = _OidsPerTop; // keine raumblöcke ohne topname
        //    var topBlocks = SelectAllTopBlocks();
        //    var fgrbsPerRb = new Dictionary<ObjectId, Plan2Ext.AreaEngine.FgRbStructure>();
        //    // FbRb struct enthält die von AreaEngine ermittelte Struktur der Zusammenhänge zwischen Raumblöcken, Flächengrenzen und Abzugsflächen
        //    foreach (var fgrb in _FgRbStructs.Values)
        //    {
        //        if (fgrb.Raumbloecke.Count == 1)
        //        {
        //            Plan2Ext.AreaEngine.FgRbStructure testFgRb = null;
        //            var rbOid = fgrb.Raumbloecke[0];
        //            if (fgrbsPerRb.TryGetValue(rbOid, out testFgRb))
        //            {
        //                InsertFehlerLineAt(rbOid, _RB_IN_MULTIPLE_ROOMS);
        //            }
        //            else
        //            {
        //                fgrbsPerRb.Add(rbOid, fgrb);
        //            }
        //        }
        //        else
        //        {
        //            InsertFehlerLineAt(fgrb.FlaechenGrenze, _INVALID_NR_OF_RBS_IN_ROOM);
        //        }
        //    }

        //    var topStructs = new List<TopStructure>();
        //    var fgRbUsed = new List<Plan2Ext.AreaEngine.FgRbStructure>();
        //    using (Transaction transaction = _TransMan.StartTransaction())
        //    {
        //        foreach (var topOid in topBlocks)
        //        {
        //            // get topnr
        //            string topNr;
        //            if (!GetCompleteTopNrFromTopBlock(topOid, transaction, out topNr)) continue;
        //            topNr = topNr.Replace(" ", ""); // topnr in raumblock hat keine leerzeichen

        //            List<ObjectId> rbs = null;
        //            if (!rbsPerTopNr.TryGetValue(topNr, out rbs))
        //            {
        //                InsertFehlerLineAt(topOid, _TOP_HAS_NO_RBS);
        //                continue;
        //            }

        //            var topStruct = new TopStructure() { TopOid = topOid, TopNummer = topNr };
        //            foreach (var rbOid in rbs)
        //            {
        //                Plan2Ext.AreaEngine.FgRbStructure fgrb = null;
        //                if (!fgrbsPerRb.TryGetValue(rbOid, out fgrb))
        //                {
        //                    InsertFehlerLineAt(rbOid, _RB_DOES_NOT_BELONG_TO_A_TOP);
        //                    continue;
        //                }
        //                topStruct.FgRbs.Add(fgrb);
        //                fgRbUsed.Add(fgrb);
        //            }

        //            topStructs.Add(topStruct);
        //        }

        //        // mail gh 6.8.2017
        //        //beim summieren  werden alle nicht zugeordneten raumpolylinien mit der fehlermeldung :  _raum gehört nicht zum top  markiert.
        //        //Bitte fehlermeldung derzeit  nicht berücksichtigen da es eiegentlich immer vorkommt dass allgemeinräume  nicht  zugeordnet sind
        //        //foreach (var fgrbStruct in _FgRbStructs.Values)
        //        //{
        //        //    if (!fgRbUsed.Contains(fgrbStruct))
        //        //    {
        //        //        InsertFehlerLineAt(fgrbStruct.FlaechenGrenze, _ROOM_DOESNT_BELONG_TO_A_TOP);
        //        //    }
        //        //}

        //        // summieren
        //        foreach (var topStruct in topStructs)
        //        {
        //            SumM2(topStruct, transaction);
        //            HatchIt(topStruct, transaction);
        //        }

        //        transaction.Commit();
        //    }
        //    return true;
        //}

        internal bool RemoveRaum()
        {
            // MarkRbs();
            using (Transaction transaction = _TransMan.StartTransaction())
            {
                AreaEngine.FgRbStructure fgrb = null;
                if (!SelectBlockViaPoint(transaction, out fgrb)) return false;
                if (fgrb == null) return true;
                //if (_CurrentBlock == ObjectId.Null) return true;

                var otherFgRbs = GetFgRbInSameTopWithExistingHatch(fgrb, transaction);
                DeleteOldHatch(fgrb.FlaechenGrenze);

                ClearFg(fgrb, transaction);

                string oldCompleteNr = null;

                if (_CurrentBlock != ObjectId.Null)
                {
                    oldCompleteNr = GetBlockAttribute(NrAttribname, _CurrentBlock, transaction);
                    SetBlockAttrib(_CurrentBlock, NrAttribname, "", transaction);

                    //SetBlockAttrib(_CurrentBlock, HIDDEN_NUMMER_ATT, "");

                    //BlockReference br = _TransMan.GetObject(_CurrentBlock, OpenMode.ForRead) as BlockReference;
                    //MarkRbIfNumber(br, transaction);
                }

                if (_RnOptions.AutoCorr)
                {
                    CalcBlocksPerTop(transaction);
                    try
                    {
                        // correction old top
                        if (!string.IsNullOrEmpty(oldCompleteNr))
                        {
                            string t, g, n;
                            if (GetTopGeschRaumnr(oldCompleteNr, out t, out g, out n)) ;
                            AutoCorrection(t, g, transaction);
                        }
                    }
                    catch (Exception)
                    {
                    }
                }

                HatchMany(otherFgRbs, transaction);

                transaction.Commit();
            }
            return true;
        }

        #endregion
        private void ClearFg(AreaEngine.FgRbStructure fgrb, Transaction transaction)
        {
            _engineParameter.FgRbPerTopNr.RemoveFgRb(fgrb, transaction);
            Plan2Ext.Globs.DelXrecord(transaction, fgrb.FlaechenGrenze, XREC_TOP_IN_FG);
            Plan2Ext.Globs.DelXrecord(transaction, fgrb.FlaechenGrenze, XREC_HATCH_OF_RAUM);
        }
        private void SetTopForFg(string topNr, AreaEngine.FgRbStructure fgrb, Transaction transaction)
        {
            _engineParameter.FgRbPerTopNr.AddFgRb(fgrb, topNr, transaction);
            Plan2Ext.Globs.SetXrecord(fgrb.FlaechenGrenze, XREC_TOP_IN_FG,
                new ResultBuffer(new TypedValue((int)DxfCode.Text, topNr)), transaction);
        }

        #region FG to Top



        #endregion

        #region Move Fußbodenhöhenblock
        private RbInfo GetNearest(Point3d point3d, List<RbInfo> RbInsPoints)
        {
            var sorted = RbInsPoints.OrderBy(x => x.Pos.Distance2dTo(point3d)).ToList();
            RbInfo ret = sorted[0];

            double dist = point3d.Distance2dTo(ret.Pos);
            if (dist > _MaxDist) return null;

            return ret;
        }

        private class RbInfo
        {
            public Point3d Pos { get; set; }
            public double Rot { get; set; }
            public double Scale { get; set; }
            public ObjectId Oid { get; set; }
        }


        private List<RbInfo> GetRbInfos()
        {
            List<RbInfo> ret = new List<RbInfo>();
            foreach (var oid in _AllRaumBlocks)
            {
                BlockReference blockRef = _TransMan.GetObject(oid, OpenMode.ForRead) as BlockReference;
                if (blockRef == null) continue;

                ret.Add(new RbInfo()
                {
                    Pos = new Point3d(blockRef.Position.X, blockRef.Position.Y, blockRef.Position.Z),
                    Rot = blockRef.Rotation,
                    Scale = blockRef.ScaleFactors.X,
                    Oid = oid
                });
            }
            return ret;
        }

        #endregion

        #region Topname Handling

        private void CalcBlocksPerTop(Transaction transaction)
        {
            // dzt nur mit Separator
            if (string.IsNullOrEmpty(_RnOptions.Separator)) return;

            _OidsPerTop.Clear();

            var rblocks = _AllRaumBlocks;

            foreach (var oid in rblocks)
            {
                string topName = GetTopNameFromRb(oid, transaction);
                if (string.IsNullOrEmpty(topName)) continue;

                //topNrIncPrefix.Remove(0, TOP_PREFIX.Length).Trim();

                List<ObjectId> oids;
                if (!_OidsPerTop.TryGetValue(topName, out oids))
                {
                    oids = new List<ObjectId>();
                    _OidsPerTop.Add(topName, oids);
                }
                oids.Add(oid);
            }
        }

        private bool GetTopGeschRaumnr(ObjectId oid, Transaction transaction, out string topnr, out string geschoss, out string nr)
        {
            string attVal = GetBlockAttribute(_RnOptions.Attribname, oid, transaction);
            return GetTopGeschRaumnr(attVal, out topnr, out geschoss, out nr);
        }

        private bool GetTopGeschRaumnr(string attVal, out string topnr, out string geschoss, out string nr)
        {
            topnr = GetTopName(attVal);
            geschoss = "";
            nr = "";
            var raumnr = GetRaumnr(attVal);
            try
            {
                Globs.GetGeschossAndNr(raumnr, out geschoss, out nr);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        private string GetRaumnr(ObjectId oid, Transaction transaction)
        {
            string attVal = GetBlockAttribute(_RnOptions.Attribname, oid, transaction);
            return GetRaumnr(attVal);
        }

        internal string GetRaumnr(string attVal)
        {
            if (string.IsNullOrEmpty(_RnOptions.Separator))
            {
                return FromNumericChar(attVal);
            }
            else
            {
                return FromSeparator(attVal);
            }
        }

        private string FromSeparator(string attVal)
        {
            var arr = attVal.Split(new string[] { _RnOptions.Separator }, StringSplitOptions.None);
            if (arr.Length < 2) return string.Empty;
            return (arr[arr.Length - 1]);
        }

        private string FromNumericChar(string attVal)
        {
            var charr = attVal.ToCharArray();
            int i = charr.Length - 1;
            for (; i >= 0; i--)
            {
                if (!IsNumeric(charr[i])) break;
            }
            if (i < 0) return attVal;
            return attVal.Remove(0, i + 1);
        }

        private bool GetPartsFromRb(ObjectId oid, Transaction transaction, out string topNr, out string raumNr)
        {
            topNr = "";
            raumNr = "";
            //var nummer = GetBlockAttribute(HIDDEN_NUMMER_ATT, oid);
            var nummer = GetBlockAttribute(_RnOptions.Attribname, oid, transaction);
            if (string.IsNullOrEmpty(nummer)) return false;

            int index = nummer.IndexOf(_RnOptions.Separator);
            if (index <= 0 || index >= (nummer.Length - 1)) return false;
            topNr = nummer.Substring(0, index);
            var startIndex = index + 1;
            raumNr = nummer.Substring(startIndex, nummer.Length - startIndex);
            return true;
        }

        private string GetTopNameFromRb(ObjectId oid, Transaction transaction)
        {
            var nummer = "";
            //nummer = GetBlockAttribute(HIDDEN_NUMMER_ATT, oid);
            nummer = GetBlockAttribute(_RnOptions.Attribname, oid, transaction);

            return GetTopName(nummer);
        }

        private string GetTopName(string number)
        {
            if (string.IsNullOrEmpty(_RnOptions.Separator))
            {
                return TillNumericChar(number);
            }
            else
            {
                return TillSeparator(number);
            }
        }

        private string TillNumericChar(string number)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var c in number.ToCharArray())
            {
                if (IsNumeric(c)) break;
                sb.Append(c);
            }
            return sb.ToString();
        }

        private bool IsNumeric(char c)
        {
            return c >= '0' && c <= '9';
        }

        private string TillSeparator(string number)
        {
            int index = number.IndexOf(_RnOptions.Separator);
            if (index <= 0) return string.Empty;
            return number.Substring(0, index);

        }
        #endregion

        #region Private

        internal static string GetTopNr(AreaEngine.FgRbStructure fgrb, Transaction transaction)
        {
            string topNr = null;

            if (fgrb == null || fgrb.FlaechenGrenze == ObjectId.Null) return topNr;
            var rb = Plan2Ext.Globs.GetXrecord(fgrb.FlaechenGrenze, XREC_TOP_IN_FG, transaction);
            if (rb != null)
            {
                TypedValue[] values = rb.AsArray();
                topNr = values[0].Value.ToString();
            }
            return topNr;
        }


        private List<AreaEngine.FgRbStructure> GetFgRbInSameTopWithExistingHatch(AreaEngine.FgRbStructure fgrb, Transaction transaction)
        {
            var fgrbs = GetFgRbInSameTop(fgrb, transaction);
            return fgrbs.Where(x => HasExistingHatch(x.FlaechenGrenze, transaction)).ToList();
        }

        private List<AreaEngine.FgRbStructure> GetFgRbInSameTop(AreaEngine.FgRbStructure fgrb, Transaction transaction)
        {
            var fgrbs = new List<AreaEngine.FgRbStructure>();
            // get topnr
            var topNr = GetTopNr(fgrb, transaction);
            if (topNr != null)
            {
                var fbrgsPerTop = _engineParameter.FgRbPerTopNr.GetFgRbStructures(topNr);
                fgrbs = fbrgsPerTop.Where(x => x != fgrb).ToList();
                //fgrbs = _FgRbStructs.Values.Where(x => x != fgrb && string.Compare(topNr, GetTopNr(x, transaction)) == 0).ToList();
            }
            return fgrbs;
        }

        private bool SelectBlockViaPoint(Transaction myT, out AreaEngine.FgRbStructure fg)
        {
            fg = null;
            _CurrentBlock = ObjectId.Null;
            Point3d raumPunkt = Point3d.Origin;
            if (!GetRaumPunkt(ref raumPunkt)) return false;
            raumPunkt = Plan2Ext.Globs.TransUcsWcs(raumPunkt);
            var foundFgs = _FgRbStructs.Values.Where(x => x.IsPointInFg(raumPunkt, myT)).ToList();
            var nrFound = foundFgs.Count;
            if (nrFound == 0)
            {
                _Editor.WriteMessage(string.Format(CultureInfo.CurrentCulture, "\nEs wurde kein Raum gefunden!"));
                return true;
            }
            else if (nrFound > 1)
            {
                _Editor.WriteMessage(string.Format(CultureInfo.CurrentCulture, "\nEs wurden {0} Räume gefunden!", nrFound.ToString()));
                return true;
            }
            else
            {
                fg = foundFgs[0];
                var nrBlocks = fg.Raumbloecke.Count;
                if (nrBlocks == 0)
                {
                    _Editor.WriteMessage(string.Format(CultureInfo.CurrentCulture, "\nEs wurde kein Raumblock gefunden!"));
                    return true;
                }
                else if (nrBlocks > 1)
                {
                    _Editor.WriteMessage(string.Format(CultureInfo.CurrentCulture, "\nEs wurden {0} Raumblöcke gefunden!", nrBlocks.ToString()));
                    return true;
                }
                else
                {
                    _CurrentBlock = fg.Raumbloecke[0];
                    _Editor.WriteMessage(string.Format(CultureInfo.CurrentCulture, "\nEs wurde ein Raum gefunden.", nrBlocks.ToString()));
                }
            }
            return true;
        }

        private void DeleteOldHatch(ObjectId fg)
        {

            var oid = GetHatch(fg);
            if (oid == ObjectId.Null) return;

            using (var trans = _TransMan.StartTransaction())
            {
                var o = trans.GetObject(oid, OpenMode.ForWrite, openErased: true);
                if (!o.IsErased)
                {
                    o.Erase();
                }
                trans.Commit();
            }
        }

        private void DeleteOldHatch(ObjectId fg, Transaction transaction)
        {

            var oid = GetHatch(fg, transaction);
            if (oid == ObjectId.Null) return;

            var o = transaction.GetObject(oid, OpenMode.ForWrite, openErased: true);
            if (!o.IsErased)
            {
                o.Erase();
            }
        }

        private bool IsErased(ObjectId oid)
        {
            if (oid == ObjectId.Null) return true;

            bool isErased = false;
            using (var trans = _TransMan.StartTransaction())
            {
                var o = trans.GetObject(oid, OpenMode.ForWrite, openErased: true);
                isErased = o.IsErased;
                trans.Commit();
            }

            return isErased;
        }

        private bool HasExistingHatch(ObjectId fg)
        {
            var h = GetHatch(fg);
            return !IsErased(h);
        }

        private bool HasExistingHatch(ObjectId fg, Transaction transaction)
        {
            var h = GetHatch(fg);
            return !IsErased(h);
        }

        private ObjectId GetHatch(ObjectId fg)
        {
            ObjectId oid = ObjectId.Null;
            var rb = Plan2Ext.Globs.GetXrecord(fg, XREC_HATCH_OF_RAUM);
            if (rb != null)
            {
                TypedValue[] values = rb.AsArray();
                try
                {
                    var handleString = values[0].Value.ToString();
                    oid = Plan2Ext.Globs.HandleStringToObjectId(oid, handleString);
                }
                catch (Exception)
                {
                }
            }
            return oid;
        }
        private ObjectId GetHatch(ObjectId fg, Transaction transaction)
        {
            ObjectId oid = ObjectId.Null;
            var rb = Plan2Ext.Globs.GetXrecord(fg, XREC_HATCH_OF_RAUM, transaction);
            if (rb != null)
            {
                TypedValue[] values = rb.AsArray();
                try
                {
                    var handleString = values[0].Value.ToString();
                    oid = Plan2Ext.Globs.HandleStringToObjectId(oid, handleString);
                }
                catch (Exception)
                {
                }
            }
            return oid;
        }

        //private string GetHatchLayer()
        //{
        //    var nrStr = _RnOptions.Top.Trim();
        //    return GetHatchLayerName(nrStr);
        //}

        private static string GetHatchLayerName(string nrStr)
        {
            return string.Format(CultureInfo.InvariantCulture, TOP_LAYER_PREFIX + "{0}_F", nrStr);
        }

        private static string GetDigitsFromTopPart(string nrStr)
        {
            var arr = nrStr.ToCharArray();
            string s = "";
            int i = nrStr.Length - 1;
            while (i >= 0 && Char.IsDigit(arr[i]))
            {
                s = arr[i] + s;
                i--;
            }
            if (s.Length < 2)
            {
                s = s.PadLeft(2, '0');
            }
            return s;
        }

        //private int GetHatchColor()
        //{
        //    var nrStr = _RnOptions.Top;
        //    return GetHatchColor(nrStr);
        //}

        //private int GetHatchColor(string nrStr)
        //{
        //    var cArr = nrStr.ToCharArray();
        //    for (int i = cArr.Length - 1; i >= 0; i--)
        //    {
        //        var c = cArr[i].ToString().ToUpperInvariant();
        //        int colorIndex;
        //        if (_ColorIndexDict.TryGetValue(c, out colorIndex))
        //        {
        //            return colorIndex;
        //        }
        //    }
        //    log.WarnFormat(CultureInfo.CurrentCulture, "Keine Farbe gefunden für Top '{0}'!", nrStr);
        //    return 7;
        //}

        private void SumM2(TopStructure topStruct, Transaction transaction)
        {
            var sumArea = 0.0;
            foreach (var fgrb in topStruct.FgRbs)
            {
                double rbArea;
                if (GetArea(fgrb.Raumbloecke[0], transaction, out rbArea))
                {
                    sumArea += rbArea;
                }
            }

            if (topStruct.TopOid == ObjectId.Null) return;

            var areaString = string.Format(CultureInfo.InvariantCulture, "{0}m2 Nfl.", sumArea.ToString("F2"));
            SetBlockAttrib(topStruct.TopOid, Commands.TOPBLOCK_M2_ATTNAME, areaString, transaction);
        }

        private double GetSumM2FromBlocks(TopStructure topStruct, Transaction myT)
        {
            double sumArea = 0.0;
            foreach (var fgrb in topStruct.FgRbs)
            {
                double rbArea;
                if (GetArea(fgrb.Raumbloecke[0], myT, out rbArea))
                {
                    sumArea += rbArea;
                }
            }
            return sumArea;
        }

        private bool GetArea(ObjectId oid, Transaction transaction, out double rbArea)
        {
            rbArea = -1.0;
            var rb = transaction.GetObject(oid, OpenMode.ForRead) as BlockReference;
            var m2Text = GetBlockAttribute(_RnOptions.FlaechenAttributName, rb, transaction);
            string prefix, suffix;
            double? d = Plan2Ext.Globs.GetFirstDoubleInString(m2Text.TextString, out prefix, out suffix);
            if (d == null)
            {
                InsertFehlerLineAt(oid, _WRONG_AREA_VALUE);
                return false;
            }
            rbArea = d.Value;
            return true;
        }

        private bool GetTopNrFromPfeilBlock(ObjectId oid, Transaction transaction, out string topNr)
        {
            topNr = string.Empty;
            var pfeilBlockRef = transaction.GetObject(oid, OpenMode.ForRead) as BlockReference;
            if (pfeilBlockRef == null)
            {
                InsertFehlerLineAt(oid, _TOP_ELEMENT_IS_NO_BLOCKREF);
                return false;
            }
            var topNrAtt = GetBlockAttribute(Commands.PFEILBLOCK_TOPNR_ATTNAME, pfeilBlockRef, transaction);
            if (topNrAtt == null)
            {
                InsertFehlerLineAt(oid, _TOP_HAS_NOT_TOP_ATTRIB);
                return false;
            }

            topNr = topNrAtt.TextString;
            return true;
        }
        private bool GetTopNrFromTopBlock(ObjectId oid, Transaction transaction, out string topNr)
        {
            topNr = string.Empty;
            var topBlockRef = transaction.GetObject(oid, OpenMode.ForRead) as BlockReference;
            if (topBlockRef == null)
            {
                InsertFehlerLineAt(oid, _TOP_ELEMENT_IS_NO_BLOCKREF);
                return false;
            }
            var topNrAtt = GetBlockAttribute(TopBlockTopNrAttName, topBlockRef, transaction);
            if (topNrAtt == null)
            {
                InsertFehlerLineAt(oid, _TOP_HAS_NOT_TOP_ATTRIB);
                return false;
            }

            topNr = topNrAtt.TextString;
            return true;
        }

        private bool GetCompleteTopNrFromTopBlock(ObjectId oid, Transaction transaction, out string topNr)
        {
            topNr = string.Empty;
            var topBlockRef = transaction.GetObject(oid, OpenMode.ForRead) as BlockReference;
            if (topBlockRef == null)
            {
                InsertFehlerLineAt(oid, _TOP_ELEMENT_IS_NO_BLOCKREF);
                return false;
            }
            var topNrAtt = GetBlockAttribute(TopBlockTopNrAttName, topBlockRef, transaction);
            if (topNrAtt == null)
            {
                InsertFehlerLineAt(oid, _TOP_HAS_NOT_TOP_ATTRIB);
                return false;
            }
            topNr = topNrAtt.TextString;
            return true;
        }

        private void HatchItAndSetTopToFg(AreaEngine.FgRbStructure fgrb, Transaction transaction)
        {
            SetTopForFg(_RnOptions.Top, fgrb, transaction);
            HatchIt(fgrb, transaction);
        }

        private void HatchMany(List<AreaEngine.FgRbStructure> otherFgRbs, Transaction transaction)
        {
            var ids = new ObjectIdCollection();
            foreach (var f in otherFgRbs)
            {
                var oid = HatchIt(f, transaction, false);
                if (oid != ObjectId.Null)
                {
                    ids.Add(oid);
                }
            }
            if (ids.Count > 0)
                Plan2Ext.Globs.DrawOrderBottom(ids, transaction);
        }

        private ObjectId HatchIt(AreaEngine.FgRbStructure fgrb, Transaction transaction, bool drawOrder = true)
        {
            var topNr = GetTopNr(fgrb, transaction);
            if (string.IsNullOrEmpty(topNr)) return ObjectId.Null;
            var inner = new List<ObjectId>();
            GetInnerPolysForHatchit(fgrb.FlaechenGrenze, fgrb.Inseln, fgrb.Abzugsflaechen, inner, transaction);
            //var inner = fgrb.Inseln;
            //inner.AddRange(fgrb.Abzugsflaechen);
            DeleteOldHatch(fgrb.FlaechenGrenze, transaction);
            var layer = GetOrCreateHatchLayer(topNr, transaction);
            bool needsRegen;
            Plan2Ext.Globs.LayerOnAndThaw(layer, true, transaction, _AcAp.Application.DocumentManager.MdiActiveDocument.Database, out needsRegen);
            var oid = fgrb.HatchPoly(fgrb.FlaechenGrenze, inner, layer, transaction);
            if (drawOrder)
            {
                var ids = new ObjectIdCollection();
                ids.Add(oid);
                Plan2Ext.Globs.DrawOrderBottom(ids, transaction);
            }
            var rb = new ResultBuffer(new TypedValue((int)DxfCode.Handle, oid.Handle));
            Plan2Ext.Globs.SetXrecord(fgrb.FlaechenGrenze, XREC_HATCH_OF_RAUM, rb, transaction);
            return oid;
        }


        /// <summary>
        /// Create Hatch for comple Top e.g. multiple outer borders.
        /// </summary>
        /// <param name="top"></param>
        /// <param name="transaction"></param>
        private ObjectId HatchIt(TopStructure top, Transaction transaction, bool drawOrder)
        {
            if (top == null || top.FgRbs == null) return ObjectId.Null;
            string nrStr = (top.TopOid.IsNull) ? top.TopNummer : GetTopNr(top);
            var layer = GetOrCreateHatchLayer(nrStr, transaction);
            bool needsRegen;
            Plan2Ext.Globs.LayerOnAndThaw(layer, true, transaction, _AcAp.Application.DocumentManager.MdiActiveDocument.Database, out needsRegen);
            var outerInner = new Dictionary<ObjectId, List<ObjectId>>();
            foreach (var fg in top.FgRbs)
            {
                var inner = new List<ObjectId>();
                GetInnerPolysForHatchit(fg.FlaechenGrenze, fg.Inseln, fg.Abzugsflaechen, inner, transaction);
                //AddToSet(inner, fg.Inseln);
                //AddToSet(inner, fg.Abzugsflaechen);
                DeleteOldHatch(fg.FlaechenGrenze);
                outerInner.Add(fg.FlaechenGrenze, inner);
            }

            var oid = Plan2Ext.Globs.HatchPoly(outerInner, layer, null, transaction);
            if (drawOrder)
            {
                var ids = new ObjectIdCollection {oid};
                Plan2Ext.Globs.DrawOrderBottom(ids);
            }
            var rb = new ResultBuffer(new TypedValue((int)DxfCode.Handle, oid.Handle));
            foreach (var fg in top.FgRbs)
            {
                Plan2Ext.Globs.SetXrecord(fg.FlaechenGrenze, XREC_HATCH_OF_RAUM, rb);
            }

            return oid;
        }

        private void GetInnerPolysForHatchit(ObjectId outerBoundary, List<ObjectId> innerboundaries, List<ObjectId> abzugsflaechen, List<ObjectId> inner, Transaction transaction)
        {

	        var allInner = new List<ObjectId>();
			allInner.AddRange(innerboundaries);
			allInner.AddRange(abzugsflaechen);
			if (allInner.Count == 0) return;

            var notErasedInnerBoundaries = allInner.Where(x => !x.IsErased);
            
            List<ObjectId> inseln2 = new List<ObjectId>();
            foreach (var oid in notErasedInnerBoundaries)
            {
                if (!PolyInPoly(transaction, oid, outerBoundary))
                {
                    log.DebugFormat("Vermeintliche Abzugsfläche {0} ist nicht innerhalb der Flächengrenze", oid.Handle.ToString());
                    continue;
                }

                inseln2.Add(oid);
            }

            List<ObjectId> inseln3 = new List<ObjectId>();
            for (int i = 0; i < inseln2.Count; i++)
            {
                ObjectId oid = inseln2[i];
                bool inOtherAf = false;
                for (int j = 0; j < inseln2.Count; j++)
                {
                    ObjectId compOid = inseln2[j];
                    if (i == j) continue;
                    if (PolyInPoly(transaction, oid, compOid))
                    {
                        //log.DebugFormat("Abzugsfläche {0} ist innerhalb anderer Abzugsfläche {1}.", oid.Handle.ToString(), compOid.Handle.ToString());
                        inOtherAf = true;
                        break;
                    }

                }
                if (!inOtherAf) inseln3.Add(oid);
            }

            inner.AddRange(inseln3);
        }

        private static bool PolyInPoly(Transaction transaction, ObjectId oid, ObjectId elFG)
        {
            using (DBObject pEntity = transaction.GetObject(oid, OpenMode.ForRead, false))
            {
                using (DBObject pElFG = transaction.GetObject(elFG, OpenMode.ForRead, false))
                {

                    if (pEntity is Polyline2d)
                    {
                        Polyline2d oldPolyline = (Polyline2d)pEntity;
                        foreach (ObjectId Vertex2d in oldPolyline)
                        {
                            using (DBObject dbobj = transaction.GetObject(Vertex2d, OpenMode.ForRead, false))
                            {
                                Vertex2d vertex = dbobj as Vertex2d;

                                if (vertex == null)
                                {
                                    string msg = string.Format(CultureInfo.CurrentCulture, "Polylinie {0} gibt falsches Objekt {1} als Vertex zurück.", oldPolyline.Handle.ToString(), dbobj.GetType().ToString());
                                    throw new InvalidOperationException(string.Format(msg));
                                }

                                Point3d vertexPoint = oldPolyline.VertexPosition(vertex);
                                if (!AreaEngine.InPoly(vertexPoint, (Entity)pElFG)) return false;

                            }
                        }
                        return true;
                    }
                    else if (pEntity is Polyline3d)
                    {
                        Polyline3d poly3d = (Polyline3d)pEntity;
                        foreach (ObjectId Vertex3d in poly3d)
                        {
                            using (DBObject dbobj = transaction.GetObject(Vertex3d, OpenMode.ForRead, false))
                            {
                                PolylineVertex3d vertex = dbobj as PolylineVertex3d;

                                if (vertex == null)
                                {
                                    string msg = string.Format(CultureInfo.CurrentCulture, "3D-Polylinie {0} gibt falsches Objekt {1} als Vertex zurück.", poly3d.Handle.ToString(), dbobj.GetType().ToString());
                                    throw new InvalidOperationException(string.Format(msg));
                                }

                                Point3d vertexPoint = vertex.Position;
                                if (!AreaEngine.InPoly(vertexPoint, (Entity)pElFG)) return false;

                            }
                        }
                        return true;
                    }
                    else if (pEntity is Polyline)
                    {
                        Polyline poly = pEntity as Polyline;
                        for (int i = 0; i < poly.NumberOfVertices; i++)
                        {
                            Point3d vertexPoint = poly.GetPoint3dAt(i);
                            if (!AreaEngine.InPoly(vertexPoint, (Entity)pElFG)) return false;

                        }
                        return true;
                    }
                }
            }
            return false;
        }


        private string GetOrCreateHatchLayer(string topNr, Transaction transaction)
        {
            string layer = GetHatchLayerName(topNr);
            var db = _AcAp.Application.DocumentManager.MdiActiveDocument.Database;
            if (!LayerManager.LayerExists(layer, transaction, db))
            {
                int colorIndex = _hatchColorServer.CurrentHatchColor;
                var color = _AcCm.Color.FromColorIndex(_AcCm.ColorMethod.ByAci, (short)colorIndex);
                LayerManager.VerifyLayerExists(layer, color, transaction, db);
                _hatchColorServer.IncrementHatchColor();
            }

            return layer;
        }

        private string GetTopNr(TopStructure top)
        {
            return top.TopNummer.Trim();
        }

        private void AddToSet(List<ObjectId> set, List<ObjectId> list)
        {
            foreach (var oid in list)
            {
                if (!set.Contains(oid)) set.Add(oid);
            }
        }

        private string GetTopNrFromCompleteNr(string completeNr)
        {
            string topPart = GetPartTilSeparator(completeNr);
            if (string.IsNullOrEmpty(topPart)) return string.Empty;
            return GetDigitsFromTopPart(topPart);
        }

        private string GetPartTilSeparator(string completeNr)
        {
            var index = completeNr.IndexOf(_RnOptions.Separator);
            if (index <= 0) return string.Empty;
            return completeNr.Substring(0, index);
        }


        private bool CheckIsSecondClickInSameRoom()
        {
            var isSecondClickInSameRoom = (_CurrentBlock == _LastBlock);
            _LastBlock = _CurrentBlock;
            return isSecondClickInSameRoom;
        }

        private bool GetRaumPunkt(ref Point3d point)
        {
            PromptPointOptions ppo = new PromptPointOptions("\nRaum wählen:");
            ppo.AllowNone = true;
            PromptPointResult ppr = _Editor.GetPoint(ppo);
            if (ppr.Status != PromptStatus.OK) return false;
            point = ppr.Value;
            return true;
        }

        private bool SelectBlock()
        {

            _CurrentBlock = ObjectId.Null;
            Editor ed = _AcAp.Application.DocumentManager.MdiActiveDocument.Editor;
            PromptEntityOptions entopts = new PromptEntityOptions("\nRaumblock wählen: ");
            entopts.SetRejectMessage(string.Format(CultureInfo.CurrentCulture, _NoRaumblockMessage, Blockname));
            entopts.AddAllowedClass(typeof(BlockReference), true);
            //entopts.Message = "Pick an entity of your choice from the drawing";
            PromptEntityResult ent = null;
            //ADDED INPUT CONTEXT REACTOR	

            bool ret = false;
            var ok = false;
            while (!ok)
            {

                //ed.PromptingForEntity += new PromptEntityOptionsEventHandler(handle_promptEntityOptions);
                //ed.PromptedForEntity += new PromptEntityResultEventHandler(handle_promptEntityResult);

                try
                {
                    //ent = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor.GetEntity("Get Object ");
                    ent = ed.GetEntity(entopts);
                }
                catch
                {
                    ed.WriteMessage("\nKein gültiges Elemente gewählt!");
                }
                finally
                {
                    //ed.PromptingForEntity -= new PromptEntityOptionsEventHandler(handle_promptEntityOptions);
                    //ed.PromptedForEntity -= new PromptEntityResultEventHandler(handle_promptEntityResult);
                }

                if (ent.Status != PromptStatus.OK)
                {
                    ok = true;
                    ret = false;
                }
                else
                {
                    ObjectId entid = ent.ObjectId;
                    using (Transaction myT = _TransMan.StartTransaction())
                    {
                        Entity entity = (Entity)_TransMan.GetObject(entid, OpenMode.ForRead);
                        BlockReference blockRef = entity as BlockReference;
                        if (string.Compare(blockRef.Name, Blockname, StringComparison.OrdinalIgnoreCase) != 0)
                        {
                            ed.WriteMessage(string.Format(CultureInfo.CurrentCulture, _NoRaumblockMessage, Blockname));
                        }
                        else
                        {
                            _CurrentBlock = entid;
                            ok = true;
                            ret = true;
                        }

                        myT.Commit();
                    }
                }
            }
            return ret;
        }

        private string GetBlockAttribute(string attName, ObjectId block, Transaction transaction)
        {
			if (block == ObjectId.Null || block.IsErased) return string.Empty;

            BlockReference blockEnt = transaction.GetObject(block, OpenMode.ForRead) as BlockReference;
            if (blockEnt != null)
            {
                AttributeReference attRef = null;

                attRef = GetBlockAttribute(attName, blockEnt, transaction);

                if (attRef != null)
                {
                    return attRef.TextString;
                }

            }
            return string.Empty;
        }

        private List<ObjectId> SelectAllHkBlocks()
        {
            string hkBlockName = HBlockname; // TheConfiguration.GetValueString(HK_BLOCKNAME_KONFIG);
            var ed = _AcAp.Application.DocumentManager.MdiActiveDocument.Editor;
            SelectionFilter filter = new SelectionFilter(new TypedValue[] {
				new TypedValue((int)DxfCode.Start,"INSERT" ),
				new TypedValue((int)DxfCode.BlockName,hkBlockName)
			});

            PromptSelectionResult res = ed.GetSelection(filter); // ed.SelectAll(filter);
            if (res.Status != PromptStatus.OK) return new List<ObjectId>();

            using (SelectionSet ss = res.Value)
            {
                return ss.GetObjectIds().ToList();
            }
        }

        public static List<ObjectId> SelectAllRaumblocks(string blockName)
        {
            var ed = _AcAp.Application.DocumentManager.MdiActiveDocument.Editor;
            SelectionFilter filter = new SelectionFilter(new TypedValue[] {
				new TypedValue((int)DxfCode.Start,"INSERT" ),
				new TypedValue((int)DxfCode.BlockName,blockName)
			});
            PromptSelectionResult res = ed.SelectAll(filter);
            if (res.Status != PromptStatus.OK) return new List<ObjectId>();

            using (SelectionSet ss = res.Value)
            {
                return ss.GetObjectIds().ToList();
            }
        }

        private List<ObjectId> SelectAllInfoElements()
        {
            var ed = _AcAp.Application.DocumentManager.MdiActiveDocument.Editor;
            SelectionFilter filter = new SelectionFilter(new TypedValue[] {
				new TypedValue((int)DxfCode.Operator,"<OR" ),

				new TypedValue((int)DxfCode.Operator,"<AND" ),
				new TypedValue((int)DxfCode.Start,"INSERT" ),
				new TypedValue((int)DxfCode.BlockName,Blockname),
				new TypedValue((int)DxfCode.Operator,"AND>" ),

				new TypedValue((int)DxfCode.Operator,"<AND" ),
				new TypedValue((int)DxfCode.Start,"INSERT" ),
				new TypedValue((int)DxfCode.BlockName,Commands.TOPBLOCKNAME),
				new TypedValue((int)DxfCode.Operator,"AND>" ),

				new TypedValue((int)DxfCode.Operator,"<AND" ),
				new TypedValue((int)DxfCode.Start,"HATCH" ),
				new TypedValue((int)DxfCode.LayerName, TOP_LAYER_PREFIX+ "*_F"),
				new TypedValue((int)DxfCode.Operator,"AND>" ),

				new TypedValue((int)DxfCode.Operator,"<AND" ),
				new TypedValue((int)DxfCode.Start,"*POLYLINE" ),
				new TypedValue((int)DxfCode.LayerName,_RnOptions.FlaechenGrenzeLayerName),
				new TypedValue((int)DxfCode.Operator,"AND>" ),

				new TypedValue((int)DxfCode.Operator,"OR>" ),

			});

            PromptSelectionOptions SelOpts = new PromptSelectionOptions();
            SelOpts.MessageForAdding = "Elemente für die Entfernung aller Raumnummern-Informationen wählen: ";
            PromptSelectionResult res = ed.GetSelection(SelOpts, filter);
            if (res.Status != PromptStatus.OK) return new List<ObjectId>();

            using (SelectionSet ss = res.Value)
            {
                return ss.GetObjectIds().ToList();
            }
        }

        private void SelectFgsandRbsAndTops(List<ObjectId> fgOids, List<ObjectId> rbOids,
            List<ObjectId> topOids)
        {
            var ed = _AcAp.Application.DocumentManager.MdiActiveDocument.Editor;
            SelectionFilter filter = new SelectionFilter(new TypedValue[] {
				new TypedValue((int)DxfCode.Operator,"<OR" ),
				new TypedValue((int)DxfCode.Operator,"<AND" ),
				new TypedValue((int)DxfCode.Start,"*POLYLINE" ),
				new TypedValue((int)DxfCode.LayerName , _RnOptions.FlaechenGrenzeLayerName),
				new TypedValue((int)DxfCode.Operator,"AND>" ),
				new TypedValue((int)DxfCode.Operator,"<AND" ),
				new TypedValue((int)DxfCode.Start,"INSERT" ),
				new TypedValue((int)DxfCode.BlockName,TopBlockName),
				new TypedValue((int)DxfCode.Operator,"AND>" ),
				new TypedValue((int)DxfCode.Operator,"<AND" ),
				new TypedValue((int)DxfCode.Start,"INSERT" ),
				new TypedValue((int)DxfCode.BlockName, _RnOptions.Blockname),
				new TypedValue((int)DxfCode.Operator,"AND>" ),
				new TypedValue((int)DxfCode.Operator,"OR>" ),
			});
            var options = new PromptSelectionOptions();
            options.MessageForAdding = "Flächengrenzen wählen: ";
            PromptSelectionResult res = ed.GetSelection(options, filter);
            if (res.Status != PromptStatus.OK) return;

            using (SelectionSet ss = res.Value)
            {
                var oids = ss.GetObjectIds().ToList();
                using (Transaction transaction = _TransMan.StartTransaction())
                {
                    foreach (var oid in oids)
                    {
                        if (oid.IsErased) continue;
                        var dbo = transaction.GetObject(oid, OpenMode.ForRead);
                        var poly = dbo as Polyline;
                        if (poly != null)
                        {
                            fgOids.Add(oid);
                            continue;
                        }

                        var blockReference = (BlockReference)dbo;
                        if (string.Equals(Plan2Ext.Globs.GetBlockname(blockReference, transaction),
                            _RnOptions.Blockname))
                        {
                            rbOids.Add(oid);
                        }
                        else
                        {
                            topOids.Add(oid);
                        }
                    }
                    transaction.Commit();
                }

                //return ss.GetObjectIds().ToList();
            }
        }


        private List<ObjectId> SelectFgs()
        {
            var ed = _AcAp.Application.DocumentManager.MdiActiveDocument.Editor;
            SelectionFilter filter = new SelectionFilter(new TypedValue[] {
				new TypedValue((int)DxfCode.Start,"*POLYLINE" ),
				new TypedValue((int)DxfCode.LayerName , _RnOptions.FlaechenGrenzeLayerName)
			});
            var options = new PromptSelectionOptions();
            options.MessageForAdding = "Flächengrenzen wählen: ";
            PromptSelectionResult res = ed.GetSelection(options, filter);
            if (res.Status != PromptStatus.OK) return new List<ObjectId>();

            using (SelectionSet ss = res.Value)
            {
                return ss.GetObjectIds().ToList();
            }
        }

        private List<ObjectId> SelectAllTopBlocks()
        {
            var ed = _AcAp.Application.DocumentManager.MdiActiveDocument.Editor;
            SelectionFilter filter = new SelectionFilter(new TypedValue[] {
				new TypedValue((int)DxfCode.Start,"INSERT" ),
				new TypedValue((int)DxfCode.BlockName,TopBlockName)
			});
            PromptSelectionResult res = ed.SelectAll(filter);
            if (res.Status != PromptStatus.OK) return new List<ObjectId>();

            using (SelectionSet ss = res.Value)
            {
                return ss.GetObjectIds().ToList();
            }
        }

        private List<ObjectId> SelectAllPfeilBlocks()
        {
            var ed = _AcAp.Application.DocumentManager.MdiActiveDocument.Editor;
            SelectionFilter filter = new SelectionFilter(new TypedValue[] {
				new TypedValue((int)DxfCode.Start,"INSERT" ),
				new TypedValue((int)DxfCode.BlockName,Commands.PFEILBLOCKNAME)
			});
            PromptSelectionResult res = ed.SelectAll(filter);
            if (res.Status != PromptStatus.OK) return new List<ObjectId>();

            using (SelectionSet ss = res.Value)
            {
                return ss.GetObjectIds().ToList();
            }
        }
        private void SetBlockAttrib(ObjectId oid, string attName, string val, Transaction transaction)
        {
            if (oid == ObjectId.Null) return;

            //using (Transaction transaction = _TransMan.StartTransaction())
            //{
            BlockReference blockEnt = _TransMan.GetObject(oid, OpenMode.ForRead) as BlockReference;
            if (blockEnt != null)
            {

                if (!SetBlockAttrib(blockEnt, attName, val, transaction))
                {
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Attribut '{0}' nicht gefunden!", attName));

                }

                //AttributeReference attRef = null;
                //attRef = GetBlockAttribute(attName, blockEnt);

                //if (attRef != null)
                //{
                //    attRef.UpgradeOpen();
                //    attRef.TextString = val;
                //}
                //else
                //{
                //    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Attribut '{0}' nicht gefunden!", attName));
                //}
            }

            //    transaction.Commit();
            //}
        }

        private bool SetBlockAttrib(BlockReference blockEnt, string attName, string val, Transaction transaction)
        {
            if (blockEnt == null) return false;
            AttributeReference attRef = null;
            attRef = GetBlockAttribute(attName, blockEnt, transaction);
            if (attRef != null)
            {
                attRef.UpgradeOpen();
                attRef.TextString = val;
                attRef.DowngradeOpen();
                return true;
            }
            else
            {
                return false;
            }
        }

        private AttributeReference GetBlockAttribute(string name, BlockReference blockEnt, Transaction transaction)
        {
            foreach (ObjectId attId in blockEnt.AttributeCollection)
            {
                if (attId.IsErased) continue;
                var anyAttRef = transaction.GetObject(attId, OpenMode.ForRead) as AttributeReference;
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

        private string GetCompleteNumber(string curNumber)
        {
            return _RnOptions.Top + _RnOptions.Separator + curNumber;
        }

        /// <summary>
        /// Es werden für das aktuelle Top für das aktuelle Geschoss die Räume neu durchnummeriert.
        /// </summary>
        /// <param name="thegeschoss"></param>
        /// <param name="transaction"></param>
        /// <param name="topName"></param>
        private void AutoCorrection(string topName, string thegeschoss, Transaction transaction)
        {
            // dzt nur mit Separator
            if (string.IsNullOrEmpty(_RnOptions.Separator)) return;

            List<ObjectId> oids;
            if (!_OidsPerTop.TryGetValue(topName, out oids)) return;
            var oidsinfo = GetOidsinfos(transaction, oids, thegeschoss);

            var sorted = oidsinfo.OrderBy(x => x.Nr).ToArray();
            for (var index = 0; index < sorted.Length; index++)
            {
                var oneOidsInfo = sorted[index];
                var num = (index + 1).ToString().PadLeft(2, '0');
                var completeNum = GetCompleteNumber(topName, thegeschoss + num);
                SetBlockAttrib(oneOidsInfo.Oid, NrAttribname, completeNum, transaction);
            }
        }

        private List<GeschNrOid> GetOidsinfos(Transaction transaction, List<ObjectId> oids, string thegeschoss)
        {
            var oidsinfo = new List<GeschNrOid>();
            foreach (var oid in oids)
            {
                string geschoss, nummer;
                var raumnr = GetRaumnr(oid, transaction);
                try
                {
                    Globs.GetGeschossAndNr(raumnr, out geschoss, out nummer);
                }
                catch (Exception e)
                {
                    continue;
                }

                if (geschoss.Equals(thegeschoss))
                    oidsinfo.Add(new GeschNrOid() { Oid = oid, Geschoss = geschoss, Nr = int.Parse(nummer) });
            }

            return oidsinfo;
        }

        private class GeschNrOid
        {
            public ObjectId Oid { get; set; }
            public string Geschoss { get; set; }
            public int Nr { get; set; }

        }

        private string GetCompleteNumber(string topName, string num)
        {
            return topName + _RnOptions.Separator + num;
        }

        private class OidComparer : IComparer<ObjectId>
        {
            private readonly Transaction _transaction;
            private readonly Engine _engine;

            public OidComparer(Transaction transaction, Engine engine)
            {
                _transaction = transaction;
                _engine = engine;
            }
            public int Compare(ObjectId x, ObjectId y)
            {
                var xnum = _engine.GetRaumnr(x, _transaction);
                int xi;
                if (!int.TryParse(xnum, out xi))
                {
                    xi = 0;
                }
                var ynum = _engine.GetRaumnr(y, _transaction);
                int yi;
                if (!int.TryParse(ynum, out yi))
                {
                    yi = 0;
                }

                return xi.CompareTo(yi);
            }
        }

        //private int Compare(ObjectId x, ObjectId y)
        //{
        //    var xnum = GetNumber(x);
        //    int xi;
        //    if (!int.TryParse(xnum, out xi))
        //    {
        //        xi = 0;
        //    }
        //    var ynum = GetNumber(y);
        //    int yi;
        //    if (!int.TryParse(ynum, out yi))
        //    {
        //        yi = 0;
        //    }

        //    return xi.CompareTo(yi);
        //}

        private void AutoIncrementHigherNumbers(string number, Transaction transaction)
        {
            // dzt nur mit Separator
            if (string.IsNullOrEmpty(_RnOptions.Separator)) return;

            string geschoss, nr;
            try
            {
                Globs.GetGeschossAndNr(number, out geschoss, out nr);
            }
            catch (InvalidOperationException)
            {
                return;
            }

            int i = int.Parse(nr);

            List<ObjectId> oids;
            string topName = _RnOptions.Top;
            if (!_OidsPerTop.TryGetValue(topName, out oids)) return;

            foreach (var oid in oids)
            {
                IncHigherNum(oid, i, geschoss, transaction);
            }
        }

        private void IncHigherNum(ObjectId oid, int num, string thegeschoss, Transaction transaction)
        {
            string raumnr = GetRaumnr(oid, transaction);
            if (string.IsNullOrEmpty(raumnr)) return;

            string geschoss = "", nr = "";
            try
            {
                Globs.GetGeschossAndNr(raumnr, out geschoss, out nr);
            }
            catch (InvalidOperationException)
            {
                return;
            }

            // nur innerhalb geschoss nummerieren
            if (!geschoss.Equals(thegeschoss)) return;

            var oldNum = int.Parse(nr);
            if (oldNum < num) return;

            raumnr = Increment(raumnr);
            var completeNum = GetCompleteNumber(raumnr);
            SetBlockAttrib(oid, NrAttribname, completeNum, transaction);
        }

        private string Increment(string number)
        {
            string geschoss, nr;
            Globs.GetGeschossAndNr(number, out geschoss, out nr);
            var i = int.Parse(nr);
            i++;
            return geschoss + i.ToString().PadLeft(2, '0');
        }
        #endregion

        #region Fehlerline-Handling

        private const string _RB_IN_MULTIPLE_ROOMS = "_Raumblock in mehreren Räumen";
        private const string _INVALID_NR_OF_RBS_IN_ROOM = "_Ungültige Anzahl Raumblöcke";
        private const string _TOP_HAS_NO_RBS = "_Top hat keine Räume";
        private const string _TOPS_HAVE_SAME_NR = "_Tops_haben_gleich_Nummer";
        private const string _RB_DOES_NOT_BELONG_TO_A_TOP = "_Raumblock gehört zu keinem Top";
        private const string _ROOM_DOESNT_BELONG_TO_A_TOP = "_Raum gehört nicht zu Top";
        private const string _WRONG_AREA_VALUE = "_Kein gültiger Flächenwert";
        private const string _TOP_ELEMENT_IS_NO_BLOCKREF = "_Top Element ist kein Block";
        private const string _TOP_HAS_NOT_TOP_ATTRIB = "_TopBlock hat kein TopAttribut";
        private const string _INVALID_TOP_NR = "_Topnummer ist ungültig";
        private const string _ROOM_HAS_WRONG_TOPNR = "_Raumblock hat falsche Topnummer";
        private const string _FG_HAS_NO_HATCH = "_Flächengrenze mit Top hat keine Schraffur";
        private const string _ROOM_HAS_WRONG_INFONR = "_Raumblock hat falsche Nummer in Info";
        private const string _NO_TOPBLOCK_FOR_ROOM = "_Kein Topblock für diesen Raum";
        private readonly Dictionary<string, FehlerLineInfo> _FehlerInfos = new Dictionary<string, FehlerLineInfo>
		{
			{_RB_IN_MULTIPLE_ROOMS, new FehlerLineInfo( layerName: _RB_IN_MULTIPLE_ROOMS) { Length=50, Ang=Math.PI*1.25, Col = _AcCm.Color.FromColorIndex(_AcCm.ColorMethod.ByColor, 1) }},
			{_INVALID_NR_OF_RBS_IN_ROOM, new FehlerLineInfo(layerName: _INVALID_NR_OF_RBS_IN_ROOM )},
			{_TOP_HAS_NO_RBS, new FehlerLineInfo(layerName: _TOP_HAS_NO_RBS )},
            {_TOPS_HAVE_SAME_NR, new FehlerLineInfo(layerName: _TOPS_HAVE_SAME_NR )},
			{_RB_DOES_NOT_BELONG_TO_A_TOP, new FehlerLineInfo(layerName: _RB_DOES_NOT_BELONG_TO_A_TOP )},
			{_ROOM_DOESNT_BELONG_TO_A_TOP, new FehlerLineInfo(layerName: _ROOM_DOESNT_BELONG_TO_A_TOP )},
			{_WRONG_AREA_VALUE, new FehlerLineInfo(layerName: _WRONG_AREA_VALUE )},
			{_TOP_ELEMENT_IS_NO_BLOCKREF, new FehlerLineInfo(layerName: _TOP_ELEMENT_IS_NO_BLOCKREF )},
			{_TOP_HAS_NOT_TOP_ATTRIB, new FehlerLineInfo(layerName: _TOP_HAS_NOT_TOP_ATTRIB )},
			{_INVALID_TOP_NR, new FehlerLineInfo(layerName: _INVALID_TOP_NR )},
			{_ROOM_HAS_WRONG_TOPNR, new FehlerLineInfo(layerName: _ROOM_HAS_WRONG_TOPNR )},
			{_ROOM_HAS_WRONG_INFONR, new FehlerLineInfo(layerName: _ROOM_HAS_WRONG_INFONR )},
			{_FG_HAS_NO_HATCH, new FehlerLineInfo(layerName: _FG_HAS_NO_HATCH )},
			{_NO_TOPBLOCK_FOR_ROOM, new FehlerLineInfo(layerName: _NO_TOPBLOCK_FOR_ROOM )},
		};

        private class FehlerLineInfo
        {
            private FehlerLineInfo() { }
            public FehlerLineInfo(string layerName)
            {
                Layername = layerName;
            }
            public string Layername { get; set; }
            private double _Length = 50;
            public double Length
            {
                get { return _Length; }
                set { _Length = value; }
            }
            private double _Ang = Math.PI * 1.25;
            public double Ang
            {
                get { return _Ang; }
                set { _Ang = value; }
            }

            private _AcCm.Color _Col = null;
            public _AcCm.Color Col
            {
                get { return _Col; }
                set { _Col = value; }
            }
        }
        private void InsertFehlerLineAt(ObjectId oid, string fehler)
        {
            var position = Plan2Ext.Globs.GetInsertPoint(oid);
            if (position == null) return;

            List<Point3d> points = new List<Point3d>() { position.Value };
            FehlerLineInfo fi = _FehlerInfos[fehler];
            Plan2Ext.Globs.InsertFehlerLines(points, fi.Layername, fi.Length, fi.Ang, fi.Col);
        }

        public void DeleteAllFehlerLines()
        {
            foreach (var kvp in _FehlerInfos)
            {
                Plan2Ext.Globs.DeleteFehlerLines(kvp.Value.Layername);
            }
        }

        private void FehlerLinesOnOff(bool on)
        {
            if (on)
            {
                foreach (var fi in _FehlerInfos.Values)
                {
                    Plan2Ext.Globs.LayerOnAndThaw(fi.Layername);
                }
            }
            else
            {
                var regexLayers = _FehlerInfos.Values.Select(x => "^" + x.Layername + "$").ToList();
                Plan2Ext.Globs.LayerOffRegex(regexLayers);
            }
        }

        //private Point3d? GetInsertPoint(ObjectId oid)
        //{
        //    Point3d? position = null;
        //    using (var trans = _TransMan.StartTransaction())
        //    {
        //        var obj = trans.GetObject(oid, OpenMode.ForRead);
        //        var rb = obj as BlockReference;
        //        if (rb != null)
        //        {
        //            position = rb.Position;
        //        }
        //        else
        //        {
        //            var poly = obj as Polyline;
        //            if (poly != null)
        //            {
        //                position = poly.GetPointAtDist(0.0);
        //            }
        //            else
        //            {
        //                log.WarnFormat(CultureInfo.CurrentCulture, "Kein Einfügepunkt für Element vom Typ '{0}'!", obj.GetType().Name);
        //            }
        //        }
        //        trans.Commit();
        //    }
        //    return position;
        //}
        #endregion
    }
}
