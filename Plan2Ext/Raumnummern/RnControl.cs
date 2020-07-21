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
// ReSharper disable CommentTypo
// ReSharper disable IdentifierTypo
#endif

namespace Plan2Ext.Raumnummern
{
    public partial class RnControl : UserControl
    {
        #region Member variables
        //private Engine _Engine; 
        private bool selectAllDone = false;

        #endregion

        private readonly RnOptions _rnOptions;
        public RnControl()
        {
            InitializeComponent();

            _rnOptions = Globs.TheRnOptions;
            _rnOptions.Form = this;
            TheConfiguration.ConfigurationChanged += TheConfiguration_ConfigurationChanged;

            FillComponents();
        }

        void TheConfiguration_ConfigurationChanged(object sender, EventArgs e)
        {
            _rnOptions.ReadConfiguration();
            FillComponents();
        }

        #region Private

        private void FillComponents()
        {
            txtTop.Text = _rnOptions.Top;
            txtSeparator.Text = _rnOptions.Separator;
            txtNumber.Text = _rnOptions.Number;
            chkAutoCorr.Checked = _rnOptions.AutoCorr;
            txtBlockname.Text = _rnOptions.Blockname;
            txtAttName.Text = _rnOptions.Attribname;
            txtHBlockname.Text = _rnOptions.HBlockname;
            txtFlaechenAttributName.Text = _rnOptions.FlaechenAttributName;
            txtFlaechenGrenzeLayerName.Text = _rnOptions.FlaechenGrenzeLayerName;
            txtAbzFlaechenGrenzeLayerName.Text = _rnOptions.AbzFlaechenGrenzeLayerName;
            //chkHiddenAttribute.Checked = _rnOptions.UseHiddenAttribute;
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

                Globs.CancelCommand();

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

        private bool _DeleteFehlerlinesShield = false;
        private void btnDeleteFehlerlines_Click(object sender, EventArgs e)
        {
            if (_DeleteFehlerlinesShield) return;
            try
            {
                _DeleteFehlerlinesShield = true;

                Globs.CancelCommand();

                using (_AcAp.DocumentLock m_doclock = _AcAp.Application.DocumentManager.MdiActiveDocument.LockDocument())
                {
#if NEWSETFOCUS
                    _AcAp.Application.DocumentManager.MdiActiveDocument.Window.Focus();
#else
                Autodesk.AutoCAD.Internal.Utils.SetFocusToDwgView(); // previous 2014 AutoCAD - Versions
#endif
                    _AcAp.Application.DocumentManager.MdiActiveDocument.SendStringToExecute("Plan2RaumnummernDeleteFehlerlines ", true, false, false);
                }
            }
            catch (Exception ex)
            {
                _AcAp.Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2RaumnummernDeleteFehlerlines aufgetreten! {0}", ex.Message));
            }
            finally
            {
                _DeleteFehlerlinesShield = false;
            }
        }


        private bool _ReinitShield = false;
        private void btnReinit_Click(object sender, EventArgs e)
        {
            if (_ReinitShield) return;
            try
            {
                _ReinitShield = true;

                Globs.CancelCommand();

                using (_AcAp.DocumentLock m_doclock = _AcAp.Application.DocumentManager.MdiActiveDocument.LockDocument())
                {
#if NEWSETFOCUS
                    _AcAp.Application.DocumentManager.MdiActiveDocument.Window.Focus();
#else
                Autodesk.AutoCAD.Internal.Utils.SetFocusToDwgView(); // previous 2014 AutoCAD - Versions
#endif
                    _AcAp.Application.DocumentManager.MdiActiveDocument.SendStringToExecute("Plan2RaumnummernReinit ", true, false, false);
                }
            }
            catch (Exception ex)
            {
                _AcAp.Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2RaumnummernReinit aufgetreten! {0}", ex.Message));
            }
            finally
            {
                _ReinitShield = false;
            }
        }

        private bool _SumShield = false;
        private void btnSum_Click(object sender, EventArgs e)
        {
            if (_SumShield) return;
            try
            {
                _SumShield = true;

                Globs.CancelCommand();

                using (_AcAp.DocumentLock m_doclock = _AcAp.Application.DocumentManager.MdiActiveDocument.LockDocument())
                {
#if NEWSETFOCUS
                    _AcAp.Application.DocumentManager.MdiActiveDocument.Window.Focus();
#else
                Autodesk.AutoCAD.Internal.Utils.SetFocusToDwgView(); // previous 2014 AutoCAD - Versions
#endif
                    _AcAp.Application.DocumentManager.MdiActiveDocument.SendStringToExecute("Plan2RaumnummernSum ", true, false, false);
                }
            }
            catch (Exception ex)
            {
                _AcAp.Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2RaumnummernSum aufgetreten! {0}", ex.Message));
            }
            finally
            {
                _SumShield = false;
            }
        }

        private bool _RemoveAllInfosShield = false;
        private void btnRemoveAllInfos_Click(object sender, EventArgs e)
        {
            if (_RemoveAllInfosShield) return;
            try
            {
                _RemoveAllInfosShield = true;

                Globs.CancelCommand();

                using (_AcAp.DocumentLock m_doclock = _AcAp.Application.DocumentManager.MdiActiveDocument.LockDocument())
                {
#if NEWSETFOCUS
                    _AcAp.Application.DocumentManager.MdiActiveDocument.Window.Focus();
#else
                Autodesk.AutoCAD.Internal.Utils.SetFocusToDwgView(); // previous 2014 AutoCAD - Versions
#endif
                    //Autodesk.AutoCAD.Interop.AcadApplication app = (Autodesk.AutoCAD.Interop.AcadApplication)_AcAp.Application.AcadApplication;
                    //app.ActiveDocument.SendCommand("Plan2Raumnummern\n");
                    _AcAp.Application.DocumentManager.MdiActiveDocument.SendStringToExecute("Plan2RaumnummernRemoveAllInfos ", true, false, false);
                }
            }
            catch (Exception ex)
            {
                _AcAp.Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2RaumnummernRemoveAllInfos aufgetreten! {0}", ex.Message));
            }
            finally
            {
                _RemoveAllInfosShield = false;
            }
        }

        private bool _RenameTopShield = false;
        private void btnRenameTop_Click(object sender, EventArgs e)
        {
            if (_RenameTopShield) return;
            try
            {
                _RenameTopShield = true;

                Globs.CancelCommand();

                using (_AcAp.DocumentLock m_doclock = _AcAp.Application.DocumentManager.MdiActiveDocument.LockDocument())
                {
#if NEWSETFOCUS
                    _AcAp.Application.DocumentManager.MdiActiveDocument.Window.Focus();
#else
                Autodesk.AutoCAD.Internal.Utils.SetFocusToDwgView(); // previous 2014 AutoCAD - Versions
#endif
                    _AcAp.Application.DocumentManager.MdiActiveDocument.SendStringToExecute("Plan2RaumnummernRenameTop ", true, false, false);
                }
            }
            catch (Exception ex)
            {
                _AcAp.Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2RaumnummernRenameTop aufgetreten! {0}", ex.Message));
            }
            finally
            {
                _RenameTopShield = false;
            }
        }

        private bool _RemoveRaumShield = false;
        private void btnRemoveRaum_Click(object sender, EventArgs e)
        {
            if (_RemoveRaumShield) return;
            try
            {
                _RemoveRaumShield = true;

                Globs.CancelCommand();

                using (_AcAp.DocumentLock m_doclock = _AcAp.Application.DocumentManager.MdiActiveDocument.LockDocument())
                {
#if NEWSETFOCUS
                    _AcAp.Application.DocumentManager.MdiActiveDocument.Window.Focus();
#else
                Autodesk.AutoCAD.Internal.Utils.SetFocusToDwgView(); // previous 2014 AutoCAD - Versions
#endif
                    //Autodesk.AutoCAD.Interop.AcadApplication app = (Autodesk.AutoCAD.Interop.AcadApplication)_AcAp.Application.AcadApplication;
                    //app.ActiveDocument.SendCommand("Plan2Raumnummern\n");
                    _AcAp.Application.DocumentManager.MdiActiveDocument.SendStringToExecute("Plan2RaumnummernRemoveRaum ", true, false, false);

                }

            }
            catch (Exception ex)
            {
                _AcAp.Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2RaumnummernRemoveRaum aufgetreten! {0}", ex.Message));
            }
            finally
            {
                _RemoveRaumShield = false;
            }

        }

        //private void btnRemoveRaum_Click(object sender, EventArgs e)
        //{

        //}




        /// <summary>
        /// Validierung Raumnummern
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>
        /// Raumnummer setzt sich zusammen aus: [geschoss][nr]
        /// [geschoss] := "" | "K" | [1-9]
        /// [nr] := [1-9][1-9]
        /// </remarks>
        private void txtNumber_Validating(object sender, CancelEventArgs e)
        {
            try
            {
                string geschoss, nr;
                Globs.GetGeschossAndNr(txtNumber.Text.Trim(), out geschoss, out nr);
                txtNumber.Text = geschoss + nr;
            }
            catch (InvalidOperationException)
            {
                txtNumber.Text = "01";
            }
        }
        #endregion


        private void txtTop_TextChanged(object sender, EventArgs e)
        {
            if (txtTop.Text != _rnOptions.Top)
            {
                ResetNr();
            }
            _rnOptions.Top = txtTop.Text;
        }

        private void ResetNr()
        {
            _rnOptions.ResetNr();
            txtNumber.Text = _rnOptions.Number;
        }

        private void txtSeparator_TextChanged(object sender, EventArgs e)
        {
            _rnOptions.Separator = txtSeparator.Text;
        }

        private void txtNumber_TextChanged(object sender, EventArgs e)
        {
            _rnOptions.Number = txtNumber.Text;
        }

        private void chkAutoCorr_CheckedChanged(object sender, EventArgs e)
        {
            _rnOptions.AutoCorr = chkAutoCorr.Checked;
        }

        private bool _chkHiddenAttributeFirstTime = true;
        private bool _chkHiddenAttributeShield = false;
        private void chkHiddenAttribute_CheckedChanged(object sender, EventArgs e)
        {
            //if (_chkHiddenAttributeFirstTime)
            //{
            //    _chkHiddenAttributeFirstTime = false;
            //    return;
            //}
            //if (_chkHiddenAttributeShield) return;
            //_rnOptions.UseHiddenAttribute = chkHiddenAttribute.Checked;
            //try
            //{
            //    _chkHiddenAttributeShield = true;

            //    Globs.CancelCommand();

            //    _AcAp.Application.DocumentManager.MdiActiveDocument.SendStringToExecute("Plan2RaumnummerRnOnOff ", true, false, false);
            //}
            //catch (Exception ex)
            //{
            //    _AcAp.Application.ShowAlertDialog(ex.Message);
            //}
            //finally
            //{
            //    _chkHiddenAttributeShield = false;
            //}
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

                _AcAp.Application.DocumentManager.MdiActiveDocument.SendStringToExecute("Plan2RaumnummernBereinig ", true, false, false);
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

        private bool _LayerRestoreShield = false;
        private void btnLayerRestore_Click(object sender, EventArgs e)
        {
            if (_LayerRestoreShield) return;
            try
            {
                _LayerRestoreShield = true;

                Globs.CancelCommand();

                _AcAp.Application.DocumentManager.MdiActiveDocument.SendStringToExecute("Plan2RestoreLayerStatus ", true, false, false);
            }
            catch (Exception ex)
            {
                _AcAp.Application.ShowAlertDialog(ex.Message);
            }
            finally
            {
                _LayerRestoreShield = false;
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

        private void chkShowSettings_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                grpManually.Visible = chkShowSettings.Checked;
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}
