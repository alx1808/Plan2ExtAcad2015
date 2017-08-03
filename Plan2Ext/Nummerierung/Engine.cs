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


namespace Plan2Ext.Nummerierung
{
    internal class Engine
    {
        #region log4net Initialization
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Convert.ToString((typeof(Engine))));
        #endregion

        #region Const
        private const string _NoRaumblockMessage = "\nDas gewählte Element ist nicht der angegebene Block '{0}'.";
        //private const string DIST_RB_TO_FB_KONFIG = "alx_V:ino_rb_rb2fbDist";
        //private const string HK_BLOCKNAME_KONFIG = "alx_V:ino_rb_HkBlockName";
        #endregion

        #region Members
        private TransactionManager _TransMan = null;
        private ObjectId _CurrentBlock = ObjectId.Null;
        private NrOptions _RnOptions = null;
        //private Dictionary<string, List<ObjectId>> _OidsPerTop = new Dictionary<string, List<ObjectId>>();
        //private List<ObjectId> _AllRaumBlocks = new List<ObjectId>();
        //private double _MaxDist = 0.25;

        #endregion

        #region Lifecycle

        public Engine(NrOptions rnOptions)
        {

            this._RnOptions = rnOptions;

            //var sMaxDist = TheConfiguration.GetValueString(DIST_RB_TO_FB_KONFIG);
            //_MaxDist = double.Parse(sMaxDist, CultureInfo.InvariantCulture);

            Database db = _AcAp.Application.DocumentManager.MdiActiveDocument.Database;
            _TransMan = db.TransactionManager;

            //_AllRaumBlocks = SelectAllRaumblocks();

        }

        //private void MarkRbs()
        //{
        //    using (Transaction myT = _TransMan.StartTransaction())
        //    {
        //        foreach (var oid in _AllRaumBlocks)
        //        {
        //            BlockReference ent = _TransMan.GetObject(oid, OpenMode.ForRead) as BlockReference;
        //            MarkRbIfNumber(ent);
        //        }
        //        myT.Commit();
        //    }
        //}

        //private void MarkRbIfNumber(BlockReference ent)
        //{
        //    if (ent == null) return;
        //    var attRef = GetBlockAttribute(NrAttribname, ent);
        //    if (attRef == null) return;
        //    if (string.IsNullOrEmpty(attRef.TextString))
        //    {
        //        ent.Unhighlight();
        //    }
        //    else
        //    {
        //        ent.Highlight();
        //    }
        //}

        #endregion

        #region Internal
        internal bool AddNumber()
        {
            //MarkRbs();

            using (Transaction myT = _TransMan.StartTransaction())
            {
                if (!SelectBlock()) return false;


                //if (_RnOptions.AutoCorr)
                //{
                //    CalcBlocksPerTop();
                //    AutoIncrementHigherNumbers(_RnOptions.Number);
                //}

                if (_RnOptions.UseFirstAttrib)
                {
                    SetFirstBlockAttrib(_CurrentBlock, GetCompleteNumber(_RnOptions.Number));
                }
                else
                {
                    SetBlockAttrib(_CurrentBlock, _RnOptions.Attribname, GetCompleteNumber(_RnOptions.Number));
                }
                BlockReference br = _TransMan.GetObject(_CurrentBlock, OpenMode.ForRead) as BlockReference;
                //MarkRbIfNumber(br);


                //if (_RnOptions.AutoCorr)
                //{
                //    CalcBlocksPerTop();
                //    AutoCorrection(_RnOptions.Number.Length);
                //}

                string newNr = Increment(_RnOptions.Number);

                //if (_RnOptions.AutoCorr)
                //{
                //    int i;
                //    if (int.TryParse(_RnOptions.Number, out i))
                //    {
                //        i++;
                //        List<ObjectId> oids;
                //        if (_OidsPerTop.TryGetValue(_RnOptions.Top, out oids))
                //        {
                //            if (i > (oids.Count + 1)) i = oids.Count + 1;
                //            newNr = i.ToString().PadLeft(newNr.Length, '0');
                //        }
                //    }
                //}

                _RnOptions.SetNumber(newNr);

                myT.Commit();
            }

            return true;

        }

        #endregion

        #region Move Fußbodenhöhenblock
        //private RbInfo GetNearest(Point3d point3d, List<RbInfo> RbInsPoints)
        //{
        //    var sorted = RbInsPoints.OrderBy(x => x.Pos.Distance2dTo(point3d)).ToList();
        //    RbInfo ret = sorted[0];

        //    double dist = point3d.Distance2dTo(ret.Pos);
        //    if (dist > _MaxDist) return null;

        //    return ret;
        //}

