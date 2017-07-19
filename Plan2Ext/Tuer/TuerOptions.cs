//using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if BRX_APP
using Bricscad.ApplicationServices;
using Teigha.DatabaseServices;
#elif ARX_APP
  using Autodesk.AutoCAD.ApplicationServices;
  using Autodesk.AutoCAD.DatabaseServices;
#endif

namespace Plan2Ext.Tuer
{
    internal class TuerOptions
    {

        public enum ZargenArt
        {
            Umfassung,
            Block,
            Eck
        }

        public enum TextBlockTp
        {
            Standard,
            Vier
        }


        private ZargenArt _TuerArt = ZargenArt.Umfassung;
        public ZargenArt TuerArt
        {
            get { return _TuerArt; }
            set
            {
                _TuerArt = value;
            }
        }

        private TextBlockTp _TextBlockTyp = TextBlockTp.Standard;
        public TextBlockTp TextBlockTyp
        {
            get { return _TextBlockTyp; }
            set
            {
                _TextBlockTyp = value;
            }
        }

        private double _Breite = 0.8;
        public double Breite
        {
            get { return _Breite; }
            set
            {
                if (value > 0.0)
                {
                    _Breite = Math.Round(value * 100.0) / 100.0; ;
                }
            }
        }
        public string BreiteString
        {
            get
            {
                return string.Format(CultureInfo.InvariantCulture, "{0:N0}", _Breite * 100.0);
            }
            set
            {
                double val;
                if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out val))
                {
                    
                    Breite = val / 100.0;
                }
            }
        }

        private double _Hoehe = 2.0;
        public double Hoehe
        {
            get { return _Hoehe; }
            set
            {
                if (value > 0.0)
                {
                    _Hoehe = Math.Round(value * 100.0) / 100.0;
                }
                else
                {
                    Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage(string.Format(CultureInfo.CurrentCulture, "\nUngültiger Wert '{0:N1}' für Höhe.\n", value));
                }

            }
        }
        public string HoeheString
        {
            get
            {
                return string.Format(CultureInfo.InvariantCulture, "{0:N0}", _Hoehe * 100.0);
            }
            set
            {
                double val;
                if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out val))
                {
                    Hoehe = val / 100.0;
                }
            }
        }

        private double _StockStaerke = 0.08;
        public double StockStaerke
        {
            get { return _StockStaerke; }
            set
            {
                if (value > 0.0)
                {
                    _StockStaerke = Math.Round(value * 100.0) / 100.0;
                }
                else
                {
                    Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage(string.Format(CultureInfo.CurrentCulture, "\nUngültiger Wert '{0:N1}' für Stockstärke.\n", value));
                }

            }
        }
        public string StockStaerkeString
        {
            get
            {
                return string.Format(CultureInfo.InvariantCulture, "{0:N0}", _StockStaerke * 100.0);
            }
            set
            {
                double val;
                if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out val))
                {
                    StockStaerke = val / 100.0;
                }
            }
        }
        

        private int _Fluegel = 0;
        public int Fluegel
        {
            get { return _Fluegel; }
            set
            {
                if (value >= 1 && value <= 2)
                    _Fluegel = value;
            }
        }

        public ResultBuffer AsResultBuffer()
        {

            return new ResultBuffer(
                new TypedValue(5001, _Breite),
                new TypedValue(5001, _Hoehe),
                new TypedValue(5003, _Fluegel),
                new TypedValue(5003, (int)_TuerArt),
                new TypedValue(5003, (int)_TextBlockTyp ),
                new TypedValue(5001, _StockStaerke )
               );


        }



        internal static ZargenArt IntToTuerArt(int ta)
        {
            switch (ta)
            {
                case 0:
                    return ZargenArt.Umfassung;
                case 1:
                    return ZargenArt.Block;
                case 2:
                    return ZargenArt.Eck;
                default:
                    return ZargenArt.Umfassung;
            }
        }

        internal static TextBlockTp IntToTextBlockTyp(int tbt)
        {
            switch (tbt)
            {
                case 0:
                    return TextBlockTp.Standard;
                case 1:
                    return TextBlockTp.Vier;
                default:
                    return TextBlockTp.Standard;
            }
        }
    }
}
