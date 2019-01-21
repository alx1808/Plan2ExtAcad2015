// ReSharper disable CommentTypo
#if BRX_APP
using _AcAp = Bricscad.ApplicationServices;
//using _AcBr = Teigha.BoundaryRepresentation;
using _AcCm = Teigha.Colors;
using _AcDb = Teigha.DatabaseServices;
using _AcEd = Bricscad.EditorInput;
using _AcGe = Teigha.Geometry;
using _AcGi = Teigha.GraphicsInterface;
using _AcGs = Teigha.GraphicsSystem;
using _AcPl = Bricscad.PlottingServices;
using _AcBrx = Bricscad.Runtime;
using _AcTrx = Teigha.Runtime;
using _AcWnd = Bricscad.Windows;
using _AcIntCom = BricscadDb;
using _AcInt = BricscadApp;
using Bricscad.Windows;
#elif ARX_APP
using _AcAp = Autodesk.AutoCAD.ApplicationServices;
using _AcCm = Autodesk.AutoCAD.Colors;
using _AcDb = Autodesk.AutoCAD.DatabaseServices;
using _AcEd = Autodesk.AutoCAD.EditorInput;
using _AcGe = Autodesk.AutoCAD.Geometry;
using _AcGi = Autodesk.AutoCAD.GraphicsInterface;
using _AcGs = Autodesk.AutoCAD.GraphicsSystem;
using _AcPl = Autodesk.AutoCAD.PlottingServices;
using _AcBrx = Autodesk.AutoCAD.Runtime;
using _AcTrx = Autodesk.AutoCAD.Runtime;
using _AcWnd = Autodesk.AutoCAD.Windows;
using _AcIntCom = Autodesk.AutoCAD.Interop.Common;
using _AcInt = Autodesk.AutoCAD.Interop;
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using Autodesk.AutoCAD.ApplicationServices.Core;

// ReSharper disable IdentifierTypo
// ReSharper disable StringLiteralTypo

namespace Plan2Ext.Vorauswahl
{
    // ReSharper disable once UnusedMember.Global
    public class Commands
    {
        #region log4net Initialization
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(Convert.ToString((typeof(Commands))));
        #endregion

        private static VorauswahlPalette Palette { get; set; }

        [_AcTrx.CommandMethod("Plan2Vorauswahl")]
        // ReSharper disable once UnusedMember.Global
        public static void Plan2Vorauswahl()
        {
            try
            {
                OpenPalette();
            }
            catch (Exception ex)
            {
                Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2Vorauswahl aufgetreten! {0}", ex.Message));
            }
        }

        [_AcTrx.CommandMethod("Plan2VorauswahlSelect", _AcBrx.CommandFlags.Modal |
                                                       _AcBrx.CommandFlags.UsePickSet |
                                                       _AcBrx.CommandFlags.Redraw)

        ]
        // ReSharper disable once UnusedMember.Global
        public static void Plan2VorauswahlSelect()
        {
            try
            {
                if (!OpenPalette()) return;

                var layerNames = Palette.LayernamesInList();
                var blockNames = Palette.BlocknamesInList();
                var entityTypes = Palette.EntityTypesInList().ToArray();

                // todo: check new list
                var layerNamesForLayerSchalt = layerNames.Select(x => x).ToList();
                AddLayersFromBlockNames(blockNames, layerNamesForLayerSchalt);
                foreach (var lay in layerNamesForLayerSchalt)
                {
                    Globs.LayerOnAndThaw(lay, unlock: true);
                }

                var doc = Application.DocumentManager.MdiActiveDocument;
                var editor = doc.Editor;

                List<_AcDb.ObjectId> oids = Select(blockNames, layerNames, entityTypes);
                _AcDb.ObjectId[] ids = new _AcDb.ObjectId[oids.Count];
                oids.CopyTo(ids, 0);
                editor.SetImpliedSelection(ids);
                Palette.SetResultTextTo("Anzahl: " + oids.Count);
            }
            catch (Exception ex)
            {
                Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2Vorauswahl aufgetreten! {0}", ex.Message));
            }
        }

