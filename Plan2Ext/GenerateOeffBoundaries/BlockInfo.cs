using Autodesk.AutoCAD.Geometry;

// ReSharper disable IdentifierTypo

namespace Plan2Ext.GenerateOeffBoundaries
{
    internal interface IBlockInfo
    {
        BlockInfo.BlockType Type { get; }
        Point3d InsertPoint { get; }
    }

    internal class BlockInfo : IBlockInfo
    {
        public enum BlockType
        {
            Fenster,
            Tuer
        }

        public BlockType Type { get; set; }
        public Point3d InsertPoint { get; set; }
    }
}
