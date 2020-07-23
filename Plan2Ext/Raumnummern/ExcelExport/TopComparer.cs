using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Plan2Ext.Raumnummern.ExcelExport
{
    internal class TopComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            if (x.Equals(y)) return 0;
            var xNum = GetFirstInt(x);
            var yNum = GetFirstInt(y);
            if (xNum == yNum) return String.CompareOrdinal(x, y);
            return xNum - yNum;
        }

        private int GetFirstInt(string s)
        {
            var m = Regex.Match(s, "[0-9]+");
            if (!m.Success) return 9999;

            return int.Parse(m.Value);
        }
    }
}
