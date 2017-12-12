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
using Autodesk.AutoCAD.Colors;
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
        private const string ID_NOT_UNIQUE = "_ATTRIBUT_NICHT_EINDEUTIG";
        #endregion

        #region Members
        private TransactionManager _TransMan = null;
        private ObjectId _CurrentBlock = ObjectId.Null;
        private ObjectId _LastBlock = ObjectId.Null;
        private string _LastAttName = string.Empty;
        private NrOptions _RnOptions = null;
        List<ObjectId> _Blocks = new List<ObjectId>();
        #endregion

        #region Lifecycle

        public Engine(NrOptions rnOptions)
        {
            this._RnOptions = rnOptions;

            Database db = _AcAp.Application.DocumentManager.MdiActiveDocument.Database;
            _TransMan = db.TransactionManager;
        }
        #endregion

        #region Internal
        internal bool AddNumber()
        {
            bool result = false;
            using (Transaction myT = _TransMan.StartTransaction())
            {
                var res = SelectBlock();
                switch (res)
                {
                    case SelectBlockReturn.terminate:
                        _LastAttName = string.Empty;
                        _LastBlock = ObjectId.Null;
                        break;
                    case SelectBlockReturn.entSelected:
                        _LastBlock = _CurrentBlock;
                        if (_RnOptions.UseFirstAttrib)
                        {
                            SetFirstBlockAttrib(_CurrentBlock, GetCompleteNumber(_RnOptions.Number));
                        }
                        else
                        {
                            SetBlockAttrib(_CurrentBlock, _RnOptions.Attribname, GetCompleteNumber(_RnOptions.Number));
                        }
                        BlockReference br = _TransMan.GetObject(_CurrentBlock, OpenMode.ForRead) as BlockReference;
                        string newNr = Increment(_RnOptions.Number);
                        _RnOptions.SetNumber(newNr);
                        result = true;
                        break;
                    case SelectBlockReturn.kwEnd:
                        if (_LastBlock != ObjectId.Null && !String.IsNullOrEmpty(_LastAttName))
                        {
                            AppendEToBlockAttrib(_LastBlock, _LastAttName);
                        }
                        break;
                    default:
                        break;
                }
                myT.Commit();
            }
            return result;
        }

        internal void CheckUniqueness()
        {
            log.Info("CheckUniqueness");
            if (!SelectBlocks()) return;

            Plan2Ext.Globs.DeleteFehlerLines(ID_NOT_UNIQUE);
            Dictionary<string, List<BlockReference>> blocksPerId = new Dictionary<string, List<BlockReference>>();
            List<object> insPoints = new List<object>();
            using (Transaction myT = _TransMan.StartTransaction())
            {
                foreach (var oid in _Blocks)
                {
                    BlockReference blockRef = _TransMan.GetObject(oid, OpenMode.ForRead) as BlockReference;
                    if (blockRef == null) continue;

                    AttributeReference idNummerAtt = null;
                    if (this._RnOptions.UseFirstAttrib)
                    {
                        idNummerAtt = GetFirstBlockAttribute(blockRef);
                    }
                    else
                    {
                        idNummerAtt = GetBlockAttribute(this._RnOptions.Attribname, blockRef);
                    }

                    if (idNummerAtt == null) continue;

                    string id = idNummerAtt.TextString.Trim().ToUpperInvariant();
                    if (string.IsNullOrEmpty(id)) continue;

                    List<BlockReference> blocks;
                    if (!blocksPerId.TryGetValue(id, out blocks))
                    {
                        blocks = new List<BlockReference>();
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

            Color col = Color.FromRgb((byte)255, (byte)10, (byte)40);
            Plan2Ext.Globs.InsertFehlerLines(insPoints, layerName: ID_NOT_UNIQUE, length: 50, ang: Math.PI * 1.25, col: col);
        }
        #endregion

        #region Private
        private enum SelectBlockReturn
        {
            terminate,
            entSelected,
            kwEnd,
        }
        private SelectBlockReturn SelectBlock()
        {
            _CurrentBlock = ObjectId.Null;
            Editor ed = _AcAp.Application.DocumentManager.MdiActiveDocument.Editor;
            PromptEntityOptions entopts = new PromptEntityOptions("\nBlock wählen: ");
            entopts.SetRejectMessage(string.Format(CultureInfo.CurrentCulture, _NoRaumblockMessage, _RnOptions.Blockname));
            entopts.AddAllowedClass(typeof(BlockReference), true);

            if (_LastBlock != ObjectId.Null)
            {
                entopts.Keywords.Add("Ende");
                var ds = entopts.Keywords.GetDisplayString(showNoDefault: true);
            }

            PromptEntityResult ent = null;
            SelectBlockReturn ret = SelectBlockReturn.terminate;
            var ok = false;
            while (!ok)
            {
                try
                {
                    ent = ed.GetEntity(entopts);
                }
                catch
                {
                    ed.WriteMessage("\nKein gültiges Elemente gewählt!");
                }

                if (ent.Status == PromptStatus.Keyword)
                {
                    ok = true;
                    ret = SelectBlockReturn.kwEnd;
                }
                else if (ent.Status != PromptStatus.OK)
                {
                    ok = true;
                    ret = SelectBlockReturn.terminate;
                }
                else if (_RnOptions.UseFirstAttrib)
                {
                    //
                    _CurrentBlock = ent.ObjectId;
                    ok = true;
                    ret = SelectBlockReturn.entSelected;
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
                            ret = SelectBlockReturn.entSelected;
                        }

                        myT.Commit();
                    }
                }
            }
            return ret;
        }

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
                        _LastAttName = attRef.Tag;
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

        private void AppendEToBlockAttrib(ObjectId oid, string attName)
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
                        attRef.TextString = attRef.TextString + "E";
                    }
                    else
                    {
                        throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Attribut '{0}' nicht gefunden!", attName));
                    }
                }

                myT.Commit();
            }
        }

        private void SetBlockAttrib(ObjectId oid, string attName, string val)
        {
            if (oid == ObjectId.Null) return;
            _LastAttName = attName;

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

        private string Increment(string number)
        {
            int len = number.Length;
            int i = int.Parse(number);
            i++;
            string s = i.ToString();
            return s.PadLeft(len, '0');
        }

        private bool SelectBlocks()
        {
            _Blocks.Clear();

            string hkBlockName = _RnOptions.Blockname;
            var ed = _AcAp.Application.DocumentManager.MdiActiveDocument.Editor;
            SelectionFilter filter = new SelectionFilter(new TypedValue[] { 
                new TypedValue((int)DxfCode.Start,"INSERT" ),
                //new _AcDb.TypedValue((int)_AcDb.DxfCode.BlockName,hkBlockName) // doesn't work with dynamic blocks
            });

            PromptSelectionResult res = ed.GetSelection(filter);
            if (res.Status != PromptStatus.OK) return false;
#if BRX_APP
            _AcEd.SelectionSet ss = res.Value;
#else
            using (SelectionSet ss = res.Value)
#endif
            {
                var allSelectedIds = ss.GetObjectIds().ToList();
                using (var myTrans = _TransMan.StartTransaction())
                {
                    foreach (var oid in allSelectedIds)
                    {
                        var bt = _TransMan.GetObject(oid, OpenMode.ForRead) as BlockReference;
                        if (string.Compare(hkBlockName, Plan2Ext.Globs.GetBlockname(bt, myTrans), StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            _Blocks.Add(oid);
                        }
                    }
                    myTrans.Commit();
                }
                if (_Blocks.Count > 0) return true;
                else return false;
            }
        }
        #endregion
    }
}
