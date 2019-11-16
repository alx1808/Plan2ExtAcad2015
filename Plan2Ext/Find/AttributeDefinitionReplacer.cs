#if BRX_APP
using Teigha.DatabaseServices;
#elif ARX_APP
using Autodesk.AutoCAD.DatabaseServices;

#endif

namespace Plan2Ext.Find
{
    class AttributeDefinitionReplacer : BaseReplacer, IReplacer
    {
        public AttributeDefinitionReplacer(bool useRegex)
        {
            UseRegex = useRegex;
        }
        private AttributeDefinition _current;
        public bool SetEntityIfApplicable(DBObject dbo)
        {
            _current = dbo as AttributeDefinition;
            return _current != null;
        }

        public void Replace(string searchText, string replaceText)
        {
            _current.Prompt = ReplaceEscaped(_current.Prompt, searchText, replaceText);
            _current.Tag = ReplaceEscaped(_current.Tag, searchText, replaceText);
        }
    }
}
