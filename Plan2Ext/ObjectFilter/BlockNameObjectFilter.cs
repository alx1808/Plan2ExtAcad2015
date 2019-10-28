#if BRX_APP
using Teigha.DatabaseServices;
#elif ARX_APP
using Autodesk.AutoCAD.DatabaseServices;
#endif

namespace Plan2Ext.ObjectFilter
{
    internal class BlockNameObjectFilter : IObjectFilter
    {
        private readonly WildcardAcad _wildcard;

        public BlockNameObjectFilter(WildcardAcad wildcard)
        {
            _wildcard = wildcard;
        }

        public bool Matches(DBObject dbObject, Transaction transaction)
        {
            if (dbObject == null) return false;
            var blockReference = dbObject as BlockReference;
            if (blockReference == null) return false;
            var blockName = Globs.GetBlockname(blockReference, transaction);
            return _wildcard.IsMatch(blockName);
        }
    }
}
