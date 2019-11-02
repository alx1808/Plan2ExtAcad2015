using System;
using System.Globalization;

// ReSharper disable StringLiteralTypo
// ReSharper disable IdentifierTypo

namespace Plan2Ext.Configuration
{
    internal static class FensterConfiguration
    {
        public static string FensterblockOben
        {
            get { return TheConfiguration.GetValue("alx_V:ino_fenster_Block_Oben").ToString(); }
        }
        public static string FensterblockUnten
        {
            get { return TheConfiguration.GetValue("alx_V:ino_fenster_Block_Unten").ToString(); }
        }
        public static string FensterblockLinks
        {
            get { return TheConfiguration.GetValue("alx_V:ino_fenster_Block_Links").ToString(); }
        }
        public static string FensterblockRechts
        {
            get { return TheConfiguration.GetValue("alx_V:ino_fenster_Block_Rechts").ToString(); }
        }


        public static bool Pertains(string blockName)
        {
            return (
                blockName.Equals(TheConfiguration.GetValue("alx_V:ino_fenster_Block_Rechts").ToString(),
                    StringComparison.InvariantCultureIgnoreCase) ||
                blockName.Equals(TheConfiguration.GetValue("alx_V:ino_fenster_Block_Oben").ToString(),
                    StringComparison.InvariantCultureIgnoreCase) ||
                blockName.Equals(TheConfiguration.GetValue("alx_V:ino_fenster_Block_Links").ToString(),
                    StringComparison.InvariantCultureIgnoreCase) ||
                blockName.Equals(TheConfiguration.GetValue("alx_V:ino_fenster_Block_Unten").ToString(),
                    StringComparison.InvariantCultureIgnoreCase)
            );
        }

        public static double GetRotation(string blockName)
        {
            if (blockName.Equals(TheConfiguration.GetValue("alx_V:ino_fenster_Block_Rechts").ToString(),
                StringComparison.InvariantCultureIgnoreCase))
            {
                return 0;
            }
            if (blockName.Equals(TheConfiguration.GetValue("alx_V:ino_fenster_Block_Oben").ToString(),
                StringComparison.InvariantCultureIgnoreCase))
            {
                return Math.PI * 0.5;
            }
            if (blockName.Equals(TheConfiguration.GetValue("alx_V:ino_fenster_Block_Links").ToString(),
                StringComparison.InvariantCultureIgnoreCase))
            {
                return Math.PI;
            }
            if (blockName.Equals(TheConfiguration.GetValue("alx_V:ino_fenster_Block_Unten").ToString(),
                StringComparison.InvariantCultureIgnoreCase))
            {
                return Math.PI * 1.5;
            }
            throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture,"{0} ist kein Fensterblockname!",blockName));
        }

    }
}
