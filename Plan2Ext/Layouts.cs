// ReSharper disable CommentTypo
using System.Collections.Generic;
using System.Linq;
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

#endif

namespace Plan2Ext
{
    public static class Layouts
    {
        //Plan2Ext.Layouts.ImportLayout("02.OG B-Bau", @"D:\Plan2\Data\Plan2RenameBlocks\work\02_OGa.dwg");
        //Plan2Ext.Layouts.CreateLayout("alx");
        //var layoutNames = Plan2Ext.Layouts.GetLayoutNames();

        public static List<string> GetLayoutNames()
        {
            var layoutNames = new List<string>();

            _AcAp.Document acDoc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            _AcDb.Database acCurDb = acDoc.Database;

            using (_AcDb.Transaction trans = acCurDb.TransactionManager.StartTransaction())
            {
                _AcDb.DBDictionary lays = (_AcDb.DBDictionary)trans.GetObject(acCurDb.LayoutDictionaryId, _AcDb.OpenMode.ForRead);

                foreach (_AcDb.DBDictionaryEntry item in lays)
                {
                    layoutNames.Add(item.Key);
                }
                trans.Commit();
            }
            return layoutNames;
        }

        public static List<string> GetOrderedLayoutNames(bool onlySelected = false)
        {
            var layouts = new List<_AcDb.Layout>();
            _AcAp.Document acDoc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            _AcDb.Database acCurDb = acDoc.Database;
            using (_AcDb.Transaction trans = acCurDb.TransactionManager.StartTransaction())
            {
                _AcDb.DBDictionary lays = (_AcDb.DBDictionary)trans.GetObject(acCurDb.LayoutDictionaryId, _AcDb.OpenMode.ForRead);
                foreach (_AcDb.DBDictionaryEntry item in lays)
                {
                    layouts.Add((_AcDb.Layout)trans.GetObject(item.Value, _AcDb.OpenMode.ForRead));
                }
                trans.Commit();
            }
            return layouts.Where(x => !onlySelected || x.TabSelected).OrderBy(x => x.TabOrder).Select(x => x.LayoutName).ToList();
        }

        public static void SetLayoutActive(string name)
        {
            if (!GetLayoutNames().Contains(name)) return;

            _AcAp.Document acDoc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            _AcDb.Database acCurDb = acDoc.Database;
            using (_AcDb.Transaction trans = acCurDb.TransactionManager.StartTransaction())
            {
                // Reference the Layout Manager
                _AcDb.LayoutManager acLayoutMgr = _AcDb.LayoutManager.Current;

                // Open the layout
                _AcDb.Layout layout = (_AcDb.Layout)trans.GetObject(acLayoutMgr.GetLayoutId(name), _AcDb.OpenMode.ForRead);
                acLayoutMgr.CurrentLayout = layout.LayoutName;

                // Save the changes made
                trans.Commit();
            }
        }

        public static void CreateLayout(string name)
        {
            _AcAp.Document acDoc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            _AcDb.Database acCurDb = acDoc.Database;

            using (_AcDb.Transaction trans = acCurDb.TransactionManager.StartTransaction())
            {
                // Reference the Layout Manager
                _AcDb.LayoutManager acLayoutMgr = _AcDb.LayoutManager.Current;

                // Create the new layout with default settings
                _AcDb.ObjectId objID = acLayoutMgr.CreateLayout(name);

                // Open the layout
                _AcDb.Layout layout = trans.GetObject(objID, _AcDb.OpenMode.ForRead) as _AcDb.Layout;

                // Set the layout current if it is not already
                if (layout.TabSelected == false)
                {
                    acLayoutMgr.CurrentLayout = layout.LayoutName;
                }

                // Output some information related to the layout object
                acDoc.Editor.WriteMessage("\nTab Order: " + layout.TabOrder +
                                          "\nTab Selected: " + layout.TabSelected +
                                          "\nBlock Table Record ID: " +
                                          layout.BlockTableRecordId.ToString());

                // Save the changes made
                trans.Commit();
            }
        }

        public static _AcDb.ObjectId GetLayoutId(string layoutName, _AcDb.Database db)
        {
            _AcDb.ObjectId layoutId = default(_AcDb.ObjectId);
            using (var transaction = db.TransactionManager.StartTransaction())
            {
                var layouts = (_AcDb.DBDictionary)transaction.GetObject(db.LayoutDictionaryId, _AcDb.OpenMode.ForRead);
                if (layouts.Contains(layoutName))
                {
                    layoutId = layouts.GetAt(layoutName);
                }
                transaction.Commit();
            }

            return layoutId;
        }

