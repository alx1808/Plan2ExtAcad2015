using System.Globalization;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
// ReSharper disable StringLiteralTypo

namespace Plan2Ext.ETransmit
{
    // ReSharper disable once UnusedMember.Global
    public class Commands
    {
        [CommandMethod("Plan2ETransmit")]
        // ReSharper disable once UnusedMember.Global
        public void Plan2ETransmit()
        {
            try
            {
                CheckXRefBinding(false);
            }
            catch (System.Exception ex)
            {
                Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture,
                    "Fehler in Plan2ETransmit aufgetreten! {0}", ex.Message));
            }
        }

        private void CheckXRefBinding(bool insertBind)
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            var xrefObjectIds = Globs.GetAllMsXrefIds(doc.Database);
            using (ObjectIdCollection acXrefIdCol = new ObjectIdCollection())
            {
                foreach (var xrefObjectId in xrefObjectIds)
                {
                    acXrefIdCol.Add(xrefObjectId);
                    
                }
                if (acXrefIdCol.Count > 0)
                    doc.Database.BindXrefs(acXrefIdCol, insertBind);
            }
        }
    }
}
