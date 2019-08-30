using System;
using System.Collections.Generic;
using System.Windows.Forms;
// ReSharper disable IdentifierTypo

namespace Plan2Ext.Massenbefehle
{
    public partial class GetPlotterName : Form
    {
        private readonly List<string> _plotterNames;

        public GetPlotterName(List<string> list)
        {
            InitializeComponent();

            _plotterNames = list;
            InitPlotterNameList();
        }

        private void InitPlotterNameList()
        {
            lstPlotterNames.Items.Clear();
            if (_plotterNames.Count == 0) return;
            foreach (var pn in _plotterNames)
            {
                lstPlotterNames.Items.Add(pn);
            }
            lstPlotterNames.SelectedIndex = 0;
        }

        public string CurrentPlotterName
        {
            get
            {
                if (lstPlotterNames.Items.Count == 0) return string.Empty;
                return _plotterNames[lstPlotterNames.SelectedIndex];
            }
        }
        public bool NoPlotterInModelspace
        {
            get
            {
                return chkModelToNone.Checked;
            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
