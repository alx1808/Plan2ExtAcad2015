using System;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
// ReSharper disable IdentifierTypo
// ReSharper disable StringLiteralTypo
// ReSharper disable CommentTypo

namespace Plan2Ext.AutoIdVergabeOeff
{
    internal class EntitySelector
    {
        private readonly IConfigurationHandler _configurationHandler;

        public EntitySelector(IConfigurationHandler configurationHandler)
        {
            _configurationHandler = configurationHandler;
        }

        public SelectedObjectIds SelectObjectsIds()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            var ed = doc.Editor;
            var filter = new SelectionFilter(new[]
            {

                new TypedValue((int) DxfCode.Operator, "<OR"),

                new TypedValue((int) DxfCode.Operator, "<AND"),
                new TypedValue((int) DxfCode.Start, "*POLYLINE"),
                new TypedValue((int) DxfCode.LayerName, _configurationHandler.ObjectPolygonLayer),
                new TypedValue((int) DxfCode.Operator, "AND>"),

                new TypedValue((int) DxfCode.Operator, "<AND"),
                new TypedValue((int) DxfCode.Start, "INSERT"),
                new TypedValue((int) DxfCode.Operator, "AND>"),

                new TypedValue((int) DxfCode.Operator, "OR>")
            });


            var selOpts = new PromptSelectionOptions {MessageForAdding = "Objekte wählen: "};
            var res = ed.GetSelection(selOpts, filter);
            if (res.Status != PromptStatus.OK) return null;
            var selectedObjectIds = new SelectedObjectIds();
            using (var ss = res.Value)
            {
                var idArray = ss.GetObjectIds();
                using (var transaction = doc.TransactionManager.StartTransaction())
                {
                    try
                    {
                        // ReSharper disable once AccessToDisposedClosure
                        var objects = idArray.Select(x => transaction.GetObject(x, OpenMode.ForRead)).ToArray();
                        selectedObjectIds.FensterIds.AddRange(objects.Where(IsFensterBlock).Select(x => x.ObjectId));
                        // todo: add türen
                        var objectPolygons = objects.Where(x => x is Polyline).Select(x => x.ObjectId).ToArray();
                        if (objectPolygons.Length != 1)
                        {
                            throw new InvalidOperationException("Ungültige Anzahl von ObjektPolygonen: " +
                                                                objectPolygons.Length);
                        }
                        else selectedObjectIds.ObjectPolygonId = objectPolygons[0];
                    }
                    finally
                    {
                        transaction.Commit();
                    }
                }
            }

            return selectedObjectIds;
        }

        private bool IsFensterBlock(DBObject dbObject)
        {
            var blockReference = dbObject as BlockReference;
            if (blockReference == null) return false;
            var fenBlockName = _configurationHandler.ConfiguredFensterBlockNames.FirstOrDefault(x =>
                string.Compare(x, blockReference.Name, StringComparison.OrdinalIgnoreCase) == 0);
            if (fenBlockName == null) return false;
            return true;
        }
    }
}
