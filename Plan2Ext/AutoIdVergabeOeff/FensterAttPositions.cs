using System;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
// ReSharper disable IdentifierTypo

namespace Plan2Ext.AutoIdVergabeOeff
{
    internal interface IFensterAttPositions
    {
        Point3d Innen { get; }
        Point3d Aussen { get; }
    }

    internal class FensterAttPositions : IFensterAttPositions
    {
        public FensterAttPositions(BlockReference blockReference, Transaction transaction,
            IConfigurationHandler configurationHandler)
        {
            var attributes = Globs.GetAttributEntities(blockReference, transaction);
            var innen = attributes.First(x =>
                string.Compare(x.Tag, configurationHandler.FenInnenAttName, StringComparison.OrdinalIgnoreCase) == 0);
            var aussen = attributes.First(x =>
                string.Compare(x.Tag, configurationHandler.FenAussenAttName, StringComparison.OrdinalIgnoreCase) == 0);
            Innen = innen.Position;
            Aussen = aussen.Position;
        }

        public Point3d Innen { get; private set; }
        public Point3d Aussen { get; private set; }
    }
}
