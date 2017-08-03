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
//using Autodesk.AutoCAD.ApplicationServices;
//using Autodesk.AutoCAD.EditorInput;
//using Autodesk.AutoCAD.DatabaseServices;


#if BRX_APP
using _AcAp = Bricscad.ApplicationServices;
using Teigha.DatabaseServices;
using Bricscad.EditorInput;
#elif ARX_APP
using _AcAp = Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
#endif

namespace Plan2Ext.Nummerierung
{
    public partial class NrControl : UserControl
    {
        #region Member variables
        //private Engine _Engine; 
        private bool selectAllDone = false;

        #endregion

        private NrOptions _NrOptions;
        public NrControl(NrOptions nrOptions)
        {
            InitializeComponent();

            _NrOptions = nrOptions;
            _NrOptions.Form = this;
            Globs.TheNrOptions = _NrOptions;

            FillComponents();
        }

        #region Private

        private void FillComponents()
        {
            txtTop.Text = _NrOptions.Top;
            txtSeparator.Text = _NrOptions.Separator;
            txtNumber.Text = _NrOptions.Number;
            txtBlockname.Text = _NrOptions.Blockname;
            txtAttName.Text = _NrOptions.Attribname;
            chkFirstAttribute.Checked = _NrOptions.UseFirstAttrib;
        }
        #endregion

        #region Public
        public bool SelectAllOnFocus { get; set; }
        #endregion

        #region Events

        private void txtNumber_Validating(object sender, CancelEventArgs e)
        {
            string num = txtNumber.Text;
            StringBuilder sb = new StringBuilder();
            foreach (char c in num.ToArray())
            {
                if (c >= '0' && c <= '9') sb.Append(c);
            }
            num = sb.ToString();
            if (string.IsNullOrEmpty(num)) num = "01";
            if (string.Compare(txtNumber.Text, num, StringComparison.OrdinalIgnoreCase) != 0) txtNumber.Text = num;

        }
        #endregion

        private void ResetNr()
        {
            _NrOptions.ResetNr();
            txtNumber.Text = _NrOptions.Number;
        }

        private void NrControl_Load(object sender, EventArgs e)
        {

        }

        private bool _SelectBlockShield = false;
        private void btnSelectBlock_Click(object sender, EventArgs e)
        {
            if (_SelectBlockShield) return;
            try
            {
                _SelectBlockShield = true;

                Globs.CancelCommand();

                _AcAp.Application.DocumentManager.MdiActiveDocument.SendStringToExecute("Plan2NummerierungSelBlockAndAtt ", true, false, false);
            }
            catch (Exception ex)
            {
                _AcAp.Application.ShowAlertDialog(ex.Message);
            }
            finally
            {
                _SelectBlockShield = false;
            }
        }

        private void txtTop_TextChanged(object sender, EventArgs e)
        {
            if (txtTop.Text != _NrOptions.Top)
            {
                ResetNr();
            }
            _NrOptions.Top = txtTop.Text;
        }

        private bool _SelectTopShield = false;
        private void btnSelectTop_Click(object sender, EventArgs e)
        {
            if (_SelectTopShield) return;
            try
            {
                _SelectTopShield = true;

                Globs.CancelCommand();

                _AcAp.Application.DocumentManager.MdiActiveDocument.SendStringToExecute("Plan2NummerierungSelPrefix ", true, false, false);
            }
            catch (Exception ex)
            {
                _AcAp.Application.ShowAlertDialog(ex.Message);
            }
            finally
            {
                _SelectTopShield = false;
            }
        }

        private void txtSeparator_TextChanged(object sender, EventArgs e)
        {
            _NrOptions.Separator = txtSeparator.Text;
        }

        private void txtNumber_TextChanged(object sender, EventArgs e)
        {
            _NrOptions.Number = txtNumber.Text;
        }

        private void chkFirstAttribute_CheckedChanged(object sender, EventArgs e)
        {
            _NrOptions.UseFirstAttrib = chkFirstAttribute.Checked;
        }

        private bool _StartShield = false;
        private void btnStart_Click(object sender, EventArgs e)
        {
            if (_StartShield) return;
            try
            {
                _StartShield = true;

                using (_AcAp.DocumentLock m_doclock = _AcAp.Application.DocumentManager.MdiActiveDocument.LockDocument())
                {


#if NEWSETFOCUS
                    _AcAp.Application.DocumentManager.MdiActiveDocument.Window.Focus();
#else
                Autodesk.AutoCAD.Internal.Utils.SetFocusToDwgView(); // previous 2014 AutoCAD - Versions
#endif
                    //Autodesk.AutoCAD.Interop.AcadApplication app = (Autodesk.AutoCAD.Interop.AcadApplication)_AcAp.Application.AcadApplication;
                    //app.ActiveDocument.SendCommand("Plan2Raumnummern\n");
                    _AcAp.Application.DocumentManager.MdiActiveDocument.SendStringToExecute("Plan2Nummerierung ", true, false, false);

                }

            }
            catch (Exception ex)
            {
                _AcAp.Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2Nummerierung aufgetreten! {0}", ex.Message));
            }
            finally
            {
                _StartShield = false;
            }
        }

    }
}
