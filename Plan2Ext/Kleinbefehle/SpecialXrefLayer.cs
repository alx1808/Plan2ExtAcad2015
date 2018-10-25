// ReSharper disable CommentTypo
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
using Autodesk.AutoCAD.ApplicationServices.Core;
#endif
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Plan2Ext.Kleinbefehle
{
    // ReSharper disable once UnusedMember.Global
    public class SpecialXrefLayer
    {

        private readonly List<string> _gewerkBezForActive = new List<string>() { "Bestand", "Hochbau" };

        [_AcTrx.CommandMethod("Plan2SpecialXrefLayer")]
        // ReSharper disable once UnusedMember.Global
        public void Plan2SpecialXrefLayer()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            _AcEd.Editor ed = doc.Editor;

            _AcEd.PromptSelectionResult res;
            if (!SelectXrefs(ed, out res)) return;


#if BRX_APP
            _AcEd.SelectionSet ss = res.Value;
#else
            using (_AcEd.SelectionSet ss = res.Value)
#endif
            {
                _AcDb.ObjectId[] idArray = ss.GetObjectIds();

                using (var trans = doc.TransactionManager.StartTransaction())
                {
                    var oidAndDwgNames = idArray.Where(x => Globs.IsXref(x, trans)).Select(x => new { oid = x, dwgName = GetXrefDwgName(x, trans) }).ToList();
                    foreach (var oidAndDwgName in oidAndDwgNames)
                    {
                        string gewerk;
                        if (!GetGewerk(ed, string.Format(CultureInfo.CurrentCulture, "\nGewerk für {0}: ",oidAndDwgName.dwgName), out gewerk))
                        {
                            trans.Abort();
                            return;
                        }
                        var activeGewerk = _gewerkBezForActive.FirstOrDefault(x => Regex.IsMatch(gewerk, x, RegexOptions.IgnoreCase));
                        var activePart = activeGewerk != null ? "Aktiv" : "Inaktiv";
                        var layerName = string.Format(CultureInfo.InvariantCulture,
                            "XREF-$-{0}-$-{1}-$-{2}", oidAndDwgName.dwgName, gewerk,activePart);
                        Globs.CreateLayer(layerName);
                        var xref = (_AcDb.Entity) trans.GetObject(oidAndDwgName.oid, _AcDb.OpenMode.ForRead);
                        xref.UpgradeOpen();
                        xref.Layer = layerName;
                        xref.DowngradeOpen();
                    }
                    trans.Commit();
                }
            }
        }

        private static bool SelectXrefs(_AcEd.Editor ed, out _AcEd.PromptSelectionResult res)
        {
            _AcEd.SelectionFilter filter = new _AcEd.SelectionFilter(new[]
            {
                new _AcDb.TypedValue((int) _AcDb.DxfCode.Start, "INSERT"),
            });

            var promptSelectionOptions = new _AcEd.PromptSelectionOptions {RejectPaperspaceViewport = true};

            res = ed.GetSelection(promptSelectionOptions, filter);
            if (res.Status != _AcEd.PromptStatus.OK)
            {
                if (res.Status == _AcEd.PromptStatus.Cancel) return false;
            }

            return true;
        }

        private static string GetXrefDwgName(_AcDb.ObjectId oid, _AcDb.Transaction tr)
        {
            var br = (_AcDb.BlockReference)tr.GetObject(oid, _AcDb.OpenMode.ForRead);
            var bd = (_AcDb.BlockTableRecord)tr.GetObject(br.BlockTableRecord, _AcDb.OpenMode.ForRead);
            return Path.GetFileNameWithoutExtension(bd.PathName);
        }

        private bool GetGewerk(_AcEd.Editor ed, string question, out string gewerk)
        {
            gewerk = "";
            var promptResult = ed.GetString(question);
            if (promptResult.Status != _AcEd.PromptStatus.OK) return false;
            gewerk = promptResult.StringResult;
            return true;
        }
    }
}
