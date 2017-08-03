using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plan2Ext.Nummerierung
{
    public class NrOptions
{
        public NrOptions()
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
        }

        private string _Top = "";
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

        private bool _AutoCorr = false;
        public bool AutoCorr { get { return _AutoCorr; } set { _AutoCorr = value; } }

        private bool _UseFirstAttrib = false;
        public bool UseFirstAttrib { get { return _UseFirstAttrib; } set { _UseFirstAttrib = value; } }

        private string _Blockname = "";
        public string Blockname { get { return _Blockname; } set { _Blockname = value; } }
        public void SetBlockname(string blockName)
        {
            Form.txtBlockname.Text = blockName;
            _Blockname = blockName;
        }   

        private string _Attribname = "";
        public string Attribname { get { return _Attribname; } set { _Attribname = value; } }
        public void SetAttribname(string attribName)
        {
            Form.txtAttName.Text = attribName;
            _Attribname = attribName;
        }


        public NrControl Form { get; set; }

        internal void ResetNr()
        {
            if (_Number.Length == 0) _Number = "01";
            else
            {
                string num = "1";
                _Number = num.PadLeft(_Number.Length, '0');
            }
        }
    }}
