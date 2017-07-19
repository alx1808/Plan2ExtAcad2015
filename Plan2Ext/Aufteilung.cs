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
using _AcLm = Autodesk.AutoCAD.LayerManager;
using System.Globalization;
#endif

//#if ACAD2015_OR_NEWER

namespace Plan2Ext
{
    public class Aufteilung
    {
#if ACAD2015_OR_NEWER

        [_AcTrx.CommandMethod("Plan2AufteilungNet")]
        public static void Plan2AufteilungNet()
        {
            var acadApp = (Autodesk.AutoCAD.Interop.AcadApplication)_AcAp.Application.AcadApplication;

            _AcAp.Document doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            _AcDb.Database db = doc.Database;
            _AcEd.Editor ed = doc.Editor;
            try
            {
                ed.Command("_.LAYER", "_TH", "*", "_ON", "*", "_UN", "*", "");
                var selOp = new _AcEd.PromptSelectionOptions();
                selOp.MessageForAdding = "Zu verschiebende Elemente wählen: ";
                _AcEd.SelectionFilter filter = new _AcEd.SelectionFilter(new _AcDb.TypedValue[] {
                    new _AcDb.TypedValue((int)_AcDb.DxfCode.Operator ,"<NOT"),
                    new _AcDb.TypedValue((int)_AcDb.DxfCode.Operator ,"<AND"),
                    new _AcDb.TypedValue((int)_AcDb.DxfCode.Start ,"*POLYLINE"),
                    new _AcDb.TypedValue((int)_AcDb.DxfCode.LayerName  , "A_AL_MANSFEN"),
                    new _AcDb.TypedValue((int)_AcDb.DxfCode.Operator ,"AND>"),
                    new _AcDb.TypedValue((int)_AcDb.DxfCode.Operator ,"NOT>")
                });
                while (true)
                {
                    var res = ed.GetSelection(selOp, filter);
                    if (res.Status != _AcEd.PromptStatus.OK)
                    {
                        break;
                    }
                    else
                    {
                        var ss = res.Value;
                        var selOpE = new _AcEd.PromptSelectionOptions();
                        _AcDb.ObjectId mf1 = default(_AcDb.ObjectId);
                        if (!GetMansfen("Quell-Mansfen wählen: ", ref mf1)) break;
                        _AcDb.ObjectId mf2 = default(_AcDb.ObjectId);
                        if (!GetMansfen("Ziel-Mansfen wählen: ", ref mf2)) break;

                        if (!SameMansfens(mf1, mf2))
                        {
                            ed.WriteMessage("\nDie gewählten Mansfens sind nicht identisch!");
                            System.Windows.Forms.MessageBox.Show("\nDie gewählten Mansfens sind nicht identisch!", "Plan2AufteilungNet");
                        }
                        else
                        {

                            _AcGe.Point3d fromPoint = GetLuPoint(mf1);
                            _AcGe.Point3d toPoint = GetLuPoint(mf2);

                            string dwgName = doc.Name;
                            var dwgProposal = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(dwgName), System.IO.Path.GetFileNameWithoutExtension(dwgName) + "_X.dwg");
                            _AcWnd.SaveFileDialog sfd = new _AcWnd.SaveFileDialog("Ziel-Zeichnung", dwgProposal, "dwg", "TargetDrawing", _AcWnd.SaveFileDialog.SaveFileDialogFlags.NoFtpSites);
                            System.Windows.Forms.DialogResult dr = sfd.ShowDialog();
                            if (dr == System.Windows.Forms.DialogResult.OK)
                            {

                                var ucs = ed.CurrentUserCoordinateSystem;
                                var fromPointU = Globs.TransWcsUcs(fromPoint); // fromPoint.TransformBy(ucs);
                                var toPointU = Globs.TransWcsUcs(toPoint); // toPoint.TransformBy(ucs);


                                // only acad2015 -
                                ed.Command("_.UNDO", "_M");

                                ed.Command("_.DIMDISASSOCIATE", ss, "");

                                ed.Command("_.MOVE", ss, "", fromPointU, toPointU);
                                //ed.Command("_.MOVE", ss, "", "0,0", "100,100");
                                ed.Command("_.ERASE", "_ALL", "_R", ss, mf2, "");

                                doc.Database.SaveAs(sfd.Filename, false, _AcDb.DwgVersion.Current, doc.Database.SecurityParameters);

                                ed.Command("_.UNDO", "_B");
                                //doc.SendStringToExecute("._UNDO B", true, false, true);
                                // also supports acad2013
                                doc.SendStringToExecute(".'_UNDO M ",true, false, true);
                                //acadApp.ActiveDocument.SendCommand("_.UNDO _M\n");
                                //acadApp.ActiveDocument.SendCommand("_.DIMDISASSOCIATE _P \n");

                            }
                        }

                        Globs.HightLight(mf1, onOff: false);
                        Globs.HightLight(mf2, onOff: false);
                    }
                }
            }
            catch (System.Exception ex)
            {
                string msg = string.Format(CultureInfo.CurrentCulture, "Fehler in (Plan2AufteilungNet): {0}", ex.Message);
                ed.WriteMessage("\n" + msg);
                System.Windows.Forms.MessageBox.Show(ex.Message, "Plan2AufteilungNet");
            }
        }
#else

