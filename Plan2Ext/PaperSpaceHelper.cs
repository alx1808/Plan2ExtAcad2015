using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.ApplicationServices;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
// ReSharper disable IdentifierTypo
// ReSharper disable StringLiteralTypo
// ReSharper disable CommentTypo

namespace Plan2Ext
{
    internal class PaperSpaceHelper
    {
#if ACAD2015_OR_NEWER
        [DllImport("accore.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "acedTrans")]
#else
        [DllImport("acad.exe", CallingConvention = CallingConvention.Cdecl, EntryPoint = "acedTrans")]
#endif
        static extern int acedTrans(double[] point, IntPtr fromRb, IntPtr toRb, int disp, double[] result);

        internal static void ConvertPaperSpaceCoordinatesToModelSpaceWcs(ObjectId viewportObjectId, List<Autodesk.AutoCAD.Geometry.Point3d> points, List<Autodesk.AutoCAD.Geometry.Point3d> wcsPoints)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Autodesk.AutoCAD.EditorInput.Editor ed = doc.Editor;

            if (!SetActivePaperspaceViewport(viewportObjectId, true)) return;

            // Transform from PS point to MS point
            var rbPsdcs = new ResultBuffer(new TypedValue(5003, 3));
            var rbDcs = new ResultBuffer(new TypedValue(5003, 2));
            var rbWcs = new ResultBuffer(new TypedValue(5003, 0));

            ed.SwitchToModelSpace();
            //using (var vp = (Viewport)ed.CurrentViewportObjectId.Open(OpenMode.ForRead))
            //{
                foreach (var point3D in points)
                {
                    double[] retPoint = { 0, 0, 0 };
                    // translate from from the DCS of Paper Space (PSDCS) RTSHORT=3
                    // to the DCS of the current model space viewport RTSHORT=2
                    acedTrans(point3D.ToArray(), rbPsdcs.UnmanagedObject, rbDcs.UnmanagedObject, 0, retPoint);

                    //translate the DCS of the current model space viewport RTSHORT=2
                    //to the WCS RTSHORT=0
                    acedTrans(retPoint, rbDcs.UnmanagedObject, rbWcs.UnmanagedObject, 0, retPoint);

                    wcsPoints.Add(new Autodesk.AutoCAD.Geometry.Point3d(retPoint));
                }
            //}
            ed.SwitchToPaperSpace();
        }

        private static bool SetActivePaperspaceViewport(ObjectId viewportObjectId, bool backToPaperspace)
        {
            var tilemode = Convert.ToInt32(Application.GetSystemVariable("TILEMODE"));
            if (tilemode != 0) return false;

            Document doc = Application.DocumentManager.MdiActiveDocument;
            var ed = doc.Editor;
            using (var tr = doc.TransactionManager.StartTransaction())
            {
                var theVp = tr.GetObject(viewportObjectId, OpenMode.ForRead) as Viewport;
                if (theVp != null)
                {
                    var theNum = theVp.Number;
                    ed.SwitchToModelSpace();
                    Application.SetSystemVariable("CVPORT", theNum);
                    if (backToPaperspace) ed.SwitchToPaperSpace();
                }

                tr.Commit();
            }
            return true;
        }
    }
}
