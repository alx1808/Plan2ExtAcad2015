﻿#if BRX_APP
using Bricscad.ApplicationServices;
using Teigha.DatabaseServices;
using Bricscad.EditorInput;
using Teigha.Runtime;
	
using Teigha.Geometry;
#elif ARX_APP
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

#endif

using System.Collections.Generic;
using System.Linq;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable CommentTypo

// ReSharper disable IdentifierTypo
// ReSharper disable StringLiteralTypo

namespace Plan2Ext
{
    public class Boundings
    {

        #region Testcommands
        //[CommandMethod("MERTEXT", CommandFlags.UsePickSet)]
        // ReSharper disable once UnusedMember.Global
        public void Mertext()
        {
            Document doc =
                Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            var res = ed.GetEntity("Textent: ");
            if (res.Status != PromptStatus.OK) return;




            Transaction tr =
                db.TransactionManager.StartTransaction();

            using (tr)
            {

                var txt = tr.GetObject(res.ObjectId, OpenMode.ForRead) as DBText;
                if (txt != null)
                {
                    tr.CreateBoundingBox(txt);
                }

                tr.Commit();
            }


        }

        [CommandMethod("MER", CommandFlags.UsePickSet)]
        // ReSharper disable once UnusedMember.Global
        public void MinimumEnclosingRectangle()
        {
            //MinimumEnclosingBoundary(false);
            MinimumEnclosingRectangular(forUcs: true, oneBoundPerEnt: false, buffer: 0.0);
        }

        //[CommandMethod("MEC", CommandFlags.UsePickSet)]
        // ReSharper disable once UnusedMember.Global
        public void MinimumEnclosingCircle()
        {
            MinimumEnclosingBoundary();
        }
        #endregion

        #region internal

        private void MinimumEnclosingRectangular(bool forUcs, bool oneBoundPerEnt, double buffer)
        {
            Document doc =
                Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            // Ask user to select entities

            PromptSelectionOptions pso =
                new PromptSelectionOptions
                {
                    MessageForAdding = "\nSelect objects to enclose: ",
                    AllowDuplicates = false,
                    AllowSubSelections = true,
                    RejectObjectsFromNonCurrentSpace = true,
                    RejectObjectsOnLockedLayers = false
                };

            PromptSelectionResult psr = ed.GetSelection(pso);
            if (psr.Status != PromptStatus.OK)
                return;

            // Collect points on the component entities

            var pts = new Point3dCollection();

            Transaction tr =
              db.TransactionManager.StartTransaction();
            using (tr)
            {
                BlockTableRecord btr =
                  (BlockTableRecord)tr.GetObject(
                    db.CurrentSpaceId,
                    OpenMode.ForWrite
                  );

                for (int i = 0; i < psr.Value.Count; i++)
                {
                    Entity ent =
                      (Entity)tr.GetObject(
                        psr.Value[i].ObjectId,
                        OpenMode.ForRead
                      );

                    // Collect the points for each selected entity

                    Point3dCollection entPts = CollectPointsWcs(tr, ent);
                    foreach (Point3d pt in entPts)
                    {
                        /*
                         * Create a DBPoint, for testing purposes
                         *
                        DBPoint dbp = new DBPoint(pt);
                        btr.AppendEntity(dbp);
                        tr.AddNewlyCreatedDBObject(dbp, true);
                         */

                        pts.Add(pt);
                    }

                    // Create a boundary for each entity (if so chosen) or
                    // just once after collecting all the points

                    if (oneBoundPerEnt || i == psr.Value.Count - 1)
                    {
                        try
                        {
                            var wcsPointList = forUcs ? BoundingPointsForCurrentUcs(pts, buffer) : BoundingPointsForWcs(pts, buffer);

                            var wcs2DPointList = wcsPointList.ToList2D();

                            var bnd = CreatePolyline(wcs2DPointList, closed: true);

                            btr.AppendEntity(bnd);
                            tr.AddNewlyCreatedDBObject(bnd, true);
                        }
                        catch
                        {
                            ed.WriteMessage(
                              "\nUnable to calculate enclosing boundary."
                            );
                        }

                        pts.Clear();
                    }
                }

                tr.Commit();
            }
        }

