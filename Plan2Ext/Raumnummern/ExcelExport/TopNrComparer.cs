using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Plan2Ext.Raumnummern.ExcelExport
{
    internal class TopNrComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            if (x.Equals(y)) return 0;

            GetStringAndNr(x, out var xStr, out var xInt);
            GetStringAndNr(y, out var yStr, out var yInt);

            if (xStr.Equals(yStr))
            {
                return xInt - yInt;
            }

            return string.CompareOrdinal(xStr, yStr);
        }

        private void GetStringAndNr(string s, out string str, out int num)
        {
            if (string.IsNullOrEmpty(s))
            {
                str = string.Empty;
                num = 9999;
            }
            var index = s.LastIndexOf('/');
            if (index < 0)
            {
                str = s;
                num = 9999;
                return;
            }

            if (index == 0)
            {
                str = string.Empty;
                num = GetFirstInt(s.Substring(index + 1));
                return;
            }

            str = s.Substring(0, index);
            num = GetFirstInt(s.Substring(index + 1));

        }

        private int GetFirstInt(string s)
        {
            var m = Regex.Match(s, "[0-9]+");
            if (!m.Success) return 9999;

            return int.Parse(m.Value);
        }

    }
}
