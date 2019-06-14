using System;
using System.Globalization;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
// ReSharper disable IdentifierTypo
// ReSharper disable StringLiteralTypo

namespace Plan2Ext.AutoIdVergabeOeff
{
    internal interface ITuerAttPositions
    {
        Point3d Innen { get; }
        Point3d Aussen { get; }
    }

    internal class TuerAttPositions : ITuerAttPositions
    {
        public TuerAttPositions(BlockReference blockReference, Transaction transaction,
            IConfigurationHandler configurationHandler)
        {
            var attributes = Globs.GetAttributEntities(blockReference, transaction);
            var innen = attributes.FirstOrDefault(x =>
                string.Compare(x.Tag, configurationHandler.InnenAttName, StringComparison.OrdinalIgnoreCase) == 0);
            if (innen == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Tuer mit Handle {0} hat kein Attribut {1}!", blockReference.Handle.ToString(), configurationHandler.InnenAttName));
            }
            var aussen = attributes.FirstOrDefault(x =>
                string.Compare(x.Tag, configurationHandler.AussenAttName, StringComparison.OrdinalIgnoreCase) == 0);
            if (aussen == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Tuer mit Handle {0} hat kein Attribut {1}!", blockReference.Handle.ToString(), configurationHandler.AussenAttName));
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
