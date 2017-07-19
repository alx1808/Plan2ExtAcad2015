//using Autodesk.AutoCAD.ApplicationServices;
//using Autodesk.AutoCAD.DatabaseServices;
//using Autodesk.AutoCAD.EditorInput;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//#if BRX_APP
//using Bricscad.ApplicationServices;
//using _AcDb = Teigha.DatabaseServices;

//using Bricscad.EditorInput;
//#elif ARX_APP
//  using Autodesk.AutoCAD.ApplicationServices;
//  using _AcDb = Autodesk.AutoCAD.DatabaseServices;
//  using Autodesk.AutoCAD.EditorInput;
//#endif

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


namespace Plan2Ext.CalcArea
{
    internal class Engine
    {
        #region log4net Initialization
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Convert.ToString((typeof(Engine))));
        #endregion

        #region Member
        private _AcDb.TransactionManager _TransMan = null;
        public string RaumBlockName { get; set; }
        public string AreaAttName { get; set; }
        public string HeightAttName { get; set; }
        public string VolAttName { get; set; }
        #endregion

        public Engine()
        {
            _TransMan = _AcAp.Application.DocumentManager.MdiActiveDocument.Database.TransactionManager;
        }

        internal void CalcVolume(string raumBlockName, string areaAttName, string heightAttName, string volAttName)
        {
            RaumBlockName = raumBlockName;
            AreaAttName = areaAttName;
            HeightAttName = heightAttName;
            VolAttName = volAttName;

            if (!(CalcVolArgsOk)) return;

            List<_AcDb.ObjectId> allRaumBlocks = SelectRaumBlocks();
            if (allRaumBlocks.Count == 0) return;

            using (_AcDb.Transaction myT = _TransMan.StartTransaction())
            {
                foreach (var oid in allRaumBlocks)
                {
                    _AcDb.BlockReference blockRef = _TransMan.GetObject(oid, _AcDb.OpenMode.ForRead) as _AcDb.BlockReference;
                    if (blockRef == null) continue;

                    CalcVolume(blockRef);
                }

                myT.Commit();
            }


        }

        private void CalcVolume(_AcDb.BlockReference blockRef)
        {
            var attHeight = GetBlockAttribute(HeightAttName, blockRef);
            var attArea = GetBlockAttribute(AreaAttName, blockRef);
            var attVol = GetBlockAttribute(VolAttName, blockRef);

            double height;
            if (!GetHeightFromString(attHeight.TextString, out height)) return;

            double area;
            if (!GetAreaFromString(attArea.TextString, out area)) return;

            double vol = height * area;
            attVol.UpgradeOpen();
            attVol.TextString = string.Format(CultureInfo.CurrentCulture, "Vol={0:F2}m3", vol);

        }

        private bool GetHeightFromString(string txt, out double height)
        {
            height = 0.0;

            if (string.IsNullOrEmpty(txt)) return false;
            txt = txt.Trim();
            if (txt.Length == 0) return false;

            var parts = txt.Split(new char[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 0) return false;

            double num1;
            if (!GetFirstDoubleInText(parts[0], out num1)) return false;

            if (parts.Length == 1)
            {
                height = num1;
                return true;
            }

            double num2;
            if (!GetFirstDoubleInText(parts[1], out num2))
            {
                height = num1;
            }
            else
            {
                height = (num1 + num2) / 2.0;
            }

            return true;
        }

        private bool GetAreaFromString(string txt, out double area)
        {
            area = 0.0;

            if (string.IsNullOrEmpty(txt)) return false;
            txt = txt.Trim();
            if (txt.Length == 0) return false;

            return GetFirstDoubleInText(txt, out area);

        }

        private bool GetFirstDoubleInText(string txt, out double dblVal)
        {
            bool inNr = false;
            bool comma = false;
            StringBuilder sb = new StringBuilder();
            foreach (var c in txt.ToArray())
            {
                if (!inNr)
                {
                    // not in nr
                    if (IsNumeric(c))
                    {
                        sb.Append(c);
                        inNr = true;
                    }
                }
                else
                {
                    // in nr
                    if (IsNumeric(c))
                    {
                        sb.Append(c);
                    }
                    else if (c == '.' || c == ',')
                    {
                        if (comma) break; // second comma -> break
                        sb.Append('.');
                        comma = true;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            string sNr = sb.ToString();
            if (!double.TryParse(sNr, NumberStyles.Any, CultureInfo.InvariantCulture, out dblVal)) return false;
            else return true;
        }

        private bool IsNumeric(char c)
        {
            return c >= '0' && c <= '9';
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



        private List<_AcDb.ObjectId> SelectRaumBlocks()
        {
            string hkBlockName = RaumBlockName;
            var ed = _AcAp.Application.DocumentManager.MdiActiveDocument.Editor;
            _AcEd.SelectionFilter filter = new _AcEd.SelectionFilter(new _AcDb.TypedValue[] { 
                new _AcDb.TypedValue((int)_AcDb.DxfCode.Start,"INSERT" ),
                //new _AcDb.TypedValue((int)_AcDb.DxfCode.BlockName,hkBlockName)
            });

            _AcEd.PromptSelectionResult res = ed.GetSelection(filter);
            if (res.Status != _AcEd.PromptStatus.OK) return new List<_AcDb.ObjectId>();

#if BRX_APP
            SelectionSet ss = res.Value;
#else
            using (_AcEd.SelectionSet ss = res.Value)
#endif

            {
                List<_AcDb.ObjectId> theBlockOids = new List<_AcDb.ObjectId>();

                _AcAp.Document doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
                using (_AcDb.Transaction myT = doc.TransactionManager.StartTransaction())
                {

                    var lstBlocks = ss.GetObjectIds();
                    foreach (var oid in lstBlocks)
                    {
                        var br = myT.GetObject(oid, _AcDb.OpenMode.ForRead) as _AcDb.BlockReference;
                        if (br != null && string.Compare(Plan2Ext.Globs.GetBlockname(br, myT), hkBlockName, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            theBlockOids.Add(oid);
                        }
                    }
                    myT.Commit();
                }
                return theBlockOids;
                //return ss.GetObjectIds().ToList();
            }
        }

        private bool CalcVolArgsOk
        {
            get
            {
                return (!string.IsNullOrEmpty(RaumBlockName) &&
                    !string.IsNullOrEmpty(AreaAttName) &&
                    !string.IsNullOrEmpty(HeightAttName) &&
                    !string.IsNullOrEmpty(VolAttName));
            }
        }
    }
}
