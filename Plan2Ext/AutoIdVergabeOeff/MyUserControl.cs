using System;
using System.Globalization;
using System.Windows.Forms;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
// ReSharper disable IdentifierTypo
// ReSharper disable StringLiteralTypo

namespace Plan2Ext.AutoIdVergabeOeff
{
    public partial class MyUserControl : UserControl
    {
        #region log4net Initialization
        // ReSharper disable once UnusedMember.Local
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(Convert.ToString((typeof(MyUserControl))));
        #endregion

        public KindOfStartEnum KindOfStart { get; private set; }

        public MyUserControl()
        {
            InitializeComponent();
        }


        private void txtFenNummer_TextChanged(object sender, EventArgs e)
        {
            int nr;
            if (!int.TryParse(txtFenNummer.Text, out nr))
            {
                // ReSharper disable once LocalizableElement
                txtFenNummer.Text = "1";
            }
        }

        private void btnStartTueren_Click(object sender, EventArgs e)
        {
            Start(KindOfStartEnum.Tueren);
        }

        private void btnStartAlle_Click(object sender, EventArgs e)
        {
            Start(KindOfStartEnum.Alle);
        }


        private bool _startShield;
        private void btnStart_Click(object sender, EventArgs e)
        {
            Start(KindOfStartEnum.Fenster);
        }

        private void Start(KindOfStartEnum kindOfStart)
        {
            if (_startShield) return;

            try
            {
                _startShield = true;

                CancelCommand();

                using (Application.DocumentManager.MdiActiveDocument.LockDocument())
                {
#if NEWSETFOCUS
                    Application.DocumentManager.MdiActiveDocument.Window.Focus();
#else
                Autodesk.AutoCAD.Internal.Utils.SetFocusToDwgView(); // previous 2014 AutoCAD - Versions
#endif

                    KindOfStart = kindOfStart;

                    Application.DocumentManager.MdiActiveDocument.SendStringToExecute("Plan2AutoIdVergabeOeffnungen ", true,
                        false, false);
                }
            }
            catch (Exception ex)
            {
                Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture,
                    "Fehler in Plan2AutoIdVergabeOeffnungen aufgetreten! {0}", ex.Message));
            }
            finally
            {
                _startShield = false;
            }
        }

        private static void CancelCommand()
        {
            Application.DocumentManager.MdiActiveDocument.SendStringToExecute("\x1B", true, false, true);
            Application.DocumentManager.MdiActiveDocument.SendStringToExecute("\x1B", true, false, true);
        }

        private bool _eindeutigkeitShield;
        private void btnEindeutigkeit_Click(object sender, EventArgs e)
        {
            if (_eindeutigkeitShield) return;
            try
            {
                _eindeutigkeitShield = true;

                Globs.CancelCommand();

                Application.DocumentManager.MdiActiveDocument.SendStringToExecute("Plan2AutoIdVergabeOeffnungenEindeutigkeit ", true, false, false);
            }
            catch (Exception ex)
            {
                Application.ShowAlertDialog(ex.Message);
            }
            finally
            {
                _eindeutigkeitShield = false;
            }
        }
    }
}
