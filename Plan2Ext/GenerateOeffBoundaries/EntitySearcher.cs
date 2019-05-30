using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using log4net;
// ReSharper disable StringLiteralTypo
// ReSharper disable IdentifierTypo

namespace Plan2Ext.GenerateOeffBoundaries
{
    internal class EntitySearcher
    {
        #region log4net Initialization
        private static readonly ILog Log = LogManager.GetLogger(Convert.ToString((typeof(EntitySearcher))));
        #endregion

        private readonly IConfigurationHandler _configurationHandler;

        public EntitySearcher(IConfigurationHandler configurationHandler)
        {
            _configurationHandler = configurationHandler;
        }

        public IEnumerable<IBlockInfo> GetInsertPointsInMs()
        {
            Log.Info("GetInsertPointsInMs");
            var blockInfos = new List<IBlockInfo>();
            var doc = Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            using (var transaction = doc.TransactionManager.StartTransaction())
            {
                var blockTable = (BlockTable)transaction.GetObject(db.BlockTableId, OpenMode.ForRead);
                var blockTableRecord = (BlockTableRecord)transaction.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForRead);
                foreach (var oid in blockTableRecord)
                {
                    var blockReference = transaction.GetObject(oid, OpenMode.ForRead) as BlockReference;
                    if (blockReference != null)
                    {
                        if (_configurationHandler.ConfiguredFensterBlockNames.Contains(blockReference.Name))
                        {
                            blockInfos.Add((new BlockInfo(){InsertPoint = blockReference.Position, Type = BlockInfo.BlockType.Fenster}));
                        }
                        else if (_configurationHandler.ConfiguredTuerBlockNames.Contains(blockReference.Name))
                        {
                            blockInfos.Add((new BlockInfo() { InsertPoint = blockReference.Position, Type = BlockInfo.BlockType.Tuer }));
                        }
                    }

                }
                transaction.Commit();
            }

            return blockInfos;
        }

        public IEnumerable<ObjectId> GetInternalPolylineOidsInMs()
        {
            Log.Info("GetInternalPolylineOidsInMs");
            var objectIds = new List<ObjectId>();
            var internalPolylineLayer = _configurationHandler.InternalPolylineLayers;
            if (string.IsNullOrEmpty(internalPolylineLayer.Trim())) return objectIds;
            var wildcardAcad = new WildcardAcad(internalPolylineLayer);

            var doc = Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            using (var transaction = doc.TransactionManager.StartTransaction())
            {
                var blockTable = (BlockTable)transaction.GetObject(db.BlockTableId, OpenMode.ForRead);
                var blockTableRecord = (BlockTableRecord)transaction.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForRead);
                foreach (var oid in blockTableRecord)
                {
                    var polyline = transaction.GetObject(oid, OpenMode.ForRead) as Polyline;
                    //if (polyline != null && string.Compare(polyline.Layer, internalPolylineLayer, StringComparison.OrdinalIgnoreCase) == 0)
                    if (polyline != null && wildcardAcad.IsMatch(polyline.Layer))
                    {
                        objectIds.Add(oid);
                    }

                }
                transaction.Commit();
            }

            return objectIds;
        }

        public IEnumerable<ObjectId> GetNonOeffHatchesInMs()
        {
            var hatches = new List<ObjectId>();
            var doc = Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            using (var transaction = doc.TransactionManager.StartTransaction())
            {
                var blockTable = (BlockTable)transaction.GetObject(db.BlockTableId, OpenMode.ForRead);
                var blockTableRecord = (BlockTableRecord)transaction.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForRead);
                foreach (var oid in blockTableRecord)
                {
                    var hatch = transaction.GetObject(oid, OpenMode.ForRead) as Hatch;
                    if (hatch != null && 
                        string.Compare(hatch.Layer, _configurationHandler.FensterSchraffLayer, StringComparison.OrdinalIgnoreCase) != 0 &&
                        string.Compare(hatch.Layer, _configurationHandler.TuerSchraffLayer, StringComparison.OrdinalIgnoreCase) != 0)
                    {
                        hatches.Add(oid);
                    }

                }
                transaction.Commit();
            }

            return hatches;
        }
    }
}
