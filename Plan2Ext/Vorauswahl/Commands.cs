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
using Autodesk.AutoCAD.ApplicationServices.Core;
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using Plan2Ext.ObjectFilter;

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
                _AcAp.Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2Vorauswahl aufgetreten! {0}", ex.Message));
            }
        }


        /// <summary>
        /// Command-Line variant
        /// </summary>
        [_AcTrx.CommandMethod("-Plan2Vorauswahl", _AcTrx.CommandFlags.Modal |
                                                       _AcTrx.CommandFlags.UsePickSet |
                                                       _AcTrx.CommandFlags.Redraw)

        ]
        // ReSharper disable once UnusedMember.Global
        public static void Plan2VorauswahCl()
        {
            try
            {
                List<WildcardAcad> blockNamesWildCards;
                List<WildcardAcad> layerNamesWildCards;
                List<Type> entityTypes;
                bool delete;
                if (!GetSelectionInfoViaCmdLine(out blockNamesWildCards, out layerNamesWildCards, out entityTypes, out delete)) return;

                // get also layers from blocks and turn layer on and thaw and unlock
                var blockNames = Globs.GetAllBlockNames()
                    .Where(x => blockNamesWildCards.Any(y => y.IsMatch(x))).ToList();
                var layerNamesForLayerSchalt = Globs.GetAllLayerNames().Where(x => layerNamesWildCards.Any(y => y.IsMatch(x))).ToList();
                AddLayersFromBlockNames(blockNames, layerNamesForLayerSchalt);
                foreach (var layerName in layerNamesForLayerSchalt)
                {
                    Globs.LayerOnAndThaw(layerName, true);
                }

                List<_AcDb.ObjectId> oids = Select(blockNamesWildCards, layerNamesWildCards, entityTypes);
                _AcDb.ObjectId[] ids = new _AcDb.ObjectId[oids.Count];
                oids.CopyTo(ids, 0);
                var doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
                var editor = doc.Editor;
                if (delete)
                {
                    DeleteObjects(ids);
                    editor.WriteMessage("\nAnzahl gelöschter Elemente: " + oids.Count);
                }
                else
                {
                    editor.SetImpliedSelection(ids);
                    editor.WriteMessage("\nAnzahl selektierter Elemente: " + oids.Count);
                }
            }
            catch (Exception ex)
            {
                _AcAp.Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in -Plan2VorauswahlSelect aufgetreten! {0}", ex.Message));
            }
        }

        private static void DeleteObjects(_AcDb.ObjectId[] objectIds)
        {
            using (var transaction =
                _AcAp.Application.DocumentManager.MdiActiveDocument.TransactionManager.StartTransaction())
            {
                foreach (var objectId in objectIds)
                {
                    if (objectId.IsErased) continue;
                    var obj = transaction.GetObject(objectId, _AcDb.OpenMode.ForWrite);
                    obj.Erase(true);
                }
                transaction.Commit();
            }
        }


        /// <summary>
        /// Command-Line variant
        /// </summary>
        [_AcTrx.CommandMethod("-Plan2XrefVorauswahl", _AcTrx.CommandFlags.Modal | _AcTrx.CommandFlags.Redraw)]
        // ReSharper disable once UnusedMember.Global
        public static void Plan2XrefVorauswahCl()
        {
            try
            {
                var doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
                var db = doc.Database;

                List<WildcardAcad> blockNamesWildCards;
                List<WildcardAcad> layerNamesWildCards;
                List<Type> entityTypes;
                if (!GetSelectionInfoViaCmdLine(out blockNamesWildCards, out layerNamesWildCards, out entityTypes)) return;

                var objectFilters = new List<IObjectFilter>();
                objectFilters.AddRange(blockNamesWildCards.Select(x => new BlockNameObjectFilter(x)));
                objectFilters.AddRange(layerNamesWildCards.Select(x => new LayerNameObjectFilter(x)));
                objectFilters.AddRange(entityTypes.Select(x => new TypeObjectFilter(x)));
                var filter = objectFilters.Count > 0 ? new OrObjectFilter(objectFilters) : null;

                var allXrefIds = XrefManager.GetAllXrefsFromCurrentSpace(db).ToArray();
                var allBlockTableIds = BlockManager.InsertXrefsAsBlocks(db, allXrefIds);
                var explodedBlocks = BlockManager.ExplodeBlocks(db, db.CurrentSpaceId, allBlockTableIds, true, false, filter).ToArray();

                var editor = doc.Editor;
                //if (explodedBlocks.Length > 0) editor.SetImpliedSelection(explodedBlocks);
                editor.WriteMessage("\nAnzahl importierter Elemente: " + explodedBlocks.Length);
            }
            catch (Exception ex)
            {
                _AcAp.Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in -Plan2XrefVorauswahl aufgetreten! {0}", ex.Message));
            }
        }


        private static bool GetSelectionInfoViaCmdLine(
            out List<WildcardAcad> blockNamesWildCards,
            out List<WildcardAcad> layerNamesWildCards,
            out List<Type> entityTypes
            )
        {
            blockNamesWildCards = new List<WildcardAcad>();
            layerNamesWildCards = new List<WildcardAcad>();
            entityTypes = new List<Type>();
            var keywords = new[] { "Block", "Layer", "Elementtyp", "Fertig"};
            string keyWord;
            while (!"Fertig".Equals(keyWord = Globs.AskKeywordFromUser("Auswahl", keywords, 3)))
            {
                if (keyWord == null) return false;

                switch (keyWord)
                {
                    case "Block":
                        blockNamesWildCards.AddRange(Globs.GetWildcards("Blocknamen: ", true));
                        break;
                    case "Layer":
                        layerNamesWildCards.AddRange(Globs.GetWildcards("Layernamen: ", true));
                        break;
                    case "Elementtyp":
                        entityTypes.AddRange(Globs.GetEntityTypesWithGermanName("Elementtypen: "));
                        break;
                }
            }

            return true;
        }

        private static bool GetSelectionInfoViaCmdLine(
            out List<WildcardAcad> blockNamesWildCards,
            out List<WildcardAcad> layerNamesWildCards,
            out List<Type> entityTypes, 
            out bool delete)
        {
            blockNamesWildCards = new List<WildcardAcad>();
            layerNamesWildCards = new List<WildcardAcad>();
            entityTypes = new List<Type>();
            delete = false;
            var keywords = new[] {"Block", "Layer", "Elementtyp", "Fertig", "lÖschen"};
            string keyWord;
            while (!"Fertig".Equals(keyWord = Globs.AskKeywordFromUser("Auswahl", keywords, 3)))
            {
                if (keyWord == null) return false;

                switch (keyWord)
                {
                    case "Block":
                        blockNamesWildCards.AddRange(Globs.GetWildcards("Blocknamen: ", true));
                        break;
                    case "Layer":
                        layerNamesWildCards.AddRange(Globs.GetWildcards("Layernamen: ", true));
                        break;
                    case "Elementtyp":
                        entityTypes.AddRange(Globs.GetEntityTypesWithGermanKeyword("Elementtypen: "));
                        break;
                    case "lÖschen":
                        delete = true;
                        break;
                }

                if (delete) break;
            }

            return true;
        }

        [_AcTrx.CommandMethod("Plan2VorauswahlSelect", _AcTrx.CommandFlags.Modal |
                                                       _AcTrx.CommandFlags.UsePickSet |
                                                       _AcTrx.CommandFlags.Redraw)

        ]
        // ReSharper disable once UnusedMember.Global
        public static void Plan2VorauswahlSelect()
        {
            try
            {
                if (!OpenPalette()) return;

                var layerNames = Palette.LayernamesInList();
                var blockNames = Palette.BlocknamesInList();
                var entityTypes = Palette.EntityTypesInList().ToList();

                var layerNamesForLayerSchalt = layerNames.Select(x => x).ToList();
                AddLayersFromBlockNames(blockNames, layerNamesForLayerSchalt);
                foreach (var lay in layerNamesForLayerSchalt)
                {
                    Globs.LayerOnAndThaw(lay, unlock: true);
                }

                var doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
                var editor = doc.Editor;

                var blockNamesWildCards = blockNames.Select(x => new WildcardAcad(x)).ToList();
                var layerNamesWildCards = layerNames.Select(x => new WildcardAcad(x)).ToList();

                List<_AcDb.ObjectId> oids = Select(blockNamesWildCards, layerNamesWildCards, entityTypes);
                _AcDb.ObjectId[] ids = new _AcDb.ObjectId[oids.Count];
                oids.CopyTo(ids, 0);
                editor.SetImpliedSelection(ids);
                Palette.SetResultTextTo("Anzahl: " + oids.Count);
            }
            catch (Exception ex)
            {
                _AcAp.Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2Vorauswahl aufgetreten! {0}", ex.Message));
            }
        }

        private static List<_AcDb.ObjectId> Select(List<WildcardAcad> blockNamesWildCards, List<WildcardAcad> layerNamesWildCards, List<Type> entityTypes)
        {
            var oids = new List<_AcDb.ObjectId>();
            if (blockNamesWildCards.Count == 0 && layerNamesWildCards.Count == 0 && entityTypes.Count == 0) return oids;
            var doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            using (var trans = db.TransactionManager.StartTransaction())
            {
                var objectsToSelectFrom = GetSelectedObjects();
                if (objectsToSelectFrom == null || objectsToSelectFrom.Length == 0)
                {
                    objectsToSelectFrom = GetAllSelectableObjectsInCurrentSpace();
                }

                foreach (var oid in objectsToSelectFrom)
                {
                    _AcDb.Entity ent = (_AcDb.Entity)trans.GetObject(oid, _AcDb.OpenMode.ForRead);

                    if (layerNamesWildCards.Any(x => x.IsMatch(ent.Layer)))
                    {
                        oids.Add(oid);
                        continue;
                    }

                    if (blockNamesWildCards.Count> 0)
                    {
                        var br = ent as _AcDb.BlockReference;
                        if (br != null)
                        {
                            var blockName = Globs.GetBlockname(br, trans);
                            if (blockNamesWildCards.Any(x => x.IsMatch(blockName)))
                            {
                                oids.Add(oid);
                                continue;
                            }
                        }
                    }

                    if (entityTypes.Count <= 0) continue;
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

        private static _AcDb.ObjectId[] GetAllSelectableObjectsInCurrentSpace()
        {

            var objectIds = new List<_AcDb.ObjectId>();
            var doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
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

                _AcDb.BlockTableRecord btr = (_AcDb.BlockTableRecord)trans.GetObject(db.CurrentSpaceId, _AcDb.OpenMode.ForRead);

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

        // ReSharper disable once UnusedMember.Local
        private static _AcDb.ObjectId[] GetAllSelectableObjectsInModelspace()
        {

            var objectIds = new List<_AcDb.ObjectId>();
            var doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
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
            _AcAp.Document doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
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

                _AcAp.Document doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
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
                        options.AddAllowedClass(typeof(_AcDb.BlockReference), true);
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
                _AcAp.Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2VorauswahlSelBlocknamen aufgetreten! {0}", ex.Message));
            }
        }

        [_AcTrx.CommandMethod("Plan2VorauswahlSelLayer")]
        // ReSharper disable once UnusedMember.Global
        public static void Plan2VorauswahlSelLayer()
        {
            try
            {
                if (!OpenPalette()) return;

                _AcAp.Document doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
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
                _AcAp.Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2VorauswahlSelLayer aufgetreten! {0}", ex.Message));
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

            _AcAp.Document doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            if (doc == null) return false;
            Log.DebugFormat("Dokumentname: {0}.", doc.Name);

            return true;
        }
    }
}
