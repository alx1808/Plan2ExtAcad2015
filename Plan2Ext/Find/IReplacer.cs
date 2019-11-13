#if BRX_APP
using Teigha.DatabaseServices;
#elif ARX_APP
using Autodesk.AutoCAD.DatabaseServices;

#endif

namespace Plan2Ext.Find
{
    interface IReplacer
    {
        bool SetEntityIfApplicable(DBObject dbo);
        void Replace(string searchText, string replaceText);
    }
}
