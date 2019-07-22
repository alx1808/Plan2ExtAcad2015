using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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


        private const string NOT_UNIQUE_ID_LAYER = "_IdNichtEindeutig";

        private static IPalette _Palette;
        private class DocumentInfo
        {
            private int _fenNr = 1;
            private int _tuerNr = 1;
            public int FenNr
            {
                get { return _fenNr; }
                set { _fenNr = value; }
            }

            public int TuerNr
            {
                get { return _tuerNr; }
                set { _tuerNr = value; }
            }
        }

        private static readonly Dictionary<Document, DocumentInfo> DocumentInfoPerDocument = new Dictionary<Document, DocumentInfo>();


        static Commands()
        {

            InitDocEvents();
        }

        private static void InitDocEvents()
        {
            try
            {
                var dc = Application.DocumentManager;
                dc.DocumentToBeDeactivated += dc_DocumentToBeDeactivated;
                dc.DocumentActivated += dc_DocumentActivated;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
            }
        }

        private static void dc_DocumentToBeDeactivated(object sender, DocumentCollectionEventArgs e)
        {
            if (_Palette == null) return;
            var documentInfo = GetDocumentInfo(e);
            documentInfo.FenNr = _Palette.FenNr;
            documentInfo.TuerNr = _Palette.TuerNr;
        }

        private static void dc_DocumentActivated(object sender, DocumentCollectionEventArgs e)
        {
            if (_Palette == null) return;
            var documentInfo = GetDocumentInfo(e);
            _Palette.FenNr = documentInfo.FenNr;
            _Palette.TuerNr = documentInfo.TuerNr;
        }

        private static DocumentInfo GetDocumentInfo(DocumentCollectionEventArgs e)
        {
            DocumentInfo documentInfo;
            if (!DocumentInfoPerDocument.TryGetValue(e.Document, out documentInfo))
            {
                documentInfo = new DocumentInfo();
                DocumentInfoPerDocument.Add(e.Document, documentInfo);
            }

            return documentInfo;
        }


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
                    var entityFilter = new EntityFilter(configurationHandler);
                    var entitySelector = new EntitySelector(configurationHandler, entityFilter);
                    var selectedObjectsIds = entitySelector.SelectObjectsIds();
                    if (selectedObjectsIds == null) return;
                    var entitySearcher = new EntitySearcher(configurationHandler, entityFilter);

                    if (_Palette.KindOfStart == KindOfStartEnum.Alle || _Palette.KindOfStart == KindOfStartEnum.Fenster)
                    {
                        var fensterInfos = entitySearcher.GetFensterInfosInMs(selectedObjectsIds.FensterIds,
                            selectedObjectsIds.ObjectPolygonId);
                        var fenSorter = new FenSorter(configurationHandler, _Palette);
                        fenSorter.Sort(fensterInfos, selectedObjectsIds.ObjectPolygonId);
                    }

                    if (_Palette.KindOfStart == KindOfStartEnum.Alle || _Palette.KindOfStart == KindOfStartEnum.Tueren)
                    {
                        var tuerInfos = entitySearcher.GetTuerInfosInMs(selectedObjectsIds.RaumBlockIds,
                            selectedObjectsIds.FlaGrenzIds, selectedObjectsIds.TuerIds,
                            selectedObjectsIds.ObjectPolygonId);
                        var tuerSorter = new TuerSorter(configurationHandler, _Palette, new ComparerRaumNummern("-"));
                        tuerSorter.Sort(tuerInfos);
                    }
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

        [CommandMethod("Plan2AutoIdVergabeOeffnungenEindeutigkeit")]
        // ReSharper disable once UnusedMember.Global
        public static void Plan2AutoIdVergabeOeffnungenEindeutigkeit()
        {
            try
            {

                if (!OpenRnPalette()) return;

                Globs.DeleteFehlerLines(NOT_UNIQUE_ID_LAYER);
                Document doc = Application.DocumentManager.MdiActiveDocument;
                using (doc.LockDocument())
                {
                    var configurationHandler = new ConfigurationHandler();
                    var entityFilter = new EntityFilter(configurationHandler);
                    var entitySearcher = new EntitySearcher(configurationHandler, entityFilter);
                    var uniqueCheckInfos = entitySearcher.GetUniqueCheckInfosInMs().ToArray();

                    var uniqueCheckInfosFenster =
                        uniqueCheckInfos.Where(x => x.Kind == UniqueCheckInfo.KindEnum.Fenster);
                    var grouped = uniqueCheckInfosFenster.GroupBy(x => x.Id);
                    foreach (var group in grouped)
                    {
                        if (group.Count() > 1)
                        {
                            Globs.InsertFehlerLines(group.Select(x => x.InsertPoint).ToList(), NOT_UNIQUE_ID_LAYER);
                        }
                    }
                    
                    var uniqueCheckInfosTuer =
                        uniqueCheckInfos.Where(x => x.Kind == UniqueCheckInfo.KindEnum.Tuer);
                    grouped = uniqueCheckInfosTuer.GroupBy(x => x.Id);
                    foreach (var group in grouped)
                    {
                        if (group.Count() > 1)
                        {
                            Globs.InsertFehlerLines(group.Select(x => x.InsertPoint).ToList(), NOT_UNIQUE_ID_LAYER);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2AutoIdVergabeOeffnungenEindeutigkeit aufgetreten! {0}", ex.Message));
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
