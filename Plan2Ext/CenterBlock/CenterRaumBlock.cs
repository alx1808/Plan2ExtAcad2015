using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

#if BRX_APP
using _AcAp = Bricscad.ApplicationServices;
//using _AcBr = Teigha.BoundaryRepresentation;
using _AcCm = Teigha.Colors;
using _AcDb = Teigha.DatabaseServices;
using _AcEd = Bricscad.EditorInput;
using _AcGe = Teigha.Geometry;
using _AcGi = Teigha.GraphicsInterface;
using _AcGs = Teigha.GraphicsSystem;
using _AcPl = Bricscad.PlottingServices;
using _AcBrx = Bricscad.Runtime;
using _AcTrx = Teigha.Runtime;
using _AcWnd = Bricscad.Windows;
using _AcInt = BricscadApp;
using _AcIntCom = BricscadDb;
#elif ARX_APP
using _AcAp = Autodesk.AutoCAD.ApplicationServices;
using _AcBr = Autodesk.AutoCAD.BoundaryRepresentation;
using _AcCm = Autodesk.AutoCAD.Colors;
using _AcDb = Autodesk.AutoCAD.DatabaseServices;
using _AcEd = Autodesk.AutoCAD.EditorInput;
using _AcGe = Autodesk.AutoCAD.Geometry;
using _AcGi = Autodesk.AutoCAD.GraphicsInterface;
using _AcGs = Autodesk.AutoCAD.GraphicsSystem;
using _AcPl = Autodesk.AutoCAD.PlottingServices;
using _AcBrx = Autodesk.AutoCAD.Runtime;
using _AcTrx = Autodesk.AutoCAD.Runtime;
using _AcWnd = Autodesk.AutoCAD.Windows;
using _AcInt = Autodesk.AutoCAD.Interop;
using _AcIntCom = Autodesk.AutoCAD.Interop.Common;
#endif

namespace Plan2Ext.CenterBlock
{
    internal class CenterRaumBlock
    {
        #region log4net Initialization
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Convert.ToString((typeof(CenterRaumBlock))));
        #endregion

        #region Constants
        private const double ABSTANDTEXT = 2.0;
        private const int AcHatchObject = 0;
        private const int AcPatternType = 0;

        #endregion

        #region Member variables

        private _AcDb.TransactionManager _TransMan = null;
        private string _RaumblockName = "";
        private string _FgLayer = "";
        private List<_AcDb.ObjectId> _FlaechenGrenzen = new List<_AcDb.ObjectId>();
        private List<_AcDb.ObjectId> _Raumbloecke = new List<_AcDb.ObjectId>();

        #endregion

        #region Lifecycle
        public CenterRaumBlock()
        {
            _TransMan = _AcAp.Application.DocumentManager.MdiActiveDocument.Database.TransactionManager;
        }
        #endregion

