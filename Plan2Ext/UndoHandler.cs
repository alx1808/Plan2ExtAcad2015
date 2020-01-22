#if BRX_APP
using Bricscad.ApplicationServices;
using BricscadApp;
#elif ARX_APP
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Autodesk.AutoCAD.ApplicationServices;
#endif

namespace Plan2Ext
{
	internal class UndoHandler
	{
		public static void StartUndoMark()
		{
#if BRX_APP
			var doc = (IAcadDocument)Application.DocumentManager.MdiActiveDocument.AcadDocument;
			doc.StartUndoMark();
#else
			var doc = Application.DocumentManager.MdiActiveDocument;
			dynamic acDoc = doc.GetAcadDocument();
			acDoc.StartUndoMark();
#endif
		}
		public static void EndUndoMark()
		{
#if BRX_APP
			var doc = (IAcadDocument)Application.DocumentManager.MdiActiveDocument.AcadDocument;
			doc.EndUndoMark();

#else
			var doc = Application.DocumentManager.MdiActiveDocument;
			dynamic acDoc = doc.GetAcadDocument();
			acDoc.EndUndoMark();
#endif
		}
	}
}
