using System;
using System.Collections.Generic;
using System.Globalization;
#if BRX_APP
using Teigha.DatabaseServices;
using Teigha.Geometry;
#elif ARX_APP
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
#endif

namespace Plan2Ext.Raumnummern.ExcelExport
{
    internal class BlockInfo
    {
        private static readonly string[] FehlerLineLayers = new string[]
        {
            "_Ungültige_Topnummer",
            "_Ungültiger_Flächenwert",
            "_Fehlendes_Attribut"
        };

        public static void DeleteFehlerlines()
        {
            foreach (var layer in FehlerLineLayers)
            {
                Plan2Ext.Globs.DeleteFehlerLines(layer);
            }
        }

        public BlockInfo(string geschoss, BlockReference blockReference, Transaction transaction, RnOptions rnOptions)
        {
            var insertPoint = blockReference.Position;
            var attributes = GetAttributes(blockReference, transaction);
            if (attributes.TryGetValue(rnOptions.Attribname.ToUpper(), out var topNr) &&
                attributes.TryGetValue(rnOptions.ZimmerAttributeName.ToUpper(), out var zimmer) &&
                attributes.TryGetValue(rnOptions.FlaechenAttributName.ToUpper(), out var area))
            {
                Topnr = topNr;
                Zimmer = zimmer;
                Geschoss = geschoss;
                if (!ToTop(topNr, rnOptions.Separator))
                {
                    Plan2Ext.Globs.InsertFehlerLines(new List<Point3d>() { insertPoint }, FehlerLineLayers[0]);
                    return;
                }

                if (!ToArea(area))
                {
                    Plan2Ext.Globs.InsertFehlerLines(new List<Point3d>() { insertPoint }, FehlerLineLayers[1]);
                    return;
                }

                Ok = true;
            }
            else
            {
                Plan2Ext.Globs.InsertFehlerLines(new List<Point3d>() {insertPoint}, FehlerLineLayers[2]);
            }
        }

        private static Dictionary<string, string> GetAttributes(BlockReference blockRef, Transaction transaction)
        {
            var valuePerTag = new Dictionary<string, string>();

            foreach (ObjectId attId in blockRef.AttributeCollection)
            {
                if (attId.IsErased) continue;
                var anyAttRef = transaction.GetObject(attId, OpenMode.ForRead) as AttributeReference;
                if (anyAttRef != null)
                {
                    valuePerTag[anyAttRef.Tag.ToUpper()] = anyAttRef.TextString;
                }
            }

            return valuePerTag;
        }

        private bool ToTop(string topnr, string separator)
        {
            var index = topnr.LastIndexOf(separator, StringComparison.CurrentCultureIgnoreCase);
            
            if (index < 0)
            {
                return false;
            }
            Top = topnr.Remove(index);
            return true;
        }

        private bool ToArea(string areaOrig)
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
                var replace = area.Replace(',', '.');
                Area = double.Parse(replace, NumberStyles.Any, CultureInfo.InvariantCulture);
                return true;

            }
            catch (Exception)
            {
                return false;
            }
        }

        public string Geschoss { get; private set; }
        public string Top { get; private set; }
        public string Topnr { get; private set; }
        public string Zimmer { get; private set; }
        public double Area { get; private set; }
        public bool Ok { get; private set; }
    }
}
