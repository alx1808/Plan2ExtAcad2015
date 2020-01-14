#if BRX_APP
using Bricscad.ApplicationServices;
#elif ARX_APP
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Autodesk.AutoCAD.ApplicationServices;
#endif


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plan2Ext
{
	internal class UndoHandler
	{
		public static void StartUndoMark()
		{
			var doc = Application.DocumentManager.MdiActiveDocument;
			dynamic acDoc = doc.GetAcadDocument();
			acDoc.StartUndoMark();
		}
		public static void EndUndoMark()
		{
			var doc = Application.DocumentManager.MdiActiveDocument;
			dynamic acDoc = doc.GetAcadDocument();
			acDoc.EndUndoMark();
		}
	}
}
