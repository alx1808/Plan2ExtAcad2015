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
    //using Teigha.DatabaseServices;
    //using Bricscad.EditorInput;
    //using _AcBrx = Bricscad.Runtime;
#elif ARX_APP
  using _AcAp = Autodesk.AutoCAD.ApplicationServices;
  //using Autodesk.AutoCAD.DatabaseServices;
  //using Autodesk.AutoCAD.EditorInput;
  //using _AcBrx = Autodesk.AutoCAD.Runtime;
#endif

namespace Plan2Ext.AutoIdVergabe
{
    public partial class AutoIdControl : UserControl
    {
        #region log4net Initialization
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Convert.ToString((typeof(AutoIdControl))));
        #endregion

        private AutoIdOptions _AutoIdOptions = null;
        public AutoIdControl(AutoIdOptions options)
        {
            InitializeComponent();

            lvZuweisungen.View = View.Details;
            lvZuweisungen.GridLines = true;
            lvZuweisungen.FullRowSelect = true;
            //Add column header
            lvZuweisungen.Columns.Add("Von", 90);
            lvZuweisungen.Columns.Add("Nach", 90);
            //Add items in the listview
            string[] arr = new string[3];
            ListViewItem itm;

            _AutoIdOptions = options;
            _AutoIdOptions.Form = this;
            Globs.TheAutoIdOptions = _AutoIdOptions;

            FillComponents();

            InitDocEvents();

            SetLvZuweisungen();

        }

        internal void SetLvZuweisungen()
        {

            lvZuweisungen.Items.Clear();

            var zuweisungen = _AutoIdOptions.GetAssignmentsDict();
            if (zuweisungen != null)
            {
                foreach (var zw in zuweisungen)
                {
                    ListViewItem lvi = new ListViewItem(zw.ToArr());
                    lvZuweisungen.Items.Add(lvi);
                }
            }
        }

        private void InitDocEvents()
        {
            try
            {
                _AcAp.DocumentCollection dc = _AcAp.Application.DocumentManager as _AcAp.DocumentCollection;
                
                dc.DocumentActivated -= dc_DocumentActivated;
                dc.DocumentActivated += dc_DocumentActivated;

                //dc.DocumentCreated -= dc_DocumentCreated;
                //dc.DocumentCreated += dc_DocumentCreated;

                //dc.DocumentCreateStarted -= dc_DocumentCreateStarted;
                //dc.DocumentCreateStarted += dc_DocumentCreateStarted;


                
            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
            }
        }

        //void dc_DocumentCreateStarted(object sender, _AcAp.DocumentCollectionEventArgs e)
        //{
        //    try
        //    {
        //        e.Document.EndDwgOpen += Document_EndDwgOpen;
        //    }
        //    catch (Exception ex)
        //    {
        //        log.Error(ex.Message, ex);
        //    }
        //}

        void dc_DocumentActivated(object sender, _AcAp.DocumentCollectionEventArgs e)
        {
            try
            {
                SetLvZuweisungen();
            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
            }
        }

        //void dc_DocumentCreated(object sender, _AcAp.DocumentCollectionEventArgs e)
        //{
        //    try
        //    {
        //        e.Document.EndDwgOpen += Document_EndDwgOpen;
        //    }
        //    catch (Exception ex)
        //    {
        //        log.Error(ex.Message, ex);
        //    }
        //}

        //void Document_EndDwgOpen(object sender, _AcAp.DrawingOpenEventArgs e)
        //{
        //    try
        //    {
        //        SetLvZuweisungen();
        //        //_AutoIdOptions.GetAssignmentsDict();
        //    }
        //    catch (Exception ex)
        //    {
        //        log.Error(ex.Message, ex);
        //    }
        //}

        private void FillComponents()
        {
            txtBlockname.Text = _AutoIdOptions.Blockname;
            txtTuerschildnummer.Text = _AutoIdOptions.AttTuerschildnummer;
            txtIdNummer.Text = _AutoIdOptions.AttIdNummer;

            txtLiegenschaft.Text = _AutoIdOptions.Liegenschaft;
            txtObjekt.Text = _AutoIdOptions.Objekt;
            txtGeschoss.Text = _AutoIdOptions.Geschoss;
            txtArial.Text = _AutoIdOptions.Arial;

            txtStelle.Text = _AutoIdOptions.RaumnummerAbStelle.ToString(CultureInfo.InvariantCulture);
            if (_AutoIdOptions.RaumnummerBisStelle > _AutoIdOptions.RaumnummerAbStelle )
            {
                txtBisStelle.Text = _AutoIdOptions.RaumnummerBisStelle.ToString(CultureInfo.InvariantCulture);
            }
            else
            {
                txtBisStelle.Text = "";
            }
        }

