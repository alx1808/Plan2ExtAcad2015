using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plan2Ext.Raumnummern
{
    public class RnOptions
    {
        public RnOptions()
        {
            try
            {
                this.Blockname  = TheConfiguration.GetValueString("alx_V:ino_rbName");
            }
            catch (Exception)
            {
                this.Blockname = "?";
            }
            try
            {
                this.Attribname = TheConfiguration.GetValueString("alx_V:ino_rb_nummer_bez");
            }
            catch (Exception)
            {
                this.Attribname = "?";
            }
            try
            {
                this.FlaechenAttributName = TheConfiguration.GetValueString("alx_V:ino_flattrib");
            }
            catch (Exception)
            {
                this.FlaechenAttributName = "?";
            }
            try
            {
                this.UmfangAttributName = TheConfiguration.GetValueString("alx_V:ino_PeriAttrib");
            }
            catch (Exception)
            {
                this.UmfangAttributName = "?";
            }
            try
            {
                this.FlaechenGrenzeLayerName = TheConfiguration.GetValueString("alx_V:ino_fglayer");
            }
            catch (Exception)
            {
                this.FlaechenGrenzeLayerName = "?";
            }
            try
            {
                this.AbzFlaechenGrenzeLayerName = TheConfiguration.GetValueString("alx_V:ino_aflayer");
            }
            catch (Exception)
            {
                this.AbzFlaechenGrenzeLayerName = "?";
            }
            try
            {
                this.HBlockname = TheConfiguration.GetValueString("alx_V:ino_rb_HkBlockName");
            }
            catch (Exception)
            {
                this.Attribname = "?";
            }
        }

        private string _Top = "01";
        public string Top { get { return _Top; } set { _Top = value; } }
        public void SetTop(string topName)
        {
            Form.txtTop.Text = topName;
            _Top = topName;
        }

        private string _Separator = "/";
        public string Separator { get { return _Separator; } set { _Separator = value; } }
        private string _Number = "01";
        public string Number { get { return _Number; } set { _Number = value; } }
        public void SetNumber(string num)
        {
            Form.txtNumber.Text = num;
        }

        private string _TopNr = "1";
        public string TopNr { get { return _TopNr; } set { _TopNr = value; } }
        public void SetTopNr(string topNr)
        {
            Form.txtTopNr.Text = topNr;
            _TopNr = topNr;
        }

        private bool _AutoCorr = true;
        public bool AutoCorr { get { return _AutoCorr; } set { _AutoCorr = value; } }

        private string _Blockname = "";
        public string Blockname { get { return _Blockname; } set { _Blockname = value; } }
        public void SetBlockname(string blockName)
        {
            Form.txtBlockname.Text = blockName;
            _Blockname = blockName;
        }

        private string _FlaechenGrenzeLayerName = "";
        public string FlaechenGrenzeLayerName { get { return _FlaechenGrenzeLayerName; } set { _FlaechenGrenzeLayerName = value; } }
        public void SetFlaechenGrenzeLayerName(string FlaechenGrenzeLayerName)
        {
            Form.txtFlaechenGrenzeLayerName.Text = FlaechenGrenzeLayerName;
            _FlaechenGrenzeLayerName = FlaechenGrenzeLayerName;
        }

        private string _AbzFlaechenGrenzeLayerName = "";
        public string AbzFlaechenGrenzeLayerName { get { return _AbzFlaechenGrenzeLayerName; } set { _AbzFlaechenGrenzeLayerName = value; } }
        public void SetAbzFlaechenGrenzeLayerName(string AbzFlaechenGrenzeLayerName)
        {
            Form.txtAbzFlaechenGrenzeLayerName.Text = AbzFlaechenGrenzeLayerName;
            _AbzFlaechenGrenzeLayerName = AbzFlaechenGrenzeLayerName;
        }

        private string _FlaechenAttributName = "";
        public string FlaechenAttributName { get { return _FlaechenAttributName; } set { _FlaechenAttributName = value; } }
        public void SetFlaechenAttributName(string FlaechenAttributName)
        {
            Form.txtFlaechenAttributName.Text = FlaechenAttributName;
            _FlaechenAttributName = FlaechenAttributName;
        }

        private string _UmfangAttributName = "";
        public string UmfangAttributName { get { return _UmfangAttributName; } set { _UmfangAttributName = value; } }
        public void SetUmfangAttributName(string UmfangAttributName)
        {
            //Form.txtUmfangAttributName.Text = UmfangAttributName;
            _UmfangAttributName = UmfangAttributName;
        }

        private string _HBlockname = "";
        public string HBlockname { get { return _HBlockname; } set { _HBlockname = value; } }
        public void SetHBlockname(string HBlockName)
        {
            Form.txtHBlockname.Text = HBlockName;
            _HBlockname = HBlockName;
        }

        private string _Attribname = "";
        public string Attribname { get { return _Attribname; } set { _Attribname = value; } }
        public void SetAttribname(string attribName)
        {
            Form.txtAttName.Text = attribName;
            _Attribname = attribName;
        }


        public RnControl Form { get; set; }

        internal void ResetNr()
        {
            if (_Number.Length == 0) _Number = "01";
            else
            {
                string num = "1";
                _Number = num.PadLeft(_Number.Length, '0');
            }
        }
    }
}
