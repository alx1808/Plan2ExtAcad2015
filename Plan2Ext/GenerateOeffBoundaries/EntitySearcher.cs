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
    }
}
