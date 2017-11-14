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

namespace Plan2Ext.Vorauswahl
{
    public class VorauswahlPalette
    {
        // We cannot derive from PaletteSet
        // so we contain it
        static PaletteSet ps;

        // We need to make the textbox available
        // via a static member
        static VorauswahlControl userControl;

        public VorauswahlPalette()
        {
            userControl = new VorauswahlControl();
        }

        public bool Show()
        {

            if (ps == null)
            {
                ps = new PaletteSet("Vorauswahl");
                ps.Style =
                  PaletteSetStyles.NameEditable |
                  PaletteSetStyles.ShowPropertiesMenu |
                  PaletteSetStyles.ShowAutoHideButton |
                  PaletteSetStyles.ShowCloseButton;
                ps.MinimumSize =
                  new System.Drawing.Size(93, 235);
#if ACAD2013_OR_NEWER
#if ARX_APP
                ps.SetSize(new System.Drawing.Size(154, 235));
#endif
#endif
                ps.Add("Vorauswahl", userControl);
                if (!ps.Visible)
                {
                    ps.Visible = true;
                }
                return false;
            }
            else
            {
                if (!ps.Visible)
                {
                    ps.Visible = true;
                    return false;
                }
                return true;
            }
        }

        public List<string> BlocknamesInList()
        {
            var blockNames = new List<string>();
            foreach (var item in userControl.lstBlocknamen.Items)
            {
                blockNames.Add(item.ToString());
            }
            return blockNames;
        }

        public void AddBlockNamesToList(List<string> blockNames)
        {
            if (blockNames == null) return;
            foreach (var blockName in blockNames)
            {
                userControl.lstBlocknamen.Items.Add(blockName);
            }
        }

        public void AddBlockNameToList(string blockName)
        {
            if (string.IsNullOrEmpty(blockName)) return;
            userControl.lstBlocknamen.Items.Add(blockName);
        }

        public List<string> LayernamesInList()
        {
            var LayerNames = new List<string>();
            foreach (var item in userControl.lstLayer.Items)
            {
                LayerNames.Add(item.ToString());
            }
            return LayerNames;
        }

        public void AddLayerNameToList(string layerName)
        {
            if (string.IsNullOrEmpty(layerName)) return;
            userControl.lstLayer.Items.Add(layerName);
        }
    }
}
