using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using Autodesk.AutoCAD.Runtime;
//using Autodesk.AutoCAD.ApplicationServices;
//using Autodesk.AutoCAD.DatabaseServices;
//using Autodesk.AutoCAD.EditorInput;


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


//[assembly: CommandClass(typeof(Plan2Ext.LayoutListSelected.Commands))]

namespace Plan2Ext.LayoutListSelected
{
    public class Commands
    {
        [_AcTrx.LispFunction("LayoutListSelected")]
        public _AcDb.ResultBuffer LayoutListSelected(_AcDb.ResultBuffer args)
        {
            if (args != null)
                throw new TooFewArgsException();

            _AcAp.Document doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            _AcDb.Database db = doc.Database;
            //Editor ed = doc.Editor;
            //LayoutManager layoutMgr = LayoutManager.Current;
            List<string> layouts = new List<string>();
            _AcDb.ResultBuffer res = new _AcDb.ResultBuffer();

            using (_AcDb.Transaction tr = db.TransactionManager.StartTransaction())
            {
                _AcDb.DBDictionary layoutDic =
                    (_AcDb.DBDictionary)tr.GetObject(db.LayoutDictionaryId, _AcDb.OpenMode.ForRead, openErased: false);

                foreach (_AcDb.DBDictionaryEntry entry in layoutDic)
                {
                    _AcDb.Layout layout =
                        (_AcDb.Layout)tr.GetObject(entry.Value, _AcDb.OpenMode.ForRead);

                    string layoutName = layout.LayoutName;

                    if (layout.TabSelected)
                        layouts.Add(layoutName);
                }

                tr.Commit();
            }

            layouts.Remove("Model");

            if (0 < layouts.Count)
            {
                layouts.Sort();

                foreach (string layoutName in layouts)
                {
                    res.Add(new _AcDb.TypedValue((int)(_AcBrx.LispDataType.Text), layoutName));
                }

                return res;
            }

            else
                return null;
        }
    }

    // Special thanks to Gile for his LispException classes:
    class LispException : System.Exception
    {
        public LispException(string msg) : base(msg) { }
    }

    class TooFewArgsException : LispException
    {
        public TooFewArgsException() : base("too few arguments") { }
    }

    class TooManyArgsException : LispException
    {
        public TooManyArgsException() : base("too many arguments") { }
    }

    class ArgumentTypeException : LispException
    {
        public ArgumentTypeException(string s, _AcDb.TypedValue tv)
            : base(string.Format(
           "invalid argument type: {0}: {1}",
           s, tv.TypeCode == (int)_AcBrx.LispDataType.Nil ? "nil" : tv.Value))
        { }
    }
}

