#if BRX_APP
using _AcAp = Bricscad.ApplicationServices;
using Teigha.DatabaseServices;
#elif ARX_APP
using Autodesk.AutoCAD.DatabaseServices;

#endif

namespace Plan2Ext.Find
{
    class AttributeReferenceReplacer : BaseReplacer, IReplacer
    {
        private AttributeReference _current;
        public bool SetEntityIfApplicable(DBObject dbo)
        {
            _current = dbo as AttributeReference;
            return _current != null;
        }

        public void Replace(string searchText, string replaceText)
        {
            _current.TextString = ReplaceEscaped(_current.TextString, searchText, replaceText);
        }
    }
}