        //private class RbInfo
        //{
        //    public Point3d Pos { get; set; }
        //    public double Rot { get; set; }
        //    public double Scale { get; set; }
        //}


        //private List<RbInfo> GetRbInfos()
        //{
        //    List<RbInfo> ret = new List<RbInfo>();
        //    foreach (var oid in _AllRaumBlocks)
        //    {
        //        BlockReference blockRef = _TransMan.GetObject(oid, OpenMode.ForRead) as BlockReference;
        //        if (blockRef == null) continue;

        //        ret.Add(new RbInfo()
        //        {
        //            Pos = new Point3d(blockRef.Position.X, blockRef.Position.Y, blockRef.Position.Z),
        //            Rot = blockRef.Rotation,
        //            Scale = blockRef.ScaleFactors.X
        //        });
        //    }

        //    return ret;

        //}

        #endregion

        #region Topname Handling

        //private void CalcBlocksPerTop()
        //{
        //    // dzt nur mit Separator
        //    if (string.IsNullOrEmpty(_RnOptions.Separator)) return;

        //    _OidsPerTop.Clear();

        //    var rblocks = _AllRaumBlocks;

        //    using (Transaction myT = _TransMan.StartTransaction())
        //    {
        //        foreach (var oid in rblocks)
        //        {
        //            string topName = GetTopName(oid);
        //            if (string.IsNullOrEmpty(topName)) continue;
        //            List<ObjectId> oids;
        //            if (!_OidsPerTop.TryGetValue(topName, out oids))
        //            {
        //                oids = new List<ObjectId>();
        //                _OidsPerTop.Add(topName, oids);
        //            }
        //            oids.Add(oid);
        //        }

        //        myT.Commit();
        //    }
        //}

        //private string GetNumber(ObjectId oid)
        //{
        //    string attVal = GetBlockAttribute(_RnOptions.Attribname, oid);
        //    return GetNumber(attVal);
        //}

        //private string GetNumber(string attVal)
        //{
        //    if (string.IsNullOrEmpty(_RnOptions.Separator))
        //    {
        //        return FromNumericChar(attVal);
        //    }
        //    else
        //    {
        //        return FromSeparator(attVal);
        //    }
        //}

        //private string FromSeparator(string attVal)
        //{
        //    int index = attVal.IndexOf(_RnOptions.Separator);
        //    if (index < 0) return string.Empty;
        //    return attVal.Remove(0, index + 1);

        //}

        //private string FromNumericChar(string attVal)
        //{
        //    var charr = attVal.ToCharArray();
        //    int i = charr.Length - 1;
        //    for (; i >= 0; i--)
        //    {
        //        if (!IsNumeric(charr[i])) break;
        //    }
        //    if (i < 0) return attVal;
        //    return attVal.Remove(0, i + 1);
        //}

        //private string GetTopName(ObjectId oid)
        //{
        //    string number = GetBlockAttribute(_RnOptions.Attribname, oid);
        //    return GetTopName(number);
        //}

        //private string GetTopName(string number)
        //{
        //    if (string.IsNullOrEmpty(_RnOptions.Separator))
        //    {
        //        return TillNumericChar(number);
        //    }
        //    else
        //    {
        //        return TillSeparator(number);
        //    }
        //}

        //private string TillNumericChar(string number)
        //{
        //    StringBuilder sb = new StringBuilder();
        //    foreach (var c in number.ToCharArray())
        //    {
        //        if (IsNumeric(c)) break;
        //        sb.Append(c);
        //    }
        //    return sb.ToString();
        //}

        //private bool IsNumeric(char c)
        //{
        //    return c >= '0' && c <= '9';
        //}

        //private string TillSeparator(string number)
        //{
        //    int index = number.IndexOf(_RnOptions.Separator);
        //    if (index <= 0) return string.Empty;
        //    return number.Substring(0, index);

        //}
        #endregion

