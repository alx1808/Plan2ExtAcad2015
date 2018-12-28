using System.Globalization;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
// ReSharper disable IdentifierTypo
// ReSharper disable StringLiteralTypo

namespace Plan2Ext.LayerNummerierung
{
    // ReSharper disable once UnusedMember.Global
    public class Commands
    {
        #region log4net Initialization
        // ReSharper disable once UnusedMember.Local
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Convert.ToString((typeof(Commands))));
        #endregion

        
        [CommandMethod("Plan2LayerNummerierung")]
        // ReSharper disable once UnusedMember.Global
        public static void Plan2LayerNummerierung()
        {
            try
            {
                if (!OpenNrPalette()) return;

                var opts = Globs.TheNrOptions;
                Document doc = Application.DocumentManager.MdiActiveDocument;

                using (doc.LockDocument())
                {
                    var engine = new Engine(opts);
                    while (engine.AddNumber()) { }
                }
            }
            catch (System.Exception ex)
            {
                Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2LayerNummerierung aufgetreten! {0}", ex.Message));
            }
        }

        static NrPalette _nrPalette;

        private static bool OpenNrPalette()
        {
            if (_nrPalette == null)
            {
                _nrPalette = new NrPalette();
            }

            bool wasOpen = _nrPalette.Show();
            if (!wasOpen) return false;

            Document doc = Application.DocumentManager.MdiActiveDocument;
            if (doc == null) return false;

            if (Globs.TheNrOptions == null) return false;
            return true;
        }
    }
}
