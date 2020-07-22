using System;
using System.Collections.Generic;
using System.Linq;

#if BRX_APP
using Bricscad.ApplicationServices;
using Teigha.DatabaseServices;
using Bricscad.EditorInput;
using Bricscad.Runtime;
using Bricscad.Internal;

#elif ARX_APP
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Internal;
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
        public List<BlockInfo> BlockInfos { get; private set; }
        public string ProjectName { get; set; }

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
                        var blockReference = (BlockReference) transaction.GetObject(objectId, OpenMode.ForRead);
                        var attributes = GetAttributes(blockReference, transaction);
                        if (attributes.TryGetValue(rnOptions.Attribname.ToUpper(), out var topNr) && attributes.TryGetValue(rnOptions.ZimmerAttributeName.ToUpper(), out var zimmer) && attributes.TryGetValue(rnOptions.FlaechenAttributName.ToUpper(), out var area))
                        {
                            BlockInfos.Add(new BlockInfo(geschoss, topNr, zimmer, area, rnOptions.Separator));
                        }
                    }
                }
                catch (Exception e)
                {
                    transaction.Abort();
                    throw;
                }

                transaction.Commit();
            }
        }

        private static Dictionary<string, string> GetAttributes(BlockReference blockRef, Transaction transaction)
        {
            var valuePerTag = new Dictionary<string, string>();

                foreach (ObjectId attId in blockRef.AttributeCollection)
                {
                    if (attId.IsErased) continue;
                    var anyAttRef = transaction.GetObject(attId, OpenMode.ForRead) as AttributeReference;
                    if (anyAttRef != null)
                    {
                        valuePerTag[anyAttRef.Tag.ToUpper()] = anyAttRef.TextString;
                    }
                }
            
            return valuePerTag;
        }
    }
}
