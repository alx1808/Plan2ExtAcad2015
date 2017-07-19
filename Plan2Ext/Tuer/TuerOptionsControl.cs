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
//using Autodesk.AutoCAD.EditorInput;
//using Autodesk.AutoCAD.DatabaseServices;
//using Autodesk.AutoCAD.ApplicationServices;

#if BRX_APP
using _AcAp = Bricscad.ApplicationServices;
using Teigha.DatabaseServices;
using Bricscad.EditorInput;
#elif ARX_APP
  using _AcAp = Autodesk.AutoCAD.ApplicationServices;
  using Autodesk.AutoCAD.DatabaseServices;
  using Autodesk.AutoCAD.EditorInput;
#endif

namespace Plan2Ext.Tuer
{
    public partial class TuerOptionsControl : UserControl
    {
        private TuerOptions _TuerOptions;

        internal TuerOptionsControl(TuerOptions TuerOptions)
        {
            InitializeComponent();

            _TuerOptions = TuerOptions;

            FillComponents();

            rbnUmfassung.Checked = true;
            rbnEins.Checked = true;
            rbnTb4.Checked = true;

            CheckStaerkeVisible();

        }

        private void FillComponents()
        {
            this.txtWidth.Text = _TuerOptions.BreiteString;
            this.txtHeight.Text = _TuerOptions.HoeheString;
            this.txtStockStaerke.Text = _TuerOptions.StockStaerkeString;

        }


        internal void SetTuerOptions(TuerOptions tuerOptions)
        {
            _TuerOptions = tuerOptions;
            FillComponents();
        }

        private bool txtWidth_Shield = false;
        private void txtWidth_Validating(object sender, CancelEventArgs e)
        {
            if (txtWidth_Shield) return;
            try
            {
                txtWidth_Shield = true;
                _TuerOptions.BreiteString = txtWidth.Text;
                txtWidth.Text = _TuerOptions.BreiteString;

            }
            catch (Exception ex)
            {
                _AcAp.Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in TuerOptions aufgetreten! {0}", ex.Message));
            }
            finally
            {
                txtWidth_Shield = false;
            }
        }

        private bool txtHeight_Shield = false;
        private void txtHeight_Validating(object sender, CancelEventArgs e)
        {
            if (txtHeight_Shield) return;
            try
            {
                txtHeight_Shield = true;
                _TuerOptions.HoeheString = txtHeight.Text;
                txtHeight.Text = _TuerOptions.HoeheString;

            }
            catch (Exception ex)
            {
                _AcAp.Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in TuerOptions aufgetreten! {0}", ex.Message));
            }
            finally
            {
                txtHeight_Shield = false;
            }

        }

        private bool txtStockStaerke_Shield = false;
        private void txtStockStaerke_Validating(object sender, CancelEventArgs e)
        {
            if (txtStockStaerke_Shield) return;
            try
            {
                txtStockStaerke_Shield = true;
                _TuerOptions.StockStaerkeString= txtStockStaerke.Text;
                txtStockStaerke.Text = _TuerOptions.StockStaerkeString;

            }
            catch (Exception ex)
            {
                _AcAp.Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in TuerOptions aufgetreten! {0}", ex.Message));
            }
            finally
            {
                txtStockStaerke_Shield = false;
            }

        }

        private void btnSelWidth_Click(object sender, EventArgs e)
        {
            try
            {
                _AcAp.DocumentCollection dm = _AcAp.Application.DocumentManager;
                _AcAp.Document doc = dm.MdiActiveDocument;
                Editor ed = doc.Editor;
#if NEWSETFOCUS
                doc.Window.Focus();
#else
                Autodesk.AutoCAD.Internal.Utils.SetFocusToDwgView(); // previous 2014 AutoCAD - Versions
#endif




                PromptDoubleResult per = ed.GetDistance("\nTürbreite zeigen: ");

                //DocumentLock loc = dm.MdiActiveDocument.LockDocument();
                //using (loc)
                //{
                //}

                if (per.Status == PromptStatus.OK)
                {

                    _TuerOptions.Breite = per.Value;
                    txtWidth.Text = _TuerOptions.BreiteString;

                }

                _AcAp.Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\n");

            }
            catch (Exception ex)
            {
                _AcAp.Application.ShowAlertDialog(ex.Message);
            }

        }

        private void butSelHeight_Click(object sender, EventArgs e)
        {
            try
            {
                double height = 0.0;
                if (GetHeightFromVermBlocks(ref height))
                {
                    _TuerOptions.Hoehe = height;
                    txtHeight.Text = _TuerOptions.HoeheString;

                }
                _AcAp.Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\n");

            }
            catch (Exception ex)
            {
                _AcAp.Application.ShowAlertDialog(ex.Message);
            }

        }

