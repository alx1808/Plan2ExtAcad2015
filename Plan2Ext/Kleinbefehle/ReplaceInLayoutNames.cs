using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
using _AcLm = Autodesk.AutoCAD.LayerManager;
using System.Globalization;
using System.Text.RegularExpressions;
#endif

namespace Plan2Ext.Kleinbefehle
{
    public class ReplaceInLayoutNamesClass
    {
        #region log4net Initialization
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Convert.ToString((typeof(ReplaceInLayoutNamesClass))));
        #endregion

        #region Member variables
        private static string _OldText = string.Empty;
        private static string _NewText = string.Empty;
        private static _AcDb.Transaction _Tr = null;
        private static _AcDb.Database _Db = null;
        #endregion

        [_AcTrx.CommandMethod("Plan2ReplaceInLayoutNames")]
        public static void Plan2ReplaceInLayoutNames()
        {
            _AcAp.Document doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            _Db = doc.Database;
            _AcEd.Editor ed = doc.Editor;

            _OldText = string.Empty;
            _NewText = string.Empty;

            try
            {
                var layoutNames = Plan2Ext.Layouts.GetLayoutNames();
                layoutNames = layoutNames.Where(x => string.Compare(x, "Model", StringComparison.OrdinalIgnoreCase) != 0).ToList();

                if (!GetOldText(ed)) return;
                if (!GetNewText(ed)) return;

                _Tr = _Db.TransactionManager.StartTransaction();
                using (_Tr)
                {
                    _AcDb.LayoutManager layoutMgr = _AcDb.LayoutManager.Current;

                    foreach (var name in layoutNames)
                    {
                        bool changed;
                        var newT = ReplaceTexts(name, out changed);
                        if (changed)
                        {
                            layoutMgr.RenameLayout(name, newT);
                        }
                    }

                    _Tr.Commit();
                }
            }
            catch (System.Exception ex)
            {
                string msg = string.Format(CultureInfo.CurrentCulture, "Fehler in (Plan2ReplaceInLayoutNames): {0}", ex.Message);
                ed.WriteMessage("\n" + msg);
                System.Windows.Forms.MessageBox.Show(ex.Message, "Plan2ReplaceInLayoutNames");
            }
        }

        private static string ReplaceTexts(string txt, out bool changed)
        {
            var newT = Regex.Replace(txt, _OldText, _NewText, RegexOptions.IgnoreCase);
            //var newT = txt.Replace(_OldText, _NewText);
            if (string.Compare(newT, txt, StringComparison.OrdinalIgnoreCase) == 0) changed = false;
            else changed = true;
            return newT;
        }

        private static bool GetNewText(_AcEd.Editor ed)
        {
            var prompt = new _AcEd.PromptStringOptions("\nNeuer Text: ");
            prompt.AllowSpaces = true;
            var prefixUserRes = ed.GetString(prompt);
            if (prefixUserRes.Status != _AcEd.PromptStatus.OK)
            {
                return false;
            }
            _NewText = prefixUserRes.StringResult;
            return true;
        }

        private static bool GetOldText(_AcEd.Editor ed)
        {
            var prompt = new _AcEd.PromptStringOptions("\nZu ersetzender Text: ");
            prompt.AllowSpaces = true;
            while (string.IsNullOrEmpty(_OldText))
            {
                var prefixUserRes = ed.GetString(prompt);
                if (prefixUserRes.Status != _AcEd.PromptStatus.OK)
                {
                    return false;
                }
                _OldText = prefixUserRes.StringResult;
            }
            return true;
        }
    }
}
