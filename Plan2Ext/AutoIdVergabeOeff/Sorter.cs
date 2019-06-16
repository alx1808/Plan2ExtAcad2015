using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices.Core;

namespace Plan2Ext.AutoIdVergabeOeff
{
    internal class Sorter
    {
        private const string TEMP_UCS_NAME = "plan2autoidoeffnungen_temp_ucs";

        protected void UcsRestore()
        {
            var ed = Application.DocumentManager.MdiActiveDocument.Editor;
            ed.Command("_.UCS", "_R", TEMP_UCS_NAME);
            ed.Command("_.UCS", "_D", TEMP_UCS_NAME);
        }

        protected void UcsToAnsicht()
        {
            var ed = Application.DocumentManager.MdiActiveDocument.Editor;
            ed.Command("_.UCS", "_D", TEMP_UCS_NAME);
            ed.Command("_.UCS", "_S", TEMP_UCS_NAME);
            ed.Command("_.UCS", "_V");
        }

    }
}
