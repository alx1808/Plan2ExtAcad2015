using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
// ReSharper disable IdentifierTypo

namespace Plan2Ext.Configuration
{
    public partial class SetConfigForm : Form
    {

        private readonly List<string> _existingConfigs;
        private string _current;

        

        public SetConfigForm(string current, List<string> configs)
        {
            InitializeComponent();

            _existingConfigs = configs.Select(x => x.ToUpperInvariant()).ToList();

            _current = current.ToUpperInvariant();

        }

        private void SetConfigForm_Load(object sender, EventArgs e)
        {
            SetButtons();


        }

        private void SetButtons()
        {

            foreach (var c in _existingConfigs)
            {
                switch (c)
                {
                    case "PLAN2.CFG":
                        rbnPlan2.Enabled = true;
                        btnOk.Enabled = true;
                        break;
                    case "FM.CFG":
                        rbnFM.Enabled = true;
                        btnOk.Enabled = true;
                        break;
                    case "PLAN2 - FM.CFG":
                        rbnPlFm.Enabled = true;
                        btnOk.Enabled = true;
                        break;
                    case "BIG.CFG":
                        rbnBig.Enabled = true;
                        btnOk.Enabled = true;
                        break;
                    case "NORM.CFG":
                        rbnNorm.Enabled = true;
                        btnOk.Enabled = true;
                        break;
                    case "SALK.CFG":
                        rbnSalk.Enabled = true;
                        btnOk.Enabled = true;
                        break;
                    case "KAV.CFG":
                        rbnKav.Enabled = true;
                        btnOk.Enabled = true;
                        break;
                }
            }

            switch (_current )
            {
                case "PLAN2.CFG":
                    rbnPlan2.Checked = true;
                    break;
                case "FM.CFG":
                    rbnFM.Checked = true;
                    break;
                case "PLAN2 - FM.CFG":
                    rbnPlFm.Checked = true;
                    break;
                case "BIG.CFG":
                    rbnBig.Checked = true;
                    break;
                case "NORM.CFG":
                    rbnNorm.Checked = true;
                    break;
                case "SALK.CFG":
                    rbnSalk.Checked = true;
                    break;
                case "KAV.CFG":
                    rbnKav.Checked = true;
                    break;
            }
        }

        private void SetCurrent()
        {
            if (rbnBig.Checked)
            {
                _current = "BIG.CFG";
            }
            else if (rbnPlan2.Checked)
            {
                _current = "PLAN2.CFG";
            }
            else if (rbnFM.Checked)
            {
                _current = "FM.CFG";
            }
            else if (rbnPlFm.Checked)
            {
                _current = "PLAN2 - FM.CFG";
            }
            else if (rbnNorm.Checked)
            {
                _current = "NORM.CFG";
            }
            else if (rbnSalk.Checked)
            {
                _current = "SALK.CFG";
            }
            else if (rbnKav.Checked)
            {
                _current = "KAV.CFG";
            }

        }


        public string Configuration
        {
            get
            {
                return _current;
            }
        }

        private void rbnPlan2_CheckedChanged(object sender, EventArgs e)
        {
            SetCurrent();
        }

        private void rbnFM_CheckedChanged(object sender, EventArgs e)
        {
            SetCurrent();

        }

        private void rbnPlFm_CheckedChanged(object sender, EventArgs e)
        {
            SetCurrent();

        }

        private void rbnBig_CheckedChanged(object sender, EventArgs e)
        {
            SetCurrent();

        }

        private void rbnNorm_CheckedChanged(object sender, EventArgs e)
        {
            SetCurrent();

        }

        private void rbnSalk_CheckedChanged(object sender, EventArgs e)
        {
            SetCurrent();

        }

        private void rbnKav_CheckedChanged(object sender, EventArgs e)
        {
            SetCurrent();

        }

        private void SetConfigForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                Abbruch();
            }
            else if (e.KeyCode == Keys.Return)
            {
                Ok();
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Abbruch();
        }

        private void Abbruch()
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            Ok();
        }

        private void Ok()
        {
            DialogResult = DialogResult.OK;
            Close();
        }



    }
}
