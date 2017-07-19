// Only for AutoCAD 2007+
// This class is using undocumented function acedEvaluateLisp
//
using System.Runtime.InteropServices;
using System;
//using Autodesk.AutoCAD.ApplicationServices;
//using Autodesk.AutoCAD.DatabaseServices;
//using Autodesk.AutoCAD.Runtime;
//using Autodesk.AutoCAD.EditorInput;
//using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;

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
#endif


//[assembly: CommandClass(typeof(Rivilis.CSharpToLisp))]
namespace Rivilis
{
    public class CSharpToLisp
    {
        //
        // From adscodes.h :
        //
        // Type of resbuf element
        const int RTNONE = 5000; /* No result */
        const int RTREAL = 5001; /* Real number */
        const int RTPOINT = 5002; /* 2D point X and Y only */
        const int RTSHORT = 5003; /* Short integer */
        const int RTANG = 5004; /* Angle */
        const int RTSTR = 5005; /* String */
        const int RTENAME = 5006; /* Entity name */
        const int RTPICKS = 5007; /* Pick set */
        const int RTORINT = 5008; /* Orientation */
        const int RT3DPOINT = 5009; /* 3D point - X, Y, and Z */
        const int RTLONG = 5010; /* Long integer */
        const int RTVOID = 5014; /* Blank symbol */
        const int RTLB = 5016; /* list begin */
        const int RTLE = 5017; /* list end */
        const int RTDOTE = 5018; /* dotted pair */
        const int RTNIL = 5019; /* nil */
        const int RTDXF0 = 5020; /* DXF code 0 for ads_buildlist only */
        const int RTT = 5021; /* T atom */
        const int RTRESBUF = 5023; /* resbuf */

#if ACAD2013_OR_NEWER
        const String _fileName = "accore.dll";        

#else

        const String _fileName = "acad.exe";

#endif


        [System.Security.SuppressUnmanagedCodeSecurity]

        [DllImport(_fileName, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl,
            /* The 'EntryPoint' value is individual for each AutoCAD version, and  platform (x86/x64) */

          EntryPoint = "?acedEvaluateLisp@@YAHPEB_WAEAPEAUresbuf@@@Z")]

        extern private static int acedEvaluateLisp(string lispLine, out IntPtr result);
        static public _AcDb.ResultBuffer AcadEvalLisp(string arg)
        {
            IntPtr rb = IntPtr.Zero;
            acedEvaluateLisp(arg, out rb);
            if (rb != IntPtr.Zero)
            {
                try
                {
                    _AcDb.ResultBuffer rbb = _AcTrx.DisposableWrapper.Create(typeof(_AcDb.ResultBuffer), rb, true) as _AcDb.ResultBuffer;
                    return rbb;
                }
                catch
                {
                    return null;
                }
            }
            return null;
        }
        // Define Command "CSharpToLisp"
        // Only for testing we can define this function.
        //
        // Example:
        //   Command: CSharpToLisp
        //   Enter lisp expression: (+ 100 50 30 20 10)
        //   -----------------------------
        //    5003 -> 210
        //   -----------------------------
        [_AcTrx.CommandMethod("CSharpToLisp")]
        static public void test()
        {
            _AcEd.PromptResult rs =
              _AcAp.Application.DocumentManager.MdiActiveDocument.Editor.GetString("\nEnter lisp expression: ");
            if (rs.Status == _AcEd.PromptStatus.OK && rs.StringResult != "")
            {
                _AcDb.ResultBuffer rb = AcadEvalLisp(rs.StringResult);
                if (rb != null)
                {
                    PrintResbuf(rb);
                }
                else
                {
                    _AcAp.Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nError in evaluation");
                }
            }
        }
        // This code was posted by Tony Tanzillo:
        // http://discussion.autodesk.com/thread...ID=5094658
        private static void PrintResbuf(_AcDb.ResultBuffer rb)
        {
            string s = "\n-----------------------------";
            foreach (_AcDb.TypedValue val in rb)
            {
                s += string.Format("\n{0} -> {1}", val.TypeCode,
                val.Value.ToString());
                s += "\n-----------------------------";
            }
            
            _AcAp.Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage(s);
        }
    }
}