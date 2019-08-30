using System.Collections.Generic;
using System.Linq;
using System.Text;

using System;
using System.Runtime.InteropServices;

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
//using _AcIntCom = BricscadDb;
//using _AcInt = BricscadApp;
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
using _AcIntCom = Autodesk.AutoCAD.Interop.Common;
using _AcInt = Autodesk.AutoCAD.Interop;
#endif


// Sample: calling acedInvoke from managed code.
// 
// To call LISP functions via acedInvoke, they must be 
// defined with a C: prefix, or they must be registered 
// using (vl-acad-defun)
//
// The test code below below requires the following LISP:
// 
// (defun testfunc ( a b )
// (princ "\nTESTFUNC called: )
// (prin1 a)
// (prin1 b)
// ;; return a list of results:
// (list 99 "Just ditch the LISP and forget this nonsense" pi)
// )
// 
// ;; register the function to be externally callable:
// 
// (vl-acad-defun 'testfunc)
// 
// 

namespace CADDZone.AutoCAD.Samples
{
    public class AcedInvokeSample
    {
        public const int RTLONG = 5010; // adscodes.h
        public const int RTSTR = 5005;

        public const int RTNORM = 5100;

        public AcedInvokeSample()
        {
        }

#if ACAD2013_OR_NEWER

        const String IMPORT_FILE_NAME = "accore.dll";
#else
		const String IMPORT_FILE_NAME = "acad.exe";
#endif

		[System.Security.SuppressUnmanagedCodeSecurity]
        [DllImport(IMPORT_FILE_NAME, CallingConvention = CallingConvention.Cdecl)]
        extern static private int acedInvoke(IntPtr args, out IntPtr result);

        // Helper for acedInvoke()

        public static _AcDb.ResultBuffer InvokeLisp(_AcDb.ResultBuffer args, ref int stat)
        {
#if BRX_APP
	        stat = RTNORM;
	        return Bricscad.Global.Editor.Invoke(args);
#else
	        IntPtr rb = IntPtr.Zero;
            stat = acedInvoke(args.UnmanagedObject, out rb);
            if (stat == (int)_AcEd.PromptStatus.OK && rb != IntPtr.Zero)
                return (_AcDb.ResultBuffer)
               _AcTrx.DisposableWrapper.Create(typeof(_AcDb.ResultBuffer), rb, true);
            return null;
#endif
		}

		static void PrintResbuf(_AcDb.ResultBuffer rb)
        {
            string s = "\n-----------------------------";
            foreach (_AcDb.TypedValue val in rb)
                s += string.Format("\n{0} -> {1}", val.TypeCode,
               val.Value.ToString());
            s += "\n-----------------------------";
            _AcAp.Application.DocumentManager.MdiActiveDocument.
            Editor.WriteMessage(s);
        }

        public static void InvokeCLisp(string funcNameWithoutC)
        {
            _AcDb.ResultBuffer args = new _AcDb.ResultBuffer();
            int stat = 0;

            args.Add(new _AcDb.TypedValue(RTSTR, "c:" + funcNameWithoutC));

            _AcDb.ResultBuffer res = InvokeLisp(args, ref stat);
            if (stat == RTNORM && res != null)
            {
                PrintResbuf(res);
                res.Dispose();
            }
        }

        public static void InvokeCGeoin()
        {
            _AcDb.ResultBuffer args = new _AcDb.ResultBuffer();
            int stat = 0;

            args.Add(new _AcDb.TypedValue(RTSTR, "c:geoin"));

            _AcDb.ResultBuffer res = InvokeLisp(args, ref stat);
            if (stat == RTNORM && res != null)
            {
                PrintResbuf(res);
                res.Dispose();
            }
        }

        public static void TestInvokeLisp()
        {
            _AcDb.ResultBuffer args = new _AcDb.ResultBuffer();
            int stat = 0;


            args.Add(new _AcDb.TypedValue(RTSTR, "getplan2config"));
            
            //args.Add(new TypedValue(RTSTR, "vlax-ldata-get"));
            //args.Add(new TypedValue(RTSTR, "AA_PLAN2"));
            //args.Add(new TypedValue(RTSTR, "ConfigFile"));

            //args.Add(new TypedValue(RTLONG, 100));
            //args.Add(new TypedValue(RTLONG, 200));

            _AcDb.ResultBuffer res = InvokeLisp(args, ref stat);
            if (stat == RTNORM && res != null)
            {
                PrintResbuf(res);
                res.Dispose();
            }
        }
    }
}

//-- 
//http://www.caddzone.com

//AcadXTabs: MDI Document Tabs for AutoCAD 2004/2005/2006
// http://www.acadxtabs.com
