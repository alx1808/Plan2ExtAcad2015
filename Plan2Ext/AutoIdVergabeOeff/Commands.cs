using System;
using System.Globalization;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using log4net;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Exception = System.Exception;
// ReSharper disable IdentifierTypo
// ReSharper disable StringLiteralTypo

namespace Plan2Ext.AutoIdVergabeOeff
{
    // ReSharper disable once UnusedMember.Global
    public class Commands
    {
        #region log4net Initialization
        private static readonly ILog Log = LogManager.GetLogger(Convert.ToString((typeof(GenerateOeffBoundaries.Commands))));
        #endregion

        private static IPalette _Palette;

        [CommandMethod("Plan2AutoIdVergabeOeffnungen")]
        // ReSharper disable once UnusedMember.Global
        public void Plan2AutoIdVergabeOeffnungen()
        {
            if (!OpenRnPalette()) return;

            Log.Info("Plan2AutoIdVergabeOeffnungen");
            try
            {
                var doc = Application.DocumentManager.MdiActiveDocument;
                using (doc.LockDocument())
                {
                    var configurationHandler = new ConfigurationHandler();
                    var entitySelector = new EntitySelector(configurationHandler);
                    var selectedObjectsIds = entitySelector.SelectObjectsIds();
                    if (selectedObjectsIds == null) return;
                    var entitySearcher = new EntitySearcher(configurationHandler);
                    var fensterInfos = entitySearcher.GetFensterInfosInMs(selectedObjectsIds.FensterIds,
                        selectedObjectsIds.ObjectPolygonId);
                    var tuerInfos = entitySearcher.GetTuerInfosInMs(selectedObjectsIds.RaumBlockIds,selectedObjectsIds.FlaGrenzIds, selectedObjectsIds.TuerIds, selectedObjectsIds.ObjectPolygonId);
                    var fenSorter = new FenSorter(configurationHandler, _Palette);
                    fenSorter.Sort(fensterInfos, selectedObjectsIds.ObjectPolygonId);
                    var tuerSorter = new TuerSorter(configurationHandler, _Palette);
                    tuerSorter.Sort(tuerInfos);
                }
            }
            catch (OperationCanceledException)
            {
                Log.Info("Abbruch durch Benutzer.");
            }
            catch (Exception ex)
            {
                var msg = string.Format(CultureInfo.CurrentCulture,
                    "Fehler in Plan2AutoIdVergabeOeffnungen aufgetreten! {0}", ex.Message);
                Log.Error(msg);
                Application.ShowAlertDialog(msg);
            }
        }
        private static bool OpenRnPalette()
        {
            if (_Palette == null)
            {
                _Palette = new Palette();
            }

            bool wasOpen = _Palette.Show();
            if (!wasOpen) return false;

            Document doc = Application.DocumentManager.MdiActiveDocument;
            if (doc == null) return false;
            Log.DebugFormat("Dokumentname: {0}.", doc.Name);

            return true;

        }

    }
}
