using System;
using System.Collections.Generic;
using System.Linq;
using Exception = Teigha.Runtime.Exception;
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
    internal class ExcelExportModel
    {
        private List<BlockInfo> _blockInfos;

        public ExcelExportModel(IEnumerable<ObjectId> rbIds, RnOptions rnOptions, Document doc)
        {
            _blockInfos = new List<BlockInfo>();
            using (var transaction = doc.TransactionManager.StartTransaction())
            {
                try
                {
                    foreach (var objectId in rbIds)
                    {
                        var blockReference = (BlockReference) transaction.GetObject(objectId, OpenMode.ForRead);
                        var attributes = GetAttributes(blockReference, transaction);
                        string topNr, zimmer, area;
                        if (attributes.TryGetValue(rnOptions.Attribname.ToUpper(), out topNr) && attributes.TryGetValue(rnOptions.ZimmerAttributeName.ToUpper(), out zimmer) && attributes.TryGetValue(rnOptions.FlaechenAttributName.ToUpper(), out area))
                        {
                            _blockInfos.Add(new BlockInfo(topNr, zimmer, area, rnOptions.Separator));
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

        public static Dictionary<string, string> GetAttributes(BlockReference blockRef, Transaction transaction)
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
