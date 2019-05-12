using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;

// ReSharper disable IdentifierTypo

namespace Plan2Ext
{
    internal class LayerManager
    {
        public class LayerTableRecordExt : LayerTableRecord
        {
            public string LineTypeName { get; set; }
            public Transparency TransparencyValue { get; set; }

            public LayerTableRecordExt(LayerTableRecord ltr, Database db)
            {
                var layerTableRecord = (LayerTableRecord)ltr.Clone();
                LineTypeName = GetLineTypeName(db, ltr);
                Name = layerTableRecord.Name;
                Description = layerTableRecord.Description;
                IsOff = layerTableRecord.IsOff;
                IsFrozen = layerTableRecord.IsFrozen;
                IsLocked = layerTableRecord.IsLocked;
                IsPlottable = layerTableRecord.IsPlottable;
                Color = layerTableRecord.Color;
                Description = layerTableRecord.Description;
                IsHidden = layerTableRecord.IsHidden;
                LineWeight = layerTableRecord.LineWeight;
                //Todo PlotStyleName = LayerTableRecord.PlotStyleName;
                TransparencyValue = layerTableRecord.Transparency;
                LinetypeObjectId = layerTableRecord.LinetypeObjectId;

            }
        }

        public static string GetLineTypeName(Database db, LayerTableRecord ltr)
        {
            string name = string.Empty;
            using (var transaction = db.TransactionManager.StartTransaction())
            {
                var linetypeTableRecord = (LinetypeTableRecord)transaction.GetObject(ltr.LinetypeObjectId, OpenMode.ForRead);
                name = linetypeTableRecord.Name;
                transaction.Commit();
            }

            return name;
        }

        public class LayerState
        {
            public string Name { get; set; }
            public bool Off { get; set; }
            public bool Frozen { get; set; }
            public bool Locked { get; set; }
            public Color Color { get; set; }
            public bool IsPlottable { get; set; }
            // todo:
            //public string LineType { get; set; }
            // lineweight
            // transparency
            // plotstyle
            // newfreeze
            // description

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

        public static IEnumerable<LayerState> GetLayerStates(Database db)
        {
            var layerStates = new List<LayerState>();
            using (var trans = db.TransactionManager.StartTransaction())
            {
                var layTb = (LayerTable)trans.GetObject(db.LayerTableId, OpenMode.ForRead) as LayerTable;
                foreach (var ltrOid in layTb)
                {
                    var ltr = (LayerTableRecord)trans.GetObject(ltrOid, OpenMode.ForRead).Clone();
                    layerStates.Add(new LayerState()
                    {
                        Name = ltr.Name, 
                        Off = ltr.IsOff, 
                        Frozen = ltr.IsFrozen, 
                        Locked = ltr.IsLocked, 
                        IsPlottable = ltr.IsPlottable, 
                        Color = ltr.Color,
                    });
                }
                trans.Commit();
            }

            return layerStates;
        }
        //public static void SetLayerStates(Database db, IEnumerable<LayerState> layersState)
        //{
        //    var curLayer = _AcAp.Application.GetSystemVariable("CLAYER").ToString();

        //    // Start a transaction
        //    using (_AcDb.Transaction trans = db.TransactionManager.StartTransaction())
        //    {
        //        _AcDb.LayerTable layTb = trans.GetObject(db.LayerTableId, _AcDb.OpenMode.ForRead) as _AcDb.LayerTable;
        //        foreach (var ltrOid in layTb)
        //        {
        //            _AcDb.LayerTableRecord ltr = (_AcDb.LayerTableRecord)trans.GetObject(ltrOid, _AcDb.OpenMode.ForRead);

        //            if (string.Compare(curLayer, ltr.Name, StringComparison.OrdinalIgnoreCase) == 0) continue;

        //            LayerState ls;
        //            if (layersState.LayerStates.TryGetValue(ltr.Name.ToUpperInvariant(), out ls))
        //            {
        //                ltr.UpgradeOpen();
        //                ltr.IsOff = ls.Off;
        //                ltr.IsFrozen = ls.Frozen;
        //                ltr.IsLocked = ls.Locked;
        //                ltr.IsPlottable = ls.IsPlottable;
        //            }
        //        }
        //        trans.Commit();
        //    }
        //}

    }
}
