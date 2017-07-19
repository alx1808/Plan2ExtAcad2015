using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

namespace Plan2Ext
{
    internal static class Extensions
    {

        // Entity extension
        public static _AcGe.Point3d? GetCenter(this _AcDb.Entity ent)
        {
            var cp = new _AcGe.Point3d?();
            if (ent == null) return cp;
            if (!ent.Bounds.HasValue) return cp;

            return GetCenter(ent.Bounds.Value.MinPoint, ent.Bounds.Value.MaxPoint);

        }

        private static _AcGe.Point3d? GetCenter(_AcGe.Point3d p1, _AcGe.Point3d p2)
        {
            return new _AcGe.Point3d((p1[0] + p2[0]) / 2.0,
                (p1[1] + p2[1]) / 2.0,
                (p1[2] + p2[2]) / 2.0);
        }

        ///<summary>
        /// Projects the provided Point3d onto the specified coordinate system.
        ///</summary>
        ///<param name="ucs">The coordinate system to project onto.</param>
        ///<returns>A Point2d projection on the plane of the
        /// coordinate system.</returns>

        public static _AcGe.Point2d ProjectToUcs(this _AcGe.Point3d pt, _AcGe.CoordinateSystem3d ucs)
        {

            var pl = new _AcGe.Plane(ucs.Origin, ucs.Zaxis);

            return pl.ParameterOf(pt);

        }

        // DBText extensions



        ///<summary>
        /// Gets the bounds of a DBText object.
        ///</summary>
        ///<param name="fac">Optional multiplier to increase/reduce buffer.</param>
        ///<returns>A collection of points defining the text's extents.</returns>
        public static _AcGe.Point3dCollection ExtractBounds(
          this _AcDb.DBText txt, double fac = 1.0
        )
        {

            var pts = new _AcGe.Point3dCollection();
            if (txt.Bounds.HasValue && txt.Visible)
            {
                // Create a straight version of the text object
                // and copy across all the relevant properties
                // (stopped copying AlignmentPoint, as it would
                // sometimes cause an eNotApplicable error)
                // We'll create the text at the WCS origin
                // with no rotation, so it's easier to use its
                // extents

                var txt2 = new _AcDb.DBText();

                txt2.Normal = _AcGe.Vector3d.ZAxis;
                txt2.Position = _AcGe.Point3d.Origin;

                // Other properties are copied from the original
                txt2.TextString = txt.TextString;
                txt2.TextStyleId = txt.TextStyleId;
                txt2.LineWeight = txt.LineWeight;
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
                    var maxPt = txt2.Bounds.Value.MaxPoint;

                    // Only worry about this single case, for now
                    _AcGe.Matrix3d mat = _AcGe.Matrix3d.Identity;
                    if (txt.Justify == _AcDb.AttachmentPoint.MiddleCenter)
                    {
                        mat = _AcGe.Matrix3d.Displacement((_AcGe.Point3d.Origin - maxPt) * 0.5);
                    }

                    // Place all four corners of the bounding box
                    // in an array

                    double minX, minY, maxX, maxY;
                    if (txt.Justify == _AcDb.AttachmentPoint.MiddleCenter)
                    {
                        minX = -maxPt.X * 0.5 * fac;
                        maxX = maxPt.X * 0.5 * fac;
                        minY = -maxPt.Y * 0.5 * fac;
                        maxY = maxPt.Y * 0.5 * fac;
                    }
                    else
                    {
                        minX = 0;
                        minY = 0;
                        maxX = maxPt.X * fac;
                        maxY = maxPt.Y * fac;
                    }

                    var bounds =
                      new _AcGe.Point2d[] {
                          new _AcGe.Point2d(minX, minY),
              new _AcGe.Point2d(minX, maxY),
             new _AcGe.Point2d(maxX, maxY),
              new _AcGe.Point2d(maxX, minY)
            };

                    // We're going to get each point's WCS coordinates
                    // using the plane the text is on
                    var pl = new _AcGe.Plane(txt.Position, txt.Normal);

                    // Rotate each point and add its WCS location to the
                    // collection
                    foreach (_AcGe.Point2d pt in bounds)
                    {
                        pts.Add(
                          pl.EvaluatePoint(
                            pt.RotateBy(txt.Rotation, _AcGe.Point2d.Origin)
                          )
                        );
                    }
                }
            }
            return pts;
        }

        // Region extensions

        ///<summary>
        /// Returns whether a Region contains a Point3d.
        ///</summary>
        ///<param name="pt">A points to test against the Region.</param>
        ///<returns>A Boolean indicating whether the Region contains
        /// the point.</returns>
        public static bool ContainsPoint(this _AcDb.Region reg, _AcGe.Point3d pt)
        {
            using (var brep = new _AcBr.Brep(reg))
            {
                var pc = new _AcBr.PointContainment();
                using (var brepEnt = brep.GetPointContainment(pt, out pc))
                {
                    return pc != _AcBr.PointContainment.Outside;
                }
            }
        }

        ///<summary>
        /// Returns whether a Region contains a set of Point3ds.
        ///</summary>
        ///<param name="pts">An array of points to test against the Region.</param>
        ///<returns>A Boolean indicating whether the Region contains
        /// all the points.</returns>
        public static bool ContainsPoints(this _AcDb.Region reg, _AcGe.Point3d[] pts)
        {
            using (var brep = new _AcBr.Brep(reg))
            {
                foreach (var pt in pts)
                {
                    var pc = new _AcBr.PointContainment();
                    using (var brepEnt = brep.GetPointContainment(pt, out pc))
                    {
                        if (pc == _AcBr.PointContainment.Outside)
                            return false;
                    }
                }
            }
            return true;
        }


#if !OLDER_THAN_2015

