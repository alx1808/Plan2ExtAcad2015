using System;
using System.Globalization;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
// ReSharper disable IdentifierTypo
// ReSharper disable StringLiteralTypo

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
            var innen = attributes.FirstOrDefault(x =>
                string.Compare(x.Tag, configurationHandler.FenInnenAttName, StringComparison.OrdinalIgnoreCase) == 0);
            if (innen == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Fenster mit Handle {0} hat kein Attribut {1}!", blockReference.Handle.ToString(), configurationHandler.FenInnenAttName));
            }
            var aussen = attributes.FirstOrDefault(x =>
                string.Compare(x.Tag, configurationHandler.FenAussenAttName, StringComparison.OrdinalIgnoreCase) == 0);
            if (aussen== null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Fenster mit Handle {0} hat kein Attribut {1}!", blockReference.Handle.ToString(), configurationHandler.FenAussenAttName));
            }
            Innen = innen.Position;
            Aussen = aussen.Position;
        }

        public Point3d Innen { get; private set; }
        public Point3d Aussen { get; private set; }
    }
}
