using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Globalization;

#if BRX_APP
using _AcAp = Bricscad.ApplicationServices;
#elif ARX_APP
  using _AcAp = Autodesk.AutoCAD.ApplicationServices;
#endif


namespace Plan2Ext.HoehenPruefung
{
    public partial class HoePrControl : UserControl
    {
        #region log4net Initialization
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Convert.ToString((typeof(HoePrControl))));
        #endregion

        private HoePrOptions _HoePrOptions= null;

        public HoePrControl(HoePrOptions options)
        {
            InitializeComponent();

            _HoePrOptions = options;
            _HoePrOptions.Form = this;
            Globs.TheHoePrOptions = _HoePrOptions;

            FillComponents();
        }

        private void FillComponents()
        {
            txtBlockname.Text = _HoePrOptions.HKBlockname;
            txtHoehenAtt.Text = _HoePrOptions.AttHoehe;
            txtFbToleranz.Text = _HoePrOptions.FbToleranz.ToString(CultureInfo.InvariantCulture );
            txtPolygonLayer.Text = _HoePrOptions.PolygonLayer;

        }

        private void txtBlockname_TextChanged(object sender, EventArgs e)
        {
            _HoePrOptions.HKBlockname = txtBlockname.Text;
        }

        private void txtHoehenAtt_TextChanged(object sender, EventArgs e)
        {
            _HoePrOptions.AttHoehe = txtHoehenAtt.Text;
        }

        private bool txtFbToleranz_TextChanged_Shield = false;
        private void txtFbToleranz_TextChanged(object sender, EventArgs e)
        {
            if (txtFbToleranz_TextChanged_Shield) return;

            try
            {
                txtFbToleranz_TextChanged_Shield = true;
                string txt = txtFbToleranz.Text;
                if (string.IsNullOrEmpty(txt)) return;

                int i;
                if (!int.TryParse(txt, out i)) i = 3;
                _HoePrOptions.FbToleranz = i;

                string validValue = _HoePrOptions.FbToleranz.ToString(CultureInfo.InvariantCulture);
                if (validValue == txt) return;

                txtFbToleranz.Text = validValue;
            }
            finally
            {
                txtFbToleranz_TextChanged_Shield = false;
            }
        }

        private bool _SelBlockAndAttShield = false;
        private void btnSelBlockAndAtt_Click(object sender, EventArgs e)
        {
            if (_SelBlockAndAttShield) return;
            try
            {
                _SelBlockAndAttShield = true;

                Globs.CancelCommand();

                _AcAp.Application.DocumentManager.MdiActiveDocument.SendStringToExecute("Plan2HoePrSelHkBlockAndAtt ", true, false, false);
            }
            catch (Exception ex)
            {
                _AcAp.Application.ShowAlertDialog(ex.Message);
            }
            finally
            {
                _SelBlockAndAttShield = false;
            }
        }

        private bool _CheckFbShield = false;
        private void btnCheckFb_Click(object sender, EventArgs e)
        {
            if (_CheckFbShield) return;
            try
            {
                _CheckFbShield = true;

                Globs.CancelCommand();

                _AcAp.Application.DocumentManager.MdiActiveDocument.SendStringToExecute("Plan2HoePrCheckFb ", true, false, false);
            }
            catch (Exception ex)
            {
                _AcAp.Application.ShowAlertDialog(ex.Message);
            }
            finally
            {
                _CheckFbShield = false;
            }
        }

        private void txtPolygonLayer_TextChanged(object sender, EventArgs e)
        {
            _HoePrOptions.PolygonLayer = txtPolygonLayer.Text ;
        }

        private bool _SelPolygonLayerShield = false;
        private void btnSelPolygonLayer_Click(object sender, EventArgs e)
        {
            if (_SelPolygonLayerShield) return;
            try
            {
                _SelPolygonLayerShield = true;

                Globs.CancelCommand();

                _AcAp.Application.DocumentManager.MdiActiveDocument.SendStringToExecute("Plan2HoePrSelPolygonLayer ", true, false, false);
            }
            catch (Exception ex)
            {
                _AcAp.Application.ShowAlertDialog(ex.Message);
            }
            finally
            {
                _SelPolygonLayerShield = false;
            }
        }

        private bool _ResetCheckShield = false;
        private void btnResetCheck_Click(object sender, EventArgs e)
        {
            if (_ResetCheckShield) return;
            try
            {
                _ResetCheckShield = true;

                Globs.CancelCommand();

                _AcAp.Application.DocumentManager.MdiActiveDocument.SendStringToExecute("Plan2HoePrResetCheck ", true, false, false);
            }
            catch (Exception ex)
            {
                _AcAp.Application.ShowAlertDialog(ex.Message);
            }
            finally
            {
                _ResetCheckShield = false;
            }
        }

    }
}