        public static void ImportLayout(string layoutName, string filename)
        {
            // Get the current document and database
            _AcAp.Document acDoc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            _AcDb.Database acCurDb = acDoc.Database;

            // Specify the layout name and drawing file to work with
            //string layoutName = "MAIN AND SECOND FLOOR PLAN";
            //string filename = "C:\\AutoCAD\\Sample\\Sheet Sets\\Architectural\\A-01.dwg";

            // Create a new database object and open the drawing into memory
            _AcDb.Database acExDb = new _AcDb.Database(false, true);
            acExDb.ReadDwgFile(filename, Autodesk.AutoCAD.DatabaseServices.FileOpenMode.OpenForReadAndAllShare, true, "");

            // Create a transaction for the external drawing
            using (_AcDb.Transaction acTransEx = acExDb.TransactionManager.StartTransaction())
            {
                // Get the layouts dictionary
                _AcDb.DBDictionary layoutsEx = acTransEx.GetObject(acExDb.LayoutDictionaryId, _AcDb.OpenMode.ForRead) as _AcDb.DBDictionary;

                // Check to see if the layout exists in the external drawing
                if (layoutsEx.Contains(layoutName) == true)
                {
                    // Get the layout and block objects from the external drawing
                    _AcDb.Layout layEx = layoutsEx.GetAt(layoutName).GetObject(_AcDb.OpenMode.ForRead) as _AcDb.Layout;
                    _AcDb.BlockTableRecord blkBlkRecEx = acTransEx.GetObject(layEx.BlockTableRecordId, _AcDb.OpenMode.ForRead) as _AcDb.BlockTableRecord;

                    // Get the objects from the block associated with the layout
                    _AcDb.ObjectIdCollection idCol = new _AcDb.ObjectIdCollection();
                    foreach (_AcDb.ObjectId id in blkBlkRecEx)
                    {
                        idCol.Add(id);
                    }

                    // Create a transaction for the current drawing
                    using (_AcDb.Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
                    {
                        // Get the block table and create a new block
                        // then copy the objects between drawings
                        _AcDb.BlockTable blkTbl = acTrans.GetObject(acCurDb.BlockTableId, _AcDb.OpenMode.ForWrite) as _AcDb.BlockTable;
                        using (_AcDb.BlockTableRecord blkBlkRec = new _AcDb.BlockTableRecord())
                        {
                            int layoutCount = layoutsEx.Count - 1;

                            blkBlkRec.Name = "*Paper_Space" + layoutCount.ToString();
                            blkTbl.Add(blkBlkRec);
                            acTrans.AddNewlyCreatedDBObject(blkBlkRec, true);
                            acExDb.WblockCloneObjects(idCol, blkBlkRec.ObjectId, new _AcDb.IdMapping(), _AcDb.DuplicateRecordCloning.Ignore, false);

                            // Create a new layout and then copy properties between drawings
                            _AcDb.DBDictionary layouts = acTrans.GetObject(acCurDb.LayoutDictionaryId, _AcDb.OpenMode.ForWrite) as _AcDb.DBDictionary;
                            using (_AcDb.Layout lay = new _AcDb.Layout())
                            {
                                lay.LayoutName = layoutName;
                                lay.AddToLayoutDictionary(acCurDb, blkBlkRec.ObjectId);
                                acTrans.AddNewlyCreatedDBObject(lay, true);
                                lay.CopyFrom(layEx);

                                _AcDb.DBDictionary plSets = acTrans.GetObject(acCurDb.PlotSettingsDictionaryId, _AcDb.OpenMode.ForRead) as _AcDb.DBDictionary;

                                // Check to see if a named page setup was assigned to the layout,
                                // if so then copy the page setup settings
                                if (lay.PlotSettingsName != "")
                                {
                                    // Check to see if the page setup exists
                                    if (plSets.Contains(lay.PlotSettingsName) == false)
                                    {
                                        plSets.UpgradeOpen();

                                        using (_AcDb.PlotSettings plSet = new _AcDb.PlotSettings(lay.ModelType))
                                        {
                                            plSet.PlotSettingsName = lay.PlotSettingsName;
                                            plSet.AddToPlotSettingsDictionary(acCurDb);
                                            acTrans.AddNewlyCreatedDBObject(plSet, true);

                                            _AcDb.DBDictionary plSetsEx = acTransEx.GetObject(acExDb.PlotSettingsDictionaryId, _AcDb.OpenMode.ForRead) as _AcDb.DBDictionary;

                                            _AcDb.PlotSettings plSetEx = plSetsEx.GetAt(lay.PlotSettingsName).GetObject(_AcDb.OpenMode.ForRead) as _AcDb.PlotSettings;

                                            plSet.CopyFrom(plSetEx);
                                        }
                                    }
                                }
                            }
                        }

                        // Regen the drawing to get the layout tab to display
                        acDoc.Editor.Regen();

                        // Save the changes made
                        acTrans.Commit();
                    }
                }
                else
                {
                    // Display a message if the layout could not be found in the specified drawing
                    acDoc.Editor.WriteMessage("\nLayout '" + layoutName +
                                              "' could not be imported from '" + filename + "'.");
                }

                // Discard the changes made to the external drawing file
                acTransEx.Abort();
            }

            // Close the external drawing file
            acExDb.Dispose();
        }
    }
}
