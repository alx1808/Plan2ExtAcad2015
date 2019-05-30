using System;
using System.Collections.Generic;
using System.Globalization;
using log4net;
// ReSharper disable IdentifierTypo
// ReSharper disable StringLiteralTypo

namespace Plan2Ext.GenerateOeffBoundaries
{
    internal interface IConfigurationHandler
    {
        List<string> ConfiguredFensterBlockNames { get; }
        List<string> ConfiguredTuerBlockNames { get; }
        string TuerSchraffLayer { get; }
        string FensterSchraffLayer { get; }
        string InternalPolylineLayers { get; }
    }

    internal class ConfigurationHandler : IConfigurationHandler
    {
        #region log4net Initialization
        private static readonly ILog Log = LogManager.GetLogger(Convert.ToString((typeof(ConfigurationHandler))));
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

        private const string TUER_HATCH_LAYER_VARIABLE = "alx_V:ino_tuer_HatchLayer";
        private const string FENSTER_HATCH_LAYER_VARIABLE = "alx_V:ino_fen_HatchLayer";
        private const string INTENAL_POLYLINE_LAYER_VARIABLE = "alx_V:ino_genOeffBoundaries_InnerPlineLayer";

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

        public string TuerSchraffLayer { get; private set; }
        public string FensterSchraffLayer { get; private set; }
        public string InternalPolylineLayers { get; private set; }

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

            GetTuerSchraffLayerFromConfig();

            GetFensterSchraffLayerFromConfig();

            GetInternalPolylineLayerFromConfig();
        }

        private void GetInternalPolylineLayerFromConfig()
        {
            string value;
            if (GetFromConfig(out value, INTENAL_POLYLINE_LAYER_VARIABLE))
            {
                InternalPolylineLayers = value;
            }
            else
            {
                Log.Warn(string.Format(CultureInfo.CurrentCulture, "Variable {0} ist nicht konfiguriert!",
                    INTENAL_POLYLINE_LAYER_VARIABLE));
                InternalPolylineLayers = "";
            }
        }

        private void GetFensterSchraffLayerFromConfig()
        {
            string value;
            if (GetFromConfig(out value, FENSTER_HATCH_LAYER_VARIABLE))
            {
                FensterSchraffLayer = value;
            }
            else
            {
                Log.Warn(string.Format(CultureInfo.CurrentCulture, "Variable {0} ist nicht konfiguriert!",
                    FENSTER_HATCH_LAYER_VARIABLE));
                FensterSchraffLayer = "NichtKonfiguriert";
            }
        }

        private void GetTuerSchraffLayerFromConfig()
        {
            string value;
            if (GetFromConfig(out value, TUER_HATCH_LAYER_VARIABLE))
            {
                TuerSchraffLayer = value;
            }
            else
            {
                Log.Warn(string.Format(CultureInfo.CurrentCulture, "Variable {0} ist nicht konfiguriert!",
                    TUER_HATCH_LAYER_VARIABLE));
                TuerSchraffLayer = "NichtKonfiguriert";
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
