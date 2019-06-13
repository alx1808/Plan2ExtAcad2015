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
    public class Commands
    {
        #region log4net Initialization
        private static readonly ILog Log = LogManager.GetLogger(Convert.ToString((typeof(GenerateOeffBoundaries.Commands))));
        #endregion

        [CommandMethod("Plan2AutoIdVergabeOeffnungen")]
        // ReSharper disable once UnusedMember.Global
        public void Plan2AutoIdVergabeOeffnungen()
        {
            Log.Info("Plan2AutoIdVergabeOeffnungen");
            try
            {
                var configurationHandler = new ConfigurationHandler();
                var entitySelector = new EntitySelector(configurationHandler);
                var selectedObjectsIds = entitySelector.SelectObjectsIds();
                if (selectedObjectsIds == null) return;
                var entitySearcher = new EntitySearcher(configurationHandler);
                var fensterInfos = entitySearcher.GetFensterInfosInMs(selectedObjectsIds.FensterIds, selectedObjectsIds.ObjectPolygonId);
                var fenSorter = new FenSorter(configurationHandler);
                fenSorter.Sort(fensterInfos, selectedObjectsIds.ObjectPolygonId);
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

    }
}
