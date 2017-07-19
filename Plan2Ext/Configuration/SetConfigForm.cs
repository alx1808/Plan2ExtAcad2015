using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Plan2Ext.Configuration
{
    public partial class SetConfigForm : Form
    {

        private List<string> _ExistingConfigs;
        private string _Current;

        

        public SetConfigForm(string current, List<string> configs)
        {
            InitializeComponent();

            _ExistingConfigs = configs.Select(x => x.ToUpperInvariant()).ToList();

            _Current = current.ToUpperInvariant();

        }

        private void SetConfigForm_Load(object sender, EventArgs e)
        {
            SetButtons();


        }

        private void SetButtons()
        {

            foreach (var c in _ExistingConfigs)
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
                    default:
                        break;
                }
            }

            switch (_Current )
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
                default:
                    break;
            }
        }

        private void SetCurrent()
        {
            if (rbnBig.Checked)
            {
                _Current = "BIG.CFG";
            }
            else if (rbnPlan2.Checked)
            {
                _Current = "PLAN2.CFG";
            }
            else if (rbnFM.Checked)
            {
                _Current = "FM.CFG";
            }
            else if (rbnPlFm.Checked)
            {
                _Current = "PLAN2 - FM.CFG";
            }
            else if (rbnNorm.Checked)
            {
                _Current = "NORM.CFG";
            }
            else if (rbnSalk.Checked)
            {
                _Current = "SALK.CFG";
            }

        }


        public string Configuration
        {
            get
            {
                return _Current;
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
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            Ok();
        }

        private void Ok()
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }


    }
}
