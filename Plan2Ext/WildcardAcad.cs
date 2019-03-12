using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
// ReSharper disable IdentifierTypo

namespace Plan2Ext
{
    internal class WildcardAcad : Regex
    {
        public WildcardAcad(string pattern)
            : base(WildcardToRegex(pattern), RegexOptions.IgnoreCase)
        {
            
        }

        private static string WildcardToRegex(string pattern)
        {

            var patterns = pattern.Split(',').Select(x => x.Trim()).ToList();
            return WildcardToRegex(patterns);
        }

        private static string WildcardToRegex(List<string> patterns)
        {
            var innerPatterns = new List<string>();
            foreach (string pattern in patterns)
            {
                innerPatterns.Add("(^" + Escape(pattern).Replace("\\*", ".*").Replace("\\?", ".") + "$)");
            }

            var innerPattern = string.Join("|", innerPatterns.ToArray());

            return innerPattern;
        }
    }
}
