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
using _AcIntCom = BricscadDb;
using _AcInt = BricscadApp;
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
using _AcIntCom = Autodesk.AutoCAD.Interop.Common;
using _AcInt = Autodesk.AutoCAD.Interop;
#endif

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plan2Ext
{
    internal class AreaEngine
    {
        #region log4net Initialization
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Convert.ToString((typeof(AreaEngine))));
        #endregion

        //#region Constants
        //internal const string FEHLERBLOCKNAME = "UPDFLA_FEHLER";
        //#endregion

        #region Members
        private _AcDb.TransactionManager _TransMan = null;
        private _AcEd.Editor _Editor = null;
        #endregion

        #region Lifecycle
        public AreaEngine()
        {
            _TransMan = _AcAp.Application.DocumentManager.MdiActiveDocument.Database.TransactionManager;
            _Editor = _AcAp.Application.DocumentManager.MdiActiveDocument.Editor;
        }
        #endregion

        #region Internal
        internal bool SelectFgAndRb(List<_AcDb.ObjectId> flaechenGrenzen, List<_AcDb.ObjectId> raumBloecke, string fgLayer, string rbName, bool selectAll=false)
        {
            flaechenGrenzen.Clear();
            raumBloecke.Clear();

            log.Debug("Auswahl Flächengrenzen und Raumblöcke");
            _AcEd.PromptSelectionResult res = null;
            _AcEd.SelectionFilter filter = new _AcEd.SelectionFilter(new _AcDb.TypedValue[] {

                new _AcDb.TypedValue((int)_AcDb.DxfCode.Operator ,"<OR"),

                new _AcDb.TypedValue((int)_AcDb.DxfCode.Operator ,"<AND"),
                new _AcDb.TypedValue((int)_AcDb.DxfCode.Start ,"*POLYLINE"),
                new _AcDb.TypedValue((int)_AcDb.DxfCode.LayerName  , fgLayer  ),
                new _AcDb.TypedValue((int)_AcDb.DxfCode.Operator ,"AND>"),

                new _AcDb.TypedValue((int)_AcDb.DxfCode.Operator ,"<AND"),
                new _AcDb.TypedValue((int)_AcDb.DxfCode.Start ,"INSERT"),
                //new _AcDb.TypedValue((int)_AcDb.DxfCode.BlockName ,rbName  ),
                new _AcDb.TypedValue((int)_AcDb.DxfCode.Operator ,"AND>"),

                new _AcDb.TypedValue((int)_AcDb.DxfCode.Operator ,"OR>")
            });

            if (selectAll)
            {
                res = _Editor.SelectAll(filter);
            }
            else
            {
                _AcEd.PromptSelectionOptions SelOpts = new _AcEd.PromptSelectionOptions();
                SelOpts.MessageForAdding = "Raumblöcke und Flächengrenzen wählen: ";
                res = _Editor.GetSelection(SelOpts, filter);
                if (res.Status != _AcEd.PromptStatus.OK)
                {
                    log.Debug("Auswahl wurde abgebrochen.");
                    if (res.Status == _AcEd.PromptStatus.Cancel) return false;
                    else return true;
                }
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
                        using (_AcDb.DBObject dbobj = tm.GetObject(oid, _AcDb.OpenMode.ForRead, false) as _AcDb.Entity)
                        {
                            _AcDb.Entity ent = dbobj as _AcDb.Entity;
                            if (ent != null)
                            {
                                if (ent is _AcDb.BlockReference)
                                {
                                    if (string.Compare(rbName, Plan2Ext.Globs.GetBlockname((_AcDb.BlockReference)ent, myT), StringComparison.OrdinalIgnoreCase) == 0)
                                    {
                                        raumBloecke.Add(oid);
                                    }
                                }
                                else flaechenGrenzen.Add(oid);
                            }
                        }
                    }
                    myT.Commit();
                }
                finally
                {
                    myT.Dispose();
                }
            }

            log.DebugFormat(CultureInfo.CurrentCulture, "Auswahl: Raumblöcke {0}, Flächengrenzen {1}", raumBloecke.Count, flaechenGrenzen.Count);

            return true;

        }


        #endregion

        #region Static Methods
        public static bool InPoly(_AcGe.Point3d vertexPoint, _AcDb.Entity pElFG)
        {
            int nrTrue = 0;
            int nrFalse = 0;

            if (InPoly(vertexPoint, 1.0, 1.0, pElFG)) nrTrue++;
            else nrFalse++;

            if (InPoly(vertexPoint, -1.0, 1.0, pElFG)) nrTrue++;
            else nrFalse++;

            if (InPoly(vertexPoint, -1.0, -1.0, pElFG)) nrTrue++;
            else nrFalse++;

            return nrTrue > nrFalse;

        }

        public static bool InPoly(_AcGe.Point3d vertexPoint, double xOffs, double yOffs, _AcDb.Entity pElFG)
        {
            using (_AcDb.Ray ray = new _AcDb.Ray())
            {
                ray.BasePoint = vertexPoint;
                ray.UnitDir = new _AcGe.Vector3d(xOffs, yOffs, 0.0);

                _AcGe.Point3dCollection col = new _AcGe.Point3dCollection();
                ((_AcDb.Entity)ray).IntersectWith(pElFG, _AcDb.Intersect.OnBothOperands, col, IntPtr.Zero, IntPtr.Zero);

                if ((col.Count % 2) == 1) return true;
                else return false;

            }
        }

        #endregion

        #region FgRb-Structure
        internal static int NrOfOverlaps = 0;
        internal static List<_AcDb.ObjectId> OrphanRaumblocks = new List<_AcDb.ObjectId>();
        private class FgRbStructureInTrans
        {
            public _AcDb.Entity FlaechenGrenze { get; set; }
            private List<_AcDb.Entity> _Inseln = new List<_AcDb.Entity>();
            public List<_AcDb.Entity> Inseln
            {
                get { return _Inseln; }
                set { _Inseln = value; }
            }
            private List<_AcDb.Entity> _Abzugsflaechen = new List<_AcDb.Entity>();
            public List<_AcDb.Entity> Abzugsflaechen
            {
                get { return _Abzugsflaechen; }
                set { _Abzugsflaechen = value; }
            }
            private List<_AcDb.Entity> _Raumbloecke = new List<_AcDb.Entity>();
            public List<_AcDb.Entity> Raumbloecke
            {
                get { return _Raumbloecke; }
                set { _Raumbloecke = value; }
            }

            public FgRbStructure AsFbRbStructure()
            {
                var idStructure = new FgRbStructure();
                idStructure.FlaechenGrenze = this.FlaechenGrenze.Id;
                idStructure.Inseln = this.Inseln.Select(x => x.Id).ToList();
                idStructure.Abzugsflaechen = this.Abzugsflaechen.Select(x => x.Id).ToList();
                idStructure.Raumbloecke = this.Raumbloecke.Select(x => x.Id).ToList();
                return idStructure;
            }
        }

        internal class FgRbStructure
        {
            public _AcDb.ObjectId FlaechenGrenze { get; set; }
            private List<_AcDb.ObjectId> _Inseln = new List<_AcDb.ObjectId>();
            public List<_AcDb.ObjectId> Inseln
            {
                get { return _Inseln; }
                set { _Inseln = value; }
            }
            private List<_AcDb.ObjectId> _Abzugsflaechen = new List<_AcDb.ObjectId>();
            public List<_AcDb.ObjectId> Abzugsflaechen
            {
                get { return _Abzugsflaechen; }
                set { _Abzugsflaechen = value; }
            }
            private List<_AcDb.ObjectId> _Raumbloecke = new List<_AcDb.ObjectId>();
            public List<_AcDb.ObjectId> Raumbloecke
            {
                get { return _Raumbloecke; }
                set { _Raumbloecke = value; }
            }

            public bool IsPointInFg(_AcGe.Point3d p, _AcDb.Transaction trans)
            {
                var fg = (_AcDb.Entity)trans.GetObject(FlaechenGrenze, _AcDb.OpenMode.ForRead);
                if (!IsPosInFg(p, fg)) return false;

                foreach (var inselOid in Inseln)
                {
                    var insel = (_AcDb.Entity)trans.GetObject(inselOid, _AcDb.OpenMode.ForRead);
                    if (IsPosInFg(p, insel)) return false;
                }
                return true;
            }

            #region Constants
            private const int AcHatchObject = 0;
            private const int AcPatternType = 0;
            #endregion

            public _AcDb.ObjectId HatchPoly(_AcDb.ObjectId oid, List<_AcDb.ObjectId> inner, string layer, int colorIndex, _AcDb.TransactionManager tm)
            {
                _AcIntCom.AcadAcCmColor col = new _AcIntCom.AcadAcCmColor(); // app.GetInterfaceObject(COLOROBJECTPROGID) as AcadAcCmColor;
                col.ColorIndex = (_AcIntCom.AcColor)colorIndex;
                return Plan2Ext.Globs.HatchPoly(oid, inner, layer, col, tm);
            }
            public _AcDb.ObjectId HatchPoly(_AcDb.ObjectId oid, List<_AcDb.ObjectId> inner, string layer, int red, int green, int blue, _AcDb.TransactionManager tm)
            {
                _AcIntCom.AcadAcCmColor col = new _AcIntCom.AcadAcCmColor(); // app.GetInterfaceObject(COLOROBJECTPROGID) as AcadAcCmColor;
                col.SetRGB(red, green, blue);
                return Plan2Ext.Globs.HatchPoly(oid, inner, layer, col, tm);
            }

            //private _AcDb.ObjectId HatchPoly(_AcDb.ObjectId oid, List<_AcDb.ObjectId> inner, string layer, _AcIntCom.AcadAcCmColor col, _AcDb.TransactionManager tm)
            //{
            //    string patternName = "_SOLID";
            //    bool bAssociativity = false;

            //    _AcIntCom.AcadEntity oCopiedPoly = CopyPoly(oid, tm);
            //    List<_AcIntCom.AcadEntity> innerPolys = inner.Select(x => CopyPoly(x, tm)).ToList();

            //    //' Create the non associative Hatch object in model space
            //    _AcInt.AcadApplication app = (_AcInt.AcadApplication)_AcAp.Application.AcadApplication;
            //    _AcIntCom.AcadHatch hatchObj = app.ActiveDocument.ModelSpace.AddHatch(AcPatternType, patternName, bAssociativity, AcHatchObject);
            //    //_AcIntCom.AcadAcCmColor col1 = new _AcIntCom.AcadAcCmColor(); // app.GetInterfaceObject(COLOROBJECTPROGID) as AcadAcCmColor;
            //    //col1.SetRGB(red, green, blue);
            //    hatchObj.TrueColor = col;
            //    _AcIntCom.AcadEntity[] outerLoop = new _AcIntCom.AcadEntity[] { oCopiedPoly };
            //    hatchObj.AppendOuterLoop(outerLoop);
            //    try
            //    {
            //        if (innerPolys.Count > 0)
            //        {
            //            foreach (var innerPoly in innerPolys)
            //            {
            //                _AcIntCom.AcadEntity[] innerLoop = new _AcIntCom.AcadEntity[] { innerPoly };
            //                hatchObj.AppendInnerLoop(innerLoop);
            //            }
            //            //_AcIntCom.AcadEntity[] innerLoop = innerPolys.ToArray();
            //            //hatchObj.AppendInnerLoop(innerLoop);
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        log.Warn(ex.Message);
            //    }
            //    hatchObj.Evaluate();
            //    SetLayer((_AcIntCom.AcadEntity)hatchObj, layer);
            //    if (oCopiedPoly != null) oCopiedPoly.Delete();
            //    foreach (var poly in innerPolys)
            //    {
            //        poly.Delete();
            //    }
                
            //    return new _AcDb.ObjectId((IntPtr)hatchObj.ObjectID);
            //}

            //private _AcIntCom.AcadEntity CopyPoly(_AcDb.ObjectId oid, _AcDb.TransactionManager tm)
            //{
            //    _AcIntCom.AcadEntity oPoly = Globs.ObjectIdToAcadEntity(oid, tm);
            //    _AcIntCom.AcadEntity oCopiedPoly = null;
            //    if (oPoly is _AcIntCom.AcadPolyline)
            //    {
            //        _AcIntCom.AcadPolyline poly1 = (_AcIntCom.AcadPolyline)oPoly;
            //        oCopiedPoly = (_AcIntCom.AcadEntity)poly1.Copy();
            //        ((_AcIntCom.AcadPolyline)oCopiedPoly).Closed = true;
            //    }
            //    else if (oPoly is _AcIntCom.AcadLWPolyline)
            //    {
            //        _AcIntCom.AcadLWPolyline poly2 = (_AcIntCom.AcadLWPolyline)oPoly;
            //        oCopiedPoly = (_AcIntCom.AcadEntity)poly2.Copy();
            //        ((_AcIntCom.AcadLWPolyline)oCopiedPoly).Closed = true;
            //    }
            //    else // 3dpoly
            //    {
            //        _AcIntCom.Acad3DPolyline poly2 = (_AcIntCom.Acad3DPolyline)oPoly;
            //        oCopiedPoly = (_AcIntCom.AcadEntity)poly2.Copy();
            //        ((_AcIntCom.Acad3DPolyline)oCopiedPoly).Closed = true;
            //    }
            //    return oCopiedPoly;
            //}

            //private void SetLayer(_AcIntCom.AcadEntity oCopiedPoly, string layerName)
            //{
            //    Globs.CreateLayer(layerName);
            //    oCopiedPoly.Layer = layerName;
            //}
        }

        internal static Dictionary<_AcDb.ObjectId, FgRbStructure> GetFgRbStructs(string rbName, string fgLayer, string afLayer, _AcDb.Database db)
        {
            NrOfOverlaps = 0;
            OrphanRaumblocks.Clear();

            var structs = new Dictionary<_AcDb.ObjectId, FgRbStructureInTrans>();
            using (var tr = db.TransactionManager.StartTransaction())
            {
                var fgs = new List<_AcDb.Entity>();
                var azf = new List<_AcDb.Entity>();
                var rbs = new List<_AcDb.Entity>();

                // Open the blocktable, get the modelspace
                GetAllFgsAbzsAndRbs(rbName, fgLayer, afLayer, db, tr, fgs, azf, rbs);

                foreach (var fg in fgs)
                {
                    var fgRbStruct = new FgRbStructureInTrans() { FlaechenGrenze = fg };
                    // get inner polylines and rbs to ignore
                    foreach (var inner in fgs)
                    {
                        if (PolyInPoly(tr, inner, fg))
                        {
                            fgRbStruct.Inseln.Add(inner);
                        }
                    }
                    foreach (var inner in azf)
                    {
                        if (PolyInPoly(tr, inner, fg))
                        {
                            fgRbStruct.Abzugsflaechen.Add(inner);
                        }
                    }
                    structs.Add(fg.Id, fgRbStruct);
                }

                foreach (var rb in rbs)
                {
                    bool added = false;
                    foreach (var stru in structs.Values)
                    {
                        if (RecAddRbToFg(rb, stru, structs))
                        {
                            added = true;
                            break;
                        }
                    }
                    if (!added)
                    {
                        OrphanRaumblocks.Add(rb.Id);
                    }
                }

                tr.Commit();
            }
            return AsFgRbStructs(structs);
        }

        private static Dictionary<_AcDb.ObjectId, FgRbStructure> AsFgRbStructs(Dictionary<_AcDb.ObjectId, FgRbStructureInTrans> fgRbStructsInTrans)
        {
            var dict = new Dictionary<_AcDb.ObjectId, FgRbStructure>();
            foreach (var kvp in fgRbStructsInTrans)
            {
                dict.Add(kvp.Key, kvp.Value.AsFbRbStructure());
            }
            return dict;
        }

        private static bool RecAddRbToFg(_AcDb.Entity rb, FgRbStructureInTrans stru, Dictionary<_AcDb.ObjectId, FgRbStructureInTrans> fgRbStructs)
        {
            
            var blockRef = rb as _AcDb.BlockReference;
            if (IsPosInFg(blockRef.Position, stru.FlaechenGrenze))
            {
                log.DebugFormat("RecAddRbToFg: rb='{0}',fg='{1}'", rb.Handle.ToString(), stru.FlaechenGrenze.Handle.ToString());
                foreach (var inner in stru.Inseln)
                {
                    if (IsPosInFg(blockRef.Position, inner))
                    {
                        var innerStru = fgRbStructs[inner.Id];
                        if (RecAddRbToFg(rb, innerStru, fgRbStructs))
                        {
                            return true;
                        }
                    }
                }
                stru.Raumbloecke.Add(rb);
                return true;
            }
            return false;
        }

        private static bool IsPosInFg(_AcGe.Point3d point3d, _AcDb.Entity entity)
        {
            if (!OverlapExtents(point3d, entity)) return false;

            if (InPoly(point3d, entity)) return true;
            else return false;
        }

        private static bool OverlapExtents(_AcGe.Point3d point3d, _AcDb.Entity ent1)
        {
            var x1Min = ent1.GeometricExtents.MinPoint.X;
            var x1Max = ent1.GeometricExtents.MaxPoint.X;
            var y1Min = ent1.GeometricExtents.MinPoint.Y;
            var y1Max = ent1.GeometricExtents.MaxPoint.Y;

            if (point3d.X > x1Min && point3d.X < x1Max && point3d.Y > y1Min && point3d.Y < y1Max)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //private static bool RecAddRbToFg(_AcDb.Entity rb, _AcDb.Entity fg, Dictionary<_AcDb.Entity, FgRbStructure> fgRbStructs)
        //{
        //    var blockRef = rb as _AcDb.BlockReference;
        //    if (IsPosInFg(blockRef.Position, fg))
        //    {
        //        foreach (var inner in )
        //    }
        //}

        private static bool OverlapExtents(_AcDb.Entity ent1, _AcDb.Entity ent2)
        {
            var x1Min = ent1.GeometricExtents.MinPoint.X;
            var x1Max = ent1.GeometricExtents.MaxPoint.X;
            var y1Min = ent1.GeometricExtents.MinPoint.Y;
            var y1Max = ent1.GeometricExtents.MaxPoint.Y;

            var x2Min = ent2.GeometricExtents.MinPoint.X;
            var x2Max = ent2.GeometricExtents.MaxPoint.X;
            var y2Min = ent2.GeometricExtents.MinPoint.Y;
            var y2Max = ent2.GeometricExtents.MaxPoint.Y;

            var xOverlap = GetOverlap(x1Min, x1Max, x2Min, x2Max);
            if (xOverlap <= 0.0) return false;
            var yOverlap = GetOverlap(y1Min, y1Max, y2Min, y2Max);
            if (yOverlap <= 0.0) return false;

            return true;
        }

        private static double GetOverlap(double c1Min, double c1Max, double c2Min, double c2Max)
        {
            return Math.Min(c1Max, c2Max) - Math.Max(c1Min, c2Min);
        }

        private static bool PolyInPoly(_AcDb.Transaction tm, _AcDb.Entity inner, _AcDb.Entity outer)
        {
            if (inner == outer) return false;
            if (!OverlapExtents(inner, outer)) return false;

            // check, ob restliche fläche zu klein um eine insel zu sein. damit werden kongruenze flächen verhindert
            var innerCurve = (_AcDb.Curve)inner;
            var outerCurve = (_AcDb.Curve)outer;
            if ((outerCurve.Area - innerCurve.Area) < 0.001 )
            {
                return false;
            }

            NrOfOverlaps++;

            if (inner is _AcDb.Polyline)
            {
                _AcDb.Polyline poly = inner as _AcDb.Polyline;
                for (int i = 0; i < poly.NumberOfVertices; i++)
                {
                    _AcGe.Point3d vertexPoint = poly.GetPoint3dAt(i);
                    if (!AreaEngine.InPoly(vertexPoint, (_AcDb.Entity)outer)) return false;

                }
                return true;
            }
            else if (inner is _AcDb.Polyline2d)
            {
                _AcDb.Polyline2d oldPolyline = (_AcDb.Polyline2d)inner;
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
                        if (!AreaEngine.InPoly(vertexPoint, (_AcDb.Entity)outer)) return false;

                    }
                }
                return true;
            }
            else if (inner is _AcDb.Polyline3d)
            {
                _AcDb.Polyline3d poly3d = (_AcDb.Polyline3d)inner;
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
                        if (!AreaEngine.InPoly(vertexPoint, (_AcDb.Entity)outer)) return false;

                    }
                }
                return true;
            }
            throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Unknown entitytype '{0}'!", inner.GetType().Name));
        }

        private static void GetAllFgsAbzsAndRbs(string rbName, string fgLayer, string afLayer, _AcDb.Database db, _AcDb.Transaction tr, List<_AcDb.Entity> fgs, List<_AcDb.Entity> azf, List<_AcDb.Entity> rbs)
        {
            _AcDb.BlockTable bt = (_AcDb.BlockTable)tr.GetObject(db.BlockTableId, _AcDb.OpenMode.ForRead);
            _AcDb.BlockTableRecord btr = (_AcDb.BlockTableRecord)tr.GetObject(bt[_AcDb.BlockTableRecord.ModelSpace], _AcDb.OpenMode.ForRead);
            foreach (_AcDb.ObjectId objId in btr)
            {
                _AcDb.Entity ent = (_AcDb.Entity)tr.GetObject(objId, _AcDb.OpenMode.ForRead);
                var poly = ent as _AcDb.Polyline;
                if (poly != null)
                {
                    if (string.Compare(poly.Layer, fgLayer, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        fgs.Add(ent);
                    }
                    else if (string.Compare(poly.Layer, afLayer, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        azf.Add(ent);
                    }
                }
                else
                {
                    var block = ent as _AcDb.BlockReference;
                    if (block != null)
                    {
                        if (string.Compare(block.Name, rbName, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            rbs.Add(ent);
                        }
                    }
                }
            }
        }

        #endregion

    }
}
