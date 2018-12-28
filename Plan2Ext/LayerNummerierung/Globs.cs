using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

// ReSharper disable IdentifierTypo

namespace Plan2Ext.LayerNummerierung
{
    internal static class Globs
    {
        public static NrOptions TheNrOptions = null;

        // ReSharper disable once UnusedMember.Global
        public static void CancelCommand()
        {
            Application.DocumentManager.MdiActiveDocument.SendStringToExecute("\x1B", true, false, true);
            Application.DocumentManager.MdiActiveDocument.SendStringToExecute("\x1B", true, false, true);
        }
    }
}
