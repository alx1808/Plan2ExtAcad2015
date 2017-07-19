using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plan2Ext.CenterBlock
{
    public class Options
    {
        public CenterBlockControl Form { get; set; }

        public Options()
        {
            try
            {
                this._Blockname = TheConfiguration.GetValueString("alx_V:ino_rbName");
            }
            catch (Exception)
            {
                this._Blockname = "RAUMSTEMPEL_50";
            }

            try
            {
                this._LayerName = TheConfiguration.GetValueString("alx_V:ino_fglayer");
            }
            catch (Exception)
            {
                this._LayerName = "A_RA_NGFL_P";
            }
        }

        private string _Blockname = "";
        public string Blockname {
            get { return _Blockname; }
            set { _Blockname = value; }
        }
        public void SetBlockname(string blockName)
        {
            Form.txtBlockname.Text = blockName;
            _Blockname = blockName;
        }

        private string _LayerName = "";
        public string LayerName {
            get { return _LayerName;}
            set { _LayerName = value; }
        }

        public bool UseXRefs { get; set; }


    }
}
