#if BRX_APP
using Teigha.DatabaseServices;
#elif ARX_APP
using Autodesk.AutoCAD.DatabaseServices;

#endif

namespace Plan2Ext.Find
{
    class DbTextReplacer : BaseReplacer, IReplacer
    {
        private DBText _current;
        public bool SetEntityIfApplicable(DBObject dbo)
        {
            _current = dbo as DBText;
            return _current != null;
        }

        public void Replace(string searchText, string replaceText)
        {
            _current.TextString = ReplaceEscaped(_current.TextString, searchText, replaceText);
        }
    }
}
