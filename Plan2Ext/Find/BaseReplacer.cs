using System.Text.RegularExpressions;

namespace Plan2Ext.Find
{
    class BaseReplacer
    {
        protected string ReplaceEscaped(string txt, string searchText, string replaceText)
        {
            var pattern = Regex.Escape(searchText);
            return Regex.Replace(txt, pattern, replaceText, RegexOptions.IgnoreCase);
        }
    }
}
