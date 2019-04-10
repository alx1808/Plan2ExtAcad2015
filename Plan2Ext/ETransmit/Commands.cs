using System;
using System.Globalization;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
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
                var dirName = string.Empty;
                string[] dwgFileNames = null;
                if (!Globs.GetMultipleFileNames(
                    "AutoCAD-Zeichnung",
                    "Dwg", 
                    "Verzeichnis mit Zeichnungen für ETransmit",
                    "Zeichnungen für ETransmit", 
                    ref dwgFileNames, 
                    ref dirName, 
                    Application.GetSystemVariable("DWGPREFIX").ToString()))
                {
                    return;
                }

                //var insertBind = GetUserInputForInsertBind();
                //CheckXRefBinding(insertBind);
            }
            catch (OperationCanceledException)
            {
                // cancelled by user
            }
            catch (System.Exception ex)
            {
                Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture,
                    "Fehler in Plan2ETransmit aufgetreten! {0}", ex.Message));
            }
        }

        private bool GetUserInputForInsertBind()
        {
            var pKeyOpts = new PromptKeywordOptions("") { Message = "\nXRefs Binden/<Einfügen>: " };
            pKeyOpts.Keywords.Add("Binden");
            pKeyOpts.Keywords.Add("Einfügen");
            pKeyOpts.AllowNone = true;
            var doc = Application.DocumentManager.MdiActiveDocument;
            var ed = doc.Editor;
            PromptResult pKeyRes = ed.GetKeywords(pKeyOpts);
            if (pKeyRes.Status == PromptStatus.Cancel)
            {
                throw new OperationCanceledException();
            }
            if (pKeyRes.Status == PromptStatus.None || pKeyRes.StringResult == "Einfügen") return true;
            if (pKeyRes.Status == PromptStatus.OK) return false;
            throw new InvalidOperationException("Userinput Status: " + pKeyRes.Status);
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
