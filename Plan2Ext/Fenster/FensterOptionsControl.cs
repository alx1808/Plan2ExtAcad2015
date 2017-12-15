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


namespace Plan2Ext.Fenster
{
    public partial class FensterOptionsControl : UserControl
    {
        private FensterOptions _FensterOptions;

        internal FensterOptionsControl(FensterOptions fensterOptions)
        {
            InitializeComponent();

            _FensterOptions = fensterOptions;

            FillComponents();

            rbnStandard.Checked = true;
            rbnSprosse0.Checked = true;

        }

        private void FillComponents()
        {
            this.txtWidth.Text = _FensterOptions.BreiteString;
            this.txtHeight.Text = _FensterOptions.HoeheString;
            this.txtParapet.Text = _FensterOptions.ParapetString;
            this.txtOlAb.Text = _FensterOptions.OlAbString;
            this.txtStaerke.Text = _FensterOptions.StaerkeString;
            this.txtStock.Text = _FensterOptions.StockString;
            this.txtSprossenBreite.Text = _FensterOptions.SprossenBreiteString;
            this.txtAbstand.Text = _FensterOptions.TextAbstandString;
            this.txtWeiteTol.Text = _FensterOptions.WeitePruefTolString;
            txtFluegelStaerke.Text = _FensterOptions.FluegelStaerkeString;

        }

        internal void SetFensterOptions(FensterOptions fensterOptions)
        {
            _FensterOptions = fensterOptions;
            FillComponents();
        }

        private bool txtWidth_Shield = false;
        private void txtWidth_Validating(object sender, CancelEventArgs e)
        {
            if (txtWidth_Shield) return;
            try
            {
                txtWidth_Shield = true;
                _FensterOptions.BreiteString = txtWidth.Text;
                txtWidth.Text = _FensterOptions.BreiteString;

            }
            catch (Exception ex)
            {
                _AcAp.Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in FensterOptions aufgetreten! {0}", ex.Message));
            }
            finally
            {
                txtWidth_Shield = false;
            }
        }

        private bool txtAbstand_Shield = false;
        private void txtAbstand_Validating(object sender, CancelEventArgs e)
        {
            if (txtAbstand_Shield) return;
            try
            {
                txtAbstand_Shield = true;
                _FensterOptions.TextAbstandString = txtAbstand.Text;
                txtAbstand.Text = _FensterOptions.TextAbstandString;

            }
            catch (Exception ex)
            {
                _AcAp.Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in FensterOptions aufgetreten! {0}", ex.Message));
            }
            finally
            {
                txtAbstand_Shield = false;
            }
        }



        private bool txtHeight_Shield = false;
        private void txtHeight_Validating(object sender, CancelEventArgs e)
        {
            if (txtHeight_Shield) return;
            try
            {
                txtHeight_Shield = true;
                _FensterOptions.HoeheString = txtHeight.Text;
                txtHeight.Text = _FensterOptions.HoeheString;

            }
            catch (Exception ex)
            {
                _AcAp.Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in FensterOptions aufgetreten! {0}", ex.Message));
            }
            finally
            {
                txtHeight_Shield = false;
            }

        }

        private bool txtParapet_Shield = false;
        private void txtParapet_Validating(object sender, CancelEventArgs e)
        {
            if (txtParapet_Shield) return;
            try
            {
                txtParapet_Shield = true;
                _FensterOptions.ParapetString = txtParapet.Text;
                txtParapet.Text = _FensterOptions.ParapetString;

            }
            catch (Exception ex)
            {
                _AcAp.Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in FensterOptions aufgetreten! {0}", ex.Message));
            }
            finally
            {
                txtParapet_Shield = false;
            }
        }

        private bool txtOlAb_Shield = false;
        private void txtOlAb_Validating(object sender, CancelEventArgs e)
        {
            if (txtOlAb_Shield) return;
            try
            {
                txtOlAb_Shield = true;
                _FensterOptions.OlAbString = txtOlAb.Text;
                txtOlAb.Text = _FensterOptions.OlAbString;

            }
            catch (Exception ex)
            {
                _AcAp.Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in FensterOptions aufgetreten! {0}", ex.Message));
            }
            finally
            {
                txtOlAb_Shield = false;
            }
        }

        private bool txtStaerke_Shield = false;
        private void txtStaerke_Validating(object sender, CancelEventArgs e)
        {
            if (txtStaerke_Shield) return;
            try
            {
                txtStaerke_Shield = true;
                _FensterOptions.StaerkeString = txtStaerke.Text;
                txtStaerke.Text = _FensterOptions.StaerkeString;

            }
            catch (Exception ex)
            {
                _AcAp.Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in FensterOptions aufgetreten! {0}", ex.Message));
            }
            finally
            {
                txtStaerke_Shield = false;
            }
        }

