using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
#endif

#if ARX_APP
namespace Plan2Ext.LayerFilters
{
    public class Commands
    {
        #region PLan2CreateX_NotPlotFilter
        private const string LAYER_FILTER_NAME_X_NICHT_PLOTTEN = "NICHT_PLOTTEN";
        private const string LAYER_FILTER_EXPRESSION_X_NICHT_PLOTTEN = "PLOTTABLE==\"False\" OR NAME==\"X_*\" OR NAME==\"RKV*\"";
        [_AcTrx.CommandMethod("Plan2CreateX_NotPlotFilter")]
        public static void Plan2CreateX_NotPlotFilter()
        {
            try
            {
                _AcAp.Document doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
                _AcEd.Editor ed = doc.Editor;
                _AcDb.Database db = doc.Database;
                _AcLm.LayerFilterTree lft = db.LayerFilters;
                Plan2CreateX_NotPlotFilter(lft, db, ed);
            }
            catch (Exception ex)
            {
                _AcAp.Application.ShowAlertDialog(ex.Message);
            }
        }

        private static void Plan2CreateX_NotPlotFilter(_AcLm.LayerFilterTree lft, _AcDb.Database db, _AcEd.Editor ed)
        {
            var layerFilter = lft.Root;
            using (var trans = db.TransactionManager.StartTransaction())
            {
                _AcLm.LayerGroup group = null;
                _AcLm.LayerFilter theLF = RecFindLayerFilterWithName(layerFilter.NestedFilters, LAYER_FILTER_NAME_X_NICHT_PLOTTEN, 0, 100);
                if (theLF != null)
                {
                    var checkGroup = theLF as _AcLm.LayerGroup;
                    if (checkGroup != null) throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Es gibt bereits eine Gruppe namens '{0}'! Bitte manuell löschen oder umbenennen.", LAYER_FILTER_NAME_X_NICHT_PLOTTEN));
                    if (!theLF.AllowDelete) throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Layerfilter '{0}' kann nicht gelöscht werden!", LAYER_FILTER_NAME_X_NICHT_PLOTTEN));
                    group = theLF.Parent as _AcLm.LayerGroup;
                    ed.WriteMessage(string.Format(CultureInfo.CurrentCulture, "\nLösche bestehenden Layerfilter {0} mit Filterexpression='{1}'.", LAYER_FILTER_NAME_X_NICHT_PLOTTEN, theLF.FilterExpression));
                    theLF.Parent.NestedFilters.Remove(theLF);
                }
                // create a new layer filter  
                ed.WriteMessage(string.Format(CultureInfo.CurrentCulture, "\nErstelle Layerfilter {0} mit Filterexpression='{1}'.", LAYER_FILTER_NAME_X_NICHT_PLOTTEN, LAYER_FILTER_EXPRESSION_X_NICHT_PLOTTEN));
                var df = new _AcLm.LayerFilter();
                df.Name = LAYER_FILTER_NAME_X_NICHT_PLOTTEN;
                df.FilterExpression = LAYER_FILTER_EXPRESSION_X_NICHT_PLOTTEN;
                if (group != null)
                {
                    group.NestedFilters.Add(df);
                }
                else
                {
                    layerFilter.NestedFilters.Add(df);
                }
                db.LayerFilters = lft;
                trans.Commit();
            }
        }
        private static _AcLm.LayerFilter RecFindLayerFilterWithName(_AcLm.LayerFilterCollection layerFilterCollection, string name, int level, int maxLevel)
        {
            level++;
            if (level >= maxLevel) return null;
            foreach (_AcLm.LayerFilter layerFilter in layerFilterCollection)
            {
                var group = layerFilter as _AcLm.LayerGroup;
                if (group != null)
                {
                    if (string.Compare(group.Name, name, StringComparison.OrdinalIgnoreCase) == 0) return layerFilter;
                    var lf = RecFindLayerFilterWithName(group.NestedFilters, name, level, maxLevel);
                    if (lf != null) return lf;
                }
                else
                {
                    if (string.Compare(layerFilter.Name, name, StringComparison.OrdinalIgnoreCase) == 0) return layerFilter;
                }
            }
            return null;
        }

        #endregion

        /// <summary>
        /// http://adndevblog.typepad.com/autocad/2014/06/importing-layer-filters.html
        /// </summary>
        [_AcTrx.CommandMethod("Plan2ImportLayerFilters")]
        public static void Plan2ImportLayerFilters()
        {
            var filePath = GetDwgName();
            if (string.IsNullOrEmpty(filePath)) return;

            if (!System.IO.File.Exists(filePath))
                return;

            _AcAp.Document doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            _AcEd.Editor ed = doc.Editor;
            _AcDb.Database destDb = doc.Database;
            _AcLm.LayerFilterTree lft = destDb.LayerFilters;
            using (_AcDb.Database srcDb = new _AcDb.Database(false, false))
            {
                srcDb.ReadDwgFile(filePath, _AcDb.FileOpenMode.OpenForReadAndAllShare, false, String.Empty);
                ImportNestedFilters(srcDb.LayerFilters.Root, lft.Root, srcDb, destDb, false, ed);
            }
            destDb.LayerFilters = lft;
        }

