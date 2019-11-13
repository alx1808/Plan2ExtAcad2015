#if BRX_APP
using _AcAp = Bricscad.ApplicationServices;
using Teigha.DatabaseServices;
#elif ARX_APP
using Autodesk.AutoCAD.DatabaseServices;

#endif

namespace Plan2Ext.Find
{
    class MTextReplacer : BaseReplacer, IReplacer
    {
        private MText _current;
        public bool SetEntityIfApplicable(DBObject dbo)
        {
            _current = dbo as MText;
            return _current != null;
        }

        public void Replace(string searchText, string replaceText)
        {
            _current.Contents = ReplaceEscaped(_current.Contents, searchText, replaceText);
        }
    }
}
