using System;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
// ReSharper disable IdentifierTypo

namespace Plan2Ext.AutoIdVergabeOeff
{
    internal interface IEntityFilter
    {
        bool IsFensterBlock(DBObject dbObject);
        bool IsTuerBlock(DBObject dbObject);
    }

    internal class EntityFilter : IEntityFilter
    {
        private readonly IConfigurationHandler _configurationHandler;

        public EntityFilter(IConfigurationHandler configurationHandler)
        {
            _configurationHandler = configurationHandler;
        }

        public bool IsFensterBlock(DBObject dbObject)
        {
            var blockReference = dbObject as BlockReference;
            if (blockReference == null) return false;
            var fenBlockName = _configurationHandler.ConfiguredFensterBlockNames.FirstOrDefault(x =>
                string.Compare(x, blockReference.Name, StringComparison.OrdinalIgnoreCase) == 0);
            return fenBlockName != null;
        }
        public bool IsTuerBlock(DBObject dbObject)
        {
            var blockReference = dbObject as BlockReference;
            if (blockReference == null) return false;
            var tuerBlockName = _configurationHandler.ConfiguredTuerBlockNames.FirstOrDefault(x =>
                string.Compare(x, blockReference.Name, StringComparison.OrdinalIgnoreCase) == 0);
            return tuerBlockName != null;
        }

    }
}
