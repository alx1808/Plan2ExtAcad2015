using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using Autodesk.AutoCAD.Runtime;
//using Autodesk.AutoCAD.DatabaseServices;
//using Autodesk.AutoCAD.Windows;

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
using _AcIntCom = BricscadDb;
using _AcInt = BricscadApp;
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


namespace Plan2Ext
{
    public class LispFileDialogs
    {

        [_AcTrx.LispFunction("SaveFileDialog")]
        public static _AcDb.TypedValue LispSaveFileDialog(_AcDb.ResultBuffer args)
        {
            if (args == null) return new _AcDb.TypedValue((int)_AcBrx.LispDataType.Nil);
            string FileName, Ext, Title;
            bool Multiple;
            if (!GetArgsFromRbForFileDialog(args, out FileName, out Ext, out Title, out Multiple ))
            {
                return new _AcDb.TypedValue((int)_AcBrx.LispDataType.Nil);
            }
            else
            {
                _AcWnd.SaveFileDialog sfd = new _AcWnd.SaveFileDialog(Title, FileName, Ext, "LispSFD", _AcWnd.SaveFileDialog.SaveFileDialogFlags.AllowAnyExtension);
                System.Windows.Forms.DialogResult dr = sfd.ShowDialog();
                if (dr == System.Windows.Forms.DialogResult.OK) return new _AcDb.TypedValue((int)_AcBrx.LispDataType.Text, sfd.Filename);
                else return new _AcDb.TypedValue((int)_AcBrx.LispDataType.Nil);

            }
        }

        
        [_AcTrx.LispFunction("OpenFileDialog")]
        public static _AcDb.ResultBuffer LispOpenFileDialog(_AcDb.ResultBuffer args)
        {
            if (args == null) return new _AcDb.ResultBuffer(new _AcDb.TypedValue((int)_AcBrx.LispDataType.Nil));
            string FileName, Ext, Title;
            bool Multiple;
            if (!GetArgsFromRbForFileDialog(args, out FileName, out Ext, out Title, out Multiple ))
            {
                return new _AcDb.ResultBuffer(new _AcDb.TypedValue((int)_AcBrx.LispDataType.Nil));
            }
            else
            {
                _AcWnd.OpenFileDialog ofd;
                if (Multiple)
                    ofd = new _AcWnd.OpenFileDialog(Title, FileName, Ext, "LispOFD", _AcWnd.OpenFileDialog.OpenFileDialogFlags.AllowAnyExtension | _AcWnd.OpenFileDialog.OpenFileDialogFlags.AllowMultiple);
                else
                    ofd = new _AcWnd.OpenFileDialog(Title, FileName, Ext, "LispOFD", _AcWnd.OpenFileDialog.OpenFileDialogFlags.AllowAnyExtension);
                System.Windows.Forms.DialogResult dr = ofd.ShowDialog();
                if (dr == System.Windows.Forms.DialogResult.OK)
                {
                    if (Multiple)
                    {
                        _AcDb.ResultBuffer rb = new _AcDb.ResultBuffer();
                        foreach (string fname in ofd.GetFilenames())
                        {
                            rb.Add(new _AcDb.TypedValue((int)_AcBrx.LispDataType.Text, fname));
                        }
                        return rb;
                    }
                    else
                    return new _AcDb.ResultBuffer(new _AcDb.TypedValue((int)_AcBrx.LispDataType.Text, ofd.Filename));
                }
                else return new _AcDb.ResultBuffer(new _AcDb.TypedValue((int)_AcBrx.LispDataType.Nil));
            }
        }