        private static List<_AcDb.ObjectId> Select(ICollection<string> blockNames, ICollection<string> layerNames, Type[] entityTypes)
        {
            var oids = new List<_AcDb.ObjectId>();
            if (blockNames.Count == 0 && layerNames.Count == 0 && entityTypes.Length == 0) return oids;
            var doc = Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            using (var trans = db.TransactionManager.StartTransaction())
            {
                var objectsToSelectFrom = GetSelectedObjects();
                if (objectsToSelectFrom == null || objectsToSelectFrom.Length == 0)
                {
                    objectsToSelectFrom = GetAllSelectableObjectsInModelspace();
                }

                foreach (var oid in objectsToSelectFrom)
                {
                    _AcDb.Entity ent = (_AcDb.Entity)trans.GetObject(oid, _AcDb.OpenMode.ForRead);

                    if (layerNames.Contains(ent.Layer))
                    {
                        oids.Add(oid);
                        continue;
                    }

                    if (blockNames.Count > 0)
                    {
                        var br = ent as _AcDb.BlockReference;
                        if (br != null)
                        {
                            var blockName = Globs.GetBlockname(br, trans);
                            if (blockNames.Contains(blockName))
                            {
                                oids.Add(oid);
                                continue;
                            }
                        }
                    }

                    if (entityTypes.Length <= 0) continue;
                    var type = ent.GetType();
                    if (entityTypes.Any(x => x == type))
                    {
                        oids.Add(oid);
                    }
                }
                trans.Commit();
            }
            return oids;
        }

        private static _AcDb.ObjectId[] GetAllSelectableObjectsInModelspace()
        {

            var objectIds = new List<_AcDb.ObjectId>();
            var doc = Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            using (var trans = db.TransactionManager.StartTransaction())
            {
                var frozenLayerIds = new List<_AcDb.ObjectId>();
                _AcDb.LayerTable layTb = (_AcDb.LayerTable)trans.GetObject(db.LayerTableId, _AcDb.OpenMode.ForRead);
                foreach (var ltrOid in layTb)
                {
                    _AcDb.LayerTableRecord ltr =
                        (_AcDb.LayerTableRecord)trans.GetObject(ltrOid, _AcDb.OpenMode.ForRead);
                    if (ltr.IsFrozen) frozenLayerIds.Add(ltrOid);
                }

                _AcDb.BlockTable bt = (_AcDb.BlockTable)trans.GetObject(db.BlockTableId, _AcDb.OpenMode.ForRead);
                _AcDb.BlockTableRecord btr = (_AcDb.BlockTableRecord)trans.GetObject(bt[_AcDb.BlockTableRecord.ModelSpace], _AcDb.OpenMode.ForRead);

                foreach (_AcDb.ObjectId oid in btr)
                {
                    _AcDb.Entity ent = trans.GetObject(oid, _AcDb.OpenMode.ForRead) as _AcDb.Entity;
                    if (ent == null) continue;
                    if (frozenLayerIds.Contains(ent.LayerId)) continue;
                    objectIds.Add(oid);
                }

                trans.Commit();
            }

            return objectIds.ToArray();
        }

        private static _AcDb.ObjectId[] GetSelectedObjects()
        {
            _AcAp.Document doc = Application.DocumentManager.MdiActiveDocument;
            _AcEd.Editor editor = doc.Editor;
            var selectionSet = editor.SelectImplied().Value;
            if (selectionSet == null) return null;

            var objectIds = new List<_AcDb.ObjectId>();
            foreach (_AcEd.SelectedObject selectedObject in selectionSet)
            {
                objectIds.Add(selectedObject.ObjectId);
            }

            return objectIds.ToArray();
        }

        private static void AddLayersFromBlockNames(List<string> blockNames, List<string> layerNames)
        {
            foreach (var bn in blockNames)
            {
                var blockAttributeLayers = Globs.GetBlockAttributeLayers(bn);
                foreach (var lay in blockAttributeLayers)
                {
                    if (!layerNames.Contains(lay)) layerNames.Add(lay);
                }
                var blockRefLayers = Globs.GetBlockRefLayers(bn);
                foreach (var lay in blockRefLayers)
                {
                    if (!layerNames.Contains(lay)) layerNames.Add(lay);
                }
            }
        }

