using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Autodesk.AutoCAD.DatabaseServices;
using System.Globalization;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;

namespace Plan2Ext.CalcArea
{
    public partial class CalcAreaControl : UserControl
    {
        #region log4net Initialization
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Convert.ToString((typeof(CalcAreaControl))));
        #endregion

        #region Lifecycle
        public CalcAreaControl()
        {
            InitializeComponent();
        }
        #endregion

        #region Control Events

        private void btnCalcArea_Click(object sender, EventArgs e)
        {
            try
            {
#if NEWSETFOCUS
                Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Window.Focus();
#else
                Autodesk.AutoCAD.Internal.Utils.SetFocusToDwgView(); // previous 2014 AutoCAD - Versions
#endif

                Plan2Ext.Flaeche.AktFlaeche(Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument,
                    txtBlockname.Text,
                    txtAttribute.Text,
                    txtFG.Text,
                    txtAG.Text
                  );
            }
            catch (Exception ex)
            {
                Autodesk.AutoCAD.ApplicationServices.Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler aufgetreten: '{0}'", ex.Message));
            }

            //if (_AktFlaecheDelegate != null)
            //{


            //    _AktFlaecheDelegate(Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument,
            //        txtBlockname.Text,
            //        txtAttribute.Text,
            //        txtFG.Text,
            //        txtAG.Text
            //      );
            //}
        }

        private void btnSelectBlock_Click(object sender, EventArgs e)
        {
            try
            {
                DocumentCollection dm = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager;
                Document doc = dm.MdiActiveDocument;
                Editor ed = doc.Editor;

#if NEWSETFOCUS
                Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Window.Focus();
#else
                Autodesk.AutoCAD.Internal.Utils.SetFocusToDwgView(); // previous 2014 AutoCAD - Versions
#endif

                PromptEntityResult per = ed.GetEntity("\nRaumblock wählen: ");

                //DocumentLock loc = dm.MdiActiveDocument.LockDocument();
                //using (loc)
                //{
                //}

                if (per.Status == PromptStatus.OK)
                {

                    Transaction tr = doc.TransactionManager.StartTransaction();
                    using (tr)
                    {
                        DBObject obj = tr.GetObject(per.ObjectId, OpenMode.ForRead);
                        BlockReference br = obj as BlockReference;
                        if (br == null) return;

                        txtBlockname.Text = br.Name;
                        tr.Commit();
                    }

                    per = ed.GetNestedEntity("\nAttribut wählen: ");

                    if (per.Status == PromptStatus.OK)
                    {

                        tr = doc.TransactionManager.StartTransaction();
                        using (tr)
                        {
                            DBObject obj = tr.GetObject(per.ObjectId, OpenMode.ForRead);
                            AttributeReference ar = obj as AttributeReference;
                            if (ar == null) return;
                            txtAttribute.Text = ar.Tag;
                            tr.Commit();
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Autodesk.AutoCAD.ApplicationServices.Application.ShowAlertDialog(ex.Message);
            }

        }

        private void btnFG_Click(object sender, EventArgs e)
        {
            try
            {
                DocumentCollection dm = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager;
                Document doc = dm.MdiActiveDocument;
                Editor ed = doc.Editor;

#if NEWSETFOCUS
                Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Window.Focus();
#else
                Autodesk.AutoCAD.Internal.Utils.SetFocusToDwgView(); // previous 2014 AutoCAD - Versions
#endif

                PromptEntityResult per = ed.GetEntity("\nFlächengrenze wählen: ");

                if (per.Status == PromptStatus.OK)
                {

                    Transaction tr = doc.TransactionManager.StartTransaction();
                    using (tr)
                    {
                        string layer = string.Empty;
                        DBObject obj = tr.GetObject(per.ObjectId, OpenMode.ForRead);
                        layer = GetPolylineLayer(obj);
                        if (string.IsNullOrEmpty(layer)) return;

                        if (string.Compare(txtAG.Text, layer, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            Autodesk.AutoCAD.ApplicationServices.Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Die Layer für Flächengrenze und Abzugsfläche müssen unterschiedlich sein."));
                            return;
                        }

                        txtFG.Text = layer;
                        tr.Commit();
                    }

                }

            }
            catch (Exception ex)
            {
                Autodesk.AutoCAD.ApplicationServices.Application.ShowAlertDialog(ex.Message);
            }
        }

        private void btnAG_Click(object sender, EventArgs e)
        {
            try
            {
                DocumentCollection dm = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager;
                Document doc = dm.MdiActiveDocument;
                Editor ed = doc.Editor;

#if NEWSETFOCUS
                Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Window.Focus();
#else
                Autodesk.AutoCAD.Internal.Utils.SetFocusToDwgView(); // previous 2014 AutoCAD - Versions
#endif

                PromptEntityResult per = ed.GetEntity("\nAbzugsfläche wählen: ");

                if (per.Status == PromptStatus.OK)
                {

                    Transaction tr = doc.TransactionManager.StartTransaction();
                    using (tr)
                    {
                        string layer = string.Empty;
                        DBObject obj = tr.GetObject(per.ObjectId, OpenMode.ForRead);
                        layer = GetPolylineLayer(obj);
                        if (string.IsNullOrEmpty(layer)) return;

                        if (string.Compare(txtFG.Text, layer, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            Autodesk.AutoCAD.ApplicationServices.Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Die Layer für Flächengrenze und Abzugsfläche müssen unterschiedlich sein."));
                            return;
                        }

                        txtAG.Text = layer;
                        tr.Commit();
                    }

                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void txtBlockname_TextChanged(object sender, EventArgs e)
        {
            HandleControls();
        }

        private void txtAttribute_TextChanged(object sender, EventArgs e)
        {
            HandleControls();
        }

        private void txtFG_TextChanged(object sender, EventArgs e)
        {
            HandleControls();
        }

        private void txtAG_TextChanged(object sender, EventArgs e)
        {
            HandleControls();
        }

        #endregion

        #region Private
        private static string GetPolylineLayer(DBObject obj)
        {
            Polyline pline = obj as Polyline;
            if (pline != null)
            {
                return pline.Layer;
            }
            else
            {
                Polyline2d pl = obj as Polyline2d;
                if (pl != null) return pl.Layer;

            }
            return string.Empty;
        }

        private void HandleControls()
        {
            if (string.IsNullOrEmpty(txtBlockname.Text) || string.IsNullOrEmpty(txtAttribute.Text) || string.IsNullOrEmpty(txtFG.Text) || string.IsNullOrEmpty(txtAG.Text))
                btnCalcArea.Enabled = false;
            else btnCalcArea.Enabled = true;
        }

        #endregion

        #region Internal
        Flaeche.AktFlaecheDelegate _AktFlaecheDelegate;
        internal void SetAktFlaecheDelegate(Flaeche.AktFlaecheDelegate aktFlaecheDelegate)
        {

            _AktFlaecheDelegate = aktFlaecheDelegate;
        }
        #endregion

    }
}
