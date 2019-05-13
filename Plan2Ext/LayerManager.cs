using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;

// ReSharper disable IdentifierTypo

namespace Plan2Ext
{
    internal static class LayerManager
    {
        public class LayerTableRecordExt : LayerTableRecord
        {
            private string LineTypeName { get; set; }
            private Transparency TransparencyValue { get; set; }
            private string DescriptionString { get; set; }

            public LayerTableRecordExt(LayerTableRecord ltr, Database db)
            {
                var layerTableRecord = (LayerTableRecord)ltr.Clone();
                LineTypeName = GetLineTypeName(db, ltr);
                Name = layerTableRecord.Name;
                // description doesn't work
                DescriptionString = layerTableRecord.Description;
                IsOff = layerTableRecord.IsOff;
                IsFrozen = layerTableRecord.IsFrozen;
                IsLocked = layerTableRecord.IsLocked;
                IsPlottable = layerTableRecord.IsPlottable;
                Color = layerTableRecord.Color;
                Description = layerTableRecord.Description;
                IsHidden = layerTableRecord.IsHidden;
                LineWeight = layerTableRecord.LineWeight;
                TransparencyValue = layerTableRecord.Transparency;
                LinetypeObjectId = layerTableRecord.LinetypeObjectId;
                ViewportVisibilityDefault = layerTableRecord.ViewportVisibilityDefault;
            }

            public void AssignValues(LayerTableRecord ltr, string curLayer, LinetypeTableRecord[] clonedLineTypeRecords)
            {
                if (string.Compare(curLayer, ltr.Name, StringComparison.OrdinalIgnoreCase) != 0)
                {
                    ltr.IsOff = IsOff;
                    ltr.IsFrozen = IsFrozen;
                }

                ltr.IsLocked = IsLocked;
                ltr.IsPlottable = IsPlottable;
                ltr.Color = Color;
                ltr.Description = DescriptionString;
                ltr.IsHidden = IsHidden;
                ltr.IsPlottable = IsPlottable;
                ltr.LineWeight = LineWeight;
                //ltr.PlotStyleName = PlotStyleName;
                ltr.Transparency = TransparencyValue;
                if (!string.IsNullOrEmpty(LineTypeName))
                {
                    var lineTypeRecord = clonedLineTypeRecords.FirstOrDefault(x => x.Name == LineTypeName);
                    if (lineTypeRecord != null)
                    {
                        ltr.LinetypeObjectId = lineTypeRecord.ObjectId;
                    }
                }

                ltr.ViewportVisibilityDefault = ViewportVisibilityDefault;
            }
        }

        private static string GetLineTypeName(Database db, LayerTableRecord ltr)
        {
            string name;
            using (var transaction = db.TransactionManager.StartTransaction())
            {
                var linetypeTableRecord = (LinetypeTableRecord)transaction.GetObject(ltr.LinetypeObjectId, OpenMode.ForRead);
                name = linetypeTableRecord.Name;
                transaction.Commit();
            }

            return name;
        }

        public static IEnumerable<LayerTableRecordExt> GetClonedLayerTableRecords(Database db)
        {
            var layerStates = new List<LayerTableRecordExt>();
            using (var trans = db.TransactionManager.StartTransaction())
            {
                var layTb = (LayerTable)trans.GetObject(db.LayerTableId, OpenMode.ForRead);
                foreach (var ltrOid in layTb)
                {
                    var ltr = (LayerTableRecord)trans.GetObject(ltrOid, OpenMode.ForRead);
                    layerStates.Add(new LayerTableRecordExt(ltr, db));
                }
                trans.Commit();
            }

            return layerStates;
        }
    }
}