        private void AutoIdControl_Load(object sender, EventArgs e)
        {

        }

        private void txtBlockname_TextChanged(object sender, EventArgs e)
        {
            _AutoIdOptions.Blockname = txtBlockname.Text;
        }

        private void txtTuerschildnummer_TextChanged(object sender, EventArgs e)
        {
            _AutoIdOptions.AttTuerschildnummer = txtTuerschildnummer.Text;
        }

        private void txtIdNummer_TextChanged(object sender, EventArgs e)
        {
            _AutoIdOptions.AttIdNummer = txtIdNummer.Text;
        }

        private void txtLiegenschaft_TextChanged(object sender, EventArgs e)
        {
            _AutoIdOptions.Liegenschaft = txtLiegenschaft.Text;
        }

        private void txtObjekt_TextChanged(object sender, EventArgs e)
        {
            _AutoIdOptions.Objekt = txtObjekt.Text;
        }

        private void txtGeschoss_TextChanged(object sender, EventArgs e)
        {
            _AutoIdOptions.Geschoss = txtGeschoss.Text;
        }

        private void txtArial_TextChanged(object sender, EventArgs e)
        {
            _AutoIdOptions.Arial = txtArial.Text;
        }

        private bool txtStelle_TextChanged_Shield = false;
        private void txtStelle_TextChanged(object sender, EventArgs e)
        {
            if (txtStelle_TextChanged_Shield) return;

            try
            {
                txtStelle_TextChanged_Shield = true;
                string txt = txtStelle.Text;
                if (string.IsNullOrEmpty(txt)) return;

                int i;
                if (!int.TryParse(txt, out i)) i = 1;
                _AutoIdOptions.RaumnummerAbStelle = i;

                string validValue = _AutoIdOptions.RaumnummerAbStelle.ToString(CultureInfo.InvariantCulture);
                if (validValue == txt) return;

                txtStelle.Text = validValue;
            }
            finally
            {
                txtStelle_TextChanged_Shield = false;
            }
        }

        private void txtStelle_Validating(object sender, CancelEventArgs e)
        {
            if (string.IsNullOrEmpty(txtStelle.Text))
            {
                int i = 1;
                _AutoIdOptions.RaumnummerAbStelle = i;
                txtStelle.Text = _AutoIdOptions.RaumnummerAbStelle.ToString(CultureInfo.InvariantCulture);
            }
        }

        private bool txtBisStelle_TextChanged_Shield = false;
        private void txtBisStelle_TextChanged(object sender, EventArgs e)
        {
            if (txtBisStelle_TextChanged_Shield) return;

            try
            {
                txtBisStelle_TextChanged_Shield = true;
                string txt = txtBisStelle.Text;
                if (string.IsNullOrEmpty(txt)) return;

                int i;
                if (!int.TryParse(txt, out i)) i = -1;
                _AutoIdOptions.RaumnummerBisStelle = i;

                if (_AutoIdOptions.RaumnummerBisStelle > _AutoIdOptions.RaumnummerAbStelle)
                {
                    string validValue = _AutoIdOptions.RaumnummerBisStelle.ToString(CultureInfo.InvariantCulture);
                    if (validValue == txt) return;

                    txtBisStelle.Text = validValue;
                }
            }
            finally
            {
                txtBisStelle_TextChanged_Shield = false;
            }
        }