        [_AcTrx.CommandMethod("Plan2VorauswahlSelBlocknamen")]
        // ReSharper disable once UnusedMember.Global
        public static void Plan2VorauswahlSelBlocknamen()
        {
            try
            {
                if (!OpenPalette()) return;

                _AcAp.Document doc = Application.DocumentManager.MdiActiveDocument;
                using (doc.LockDocument())
                {
                    _AcEd.Editor ed = doc.Editor;
#if NEWSETFOCUS
                    doc.Window.Focus();
#else
                    Autodesk.AutoCAD.Internal.Utils.SetFocusToDwgView(); // previous 2014 AutoCAD - Versions
#endif
                    using (var trans = doc.TransactionManager.StartTransaction())
                    {
                        var blockNamesInList = Palette.BlocknamesInList();
                        var options = new _AcEd.PromptEntityOptions("\nBlock wählen <Return für beenden>: ");
                        options.SetRejectMessage("\nGewähltes Element ist kein Block.");
                        options.AddAllowedClass(typeof(_AcDb.BlockReference), exactMatch: true);
                        var per = ed.GetEntity(options);
                        var selBlockRefs = new List<_AcDb.BlockReference>();
                        while (per.Status == _AcEd.PromptStatus.OK)
                        {
                            var blockRef = (_AcDb.BlockReference)trans.GetObject(per.ObjectId, _AcDb.OpenMode.ForRead);
                            var blockName = Globs.GetBlockname(blockRef, trans);
                            if (!blockNamesInList.Contains(blockName))
                            {
                                blockRef.Highlight();
                                selBlockRefs.Add(blockRef);
                                Palette.AddBlockNameToList(blockName);
                                blockNamesInList.Add(blockName);
                            }
                            per = ed.GetEntity(options);
                        }
                        foreach (var br in selBlockRefs)
                        {
                            br.Unhighlight();
                        }
                        trans.Commit();
                    }
                }
            }
            catch (Exception ex)
            {
                Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2VorauswahlSelBlocknamen aufgetreten! {0}", ex.Message));
            }
        }

        [_AcTrx.CommandMethod("Plan2VorauswahlSelLayer")]
        // ReSharper disable once UnusedMember.Global
        public static void Plan2VorauswahlSelLayer()
        {
            try
            {
                if (!OpenPalette()) return;

                _AcAp.Document doc = Application.DocumentManager.MdiActiveDocument;
                using (doc.LockDocument())
                {
                    _AcEd.Editor ed = doc.Editor;
#if NEWSETFOCUS
                    doc.Window.Focus();
#else
                    Autodesk.AutoCAD.Internal.Utils.SetFocusToDwgView(); // previous 2014 AutoCAD - Versions
#endif
                    using (var trans = doc.TransactionManager.StartTransaction())
                    {
                        var layerNamesInList = Palette.LayernamesInList();
                        var options = new _AcEd.PromptEntityOptions("\nElement für Layer wählen <Return für beenden>: ");
                        var per = ed.GetEntity(options);
                        var selEnts = new List<_AcDb.Entity>();
                        while (per.Status == _AcEd.PromptStatus.OK)
                        {
                            var ent = trans.GetObject(per.ObjectId, _AcDb.OpenMode.ForRead) as _AcDb.Entity;
                            if (ent != null)
                            {
                                var layerName = ent.Layer;
                                if (!layerNamesInList.Contains(layerName))
                                {
                                    ent.Highlight();
                                    selEnts.Add(ent);
                                    Palette.AddLayerNameToList(layerName);
                                    layerNamesInList.Add(layerName);
                                }
                            }
                            per = ed.GetEntity(options);
                        }
                        foreach (var br in selEnts)
                        {
                            br.Unhighlight();
                        }
                        trans.Commit();
                    }
                }
            }
            catch (Exception ex)
            {
                Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2VorauswahlSelLayer aufgetreten! {0}", ex.Message));
            }
        }

        private static bool OpenPalette()
        {
            if (Palette == null)
            {
                Palette = new VorauswahlPalette();
            }

            bool wasOpen = Palette.Show();
            if (!wasOpen) return false;

            _AcAp.Document doc = Application.DocumentManager.MdiActiveDocument;
            if (doc == null) return false;
            Log.DebugFormat("Dokumentname: {0}.", doc.Name);

            return true;
        }
    }
}
