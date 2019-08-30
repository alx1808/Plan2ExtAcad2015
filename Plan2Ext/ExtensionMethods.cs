// ReSharper disable CommentTypo
using System;
#if BRX_APP
using _AcAp = Bricscad.ApplicationServices;
//using _AcBr = Teigha.BoundaryRepresentation;
using _AcCm = Teigha.Colors;
using _AcDb = Teigha.DatabaseServices;
using _AcEd = Bricscad.EditorInput;
using _AcGe = Teigha.Geometry;
using _AcGi = Teigha.GraphicsInterface;
using _AcGs = Teigha.GraphicsSystem;
using _AcPl = Bricscad.PlottingServices;
using _AcBrx = Bricscad.Runtime;
using _AcTrx = Teigha.Runtime;
using _AcWnd = Bricscad.Windows;
#elif ARX_APP
  using _AcAp = Autodesk.AutoCAD.ApplicationServices;
  using _AcBr = Autodesk.AutoCAD.BoundaryRepresentation;
  using _AcCm = Autodesk.AutoCAD.Colors;
  using _AcDb = Autodesk.AutoCAD.DatabaseServices;
  using _AcEd = Autodesk.AutoCAD.EditorInput;
  using _AcGe = Autodesk.AutoCAD.Geometry;
  using _AcGi = Autodesk.AutoCAD.GraphicsInterface;
  using _AcGs = Autodesk.AutoCAD.GraphicsSystem;
  using _AcPl = Autodesk.AutoCAD.PlottingServices;
  using _AcBrx = Autodesk.AutoCAD.Runtime;
  using _AcTrx = Autodesk.AutoCAD.Runtime;
  using _AcWnd = Autodesk.AutoCAD.Windows;
// ReSharper disable CommentTypo
#endif


namespace Plan2Ext
{
    public static class ExtensionMethods
    {
        // ReSharper disable once InconsistentNaming
        public static double Distance2dTo(this _AcGe.Point3d point, _AcGe.Point3d otherPoint)
        {
            return Math.Sqrt(Math.Pow((otherPoint.X - point.X), 2.0) + Math.Pow((otherPoint.Y - point.Y), 2.0));
        }

		/// <summary>
		/// Unterschied Acad-BricsCAD bei Returneingabe von GetNestedEntity, wenn AllowNone=false
		/// </summary>
		/// <param name="editor"></param>
		/// <param name="msg"></param>
		/// <param name="allowNone"></param>
		/// <remarks>
		/// Acad: Liefert Status Cancel
		/// BricsCAD: Bleibt in Abfragemodus
		/// Daher wird mit GetNestedEntityEx der Defaultmodus für allowNone auf true gesetzt.
		/// </remarks>
		/// <returns></returns>
		public static _AcEd.PromptNestedEntityResult GetNestedEntityEx(this _AcEd.Editor editor, string msg, bool allowNone = true)
        {
            var promptNestedEntityOptions =
                new _AcEd.PromptNestedEntityOptions(msg) { AllowNone = allowNone};
            return editor.GetNestedEntity(promptNestedEntityOptions);
        }
    }
}
