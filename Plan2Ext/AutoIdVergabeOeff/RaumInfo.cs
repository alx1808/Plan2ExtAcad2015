using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

// ReSharper disable IdentifierTypo

namespace Plan2Ext.AutoIdVergabeOeff
{
    internal class RaumInfo
    {

        public enum Status
        {
            None,
            Ok,
            NoRaumblock,
            MoreRaumblock
        }
        public Status TheStatus { get; set; }
        public ObjectId Polygon { get; set; }
        public ObjectId Raumblock { get; set; }

        public RaumInfo(ObjectId polygon, List<ObjectId> raumblocks, List<ITuerInfo> tuerInfos)
        {
            Init();

            Polygon = polygon;

            FindRaumBlock(raumblocks);

            FindTuerInfosViaAussenAtt(tuerInfos);
        }

        private void Init()
        {
            TheStatus = Status.None;
        }

        private void FindRaumBlock(ICollection<ObjectId> raumBlocks)
        {
            var transMan = Application.DocumentManager.MdiActiveDocument.TransactionManager;
            using (var transaction = transMan.StartTransaction())
            {
                var poly = transaction.GetObject(Polygon, OpenMode.ForRead) as Entity;
                if (poly != null)
                {
                    var toRemove = new List<ObjectId>();
                    foreach (var oid in raumBlocks)
                    {
                        var block = transaction.GetObject(oid, OpenMode.ForRead) as BlockReference;
                        if (block == null) continue;
                        if (AreaEngine.InPoly(block.Position, poly))
                        {
                            toRemove.Add(oid);
                        }
                    }

                    foreach (var oid in toRemove)
                    {
                        raumBlocks.Remove(oid);
                    }

                    if (toRemove.Count > 1)
                    {
                        TheStatus = Status.MoreRaumblock;
                    }
                    else if (toRemove.Count == 0)
                    {
                        TheStatus = Status.NoRaumblock;
                    }
                    else
                    {
                        Raumblock = toRemove[0];
                        TheStatus = Status.Ok;
                    }
                }

                transaction.Commit();
            }
        }

        private void FindTuerInfosViaAussenAtt(ICollection<ITuerInfo> tuerInfos)
        {
            var transMan = Application.DocumentManager.MdiActiveDocument.TransactionManager;
            using (var transaction = transMan.StartTransaction())
            {
                Entity poly = transaction.GetObject(Polygon, OpenMode.ForRead) as Entity;
                if (poly != null)
                {
                    var toRemove = new List<ITuerInfo>();
                    foreach (var tuerInfo in tuerInfos)
                    {
                        if (!AreaEngine.InPoly(tuerInfo.AttAussenInsertPoint, poly)) continue;
                        tuerInfo.RaumblockId = Raumblock;
                        toRemove.Add(tuerInfo);
                    }
                    foreach (var oid in toRemove)
                    {
                        tuerInfos.Remove(oid);
                    }
                }

                transaction.Commit();
            }
        }
        public void FindTuerInfosViaInnenAtt(ICollection<ITuerInfo> tuerInfos)
        {
            var transMan = Application.DocumentManager.MdiActiveDocument.TransactionManager;
            using (var transaction = transMan.StartTransaction())
            {
                Entity poly = transaction.GetObject(Polygon, OpenMode.ForRead) as Entity;
                if (poly != null)
                {
                    var toRemove = new List<ITuerInfo>();
                    foreach (var tuerInfo in tuerInfos)
                    {
                        if (!AreaEngine.InPoly(tuerInfo.AttInnenInsertPoint, poly)) continue;
                        tuerInfo.RaumblockId = Raumblock;
                        toRemove.Add(tuerInfo);
                    }
                    foreach (var oid in toRemove)
                    {
                        tuerInfos.Remove(oid);
                    }
                }

                transaction.Commit();
            }
        }
    }
}
