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
using System.Text.RegularExpressions;
#endif

namespace Plan2Ext.Kleinbefehle
{
    public class Layers
    {
        private static Plan2Ext.Globs.LayersState _LayersState = new Globs.LayersState();

        [_AcTrx.CommandMethod("Plan2SaveLayerStatus")]
        public static void Plan2SaveLayerStatus()
        {
            _AcEd.Editor ed = _AcAp.Application.DocumentManager.MdiActiveDocument.Editor;

            try
            {
                _LayersState = Plan2Ext.Globs.SaveLayersState();
            }
            catch (System.Exception ex)
            {
                string msg = string.Format(CultureInfo.CurrentCulture, "Fehler in (Plan2SaveLayerStatus): {0}", ex.Message);
                ed.WriteMessage("\n" + msg);
                System.Windows.Forms.MessageBox.Show(ex.Message, "Plan2SaveLayerStatus");
            }
        }

        [_AcTrx.CommandMethod("Plan2RestoreLayerStatus")]
        public static void Plan2RestoreLayerStatus()
        {
            _AcEd.Editor ed = _AcAp.Application.DocumentManager.MdiActiveDocument.Editor;

            try
            {
                if (_LayersState == null)
                {
                    ed.WriteMessage("\nEs ist noch kein Layerstatus gesichert.");
                    return;
                }
                Plan2Ext.Globs.RestoreLayersState(_LayersState);
            }
            catch (System.Exception ex)
            {
                string msg = string.Format(CultureInfo.CurrentCulture, "Fehler in (Plan2RestoreLayerStatus): {0}", ex.Message);
                ed.WriteMessage("\n" + msg);
                System.Windows.Forms.MessageBox.Show(ex.Message, "Plan2RestoreLayerStatus");
            }
        }
    }
}