        #region Private
        private bool SelectBlock()
        {

            _CurrentBlock = ObjectId.Null;
            Editor ed = _AcAp.Application.DocumentManager.MdiActiveDocument.Editor;
            PromptEntityOptions entopts = new PromptEntityOptions("\nBlock wählen: ");
            entopts.SetRejectMessage(string.Format(CultureInfo.CurrentCulture, _NoRaumblockMessage, _RnOptions.Blockname));
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
                else if (_RnOptions.UseFirstAttrib)
                {
                    //
                    _CurrentBlock = ent.ObjectId;
                    ok = true;
                    ret = true;
                }
                else
                {

                    ObjectId entid = ent.ObjectId;
                    using (Transaction myT = _TransMan.StartTransaction())
                    {
                        Entity entity = (Entity)_TransMan.GetObject(entid, OpenMode.ForRead);
                        BlockReference blockRef = entity as BlockReference;
                        if (string.Compare(blockRef.Name, _RnOptions.Blockname, StringComparison.OrdinalIgnoreCase) != 0)
                        {
                            ed.WriteMessage(string.Format(CultureInfo.CurrentCulture, _NoRaumblockMessage, _RnOptions.Blockname));
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

        //private string GetBlockAttribute(string attName, ObjectId block)
        //{
        //    BlockReference blockEnt = _TransMan.GetObject(block, OpenMode.ForRead) as BlockReference;
        //    if (blockEnt != null)
        //    {
        //        AttributeReference attRef = null;

        //        attRef = GetBlockAttribute(attName, blockEnt);

        //        if (attRef != null)
        //        {
        //            return attRef.TextString;
        //        }

        //    }
        //    return string.Empty;
        //}

        //        private List<ObjectId> SelectAllRaumblocks()
        //        {
        //            var ed = _AcAp.Application.DocumentManager.MdiActiveDocument.Editor;
        //            SelectionFilter filter = new SelectionFilter(new TypedValue[] { 
        //                new TypedValue((int)DxfCode.Start,"INSERT" ),
        //                new TypedValue((int)DxfCode.BlockName,_RnOptions.Blockname)
        //            });
        //            PromptSelectionResult res = ed.SelectAll(filter);
        //            if (res.Status != PromptStatus.OK) return new List<ObjectId>();

        //#if BRX_APP
        //            SelectionSet ss = res.Value;
        //#else
        //            using (SelectionSet ss = res.Value)
        //#endif
        //            {
        //                return ss.GetObjectIds().ToList();
        //            }
        //        }

        private void SetFirstBlockAttrib(ObjectId oid, string val)
        {
            if (oid == ObjectId.Null) return;

            using (Transaction myT = _TransMan.StartTransaction())
            {
                BlockReference blockEnt = _TransMan.GetObject(oid, OpenMode.ForRead) as BlockReference;
                if (blockEnt != null)
                {
                    AttributeReference attRef = GetFirstBlockAttribute(blockEnt);
                    if (attRef != null)
                    {
                        attRef.UpgradeOpen();
                        attRef.TextString = val;
                    }
                    else
                    {
                        throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Der Block '{0}' hat keine Attribute '{0}'!", blockEnt.Name));
                    }
                }

                myT.Commit();
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

        private AttributeReference GetFirstBlockAttribute(BlockReference blockEnt)
        {
            if (blockEnt.AttributeCollection.Count == 0) return null;
            var attId = blockEnt.AttributeCollection[0];
            return _TransMan.GetObject(attId, OpenMode.ForRead) as AttributeReference;
        }


        private string GetCompleteNumber(string curNumber)
        {
            return _RnOptions.Top + _RnOptions.Separator + curNumber;
        }

        //private void AutoCorrection(int numlen)
        //{
        //    // dzt nur mit Separator
        //    if (string.IsNullOrEmpty(_RnOptions.Separator)) return;

        //    foreach (var topName in _OidsPerTop.Keys)
        //    {

        //        List<ObjectId> oids = _OidsPerTop[topName];
        //        //string topNameU = _RnOptions.Top.ToUpperInvariant();
        //        //if (!_OidsPerTop.TryGetValue(topNameU, out oids)) return;

        //        oids.Sort(Compare);

        //        for (int i = 0; i < oids.Count; i++)
        //        {
        //            string num = (i + 1).ToString().PadLeft(numlen, '0');
        //            SetBlockAttrib(oids[i], _RnOptions.Attribname, GetCompleteNumber(topName, num));

        //        }
        //    }

        //}

        //private string GetCompleteNumber(string topName, string num)
        //{
        //    return topName + _RnOptions.Separator + num;
        //}

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

        //private void AutoIncrementHigherNumbers(string number)
        //{
        //    // dzt nur mit Separator
        //    if (string.IsNullOrEmpty(_RnOptions.Separator)) return;

        //    int num;
        //    if (!int.TryParse(number, out num)) return;

        //    List<ObjectId> oids;
        //    string topName = _RnOptions.Top;
        //    if (!_OidsPerTop.TryGetValue(topName, out oids)) return;

        //    foreach (var oid in oids)
        //    {
        //        IncHigherNum(oid, num);
        //    }
        //}

        //private void IncHigherNum(ObjectId oid, int num)
        //{
        //    string number = GetNumber(oid);
        //    if (string.IsNullOrEmpty(number)) return;

        //    int oldNum;
        //    if (!int.TryParse(number, out oldNum)) return;

        //    if (oldNum >= num)
        //    {
        //        number = Increment(number);
        //        SetBlockAttrib(oid, _RnOptions.Attribname, GetCompleteNumber(number));
        //    }
        //}

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
