using System.Collections.Generic;
using System.Linq;
#if BRX_APP
using  Bricscad.ApplicationServices;
#elif ARX_APP
using Autodesk.AutoCAD.DatabaseServices;
#endif

namespace Plan2Ext.ObjectFilter
{
    internal class OrObjectFilter : IObjectFilter
    {
        private readonly IObjectFilter[] _objectFilters;
        public OrObjectFilter(IEnumerable<IObjectFilter> objectFilters)
        {
            _objectFilters = objectFilters.ToArray();
        }

        public bool Matches(DBObject dbObject, Transaction transaction)
        {
            return _objectFilters.Any(x => x.Matches(dbObject,transaction));
        }
    }
}
