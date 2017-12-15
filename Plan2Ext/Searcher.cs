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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plan2Ext
{
    //public class SearcherCommands
    //{
    //    [_AcTrx.CommandMethod("SearcherTest")]
    //    public static void SearcherTest()
    //    {
    //        var searchers = new List<ObjectSearcher> {
    //            new Plan2Ext.Fenster.FensterBlockInfo(),
    //            new Plan2Ext.Fenster.FensterLineInfo(),
    //            new Plan2Ext.Fenster.SturzParaLineInfo()
    //        };
    //        var foundObjects = Searcher.Search(searchers);
    //        var fenBlocks = foundObjects.Select(x => x as Plan2Ext.Fenster.FensterBlockInfo).Where(x => x != null).ToList();
    //        var fenBLines = foundObjects.Select(x => x as Plan2Ext.Fenster.FensterLineInfo).Where(x => x != null).ToList();
    //        var sturzParaLines = foundObjects.Select(x => x as Plan2Ext.Fenster.SturzParaLineInfo).Where(x => x != null).ToList();

    //        const double SEARCH_TOL = 0.5;
    //        int nrSpLinesNotFound = 0;
    //        int nrFbLinesNotFound = 0;

    //        foreach (var fenBlock in fenBlocks)
    //        {
    //            var p = fenBlock.InsertPoint;

    //            var orderedDistsToFenLines = fenBLines.Select(x => new { fbLine = x, dist = Dist2d(p, x.MiddelPoint) }).Where(x => x.dist < SEARCH_TOL).OrderBy(x => x.dist);
    //            var fenLineInfo = orderedDistsToFenLines.FirstOrDefault(x => true);
    //            Plan2Ext.Fenster.FensterLineInfo fbLine = null;
    //            if (fenLineInfo != null)
    //            {
    //                fbLine = fenLineInfo.fbLine;
    //                fenBLines.Remove(fbLine);
    //            }
    //            else
    //            {
    //                var ip = p;
    //                nrFbLinesNotFound++;
    //            }

    //            var orderedDistsToSPLines = sturzParaLines.Select(x => new { spLine = x, dist = Dist2d(p, x.MiddelPoint) }).Where(x => x.dist < SEARCH_TOL).OrderBy(x => x.dist);
    //            var spLineInfo = orderedDistsToSPLines.FirstOrDefault(x => true);
    //            Plan2Ext.Fenster.SturzParaLineInfo spLine = null;
    //            if (spLineInfo != null)
    //            {
    //                spLine = spLineInfo.spLine;
    //                sturzParaLines.Remove(spLine);
    //            }
    //            else
    //            {
    //                nrSpLinesNotFound++;
    //                var ip = p;
    //            }
    //        }
    //    }
    //    public static double Dist2d(_AcGe.Point3d p1, _AcGe.Point3d p2)
    //    {
    //        var distX = p2.X - p1.X;
    //        var distY = p2.Y - p1.Y;
    //        return Math.Sqrt((distX * distX) + (distY * distY));
    //    }
    //}

    internal abstract class ObjectSearcher
    {
        public _AcDb.ObjectId Oid { get; set; }
        public abstract ObjectSearcher GetObject(_AcDb.DBObject dbo, _AcDb.Transaction tr);
    }

    internal static class Searcher
    {
        public static List<ObjectSearcher> Search(List<ObjectSearcher> searchers)
        {
            var lst = new List<ObjectSearcher>();
            _AcAp.Document doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            using (var tr = db.TransactionManager.StartTransaction())
            {
                _AcDb.BlockTable bt = (_AcDb.BlockTable)tr.GetObject(db.BlockTableId, _AcDb.OpenMode.ForRead);
                _AcDb.BlockTableRecord btr = (_AcDb.BlockTableRecord)tr.GetObject(bt[_AcDb.BlockTableRecord.ModelSpace], _AcDb.OpenMode.ForRead);
                foreach (_AcDb.ObjectId oid in btr)
                {
                    var dbo = tr.GetObject(oid, _AcDb.OpenMode.ForRead);
                    foreach (var s in searchers)
                    {
                        var foundObject = s.GetObject(dbo, tr);
                        if (foundObject != null)
                        {
                            lst.Add(foundObject);
                            break;
                        }
                    }
                }
                tr.Commit();
            }
            return lst;
        }
    }
}
