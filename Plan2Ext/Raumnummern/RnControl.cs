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

namespace Plan2Ext.Raumnummern
{
    public partial class RnControl : UserControl
    {
        #region Member variables
        //private Engine _Engine; 
        private bool selectAllDone = false;

        #endregion

        private RnOptions _RnOptions;
        public RnControl(RnOptions rnOptions)
        {
            InitializeComponent();

            _RnOptions = rnOptions;
            _RnOptions.Form = this;
            Globs.TheRnOptions = _RnOptions;

            FillComponents();

            //this.txtTop.Leave += new System.EventHandler(txtTop_Leave);
            //this.txtTop.GotFocus += new System.EventHandler(txtTop_GotFocus);
            //this.txtTop.MouseUp += new System.Windows.Forms.MouseEventHandler(txtTop_MouseUp);

            //this.txtSeparator.Leave += new System.EventHandler(txtSeparator_Leave);
            //this.txtSeparator.GotFocus += new System.EventHandler(txtSeparator_GotFocus);
            //this.txtSeparator.MouseUp += new System.Windows.Forms.MouseEventHandler(txtSeparator_MouseUp);

            //this.txtNumber.Leave += new System.EventHandler(txtNumber_Leave);
            //this.txtNumber.GotFocus += new System.EventHandler(txtNumber_GotFocus);
            //this.txtNumber.MouseUp += new System.Windows.Forms.MouseEventHandler(txtNumber_MouseUp);

            //_Engine = new Engine(this); 
        }

        #region Private

        private void FillComponents()
        {
            txtTop.Text = _RnOptions.Top;
            txtSeparator.Text = _RnOptions.Separator;
            txtTopNr.Text = _RnOptions.TopNr;
            txtNumber.Text = _RnOptions.Number;
            chkAutoCorr.Checked = _RnOptions.AutoCorr;
            txtBlockname.Text = _RnOptions.Blockname;
            txtAttName.Text = _RnOptions.Attribname;
            txtHBlockname.Text = _RnOptions.HBlockname;
            txtFlaechenAttributName.Text = _RnOptions.FlaechenAttributName;
            txtFlaechenGrenzeLayerName.Text = _RnOptions.FlaechenGrenzeLayerName;
            txtAbzFlaechenGrenzeLayerName.Text = _RnOptions.AbzFlaechenGrenzeLayerName;
        }
        #endregion

        #region Public
        public bool SelectAllOnFocus { get; set; }
        #endregion

        #region Events

        private bool _FbhWithoutNrShield = false;
        private void btnFbhWithoutNr_Click(object sender, EventArgs e)
        {
            if (_FbhWithoutNrShield) return;
            try
            {
                _FbhWithoutNrShield = true;

                using (_AcAp.DocumentLock m_doclock = _AcAp.Application.DocumentManager.MdiActiveDocument.LockDocument())
                {


#if NEWSETFOCUS
                    _AcAp.Application.DocumentManager.MdiActiveDocument.Window.Focus();
#else
                Autodesk.AutoCAD.Internal.Utils.SetFocusToDwgView(); // previous 2014 AutoCAD - Versions
#endif
                    _AcAp.Application.DocumentManager.MdiActiveDocument.SendStringToExecute("Plan2MoveFbhWithoutNumber ", true, false, false);

                }

            }
            catch (Exception ex)
            {
                _AcAp.Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2Raumnummern aufgetreten! {0}", ex.Message));
            }
            finally
            {
                _FbhWithoutNrShield = false;
            }

        }