        #region Internal
        internal void DoIt(_AcAp.Document doc, string rbName, string fgLayer, bool useXRefs)
        {
            log.Debug("--------------------------");

            if (string.IsNullOrEmpty(rbName)) return;
            if (string.IsNullOrEmpty(fgLayer)) return;


            _FlaechenGrenzen.Clear();
            _Raumbloecke.Clear();

            var invalidNrRbLayerName = "Blockanzahl_ungleich_1";
            var centroidOutsideLayerName = "Centroid_liegt_außerhalb.";

            DeleteFehlerSymbols(new List<string>() { invalidNrRbLayerName, centroidOutsideLayerName });

            var _AreaEngine = new AreaEngine();

            _AcGe.Matrix3d ucs = _AcGe.Matrix3d.Identity;
            try
            {
                ucs = doc.Editor.CurrentUserCoordinateSystem;
                doc.Editor.CurrentUserCoordinateSystem = _AcGe.Matrix3d.Identity;

                if (useXRefs) ExplodeAllXrefs();

                _RaumblockName = rbName;
                _FgLayer = fgLayer;

                LayerOnAndThaw(fgLayer);

                _AreaEngine.SelectFgAndRb(_FlaechenGrenzen, _Raumbloecke, _FgLayer, _RaumblockName);

                if (_FlaechenGrenzen.Count == 0) return;

                ZoomToFlaechenGrenzen();

                // init div
                int fehlerKeinRb = 0;
                int fehlerMehrRb = 0;
                int centroidOutside = 0;

                _AcDb.Database db = doc.Database;
                _AcEd.Editor ed = doc.Editor;
                _AcDb.TransactionManager tm = db.TransactionManager;
                _AcDb.Transaction myT = tm.StartTransaction();
                try
                {
                    _AcGe.Point2d lu = new _AcGe.Point2d();
                    _AcGe.Point2d ro = new _AcGe.Point2d();

                    for (int i = 0; i < _FlaechenGrenzen.Count; i++)
                    {
                        log.Debug("--------------------------");

                        double sumAF = 0;
                        int rbInd = -1;
                        _AcDb.ObjectId elFG = _FlaechenGrenzen[i];
                        log.DebugFormat("Flächengrenze {0}", elFG.Handle.ToString());

                        _AcDb.Extents3d ext = GetExtents(tm, elFG);
                        _AcGe.Point3d minExt = new _AcGe.Point3d(ext.MinPoint.X - ABSTANDTEXT, ext.MinPoint.Y - ABSTANDTEXT, ext.MinPoint.Z);
                        _AcGe.Point3d maxExt = new _AcGe.Point3d(ext.MaxPoint.X + ABSTANDTEXT, ext.MaxPoint.Y + ABSTANDTEXT, ext.MaxPoint.Z);

                        List<_AcDb.ObjectId> rbsToIgnoreCol = GetFgAnz(minExt, maxExt, elFG);
                        if (rbsToIgnoreCol.Count > 0)
                        {
                            string handles = string.Join(",", rbsToIgnoreCol.Select(x => x.Handle.ToString()).ToArray());
                            log.DebugFormat("Zu ignorierende Raumblöcke: {0}", handles);
                        }

                        //    'raumbloecke holen
                        List<_AcDb.ObjectId> ssRB = selRB(minExt, maxExt);
                        if (ssRB.Count > 0)
                        {
                            string handles = string.Join(",", ssRB.Select(x => x.Handle.ToString()).ToArray());
                            log.DebugFormat("Raumblöcke: {0}", handles);

                        }


                        int rbAnz = 0;
                        //    'raumbloecke pruefen
                        for (int rbCnt = 0; rbCnt < ssRB.Count; rbCnt++)
                        {
                            _AcDb.ObjectId rbBlock2 = ssRB[rbCnt];

                            //      ' ignore rbs
                            _AcDb.ObjectId found = rbsToIgnoreCol.FirstOrDefault(x => x.Equals(rbBlock2));
                            if (found != default(_AcDb.ObjectId)) continue;

                            using (_AcDb.DBObject dbObj = tm.GetObject(rbBlock2, _AcDb.OpenMode.ForRead, false))
                            {
                                _AcGe.Point3d rbEp = ((_AcDb.BlockReference)dbObj).Position;

                                using (_AcDb.Entity elFGEnt = (_AcDb.Entity)tm.GetObject(elFG, _AcDb.OpenMode.ForRead, false))
                                {
                                    if (AreaEngine.InPoly(rbEp, elFGEnt))
                                    {
                                        log.DebugFormat("Raumblock {0} ist innerhalb der Flächengrenze.", rbBlock2.Handle.ToString());

                                        if (_Raumbloecke.Contains(rbBlock2)) _Raumbloecke.Remove(rbBlock2);
                                        rbAnz++;
                                        rbInd = rbCnt;
                                    }
                                    else
                                    {
                                        log.DebugFormat("Außen liegender Raumblock {0} wird ignoriert.", rbBlock2.Handle.ToString());
                                    }

                                }
                            }
                        }


                        if (rbAnz < 1)
                        {
                            log.WarnFormat("Kein Raumblock in Flächengrenze {0}!", elFG.Handle.ToString());
                            FehlerLineOrHatchPoly(elFG, invalidNrRbLayerName, 255, 0, 0, tm, Plan2Ext.Globs.GetCentroid(elFG));
                            fehlerKeinRb++;

                        }
                        else if (rbAnz > 1)
                        {
                            log.WarnFormat("Mehr als ein Raumblock in Flächengrenze {0}!", elFG.Handle.ToString());
                            FehlerLineOrHatchPoly(elFG, invalidNrRbLayerName, 0, 255, 0, tm, Plan2Ext.Globs.GetCentroid(elFG));
                            fehlerMehrRb++;

                        }
                        else
                        {
                            using (var tr = doc.TransactionManager.StartTransaction())
                            {
                                var pt = Plan2Ext.Globs.GetLabelPoint(elFG);
                                if (pt.HasValue)
                                {
                                    var rblock = tr.GetObject(ssRB[rbInd], _AcDb.OpenMode.ForWrite) as _AcDb.BlockReference;

                                    var pos = rblock.GetCenter();
                                    if (!pos.HasValue)
                                    {
                                        pos = rblock.Position;
                                    }
                                    _AcGe.Vector3d acVec3d = pos.Value.GetVectorTo(pt.Value);
                                    rblock.TransformBy(_AcGe.Matrix3d.Displacement(acVec3d));
                                    //ed.WriteMessage("\nCentroid is {0}", pt);
                                }
                                else
                                {
                                    var poly = tr.GetObject(elFG, _AcDb.OpenMode.ForRead) as _AcDb.Polyline;
                                    string msg = string.Format(CultureInfo.CurrentCulture, "\nFläche {0}. Centroid liegt außerhalb.", poly.Handle.ToString());
                                    ed.WriteMessage(msg);
                                    log.Warn(msg);

                                    FehlerLineOrHatchPoly(elFG, centroidOutsideLayerName, 0, 0, 255, tm, Plan2Ext.Globs.GetCentroid(elFG));
                                    centroidOutside++;
                                }

                                tr.Commit();
                            }
                        }
                    }

                    //if (_Raumbloecke.Count > 0)
                    //{
                    //    List<object> insPoints = new List<object>();
                    //    for (int i = 0; i < _Raumbloecke.Count; i++)
                    //    {
                    //        _AcIntCom.AcadBlockReference rbBlock = (_AcIntCom.AcadBlockReference)Globs.ObjectIdToAcadEntity(_Raumbloecke[i], tm);
                    //        insPoints.Add(rbBlock.InsertionPoint);
                    //    }

                    //    _AcCm.Color col = _AcCm.Color.FromRgb((byte)0, (byte)255, (byte)0);

                    //    Plan2Ext.Globs.InsertFehlerLines(insPoints, _LooseBlockLayer, 50, Math.PI * 1.25, col);

                    //}



                    if (fehlerKeinRb > 0 || fehlerMehrRb > 0 || _Raumbloecke.Count > 0 || centroidOutside > 0)
                    {
                        string msg = string.Format(CultureInfo.CurrentCulture, "Räume ohne Raumblock: {0}\nRäume mit mehr als einem Raumblock: {1}\nRaumblöcke ohne entsprechende Flächengrenzen: {2}\nCentroid liegt außerhalb: {3}", fehlerKeinRb, fehlerMehrRb, _Raumbloecke.Count, centroidOutside);
                        log.Debug(msg);
                        _AcAp.Application.ShowAlertDialog(msg);

                    }

                    //If wucs = 0 Then
                    //    ThisDrawing.SendCommand "(command ""_.UCS"" ""_P"") "
                    //End If

                    myT.Commit();

                }
                finally
                {
                    myT.Dispose();
                }

            }
            finally
            {
                if (useXRefs) DeleteExplodedXrefEntities();

                doc.Editor.CurrentUserCoordinateSystem = ucs;
            }
        }

