using System;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable IdentifierTypo

#if BRX_APP
using Teigha.Colors;
using Teigha.DatabaseServices;
using Bricscad.ApplicationServices;
#elif ARX_APP
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
#endif


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

        public static IEnumerable<string> GetNamesOfNonPlottableLayers(Database db = null)
        {
            var layerNames = new List<string>();
            if (db == null) db = Application.DocumentManager.MdiActiveDocument.Database;
            using (var trans = db.TransactionManager.StartTransaction())
            {
                var layTb = (LayerTable)trans.GetObject(db.LayerTableId, OpenMode.ForRead);
                foreach (var ltrOid in layTb)
                {
                    var ltr = (LayerTableRecord)trans.GetObject(ltrOid, OpenMode.ForRead);
                    if (!ltr.IsPlottable)
                    {
                        layerNames.Add(ltr.Name);
                    }
                }
                trans.Commit();
            }

            return layerNames;
        }

        /// <summary>
        /// Freezes and Offs layer.
        /// </summary>
        /// <param name="layerNames"></param>
        /// <param name="db"></param>
        /// <returns>True if regen is needed</returns>
        public static bool FreezeOff(IEnumerable<string> layerNames, Database db = null)
        {
            if (db == null) db = Application.DocumentManager.MdiActiveDocument.Database;
            var needsRegen = false;
            using (var transaction = db.TransactionManager.StartTransaction())
            {
                var layerTable = (LayerTable)transaction.GetObject(db.LayerTableId, OpenMode.ForRead);
                foreach (var layerName in layerNames)
                {
                    if (!layerTable.Has(layerName)) continue;
                    var oid = layerTable[layerName];
                    var ltr = (LayerTableRecord)transaction.GetObject(oid, OpenMode.ForWrite);
                    ltr.IsOff = true;
                    if (db.Clayer == oid) continue;
                    if (!ltr.IsFrozen)
                    {
                        ltr.IsFrozen = true;
                        needsRegen = true;
                    }
                }

                transaction.Commit();
            }

            return needsRegen;
        }

        /// <summary>
        /// Sets color of layer
        /// </summary>
        /// <param name="layerName"></param>
        /// <param name="col"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public static void SetLayerColor(string layerName, Color col, Database db = null)
        {
            if (db == null) db = Application.DocumentManager.MdiActiveDocument.Database;
            using (var transaction = db.TransactionManager.StartTransaction())
            {
                var layerTable = (LayerTable)transaction.GetObject(db.LayerTableId, OpenMode.ForRead);
                if (layerTable.Has(layerName))
                {
                    var oid = layerTable[layerName];
                    var ltr = (LayerTableRecord)transaction.GetObject(oid, OpenMode.ForWrite);
                    ltr.Color = col;
                }
                transaction.Commit();
            }
        }
    }
}
