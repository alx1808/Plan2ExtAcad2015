using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using log4net;
using Exception = System.Exception;
// ReSharper disable StringLiteralTypo
// ReSharper disable IdentifierTypo

namespace Plan2Ext.GenerateOeffBoundaries
{
    public class Commands
    {
        #region log4net Initialization
        private static readonly ILog Log = LogManager.GetLogger(Convert.ToString((typeof(Commands))));
        #endregion

        private const double ZoomWith = 10.0;

        [CommandMethod("Plan2GenerateOeffBoundaries")]
        // ReSharper disable once UnusedMember.Global
        public void Plan2GenerateOeffBoundaries()
        {
            Log.Info("Plan2GenerateOeffBoundaries");
            try
            {
                var entitySearcher = new EntitySearcher();
                var points = entitySearcher.GetInsertPointsInMs().ToArray();
                if (!points.Any())
                {
                    LogInfo("\nEs wurden kein Öffnungsblöcke gefunden.");
                    return;
                }

                foreach (var point3D in points)
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

        private void CreateBoundary(Point3d point3D)
        {
            var pointUcs = Globs.TransWcsUcs(point3D);
            ZoomToPoint(pointUcs);
            CreateBoundaryForPoint(pointUcs);

        }

        private void CreateBoundaryForPoint(Point3d pointUcs)
        {
            var oid = EditorHelper.Entlast();
            Application.DocumentManager.MdiActiveDocument.Editor.Command("_.bhatch", "_P", "_SOLID", pointUcs, "");
            var oid2 = EditorHelper.Entlast();
            // todo: fehlerline
            var ok = (oid != oid2);
        }

        private void ZoomToPoint(Point3d point3D)
        {
            //var ll = point3D.Add(new Vector3d(ZoomWith / -2.0, ZoomWith / -2.0, 0.0));
            //var ur = point3D.Add(new Vector3d(ZoomWith / 2.0, ZoomWith / 2.0, 0.0));

            var ed = Application.DocumentManager.MdiActiveDocument.Editor;
            var view = ed.GetCurrentView();
            //ViewTableRecord view = new ViewTableRecord();
            view.CenterPoint = new Point2d(point3D.X, point3D.Y);
            view.Height = ZoomWith;
            view.Width = ZoomWith;
            ed.SetCurrentView(view);
        }

        private void LogInfo(string msg)
        {
            Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\n" + msg);
        }
    }
}
