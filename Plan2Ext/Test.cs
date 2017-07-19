using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//using Autodesk.AutoCAD.ApplicationServices;

//using Autodesk.AutoCAD.DatabaseServices;

//using Autodesk.AutoCAD.EditorInput;

//using Autodesk.AutoCAD.Runtime;

//using Autodesk.AutoCAD.Geometry;

//using Autodesk.AutoCAD.Interop;

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

namespace ZoomZoom
{

    public class Commands
    {

        // Zoom to a window specified by the user



   

        [_AcTrx.CommandMethod("ZW")]
        static public void ZoomWindow()
        {

            _AcAp.Document doc =

              _AcAp.Application.DocumentManager.MdiActiveDocument;

            _AcDb.Database db = doc.Database;

            _AcEd.Editor ed = doc.Editor;


            // Get the window coordinates


            _AcEd.PromptPointOptions ppo =

              new _AcEd.PromptPointOptions(

                "\nSpecify first corner:"

              );


            _AcEd.PromptPointResult ppr =

              ed.GetPoint(ppo);


            if (ppr.Status != _AcEd.PromptStatus.OK)

                return;


            _AcGe.Point3d min = ppr.Value;


            _AcEd.PromptCornerOptions pco =

              new _AcEd.PromptCornerOptions(

                "\nSpecify opposite corner: ",

                ppr.Value

              );


            ppr = ed.GetCorner(pco);


            if (ppr.Status != _AcEd.PromptStatus.OK)

                return;


            _AcGe.Point3d max = ppr.Value;


            // Call out helper function

            // [Change this to ZoomWin2 or WoomWin3 to

            // use different zoom techniques]


            //ZoomWin(ed, min, max);
            Plan2Ext.Globs.Zoom(new _AcGe.Point3d(min.X, min.Y, 0.0), new _AcGe.Point3d(max.X, max.Y, 0), new _AcGe.Point3d(), 1.0);
        }


        // Zoom to the extents of an entity


        [_AcTrx.CommandMethod("ZE")]

        static public void ZoomToEntity()
        {

            _AcAp.Document doc =

              _AcAp.Application.DocumentManager.MdiActiveDocument;

            _AcDb.Database db = doc.Database;

            _AcEd.Editor ed = doc.Editor;


            // Get the entity to which we'll zoom


            _AcEd.PromptEntityOptions peo =

              new _AcEd.PromptEntityOptions(

                "\nSelect an entity:"

              );


            _AcEd.PromptEntityResult per = ed.GetEntity(peo);


            if (per.Status != _AcEd.PromptStatus.OK)

                return;


            // Extract its extents


            _AcDb.Extents3d ext;


            _AcDb.Transaction tr =

              db.TransactionManager.StartTransaction();

            using (tr)
            {

                _AcDb.Entity ent =

                  (_AcDb.Entity)tr.GetObject(

                    per.ObjectId,

                    _AcDb.OpenMode.ForRead

                  );

                ext =

                  ent.GeometricExtents;

                tr.Commit();

            }


            ext.TransformBy(

              ed.CurrentUserCoordinateSystem.Inverse()

            );


            // Call our helper function

            // [Change this to ZoomWin2 or WoomWin3 to

            // use different zoom techniques]


            ZoomWin(ed, ext.MinPoint, ext.MaxPoint);

        }


        // Helper functions to zoom using different techniques


        // Zoom using a view object


        private static void ZoomWin(

          _AcEd.Editor ed, _AcGe.Point3d min, _AcGe.Point3d max

        )
        {

            _AcGe.Point2d min2d = new _AcGe.Point2d(min.X, min.Y);

            _AcGe.Point2d max2d = new _AcGe.Point2d(max.X, max.Y);


            _AcDb.ViewTableRecord view =

              new _AcDb.ViewTableRecord();


            view.CenterPoint =

              min2d + ((max2d - min2d) / 2.0);

            view.Height = max2d.Y - min2d.Y;

            view.Width = max2d.X - min2d.X;

            ed.SetCurrentView(view);

        }


        // Zoom via COM


        private static void ZoomWin2(

          _AcEd.Editor ed, _AcGe.Point3d min, _AcGe.Point3d max

        )
        {

            _AcInt.AcadApplication app =

              (_AcInt.AcadApplication)_AcAp.Application.AcadApplication;


            double[] lower =

              new double[3] { min.X, min.Y, min.Z };

            double[] upper

              = new double[3] { max.X, max.Y, max.Z };


            app.ZoomWindow(lower, upper);

        }


        // Zoom by sending a command


        private static void ZoomWin3(

          _AcEd.Editor ed, _AcGe.Point3d min, _AcGe.Point3d max

        )
        {

            string lower =

              min.ToString().Substring(

                1,

                min.ToString().Length - 2

              );

            string upper =

              max.ToString().Substring(

                1,

                max.ToString().Length - 2

              );


            string cmd =

              "_.ZOOM _W " + lower + " " + upper + " ";


            // Call the command synchronously using COM


            _AcInt.AcadApplication app =

              (_AcInt.AcadApplication)_AcAp.Application.AcadApplication;


            app.ActiveDocument.SendCommand(cmd);


            // Could also use async command calling:

            //ed.Document.SendStringToExecute(

            //  cmd, true, false, true

            //);

        }

    }

}
