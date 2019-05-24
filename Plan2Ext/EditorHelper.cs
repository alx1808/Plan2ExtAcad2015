using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
// ReSharper disable IdentifierTypo

namespace Plan2Ext
{
    internal static class EditorHelper
    {
        public static ObjectId Entlast()
        {
            var res = Application.DocumentManager.MdiActiveDocument.Editor.SelectLast();
            if (res.Status != PromptStatus.OK) return ObjectId.Null;

            var ss = res.Value;
            if (ss == null) return ObjectId.Null;

            var oids = ss.GetObjectIds();
            if (oids == null || oids.Length == 0) return ObjectId.Null;

            return oids[0];
        }
    }
}