        private bool txtWeiteTol_Shield = false;
        private void txtWeiteTol_Validating(object sender, CancelEventArgs e)
        {
            if (txtWeiteTol_Shield) return;
            try
            {
                txtWeiteTol_Shield = true;
                _FensterOptions.WeitePruefTolString = txtWeiteTol.Text;
                txtWeiteTol.Text = _FensterOptions.WeitePruefTolString;

            }
            catch (Exception ex)
            {
                _AcAp.Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in FensterOptions aufgetreten! {0}", ex.Message));
            }
            finally
            {
                txtWeiteTol_Shield = false;
            }
        }

        private bool txtStock_Shield = false;
        private void txtStock_Validating(object sender, CancelEventArgs e)
        {
            if (txtStock_Shield) return;
            try
            {
                txtStock_Shield = true;
                _FensterOptions.StockString = txtStock.Text;
                txtStock.Text = _FensterOptions.StockString;

            }
            catch (Exception ex)
            {
                _AcAp.Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in FensterOptions aufgetreten! {0}", ex.Message));
            }
            finally
            {
                txtStock_Shield = false;
            }
        }

        private bool txtFluegelStaerke_Shield = false;
        private void txtFluegelStaerke_Validating(object sender, CancelEventArgs e)
        {
            if (txtFluegelStaerke_Shield) return;
            try
            {
                txtFluegelStaerke_Shield = true;
                _FensterOptions.FluegelStaerkeString = txtFluegelStaerke.Text;
                txtFluegelStaerke.Text = _FensterOptions.FluegelStaerkeString;

            }
            catch (Exception ex)
            {
                _AcAp.Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in FensterOptions aufgetreten! {0}", ex.Message));
            }
            finally
            {
                txtFluegelStaerke_Shield = false;
            }
        }

