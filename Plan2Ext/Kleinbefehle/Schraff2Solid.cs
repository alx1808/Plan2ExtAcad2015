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
using System.Text.RegularExpressions;
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plan2Ext.Kleinbefehle
{
    public class Schraff2Solid
    {
        #region log4net Initialization
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Convert.ToString((typeof(Schraff2Solid))));
        #endregion

        #region Constants
        private const string PREDEFINED_SOLID_PATTERN_NAME = "_SOLID";
        private const string REGEX_SOLID_PATTERN = "SOLID";
        private const int LAYER_COLOR_INDEX = 253;
        private const string NEW_SUFFIX = "_F";
        private const string SUFFIX_TO_REPLACE = "_S";
        #endregion

        [_AcTrx.CommandMethod("Plan2Schraff2Solid")]
        public static void Plan2Schraff2Solid()
        {
            _AcAp.Document doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            _AcEd.Editor ed = doc.Editor;
            try
            {
                _AcEd.PromptSelectionOptions options = new _AcEd.PromptSelectionOptions();
                options.MessageForAdding = "Schraffuren wählen: ";
                options.RejectObjectsFromNonCurrentSpace = true;
                _AcEd.SelectionFilter filter = new _AcEd.SelectionFilter(new _AcDb.TypedValue[] { new _AcDb.TypedValue((int)_AcDb.DxfCode.Start, "HATCH"), });
                _AcEd.PromptSelectionResult res = ed.GetSelection(options, filter);
                if (res.Status != _AcEd.PromptStatus.OK) return;
#if BRX_APP
            _AcEd.SelectionSet ss = res.Value;
#else
                using (_AcEd.SelectionSet ss = res.Value)
#endif
                {
                    var allSelectedIds = ss.GetObjectIds().ToList();
                    using (var myTrans = db.TransactionManager.StartTransaction())
                    {
                        var btrId = Plan2Ext.Globs.GetBtrOfCurrentLayout(myTrans);
                        var ids = new _AcDb.ObjectIdCollection();
                        foreach (var oid in allSelectedIds)
                        {
                            var hatch = (_AcDb.Hatch)myTrans.GetObject(oid, _AcDb.OpenMode.ForRead);
                            if (!Regex.IsMatch(hatch.PatternName, REGEX_SOLID_PATTERN, RegexOptions.IgnoreCase))
                            {

                                var btr = (_AcDb.BlockTableRecord)myTrans.GetObject(hatch.BlockId, _AcDb.OpenMode.ForRead);

                                var hatch2 = (_AcDb.Hatch)hatch.Clone();
                                hatch2.SetHatchPattern(_AcDb.HatchPatternType.PreDefined, PREDEFINED_SOLID_PATTERN_NAME);
                                string layerName = GetLayerName(hatch.Layer);
                                var layerColor = _AcCm.Color.FromColorIndex(_AcCm.ColorMethod.ByLayer, LAYER_COLOR_INDEX);
                                Plan2Ext.Globs.CreateLayer(layerName, layerColor);
                                hatch2.Layer = layerName;

                                btr.UpgradeOpen();
                                btr.AppendEntity(hatch2);
                                btr.DowngradeOpen();
                                myTrans.AddNewlyCreatedDBObject(hatch2, add: true);

                                ids.Add(hatch2.ObjectId);
                            }
                        }

                        if (ids.Count > 0)
                        {
                            Plan2Ext.Globs.DrawOrderBottom(ids, btrId);
                            ed.WriteMessage(string.Format("\nAnzahl der neu erzeugten Solid-Schraffuren: {0}", ids.Count.ToString()));
                        }
                        else
                        {
                            ed.WriteMessage(string.Format("\nAnzahl der neu erzeugten Solid-Schraffuren: {0}", 0.ToString()));
                        }
                        myTrans.Commit();
                    }
                }
            }
            catch (System.Exception ex)
            {
                string msg = string.Format(CultureInfo.CurrentCulture, "Fehler in (Plan2Schraff2Solid): {0}", ex.Message);
                ed.WriteMessage("\n" + msg);
                System.Windows.Forms.MessageBox.Show(ex.Message, "Plan2Schraff2Solid");
            }
        }

        private static string GetLayerName(string oldLayerName)
        {
            if (oldLayerName.EndsWith(SUFFIX_TO_REPLACE))
            {
                int oldSufLen = SUFFIX_TO_REPLACE.Length;
                return oldLayerName.Remove(oldLayerName.Length - oldSufLen, oldSufLen) + NEW_SUFFIX;
            }
            else
            {
                return oldLayerName + NEW_SUFFIX;
            }
        }
    }
}
