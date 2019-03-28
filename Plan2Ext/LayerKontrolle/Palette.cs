using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Windows;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

// ReSharper disable IdentifierTypo
// ReSharper disable StringLiteralTypo

namespace Plan2Ext.LayerKontrolle
{
    public class Palette
    {
        private static PaletteSet _PaletteSet;
        private static LayerKontrolleControl _Control;

        public Palette()
        {
            _Control = new LayerKontrolleControl();
            Application.DocumentManager.DocumentActivated += DocumentManager_DocumentActivated;
        }

        void DocumentManager_DocumentActivated(object sender, DocumentCollectionEventArgs e)
        {
            if (!_PaletteSet.Visible) return;
            if (e.Document == null) _Control.ClearLists();
            else _Control.InitLayers(ignoreSetLayers: true);
        }

        private string CurrentLayerName
        {
            get { return _Control.CurrentLayerName; }
        }

        public void Show()
        {
            if (_PaletteSet == null)
            {
                _PaletteSet = new PaletteSet("LayerKontrolle")
                {
                    Style = PaletteSetStyles.NameEditable |
                            PaletteSetStyles.ShowPropertiesMenu |
                            PaletteSetStyles.ShowAutoHideButton |
                            PaletteSetStyles.ShowCloseButton,
                    MinimumSize = new System.Drawing.Size(170, 164),
                    KeepFocus = false

                };
                _PaletteSet.Add("LayerKontrolle", _Control);

                if (!_PaletteSet.Visible)
                {
                    _PaletteSet.Visible = true;
                }
#if ACAD2013_OR_NEWER
                // moved here since bug in acad 2016 and 2017 (must be visible before setting dock)
                // https://forums.autodesk.com/t5/net/paletteset-docking-via-c-not-working-in-autocad-2016/td-p/5568372
                //_PaletteSet.SetSize(new System.Drawing.Size(210, 164));
                _PaletteSet.DockEnabled = DockSides.Left;
                _PaletteSet.Dock = DockSides.Left;
#endif

            }
            else
            {
                if (!_PaletteSet.Visible)
                {
                    _PaletteSet.Visible = true;
                }
            }

        }

        internal void InitLayers(bool ignoreSetLayers)
        {
            _Control.InitLayers(ignoreSetLayers);
        }

        internal void SetLayers()
        {

            var doc = Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            using (var trans = doc.TransactionManager.StartTransaction())
            {
                var layTb = (LayerTable)trans.GetObject(db.LayerTableId, OpenMode.ForRead);
                foreach (var ltrOid in layTb)
                {
                    bool dontFreeze = (ltrOid == db.Clayer);
                    var ltr = (LayerTableRecord)trans.GetObject(ltrOid, OpenMode.ForRead);
                    var name = ltr.Name;
                    var on = IsAlwaysOn(name) || (name == CurrentLayerName);
                    SetLayer(ltr, !on, dontFreeze);
                }
                trans.Commit();
            }
        }

        internal static void AllLayersOn()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            using (doc.LockDocument())
            {
#if NEWSETFOCUS
                doc.Window.Focus();
#else
                Autodesk.AutoCAD.Internal.Utils.SetFocusToDwgView(); // previous 2014 AutoCAD - Versions
#endif
                Globs.LayerOn(".*");
            }
        }

