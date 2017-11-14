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
using Autodesk.AutoCAD.Windows;
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace Plan2Ext.Vorauswahl
{
    public class Commands
    {
        #region log4net Initialization
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Convert.ToString((typeof(Commands))));
        static Commands()
        {
            if (log4net.LogManager.GetRepository(System.Reflection.Assembly.GetExecutingAssembly()).Configured == false)
            {
                log4net.Config.XmlConfigurator.ConfigureAndWatch(new System.IO.FileInfo(System.IO.Path.Combine(new System.IO.FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).DirectoryName, "_log4net.config")));
            }

        }
        #endregion

        static VorauswahlPalette _Palette;
        internal static VorauswahlPalette Palette { get { return _Palette; } }

        [_AcTrx.CommandMethod("Plan2Vorauswahl")]
        static public void Plan2Vorauswahl()
        {
            try
            {
                if (!OpenPalette()) return;
            }
            catch (System.Exception ex)
            {
                _AcAp.Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2Vorauswahl aufgetreten! {0}", ex.Message));
            }
        }

        [_AcTrx.CommandMethod("Plan2VorauswahlSelect")]
        static public void Plan2VorauswahlSelect()
        {
            try
            {
                if (!OpenPalette()) return;

                var layerNames = _Palette.LayernamesInList();
                var blockNames = _Palette.BlocknamesInList();

                // todo: check new list
                var layerNamesForLayerSchalt = layerNames.Select(x => x).ToList();
                AddLayersFromBlockNames(blockNames, layerNamesForLayerSchalt);
                foreach (var lay in layerNamesForLayerSchalt)
                {
                    Plan2Ext.Globs.LayerOnAndThaw(lay,unlock: true);
                }

                var doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
                var editor = doc.Editor;
                editor.SetImpliedSelection(new _AcDb.ObjectId[0]);

                List<_AcDb.ObjectId> oids = Select(blockNames, layerNames);
                if (oids.Count == 0) return;

                _AcDb.ObjectId[] ids = new _AcDb.ObjectId[oids.Count];
                oids.CopyTo(ids, 0);
                editor.SetImpliedSelection(ids);
            }
            catch (System.Exception ex)
            {
                _AcAp.Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2Vorauswahl aufgetreten! {0}", ex.Message));
            }
        }

        private static List<_AcDb.ObjectId> Select(List<string> blockNames, List<string> layerNames)
        {
            var oids = new List<_AcDb.ObjectId>();
            _AcAp.Document doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            _AcDb.Database db = doc.Database;
            using (var trans = db.TransactionManager.StartTransaction())
            {
                _AcDb.BlockTable bt = (_AcDb.BlockTable)trans.GetObject(db.BlockTableId, _AcDb.OpenMode.ForRead);
                _AcDb.BlockTableRecord btr = (_AcDb.BlockTableRecord)trans.GetObject(bt[_AcDb.BlockTableRecord.ModelSpace], _AcDb.OpenMode.ForRead);

                // Iterate through it, dumping objects
                foreach (_AcDb.ObjectId oid in btr)
                {
                    _AcDb.Entity ent = trans.GetObject(oid, _AcDb.OpenMode.ForRead) as _AcDb.Entity;
                    if (ent != null)
                    {
                        if (layerNames.Contains(ent.Layer))
                        {
                            oids.Add(oid);
                        }
                        else
                        {
                            var br = ent as _AcDb.BlockReference;
                            if (br != null)
                            {
                                var blockName = Plan2Ext.Globs.GetBlockname(br, trans);
                                if (blockNames.Contains(blockName))
                                {
                                    oids.Add(oid);
                                }
                            }
                        }
                    }
                }
                trans.Commit();
            }
            return oids;
        }

        private static void AddLayersFromBlockNames(List<string> blockNames, List<string> layerNames)
        {
            foreach (var bn in blockNames)
            {
                var blockAttributeLayers = Plan2Ext.Globs.GetBlockAttributeLayers(bn);
                foreach (var lay in blockAttributeLayers)
                {
                    if (!layerNames.Contains(lay)) layerNames.Add(lay);
                }
                var blockRefLayers = Plan2Ext.Globs.GetBlockRefLayers(bn);
                foreach (var lay in blockRefLayers)
                {
                    if (!layerNames.Contains(lay)) layerNames.Add(lay);
                }
            }
        }

        [_AcTrx.CommandMethod("Plan2VorauswahlSelBlocknamen")]
        static public void Plan2VorauswahlSelBlocknamen()
        {
            try
            {
                if (!OpenPalette()) return;

                _AcAp.Document doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
                using (_AcAp.DocumentLock m_doclock = doc.LockDocument())
                {
                    _AcAp.DocumentCollection dm = _AcAp.Application.DocumentManager;
                    if (doc == null) return;
                    _AcEd.Editor ed = doc.Editor;
#if NEWSETFOCUS
                    doc.Window.Focus();
#else
                    Autodesk.AutoCAD.Internal.Utils.SetFocusToDwgView(); // previous 2014 AutoCAD - Versions
#endif
                    using (var trans = doc.TransactionManager.StartTransaction())
                    {
                        var blockNamesInList = _Palette.BlocknamesInList();
                        var options = new _AcEd.PromptEntityOptions("\nBlock wählen <Return für beenden>: ");
                        options.SetRejectMessage("\nGewähltes Element ist kein Block.");
                        options.AddAllowedClass(typeof(_AcDb.BlockReference), exactMatch: true);
                        var per = ed.GetEntity(options);
                        var selBlockRefs = new List<_AcDb.BlockReference>();
                        while (per.Status == _AcEd.PromptStatus.OK)
                        {
                            var blockRef = (_AcDb.BlockReference)trans.GetObject(per.ObjectId, _AcDb.OpenMode.ForRead);
                            var blockName = Plan2Ext.Globs.GetBlockname(blockRef, trans);
                            if (!blockNamesInList.Contains(blockName))
                            {
                                blockRef.Highlight();
                                selBlockRefs.Add(blockRef);
                                _Palette.AddBlockNameToList(blockName);
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
            catch (System.Exception ex)
            {
                _AcAp.Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2VorauswahlSelBlocknamen aufgetreten! {0}", ex.Message));
            }
        }

        [_AcTrx.CommandMethod("Plan2VorauswahlSelLayer")]
        static public void Plan2VorauswahlSelLayer()
        {
            try
            {
                if (!OpenPalette()) return;

                _AcAp.Document doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
                using (_AcAp.DocumentLock m_doclock = doc.LockDocument())
                {
                    _AcAp.DocumentCollection dm = _AcAp.Application.DocumentManager;
                    if (doc == null) return;
                    _AcEd.Editor ed = doc.Editor;
#if NEWSETFOCUS
                    doc.Window.Focus();
#else
                    Autodesk.AutoCAD.Internal.Utils.SetFocusToDwgView(); // previous 2014 AutoCAD - Versions
#endif
                    using (var trans = doc.TransactionManager.StartTransaction())
                    {
                        var layerNamesInList = _Palette.LayernamesInList();
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
                                    _Palette.AddLayerNameToList(layerName);
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
            catch (System.Exception ex)
            {
                _AcAp.Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2VorauswahlSelLayer aufgetreten! {0}", ex.Message));
            }
        }

        private static bool OpenPalette()
        {
            if (_Palette == null)
            {
                _Palette = new VorauswahlPalette();
            }

            bool wasOpen = _Palette.Show();
            if (!wasOpen) return false;

            _AcAp.Document doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            if (doc == null) return false;
            log.DebugFormat("Dokumentname: {0}.", doc.Name);

            return true;
        }
    }
}
