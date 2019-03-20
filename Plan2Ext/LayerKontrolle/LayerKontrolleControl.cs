using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using AcApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;
// ReSharper disable IdentifierTypo

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
                lstEntityTypes.Items.Clear();
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
            var entityTypesDictionary = new Dictionary<string, int>();
            Palette.GetEntityTypesForLayer(lstAllLayers.SelectedItem.ToString(), entityTypesDictionary);
            foreach (var kvp in entityTypesDictionary)
            {
                lstEntityTypes.Items.Add(kvp.Key + " (" + kvp.Value + ")");
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
            SetLayers();
        }

        private void btnAllLayerOn_Click(object sender, EventArgs e)
        {
            Palette.AllLayersOn();
        }
    }
}