        internal void DelErrSyms(_AcAp.Document doc)
        {
            var invalidNrRbLayerName = "Blockanzahl_ungleich_1";
            var centroidOutsideLayerName = "Centroid_liegt_außerhalb.";

            DeleteFehlerSymbols(new List<string>() { invalidNrRbLayerName, centroidOutsideLayerName });

        }

        #endregion

        #region Private

        private void LayerOnAndThaw(string layerName)
        {
            log.DebugFormat(CultureInfo.CurrentCulture, "Layer '{0}' ein und tauen.", layerName);
            var doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            using (_AcDb.Transaction trans = doc.TransactionManager.StartTransaction())
            {
                try
                {
                    _AcDb.LayerTable layTb = trans.GetObject(db.LayerTableId, _AcDb.OpenMode.ForRead) as _AcDb.LayerTable;
                    if (layTb.Has(layerName))
                    {
                        var layId = layTb[layerName];
                        _AcDb.LayerTableRecord ltr = trans.GetObject(layId, _AcDb.OpenMode.ForRead) as _AcDb.LayerTableRecord;
                        log.InfoFormat("Taue und schalte Layer {0} ein.", ltr.Name);
                        ltr.UpgradeOpen();
                        ltr.IsOff = false;
                        if (string.Compare(_AcAp.Application.GetSystemVariable("CLAYER").ToString(), ltr.Name, StringComparison.OrdinalIgnoreCase) != 0)
                        {
                            ltr.IsFrozen = false;
                        }
                    }
                }
                finally
                {
                    trans.Commit();
                }
            }
        }
        private static void DeleteFehlerSymbols(List<string> layerNames)
        {
            foreach (var layerName in layerNames)
            {
                Plan2Ext.Globs.DeleteFehlerLines(layerName);
                Plan2Ext.Globs.DeleteHatches(layerName);
            }
        }