        internal static void SelectAllVariableEntitiesInModelSpace()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
#if NEWSETFOCUS
            doc.Window.Focus();
#else
                Autodesk.AutoCAD.Internal.Utils.SetFocusToDwgView(); // previous 2014 AutoCAD - Versions
#endif
            using (doc.LockDocument())
            {
                var db = doc.Database;
                var listOfVariableEntityOids = new List<ObjectId>();
                using (var transaction = doc.TransactionManager.StartTransaction())
                {
                    var blockTable = (BlockTable) transaction.GetObject(db.BlockTableId, OpenMode.ForRead);
                    var blockTableRecord =
                        (BlockTableRecord) transaction.GetObject(blockTable[BlockTableRecord.ModelSpace],
                            OpenMode.ForRead);

                    foreach (var oid in blockTableRecord)
                    {
                        var entity = transaction.GetObject(oid, OpenMode.ForRead) as Entity;
                        if (IsVariable(entity))
                        {
                            listOfVariableEntityOids.Add(oid);
                        }
                    }
                    transaction.Commit();
                }

                doc.Editor.SetImpliedSelection(new ObjectId[0]);

                if (listOfVariableEntityOids.Count <= 0)
                {
                    doc.Editor.WriteMessage("\nKein Entity gefunden.");
                    return;
                }
                Globs.LayerOn(".*");
                doc.Editor.SetImpliedSelection(listOfVariableEntityOids.ToArray());
                doc.Editor.WriteMessage("\nAnzahl gefundener Entities: " + listOfVariableEntityOids.Count);
            }
        }

        private static bool IsVariable(Entity entity)
        {
            if (!entity.EntityColor.IsByLayer) return true;
            if (entity.Linetype != "ByLayer") return true;
            if (entity.LineWeight != LineWeight.ByLayer) return true;
            return false;
        }

        private void SetLayer(LayerTableRecord ltr, bool off, bool dontFreeze)
        {
            if (!ltr.IsFrozen && ltr.IsOff == off) return;

            ltr.UpgradeOpen();
            if (!dontFreeze) ltr.IsFrozen = false;
            ltr.IsOff = off;
        }

        public void AddAlwaysOnLayer(string entityLayer)
        {
            _Control.AddAlwaysOnLayer(entityLayer);
        }

        private bool IsAlwaysOn(string name)
        {
            return _Control.IsAlwaysOn(name);
        }

        internal enum EntityPropertyMode
        {
            ByLayer,
            Variabel,
        };

        internal static void GetEntityTypesForLayer(string layerName, Dictionary<Type, int> entityTypesDictionary, 
            out  EntityPropertyMode colorPropertyMode,
            out  EntityPropertyMode lineTypePropertyMode,
            out EntityPropertyMode lineWeightPropertyMode)
        {
            colorPropertyMode = EntityPropertyMode.ByLayer;
            lineTypePropertyMode = EntityPropertyMode.ByLayer;
            lineWeightPropertyMode = EntityPropertyMode.ByLayer;
            var doc = Application.DocumentManager.MdiActiveDocument;
            using (doc.LockDocument())
            {
                using (var transaction = doc.TransactionManager.StartTransaction())
                {
                    var blockTable = (BlockTable)transaction.GetObject(doc.Database.BlockTableId, OpenMode.ForRead);
                    foreach (var blockTableRecordOid in blockTable)
                    {
                        var blockTableRecord =
                            (BlockTableRecord) transaction.GetObject(blockTableRecordOid, OpenMode.ForRead);
                        foreach (var oid in blockTableRecord)
                        {
                            var entity = transaction.GetObject(oid, OpenMode.ForRead) as Entity;
                            if (entity == null) continue;
                            if (!entity.Layer.Equals(layerName)) continue;

                            if (!entity.EntityColor.IsByLayer) colorPropertyMode = EntityPropertyMode.Variabel;
                            if (entity.Linetype != "ByLayer") lineTypePropertyMode = EntityPropertyMode.Variabel;
                            if (entity.LineWeight != LineWeight.ByLayer)
                                lineWeightPropertyMode = EntityPropertyMode.Variabel;
                            

                            var type = entity.GetType();
                            int cnt;
                            if (entityTypesDictionary.TryGetValue(type, out cnt))
                            {
                                entityTypesDictionary[type] = cnt + 1;
                            }
                            else entityTypesDictionary.Add(type, 1);
                        }
                    }
                    transaction.Commit();
                }
            }
        }
    }
}
