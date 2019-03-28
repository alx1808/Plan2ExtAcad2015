using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using AcApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;
// ReSharper disable IdentifierTypo
// ReSharper disable StringLiteralTypo

namespace Plan2Ext.LayerKontrolle
{
    public partial class LayerKontrolleControl : UserControl
    {
        public LayerKontrolleControl()
        {
            InitializeComponent();
        }

        public string CurrentLayerName
        {
            get { return lstAllLayers.SelectedIndex > 0 ? lstAllLayers.SelectedItem.ToString() : "0"; }
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            var index = lstAllLayers.SelectedIndex;
            index++;
            if (index >= lstAllLayers.Items.Count) index = 0;
            lstAllLayers.SelectedIndex = index;
        }

        private void btnPrevious_Click(object sender, EventArgs e)
        {
            var index = lstAllLayers.SelectedIndex;
            index--;
            if (index < 0) index = lstAllLayers.Items.Count - 1;
            lstAllLayers.SelectedIndex = index;
        }

        private bool _ignoreIndexChangedReaction;
        private void SetLayers()
        {
            if (_ignoreIndexChangedReaction) return;
            try
            {
                Globs.CancelCommand();

                var doc = AcApp.DocumentManager.MdiActiveDocument;
                doc.SendStringToExecute("Plan2LayerKontrolleSetLayers ", true, false, false);
            }
            catch (Exception ex)
            {
                AcApp.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler aufgetreten! {0}", ex.Message));
            }
        }

        public void InitLayers(bool ignoreSetLayers)
        {
            try
            {
                _ignoreIndexChangedReaction = ignoreSetLayers;
                lstAllLayers.Items.Clear();
                lstEntityTypes.Items.Clear();
                lstAlwaysOn.Items.Clear(); 
                lblColorPropertyMode.Text = "";
                lblLineTypePropertyMode.Text = "";
                lblLineWeightPropertyMode.Text = "";
                var layerNames = new List<string>();
                Globs.GetAllLayerNames(layerNames);
                layerNames = layerNames.OrderBy(x => x).ToList();
                if (layerNames.Count == 0) return;
                layerNames.ForEach(x => lstAllLayers.Items.Add(x));
                lstAllLayers.SelectedIndex = 0;
            }
            finally
            {
                _ignoreIndexChangedReaction = false;
            }
        }

        public void ClearLists()
        {
            try
            {
                _ignoreIndexChangedReaction = true;
                lstAllLayers.Items.Clear();
                lstAlwaysOn.Items.Clear();
                lstEntityTypes.Items.Clear();
                lblColorPropertyMode.Text = "";
                lblLineTypePropertyMode.Text = "";
                lblLineWeightPropertyMode.Text = "";
            }
            finally
            {
                _ignoreIndexChangedReaction = false;
            }
        }

        private void lstAllLayers_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetEntityTypesList();
            SetLayers();
        }