        private static void FehlerLineOrHatchPoly(_AcDb.ObjectId oid, string layer, int red, int green, int blue, _AcDb.TransactionManager tm, _AcGe.Point3d? label)
        {
            if (label.HasValue)
            {
                _AcCm.Color col = _AcCm.Color.FromRgb((byte)red, (byte)green, (byte)blue);
                Plan2Ext.Globs.InsertFehlerLines(new List<_AcGe.Point3d> { label.Value }, layer, 50, Math.PI * 1.25, col);
            }
            else
            {
                HatchPoly(oid, layer, red, green, blue, tm);
            }
        }

        private static void HatchPoly(_AcDb.ObjectId oid, string layer, int red, int green, int blue, _AcDb.TransactionManager tm)
        {
            string patternName = "_SOLID";
            bool bAssociativity = false;

            _AcIntCom.AcadEntity oPoly = Plan2Ext.Globs.ObjectIdToAcadEntity(oid, tm);
            _AcIntCom.AcadEntity oCopiedPoly = null;
            if (oPoly is _AcIntCom.AcadPolyline)
            {
                _AcIntCom.AcadPolyline poly1 = (_AcIntCom.AcadPolyline)oPoly;
                oCopiedPoly = (_AcIntCom.AcadEntity)poly1.Copy();
                ((_AcIntCom.AcadPolyline)oCopiedPoly).Closed = true;
            }
            else if (oPoly is _AcIntCom.AcadLWPolyline)
            {
                _AcIntCom.AcadLWPolyline poly2 = (_AcIntCom.AcadLWPolyline)oPoly;
                oCopiedPoly = (_AcIntCom.AcadEntity)poly2.Copy();
                ((_AcIntCom.AcadLWPolyline)oCopiedPoly).Closed = true;
            }
            else // 3dpoly
            {
                _AcIntCom.Acad3DPolyline poly2 = (_AcIntCom.Acad3DPolyline)oPoly;
                oCopiedPoly = (_AcIntCom.AcadEntity)poly2.Copy();
                ((_AcIntCom.Acad3DPolyline)oCopiedPoly).Closed = true;
            }

            //' Create the non associative Hatch object in model space
            _AcInt.AcadApplication app = (_AcInt.AcadApplication)_AcAp.Application.AcadApplication;
            //log.Info("1..");
            //Autodesk.AutoCAD.Interop.AcadApplication abc = (Autodesk.AutoCAD.Interop.AcadApplication)Autodesk.AutoCAD.ApplicationServices.Application.AcadApplication;
            //log.Info("2..");

            _AcIntCom.AcadHatch hatchObj = app.ActiveDocument.ModelSpace.AddHatch(AcPatternType, patternName, bAssociativity, AcHatchObject);
            _AcIntCom.AcadAcCmColor col1 = new _AcIntCom.AcadAcCmColor(); // app.GetInterfaceObject(COLOROBJECTPROGID) as AcadAcCmColor;
            //AcadAcCmColor col2 = app.GetInterfaceObject(COLOROBJECTPROGID) as AcadAcCmColor;
            col1.SetRGB(red, green, blue);
            hatchObj.TrueColor = col1;
            _AcIntCom.AcadEntity[] outerLoop = new _AcIntCom.AcadEntity[] { oCopiedPoly };
            hatchObj.AppendOuterLoop(outerLoop);
            SetLayer((_AcIntCom.AcadEntity)hatchObj, layer);
            if (oCopiedPoly != null) oCopiedPoly.Delete();

        }

        private static void SetLayer(_AcIntCom.AcadEntity oCopiedPoly, string layerName)
        {
            Plan2Ext.Globs.CreateLayer(layerName);
            oCopiedPoly.Layer = layerName;

        }

