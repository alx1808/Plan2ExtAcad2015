using Autodesk.AutoCAD.Geometry;
// ReSharper disable IdentifierTypo

namespace Plan2Ext.AutoIdVergabeOeff
{
    internal interface IUniqueCheckInfo
    {
        string Id { get; }
        Point3d InsertPoint { get; }
    }

    internal class UniqueCheckInfo : IUniqueCheckInfo
    {
        public string Id { get; set; }
        public Point3d InsertPoint { get; set; }
    }
}
