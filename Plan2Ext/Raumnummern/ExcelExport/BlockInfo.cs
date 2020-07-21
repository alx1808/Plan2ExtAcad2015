using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
#if BRX_APP
using Bricscad.ApplicationServices;
using Teigha.DatabaseServices;
using Bricscad.EditorInput;
using Bricscad.Runtime;
using Teigha.Runtime;
using Bricscad.Internal;

#elif ARX_APP
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Internal;
#endif

namespace Plan2Ext.Raumnummern.ExcelExport
{
    internal class BlockInfo
    {
        public BlockInfo(string topnr, string zimmer, string area, string separator)
        {
            Top = ToTop(topnr, separator);
            Topnr = topnr;
            Zimmer = zimmer;
            Area = ToArea(area);
        }

        private string ToTop(string topnr, string separator)
        {
            var index = topnr.LastIndexOf(separator, StringComparison.CurrentCultureIgnoreCase);
            
            if (index < 0)
            {
                throw new InvalidOperationException($"\"{topnr}\" ist keine gültige Topnummer.");
            }
            return topnr.Remove(index);
        }

        private double ToArea(string areaOrig)
        {
            var area = areaOrig;
            try
            {
                area = area.Trim();
                if (area.EndsWith("m2", StringComparison.CurrentCultureIgnoreCase))
                {
                    area = area.Remove(area.Length - 2);
                }

                area = area.Trim();
                area.Replace(',', '.');
                return double.Parse(area, NumberStyles.Any, CultureInfo.InvariantCulture);

            }
            catch (System.Exception e)
            {
                throw new InvalidOperationException($"Konnte Fläche {areaOrig} in Raum {Topnr} nicht als Zahl interpretieren.");
            }
        }

        public string Top { get; set; }
        public string Topnr { get; set; }
        public string Zimmer { get; set; }
        public double Area { get; set; }
    }
}
