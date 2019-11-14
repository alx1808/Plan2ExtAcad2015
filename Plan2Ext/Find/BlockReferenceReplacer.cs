#if BRX_APP
using Teigha.DatabaseServices;
#elif ARX_APP
using Autodesk.AutoCAD.DatabaseServices;
#endif
using System.Linq;

namespace Plan2Ext.Find
{
    class BlockReferenceReplacer : BaseReplacer, IReplacer
    {
        private BlockReference _current;
        private readonly AttributeReferenceReplacer _attributeReferenceReplacer;

        public BlockReferenceReplacer(AttributeReferenceReplacer attributeReferenceReplacer)
        {
            _attributeReferenceReplacer = attributeReferenceReplacer;
        }

        public bool SetEntityIfApplicable(DBObject dbo)
        {
            _current = dbo as BlockReference;
            return _current != null;
        }

        public void Replace(string searchText, string replaceText)
        {
            var attributeReferences = _current.AttributeReferences().ToArray();
            if (!attributeReferences.Any()) return;
            foreach (var attributeReference in attributeReferences)
            {
                attributeReference.UpgradeOpen();
                if (_attributeReferenceReplacer.SetEntityIfApplicable(attributeReference)) _attributeReferenceReplacer.Replace(searchText,replaceText);
                attributeReference.DowngradeOpen();
            }
        }
    }
}