        private static void ImportNestedFilters(

                                    _AcLm.LayerFilter srcFilter,
                                    _AcLm.LayerFilter destFilter,
                                    _AcDb.Database srcDb,
                                    _AcDb.Database destDb,
                                    bool copyLayer,
                                    _AcEd.Editor ed)
        {

            using (_AcDb.Transaction tr = srcDb.TransactionManager.StartTransaction())
            {
                _AcDb.LayerTable lt = tr.GetObject(srcDb.LayerTableId, _AcDb.OpenMode.ForRead, false) as _AcDb.LayerTable;

                foreach (_AcLm.LayerFilter sf in srcFilter.NestedFilters)
                {
                    _AcDb.IdMapping idmap = new _AcDb.IdMapping();
                    if (copyLayer)
                    {
                        // Get the layers to be cloned to the dest db.  
                        // Only those that are pass the filter  
                        _AcDb.ObjectIdCollection layerIds = new _AcDb.ObjectIdCollection();
                        foreach (_AcDb.ObjectId layerId in lt)
                        {
                            _AcDb.LayerTableRecord ltr = tr.GetObject(layerId, _AcDb.OpenMode.ForRead, false) as _AcDb.LayerTableRecord;
                            if (sf.Filter(ltr))
                            {
                                layerIds.Add(layerId);
                            }
                        }

                        // clone the layers to the dest db  
                        if (layerIds.Count > 0)
                        {
                            srcDb.WblockCloneObjects(
                                            layerIds,
                                            destDb.LayerTableId,
                                            idmap,
                                            _AcDb.DuplicateRecordCloning.Replace,
                                            false);

                        }
                    }

                    // Find if a destination database already   
                    // has a layer filter with the same name  
                    _AcLm.LayerFilter df = null;
                    foreach (_AcLm.LayerFilter f in destFilter.NestedFilters)
                    {
                        if (f.Name.Equals(sf.Name))
                        {
                            df = f;
                            break;
                        }
                    }

                    if (df == null)
                    {
                        if (sf is _AcLm.LayerGroup)
                        {
                            // create a new layer filter group  
                            // if nothing found  
                            _AcLm.LayerGroup sfgroup = sf as _AcLm.LayerGroup;
                            _AcLm.LayerGroup dfgroup = new _AcLm.LayerGroup();
                            ed.WriteMessage(string.Format(CultureInfo.CurrentCulture, "\nImportiere Layerfilter-Gruppe {0}.", sf.Name));
                            dfgroup.Name = sf.Name;
                            df = dfgroup;
                            if (copyLayer)
                            {
                                _AcLm.LayerCollection lyrs = sfgroup.LayerIds;
                                foreach (_AcDb.ObjectId lid in lyrs)
                                {
                                    if (idmap.Contains(lid))
                                    {
                                        _AcDb.IdPair idp = idmap[lid];

                                        dfgroup.LayerIds.Add(idp.Value);

                                    }

                                }
                            }
                            destFilter.NestedFilters.Add(df);
                        }
                        else
                        {
                            // create a new layer filter  
                            // if nothing found  
                            ed.WriteMessage(string.Format(CultureInfo.CurrentCulture, "\nImportiere Layerfilter {0}.", sf.Name));
                            df = new _AcLm.LayerFilter();
                            df.Name = sf.Name;
                            df.FilterExpression = sf.FilterExpression;
                            destFilter.NestedFilters.Add(df);
                        }
                    }
                    // Import other filters  
                    ImportNestedFilters(sf, df, srcDb, destDb, copyLayer, ed);
                }
                tr.Commit();
            }
        }

        private static string GetDwgName()
        {
            _AcAp.Document doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            _AcEd.Editor ed = doc.Editor;

            _AcWnd.OpenFileDialog ofd = new _AcWnd.OpenFileDialog(
                "AutoCAD-Datei mit den zu importierenden Layerfilter wählen", "", "dwg", "AutoCAD-Datei mit den zu importierenden Layerfilter wählen", _AcWnd.OpenFileDialog.OpenFileDialogFlags.AllowAnyExtension
            );
            System.Windows.Forms.DialogResult dr = ofd.ShowDialog();
            if (dr != System.Windows.Forms.DialogResult.OK) return null;

            return ofd.Filename;
        }
    }
}

#endif