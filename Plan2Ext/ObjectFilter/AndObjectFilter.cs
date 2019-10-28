using System.Collections.Generic;
using System.Linq;
#if BRX_APP
using Teigha.DatabaseServices;
#elif ARX_APP
using Autodesk.AutoCAD.DatabaseServices;
#endif

namespace Plan2Ext.ObjectFilter
{
    internal class AndObjectFilter : IObjectFilter
    {
        private readonly IObjectFilter[] _objectFilters;
        public AndObjectFilter(IEnumerable<IObjectFilter> objectFilters)
        {
            _objectFilters = objectFilters.ToArray();
        }

        public bool Matches(DBObject dbObject, Transaction transaction)
        {
            return _objectFilters.All(x => x.Matches(dbObject,transaction));
        }
    }
}