        ///<summary>
        /// Get the centroid of a Region.
        ///</summary>
        ///<param name="cur">An optional curve used to define the region.</param>
        ///<returns>A nullable Point3d containing the centroid of the Region.</returns>
        public static _AcGe.Point3d? GetCentroid(this _AcDb.Region reg, _AcDb.Curve cur = null)
        {
            if (cur == null)
            {
                var idc = new _AcDb.DBObjectCollection();
                reg.Explode(idc);
                if (idc.Count == 0)
                    return null;
                cur = idc[0] as _AcDb.Curve;
            }

            if (cur == null)
                return null;

            var cs = cur.GetPlane().GetCoordinateSystem();
            var o = cs.Origin;
            var x = cs.Xaxis;
            var y = cs.Yaxis;

            var a = reg.AreaProperties(ref o, ref x, ref y);
            var pl = new _AcGe.Plane(o, x, y);

            return pl.EvaluatePoint(a.Centroid);
        }

#endif


        // Database extensions
        ///<summary>
        /// Create a piece of text of a specified size at a specified location.
        ///</summary>
        ///<param name="norm">The normal to the text object.</param>
        ///<param name="pt">The position for the text.</param>
        ///<param name="conts">The contents of the text.</param>
        ///<param name="size">The size of the text.</param>
        public static void CreateText(

          this _AcDb.Database db, _AcGe.Vector3d norm, _AcGe.Point3d pt, string conts, double size

        )
        {
            using (var tr = db.TransactionManager.StartTransaction())
            {
                var ms =
                  tr.GetObject(
                    _AcDb.SymbolUtilityServices.GetBlockModelSpaceId(db),
                    _AcDb.OpenMode.ForWrite

                  ) as _AcDb.BlockTableRecord;

                if (ms != null)
                {
                    var txt = new _AcDb.DBText();
                    txt.Normal = norm;
                    txt.Position = pt;
                    txt.Justify = _AcDb.AttachmentPoint.MiddleCenter;
                    txt.AlignmentPoint = pt;
                    txt.TextString = conts;
                    txt.Height = size;

                    var id = ms.AppendEntity(txt);
                    tr.AddNewlyCreatedDBObject(txt, true);
                }

                tr.Commit();
            }
        }

        // Transaction extensions

        ///<summary>
        /// Create a bounding rectangle around a piece of text (for debugging).
        ///</summary>
        ///<param name="txt">The text object around which to create a box.</param>
        public static void CreateBoundingBox(this _AcDb.Transaction tr, _AcDb.DBText txt)
        {

            var ms =
              tr.GetObject(
                _AcDb.SymbolUtilityServices.GetBlockModelSpaceId(txt.Database),
                _AcDb.OpenMode.ForWrite

              ) as _AcDb.BlockTableRecord;

            if (ms != null)
            {
                var corners = txt.ExtractBounds();
                if (corners.Count >= 4)
                {
                    var doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
                    if (doc == null) return;
                    var ed = doc.Editor;
                    var ucs = ed.CurrentUserCoordinateSystem;
                    var cs = ucs.CoordinateSystem3d;
                    var pl = new _AcDb.Polyline(4);

                    for (int i = 0; i < 4; i++)
                    {

                        pl.AddVertexAt(i, corners[i].ProjectToUcs(cs), 0, 0, 0);

                    }

                    pl.Closed = true;
                    pl.TransformBy(ucs);
                    ms.AppendEntity(pl);
                    tr.AddNewlyCreatedDBObject(pl, true);
                }
            }
        }

        // Int extensions
        // Based on:
        // http://stackoverflow.com/questions/2729752/converting-numbers-in-to-words-c-sharp
        ///<summary>
        /// Return the description of an integer in string form.
        ///</summary>
        ///<returns>The words describing an integer
        /// e.g. "one hundred and twenty-eight."</returns>
        public static string ToWords(this int number)
        {
            if (number == 0)
                return "zero";
            if (number < 0)
                return "minus " + ToWords(Math.Abs(number));
            string words = "";
            if ((number / 1000000) > 0)
            {
                words += ToWords(number / 1000000) + " million ";
                number %= 1000000;
            }
            if ((number / 1000) > 0)
            {
                words += ToWords(number / 1000) + " thousand ";
                number %= 1000;
            }
            if ((number / 100) > 0)
            {
                words += ToWords(number / 100) + " hundred ";
                number %= 100;
            }
            if (number > 0)
            {
                if (words != "")
                    words += "and ";
                var unitsMap =
                  new[] {
            "zero", "one", "two", "three", "four", "five", "six", "seven",
            "eight", "nine", "ten", "eleven", "twelve", "thirteen",
            "fourteen", "fifteen", "sixteen", "seventeen", "eighteen",
            "nineteen"
          };
                var tensMap =
                   new[] {
            "zero", "ten", "twenty", "thirty", "forty", "fifty",
            "sixty", "seventy", "eighty", "ninety"
          };
                if (number < 20)
                    words += unitsMap[number];
                else
                {
                    words += tensMap[number / 10];
                    if ((number % 10) > 0)
                        words += "-" + unitsMap[number % 10];
                }
            }
            return words;
        }
    }
}