        private void ZoomToFlaechenGrenzen()
        {
            log.DebugFormat(CultureInfo.CurrentCulture, "Zoom auf Flächengrenzen");
            if (_FlaechenGrenzen.Count == 0) return;

            double MinX, MinY, MaxX, MaxY;

            _AcAp.Document doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            _AcDb.Database db = doc.Database;
            _AcEd.Editor ed = doc.Editor;
            _AcDb.TransactionManager tm = db.TransactionManager;
            _AcDb.Transaction myT = tm.StartTransaction();
            try
            {

                _AcDb.Extents3d ext = GetExtents(tm, _FlaechenGrenzen[0]);
                MinX = ext.MinPoint.X;
                MinY = ext.MinPoint.Y;
                MaxX = ext.MaxPoint.X;
                MaxY = ext.MaxPoint.Y;

                for (int i = 1; i < _FlaechenGrenzen.Count; i++)
                {
                    _AcDb.ObjectId oid = _FlaechenGrenzen[i];
                    ext = GetExtents(tm, oid);
                    if (ext.MinPoint.X < MinX) MinX = ext.MinPoint.X;
                    if (ext.MinPoint.Y < MinY) MinY = ext.MinPoint.Y;
                    if (ext.MaxPoint.X > MaxX) MaxX = ext.MaxPoint.X;
                    if (ext.MaxPoint.Y > MaxY) MaxY = ext.MaxPoint.Y;
                }


                //Globs.Zoom( new Point3d(MinX, MinY, 0.0), new Point3d(MaxX, MaxY, 0.0),new Point3d(), 1.0);
                //Globs.ZoomWin3(ed, new Point3d(MinX, MinY, 0.0), new Point3d(MaxX, MaxY, 0.0));
                //Globs.ZoomWin2(ed, new Point3d(MinX, MinY, 0.0), new Point3d(MaxX, MaxY, 0.0));

                myT.Commit();

            }
            finally
            {
                myT.Dispose();
            }

            // Rauszoomen, sonst werden Blöcken nicht gefunden, die außerhalb der Flächengrenzen liegen.
            MinX -= ABSTANDTEXT;
            MinY -= ABSTANDTEXT;
            MaxX += ABSTANDTEXT;
            MaxY += ABSTANDTEXT;

            Plan2Ext.Globs.Zoom(new _AcGe.Point3d(MinX, MinY, 0.0), new _AcGe.Point3d(MaxX, MaxY, 0.0), new _AcGe.Point3d(), 1.0);

        }

        private static _AcDb.Extents3d GetExtents(_AcDb.TransactionManager tm, _AcDb.ObjectId oid)
        {
            using (_AcDb.DBObject dbobj = tm.GetObject(oid, _AcDb.OpenMode.ForRead, false))
            {
                _AcDb.Entity ent = dbobj as _AcDb.Entity;
                return ent.GeometricExtents;
            }
        }

        private List<_AcDb.ObjectId> GetFgAnz(_AcGe.Point3d minExt, _AcGe.Point3d maxExt, _AcDb.ObjectId elFG)
        {
            List<_AcDb.ObjectId> Ret = new List<_AcDb.ObjectId>();

            _AcEd.Editor ed = _AcAp.Application.DocumentManager.MdiActiveDocument.Editor;
            _AcEd.SelectionFilter filter = new _AcEd.SelectionFilter(new _AcDb.TypedValue[] { 
                new _AcDb.TypedValue((int)_AcDb.DxfCode.Start,"*POLYLINE" ),
                new _AcDb.TypedValue((int)_AcDb.DxfCode.LayerName,_FgLayer )
            });
            _AcEd.PromptSelectionResult res = null;
            res = ed.SelectCrossingWindow(minExt, maxExt, filter);
            //res = ed.SelectAll(filter);
            if (res.Status != _AcEd.PromptStatus.OK)
            {
                // todo: logging: lot4net?
                return Ret;
            }

#if BRX_APP
            _AcEd.SelectionSet ss = res.Value;
#else
            using (_AcEd.SelectionSet ss = res.Value)
#endif
            {

                _AcDb.ObjectId[] idArray = ss.GetObjectIds();
                _AcDb.Database db = _AcAp.Application.DocumentManager.MdiActiveDocument.Database;
                _AcDb.TransactionManager tm = db.TransactionManager;
                _AcDb.Transaction myT = tm.StartTransaction();
                try
                {
                    for (int i = 0; i < idArray.Length; i++)
                    {
                        _AcDb.ObjectId oid = idArray[i];
                        if (!oid.Equals(elFG))
                        {
                            if (PolyInPoly(tm, oid, elFG))
                                AddRbToRetCol(Ret, tm, oid);
                        }
                    }
                    myT.Commit();
                }
                finally
                {
                    myT.Dispose();
                }

            }

            return Ret;

        }