        //[_AcTrx.LispFunction("C:Plan2AufteilungNet")]
        //public static void Plan2AufteilungNet(_AcDb.ResultBuffer rb)
        //{
        //    var acadApp = (Autodesk.AutoCAD.Interop.AcadApplication)_AcAp.Application.AcadApplication;

        //    _AcAp.Document doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
        //    _AcDb.Database db = doc.Database;
        //    _AcEd.Editor ed = doc.Editor;
        //    try
        //    {
        //        ed.Command("_.LAYER", "_TH", "*", "_ON", "*", "_UN", "*", "");
        //        var selOp = new _AcEd.PromptSelectionOptions();
        //        selOp.MessageForAdding = "Zu verschiebende Elemente wählen: ";
        //        _AcEd.SelectionFilter filter = new _AcEd.SelectionFilter(new _AcDb.TypedValue[] {
        //            new _AcDb.TypedValue((int)_AcDb.DxfCode.Operator ,"<NOT"),
        //            new _AcDb.TypedValue((int)_AcDb.DxfCode.Operator ,"<AND"),
        //            new _AcDb.TypedValue((int)_AcDb.DxfCode.Start ,"*POLYLINE"),
        //            new _AcDb.TypedValue((int)_AcDb.DxfCode.LayerName  , "A_AL_MANSFEN"),
        //            new _AcDb.TypedValue((int)_AcDb.DxfCode.Operator ,"AND>"),
        //            new _AcDb.TypedValue((int)_AcDb.DxfCode.Operator ,"NOT>")
        //        });
        //        while (true)
        //        {
        //            var res = ed.GetSelection(selOp, filter);
        //            if (res.Status != _AcEd.PromptStatus.OK)
        //            {
        //                break;
        //            }
        //            else
        //            {
        //                var ss = res.Value;
        //                var selOpE = new _AcEd.PromptSelectionOptions();
        //                _AcDb.ObjectId mf1 = default(_AcDb.ObjectId);
        //                if (!GetMansfen("Quell-Mansfen wählen: ", ref mf1)) break;
        //                _AcDb.ObjectId mf2 = default(_AcDb.ObjectId);
        //                if (!GetMansfen("Ziel-Mansfen wählen: ", ref mf2)) break;

        //                if (!SameMansfens(mf1, mf2))
        //                {
        //                    ed.WriteMessage("\nDie gewählten Mansfens sind nicht identisch!");
        //                    System.Windows.Forms.MessageBox.Show("\nDie gewählten Mansfens sind nicht identisch!", "Plan2AufteilungNet");
        //                }
        //                else
        //                {

        //                    _AcGe.Point3d fromPoint = GetLuPoint(mf1);
        //                    _AcGe.Point3d toPoint = GetLuPoint(mf2);

        //                    string dwgName = doc.Name;
        //                    var dwgProposal = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(dwgName), System.IO.Path.GetFileNameWithoutExtension(dwgName) + "_X.dwg");
        //                    _AcWnd.SaveFileDialog sfd = new _AcWnd.SaveFileDialog("Ziel-Zeichnung", dwgProposal, "dwg", "TargetDrawing", _AcWnd.SaveFileDialog.SaveFileDialogFlags.NoFtpSites);
        //                    System.Windows.Forms.DialogResult dr = sfd.ShowDialog();
        //                    if (dr == System.Windows.Forms.DialogResult.OK)
        //                    {

        //                        var ucs = ed.CurrentUserCoordinateSystem;
        //                        var fromPointU = Globs.TransWcsUcs(fromPoint); // fromPoint.TransformBy(ucs);
        //                        var toPointU = Globs.TransWcsUcs(toPoint); // toPoint.TransformBy(ucs);



        //                        // only acad2015 -
        //                        //ed.Command("_.UNDO", "_M");

        //                        //ed.Command("_.DIMDISASSOCIATE", ss, "");

        //                        //ed.Command("_.MOVE", ss, "", fromPointU, toPointU);
        //                        ////ed.Command("_.MOVE", ss, "", "0,0", "100,100");
        //                        //ed.Command("_.ERASE", "_ALL", "_R", ss, mf2, "");

        //                        //doc.Database.SaveAs(sfd.Filename, false, _AcDb.DwgVersion.Current, doc.Database.SecurityParameters);

