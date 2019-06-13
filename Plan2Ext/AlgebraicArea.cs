using System;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace Plan2Ext
{
    internal static class AlgebraicArea
    {
        private static double GetAlgebraicArea(Point2d pt1, Point2d pt2, Point2d pt3)
        {
            return (((pt2.X - pt1.X) * (pt3.Y - pt1.Y)) -
                    ((pt3.X - pt1.X) * (pt2.Y - pt1.Y))) / 2.0;
        }

        private static double GetAlgebraicArea(this CircularArc2d arc)
        {
            double rad = arc.Radius;
            double ang = arc.IsClockWise ? arc.StartAngle - arc.EndAngle : arc.EndAngle - arc.StartAngle;
            return rad * rad * (ang - Math.Sin(ang)) / 2.0;
        }

        private static bool DblEqual(double d1, double d2, double eps)
        {
            return Math.Abs(d1 - d2) < eps;
        }

        private static bool IsDbl0(double d, double eps)
        {
            return DblEqual(d, 0.0, eps);
        }

        public static bool IsClockWise(this Polyline pline)
        {
            return pline.GetAlgebraicArea() < 0.0;
        }

        public static double GetAlgebraicArea(this Polyline pline)
        {
            const double eps = 0.00001;
            CircularArc2d arc = new CircularArc2d();
            double area = 0.0;
            int last = pline.NumberOfVertices - 1;
            Point2d p0 = pline.GetPoint2dAt(0);

            if (!IsDbl0(pline.GetBulgeAt(0),eps))
            {
                area += pline.GetArcSegment2dAt(0).GetAlgebraicArea();
            }

            for (int i = 1; i < last; i++)
            {
                area += GetAlgebraicArea(p0, pline.GetPoint2dAt(i), pline.GetPoint2dAt(i + 1));
                if (!IsDbl0(pline.GetBulgeAt(i), eps))
                {
                    area += pline.GetArcSegment2dAt(i).GetAlgebraicArea();
                    ;
                }
            }

            if ((!IsDbl0(pline.GetBulgeAt(last),eps)) && pline.Closed)
            {
                area += pline.GetArcSegment2dAt(last).GetAlgebraicArea();
            }

            return area;
        }
    }
}
