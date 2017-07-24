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

#if BRX_APP
using _AcAp = Bricscad.ApplicationServices;
using Teigha.DatabaseServices;
using Bricscad.EditorInput;
using Teigha.Geometry;
using _AcIntCom = BricscadDb;
#elif ARX_APP
using _AcAp = Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using _AcIntCom = Autodesk.AutoCAD.Interop.Common;
#endif


namespace Plan2Ext.Raumnummern
{
    internal class Engine
    {
        #region log4net Initialization
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Convert.ToString((typeof(Engine))));
        #endregion

        #region Const
        private const string _NoRaumblockMessage = "\nDas gewählte Element ist kein Raumblock '{0}'.";
        private const string DIST_RB_TO_FB_KONFIG = "alx_V:ino_rb_rb2fbDist";
        private const string HATCH_OF_RAUM = "Plan2RoomHatch";

        private readonly Dictionary<int, int> _ColorIndexDict = new Dictionary<int, int>() { 
            { 1, 141},
            { 2, 21},
            { 3, 72},
            { 4, 50},
            { 5, 24},
            { 6, 111},
            { 7, 11},
            { 8, 53},
            { 9, 101},
            { 0, 31},
        };

        #endregion

        #region Members
        private TransactionManager _TransMan = null;
        private ObjectId _CurrentBlock = ObjectId.Null;
        private Editor _Editor = null;
        private RnOptions _RnOptions = null;
        private Dictionary<string, List<ObjectId>> _OidsPerTop = new Dictionary<string, List<ObjectId>>();
        private List<ObjectId> _AllRaumBlocks = new List<ObjectId>();
        private double _MaxDist = 0.25;
        Dictionary<ObjectId, Plan2Ext.AreaEngine.FgRbStructure> _FgRbStructs = new Dictionary<ObjectId, AreaEngine.FgRbStructure>();

        public string Blockname { get; set; }
        public string NrAttribname { get; set; }
        public string HBlockname { get; set; }

        #endregion

        #region Lifecycle

        public Engine(RnOptions rnOptions)
        {

            this._RnOptions = rnOptions;
            Blockname = rnOptions.Blockname;
            NrAttribname = rnOptions.Attribname;
            HBlockname = rnOptions.HBlockname;

            var sMaxDist = TheConfiguration.GetValueString(DIST_RB_TO_FB_KONFIG);
            _MaxDist = double.Parse(sMaxDist, CultureInfo.InvariantCulture);

            Database db = _AcAp.Application.DocumentManager.MdiActiveDocument.Database;
            _TransMan = db.TransactionManager;
            _Editor = _AcAp.Application.DocumentManager.MdiActiveDocument.Editor;

            _FgRbStructs = AreaEngine.GetFgRbStructs(this._RnOptions.Blockname, this._RnOptions.FlaechenGrenzeLayerName, this._RnOptions.AbzFlaechenGrenzeLayerName, db);


            _AllRaumBlocks = SelectAllRaumblocks();

        }

        private void MarkRbs()
        {
            using (Transaction myT = _TransMan.StartTransaction())
            {
                foreach (var oid in _AllRaumBlocks)
                {
                    BlockReference ent = _TransMan.GetObject(oid, OpenMode.ForRead) as BlockReference;
                    MarkRbIfNumber(ent);
                }
                myT.Commit();
            }
        }