        //                        //ed.Command("_.UNDO", "_B");
        //                        //doc.SendStringToExecute("._UNDO B", true, false, true);
        //                        // also supports acad2013
        //                        //doc.SendStringToExecute(".'_UNDO M ", true, false, true);
        //                        acadApp.ActiveDocument.SendCommand("_.UNDO _M ");
        //                        acadApp.ActiveDocument.SendCommand("_.DIMDISASSOCIATE _P  ");

        //                    }
        //                }

        //                Globs.HightLight(mf1, onOff: false);
        //                Globs.HightLight(mf2, onOff: false);
        //            }
        //        }
        //    }
        //    catch (System.Exception ex)
        //    {
        //        string msg = string.Format(CultureInfo.CurrentCulture, "Fehler in (Plan2AufteilungNet): {0}", ex.Message);
        //        ed.WriteMessage("\n" + msg);
        //        System.Windows.Forms.MessageBox.Show(ex.Message, "Plan2AufteilungNet");
        //    }
        //}
#endif

        private static _AcGe.Point3d GetLuPoint(_AcDb.ObjectId mf1)
        {
            List<_AcGe.Point2d> pts1 = GetLwPoints(mf1);
            var xMin = pts1.Select(x => x.X).Min();
            var yMin = pts1.Select(x => x.Y).Min();

            return new _AcGe.Point3d(xMin, yMin, 0.0);
        }

        private static bool SameMansfens(_AcDb.ObjectId mf1, _AcDb.ObjectId mf2)
        {
            List<_AcGe.Point2d> pts1 = GetLwPoints(mf1);
            List<_AcGe.Point2d> pts2 = GetLwPoints(mf2);

            List<double> minMax1 = GetMinMaxDist(pts1);
            List<double> minMax2 = GetMinMaxDist(pts2);

            if (!DblEqual(minMax1[0], minMax2[0], 0.0000001)) return false;
            if (!DblEqual(minMax1[1], minMax2[1], 0.0000001)) return false;

            return true;
        }

        private static bool DblEqual(double d1, double d2, double eps)
        {
            return (Math.Abs(d1 - d2) <= eps);
        }

        private static List<double> GetMinMaxDist(List<_AcGe.Point2d> pts1)
        {
            double? min = null, max = null;
            var dists = new List<double>();
            for (int i = 1; i < pts1.Count; i++)
            {
                var len = GetDistance(pts1[i], pts1[i - 1]);
                if (!min.HasValue || min.Value > len) min = len;
                if (!max.HasValue || max.Value < len) max = len;
            }
            dists.Add(min.Value);
            dists.Add(max.Value);
            return dists;
        }

        private static double GetDistance(_AcGe.Point2d p1, _AcGe.Point2d p2)
        {
            return Math.Sqrt(Math.Pow(p1.X - p2.X, 2.0) + Math.Pow(p1.Y - p2.Y, 2.0));
        }

        private static List<_AcGe.Point2d> GetLwPoints(_AcDb.ObjectId objectId)
        {
            var pts = new List<_AcGe.Point2d>();

            _AcAp.Document doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            _AcDb.Database db = doc.Database;
            using (var trans = doc.TransactionManager.StartTransaction())
            {
                var lwp = (_AcDb.Polyline)trans.GetObject(objectId, _AcDb.OpenMode.ForRead);
                int vn = lwp.NumberOfVertices;
                for (int i = 0; i < vn; i++)
                {
                    _AcGe.Point2d pt = lwp.GetPoint2dAt(i);
                    pts.Add(pt);
                }

                trans.Commit();
            }
            return pts;
        }

        private static bool GetMansfen(string msg, ref  _AcDb.ObjectId mf)
        {
            _AcAp.Document doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            _AcEd.Editor ed = doc.Editor;

            while (true)
            {
                var res = ed.GetEntity(msg);
                if (res.Status == _AcEd.PromptStatus.Cancel) return false;
                if (res.Status == _AcEd.PromptStatus.OK)
                {
                    if (IsMansfen(res.ObjectId))
                    {
                        mf = res.ObjectId;
                        Globs.HightLight(mf, onOff: true);
                        return true;
                    }
                }
            }
        }

        private static bool IsMansfen(_AcDb.ObjectId objectId)
        {
            _AcAp.Document doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            _AcDb.Database db = doc.Database;

            bool ret = false;

            using (var trans = doc.TransactionManager.StartTransaction())
            {
                var poly = trans.GetObject(objectId, _AcDb.OpenMode.ForRead) as _AcDb.Polyline;
                if (poly != null)
                {
                    if (string.Compare(poly.Layer, "A_AL_MANSFEN", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        ret = true;
                    }
                }
                trans.Commit();
            }

            return ret;
        }
    }
}

//#endif