        private bool GetHeightFromVermBlocks(ref double height)
        {
            _AcAp.DocumentCollection dm = _AcAp.Application.DocumentManager;
            _AcAp.Document doc = dm.MdiActiveDocument;
            Editor ed = doc.Editor;

#if NEWSETFOCUS
            doc.Window.Focus();
#else
            Autodesk.AutoCAD.Internal.Utils.SetFocusToDwgView(); // previous 2014 AutoCAD - Versions
#endif


            PromptEntityResult per = ed.GetEntity("\nErsten Vermessungsblock wählen: ");

            //DocumentLock loc = dm.MdiActiveDocument.LockDocument();
            //using (loc)
            //{
            //}

            bool blockFound = false;
            double height1 = 0.0;
            double height2 = 0.0;
            if (per.Status == PromptStatus.OK)
            {

                Transaction tr = doc.TransactionManager.StartTransaction();
                using (tr)
                {
                    DBObject obj = tr.GetObject(per.ObjectId, OpenMode.ForRead);

                    BlockReference br = obj as BlockReference;
                    if (br == null) return false;

                    if (br.Name == "GEOINOVA")
                    {
                        blockFound = true;
                        height1 = br.Position.Z;
                        br.Highlight();
                    }

                    if (blockFound)
                    {
                        blockFound = false;
                        per = ed.GetEntity("\nZweiten Vermessungsblock wählen: ");
                        if (per.Status == PromptStatus.OK)
                        {
                            obj = tr.GetObject(per.ObjectId, OpenMode.ForRead);
                            BlockReference br2 = obj as BlockReference;
                            if (br2 == null) return false;

                            if (br2.Name == "GEOINOVA")
                            {
                                blockFound = true;
                                height2 = br2.Position.Z;

                            }

                        }

                        if (blockFound)
                        {
                            height = Math.Abs(height1 - height2);
                        }

                        br.Unhighlight();

                    }

                    tr.Commit();
                }

                if (!blockFound) return false;
                return true;

            }

            return false;
        }

        private void rbnUmfassung_CheckedChanged(object sender, EventArgs e)
        {
            if (rbnUmfassung.Checked)
            {
                if (_TuerOptions.TuerArt != TuerOptions.ZargenArt.Umfassung)
                {
                    _TuerOptions.TuerArt = TuerOptions.ZargenArt.Umfassung;
                    CheckStaerkeVisible();
                }
            }
        }

        private void rbnBlock_CheckedChanged(object sender, EventArgs e)
        {
            if (rbnBlock.Checked)
            {
                if (_TuerOptions.TuerArt != TuerOptions.ZargenArt.Block)
                {
                    _TuerOptions.TuerArt = TuerOptions.ZargenArt.Block;
                    CheckStaerkeVisible();
                }
            }
        }

        private void rbnEck_CheckedChanged(object sender, EventArgs e)
        {
            if (rbnEck.Checked)
            {
                if (_TuerOptions.TuerArt != TuerOptions.ZargenArt.Eck)
                {
                    _TuerOptions.TuerArt = TuerOptions.ZargenArt.Eck;
                    CheckStaerkeVisible();
                }

            }
        }

        private void CheckStaerkeVisible()
        {
            if (rbnBlock.Checked)
            {
                txtStockStaerke.Visible = true;
                lblStockStaerke.Visible = true;
            }
            else
            {
                txtStockStaerke.Visible = false;
                lblStockStaerke.Visible = false;
            }
        }

        private void rbnEins_CheckedChanged(object sender, EventArgs e)
        {
            if (rbnEins.Checked) _TuerOptions.Fluegel = 1;
        }

        private void rbnZwei_CheckedChanged(object sender, EventArgs e)
        {
            if (rbnZwei.Checked) _TuerOptions.Fluegel = 2;
        }

        private void rbnTbStandard_CheckedChanged(object sender, EventArgs e)
        {
            if (rbnTbStandard.Checked)
            {
                if (_TuerOptions.TextBlockTyp != TuerOptions.TextBlockTp.Standard)
                {
                    _TuerOptions.TextBlockTyp = TuerOptions.TextBlockTp.Standard;
                }
            }
        }

        private void rbnTb4_CheckedChanged(object sender, EventArgs e)
        {
            if (rbnTb4.Checked)
            {
                if (_TuerOptions.TextBlockTyp != TuerOptions.TextBlockTp.Vier)
                {
                    _TuerOptions.TextBlockTyp = TuerOptions.TextBlockTp.Vier;
                }
            }
        }



    }
}