        private void SetEntityTypesList()
        {
            if (_ignoreIndexChangedReaction) return;
            lstEntityTypes.Items.Clear();
            if (lstAllLayers.SelectedItem == null) return;
            var entityTypesDictionary = new Dictionary<Type, int>();
            Palette.EntityPropertyMode colorPropertyMode;
            Palette.EntityPropertyMode lineTypePropertyMode;
            Palette.EntityPropertyMode lineWeightPropertyMode;
            Palette.GetEntityTypesForLayer(lstAllLayers.SelectedItem.ToString(), entityTypesDictionary, out colorPropertyMode, out lineTypePropertyMode, out lineWeightPropertyMode);
            foreach (var kvp in entityTypesDictionary)
            {
                lstEntityTypes.Items.Add(kvp.Key.GetGermanName() + " (" + kvp.Value + ")");
            }

            // ReSharper disable once LocalizableElement
            lblColorPropertyMode.Text = "Farbe: " + ToGerman(colorPropertyMode);
            lblColorPropertyMode.ForeColor = colorPropertyMode == Palette.EntityPropertyMode.Variabel ? System.Drawing.Color.Red : System.Drawing.SystemColors.ControlText;
            lblColorPropertyMode.Font = colorPropertyMode == Palette.EntityPropertyMode.Variabel
                ? new Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold)
                : new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular);
            // ReSharper disable once LocalizableElement
            lblLineTypePropertyMode.Text = "Linientyp: " + ToGerman(lineTypePropertyMode);
            lblLineTypePropertyMode.ForeColor = lineTypePropertyMode == Palette.EntityPropertyMode.Variabel ? System.Drawing.Color.Red : System.Drawing.SystemColors.ControlText;
            lblLineTypePropertyMode.Font = lineTypePropertyMode == Palette.EntityPropertyMode.Variabel
                ? new Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold)
                : new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular);
            //    ? new Font(lblLineTypePropertyMode.Font.Name, lblLineTypePropertyMode.Font.Size, FontStyle.Bold)
            //    : new Font(lblLineTypePropertyMode.Font.Name, lblLineTypePropertyMode.Font.Size, FontStyle.Regular);
            // ReSharper disable once LocalizableElement
            lblLineWeightPropertyMode.Text = "Linienstärke: " + ToGerman(lineWeightPropertyMode);
            lblLineWeightPropertyMode.ForeColor = lineWeightPropertyMode == Palette.EntityPropertyMode.Variabel ? System.Drawing.Color.Red : System.Drawing.SystemColors.ControlText;
            lblLineWeightPropertyMode.Font = lineWeightPropertyMode == Palette.EntityPropertyMode.Variabel
                ? new Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold)
                : new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular);
            //    ? new Font(lblLineWeightPropertyMode.Font.Name, lblLineWeightPropertyMode.Font.Size, FontStyle.Bold)
            //    : new Font(lblLineWeightPropertyMode.Font.Name, lblLineWeightPropertyMode.Font.Size, FontStyle.Regular);

        }

        private string ToGerman(Palette.EntityPropertyMode colorPropertyMode)
        {
            switch (colorPropertyMode)
            {
                case Palette.EntityPropertyMode.ByLayer:
                    return "VonLayer";
                case Palette.EntityPropertyMode.Variabel:
                    return "Variabel";
                default:
                    throw new ArgumentOutOfRangeException("colorPropertyMode", colorPropertyMode, null);
            }
        }

        private bool _getAlwaysOnShield;
        private void btnGetAlwaysOn_Click(object sender, EventArgs e)
        {
            if (_getAlwaysOnShield) return;
            try
            {
                _getAlwaysOnShield = true;

                Globs.CancelCommand();

                AcApp.DocumentManager.MdiActiveDocument.SendStringToExecute("Plan2LayerKontrolleSelectAlwaysOnLayer ", true, false, false);
            }
            catch (Exception ex)
            {
                AcApp.ShowAlertDialog(ex.Message);
            }
            finally
            {
                _getAlwaysOnShield = false;
            }
        }

        public bool IsAlwaysOn(string layerName)
        {
            foreach (var item in lstAlwaysOn.Items)
            {
                if (item.ToString() == layerName) return true;
            }

            return false;
        }

        public void AddAlwaysOnLayer(string layerName)
        {
            foreach (var item in lstAlwaysOn.Items)
            {
                if (item.ToString() == layerName) return;
            }

            lstAlwaysOn.Items.Add(layerName);
        }

        private void lstAlwaysOn_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Delete) return;

            if (lstAlwaysOn.SelectedItem != null)
                lstAlwaysOn.Items.Remove(lstAlwaysOn.SelectedItem);
            //SetLayers();
        }

        private void btnAllLayerOn_Click(object sender, EventArgs e)
        {
            try
            {
                Globs.CancelCommand();

                var doc = AcApp.DocumentManager.MdiActiveDocument;
                doc.SendStringToExecute("Plan2LayerKontrolleAllLayersOn ", true, false, false);
            }
            catch (Exception ex)
            {
                AcApp.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler aufgetreten! {0}", ex.Message));
            }
        }

        private void btnCheckVonlayer_Click(object sender, EventArgs e)
        {
            try
            {
                Globs.CancelCommand();

                var doc = AcApp.DocumentManager.MdiActiveDocument;
                doc.SendStringToExecute("Plan2LayerKontrolleSelectAllVariableEntitiesInModelSpace ", true, false, false);
            }
            catch (Exception ex)
            {
                AcApp.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler aufgetreten! {0}", ex.Message));
            }
        }
    }
}
