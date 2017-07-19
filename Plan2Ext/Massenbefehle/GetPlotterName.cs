using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Plan2Ext.Massenbefehle
{
    public partial class GetPlotterName : Form
    {
        private List<string> _PlotterNames;

        public GetPlotterName(List<string> list)
        {
            InitializeComponent();

            this._PlotterNames = list;
            InitPlotterNameList();
        }

        private void InitPlotterNameList()
        {
            lstPlotterNames.Items.Clear();
            if (_PlotterNames.Count == 0) return;
            foreach (var pn in _PlotterNames)
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
                return _PlotterNames[lstPlotterNames.SelectedIndex];
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
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }
    }
}
