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
            
            Innen = GetCenterPosition(innen);
            Aussen = GetCenterPosition(aussen);
        }

        private static Point3d GetCenterPosition(AttributeReference innen)
        {
            return innen.Bounds.HasValue ? GetCenter(innen.Bounds) : innen.Position;
        }

        private static Point3d GetCenter(Extents3d? bounds)
        {
            // ReSharper disable once PossibleInvalidOperationException
            return new Point3d((bounds.Value.MinPoint.X + bounds.Value.MaxPoint.X) / 2.0,
                (bounds.Value.MinPoint.Y + bounds.Value.MaxPoint.Y) / 2.0, 0.0);
        }

        public Point3d Innen { get; private set; }
        public Point3d Aussen { get; private set; }
    }
}
