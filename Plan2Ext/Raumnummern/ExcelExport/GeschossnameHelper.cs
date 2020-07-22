using System.Collections.Generic;

namespace Plan2Ext.Raumnummern.ExcelExport
{
    internal interface IGeschossnameHelper
    {
        string Langbez(string kurzbez);
        IEnumerable<string> GetKeywords();
    }

    internal class GeschossnameHelper : IGeschossnameHelper
    {
        private Dictionary<string, string> _langPerKurz = new Dictionary<string, string>()
        {
            { "Kg", "KELLERGESCHOSS" },
            { "Eg", "ERDGESCHOSS" },
            { "1.OG", "1.OBERGESCHOSS" },
            { "2.OG", "2.OBERGESCHOSS" },
            { "3.OG", "3.OBERGESCHOSS" },
            { "4.OG", "4.OBERGESCHOSS" },
            { "5.OG", "5.OBERGESCHOSS" },
            { "6.OG", "6.OBERGESCHOSS" },
            { "7.OG", "7.OBERGESCHOSS" },
            { "8.OG", "8.OBERGESCHOSS" },
            { "9.OG", "9.OBERGESCHOSS" },
            { "Dg", "DACHGESCHOSS" },
        };

        public string Langbez(string kurzbez)
        {
            if (_langPerKurz.TryGetValue(kurzbez, out var langbez))
            {
                return langbez;
            }

            return kurzbez;
        }

        public IEnumerable<string> GetKeywords()
        {
            return _langPerKurz.Keys;
        }
    }
}
