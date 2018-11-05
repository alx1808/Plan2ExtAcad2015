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
#elif ARX_APP
using Autodesk.AutoCAD.ApplicationServices;
using _AcAp = Autodesk.AutoCAD.ApplicationServices;
using _AcBr = Autodesk.AutoCAD.BoundaryRepresentation;
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
using _AcLm = Autodesk.AutoCAD.LayerManager;
using System.Globalization;
using System.Text.RegularExpressions;
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plan2Ext.Kleinbefehle
{
    public class KurzBereinig
    {
        #region log4net Initialization
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Convert.ToString((typeof(KurzBereinig))));
        #endregion

        [_AcTrx.CommandMethod("Plan2KurzBereinigNet")]
        public static void Plan2KurzBereinigNet()
        {
            var doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            _AcEd.Editor ed = _AcAp.Application.DocumentManager.MdiActiveDocument.Editor;
            try
            {
                // Alle Layer tauen, ein, entsperren.
                Plan2Ext.Globs.UnlockAllLayers();
                Plan2Ext.Globs.OnThawAllLayers();

                //In Modelspace: Alle Schrafuren und Körper nach unten legen. (HATCH, SOLID, 3DSOLID)
                _AcAp.Application.SetSystemVariable("CTAB", "MODEL");
                var idsToMove = SelectIdsToMove(doc);
                var ids = new _AcDb.ObjectIdCollection();
                foreach (var oid in idsToMove)
                {
                    ids.Add(oid);
                }
                Plan2Ext.Globs.DrawOrderBottom(ids);

                // Aktiven Layer vorzugsweise auf 0-Layer stellen. Falls dieser den Status "nicht plotten" hat, aktiven Layer auf ersten Layer der Gruppe „nicht plotten“ stellen. Falls es hier keinen gibt, dann Layer 0 als aktuellen Layer erzwingen.
                var layerState = Plan2Ext.Globs.SaveLayersState();
                var allPlottablesLs = layerState.LayerStates.Values.Where(x => x.IsPlottable).ToList();
                var allNotPlottablesLs = layerState.LayerStates.Values.Where(x => !x.IsPlottable).ToList();
                var lsToActivate = layerState.LayerStates["0"];
                if (!lsToActivate.IsPlottable)
                {
                    if (allPlottablesLs.Count > 0)
                    {
                        lsToActivate = allPlottablesLs[0];
                    }
                }
                Plan2Ext.Globs.SetLayerCurrent(lsToActivate.Name);

                // Alle Layer auf „nicht plotten“ aus und frieren.
                foreach (var kvp in layerState.LayerStates)
                {
                    var ls = kvp.Value;
                    if (!ls.IsPlottable)
                    {
                        ls.Frozen = true;
                        ls.Off = true;
                    }
                }
                layerState.CurLayer = lsToActivate.Name;
                Plan2Ext.Globs.RestoreLayersState(layerState);

                // Aufs erstes Layout springen.
                var loNames = Plan2Ext.Layouts.GetOrderedLayoutNames();
                Plan2Ext.Layouts.SetLayoutActive(loNames[1]);
            }
            catch (System.Exception ex)
            {
                string msg = string.Format(CultureInfo.CurrentCulture, "Fehler in (Plan2KurzBereinig): {0}", ex.Message);
                ed.WriteMessage("\n" + msg);
                System.Windows.Forms.MessageBox.Show(ex.Message, "Plan2KurzBereinig");
            }
        }

        [_AcTrx.CommandMethod("Plan2KurzBereinigBulk", _AcTrx.CommandFlags.Session)]
        static public void Plan2KurzBereinigBulk()
        {
            try
            {
                log.Info("----------------------------------------------------------------------------------");
                log.Info("Plan2KurzBereinigBulk");

                List<string> saveNotPossible = new List<string>();

                string dirName = string.Empty;
                string[] dwgFileNames = null;
                if (!Plan2Ext.Globs.GetMultipleFileNames("AutoCAD-Zeichnung", "Dwg", "Verzeichnis mit Zeichnungen für die Kurzbereinigung", "Verzeichnis mit Zeichnungen für die Kurzbereinigung", ref dwgFileNames, ref dirName))
                {
                    return;
                }
                foreach (var fileName in dwgFileNames)
                {
                    Globs.SetReadOnlyAttribute(fileName, false);

                    bool ok = false;

                    log.Info("----------------------------------------------------------------------------------");
                    log.Info(string.Format(CultureInfo.CurrentCulture, "Öffne Zeichnung {0}", fileName));

                    _AcAp.Application.DocumentManager.Open(fileName, false);
                    _AcAp.Document doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
                    _AcDb.Database db = doc.Database;
                    _AcEd.Editor ed = _AcAp.Application.DocumentManager.MdiActiveDocument.Editor;

                    //Lock the new document
                    using (DocumentLock acLckDoc = doc.LockDocument())
                    {
                        // main part
                        // Alle Layer tauen, ein, entsperren.
                        Plan2Ext.Globs.UnlockAllLayers();
                        Plan2Ext.Globs.OnThawAllLayers();

                        //In Modelspace: Alle Schrafuren und Körper nach unten legen. (HATCH, SOLID, 3DSOLID)
                        _AcAp.Application.SetSystemVariable("CTAB", "MODEL");
                        var idsToMove = SelectIdsToMove(doc);
                        var ids = new _AcDb.ObjectIdCollection();
                        foreach (var oid in idsToMove)
                        {
                            ids.Add(oid);
                        }
                        Plan2Ext.Globs.DrawOrderBottom(ids);

                        // Aktiven Layer vorzugsweise auf 0-Layer stellen. Falls dieser den Status "nicht plotten" hat, aktiven Layer auf ersten Layer der Gruppe „nicht plotten“ stellen. Falls es hier keinen gibt, dann Layer 0 als aktuellen Layer erzwingen.
                        var layerState = Plan2Ext.Globs.SaveLayersState();
                        var allPlottablesLs = layerState.LayerStates.Values.Where(x => x.IsPlottable).ToList();
                        var allNotPlottablesLs = layerState.LayerStates.Values.Where(x => !x.IsPlottable).ToList();
                        var lsToActivate = layerState.LayerStates["0"];
                        if (!lsToActivate.IsPlottable)
                        {
                            if (allPlottablesLs.Count > 0)
                            {
                                lsToActivate = allPlottablesLs[0];
                            }
                        }
                        Plan2Ext.Globs.SetLayerCurrent(lsToActivate.Name);

                        // Alle Layer auf „nicht plotten“ aus und frieren.
                        foreach (var kvp in layerState.LayerStates)
                        {
                            var ls = kvp.Value;
                            if (!ls.IsPlottable)
                            {
                                ls.Frozen = true;
                                ls.Off = true;
                            }
                        }
                        layerState.CurLayer = lsToActivate.Name;
                        Plan2Ext.Globs.RestoreLayersState(layerState);

                        // Aufs erstes Layout springen.
                        var loNames = Plan2Ext.Layouts.GetOrderedLayoutNames();
                        Plan2Ext.Layouts.SetLayoutActive(loNames[1]);
                    }

                    log.Info(string.Format(CultureInfo.CurrentCulture, "Zeichnung speichern und schließen: {0}", fileName));
                    try
                    {
                        doc.CloseAndSave(fileName);
                    }
                    catch (System.Exception ex)
                    {
                        log.Warn(string.Format(CultureInfo.CurrentCulture, "Zeichnung konnte nicht gespeichert werden! {0}. {1}", fileName, ex.Message));
                        saveNotPossible.Add(fileName);
                        doc.CloseAndDiscard();
                    }
                }

                if (saveNotPossible.Count > 0)
                {
                    var names = saveNotPossible.Select(x => System.IO.Path.GetFileName(x)).ToArray();
                    var allNames = string.Join(", ", names);

                    System.Windows.Forms.MessageBox.Show(string.Format("Folgende Zeichnungen konnen nicht gespeichert werden: {0}", allNames), "Plan2KurzBereinigBulk");
                }
            }
            catch (System.Exception ex)
            {
                string msg = string.Format(CultureInfo.CurrentCulture, "{0}", ex.Message);
                System.Windows.Forms.MessageBox.Show(msg, "Plan2KurzBereinigBulk");
            }
        }

        private static List<_AcDb.ObjectId> SelectIdsToMove(_AcAp.Document doc)
        {
            var oids = new List<_AcDb.ObjectId>();
            var db = doc.Database;
            using (_AcDb.Transaction tr = doc.TransactionManager.StartTransaction())
            {
                _AcDb.BlockTable bt = (_AcDb.BlockTable)tr.GetObject(db.BlockTableId, _AcDb.OpenMode.ForRead);
                _AcDb.BlockTableRecord btr = (_AcDb.BlockTableRecord)tr.GetObject(_AcDb.SymbolUtilityServices.GetBlockModelSpaceId(db), _AcDb.OpenMode.ForRead);
                foreach (var oid in btr)
                {
                    var obj = tr.GetObject(oid, _AcDb.OpenMode.ForRead);
                    var tp = obj.GetType();
                    if (tp == typeof(_AcDb.Hatch) || tp == typeof(_AcDb.Solid) || tp == typeof(_AcDb.Solid3d))
                    {
                        oids.Add(oid);
                    }
                }
                tr.Commit();
            }
            return oids;
        }
    }
}