        private static bool PolyInPoly(_AcDb.TransactionManager tm, _AcDb.ObjectId oid, _AcDb.ObjectId elFG)
        {
            using (_AcDb.DBObject pEntity = tm.GetObject(oid, _AcDb.OpenMode.ForRead, false))
            {
                using (_AcDb.DBObject pElFG = tm.GetObject(elFG, _AcDb.OpenMode.ForRead, false))
                {

                    if (pEntity is _AcDb.Polyline2d)
                    {
                        _AcDb.Polyline2d oldPolyline = (_AcDb.Polyline2d)pEntity;
                        foreach (_AcDb.ObjectId Vertex2d in oldPolyline)
                        {
                            using (_AcDb.DBObject dbobj = tm.GetObject(Vertex2d, _AcDb.OpenMode.ForRead, false))
                            {
                                _AcDb.Vertex2d vertex = dbobj as _AcDb.Vertex2d;

                                if (vertex == null)
                                {
                                    string msg = string.Format(CultureInfo.CurrentCulture, "Polylinie {0} gibt falsches Objekt {1} als Vertex zurück.", oldPolyline.Handle.ToString(), dbobj.GetType().ToString());
                                    throw new InvalidOperationException(string.Format(msg));
                                }

                                _AcGe.Point3d vertexPoint = oldPolyline.VertexPosition(vertex);
                                if (!AreaEngine.InPoly(vertexPoint, (_AcDb.Entity)pElFG)) return false;

                            }
                        }
                        return true;
                    }
                    else if (pEntity is _AcDb.Polyline3d)
                    {
                        _AcDb.Polyline3d poly3d = (_AcDb.Polyline3d)pEntity;
                        foreach (_AcDb.ObjectId Vertex3d in poly3d)
                        {
                            using (_AcDb.DBObject dbobj = tm.GetObject(Vertex3d, _AcDb.OpenMode.ForRead, false))
                            {
                                _AcDb.PolylineVertex3d vertex = dbobj as _AcDb.PolylineVertex3d;

                                if (vertex == null)
                                {
                                    string msg = string.Format(CultureInfo.CurrentCulture, "3D-Polylinie {0} gibt falsches Objekt {1} als Vertex zurück.", poly3d.Handle.ToString(), dbobj.GetType().ToString());
                                    throw new InvalidOperationException(string.Format(msg));
                                }

                                _AcGe.Point3d vertexPoint = vertex.Position;
                                if (!AreaEngine.InPoly(vertexPoint, (_AcDb.Entity)pElFG)) return false;

                            }
                        }
                        return true;
                    }
                    else if (pEntity is _AcDb.Polyline)
                    {
                        _AcDb.Polyline poly = pEntity as _AcDb.Polyline;
                        for (int i = 0; i < poly.NumberOfVertices; i++)
                        {
                            _AcGe.Point3d vertexPoint = poly.GetPoint3dAt(i);
                            if (!AreaEngine.InPoly(vertexPoint, (_AcDb.Entity)pElFG)) return false;

                        }
                        return true;
                    }
                }
            }
            return false;
        }

        private void AddRbToRetCol(List<_AcDb.ObjectId> Ret, _AcDb.TransactionManager tm, _AcDb.ObjectId elFG)
        {

            _AcDb.Extents3d ext = GetExtents(tm, elFG);
            _AcGe.Point3d minExt = new _AcGe.Point3d(ext.MinPoint.X - ABSTANDTEXT, ext.MinPoint.Y - ABSTANDTEXT, ext.MinPoint.Z);
            _AcGe.Point3d maxExt = new _AcGe.Point3d(ext.MaxPoint.X + ABSTANDTEXT, ext.MaxPoint.Y + ABSTANDTEXT, ext.MaxPoint.Z);

            _AcEd.Editor ed = _AcAp.Application.DocumentManager.MdiActiveDocument.Editor;
            _AcEd.SelectionFilter filter = new _AcEd.SelectionFilter(new _AcDb.TypedValue[] { 
                new _AcDb.TypedValue((int)_AcDb.DxfCode.Start,"INSERT" ),
                new _AcDb.TypedValue((int)_AcDb.DxfCode.BlockName,_RaumblockName)
            });
            _AcEd.PromptSelectionResult res = null;
            res = ed.SelectCrossingWindow(minExt, maxExt, filter);
            if (res.Status != _AcEd.PromptStatus.OK)
            {
                // todo: logging: lot4net?
                return;
            }

#if BRX_APP
            _AcEd.SelectionSet ss = res.Value;
#else
            using (_AcEd.SelectionSet ss = res.Value)
#endif
            {


                _AcDb.ObjectId[] idArray = ss.GetObjectIds();
                for (int i = 0; i < idArray.Length; i++)
                {
                    _AcDb.ObjectId oid = idArray[i];
                    using (_AcDb.DBObject pEntity = tm.GetObject(oid, _AcDb.OpenMode.ForRead, false))
                    {
                        using (_AcDb.Entity entElFG = tm.GetObject(elFG, _AcDb.OpenMode.ForRead, false) as _AcDb.Entity)
                        {
                            if (pEntity is _AcDb.BlockReference)
                            {
                                _AcDb.BlockReference br = pEntity as _AcDb.BlockReference;
                                if (AreaEngine.InPoly(br.Position, entElFG))
                                {
                                    Ret.Add(oid);
                                }
                            }
                        }
                    }
                }
            }
        }

