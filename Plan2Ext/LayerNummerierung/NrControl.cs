using System;
using System.Globalization;
using System.Windows.Forms;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
// ReSharper disable StringLiteralTypo

// ReSharper disable IdentifierTypo

namespace Plan2Ext.LayerNummerierung
{
    public partial class NrControl : UserControl
    {
        private readonly NrOptions _nrOptions;

        public NrControl(NrOptions nrOptions)
        {
            InitializeComponent();

            _nrOptions = nrOptions;
            _nrOptions.Form = this;
            Globs.TheNrOptions = _nrOptions;

            FillComponents();
        }
        #region Private

        private void FillComponents()
        {
            txtPrefix.Text = _nrOptions.Prefix;
            txtSuffix.Text = _nrOptions.Suffix;
            txtNumber.Text = _nrOptions.Number;
        }
        #endregion

        private void txtPrefix_TextChanged(object sender, EventArgs e)
        {
            if (txtPrefix.Text != _nrOptions.Prefix)
            {
                ResetNr();
            }
            _nrOptions.Prefix = txtPrefix.Text;
        }

        private void txtSuffix_TextChanged(object sender, EventArgs e)
        {
            if (txtSuffix.Text != _nrOptions.Suffix)
            {
                ResetNr();
            }
            _nrOptions.Suffix = txtSuffix.Text;
        }

        private bool _startShield;
        private void btnStart_Click(object sender, EventArgs e)
        {
            if (_startShield) return;
            try
            {
                _startShield = true;

                using (Application.DocumentManager.MdiActiveDocument.LockDocument())
                {
#if NEWSETFOCUS
                    Application.DocumentManager.MdiActiveDocument.Window.Focus();
#else
                Autodesk.AutoCAD.Internal.Utils.SetFocusToDwgView(); // previous 2014 AutoCAD - Versions
#endif
                    Application.DocumentManager.MdiActiveDocument.SendStringToExecute("Plan2LayerNummerierung ", true, false, false);

                }
            }
            catch (Exception ex)
            {
                Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2LayerNummerierung aufgetreten! {0}", ex.Message));
            }
            finally
            {
                _startShield = false;
            }
        }

        private void ResetNr()
        {
            _nrOptions.ResetNr();
            txtNumber.Text = _nrOptions.Number;
        }

        private void txtNumber_TextChanged(object sender, EventArgs e)
        {
            int test;
            if (!int.TryParse(txtNumber.Text, out test))
                // ReSharper disable once LocalizableElement
                txtNumber.Text = "01";

            _nrOptions.Number = txtNumber.Text;
        }
    }
}