        private void btnExamine_Click(object sender, EventArgs e)
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
                using (_AcAp.DocumentLock m_doclock = doc.LockDocument())
                {

                    var examiner = new Examiner();
                    Examiner.Weite_Eps = _FensterOptions.WeitePruefTol;
                    var nrErrors = examiner.CheckWindowWidth();
                    if (nrErrors == 0)
                    {
                        _AcAp.Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage(string.Format(CultureInfo.InvariantCulture, "\nFensterprüfung erfolgreich."));
                    }
                    else
                    {
                        _AcAp.Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage(string.Format(CultureInfo.InvariantCulture, "\nFensterprüfung: Anzahl der Fehler: {0}.",nrErrors));
                    }
                }
            }
            catch (Exception ex)
            {
                _AcAp.Application.ShowAlertDialog(ex.Message);
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




                PromptDoubleResult per = ed.GetDistance("\nFensterbreite zeigen: ");

                //DocumentLock loc = dm.MdiActiveDocument.LockDocument();
                //using (loc)
                //{
                //}

                if (per.Status == PromptStatus.OK)
                {

                    _FensterOptions.Breite = per.Value;
                    txtWidth.Text = _FensterOptions.BreiteString;

                }

                _AcAp.Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\n");

            }
            catch (Exception ex)
            {
                _AcAp.Application.ShowAlertDialog(ex.Message);
            }

        }

        private void btnSelAbstand_Click(object sender, EventArgs e)
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




                PromptDoubleResult per = ed.GetDistance("\nTextabstand zeigen: ");

                if (per.Status == PromptStatus.OK)
                {

                    _FensterOptions.TextAbstand = per.Value;
                    txtAbstand.Text = _FensterOptions.TextAbstandString;

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
                    _FensterOptions.Hoehe = height;
                    txtHeight.Text = _FensterOptions.HoeheString;

                }
                _AcAp.Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\n");

            }
            catch (Exception ex)
            {
                _AcAp.Application.ShowAlertDialog(ex.Message);
            }

        }

        private void btnSelParapet_Click(object sender, EventArgs e)
        {
            try
            {
                double height = 0.0;
                if (GetHeightFromVermBlocks(ref height))
                {
                    _FensterOptions.Parapet = height;
                    txtParapet.Text = _FensterOptions.ParapetString;

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

        private bool txtSprossenBreite_Shield = false;
        private void txtSprossenBreite_Validating(object sender, CancelEventArgs e)
        {
            if (txtSprossenBreite_Shield) return;
            try
            {
                txtSprossenBreite_Shield = true;
                _FensterOptions.SprossenBreiteString = txtSprossenBreite.Text;
                txtSprossenBreite.Text = _FensterOptions.SprossenBreiteString;

            }
            catch (Exception ex)
            {
                _AcAp.Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in FensterOptions aufgetreten! {0}", ex.Message));
            }
            finally
            {
                txtSprossenBreite_Shield = false;
            }

        }


        private void cmbArt_SelectedIndexChanged(object sender, EventArgs e)
        {
            //if (_FensterOptions.FensterArt != cmbArt.Text)
            //{
            //    _FensterOptions.FensterArt = cmbArt.Text;
            //    if (_FensterOptions.FensterArt == "Standard")
            //    {
            //        this.txtFluegelStaerke.Visible = false;
            //        this.lblFluegelStaerke.Visible = false;
            //        _FensterOptions.Staerke = 0.07;
            //        this.txtStaerke.Text = _FensterOptions.StaerkeString;
            //    }
            //    else
            //    {
            //        this.txtFluegelStaerke.Visible = true;
            //        this.lblFluegelStaerke.Visible = true;
            //        _FensterOptions.Staerke = 0.20;
            //        this.txtStaerke.Text = _FensterOptions.StaerkeString;
            //    }

            //}

        }

        private void rbnStandard_CheckedChanged(object sender, EventArgs e)
        {
            if (rbnStandard.Checked)
            {
                if (_FensterOptions.FensterArt != FensterOptions.FenArt.Standard)
                {
                    _FensterOptions.FensterArt = FensterOptions.FenArt.Standard;
                }
                this.txtFluegelStaerke.Visible = false;
                this.lblFluegelStaerke.Visible = false;
                _FensterOptions.Staerke = 0.07;
                this.txtStaerke.Text = _FensterOptions.StaerkeString;

            }
            //else
            //{
            //    if (_FensterOptions.FensterArt =
            //    {
            //        _FensterOptions.FensterArt = "Kastenfenster";
            //        this.txtFluegelStaerke.Visible = true;
            //        this.lblFluegelStaerke.Visible = true;
            //        _FensterOptions.Staerke = 0.20;
            //        this.txtStaerke.Text = _FensterOptions.StaerkeString;
            //    }
            //}
        }

        private void rbnKasten_CheckedChanged(object sender, EventArgs e)
        {
            if (rbnKasten.Checked)
            {
                if (_FensterOptions.FensterArt != FensterOptions.FenArt.Kasten)
                {
                    _FensterOptions.FensterArt = FensterOptions.FenArt.Kasten;
                }
                this.txtFluegelStaerke.Visible = true;
                this.lblFluegelStaerke.Visible = true;
                _FensterOptions.Staerke = 0.20;
                this.txtStaerke.Text = _FensterOptions.StaerkeString;

            }
        }

        private void rbnSprosse0_CheckedChanged(object sender, EventArgs e)
        {
            if (rbnSprosse0.Checked) _FensterOptions.Sprossen = 0;
        }

        private void rbnSprosse1_CheckedChanged(object sender, EventArgs e)
        {
            if (rbnSprosse1.Checked) _FensterOptions.Sprossen = 1;
        }

        private void rbnSprosse2_CheckedChanged(object sender, EventArgs e)
        {
            if (rbnSprosse2.Checked) _FensterOptions.Sprossen = 2;
        }

        private void btnFenster_Click(object sender, EventArgs e)
        {
            try
            {
                //CADDZone.AutoCAD.Samples.AcedInvokeSample.InvokeCLisp("FEN"); // funkt nicht mit fensterbefehl
                // Rivilis.CSharpToLisp.test(); different entrypoints for each acad-version

                // build the arguments list // -> funkt auch nicht mit fensterbefehl
                //ResultBuffer args = new ResultBuffer(new TypedValue((int)Autodesk.AutoCAD.Runtime.LispDataType.Text, "C:FEN")
                //    //new TypedValue((int)LispDataType.ListBegin),
                //    //new TypedValue((int)LispDataType.Text, "T-600"),
                //    //new TypedValue((int)LispDataType.Text, "t")
                //    //new TypedValue((int)LispDataType.ListEnd)
                //);
                //// call the LISP fuction anf get the return value 
                //ResultBuffer result = _AcAp.Application.Invoke(args);
                //// print the return value
                //Editor ed = _AcAp.Application.DocumentManager.MdiActiveDocument.Editor;
                //ed.WriteMessage("result.toString()");


            }
            catch (Exception ex)
            {
                _AcAp.Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Befehl Fenster aufgetreten! {0}", ex.Message));
            }
        }
    }
}
