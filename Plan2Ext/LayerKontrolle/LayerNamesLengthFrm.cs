using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using AcApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;
// ReSharper disable LocalizableElement

namespace Plan2Ext.LayerKontrolle
{
    public partial class LayerNamesLengthFrm : Form
    {
        private bool _layerRenamed;
        private System.Drawing.Color _btnRenameBackColorOrig;

        public LayerNamesLengthFrm()
        {
            InitializeComponent();
            _btnRenameBackColorOrig = btnRename.BackColor;
        }

        private void txtLayerNameLength_TextChanged(object sender, EventArgs e)
        {
            //int len;
            //if (!GetValidTextLength(out len)) txtLayerNameLength.Text = txtLayerNameLength.Text = "10";
        }

        private bool GetValidTextLength(out int len)
        {
            len = 10;
            if (!int.TryParse(txtLayerNameLength.Text, out len)) return false;
            if (len > 255) return false;
            return true;
        }

        private void btnDifferingInList_Click(object sender, EventArgs e)
        {
            try
            {
                BuildDifferingList();
            }
            catch (Exception exception)
            {
                AcApp.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler aufgetreten! {0}", exception.Message));
            }
        }

        private void BuildDifferingList()
        {
            int len;
            if (!GetValidTextLength(out len)) return;

            lstDifferingLayerNames.Items.Clear();

            var allLayerNames = new List<string>();
            Globs.GetAllLayerNames(allLayerNames);
            var differingLayers = allLayerNames.Where(x => x != "0" && x.Length != len).OrderBy(x => x);
            foreach (var layerName in differingLayers)
            {
                lstDifferingLayerNames.Items.Add(layerName);
            }
        }

        private void lstDifferingLayerNames_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                txtLayerName.Text = lstDifferingLayerNames.SelectedItem.ToString();
            }
            catch (Exception exception)
            {
                AcApp.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler aufgetreten! {0}", exception.Message));
            }

        }

        private void btnRename_Click(object sender, EventArgs e)
        {
            try
            {
                if (lstDifferingLayerNames.SelectedIndex < 0) return;

                var txt = txtLayerName.Text;
                if (string.IsNullOrEmpty(txt)) return;
                if (txt.Length > 255) return;


                var txtOld = lstDifferingLayerNames.SelectedItem.ToString();
                if (txt.Equals(txtOld)) return;

                var doc = AcApp.DocumentManager.MdiActiveDocument;
                using (doc.LockDocument())
                {
                    Globs.RenameLayers(doc.Database, txtOld, txt);
                    _layerRenamed = true;
                }
                BuildDifferingList();

            }
            catch (Exception exception)
            {
                AcApp.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler aufgetreten! {0}", exception.Message));
            }

        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = _layerRenamed ? DialogResult.Yes : DialogResult.No;
            Close();
        }

        private void txtLayerName_TextChanged(object sender, EventArgs e)
        {
            btnRename.Enabled = false;
            int len;
            if (!GetValidTextLength(out len)) return;
            btnRename.BackColor =
                (len == txtLayerName.Text.Length) ? System.Drawing.Color.LightGreen : _btnRenameBackColorOrig;
            btnRename.Enabled = !Globs.LayerExists(txtLayerName.Text);
        }
    }
}
