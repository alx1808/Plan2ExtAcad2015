using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
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
        private const short FENSTER_LAYER_COLOR = 141;
        private const short TUER_LAYER_COLOR = 42;
        private const string NO_BOUNDARY_ERROR_LAYER_NAME = "_Keine_Umgrenzung_gefunden";
        private const string HATCH_EXISTS_ERROR_LAYER_NAME = "_Schraffur_existiert_bereits";
        private const string BLOCK_IP_ERROR_LAYER_NAME = "_Block_Innerhalb_RaumPolylinie";

        private string _targetLayer = "NewLayer";
        private bool _asHatch;
        private Document _document;
        private Database _db;
        private IConfigurationHandler _configurationHandler;
        private readonly List<ObjectId> _generatedPolylines = new List<ObjectId>();
        private readonly List<Point3d> _errorLinePositionsNoBoundary = new List<Point3d>();
        private readonly List<Point3d> _errorLinePositionsHatchExists = new List<Point3d>();
        private readonly List<Point3d> _errorLinePositionsBlockInInnerPolyline = new List<Point3d>();

        [CommandMethod("Plan2GenerateOeffBoundaries")]
        // ReSharper disable once UnusedMember.Global
        public void Plan2GenerateOeffBoundaries()
        {
            Log.Info("Plan2GenerateOeffBoundaries");
            try
            {
                _generatedPolylines.Clear();
                _errorLinePositionsHatchExists.Clear();
                _errorLinePositionsNoBoundary.Clear();
                _errorLinePositionsBlockInInnerPolyline.Clear();
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

                CheckBlockInsertPointInsideInternalPolyline(blockInfos, entitySearcher.GetInternalPolylineOidsInMs().ToArray());

                GeneratePolylinesFromHatches(entitySearcher.GetNonOeffHatchesInMs().ToArray());

                var fensterBlockInfos = blockInfos.Where(x => x.Type == BlockInfo.BlockType.Fenster)
                    .Select(x => x.InsertPoint);
                ActivateFensterTargetLayer();
                foreach (var point3D in fensterBlockInfos)
                {
                    CreateBoundary(point3D);
                }

                var tuerBlockInfos = blockInfos.Where(x => x.Type == BlockInfo.BlockType.Tuer)
                    .Select(x => x.InsertPoint);
                ActivateTuerTargetLayer();
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
            finally
            {
                InsertFehlerLines();
                DeleteGeneratedPolylines();
            }
        }

        private void CheckBlockInsertPointInsideInternalPolyline(IBlockInfo[] blockInfos, ObjectId[] polylineObjectIds)
        {
            using (var transaction = _document.TransactionManager.StartTransaction())
            {
                if (polylineObjectIds.Length == 0) return;
                foreach (var point3D in blockInfos.Select(x => x.InsertPoint))
                {
                    foreach (var polylineObjectId in polylineObjectIds)
                    {
                        var polyline = (Entity)transaction.GetObject(polylineObjectId, OpenMode.ForRead);
                        if (AreaEngine.InPoly(point3D, polyline))
                        {
                            _errorLinePositionsBlockInInnerPolyline.Add(point3D);
                        }
                    }
                }
                transaction.Commit();
            }
        }

        private void InsertFehlerLines()
        {
            Globs.InsertFehlerLines(_errorLinePositionsNoBoundary, NO_BOUNDARY_ERROR_LAYER_NAME);
            Globs.InsertFehlerLines(_errorLinePositionsHatchExists, HATCH_EXISTS_ERROR_LAYER_NAME);
            Globs.InsertFehlerLines(_errorLinePositionsBlockInInnerPolyline, BLOCK_IP_ERROR_LAYER_NAME);
        }

        private void ActivateTuerTargetLayer()
        {
            _targetLayer = _configurationHandler.TuerSchraffLayer;
            ActivateTargetLayer(TUER_LAYER_COLOR);
        }

        private void ActivateFensterTargetLayer()
        {
            _targetLayer = _configurationHandler.FensterSchraffLayer;
            ActivateTargetLayer(FENSTER_LAYER_COLOR);
        }

        private void ActivateTargetLayer(short laycol)
        {
            Globs.CreateLayer(_targetLayer, Color.FromColorIndex(ColorMethod.ByAci, laycol), false);
            Globs.LayerOnAndThaw(_targetLayer, unlock: true);
            Globs.SetLayerCurrent(_targetLayer);
        }

        private void DeleteGeneratedPolylines()
        {
            if (_generatedPolylines.Count == 0) return;
            using (var transaction = _document.TransactionManager.StartTransaction())
            {
                foreach (var generatedPolyline in _generatedPolylines)
                {
                    var polyline = (Entity)transaction.GetObject(generatedPolyline, OpenMode.ForWrite);
                    polyline.Erase(true);
                }
                transaction.Commit();
            }
        }

        private void GeneratePolylinesFromHatches(ObjectId[] hatches)
        {
            foreach (var hatchOid in hatches)
            {
                var lastOid = EditorHelper.Entlast();
                _document.Editor.Command("_.hatchedit", hatchOid, "_B", "_P", "_N");
                var polylineObjectId = EditorHelper.Entlast();
                var newEntityCreated = (lastOid != polylineObjectId);
                if (!newEntityCreated)
                {
                    LogWarning(string.Format(CultureInfo.CurrentCulture, "Konnte keine Polylinie für Schraffur '{0}' erzeugen!", hatchOid.Handle.ToString()));
                }
                else
                {
                    _generatedPolylines.Add(polylineObjectId);
                    SetPolylineLayerToHatchLayer(polylineObjectId, hatchOid);
                }
            }
        }

        private void SetPolylineLayerToHatchLayer(ObjectId polylineObjectId, ObjectId hatchOid)
        {
            using (var transaction = _document.TransactionManager.StartTransaction())
            {
                var hatch = (Entity)transaction.GetObject(hatchOid, OpenMode.ForRead);
                var polyline = (Entity)transaction.GetObject(polylineObjectId, OpenMode.ForWrite);
                polyline.Layer = hatch.Layer;
                transaction.Commit();
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
                _errorLinePositionsNoBoundary.Add(pointWcs);
                return;
            }

            if (HatchExistsAt(polylineObjectId))
            {
                _errorLinePositionsHatchExists.Add(pointWcs);
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
            Log.Info(msg);
            _document.Editor.WriteMessage("\n" + msg);
        }
        private void LogWarning(string msg)
        {
            Log.Warn(msg);
            _document.Editor.WriteMessage("\nWarnung: " + msg);
        }
    }
}