        private void MarkRbIfNumber(BlockReference ent)
        {
            if (ent == null) return;
            var attRef = GetBlockAttribute(NrAttribname, ent);
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
        internal void MoveFbh(double xDist, double yDist)
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

                    Vector2d v = new Vector2d(xDist * par.Scale, yDist * par.Scale);
                    v = v.RotateBy(par.Rot);
                    Vector3d v3 = new Vector3d(v.X, v.Y, 0.0);
                    Point3d newP = par.Pos.Add(v3);

                    Vector3d vAtt = blockRef.Position.GetVectorTo(newP);
                    blockRef.UpgradeOpen();
                    blockRef.Position = newP;
                    foreach (ObjectId attId in blockRef.AttributeCollection)
                    {
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

        internal bool AddNumber()
        {
            MarkRbs();

            using (Transaction myT = _TransMan.StartTransaction())
            {
                //if (!SelectBlock()) return false; // old variante

                AreaEngine.FgRbStructure fg = null;
                if (!SelectBlockViaPoint(myT, out fg)) return false;
                if (fg == null) return true;
                if (_CurrentBlock == ObjectId.Null) return true;

                HatchIt(fg);

                if (_RnOptions.AutoCorr)
                {
                    CalcBlocksPerTop();
                    AutoIncrementHigherNumbers(_RnOptions.Number);
                }

                SetBlockAttrib(_CurrentBlock, NrAttribname, GetCompleteNumber(_RnOptions.Number));

                BlockReference br = _TransMan.GetObject(_CurrentBlock, OpenMode.ForRead) as BlockReference;
                MarkRbIfNumber(br);


                if (_RnOptions.AutoCorr)
                {
                    CalcBlocksPerTop();
                    AutoCorrection(_RnOptions.Number.Length);
                }

                string newNr = Increment(_RnOptions.Number);

                if (_RnOptions.AutoCorr)
                {
                    int i;
                    if (int.TryParse(_RnOptions.Number, out i))
                    {
                        i++;
                        List<ObjectId> oids;
                        if (_OidsPerTop.TryGetValue(_RnOptions.Top, out oids))
                        {
                            if (i > (oids.Count + 1)) i = oids.Count + 1;
                            newNr = i.ToString().PadLeft(newNr.Length, '0');
                        }
                    }
                }

                _RnOptions.SetNumber(newNr);

                myT.Commit();
            }

            return true;
        }

        private void HatchIt(AreaEngine.FgRbStructure fg)
        {
            var inner = fg.Inseln.ToList();
            inner.AddRange(fg.Abzugsflaechen.ToList());
            int color = GetHatchColor();
            string layer = GetHatchLayer();
            Plan2Ext.Globs.LayerOnAndThaw(layer);
            DeleteOldHatch(fg.FlaechenGrenze);
            var oid = fg.HatchPoly(fg.FlaechenGrenze, inner, layer, color, _TransMan);
            var ids = new ObjectIdCollection();
            ids.Add(oid);
            Plan2Ext.Globs.DrawOrderBottom(ids);
            var rb = new ResultBuffer(new TypedValue((int)DxfCode.Handle, oid.Handle));
            Plan2Ext.Globs.SetXrecord(fg.FlaechenGrenze, HATCH_OF_RAUM, rb);

        }

        //        private void DeleteHatchesForTop(AreaEngine.FgRbStructure fg)
        //        {
        //            var completeNr = GetBlockAttrib(_CurrentBlock, _RnOptions.Attribname);  
        //            if (string.IsNullOrEmpty(completeNr))                return;
        //            string nrPart = GetTopNrFromCompleteNr(completeNr);
        //            if (string.IsNullOrEmpty(nrPart)) return;
        //            var layer = string.Format(CultureInfo.InvariantCulture, "A_RA_TOP_{0}_F", nrPart);


        //            var ed = _AcAp.Application.DocumentManager.MdiActiveDocument.Editor;
        //            SelectionFilter filter = new SelectionFilter(new TypedValue[] { 
        //                new TypedValue((int)DxfCode.Start,"HATCH" ),
        //                new TypedValue((int)DxfCode.LayerName,layer)
        //            });

        //            PromptSelectionResult res = ed.SelectAll(filter);
        //            if (res.Status != PromptStatus.OK) return;

        //#if BRX_APP
        //            SelectionSet ss = res.Value;
        //#else
        //            using (SelectionSet ss = res.Value)
        //#endif
        //            {
        //                var oids = ss.GetObjectIds().ToList();
        //                using (var trans = _TransMan.StartTransaction())
        //                {
        //                    foreach (var oid in oids)
        //                    {
        //                        var hatch = trans.GetObject(oid, OpenMode.ForWrite);
        //                        hatch.Erase();
        //                    }
        //                    trans.Commit();
        //                }
        //            }
        //        }

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

        internal bool RemoveRaum()
        {
            MarkRbs();
            using (Transaction myT = _TransMan.StartTransaction())
            {
                AreaEngine.FgRbStructure fg = null;
                if (!SelectBlockViaPoint(myT, out fg)) return false;
                if (fg == null) return true;
                if (_CurrentBlock == ObjectId.Null) return true;

                DeleteOldHatch(fg.FlaechenGrenze);
                SetBlockAttrib(_CurrentBlock, NrAttribname, "");

                BlockReference br = _TransMan.GetObject(_CurrentBlock, OpenMode.ForRead) as BlockReference;
                MarkRbIfNumber(br);
                myT.Commit();
            }
            return true;
        }

        private bool SelectBlockViaPoint(Transaction myT, out AreaEngine.FgRbStructure fg)
        {
            fg = null;
            _CurrentBlock = ObjectId.Null;
            Point3d raumPunkt = Point3d.Origin;
            if (!GetRaumPunkt(ref raumPunkt)) return false;
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
            ObjectId oid = ObjectId.Null;
            var rb = Plan2Ext.Globs.GetXrecord(fg, HATCH_OF_RAUM);
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
            if (oid == ObjectId.Null) return;

            using (var trans = _TransMan.StartTransaction())
            {
                var o = trans.GetObject(oid, OpenMode.ForWrite, true);
                if (!o.IsErased)
                {
                    o.Erase();
                }
                trans.Commit();
            }
        }

        private string GetHatchLayer()
        {
            return string.Format(CultureInfo.InvariantCulture, "A_RA_TOP_{0}_F", _RnOptions.Top.Trim());
            //var nrStr = _RnOptions.Top;
            //if (!string.IsNullOrEmpty(nrStr))
            //{
            //    string s = GetDigitsFromTopPart(nrStr);
            //    return string.Format(CultureInfo.InvariantCulture, "A_RA_TOP_{0}_F", s);
            //}

            //log.WarnFormat(CultureInfo.CurrentCulture, "Keinen Layer gefunden für Top '{0}'!", nrStr);
            //return "A_RA_TOP__F";
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

        private int GetHatchColor()
        {
            var nrStr = _RnOptions.Top;
            var cArr = nrStr.ToCharArray();
            for (int i = cArr.Length - 1; i >= 0; i--)
            {
                var c = cArr[i];
                if (Char.IsDigit(c))
                {
                    return _ColorIndexDict[int.Parse(c.ToString())];
                }
            }
            //if (!string.IsNullOrEmpty(nrStr))
            //{
            //    var lastZiffer = nrStr.Substring(nrStr.Length - 1, 1);
            //    int i;
            //    if (int.TryParse(lastZiffer, out i))
            //    {
            //        return _ColorIndexDict[i];
            //    }
            //}
            log.WarnFormat(CultureInfo.CurrentCulture, "Keine Farbe gefunden für Top '{0}'!", nrStr);
            return 7;
        }

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
                    Scale = blockRef.ScaleFactors.X
                });
            }
            return ret;
        }

        #endregion

        #region Topname Handling

        private void CalcBlocksPerTop()
        {
            // dzt nur mit Separator
            if (string.IsNullOrEmpty(_RnOptions.Separator)) return;

            _OidsPerTop.Clear();

            var rblocks = _AllRaumBlocks;

            using (Transaction myT = _TransMan.StartTransaction())
            {
                foreach (var oid in rblocks)
                {
                    string topName = GetTopName(oid);
                    if (string.IsNullOrEmpty(topName)) continue;
                    List<ObjectId> oids;
                    if (!_OidsPerTop.TryGetValue(topName, out oids))
                    {
                        oids = new List<ObjectId>();
                        _OidsPerTop.Add(topName, oids);
                    }
                    oids.Add(oid);
                }

                myT.Commit();
            }
        }

        private string GetNumber(ObjectId oid)
        {
            string attVal = GetBlockAttribute(NrAttribname, oid);
            return GetNumber(attVal);
        }

        private string GetNumber(string attVal)
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
            int index = attVal.IndexOf(_RnOptions.Separator);
            if (index < 0) return string.Empty;
            return attVal.Remove(0, index + 1);

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

        private string GetTopName(ObjectId oid)
        {
            string number = GetBlockAttribute(NrAttribname, oid);
            return GetTopName(number);
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

        private string GetBlockAttribute(string attName, ObjectId block)
        {
            BlockReference blockEnt = _TransMan.GetObject(block, OpenMode.ForRead) as BlockReference;
            if (blockEnt != null)
            {
                AttributeReference attRef = null;

                attRef = GetBlockAttribute(attName, blockEnt);

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

#if BRX_APP
            SelectionSet ss = res.Value;
#else
            using (SelectionSet ss = res.Value)
#endif
            {
                return ss.GetObjectIds().ToList();
            }
        }

        private List<ObjectId> SelectAllRaumblocks()
        {
            var ed = _AcAp.Application.DocumentManager.MdiActiveDocument.Editor;
            SelectionFilter filter = new SelectionFilter(new TypedValue[] { 
                new TypedValue((int)DxfCode.Start,"INSERT" ),
                new TypedValue((int)DxfCode.BlockName,Blockname)
            });
            PromptSelectionResult res = ed.SelectAll(filter);
            if (res.Status != PromptStatus.OK) return new List<ObjectId>();

#if BRX_APP
            SelectionSet ss = res.Value;
#else
            using (SelectionSet ss = res.Value)
#endif
            {
                return ss.GetObjectIds().ToList();
            }
        }

        private void SetBlockAttrib(ObjectId oid, string attName, string val)
        {
            if (oid == ObjectId.Null) return;

            using (Transaction myT = _TransMan.StartTransaction())
            {
                BlockReference blockEnt = _TransMan.GetObject(oid, OpenMode.ForRead) as BlockReference;
                if (blockEnt != null)
                {
                    AttributeReference attRef = null;

                    attRef = GetBlockAttribute(attName, blockEnt);

                    if (attRef != null)
                    {
                        attRef.UpgradeOpen();
                        attRef.TextString = val;
                    }
                    else
                    {
                        throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Attribut '{0}' nicht gefunden!", attName));
                    }
                }

                myT.Commit();
            }

        }

        private string GetBlockAttrib(ObjectId oid, string attName)
        {
            if (oid == ObjectId.Null) throw new ArgumentNullException();

            string val = string.Empty;

            using (Transaction myT = _TransMan.StartTransaction())
            {
                BlockReference blockEnt = _TransMan.GetObject(oid, OpenMode.ForRead) as BlockReference;
                if (blockEnt != null)
                {
                    var attRef = GetBlockAttribute(attName, blockEnt);
                    val = attRef.TextString;
                }

                myT.Commit();
            }
            return val;
        }

        private AttributeReference GetBlockAttribute(string name, BlockReference blockEnt)
        {
            foreach (ObjectId attId in blockEnt.AttributeCollection)
            {
                var anyAttRef = _TransMan.GetObject(attId, OpenMode.ForRead) as AttributeReference;
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
            return "TOP" + _RnOptions.Top + _RnOptions.Separator + curNumber;
        }

        private void AutoCorrection(int numlen)
        {
            // dzt nur mit Separator
            if (string.IsNullOrEmpty(_RnOptions.Separator)) return;

            foreach (var topName in _OidsPerTop.Keys)
            {

                List<ObjectId> oids = _OidsPerTop[topName];
                //string topNameU = _RnOptions.Top.ToUpperInvariant();
                //if (!_OidsPerTop.TryGetValue(topNameU, out oids)) return;

                oids.Sort(Compare);

                for (int i = 0; i < oids.Count; i++)
                {
                    string num = (i + 1).ToString().PadLeft(numlen, '0');
                    SetBlockAttrib(oids[i], NrAttribname, GetCompleteNumber(topName, num));

                }
            }

        }

        private string GetCompleteNumber(string topName, string num)
        {
            return topName + _RnOptions.Separator + num;
        }

        private int Compare(ObjectId x, ObjectId y)
        {
            var xnum = GetNumber(x);
            int xi;
            if (!int.TryParse(xnum, out xi))
            {
                xi = 0;
            }
            var ynum = GetNumber(y);
            int yi;
            if (!int.TryParse(ynum, out yi))
            {
                yi = 0;
            }

            return xi.CompareTo(yi);
        }

        private void AutoIncrementHigherNumbers(string number)
        {
            // dzt nur mit Separator
            if (string.IsNullOrEmpty(_RnOptions.Separator)) return;

            int num;
            if (!int.TryParse(number, out num)) return;

            List<ObjectId> oids;
            string topName = _RnOptions.Top;
            if (!_OidsPerTop.TryGetValue(topName, out oids)) return;

            foreach (var oid in oids)
            {
                IncHigherNum(oid, num);
            }
        }

        private void IncHigherNum(ObjectId oid, int num)
        {
            string number = GetNumber(oid);
            if (string.IsNullOrEmpty(number)) return;

            int oldNum;
            if (!int.TryParse(number, out oldNum)) return;

            if (oldNum >= num)
            {
                number = Increment(number);
                SetBlockAttrib(oid, NrAttribname, GetCompleteNumber(number));
            }
        }

        private string Increment(string number)
        {
            int len = number.Length;
            int i = int.Parse(number);
            i++;
            string s = i.ToString();
            return s.PadLeft(len, '0');
        }
        #endregion
    }
}