        private bool _FbhWithNrShield = false;
        private void btnFbhWithNr_Click(object sender, EventArgs e)
        {
            if (_FbhWithNrShield) return;
            try
            {
                _FbhWithNrShield = true;

                using (_AcAp.DocumentLock m_doclock = _AcAp.Application.DocumentManager.MdiActiveDocument.LockDocument())
                {


#if NEWSETFOCUS
                    _AcAp.Application.DocumentManager.MdiActiveDocument.Window.Focus();
#else
                Autodesk.AutoCAD.Internal.Utils.SetFocusToDwgView(); // previous 2014 AutoCAD - Versions
#endif
                    _AcAp.Application.DocumentManager.MdiActiveDocument.SendStringToExecute("Plan2MoveFbhWithNumber ", true, false, false);

                }

            }
            catch (Exception ex)
            {
                _AcAp.Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2Raumnummern aufgetreten! {0}", ex.Message));
            }
            finally
            {
                _FbhWithNrShield = false;
            }

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
                    _AcAp.Application.DocumentManager.MdiActiveDocument.SendStringToExecute("Plan2Raumnummern ", true, false, false);

                }

            }
            catch (Exception ex)
            {
                _AcAp.Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2Raumnummern aufgetreten! {0}", ex.Message));
            }
            finally
            {
                _StartShield = false;
            }

        }



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


        private void txtTop_TextChanged(object sender, EventArgs e)
        {
            if (txtTop.Text != _RnOptions.Top)
            {
                ResetNr();
            }
            _RnOptions.Top = txtTop.Text;
        }

        private void ResetNr()
        {
            _RnOptions.ResetNr();
            txtNumber.Text = _RnOptions.Number;
        }

        private void txtSeparator_TextChanged(object sender, EventArgs e)
        {
            _RnOptions.Separator = txtSeparator.Text;
        }

        private void txtTopNr_TextChanged(object sender, EventArgs e)
        {
            _RnOptions.TopNr = txtTopNr.Text;
        }

        private void txtNumber_TextChanged(object sender, EventArgs e)
        {
            _RnOptions.Number = txtNumber.Text;
        }

        private void chkAutoCorr_CheckedChanged(object sender, EventArgs e)
        {
            _RnOptions.AutoCorr = chkAutoCorr.Checked;
        }

        private bool _SelectBlockShield = false;
        private void btnSelectBlock_Click(object sender, EventArgs e)
        {
            if (_SelectBlockShield) return;
            try
            {
                _SelectBlockShield = true;

                Globs.CancelCommand();

                _AcAp.Application.DocumentManager.MdiActiveDocument.SendStringToExecute("Plan2RaumnummerSelBlockAndAtt ", true, false, false);
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

        private bool _SelectHBlockShield = false;
        private void btnSelectHBlock_Click(object sender, EventArgs e)
        {
            if (_SelectHBlockShield) return;
            try
            {
                _SelectHBlockShield = true;

                Globs.CancelCommand();

                _AcAp.Application.DocumentManager.MdiActiveDocument.SendStringToExecute("Plan2RaumnummerSelHBlock ", true, false, false);
            }
            catch (Exception ex)
            {
                _AcAp.Application.ShowAlertDialog(ex.Message);
            }
            finally
            {
                _SelectHBlockShield = false;
            }

        }

        private bool _SelectTopShield = false;
        private void btnSelectTop_Click(object sender, EventArgs e)
        {
            if (_SelectTopShield) return;
            try
            {
                _SelectTopShield = true;

                Globs.CancelCommand();

                _AcAp.Application.DocumentManager.MdiActiveDocument.SendStringToExecute("Plan2RaumnummerSelTop ", true, false, false);
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

        private bool _FlaechenGrenzeLayerNameShield = false;
        private void btnFlaechenGrenzeLayerName_Click(object sender, EventArgs e)
        {
            if (_FlaechenGrenzeLayerNameShield) return;
            try
            {
                _FlaechenGrenzeLayerNameShield = true;

                Globs.CancelCommand();

                _AcAp.Application.DocumentManager.MdiActiveDocument.SendStringToExecute("Plan2RaumnummerSelFgLayer ", true, false, false);
            }
            catch (Exception ex)
            {
                _AcAp.Application.ShowAlertDialog(ex.Message);
            }
            finally
            {
                _FlaechenGrenzeLayerNameShield = false;
            }
        }

        private bool _AbzFlaechenGrenzeLayerNameShield = false;
        private void btnAbzFlaechenGrenzeLayerName_Click(object sender, EventArgs e)
        {
            if (_AbzFlaechenGrenzeLayerNameShield) return;
            try
            {
                _AbzFlaechenGrenzeLayerNameShield = true;

                Globs.CancelCommand();

                _AcAp.Application.DocumentManager.MdiActiveDocument.SendStringToExecute("Plan2RaumnummerSelAbzFgLayer ", true, false, false);
            }
            catch (Exception ex)
            {
                _AcAp.Application.ShowAlertDialog(ex.Message);
            }
            finally
            {
                _AbzFlaechenGrenzeLayerNameShield = false;
            }
        }

        private bool _CalcAreaShield = false;
        private void btnCalcArea_Click(object sender, EventArgs e)
        {
            if (_CalcAreaShield) return;
            try
            {
                _CalcAreaShield = true;

                using (_AcAp.DocumentLock m_doclock = _AcAp.Application.DocumentManager.MdiActiveDocument.LockDocument())
                {


#if NEWSETFOCUS
                    _AcAp.Application.DocumentManager.MdiActiveDocument.Window.Focus();
#else
                Autodesk.AutoCAD.Internal.Utils.SetFocusToDwgView(); // previous 2014 AutoCAD - Versions
#endif
                    _AcAp.Application.DocumentManager.MdiActiveDocument.SendStringToExecute("Plan2RaumnummernCalcArea ", true, false, false);
                }
            }
            catch (Exception ex)
            {
                _AcAp.Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2RaumnummernCalcArea aufgetreten! {0}", ex.Message));
            }
            finally
            {
                _CalcAreaShield = false;
            }
        }

        private bool _FlaBereinigShield = false;
        private void btnFlaBereinig_Click(object sender, EventArgs e)
        {
            if (_FlaBereinigShield) return;
            try
            {
                _FlaBereinigShield = true;

                Globs.CancelCommand();

                _AcAp.Application.DocumentManager.MdiActiveDocument.SendStringToExecute("Plan2FlaBereinig ", true, false, false);
            }
            catch (Exception ex)
            {
                _AcAp.Application.ShowAlertDialog(ex.Message);
            }
            finally
            {
                _FlaBereinigShield = false;
            }
        }

        private bool _InsertTopShield = false;
        private void btnInsertTop_Click(object sender, EventArgs e)
        {
            if (_InsertTopShield) return;
            try
            {
                _InsertTopShield = true;

                Globs.CancelCommand();

#if OLDER_THAN_2015
                _AcAp.Application.DocumentManager.MdiActiveDocument.SendStringToExecute("Plan2RaumnummernInsertTop2 ", true, false, false);
#else
                _AcAp.Application.DocumentManager.MdiActiveDocument.SendStringToExecute("Plan2RaumnummernInsertTop ", true, false, false);
#endif
            }
            catch (Exception ex)
            {
                _AcAp.Application.ShowAlertDialog(ex.Message);
            }
            finally
            {
                _InsertTopShield = false;
            }
        }

        //private void btnCalcArea_Click(object sender, EventArgs e)
        //{

        //}


        //void txtTop_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        //{
        //    if (SelectAllOnFocus && !selectAllDone && txtTop.SelectionLength == 0)
        //    {
        //        selectAllDone = true;
        //        txtTop.SelectAll();
        //    }
        //}

        //void txtTop_GotFocus(object sender, System.EventArgs e)
        //{
        //    if (SelectAllOnFocus && MouseButtons == MouseButtons.None)
        //    {
        //        txtTop.SelectAll();
        //        selectAllDone = true;
        //    }
        //}

        //void txtTop_Leave(object sender, System.EventArgs e)
        //{
        //    selectAllDone = false;
        //}

        //void txtSeparator_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        //{
        //    if (SelectAllOnFocus && !selectAllDone && txtSeparator.SelectionLength == 0)
        //    {
        //        selectAllDone = true;
        //        txtSeparator.SelectAll();
        //    }
        //}

        //void txtSeparator_GotFocus(object sender, System.EventArgs e)
        //{
        //    if (SelectAllOnFocus && MouseButtons == MouseButtons.None)
        //    {
        //        txtSeparator.SelectAll();
        //        selectAllDone = true;
        //    }
        //}

        //void txtSeparator_Leave(object sender, System.EventArgs e)
        //{
        //    selectAllDone = false;
        //}

        //void txtNumber_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        //{
        //    if (SelectAllOnFocus && !selectAllDone && txtNumber.SelectionLength == 0)
        //    {
        //        selectAllDone = true;
        //        txtNumber.SelectAll();
        //    }
        //}

        //void txtNumber_GotFocus(object sender, System.EventArgs e)
        //{
        //    if (SelectAllOnFocus && MouseButtons == MouseButtons.None)
        //    {
        //        txtNumber.SelectAll();
        //        selectAllDone = true;
        //    }
        //}

        //void txtNumber_Leave(object sender, System.EventArgs e)
        //{
        //    selectAllDone = false;
        //}


    }
}
