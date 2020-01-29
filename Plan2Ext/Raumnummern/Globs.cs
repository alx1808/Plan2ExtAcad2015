using System;
#if BRX_APP
using Bricscad.ApplicationServices;
#elif ARX_APP
  using Autodesk.AutoCAD.ApplicationServices;
// ReSharper disable IdentifierTypo
#endif

namespace Plan2Ext.Raumnummern
{
    internal static class Globs
    {
        public static RnOptions TheRnOptions = null;

        public static void CancelCommand()
        {
            Application.DocumentManager.MdiActiveDocument.SendStringToExecute("\x1B", true, false, true);
            Application.DocumentManager.MdiActiveDocument.SendStringToExecute("\x1B", true, false, true);
        }

        public static bool IsGeschossSpecifier(string s)
        {
            if (string.IsNullOrEmpty(s)) return true;
            if (s.Equals("K", StringComparison.CurrentCultureIgnoreCase)) return true;
            int nr;
            return int.TryParse(s, out nr) && nr > 0;
        }

        public static void GetGeschossAndNr(string raumnr, out string geschoss, out string nr)
        {
            geschoss = "";
            if (string.IsNullOrEmpty(raumnr))
                throw new InvalidOperationException(string.Format("Ungültige Raumnummer '{0}'!", raumnr));
            
            int i;
            if (raumnr.Length < 3)
            {
                if (!int.TryParse(raumnr, out i)) throw new InvalidOperationException(string.Format("Ungültige Raumnummer '{0}'!", raumnr));
                nr = i.ToString().PadLeft(2, '0');
                return;
            }

            geschoss = raumnr.Remove(raumnr.Length-2, 2);
            var geschossLen = geschoss.Length;
            if (geschoss == "0") geschoss = "";
            if (!IsGeschossSpecifier(geschoss))
            {
                throw new InvalidOperationException(string.Format("Ungültige Raumnummer '{0}'!", raumnr));
            }

            var raumnr2 = raumnr.Remove(0, geschossLen);
            if (!int.TryParse(raumnr2, out i)) throw new InvalidOperationException(string.Format("Ungültige Raumnummer '{0}'!", raumnr));
            nr = i.ToString().PadLeft(2, '0');
        }
    }
}