        private List<_AcDb.ObjectId> selRB(_AcGe.Point3d minExt, _AcGe.Point3d maxExt)
        {
            _AcEd.Editor ed = _AcAp.Application.DocumentManager.MdiActiveDocument.Editor;
            _AcEd.SelectionFilter filter = new _AcEd.SelectionFilter(new _AcDb.TypedValue[] { 
                new _AcDb.TypedValue((int)_AcDb.DxfCode.Start,"INSERT" ),
                new _AcDb.TypedValue((int)_AcDb.DxfCode.BlockName,_RaumblockName)
            });
            _AcEd.PromptSelectionResult res = null;
            res = ed.SelectCrossingWindow(minExt, maxExt, filter);
            if (res.Status != _AcEd.PromptStatus.OK)
            {
                log.Warn("Fehler beim Auswählen der Raumblöcke!");
                return new List<_AcDb.ObjectId>();
            }

#if BRX_APP
            _AcEd.SelectionSet ss = res.Value;
#else
            using (_AcEd.SelectionSet ss = res.Value)
#endif
            {

                return ss.GetObjectIds().ToList();
            }

        }

        #endregion

        #region XREF-Handling
        List<_AcDb.ObjectId> _ExplodedXrefEntities = new List<_AcDb.ObjectId>();

        private void ExplodeAllXrefs()
        {
            try
            {

                log.Debug("ExplodeAllXrefs");
                _ExplodedXrefEntities.Clear();
                // todo

                var doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
                var db = doc.Database;
                List<_AcDb.ObjectId> allXrefsInMs = new List<_AcDb.ObjectId>();
                GetAllMsXrefs(db, allXrefsInMs);

                // todo
                List<_AcDb.ObjectId> blockRefs = new List<_AcDb.ObjectId>();
                InsertBlocks(db, allXrefsInMs, blockRefs);

                List<_AcDb.ObjectId> newlyCreatedObjects = new List<_AcDb.ObjectId>();
                // todo
                ExplodeBlocks(db, blockRefs, newlyCreatedObjects, deleteRef: true, deleteBtr: true);

                _ExplodedXrefEntities.AddRange(newlyCreatedObjects);
            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
            }
        }

        private void InsertBlocks(_AcDb.Database db, List<_AcDb.ObjectId> allXrefsInMs, List<_AcDb.ObjectId> _BlockRefs)
        {
            log.Debug("InsertBlocks");
            using (_AcDb.Transaction tr = db.TransactionManager.StartTransaction())
            {
                foreach (var oid in allXrefsInMs)
                {
                    var br = tr.GetObject(oid, _AcDb.OpenMode.ForRead) as _AcDb.BlockReference;
                    if (br != null)
                    {
                        var bd = (_AcDb.BlockTableRecord)tr.GetObject(br.BlockTableRecord, _AcDb.OpenMode.ForRead);
                        if (bd.IsFromExternalReference)
                        {
                            string name = bd.PathName;
                            var dwgPath = _AcAp.Application.GetSystemVariable("DWGPREFIX").ToString();
                            if (System.IO.Path.IsPathRooted(bd.PathName))
                            {
                                log.DebugFormat(string.Format(CultureInfo.CurrentCulture, "Füge Block '{0}' ein. XREF-Pfad: '{1}'.", br.Name + "_AS_BLOCK", bd.PathName));
                                var blockOid = Plan2Ext.Globs.InsertDwg(bd.PathName, br.Position, br.Rotation, br.Name + "_AS_BLOCK");
                                _BlockRefs.Add(blockOid);
                            }
                            else
                            {
                                log.DebugFormat(string.Format(CultureInfo.CurrentCulture, "Füge Block '{0}' ein. XREF-Pfad: '{1}'.", br.Name + "_AS_BLOCK", System.IO.Path.GetFullPath(dwgPath + bd.PathName)));
                                var blockOid = Plan2Ext.Globs.InsertDwg(System.IO.Path.GetFullPath(dwgPath + bd.PathName), br.Position, br.Rotation, br.Name + "_AS_BLOCK");
                                _BlockRefs.Add(blockOid);
                            }
                        }
                    }

                }

                tr.Commit();
            }
        }

