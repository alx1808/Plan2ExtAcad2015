using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

#if BRX_APP
using _AcAp = Bricscad.ApplicationServices;
#elif ARX_APP
using _AcAp = Autodesk.AutoCAD.ApplicationServices;
#endif

namespace Plan2Ext.CenterBlock
{
    public partial class CenterBlockControl : UserControl
    {
        private Options _Options = null;

        public CenterBlockControl(Options options)
        {
            InitializeComponent();

            _Options = options;
            _Options.Form = this;
            Globs.TheOptions = options;

            FillComponents();


        }

        private void FillComponents()
        {
            this.txtBlockname.Text = _Options.Blockname;
            this.txtLayer.Text = _Options.LayerName;
            this.chkUseXrefs.Checked = _Options.UseXRefs;
        }

        private void txtBlockname_TextChanged(object sender, EventArgs e)
        {
            _Options.Blockname = txtBlockname.Text;
        }

        private void txtLayer_TextChanged(object sender, EventArgs e)
        {
            _Options.LayerName = txtLayer.Text;
        }

        private void chkUseXrefs_CheckedChanged(object sender, EventArgs e)
        {
            _Options.UseXRefs = chkUseXrefs.Checked;
        }

        
        private bool _SelBlockShield = false;
        private void btnSelBlock_Click(object sender, EventArgs e)
        {
            if (_SelBlockShield) return;
            try
            {
                _SelBlockShield = true;

                Plan2Ext.Globs.CancelCommand();

                _AcAp.Application.DocumentManager.MdiActiveDocument.SendStringToExecute("Plan2CenterBlockSelBlock ", true, false, false);
            }
            catch (Exception ex)
            {
                _AcAp.Application.ShowAlertDialog(ex.Message);
            }
            finally
            {
                _SelBlockShield = false;
            }
        }

        private bool _StartShield = false;
        private void btnStart_Click(object sender, EventArgs e)
        {
            if (_StartShield) return;
            try
            {
                _StartShield = true;

                Plan2Ext.Globs.CancelCommand();

                _AcAp.Application.DocumentManager.MdiActiveDocument.SendStringToExecute("Plan2CenterBlock ", true, false, false);
            }
            catch (Exception ex)
            {
                _AcAp.Application.ShowAlertDialog(ex.Message);
            }
            finally
            {
                _StartShield = false;
            }
        }

        private bool _DelErrorSymsShield = false;
        private void btnDelErrorSyms_Click(object sender, EventArgs e)
        {
            if (_DelErrorSymsShield) return;
            try
            {
                _DelErrorSymsShield = true;

                Plan2Ext.Globs.CancelCommand();

                _AcAp.Application.DocumentManager.MdiActiveDocument.SendStringToExecute("Plan2CenterBlockDelErrSyms ", true, false, false);
            }
            catch (Exception ex)
            {
                _AcAp.Application.ShowAlertDialog(ex.Message);
            }
            finally
            {
                _DelErrorSymsShield = false;
            }
        }
    }
}
