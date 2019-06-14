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
        string ObjectPolygonLayer { get; }
        string InnenAttName { get; }
        string AussenAttName { get; }
        string NrAttName { get; }
        string RaumBlockName { get; }
        string FlaGrenzLayer { get; }
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
        private const string FLA_GRENZ_LAYER_ATT_NAME_VARIABLE = "alx_V:ino_fglayer";
        private const string RAUMBLOCK_NAME_ATT_NAME_VARIABLE = "alx_V:ino_rbName";
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

        public string ObjectPolygonLayer { get; private set; }
        public string InnenAttName { get; private set; }
        public string AussenAttName { get; private set; }
        public string NrAttName { get; private set; }
        public string FlaGrenzLayer { get; private set; }
        public string RaumBlockName { get; private set; }
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
            GetFlaGrenzLayerFromConfig();
            GetRaumBlockNameFromConfig();
        }

        private void GetRaumBlockNameFromConfig()
        {
            string value;
            if (GetFromConfig(out value, RAUMBLOCK_NAME_ATT_NAME_VARIABLE))
            {
                RaumBlockName = value;
            }
            else
            {
                Log.Warn(string.Format(CultureInfo.CurrentCulture, "Variable {0} ist nicht konfiguriert!",
                    RAUMBLOCK_NAME_ATT_NAME_VARIABLE));
                RaumBlockName = "HB_RA_RAUMBLOCK";
            }
        }

        private void GetFlaGrenzLayerFromConfig()
        {
            string value;
            if (GetFromConfig(out value, FLA_GRENZ_LAYER_ATT_NAME_VARIABLE))
            {
                FlaGrenzLayer = value;
            }
            else
            {
                Log.Warn(string.Format(CultureInfo.CurrentCulture, "Variable {0} ist nicht konfiguriert!",
                    FLA_GRENZ_LAYER_ATT_NAME_VARIABLE));
                FlaGrenzLayer = "B_HB_RA_NGFL_G";
            }
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
                InnenAttName = value;
            }
            else
            {
                Log.Warn(string.Format(CultureInfo.CurrentCulture, "Variable {0} ist nicht konfiguriert!",
                    FEN_INNEN_ATT_NAME_VARIABLE));
                InnenAttName = "ZU_RAUM_INNEN";
            }
        }
        private void GetFenAussenAttFromConfig()
        {
            string value;
            if (GetFromConfig(out value, FEN_AUSSEN_ATT_NAME_VARIABLE))
            {
                AussenAttName = value;
            }
            else
            {
                Log.Warn(string.Format(CultureInfo.CurrentCulture, "Variable {0} ist nicht konfiguriert!",
                    FEN_AUSSEN_ATT_NAME_VARIABLE));
                AussenAttName = "ZU_RAUM_AUSSEN";
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
