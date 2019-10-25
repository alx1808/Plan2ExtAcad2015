using System;
#if BRX_APP
using  Bricscad.ApplicationServices;
#elif ARX_APP
using Autodesk.AutoCAD.DatabaseServices;
#endif

namespace Plan2Ext.ObjectFilter
{
    internal class TypeObjectFilter : IObjectFilter
    {
        private readonly Type _type;

        public TypeObjectFilter(Type type)
        {
            _type = type;
        }
        public bool Matches(DBObject dbObject, Transaction transaction)
        {
            if (dbObject == null) return false;
            return (dbObject.GetType() == _type);
        }
    }
}
