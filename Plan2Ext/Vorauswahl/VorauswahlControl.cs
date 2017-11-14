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
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Plan2Ext.Vorauswahl
{
    public partial class VorauswahlControl : UserControl
    {
        public VorauswahlControl()
        {
            InitializeComponent();
        }

        private bool _SelBlocknamenShield = false;
        private void btnSelBlocknamen_Click(object sender, EventArgs e)
        {
            if (_SelBlocknamenShield) return;
            try
            {
                _SelBlocknamenShield = true;

                Globs.CancelCommand();

                _AcAp.Application.DocumentManager.MdiActiveDocument.SendStringToExecute("Plan2VorauswahlSelBlocknamen ", true, false, false);
            }
            catch (Exception ex)
            {
                _AcAp.Application.ShowAlertDialog(ex.Message);
            }
            finally
            {
                _SelBlocknamenShield = false;
            }
        }

        private bool _SelLayerShield = false;
        private void btnSelLayer_Click(object sender, EventArgs e)
        {
            if (_SelLayerShield) return;
            try
            {
                _SelLayerShield = true;

                Globs.CancelCommand();

                _AcAp.Application.DocumentManager.MdiActiveDocument.SendStringToExecute("Plan2VorauswahlSelLayer ", true, false, false);
            }
            catch (Exception ex)
            {
                _AcAp.Application.ShowAlertDialog(ex.Message);
            }
            finally
            {
                _SelLayerShield = false;
            }
        }

        private bool _SelectShield = false;
        private void btnSelect_Click(object sender, EventArgs e)
        {
            if (_SelectShield) return;
            try
            {
                _SelectShield = true;

                Globs.CancelCommand();

                _AcAp.Application.DocumentManager.MdiActiveDocument.SendStringToExecute("Plan2VorauswahlSelect ", true, false, false);
            }
            catch (Exception ex)
            {
                _AcAp.Application.ShowAlertDialog(ex.Message);
            }
            finally
            {
                _SelectShield = false;
            }
        }

        private void lstBlocknamen_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.KeyCode == Keys.Delete)
                {
                    var selItem = lstBlocknamen.SelectedItem;
                    if (selItem == null) return;
                    lstBlocknamen.Items.Remove(selItem);
                }
            }
            catch (Exception)
            {
            }
        }

        private void lstLayer_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.KeyCode == Keys.Delete)
                {
                    var selItem = lstLayer.SelectedItem;
                    if (selItem == null) return;
                    lstLayer.Items.Remove(selItem);
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
