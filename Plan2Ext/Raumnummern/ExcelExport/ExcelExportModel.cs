using System;
using System.Collections.Generic;
#if BRX_APP
using Bricscad.ApplicationServices;
using Teigha.DatabaseServices;

#elif ARX_APP
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
#endif


namespace Plan2Ext.Raumnummern.ExcelExport
{
    internal interface IExcelExportModel
    {
        List<BlockInfo> BlockInfos { get; }
        string ProjectName { get; }
    }

    internal class ExcelExportModel : IExcelExportModel
    {
        public List<BlockInfo> BlockInfos { get; }
        public string ProjectName { get; }

        public ExcelExportModel(string projectName)
        {
            ProjectName = projectName;
            BlockInfos = new List<BlockInfo>();
        }

        public void Add(string geschoss, IEnumerable<ObjectId> rbIds, RnOptions rnOptions, Document doc)
        {
            using (var transaction = doc.TransactionManager.StartTransaction())
            {
                try
                {
                    foreach (var objectId in rbIds)
                    {
                        var blockReference = (BlockReference)transaction.GetObject(objectId, OpenMode.ForRead);
                        var blockInfo = new BlockInfo(geschoss, blockReference, transaction, rnOptions);
                        if (blockInfo.Ok) BlockInfos.Add(blockInfo);
                    }
                }
                catch (Exception)
                {
                    transaction.Abort();
                    throw;
                }

                transaction.Commit();
            }
        }

    }
}
