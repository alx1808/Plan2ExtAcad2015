using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
// ReSharper disable IdentifierTypo
// ReSharper disable CommentTypo

namespace Plan2Ext.BlockInfo
{
    internal interface ILegendInserter
    {
        void InsertLegend(List<string> blocksInProtodwg, HashSet<string> legendBlockNames, string prototypedwgName, Point3d positionWcs, Transaction transaction, double scaleFactor, int nrOfVerticalBlockElements);
    }

    internal class LegendInserter : ILegendInserter
    {
        private double VerticalDistance { get; set; }
        private double HorizontalDistance { get; set; }
        private double FrameOffset { get; set; }
        public LegendInserter(double verticalDistance, double horizontalDistance, double frameOffset)
        {
            VerticalDistance = verticalDistance;
            HorizontalDistance = horizontalDistance;
            FrameOffset = frameOffset;
        }

        public void InsertLegend(List<string> blocksInProtodwg, HashSet<string> legendBlockNames, string prototypedwgName, Point3d positionWcs, Transaction transaction, double scaleFactor, int nrOfVerticalBlockElements)
        {

            var positionUcs = Globs.TransWcsUcs(positionWcs);
            var origPositionY = positionUcs.Y;

            var verticalIncrement = VerticalDistance * -1.0 * scaleFactor;
            var horizontalAddition = HorizontalDistance * scaleFactor;

            var positionUcsX = positionUcs.X;
            var verticalNr = 0;

            var ucsPointList = new List<Point3d>();

            foreach (var legendBlockname in blocksInProtodwg)
            {
                if (legendBlockNames.Contains(legendBlockname))
                {
                    double newOPositionUcsX;
                    if (!InsertLocalOrFromProto(legendBlockname, positionUcs, prototypedwgName, explode: true,
                        transaction: transaction, scaleFactor: scaleFactor, ucsPointList: ucsPointList, newPositionX: out newOPositionUcsX)) continue;
                    if (newOPositionUcsX > positionUcsX) positionUcsX = newOPositionUcsX;
                    verticalNr += 1;
                    // insertpoint up and right
                    if (verticalNr >= nrOfVerticalBlockElements)
                    {
                        verticalNr = 0;
                        positionUcs = new Point3d(positionUcsX + horizontalAddition, origPositionY, 0);
                    }
                    // insertpoint down
                    else positionUcs += new Vector3d(0, verticalIncrement, 0);
                }
            }

            if (ucsPointList.Count <= 0) return;

            // frame
            var framePointsUcs = Boundings.GetRectanglePointsFromBounding(buffer: FrameOffset * scaleFactor, pts: ucsPointList);
            var wcsPointList = framePointsUcs.Select(Globs.TransUcsWcs).ToList();
            var wcs2DPointList = wcsPointList.ToList2D();
            var bnd = Boundings.CreatePolyline(wcs2DPointList, closed: true);
            bnd.Layer = "0";
            var btr = (BlockTableRecord)transaction.GetObject(Application.DocumentManager.MdiActiveDocument.Database.CurrentSpaceId, OpenMode.ForWrite);
            btr.AppendEntity(bnd);
            transaction.AddNewlyCreatedDBObject(bnd, true);

            Document doc = Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;

            // missing blocks in proto as text
            foreach (var legendBlockName in legendBlockNames)
            {
                if (blocksInProtodwg.Contains(legendBlockName)) continue;
                using (var text = new DBText())
                {
                    text.Height = 3.0;
                    text.TextString = legendBlockName;
                    text.Position = positionWcs;
                    text.Layer = "0";
                    var acCurSpaceBlkTblRec = (BlockTableRecord)transaction.GetObject(db.CurrentSpaceId, OpenMode.ForWrite);
                    acCurSpaceBlkTblRec.AppendEntity(text);
                    transaction.AddNewlyCreatedDBObject(text, true);
                }

                positionWcs += new Vector3d(0, VerticalDistance, 0);
            }
        }
        /// <summary>
        /// Inserts block local or from proto
        /// </summary>
        /// <param name="blockName"></param>
        /// <param name="positionUcs"></param>
        /// <param name="dwgName"></param>
        /// <param name="explode"></param>
        /// <param name="transaction"></param>
        /// <param name="scaleFactor"></param>
        /// <param name="ucsPointList"></param>
        /// <param name="newPositionX"></param>
        /// <returns>X-Value of boundary on the right side in UCS</returns>
        private static bool InsertLocalOrFromProto(string blockName, Point3d positionUcs, string dwgName, bool explode, Transaction transaction, double scaleFactor, List<Point3d> ucsPointList, out double newPositionX)
        {
            newPositionX = 0.0;

            Document doc = Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;

            if (!Globs.BlockExists(blockName) && !Globs.InsertFromPrototype(blockName, dwgName)) return false;
            var blockTable = (BlockTable)transaction.GetObject(db.BlockTableId, OpenMode.ForRead);
            var oid = blockTable[blockName];
            using (var bref = new BlockReference(Globs.TransUcsWcs(positionUcs), oid))
            {
                bref.ScaleFactors = new Scale3d(scaleFactor);
                bref.Rotation = Globs.GetUcsDirection();
                var acCurSpaceBlkTblRec = (BlockTableRecord)transaction.GetObject(db.CurrentSpaceId, OpenMode.ForWrite);
                acCurSpaceBlkTblRec.AppendEntity(bref);
                transaction.AddNewlyCreatedDBObject(bref, true);

                var boundingPointsWcs = Boundings.CollectPointsWcs(transaction, bref);
                var ptsUcs = boundingPointsWcs.ToList().Select(Globs.TransWcsUcs).ToList();
                var recPointsUcs = Boundings.GetRectanglePointsFromBounding(buffer: 0.0, pts: ptsUcs);
                ucsPointList.AddRange(recPointsUcs);
                newPositionX = recPointsUcs.Select(x => x.X).Max();

                if (explode)
                {
                    var objs = new DBObjectCollection();
                    bref.Explode(objs);
                    foreach (DBObject obj in objs)
                    {
                        var ent = (Entity)obj;
                        acCurSpaceBlkTblRec.AppendEntity(ent);
                        transaction.AddNewlyCreatedDBObject(ent, true);
                    }
                    bref.UpgradeOpen();
                    bref.Erase();
                }
            }

            return true;
        }

    }
}
