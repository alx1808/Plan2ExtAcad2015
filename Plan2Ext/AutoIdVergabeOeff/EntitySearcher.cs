using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using log4net;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
// ReSharper disable IdentifierTypo
// ReSharper disable StringLiteralTypo

namespace Plan2Ext.AutoIdVergabeOeff
{
    internal class EntitySearcher
    {
        private readonly IConfigurationHandler _configurationHandler;

        #region log4net Initialization
        private static readonly ILog Log = LogManager.GetLogger(Convert.ToString((typeof(GenerateOeffBoundaries.EntitySearcher))));
        #endregion

        public EntitySearcher(IConfigurationHandler configurationHandler)
        {
            _configurationHandler = configurationHandler;
        }

        public IEnumerable<IFensterInfo> GetFensterInfosInMs(IEnumerable<ObjectId> fensterIds, ObjectId objectPolygonId)
        {
            Log.Info("GetFensterInfosInMs");
            var fensterInfos = new List<IFensterInfo>();
            var doc = Application.DocumentManager.MdiActiveDocument;
            using (var transaction = doc.TransactionManager.StartTransaction())
            {
                var objectPolygon = (Polyline) transaction.GetObject(objectPolygonId, OpenMode.ForRead);
                var blockReferences =
                    // ReSharper disable once AccessToDisposedClosure
                    fensterIds.Select(x => (BlockReference) transaction.GetObject(x, OpenMode.ForRead));
                // ReSharper disable once AccessToDisposedClosure
                var fenAndPos = blockReferences.Select(x => new {fen = x, position = CreateFensterAttPositions(x, transaction)});
                foreach (var fenAndPo in fenAndPos)
                {
                    if (AreaEngine.InPoly(fenAndPo.position.Innen, objectPolygon))
                    {
                        if (AreaEngine.InPoly(fenAndPo.position.Aussen, objectPolygon))
                        {
                            fensterInfos.Add(new FensterInfo(){Oid = fenAndPo.fen.ObjectId, Kind =  FensterInfo.KindEnum.InsidePolygon, InsertPoint = fenAndPo.fen.Position});
                        }
                        else fensterInfos.Add(new FensterInfo() { Oid = fenAndPo.fen.ObjectId, Kind = FensterInfo.KindEnum.OnPolygon, InsertPoint = fenAndPo.fen.Position });
                    }
                    else if (AreaEngine.InPoly(fenAndPo.position.Aussen, objectPolygon))
                    {
                        fensterInfos.Add(new FensterInfo() { Oid = fenAndPo.fen.ObjectId, Kind = FensterInfo.KindEnum.OnPolygon, InsertPoint = fenAndPo.fen.Position });
                    }
                    else fensterInfos.Add(new FensterInfo() { Oid = fenAndPo.fen.ObjectId, Kind = FensterInfo.KindEnum.OutsidePolygon, InsertPoint = fenAndPo.fen.Position });
                }
                transaction.Commit();
            }

            return fensterInfos;
        }

        public IEnumerable<ITuerInfo> GetTuerInfosInMs(IEnumerable<ObjectId> raumBlockIds, IEnumerable<ObjectId> flaGrenzIds, IEnumerable<ObjectId> tuerIds, ObjectId objectPolygonId)
        {
            Log.Info("GetTuerInfosInMs");

            // ReSharper disable once CollectionNeverQueried.Local
            var raumInfos = new List<RaumInfo>();

            var raumBlockIdsList = new List<ObjectId>();
            raumBlockIdsList.AddRange(raumBlockIds);


            var doc = Application.DocumentManager.MdiActiveDocument;
            ITuerInfo[] tuerInfos;
            using (var transaction = doc.TransactionManager.StartTransaction())
            {
                tuerInfos = GetTuerInfos(tuerIds, transaction).ToArray();
                var restlicheTuerInfos = new List<ITuerInfo>();
                restlicheTuerInfos.AddRange(tuerInfos);
                foreach (var flaGrenzId in flaGrenzIds)
                {
                    raumInfos.Add(new RaumInfo(flaGrenzId, raumBlockIdsList, restlicheTuerInfos));
                }

                if (restlicheTuerInfos.Count > 0)
                {
                    foreach (var raumInfo in raumInfos)
                    {
                        raumInfo.FindTuerInfosViaInnenAtt(restlicheTuerInfos);
                    }
                }

                transaction.Commit();
            }

            return tuerInfos;
        }

        private IEnumerable<ITuerInfo> GetTuerInfos(IEnumerable<ObjectId> tuerIds, Transaction transaction)
        {
            var tuerinfos = new List<ITuerInfo>();
            var tuerObjs = tuerIds.Select(x => (BlockReference)transaction.GetObject(x, OpenMode.ForRead)).ToArray();
            foreach (var tuerObj in tuerObjs)
            {
                var tuerAttPositions = new TuerAttPositions(tuerObj, transaction, _configurationHandler);
                tuerinfos.Add(new TuerInfo(){InsertPoint = tuerObj.Position, AttAussenInsertPoint = tuerAttPositions.Aussen, AttInnenInsertPoint  =  tuerAttPositions.Innen, Oid = tuerObj.ObjectId, Handle = tuerObj.Handle.ToString()});
            }

            return tuerinfos;
        }

        private IFensterAttPositions CreateFensterAttPositions(BlockReference x, Transaction transaction)
        {
            return new FensterAttPositions(x, transaction, _configurationHandler);
        }
    }
}
