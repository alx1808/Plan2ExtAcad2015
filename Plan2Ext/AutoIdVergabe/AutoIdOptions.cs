//using Autodesk.AutoCAD.ApplicationServices;
//using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
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
    public class AutoIdOptions
    {

        public AutoIdControl Form { get; set; }

        private int _RaumnummerAbStelle = 1;
        public int RaumnummerAbStelle
        {
            get { return _RaumnummerAbStelle; }
            set
            {
                if (_RaumnummerAbStelle != value && (value > 0))
                {
                    _RaumnummerAbStelle = value;
                }
            }
        }

        private int _RaumnummerBisStelle = -1;
        public int RaumnummerBisStelle
        {
            get { return _RaumnummerBisStelle; }
            set
            {
                if (_RaumnummerBisStelle != value)
                {
                    _RaumnummerBisStelle = value;
                }
            }
        }

        private string _Blockname = string.Empty;
        public string Blockname
        {
            get { return _Blockname; }
            set
            {
                _Blockname = value;
            }
        }
        public void SetBlockname(string blockName)
        {
            Form.txtBlockname.Text = blockName;
            _Blockname = blockName;
        }

        private string _AttTuerschildnummer = string.Empty;
        public string AttTuerschildnummer
        {
            get { return _AttTuerschildnummer; }
            set
            {
                _AttTuerschildnummer = value;
            }
        }
        public void SetTuerschildAtt(string attName)
        {
            Form.txtTuerschildnummer.Text = attName;
            _AttTuerschildnummer = attName;
        }

        private string _AttIdNummer = string.Empty;
        public string AttIdNummer
        {
            get { return _AttIdNummer; }
            set
            {
                _AttIdNummer = value;
            }
        }
        public void SetIdAtt(string attName)
        {
            Form.txtIdNummer.Text = attName;
            _AttIdNummer = attName;
        }

        private string _Liegenschaft = "000000";
        public string Liegenschaft
        {
            get { return _Liegenschaft; }
            set
            {
                _Liegenschaft = value;
            }
        }

        private string _Objekt = "000";
        public string Objekt
        {
            get { return _Objekt; }
            set
            {
                _Objekt = value;
            }
        }

        private string _Geschoss = "000";
        public string Geschoss
        {
            get { return _Geschoss; }
            set
            {
                _Geschoss = value;
            }
        }

        private string _Arial = "000";
        public string Arial
        {
            get { return _Arial; }
            set
            {
                _Arial = value;
            }
        }

        // Zu-Raum-ID-Vergabe
        private string _PolygonLayer = string.Empty;
        public string PolygonLayer
        {
            get { return _PolygonLayer; }
            set
            {
                _PolygonLayer = value;
            }
        }
        public void SetPolygonLayer(string layer)
        {
            Form.txtPolygonLayer.Text = layer;
            _PolygonLayer = layer;
        }

        //private string _ZuRaumIdAtt = string.Empty;
        //public string ZuRaumIdAtt
        //{
        //    get { return _ZuRaumIdAtt; }
        //    set
        //    {
        //        _ZuRaumIdAtt = value;
        //    }
        //}
        //public void SetZuRaumIdAtt(string att)
        //{
        //    Form.txtZuRaumIdAtt.Text = att;
        //    _ZuRaumIdAtt = att;
        //}

        private List<ZuweisungsInfo> _Zuweisungen = new List<ZuweisungsInfo>();
        internal List<ZuweisungsInfo> Zuweisungen
        {
            get
            {
                return _Zuweisungen;
            }
        }

        internal List<ZuweisungsInfo> GetAssignmentsDict()
        {
            List<ZuweisungsInfo> zuweisungen = new List<ZuweisungsInfo>();

            var doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            if (doc == null) return zuweisungen;

            var db = doc.Database;
            using (_AcDb.Transaction trans = db.TransactionManager.StartTransaction())
            {
                try
                {
                    // Find the NOD in the database

                    _AcDb.ObjectId myDataId = default(_AcDb.ObjectId);
                    using (_AcDb.DBDictionary nod = (_AcDb.DBDictionary)trans.GetObject(
                                db.NamedObjectsDictionaryId, _AcDb.OpenMode.ForRead))
                    {

                        foreach (var subnode in nod)
                        {
                            if (subnode.Key == "AutoIdAssignments")
                            {
                                myDataId = subnode.Value;
                                break;
                            }
                        }
                    }

                    if (myDataId != default(_AcDb.ObjectId))
                    {

                        //ObjectId myDataId = nod.GetAt("AutoIdAssignments");
                        var readBack = trans.GetObject(myDataId, _AcDb.OpenMode.ForRead);
                        var readBack2 = readBack as _AcDb.Xrecord;
                        if (readBack2 != null)
                        {
                            List<string> rbs = new List<string>();
                            foreach (_AcDb.TypedValue value in readBack2.Data)
                                rbs.Add(value.Value.ToString());

                            for (int i = 0; i < rbs.Count; i += 2)
                            {
                                zuweisungen.Add(new ZuweisungsInfo() { FromAtt = rbs[i], ToAtt = rbs[i + 1] });
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    // todo: log error
                }
                finally
                {
                    
                    trans.Commit();
                }

            }

            if (zuweisungen.Count > 0)
            {
                // zuweisungen erhalten, wenn es noch keine gibt
                _Zuweisungen.Clear();
                _Zuweisungen.AddRange(zuweisungen);
            }

            return _Zuweisungen ;

        }


        private const int RTNORM = 5100;
        [Obsolete()]
        internal List<ZuweisungsInfo> GetAssignmentsDictOld()
        {
            List<ZuweisungsInfo> zwInfos = new List<ZuweisungsInfo>();
            try
            {
                //using (var rb = new ResultBuffer(new TypedValue((int)Autodesk.AutoCAD.Runtime.LispDataType.Text, "c:Plan2AutoIdVergabeSetAssignments")))
                using (var rb = new _AcDb.ResultBuffer(new _AcDb.TypedValue((int)_AcBrx.LispDataType.Text, "c:GetPlan2AutoIdVergabeAssignmentsDict")))
                {
                    int stat = 0;
                    _AcDb.ResultBuffer res = CADDZone.AutoCAD.Samples.AcedInvokeSample.InvokeLisp(rb, ref stat);
                    if (stat == RTNORM && res != null)
                    {

                        var arr = res.AsArray();
                        if (arr != null && arr.Length > 1)
                        {
                            int pos = 0;

                            while (pos < arr.Length)
                            {
                                // list begin
                                var rbFrom = arr[pos + 1];
                                var rbTo = arr[pos + 2];
                                // dotted pair

                                zwInfos.Add(new ZuweisungsInfo() { FromAtt = rbFrom.Value.ToString(), ToAtt = rbTo.Value.ToString() });

                                pos += 4;
                            }
                        }
                        res.Dispose();
                    }
                }
            }
            catch (Exception)
            {
                // todo: log error
            }



            return zwInfos;
        }


    }
}
