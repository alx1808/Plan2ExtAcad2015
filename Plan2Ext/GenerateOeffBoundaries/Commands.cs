using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using log4net;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Exception = System.Exception;
// ReSharper disable StringLiteralTypo
// ReSharper disable IdentifierTypo
// ReSharper disable CommentTypo

namespace Plan2Ext.GenerateOeffBoundaries
{
    // ReSharper disable once UnusedMember.Global
    public class Commands
    {
        #region log4net Initialization
        private static readonly ILog Log = LogManager.GetLogger(Convert.ToString((typeof(Commands))));
        #endregion

        private const double ZOOM_WIDTH = 10.0;
        private string _targetLayer = "NewLayer";
        private bool _asHatch;
        private Document _document;
        private Database _db;
        private IConfigurationHandler _configurationHandler;


        [CommandMethod("Plan2GenerateOeffBoundaries")]
        // ReSharper disable once UnusedMember.Global
        public void Plan2GenerateOeffBoundaries()
        {
            Log.Info("Plan2GenerateOeffBoundaries");
            try
            {
                _configurationHandler = new ConfigurationHandler();
                _document = Application.DocumentManager.MdiActiveDocument;
                _db = _document.Database;

                if (string.IsNullOrEmpty(_configurationHandler.FensterSchraffLayer))
                {
                    var msg = "Solid-Schraffurlayer für Fenster ist nicht konfiguriert.";
                    _document.Editor.WriteMessage("\n" + msg);
                    LogInfo(msg);
                    return;
                }
                if (string.IsNullOrEmpty(_configurationHandler.TuerSchraffLayer))
                {
                    var msg = "Solid-Schraffurlayer für Türen ist nicht konfiguriert.";
                    _document.Editor.WriteMessage("\n" + msg);
                    LogInfo(msg);
                    return;
                }

                if (!AskUserHatchOrPolyline()) return;

                var entitySearcher = new EntitySearcher(_configurationHandler);
                var blockInfos = entitySearcher.GetInsertPointsInMs().ToArray();
                if (!blockInfos.Any())
                {
                    LogInfo("\nEs wurden kein Öffnungsblöcke gefunden.");
                    return;
                }

                var fensterBlockInfos = blockInfos.Where(x => x.Type == BlockInfo.BlockType.Fenster).Select(x => x.InsertPoint);
                _targetLayer = _configurationHandler.TuerSchraffLayer;
                Globs.CreateLayer(_targetLayer);
                Globs.SetLayerCurrent(_targetLayer);
                foreach (var point3D in fensterBlockInfos)
                {
                    CreateBoundary(point3D);
                }

                var tuerBlockInfos = blockInfos.Where(x => x.Type == BlockInfo.BlockType.Tuer).Select(x => x.InsertPoint);
                _targetLayer = _configurationHandler.FensterSchraffLayer;
                Globs.CreateLayer(_targetLayer);
                Globs.SetLayerCurrent(_targetLayer);
                foreach (var point3D in tuerBlockInfos)
                {
                    CreateBoundary(point3D);
                }
            }
            catch (OperationCanceledException)
            {
                Log.Info("Abbruch durch Benutzer.");
            }
            catch (Exception ex)
            {
                var msg = string.Format(CultureInfo.CurrentCulture,
                    "Fehler in Plan2GenerateOeffBoundaries aufgetreten! {0}", ex.Message);
                Log.Error(msg);
                Application.ShowAlertDialog(msg);
            }
        }

        private bool AskUserHatchOrPolyline()
        {
            var pko =
                new PromptKeywordOptions(
                    "\nErzeugung als Polylinie/<Schraffur>: "
                ) { AllowNone = true };
            pko.Keywords.Add("Polylinie");
            pko.Keywords.Add("Schraffur");
            pko.Keywords.Default = "Schraffur";
            var pkr = _document.Editor.GetKeywords(pko);
            if (pkr.Status != PromptStatus.OK)
                return false;
            _asHatch = string.CompareOrdinal(pkr.StringResult, "Schraffur") == 0;
            return true;
        }

        private void CreateBoundary(Point3d pointWcs)
        {
            var pointUcs = Globs.TransWcsUcs(pointWcs);
            ZoomToPoint(pointUcs);
            CreateBoundaryForPoint(pointUcs, pointWcs);
        }

        private void CreateBoundaryForPoint(Point3d pointUcs, Point3d pointWcs)
        {
            var lastOid = EditorHelper.Entlast();
            _document.Editor.Command("_.bpoly", pointUcs, "");
            //Application.DocumentManager.MdiActiveDocument.Editor.Command("_.bhatch", "_P", "_SOLID", pointUcs, "");
            var polylineObjectId = EditorHelper.Entlast();
            var newEntityCreated = (lastOid != polylineObjectId);
            if (!newEntityCreated)
            {
                Globs.InsertFehlerLines(new List<Point3d> { pointWcs }, "_Keine_Umgrenzung_gefunden");
                return;
            }

            if (HatchExistsAt(polylineObjectId))
            {
                Globs.InsertFehlerLines(new List<Point3d> { pointWcs }, "_Schraffur_existiert_bereits");
                Globs.DeleteEnttityWithOid(polylineObjectId);
                return;
            }

            if (_asHatch)
            {
                ObjectId hatchOid;
                using (var transaction = _document.TransactionManager.StartTransaction())
                {
                    var blockTable = (BlockTable)transaction.GetObject(_db.BlockTableId, OpenMode.ForRead);
                    var blockTableRecord = (BlockTableRecord)transaction.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
                    var hatch = Globs.CreateHatch(new List<ObjectId> { polylineObjectId }, null, _targetLayer, blockTableRecord, transaction);
                    hatchOid = hatch.ObjectId;
                    transaction.Commit();
                }
                Globs.DrawOrderTop(new List<ObjectId> { hatchOid });
                Globs.DeleteEnttityWithOid(polylineObjectId);
            }
        }
        private bool HatchExistsAt(ObjectId polylineObjectId)
        {
            var point3DCollectionUcs = new Point3dCollection();
            using (var transaction = _document.TransactionManager.StartTransaction())
            {
                var polyline = (Polyline)transaction.GetObject(polylineObjectId, OpenMode.ForRead);
                for (var i = 0; i < polyline.NumberOfVertices; i++)
                {
                    point3DCollectionUcs.Add(Globs.TransWcsUcs(polyline.GetPoint3dAt(i)));
                }
                transaction.Commit();
            }

            var filter = new SelectionFilter(new[]
            {
                new TypedValue((int)DxfCode.Operator ,"<AND"),
                new TypedValue((int) DxfCode.Start, "HATCH"),
                new TypedValue((int)DxfCode.LayerName  , _targetLayer  ),
                new TypedValue((int)DxfCode.Operator ,"AND>"),
            });
            var promptSelectionResult = _document.Editor.SelectCrossingPolygon(point3DCollectionUcs, filter);
            if (promptSelectionResult.Status != PromptStatus.OK) return false;
            using (var ss = promptSelectionResult.Value)
            {
                if (ss != null && ss.Count > 0) return true;
            }
            return false;
        }

        private void ZoomToPoint(Point3d point3D)
        {
            var ed = _document.Editor;
            var view = ed.GetCurrentView();
            view.CenterPoint = new Point2d(point3D.X, point3D.Y);
            view.Height = ZOOM_WIDTH;
            view.Width = ZOOM_WIDTH;
            ed.SetCurrentView(view);
        }

        private void LogInfo(string msg)
        {
            _document.Editor.WriteMessage("\n" + msg);
        }
    }
}
