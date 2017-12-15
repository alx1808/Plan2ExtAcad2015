//using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if BRX_APP
using _AcAp = Bricscad.ApplicationServices;
using Teigha.DatabaseServices;
#elif ARX_APP
  using _AcAp = Autodesk.AutoCAD.ApplicationServices;
  using Autodesk.AutoCAD.DatabaseServices;
#endif


namespace Plan2Ext.Fenster
{
    internal class FensterOptions
    {
        public enum FenArt
        {
            Standard,
            Kasten
        }

        private double _Breite = 1.0;
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


        private double _FluegelStaerke = 0.04;
        public double FluegelStaerke
        {
            get { return _FluegelStaerke; }
            set
            {
                if (value > 0.0) _FluegelStaerke = value;
            }
        }
        public string FluegelStaerkeString
        {
            get
            {
                return (_FluegelStaerke * 100.0).ToString(CultureInfo.InvariantCulture);
            }
            set
            {
                double val;
                if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out val))
                {
                    FluegelStaerke = (val / 100.0);
                }
            }
        }



        private double _TextAbstand = 0.15;
        public double TextAbstand
        {
            get { return _TextAbstand; }
            set
            {
                _TextAbstand = Math.Round(value * 100.0) / 100.0;
            }
        }
        public string TextAbstandString
        {
            get
            {
                return string.Format(CultureInfo.InvariantCulture, "{0:N0}", _TextAbstand * 100.0);
            }
            set
            {
                double val;
                if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out val))
                {
                    TextAbstand = val / 100.0;
                }
            }
        }

        private double _Hoehe = 1.0;
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
                    _AcAp.Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage(string.Format(CultureInfo.CurrentCulture, "\nUngültiger Wert '{0:N1}' für Höhe.\n", value));
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

        private double _SprossenBreite = 0.05;
        public double SprossenBreite
        {
            get { return _SprossenBreite; }
            set { if (value > 0.0) _SprossenBreite = value; }
        }
        public string SprossenBreiteString
        {
            get { return (_SprossenBreite * 100.0).ToString(CultureInfo.InvariantCulture); }
            set
            {
                double val;
                if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out val))
                {
                    _SprossenBreite = (val / 100.0);
                }
            }
        }


        private double _Parapet = 1.0;
        public double Parapet
        {
            get { return _Parapet; }
            set
            {
                if (value > 0.0)
                {
                    _Parapet = Math.Round(value * 100.0) / 100.0;
                }
                else
                {
                    _AcAp.Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage(string.Format(CultureInfo.CurrentCulture, "\nUngültiger Wert '{0:N1}' für Parapet.\n", value));
                }
            }
        }
        public string ParapetString
        {
            get
            {
                //return (_Parapet * 100.0).ToString(CultureInfo.InvariantCulture);
                return string.Format(CultureInfo.InvariantCulture, "{0:N0}", _Parapet * 100.0);
            }
            set
            {
                double val;
                if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out val))
                {
                    Parapet = (val / 100.0);
                }
            }
        }


        private double _OlAb = 1.5;
        public double OlAb
        {
            get { return _OlAb; }
            set { _OlAb = value; }
        }
        public string OlAbString
        {
            get
            {
                return (_OlAb * 100.0).ToString(CultureInfo.InvariantCulture);
            }
            set
            {
                double val;
                if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out val))
                {
                    OlAb = (val / 100.0);
                }
            }
        }

        private double _Staerke = 0.07;
        public double Staerke
        {
            get { return _Staerke; }
            set { if (value > 0.0) _Staerke = value; }
        }
        public string StaerkeString
        {
            get
            {
                return (_Staerke * 100.0).ToString(CultureInfo.InvariantCulture);
            }
            set
            {
                double val;
                if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out val))
                {
                    Staerke = (val / 100.0);
                }
            }
        }

        private double _WeitePruefTol = 0.01;
        public double WeitePruefTol
        {
            get { return _WeitePruefTol; }
            set { if (value > 0.0) _WeitePruefTol = value; }
        }
        public string WeitePruefTolString
        {
            get
            {
                return (_WeitePruefTol * 100.0).ToString(CultureInfo.InvariantCulture);
            }
            set
            {
                double val;
                if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out val))
                {
                    WeitePruefTol = (val / 100.0);
                }
            }
        }

        private double _Stock = 0.06;
        public double Stock
        {
            get { return _Stock; }
            set { if (value > 0.0) _Stock = value; }
        }
        public string StockString
        {
            get
            {
                return (_Stock * 100.0).ToString(CultureInfo.InvariantCulture);
            }
            set
            {
                double val;
                if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out val))
                {
                    Stock = (val / 100.0);
                }
            }
        }

        private int _Sprossen = 0;
        public int Sprossen
        {
            get { return _Sprossen; }
            set
            {
                //if (value == "Keine Sprosse" || value == "Eine Sprosse" || value == "Zwei Sprossen")
                if (value >= 0 && value < 3)
                    _Sprossen = value;
            }
        }

        private FenArt _FensterArt = FenArt.Standard;
        public FenArt FensterArt
        {
            get { return _FensterArt; }
            set
            {
                //if (value == "Standard" || value == "Kastenfenster" )
                _FensterArt = value;
            }
        }
        public string FensterArtString
        {
            get
            {
                return FenArtToString(_FensterArt);
            }
        }


        public ResultBuffer AsResultBuffer()
        {

            string faString = FenArtToString(_FensterArt);

            return new ResultBuffer(
                new TypedValue(5001, _Breite),
                new TypedValue(5001, _Hoehe),
                new TypedValue(5001, _Parapet),
                new TypedValue(5001, _OlAb),
                new TypedValue(5001, _Staerke),
                new TypedValue(5001, _Stock),
                new TypedValue(5001, _SprossenBreite),
                new TypedValue(5003, _Sprossen),
                new TypedValue(5001, _TextAbstand),
                new TypedValue(5001, _FluegelStaerke),
                new TypedValue(5005, faString)
                );


        }

        public static string FenArtToString(FenArt fenArt)
        {
            string faString = string.Empty;
            switch (fenArt)
            {
                case FenArt.Standard:
                    faString = "Standard";
                    break;
                case FenArt.Kasten:
                    faString = "Kastenfenster";
                    break;
            }
            return faString;
        }

        public static FenArt StringToFenArt(string faString)
        {
            FenArt fa = FenArt.Standard;
            if (string.Compare(faString, "Kastenfenster", StringComparison.OrdinalIgnoreCase) == 0)
            {
                fa = FenArt.Kasten;
            }
            return fa;
        }
    }
}