        private void txtBisStelle_Validating(object sender, CancelEventArgs e)
        {
            txtBisStelle.Text = txtBisStelle.Text.Trim();
            if (string.IsNullOrEmpty(txtBisStelle.Text))
            {
                _AutoIdOptions.RaumnummerBisStelle = -1;
            }
            else
            {
                if (_AutoIdOptions.RaumnummerBisStelle <= _AutoIdOptions.RaumnummerAbStelle)
                {
                    _AutoIdOptions.RaumnummerBisStelle = -1;
                    txtBisStelle.Text = "";
                }
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

                _AcAp.Application.DocumentManager.MdiActiveDocument.SendStringToExecute("Plan2AutoIdSelBlockAndAtt ", true, false, false);
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

        private bool _ZuordnenShield = false;
        private void btnZuordnen_Click(object sender, EventArgs e)
        {
            if (_ZuordnenShield) return;
            try
            {
                _ZuordnenShield = true;

                Globs.CancelCommand();

                _AcAp.Application.DocumentManager.MdiActiveDocument.SendStringToExecute("Plan2AutoIdVergabe ", true, false, false);
            }
            catch (Exception ex)
            {
                _AcAp.Application.ShowAlertDialog(ex.Message);
            }
            finally
            {
                _ZuordnenShield = false;
            }
        }

        private bool _EindeutigkeitShield = false;
        private void btnEindeutigkeit_Click(object sender, EventArgs e)
        {
            if (_EindeutigkeitShield) return;
            try
            {
                _EindeutigkeitShield = true;

                Globs.CancelCommand();

                _AcAp.Application.DocumentManager.MdiActiveDocument.SendStringToExecute("Plan2AutoIdEindeutigkeit ", true, false, false);
            }
            catch (Exception ex)
            {
                _AcAp.Application.ShowAlertDialog(ex.Message);
            }
            finally
            {
                _EindeutigkeitShield = false;
            }
        }

        private bool _SelPolygonLayerShield = false;
        private void btnSelPolygonLayer_Click(object sender, EventArgs e)
        {
            if (_SelPolygonLayerShield) return;
            try
            {
                _SelPolygonLayerShield = true;

                Globs.CancelCommand();

                _AcAp.Application.DocumentManager.MdiActiveDocument.SendStringToExecute("Plan2AutoIdSelPolygonLayer ", true, false, false);
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

        private bool _SelToRaumIdAttShield = false;
        private void btnSelToRaumIdAtt_Click(object sender, EventArgs e)
        {
            if (_SelToRaumIdAttShield) return;
            try
            {
                _SelToRaumIdAttShield = true;

                Globs.CancelCommand();

                _AcAp.Application.DocumentManager.MdiActiveDocument.SendStringToExecute("Plan2AutoIdAssignments ", true, false, false);

            }
            catch (Exception ex)
            {
                _AcAp.Application.ShowAlertDialog(ex.Message);
            }
            finally
            {
                _SelToRaumIdAttShield = false;
            }
        }

        private bool _ZuRaumIdVergabeShield = false;
        private void btnZuRaumIdVergabe_Click(object sender, EventArgs e)
        {
            if (_ZuRaumIdVergabeShield) return;
            try
            {
                _ZuRaumIdVergabeShield = true;

                Globs.CancelCommand();

                _AcAp.Application.DocumentManager.MdiActiveDocument.SendStringToExecute("Plan2AutoIdZuRaumIdVergabe ", true, false, false);
            }
            catch (Exception ex)
            {
                _AcAp.Application.ShowAlertDialog(ex.Message);
            }
            finally
            {
                _ZuRaumIdVergabeShield = false;
            }
        }

        private bool _ZuRaumIdVergabeAttributShield = false;
        private void btnZuRaumIdVergabeAttribut_Click(object sender, EventArgs e)
        {
            if (_ZuRaumIdVergabeAttributShield) return;
            try
            {
                _ZuRaumIdVergabeAttributShield = true;

                Globs.CancelCommand();

                _AcAp.Application.DocumentManager.MdiActiveDocument.SendStringToExecute("Plan2AutoIdZuRaumIdVergabeAttribut ", true, false, false);
            }
            catch (Exception ex)
            {
                _AcAp.Application.ShowAlertDialog(ex.Message);
            }
            finally
            {
                _ZuRaumIdVergabeAttributShield = false;
            }
        }

        private void txtPolygonLayer_TextChanged(object sender, EventArgs e)
        {
            _AutoIdOptions.PolygonLayer = txtPolygonLayer.Text;
        }

        private void txtZuRaumIdAtt_TextChanged(object sender, EventArgs e)
        {
            //_AutoIdOptions.ZuRaumIdAtt = txtZuRaumIdAtt.Text;
        }

        private void lblPolygonLayer_Click(object sender, EventArgs e)
        {

        }

        private bool _ExcelExportShield = false;
        private void btnExcelExport_Click(object sender, EventArgs e)
        {
            if (_ExcelExportShield) return;
            try
            {
                _ExcelExportShield = true;

                Globs.CancelCommand();

                _AcAp.Application.DocumentManager.MdiActiveDocument.SendStringToExecute("Plan2AutoIdExcelExport ", true, false, false);
            }
            catch (Exception ex)
            {
                _AcAp.Application.ShowAlertDialog(ex.Message);
            }
            finally
            {
                _ExcelExportShield = false;
            }
        }

        private bool _ExcelImportShield = false;
        private void btnExcelImport_Click(object sender, EventArgs e)
        {
            if (_ExcelImportShield) return;
            try
            {
                _ExcelImportShield = true;

                Globs.CancelCommand();

                _AcAp.Application.DocumentManager.MdiActiveDocument.SendStringToExecute("Plan2AutoIdExcelImport ", true, false, false);
            }
            catch (Exception ex)
            {
                _AcAp.Application.ShowAlertDialog(ex.Message);
            }
            finally
            {
                _ExcelImportShield = false;
            }
        }

        private void lblStelle_Click(object sender, EventArgs e)
        {

        }

    }
}