        private void MinimumEnclosingBoundary(bool circularBoundary = true)
        {
            Document doc =
                Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            // Ask user to select entities

            PromptSelectionOptions pso =
                new PromptSelectionOptions
                {
                    MessageForAdding = "\nSelect objects to enclose: ",
                    AllowDuplicates = false,
                    AllowSubSelections = true,
                    RejectObjectsFromNonCurrentSpace = true,
                    RejectObjectsOnLockedLayers = false
                };

            PromptSelectionResult psr = ed.GetSelection(pso);
            if (psr.Status != PromptStatus.OK)
                return;

            bool oneBoundPerEnt = false;

            if (psr.Value.Count > 1)
            {
                PromptKeywordOptions pko =
                    new PromptKeywordOptions(
                        "\nMultiple objects selected: create " +
                        "individual boundaries around each one?"
                    ) {AllowNone = true};
                pko.Keywords.Add("Yes");
                pko.Keywords.Add("No");
                pko.Keywords.Default = "No";

                PromptResult pkr = ed.GetKeywords(pko);
                if (pkr.Status != PromptStatus.OK)
                    return;

                oneBoundPerEnt = (pkr.StringResult == "Yes");
            }

            // There may be a SysVar defining the buffer
            // to add to our radius

            double buffer = 0.0;
            try
            {
                object bufvar =
                  Application.GetSystemVariable(
                    "ENCLOSINGBOUNDARYBUFFER"
                  );
                if (bufvar != null)
                {
                    short bufval = (short)bufvar;
                    buffer = bufval / 100.0;
                }
            }
            catch
            {
                object bufvar =
                  Application.GetSystemVariable("USERI1");
                if (bufvar != null)
                {
                    short bufval = (short)bufvar;
                    buffer = bufval / 100.0;
                }
            }

            // Get the current UCS

            CoordinateSystem3d ucs =
              ed.CurrentUserCoordinateSystem.CoordinateSystem3d;

            // Collect points on the component entities

            Point3dCollection pts = new Point3dCollection();

            Transaction tr =
              db.TransactionManager.StartTransaction();
            using (tr)
            {
                BlockTableRecord btr =
                  (BlockTableRecord)tr.GetObject(
                    db.CurrentSpaceId,
                    OpenMode.ForWrite
                  );

                for (int i = 0; i < psr.Value.Count; i++)
                {
                    Entity ent =
                      (Entity)tr.GetObject(
                        psr.Value[i].ObjectId,
                        OpenMode.ForRead
                      );

                    // Collect the points for each selected entity

                    Point3dCollection entPts = CollectPointsWcs(tr, ent);
                    foreach (Point3d pt in entPts)
                    {
                        /*
                         * Create a DBPoint, for testing purposes
                         *
                        DBPoint dbp = new DBPoint(pt);
                        btr.AppendEntity(dbp);
                        tr.AddNewlyCreatedDBObject(dbp, true);
                         */

                        pts.Add(pt);
                    }

                    // Create a boundary for each entity (if so chosen) or
                    // just once after collecting all the points

                    if (oneBoundPerEnt || i == psr.Value.Count - 1)
                    {
                        try
                        {
                            Entity bnd =
                              (circularBoundary ?
                                CircleFromPoints(pts, ucs, buffer) :
                                RectangleFromPoints(pts, buffer)
                              );
                            btr.AppendEntity(bnd);
                            tr.AddNewlyCreatedDBObject(bnd, true);
                        }
                        catch
                        {
                            ed.WriteMessage(
                              "\nUnable to calculate enclosing boundary."
                            );
                        }

                        pts.Clear();
                    }
                }

                tr.Commit();
            }
        }

