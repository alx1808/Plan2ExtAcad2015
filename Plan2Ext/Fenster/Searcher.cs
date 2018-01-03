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
using _AcLm = Autodesk.AutoCAD.LayerManager;
using System.Globalization;
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plan2Ext.Fenster
{
    internal class RotatedDimensionInfo : ObjectSearcher
    {
        public _AcGe.Point3d MiddlePoint { get; set; }
        public _AcGe.Point3d StartPoint { get; set; }
        public _AcGe.Point3d EndPoint { get; set; }
        public double Rotation { get; set; }

        public override ObjectSearcher GetObject(_AcDb.DBObject dbo, _AcDb.Transaction tr)
        {
            _AcDb.RotatedDimension dim = dbo as _AcDb.RotatedDimension;
            if (dim != null)
            {
                var linSeg = new _AcGe.LineSegment3d(dim.XLine1Point, dim.XLine2Point);
                return new RotatedDimensionInfo() { Oid = dbo.ObjectId, MiddlePoint = linSeg.MidPoint, StartPoint = linSeg.StartPoint, EndPoint = linSeg.EndPoint, Rotation = dim.Rotation};
            }
            return null;
        }
    }

    internal class FensterBlockInfo : ObjectSearcher
    {
        private static List<string> _FensterBlockNames = new List<string>()
        {
            "FENSTERBLOCK_UNTEN",
            "FENSTERBLOCK_OBEN",
            "FENSTERBLOCK_LINKS",
            "FENSTERBLOCK_RECHTS",
        };
        public static List<string> FensterBlockNames
        {
            get { return FensterBlockInfo._FensterBlockNames; }
            set { FensterBlockInfo._FensterBlockNames = value; }
        }

        public override ObjectSearcher GetObject(_AcDb.DBObject dbo, _AcDb.Transaction tr)
        {
            _AcDb.BlockReference br = dbo as _AcDb.BlockReference;
            if (br != null)
            {
                var blockName = Plan2Ext.Globs.GetBlockname(br, tr);
                if (_FensterBlockNames.Contains(blockName.ToUpperInvariant()))
                {
                    return new FensterBlockInfo() { Oid = dbo.ObjectId, InsertPoint = br.Position };
                }
            }
            return null;
        }
        public _AcGe.Point3d InsertPoint { get; set; }
    }

    internal class FensterLineInfo : ObjectSearcher
    {
        public _AcGe.Point3d MiddlePoint { get; set; }
        public _AcGe.Point3d StartPoint { get; set; }
        public _AcGe.Point3d EndPoint { get; set; }

        private static string _Layer = "A_OF_TXT";
        public static string Layer
        {
            get { return FensterLineInfo._Layer; }
            set { FensterLineInfo._Layer = value; }
        }
        public override ObjectSearcher GetObject(_AcDb.DBObject dbo, _AcDb.Transaction tr)
        {
            _AcDb.Line line = dbo as _AcDb.Line;
            if (line == null) return null;
            if (line.Layer != FensterLineInfo.Layer) return null;
            var linSeg = new _AcGe.LineSegment3d(line.StartPoint, line.EndPoint);
            return new FensterLineInfo() { Oid = dbo.ObjectId, MiddlePoint = linSeg.MidPoint, StartPoint = linSeg.StartPoint, EndPoint = linSeg.EndPoint };
        }
    }
    internal class SturzParaLineInfo : ObjectSearcher
    {
        public _AcGe.Point3d MiddlePoint { get; set; }
        public _AcGe.Point3d StartPoint { get; set; }
        public _AcGe.Point3d EndPoint { get; set; }

        private static List<string> _Layers = new List<string>() { "A_OF_STURZ", "A_OF_PARA" };
        public static List<string> Layers
        {
            get { return SturzParaLineInfo._Layers; }
            set { SturzParaLineInfo._Layers = value; }
        }
        public override ObjectSearcher GetObject(_AcDb.DBObject dbo, _AcDb.Transaction tr)
        {
            _AcDb.Line line = dbo as _AcDb.Line;
            if (line == null) return null;
            if (!SturzParaLineInfo.Layers.Contains(line.Layer)) return null;
            var linSeg = new _AcGe.LineSegment3d(line.StartPoint, line.EndPoint);
            return new SturzParaLineInfo() { Oid = dbo.ObjectId, MiddlePoint = linSeg.MidPoint, StartPoint = linSeg.StartPoint, EndPoint = linSeg.EndPoint};
        }
    }
}
