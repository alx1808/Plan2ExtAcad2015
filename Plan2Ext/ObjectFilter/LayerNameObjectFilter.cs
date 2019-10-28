#if BRX_APP
using Teigha.DatabaseServices;
#elif ARX_APP
using Autodesk.AutoCAD.DatabaseServices;
#endif

namespace Plan2Ext.ObjectFilter
{
    internal class LayerNameObjectFilter : IObjectFilter
    {
        private readonly WildcardAcad _wildcard;

        public LayerNameObjectFilter(WildcardAcad wildcard)
        {
            _wildcard = wildcard;
        }
        public bool Matches(DBObject dbObject, Transaction transaction)
        {
            if (dbObject == null) return false;
            var entity = dbObject as Entity;
            return entity != null && _wildcard.IsMatch(entity.Layer);
        }
    }
}