        private void ExplodeBlocks(_AcDb.Database db, List<_AcDb.ObjectId> allXrefsInMs, List<_AcDb.ObjectId> newlyCreatedObjects, bool deleteRef, bool deleteBtr)
        {
            log.Debug("ExplodeXRefs");
            using (_AcDb.Transaction tr = _TransMan.StartTransaction())
            {
                _AcDb.BlockTable bt = (_AcDb.BlockTable)tr.GetObject(db.BlockTableId, _AcDb.OpenMode.ForRead);

                _AcDb.BlockTableRecord btr = (_AcDb.BlockTableRecord)tr.GetObject(_AcDb.SymbolUtilityServices.GetBlockModelSpaceId(db), _AcDb.OpenMode.ForWrite);


                foreach (var oid in allXrefsInMs)
                {
                    _AcDb.DBObjectCollection objs = new _AcDb.DBObjectCollection();
                    _AcDb.BlockReference block = (_AcDb.BlockReference)tr.GetObject(oid, _AcDb.OpenMode.ForRead);
                    log.DebugFormat(CultureInfo.CurrentCulture, "Explode von Block '{0}'.", block.Name);
                    block.Explode(objs);
                    log.DebugFormat(CultureInfo.CurrentCulture, "Block enthält {0} Entities.", objs.Count);
                    _AcDb.ObjectId blockRefTableId = block.BlockTableRecord;


                    foreach (_AcDb.DBObject obj in objs)
                    {
                        _AcDb.Entity ent = (_AcDb.Entity)obj;
                        btr.AppendEntity(ent);
                        tr.AddNewlyCreatedDBObject(ent, true);

                        newlyCreatedObjects.Add(ent.ObjectId);
                    }

                    if (deleteRef)
                    {
                        log.DebugFormat(CultureInfo.CurrentCulture, "Lösche Block '{0}'.", block.Name);   
                        block.UpgradeOpen();
                        block.Erase();
                    }

                    if (deleteBtr)
                    {
                        log.DebugFormat("DeleteBtr");
                        // funkt nicht -> xref würde gelöscht
                        var bd = (_AcDb.BlockTableRecord)tr.GetObject(blockRefTableId, _AcDb.OpenMode.ForWrite);
                        bd.Erase();
                        log.DebugFormat("Endof DeleteBtr");

                    }
                }
                tr.Commit();
            }
        }

        private void GetAllMsXrefs(_AcDb.Database db, List<_AcDb.ObjectId> allXrefsInMs)
        {
            log.Debug("GetAllMsXrefs");
            using (_AcDb.Transaction tr = _TransMan.StartTransaction())
            {
                _AcDb.BlockTable bt = (_AcDb.BlockTable)tr.GetObject(db.BlockTableId, _AcDb.OpenMode.ForRead);

                _AcDb.BlockTableRecord btr = (_AcDb.BlockTableRecord)tr.GetObject(_AcDb.SymbolUtilityServices.GetBlockModelSpaceId(db), _AcDb.OpenMode.ForRead);

                foreach (var oid in btr)
                {
                    var br = tr.GetObject(oid, _AcDb.OpenMode.ForRead) as _AcDb.BlockReference;
                    if (br != null)
                    {
                        var bd = (_AcDb.BlockTableRecord)tr.GetObject(br.BlockTableRecord, _AcDb.OpenMode.ForRead);
                        if (bd.IsFromExternalReference)
                        {
                            allXrefsInMs.Add(br.ObjectId);
                        }
                    }
                }

                tr.Commit();
            }
        }

        private void DeleteExplodedXrefEntities()
        {
            using (_AcDb.Transaction tr = _TransMan.StartTransaction())
            {
                foreach (var oid in _ExplodedXrefEntities)
                {
                    var ent = tr.GetObject(oid, _AcDb.OpenMode.ForWrite);
                    ent.Erase();
                }

                tr.Commit();
            }
        }

        public static int XrefCompare(string name, string xrefName, StringComparison sc)
        {
            var parts = xrefName.Split(new char[] { '|' });
            return string.Compare(name, parts[parts.Length - 1], sc);
        }

        public static string RemoveXRefPart(string name)
        {
            var parts = name.Split(new char[] { '|' });
            return parts[parts.Length - 1];
        }

        #endregion

    }
}
