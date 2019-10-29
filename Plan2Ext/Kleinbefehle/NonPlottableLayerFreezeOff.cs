using System.Collections.Generic;
using System.Globalization;
using System.Linq;
// ReSharper disable IdentifierTypo
// ReSharper disable StringLiteralTypo
// ReSharper disable CommentTypo

#if BRX_APP
using Teigha.Runtime;
using Bricscad.ApplicationServices;
#elif ARX_APP
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices.Core;
#endif


namespace Plan2Ext.Kleinbefehle
{
    public class NonPlottableLayerFreezeOff
    {

        private static readonly List<string> MatchCodes = new List<string>
        {
            "*_AL_MANS_*",
            "*MANSFEN*",
            "*AFEN*",
        };

        private static WildcardAcad[] _wildcards;

        [CommandMethod("Plan2NonPlottableLayerFreezeOff")]
        public static void Plan2NonPlottableLayerFreezeOff()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            var editor = doc.Editor;
            try
            {
                _wildcards = MatchCodes.Select(x => new WildcardAcad(x)).ToArray();
                var nonPlottableLayerNames = LayerManager.GetNamesOfNonPlottableLayers(db).Where(IsAllowed);
                if (LayerManager.FreezeOff(nonPlottableLayerNames, db))
                {
                    doc.Editor.Regen();
                }
            }
            catch (Exception ex)
            {
                string msg = string.Format(CultureInfo.CurrentCulture, "Fehler in (Plan2NonPlottableLayerFreezeOff): {0}", ex.Message);
                editor.WriteMessage("\n" + msg);
            }
        }

        /// <summary>
        /// Ignore layer names matching matchcodes
        /// </summary>
        /// <param name="layerName"></param>
        /// <returns></returns>
        private static bool IsAllowed(string layerName)
        {
            return !_wildcards.Any(x => x.IsMatch(layerName));
        }
    }
}
