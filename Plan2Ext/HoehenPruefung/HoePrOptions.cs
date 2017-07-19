using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plan2Ext.HoehenPruefung
{
    public class HoePrOptions
    {
        public HoePrControl Form { get; set; }

        public HoePrOptions()
        {
            try
            {
                this.HKBlockname = TheConfiguration.GetValueString("alx_V:ino_rb_HkBlockName");
            }
            catch (Exception)
            {
                this.HKBlockname = "HÖHENKOTE";
            }

            try
            {
                this.PolygonLayer = TheConfiguration.GetValueString("alx_V:ino_fglayer");
            }
            catch (Exception)
            {
                this.PolygonLayer = "A_RA_NGFL_P";
            }
        }

        private string _HKBlockname = "HÖHENKOTE";
        public string HKBlockname
        {
            get { return _HKBlockname; }
            set
            {
                _HKBlockname = value;
            }
        }
        public void SetHKBlockname(string blockName)
        {
            Form.txtBlockname.Text = blockName;
            _HKBlockname = blockName;
        }

        private string _AttHoehe = "HOEHE";
        public string AttHoehe
        {
            get { return _AttHoehe; }
            set
            {
                _AttHoehe = value;
            }
        }
        public void SetHoehenAtt(string attName)
        {
            Form.txtHoehenAtt.Text = attName;
            _AttHoehe = attName;
        }

        private int _FbToleranz = 3;
        public int FbToleranz
        {
            get { return _FbToleranz; }
            set
            {
                if (_FbToleranz != value && (value > 0))
                {
                    _FbToleranz = value;
                }
            }
        }

        private string _PolygonLayer = string.Empty;
        public string PolygonLayer
        {
            get { return _PolygonLayer; }
            set
            {
                _PolygonLayer = value;
            }
        }
        public void SetPolygonLayer(string layer)
        {
            Form.txtPolygonLayer.Text = layer;
            _PolygonLayer = layer;
        }

    }
}
