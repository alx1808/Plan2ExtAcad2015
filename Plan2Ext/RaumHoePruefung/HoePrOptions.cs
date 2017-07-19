using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plan2Ext.RaumHoePruefung
{
    public class HoePrOptions
    {
        public HoePrControl Form { get; set; }

        public HoePrOptions()
        {
            try
            {
                this.RaumBlockname = TheConfiguration.GetValueString("alx_V:ino_rbName");
            }
            catch (Exception)
            {
                this.RaumBlockname = "RAUMSTEMPEL_50";
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

        private string _RaumBlockname = "RAUMSTEMPEL_50";
        public string RaumBlockname
        {
            get { return _RaumBlockname; }
            set
            {
                _RaumBlockname = value;
            }
        }
        public void SetHKBlockname(string blockName)
        {
            Form.txtBlockname.Text = blockName;
            _RaumBlockname = blockName;
        }

        private string _AttHoehe = "RH.";
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

        private int _RhToleranz = 3;
        public int RhToleranz
        {
            get { return _RhToleranz; }
            set
            {
                if (_RhToleranz != value && (value > 0))
                {
                    _RhToleranz = value;
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
