using System;
using System.Collections.Generic;

// ReSharper disable IdentifierTypo

namespace Plan2Ext.AutoIdVergabeOeff
{
    internal class ComparerRaumNummern : IComparer<string>
    {
        private readonly string _separator;

        public ComparerRaumNummern(string separator)
        {
            _separator = separator;
        }
        public int Compare(string x, string y)
        {
            if (x == null || y == null) throw new InvalidOperationException("X or Y is null!");
            var arrX = x.Split(new[] { _separator }, StringSplitOptions.None);
            var arrY = y.Split(new[] { _separator }, StringSplitOptions.None);

            var maxl = Math.Min(arrX.Length, arrY.Length);
            for (var i = 0; i < maxl; i++)
            {
                var valX = arrX[i];
                var valY = arrY[i];
                int intValX, intValY;

                var comp = 0;
                if (int.TryParse(valX, out intValX) && int.TryParse(valY, out intValY))
                {
                    comp = intValX.CompareTo(intValY);
                }
                else
                {
                    comp = string.Compare(valX, valY, StringComparison.Ordinal);
                }
                if (comp == 0) continue;
                return comp;
            }

            return 0;
        }
    }
}
