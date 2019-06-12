using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

// ReSharper disable IdentifierTypo

namespace Plan2Ext.AutoIdVergabeOeff
{
    internal interface IFensterInfo
    {
        FensterInfo.KindEnum Kind { get; set; }
        ObjectId Oid { get; set; }
        Point3d InsertPoint { get; }
    }

    internal class FensterInfo : IFensterInfo
    {
        public enum KindEnum
        {
            OnPolygon,
            InsidePolygon,
            OutsidePolygon
        }

        public KindEnum Kind { get; set; }
        public ObjectId Oid { get; set; }
        public Point3d InsertPoint { get; set; }
    }
}
