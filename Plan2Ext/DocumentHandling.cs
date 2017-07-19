#if ARX_APP
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Interop;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using System.Globalization;

namespace Plan2Ext
{
    public class DocumentHandling
    {

        //[LispFunction("Plan2CloseOpen")]
        /// <summary>
        /// Closes current DWG (discards changes) and opens it again
        /// </summary>
        [CommandMethod("$Plan2Reopen", CommandFlags.Session)]
        static public void Plan2Reopen()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            string strDWGName = doc.Name;
            // First cancel any running command
            if (doc.CommandInProgress != "" && string.Compare(doc.CommandInProgress, "$Plan2Reopen", StringComparison.OrdinalIgnoreCase) != 0)
            {
                AcadDocument oDoc = (AcadDocument)doc.GetAcadDocument();
                oDoc.SendCommand("\x03\x03");
            }

            doc.CloseAndDiscard();

            Application.DocumentManager.Open(strDWGName, false);
        }

        [LispFunction("Plan2SaveAs")]
        public static bool Plan2SaveAs(ResultBuffer rb)
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            try
            {
                //string strDWGName = acDoc.Name;
                //object obj = Application.GetSystemVariable("DWGTITLED");
                // Check to see if the drawing has been named
                //if (System.Convert.ToInt16(obj) == 0) return false;


                if (rb == null)
                {
                    Plan2SaveAsAufrufInfo(ed);
                    return false;
                }
                TypedValue[] values = rb.AsArray();
                if (values == null || values.Length < 1)
                {
                    Plan2SaveAsAufrufInfo(ed);
                    return false;
                }
                if (values[0].Value == null) return false;
                string newDwgName = values[0].Value.ToString();
                if (string.IsNullOrEmpty(newDwgName))
                {
                    Plan2SaveAsAufrufInfo(ed);
                    return false;
                }
                //if (System.IO.File.Exists(newDwgName)) return false;

                Document acDoc = Application.DocumentManager.MdiActiveDocument;
                acDoc.Database.SaveAs(newDwgName, false, DwgVersion.Current, acDoc.Database.SecurityParameters);
                return true;
            }
            catch (System.Exception ex)
            {
                string msg = string.Format(CultureInfo.CurrentCulture, "Fehler in (Plan2FinishPlot): {0}", ex.Message);
                if (ex.Message == "eFileSharingViolation")
                {
                    msg = "Die Datei kann nicht geschrieben werden. Eventuell ist sie im Editor geöffnet.";
                }
                ed.WriteMessage("\n" + msg);
                System.Windows.Forms.MessageBox.Show(msg, "Plan2SaveAs");
                return false;
            }
        }

        private static void Plan2SaveAsAufrufInfo(Editor ed)
        {
            ed.WriteMessage("\nAufruf: (Plan2SaveAs \"<Dwgname>\")!");
        }
    }
}
#endif