        [_AcTrx.LispFunction("OpenFileDialog2")]
        public static _AcDb.ResultBuffer LispOpenFileDialog2(_AcDb.ResultBuffer args)
        {
            if (args == null) return new _AcDb.ResultBuffer(new _AcDb.TypedValue((int)_AcBrx.LispDataType.Nil));
            string FileName, Ext, Title;
            bool Multiple;
            if (!GetArgsFromRbForFileDialog(args, out FileName, out Ext, out Title, out Multiple))
            {
                return new _AcDb.ResultBuffer(new _AcDb.TypedValue((int)_AcBrx.LispDataType.Nil));
            }
            else
            {
                System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
                ofd.CheckFileExists = true;
                ofd.Filter = Ext + "|*." + Ext;
                ofd.Multiselect = Multiple;
                ofd.Title = Title;
                if (!string.IsNullOrEmpty(FileName))
                {
                    ofd.InitialDirectory = System.IO.Path.GetDirectoryName(FileName);
                }

                System.Windows.Forms.DialogResult res = ofd.ShowDialog();
                if (res == System.Windows.Forms.DialogResult.OK)
                {
                    if (Multiple)
                    {
                        _AcDb.ResultBuffer rb = new _AcDb.ResultBuffer();
                        foreach (string fname in ofd.FileNames)
                        {
                            rb.Add(new _AcDb.TypedValue((int)_AcBrx.LispDataType.Text, fname));
                        }
                        return rb;
                    }
                    else
                        return new _AcDb.ResultBuffer(new _AcDb.TypedValue((int)_AcBrx.LispDataType.Text, ofd.FileName));

                }
                else return new _AcDb.ResultBuffer(new _AcDb.TypedValue((int)_AcBrx.LispDataType.Nil));

            }
        }

        [_AcTrx.LispFunction("Plan2FolderDialog")]
        public static object Plan2FolderDialog(_AcDb.ResultBuffer args)
        {
            string title, defaultPath;
            if (!GetArgsFromRbForFolderDialog(args, out title, out defaultPath))
            {
                return null;
            }

            using (var folderBrowser = new System.Windows.Forms.FolderBrowserDialog())
            {
                if (!string.IsNullOrEmpty(title)) folderBrowser.Description = title;
                folderBrowser.RootFolder = Environment.SpecialFolder.MyComputer;
                if (!string.IsNullOrEmpty(defaultPath))
                {
                    folderBrowser.SelectedPath = defaultPath;
                }

                if (folderBrowser.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                {
                    return null;
                }

                return folderBrowser.SelectedPath;
            }
        }

        private static bool GetArgsFromRbForFileDialog(_AcDb.ResultBuffer args, out string FileName, out string Ext, out string Title, out bool Multiple)
        {
            FileName = ""; Ext = ""; Title = ""; Multiple = false;
            _AcDb.TypedValue[] array = args.AsArray();
            if (array.Length < 3) return false;
            if (array[0].TypeCode != (int)_AcBrx.LispDataType.Text) return false;
            Title = array[0].Value.ToString();
            if (array[1].TypeCode != (int)_AcBrx.LispDataType.Text) return false;
            FileName = array[1].Value.ToString();
            if (array[2].TypeCode != (int)_AcBrx.LispDataType.Text) return false;
            Ext = array[2].Value.ToString();
            if (array.Length > 3)
            {
                if (array[3].TypeCode == (int)_AcBrx.LispDataType.T_atom)
                {
                    if (array[3].Value == null) Multiple = false;
                    else Multiple = (bool)array[3].Value;
                }
            }

            return true;
        }
        private static bool GetArgsFromRbForFolderDialog(_AcDb.ResultBuffer args, out string title, out string defaultPath)
        {
            title = null;
            defaultPath = null;
            if (args == null) return false;
            _AcDb.TypedValue[] array = args.AsArray();
            if (array.Length < 2) return false;
            if (array[0].TypeCode != (int)_AcBrx.LispDataType.Text) return false;
            title = array[0].Value.ToString();
            if (array[1].TypeCode != (int)_AcBrx.LispDataType.Text) return false;
            defaultPath = array[1].Value.ToString();

            return true;
        }

    }
}