        // ReSharper disable once CyclomaticComplexity
        internal static Point3dCollection CollectPointsWcs(
          Transaction tr, Entity ent
        )
        {
            // The collection of points to populate and return

            Point3dCollection pts = new Point3dCollection();

            // We'll start by checking a block reference for
            // attributes, getting their bounds and adding
            // them to the point list. We'll still explode
            // the BlockReference later, to gather points
            // from other geometry, it's just that approach
            // doesn't work for attributes (we only get the
            // AttributeDefinitions, which don't have bounds)

            BlockReference br = ent as BlockReference;
            if (br != null)
            {
                foreach (var arId in br.AttributeCollection)
                {
                    // block in block. attributcollection yields attributereferences
                    var dbText = arId as DBText;
                    if (dbText != null)
                    {
                        ExtractBounds(dbText, pts);
                        continue;
                    }

                    if (!(arId is ObjectId))
                    {
	                    continue;
                    }

                    var aroid = (ObjectId) arId;
                    if (aroid.IsErased) continue;
                    DBObject obj = tr.GetObject(aroid, OpenMode.ForRead);
                    var ar = obj as AttributeReference;
                    if (ar != null)
                    {
                        ExtractBounds(ar, pts);
                    }
                }
            }

            // If we have a curve - other than a polyline, which
            // we will want to explode - we'll get points along
            // its length

            Curve cur = ent as Curve;
            if (cur != null &&
                !(cur is Polyline ||
                  cur is Polyline2d ||
                  cur is Polyline3d))
            {
                // Two points are enough for a line, we'll go with
                // a higher number for other curves

                int segs = (ent is Line ? 2 : 20);

                double param = cur.EndParam - cur.StartParam;
                for (int i = 0; i < segs; i++)
                {
                    try
                    {
                        Point3d pt =
                          cur.GetPointAtParameter(
                            cur.StartParam + (i * param / (segs - 1))
                          );
                        pts.Add(pt);
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }
            else if (ent is DBPoint)
            {
                // Points are easy

                pts.Add(((DBPoint)ent).Position);
            }
            else if (ent is DBText)
            {
                // For DBText we use the same approach as
                // for AttributeReferences

                ExtractBounds((DBText)ent, pts);
            }
            else if (ent is MText)
            {
                // MText is also easy - you get all four corners
                // returned by a function. That said, the points
                // are of the MText's box, so may well be different
                // from the bounds of the actual contents

                MText txt = (MText)ent;
                Point3dCollection pts2 = txt.GetBoundingPoints();
                foreach (Point3d pt in pts2)
                {
                    pts.Add(pt);
                }
            }
            else if (ent is Face)
            {
                Face f = (Face)ent;
                try
                {
                    for (short i = 0; i < 4; i++)
                    {
                        pts.Add(f.GetVertexAt(i));
                    }
                }
                catch
                {
                    // ignored
                }
            }
            else if (ent is Solid)
            {
                Solid sol = (Solid)ent;
                try
                {
                    for (short i = 0; i < 4; i++)
                    {
                        pts.Add(sol.GetPointAt(i));
                    }
                }
                catch
                {
                    // ignored
                }
            }
            else
            {
                // Here's where we attempt to explode other types
                // of object
                DBObjectCollection oc = new DBObjectCollection();
                try
                {
                    if (ent is Hatch) return pts;
                    ent.Explode(oc);
                    if (oc.Count > 0)
                    {
                        foreach (DBObject obj in oc)
                        {
                            Entity ent2 = obj as Entity;
                            if (ent2 != null && ent2.Visible)
                            {
                                foreach (Point3d pt in CollectPointsWcs(tr, ent2))
                                {
                                    pts.Add(pt);
                                }
                            }
                            obj.Dispose();
                        }
                    }
                }
                catch
                {
                    // ignored
                }
            }
            return pts;
        }

        internal static Polyline CreatePolyline(List<Point2d> wcs2DPointList, bool closed)
        {
            var p = new Polyline(4);
            //p.Normal = pl.Normal;
            for (int i = 0; i < wcs2DPointList.Count; i++)
            {
                p.AddVertexAt(i, wcs2DPointList[i], 0, 0, 0);
            }

            p.Closed = closed;
            return p;
        }

        internal static List<Point3d> GetRectanglePointsFromBounding(double buffer, List<Point3d> pts)
        {
            double minX = pts[0].X,
                maxX = minX,
                minY = pts[0].Y,
                maxY = minY;


            for (int i = 1; i < pts.Count; i++)
            {
                var pt = pts[i];
                if (pt.X < minX) minX = pt.X;
                if (pt.X > maxX) maxX = pt.X;
                if (pt.Y < minY) minY = pt.Y;
                if (pt.Y > maxY) maxY = pt.Y;
            }

            double buf = buffer;
                //Math.Min(maxX - minX, maxY - minY) * buffer;

            minX -= buf;
            minY -= buf;
            maxX += buf;
            maxY += buf;

            var ucsPointList = new List<Point3d>
            {
                new Point3d(minX, minY, 0.0),
                new Point3d(minX, maxY, 0.0),
                new Point3d(maxX, maxY, 0.0),
                new Point3d(maxX, minY, 0.0),
            };
            return ucsPointList;
        }
        
        #endregion

        #region private

        private static void ExtractBounds(
          DBText txt, Point3dCollection pts
        )
        {
            // We have a special approach for DBText and
            // AttributeReference objects, as we want to get
            // all four corners of the bounding box, even
            // when the text or the containing block reference
            // is rotated

            if (txt.Bounds.HasValue && txt.Visible)
            {
                // Create a straight version of the text object
                // and copy across all the relevant properties
                // (stopped copying AlignmentPoint, as it would
                // sometimes cause an eNotApplicable error)

                // We'll create the text at the WCS origin
                // with no rotation, so it's easier to use its
                // extents

                DBText txt2 = new DBText
                {
                    Normal = Vector3d.ZAxis,
                    Position = Point3d.Origin,
                    TextString = txt.TextString,
                    TextStyleId = txt.TextStyleId,
                    LineWeight = txt.LineWeight
                };

                // Other properties are copied from the original

                txt2.Thickness = txt2.Thickness;
                txt2.HorizontalMode = txt.HorizontalMode;
                txt2.VerticalMode = txt.VerticalMode;
                txt2.WidthFactor = txt.WidthFactor;
                txt2.Height = txt.Height;
                txt2.IsMirroredInX = txt2.IsMirroredInX;
                txt2.IsMirroredInY = txt2.IsMirroredInY;
                txt2.Oblique = txt.Oblique;

                // Get its bounds if it has them defined
                // (which it should, as the original did)

                if (txt2.Bounds.HasValue)
                {
                    Point3d maxPt = txt2.Bounds.Value.MaxPoint;

                    // Place all four corners of the bounding box
                    // in an array

                    Point2d[] bounds =
                      new[] {Point2d.Origin,new Point2d(0.0, maxPt.Y),new Point2d(maxPt.X, maxPt.Y),new Point2d(maxPt.X, 0.0)};

                    // We're going to get each point's WCS coordinates
                    // using the plane the text is on

                    Plane pl = new Plane(txt.Position, txt.Normal);

                    // Rotate each point and add its WCS location to the
                    // collection

                    foreach (Point2d pt in bounds)
                    {
                        pts.Add(
                          pl.EvaluatePoint(
                            pt.RotateBy(txt.Rotation, Point2d.Origin)
                          )
                        );
                    }
                }
            }
        }

        private Entity RectangleFromPoints(
          Point3dCollection pts, double buffer
        )
        {
            if (pts.Count == 0) return null;

            var wcsPointList = BoundingPointsForCurrentUcs(pts, buffer);

            var wcs2DPointList = wcsPointList.ToList2D();

            var p = CreatePolyline(wcs2DPointList, closed: true);

            return p;
        }

        private static List<Point3d> BoundingPointsForCurrentUcs(Point3dCollection pts, double buffer)
        {
            var ptsUcs = pts.ToList().Select(Globs.TransWcsUcs).ToList();

            var ucsPointList = GetRectanglePointsFromBounding(buffer, ptsUcs);

            var wcsPointList = ucsPointList.Select(Globs.TransUcsWcs).ToList();
            return wcsPointList;
        }

        private static List<Point3d> BoundingPointsForWcs(Point3dCollection pts, double buffer)
        {

            return GetRectanglePointsFromBounding(buffer, pts.ToList());
        }

        #endregion

        #region todo circlepoints

        // todo: not tested yet
        private Entity CircleFromPoints(
          Point3dCollection pts, CoordinateSystem3d ucs, double buffer
        )
        {
            // Get the plane of the UCS

            Plane pl = new Plane(ucs.Origin, ucs.Zaxis);

            // We will project these (possibly 3D) points onto
            // the plane of the current UCS, as that's where
            // we will create our circle

            // Project the points onto it

            List<Point2d> pts2D = new List<Point2d>(pts.Count);
            for (int i = 0; i < pts.Count; i++)
            {
                pts2D.Add(pl.ParameterOf(pts[i]));
            }

            // Assuming we have some points in our list...

            if (pts.Count > 0)
            {
                // We need the center and radius of our circle

                Point2d center;
                double radius;

                // Use our fast approximation algorithm to
                // calculate the center and radius of our
                // circle to within 1% (calling the function
                // with 100 iterations gives 10%, calling it
                // with 10K gives 1%)

                BadoiuClarksonIteration(
                  pts2D, 10000, out center, out radius
                );

                // Get our center point in WCS (on the plane
                // of our UCS)

                Point3d cen3D = pl.EvaluatePoint(center);

                // Create the circle and add it to the drawing

                return new Circle(
                  cen3D, ucs.Zaxis, radius * (1.0 + buffer)
                );
            }
            return null;
        }

        // Algorithm courtesy (and copyright of) Frank Nielsen
        // http://blog.informationgeometry.org/article.php?id=164

        private void BadoiuClarksonIteration(
          List<Point2d> set, int iter,
          out Point2d cen, out double rad
        )
        {
            // Choose any point of the set as the initial
            // circumcenter

            cen = set[0];
            rad = 0;

            for (int i = 0; i < iter; i++)
            {
                int winner = 0;
                double distmax = (cen - set[0]).Length;

                // Maximum distance point

                for (int j = 1; j < set.Count; j++)
                {
                    double dist = (cen - set[j]).Length;
                    if (dist > distmax)
                    {
                        winner = j;
                        distmax = dist;
                    }
                }
                rad = distmax;

                // Update

                cen =
                  new Point2d(
                    cen.X + (1.0 / (i + 1.0)) * (set[winner].X - cen.X),
                    cen.Y + (1.0 / (i + 1.0)) * (set[winner].Y - cen.Y)
                  );
            }
        }
        

        #endregion

    }

}
