#if BRX_APP
using  Bricscad.ApplicationServices;
#elif ARX_APP
using Autodesk.AutoCAD.DatabaseServices;
#endif

namespace Plan2Ext.ObjectFilter
{
    interface IObjectFilter
    {
        bool Matches(DBObject dbObject, Transaction transaction);
    }
}
