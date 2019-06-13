using System;
using System.Collections.Generic;
using System.Globalization;
using log4net;
// ReSharper disable IdentifierTypo
// ReSharper disable StringLiteralTypo

namespace Plan2Ext.AutoIdVergabeOeff
{
    internal interface IConfigurationHandler
    {
        List<string> ConfiguredFensterBlockNames { get; }
        List<string> ConfiguredTuerBlockNames { get; }
        string ObjectPolygonLayer { get; set; }
        string FenInnenAttName { get; set; }
        string FenAussenAttName { get; set; }
        string NrAttName { get; set; }
    }

    internal class ConfigurationHandler : IConfigurationHandler
    {
        #region log4net Initialization
        private static readonly ILog Log = LogManager.GetLogger(Convert.ToString((typeof(GenerateOeffBoundaries.ConfigurationHandler))));
        #endregion
        private readonly List<string> _configuredFensterBlockNames = new List<string>();
        private readonly List<string> _configuredFensterBlockNamesVariables = new List<string>()
        {
            "alx_V:ino_fenster_Block_Oben",
            "alx_V:ino_fenster_Block_Unten",
            "alx_V:ino_fenster_Block_Links",
            "alx_V:ino_fenster_Block_Rechts",
        };

        private readonly List<string> _configuredTuerBlockNames = new List<string>();
        private readonly List<string> _configuredTuerBlockNamesVariables = new List<string>()
        {
            "alx_V:ino_tuerBlock_Oben",
            "alx_V:ino_tuerBlock_Unten",
            "alx_V:ino_tuerBlock_Links",
            "alx_V:ino_tuerBlock_Rechts",
        };
        // todo:
        private const string OBJECT_POLYGON_LAYER_VARIABLE = "alx_V:ino_autoidoeff_ObjectPolygonLayer";
        // todo:
        private const string NUMBER_ATT_NAME_VARIABLE = "alx_V:ino_autoidoeff_AttNr";
        private const string FEN_INNEN_ATT_NAME_VARIABLE = "alx_V:ino_zrids_AttInnen";
        private const string FEN_AUSSEN_ATT_NAME_VARIABLE = "alx_V:ino_zrids_AttAussen";
        public ConfigurationHandler()
        {
            ReadConfiguration();
        }

        public List<string> ConfiguredFensterBlockNames
        {
            get { return _configuredFensterBlockNames; }
        }

        public List<string> ConfiguredTuerBlockNames
        {
            get { return _configuredTuerBlockNames; }
        }

        public string ObjectPolygonLayer { get; set; }
        public string FenInnenAttName { get; set; }
        public string FenAussenAttName { get; set; }
        public string NrAttName { get; set; }

        private void ReadConfiguration()
        {
            foreach (var blockNamesVariable in _configuredFensterBlockNamesVariables)
            {
                string val;
                if (GetFromConfig(out val, blockNamesVariable))
                {
                    var valUc = val.ToUpperInvariant();
                    if (!ConfiguredFensterBlockNames.Contains(valUc)) ConfiguredFensterBlockNames.Add(valUc);
                }
                else Log.Warn(string.Format(CultureInfo.CurrentCulture, "Variable {0} ist nicht konfiguriert!", blockNamesVariable));
            }
            foreach (var blockNamesVariable in _configuredTuerBlockNamesVariables)
            {
                string val;
                if (GetFromConfig(out val, blockNamesVariable))
                {
                    var valUc = val.ToUpperInvariant();
                    if (!ConfiguredTuerBlockNames.Contains(valUc)) ConfiguredTuerBlockNames.Add(valUc);
                }
                else Log.Warn(string.Format(CultureInfo.CurrentCulture, "Variable {0} ist nicht konfiguriert!", blockNamesVariable));
            }

            GetObjectPolygonLayerFromConfig();
            GetFenInnenAttFromConfig();
            GetFenAussenAttFromConfig();
            GetNrAttFromConfig();
        }

        private void GetObjectPolygonLayerFromConfig()
        {
            string value;
            if (GetFromConfig(out value, OBJECT_POLYGON_LAYER_VARIABLE))
            {
                ObjectPolygonLayer = value;
            }
            else
            {
                Log.Warn(string.Format(CultureInfo.CurrentCulture, "Variable {0} ist nicht konfiguriert!",
                    OBJECT_POLYGON_LAYER_VARIABLE));
                ObjectPolygonLayer = "B_HB_RA_IDGE_G";
            }
        }

        private void GetNrAttFromConfig()
        {
            string value;
            if (GetFromConfig(out value, NUMBER_ATT_NAME_VARIABLE))
            {
                NrAttName = value;
            }
            else
            {
                Log.Warn(string.Format(CultureInfo.CurrentCulture, "Variable {0} ist nicht konfiguriert!",
                    NUMBER_ATT_NAME_VARIABLE));
                NrAttName = "FENSTER_ID";
            }
        }
        private void GetFenInnenAttFromConfig()
        {
            string value;
            if (GetFromConfig(out value, FEN_INNEN_ATT_NAME_VARIABLE))
            {
                FenInnenAttName = value;
            }
            else
            {
                Log.Warn(string.Format(CultureInfo.CurrentCulture, "Variable {0} ist nicht konfiguriert!",
                    FEN_INNEN_ATT_NAME_VARIABLE));
                FenInnenAttName = "ZU_RAUM_INNEN";
            }
        }
        private void GetFenAussenAttFromConfig()
        {
            string value;
            if (GetFromConfig(out value, FEN_AUSSEN_ATT_NAME_VARIABLE))
            {
                FenAussenAttName = value;
            }
            else
            {
                Log.Warn(string.Format(CultureInfo.CurrentCulture, "Variable {0} ist nicht konfiguriert!",
                    FEN_AUSSEN_ATT_NAME_VARIABLE));
                FenAussenAttName = "ZU_RAUM_AUSSEN";
            }
        }

        private static bool GetFromConfig(out string val, string varName)
        {
            val = null;
            try
            {
                val = TheConfiguration.GetValueString(varName);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

    }
}
