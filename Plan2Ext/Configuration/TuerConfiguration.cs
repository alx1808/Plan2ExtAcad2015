using System;
using System.Globalization;

// ReSharper disable StringLiteralTypo
// ReSharper disable IdentifierTypo

namespace Plan2Ext.Configuration
{
    internal static class TuerConfiguration
    {
        public static string TuerblockOben
        {
            get { return TheConfiguration.GetValue("alx_V:ino_tuerBlock_Oben").ToString(); }
        }
        public static string TuerblockUnten
        {
            get { return TheConfiguration.GetValue("alx_V:ino_tuerBlock_Unten").ToString(); }
        }
        public static string TuerblockLinks
        {
            get { return TheConfiguration.GetValue("alx_V:ino_tuerBlock_Links").ToString(); }
        }
        public static string TuerblockRechts
        {
            get { return TheConfiguration.GetValue("alx_V:ino_tuerBlock_Rechts").ToString(); }
        }

        public static bool Pertains(string blockName)
        {
            return (
                blockName.Equals(TheConfiguration.GetValue("alx_V:ino_tuerBlock_Rechts").ToString(),
                    StringComparison.InvariantCultureIgnoreCase) ||
                blockName.Equals(TheConfiguration.GetValue("alx_V:ino_tuerBlock_Oben").ToString(),
                    StringComparison.InvariantCultureIgnoreCase) ||
                blockName.Equals(TheConfiguration.GetValue("alx_V:ino_tuerBlock_Links").ToString(),
                    StringComparison.InvariantCultureIgnoreCase) ||
                blockName.Equals(TheConfiguration.GetValue("alx_V:ino_tuerBlock_Unten").ToString(),
                    StringComparison.InvariantCultureIgnoreCase)
            );
        }

        public static double GetRotation(string blockName)
        {
            if (blockName.Equals(TheConfiguration.GetValue("alx_V:ino_tuerBlock_Rechts").ToString(),
                StringComparison.InvariantCultureIgnoreCase))
            {
                return 0;
            }
            if (blockName.Equals(TheConfiguration.GetValue("alx_V:ino_tuerBlock_Oben").ToString(),
                StringComparison.InvariantCultureIgnoreCase))
            {
                return Math.PI * 0.5;
            }
            if (blockName.Equals(TheConfiguration.GetValue("alx_V:ino_tuerBlock_Links").ToString(),
                StringComparison.InvariantCultureIgnoreCase))
            {
                return Math.PI;
            }
            if (blockName.Equals(TheConfiguration.GetValue("alx_V:ino_tuerBlock_Unten").ToString(),
                StringComparison.InvariantCultureIgnoreCase))
            {
                return Math.PI * 1.5;
            }
            throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, "{0} ist kein Türblockname!", blockName));
        }
    }
}
