using System.Text.RegularExpressions;

namespace Plan2Ext.Find
{
    class BaseReplacer
    {
        protected bool UseRegex;
        protected string ReplaceEscaped(string txt, string searchText, string replaceText)
        {
            var pattern = UseRegex ? searchText : Regex.Escape(searchText);
            return Regex.Replace(txt, pattern, replaceText, RegexOptions.IgnoreCase);
        }
    }
}
