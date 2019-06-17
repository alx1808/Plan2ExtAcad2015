using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
// ReSharper disable IdentifierTypo

namespace Plan2Ext.AutoIdVergabeOeff
{
    internal interface ITuerInfo
    {
        ObjectId Oid { get; set; }
        Point3d AttAussenInsertPoint { get; set; }
        ObjectId RaumblockId { get; set; }
        string Handle { get; set; }
        Point3d AttInnenInsertPoint { get; set; }
        Point3d InsertPoint { get; set; }
    }

    internal class TuerInfo : ITuerInfo
    {
        public ObjectId Oid { get; set; }
        public Point3d AttAussenInsertPoint { get; set; }
        public Point3d AttInnenInsertPoint { get; set; }
        public Point3d InsertPoint { get; set; }
        public ObjectId RaumblockId { get; set; }
        public string Handle { get; set; }
    }
}
