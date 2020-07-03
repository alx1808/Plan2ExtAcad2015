using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Exception = System.Exception;

namespace Plan2Ext.Kleinbefehle
{
    public class RaumGruppierung
    {
        [CommandMethod("Plan2RaumGruppierung")]
        public void Plan2RaumGruppierung()
        {

            var doc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
            var editor = doc.Editor;
            try
            {

                // todo: ask gruppieren/prüfen

                var groupHelper = new GroupHelper();
                var invalidNrRbErrorLayerName = GetErrorLayerNames(out var noFgErrorLayerName);

                // todo: delete fehlerlines

                // todo: get layer and blockname and blocklayer
                if (!GetFgLayerAndBlockName(out var fgLayer, out var raumblockName, doc)) return;


                var fgOids = new List<ObjectId>();
                var raumBlockOids = new List<ObjectId>();

                var areaEngine = new AreaEngine();
                if (!areaEngine.SelectFgAndRb(fgOids, raumBlockOids, fgLayer, raumblockName))
                    return;
                var fgRbStructs = AreaEngine.GetFgRbStructs(fgOids, new ObjectId[0], raumBlockOids, doc.Database);
                var orphans = AreaEngine.OrphanRaumblocks;

                foreach (var fgOid in fgOids)
                {
                    var info = fgRbStructs[fgOid];
                    var rbCnt = info.Raumbloecke.Count;
                    if (rbCnt != 1)
                    {
                        // ReSharper disable once PossibleInvalidOperationException
                        FehlerLine(invalidNrRbErrorLayerName, 255, 0, 0, Globs.GetMiddlePoint(fgOid).Value);
                    }
                    else
                    {
                        // group
                        groupHelper.AddToGroup(new[] {info.FlaechenGrenze, info.Raumbloecke[0]},
                            info.Raumbloecke[0].Handle.ToString(), doc, true);
                    }
                }

                foreach (var orphan in orphans)
                {
                    // ReSharper disable once PossibleInvalidOperationException
                    FehlerLine(noFgErrorLayerName, 0, 0, 255, Globs.GetInsertPoint(orphan).Value);
                }
            }
            catch (Exception ex)
            {
                string msg = string.Format(CultureInfo.CurrentCulture, "Fehler in (Plan2RaumGruppierung): {0}",
                    ex.Message);
                editor.WriteMessage("\n" + msg);
                MessageBox.Show(ex.Message, @"Plan2RaumGruppierung");
            }
        }

        private bool GetFgLayerAndBlockName(out string fgLayer, out string raumblockName, Document doc)
        {

            var filter = new SelectionFilter(new[] {

                new TypedValue((int)DxfCode.Operator ,"<OR"),
                new TypedValue((int)DxfCode.Start ,"*POLYLINE"),
                new TypedValue((int)DxfCode.Start ,"INSERT"),
                new TypedValue((int)DxfCode.Operator ,"OR>")
            });


            fgLayer = null;
            raumblockName = null;

            var editor = doc.Editor;
            var selectionOptions = new PromptSelectionOptions
            {
                MessageForAdding = "Wählen Sie bitte den Raumblock und die Raumpolylinie: "
            };
            var res = editor.GetSelection(selectionOptions, filter);
            if (res.Status != PromptStatus.OK)
            {
                return false;
            }

            bool raumblockFound = false;
            bool fgFound = false;
            using (var transaction = doc.TransactionManager.StartTransaction())
            {
                try
                {

                    var oids = res.Value.GetObjectIds();
                    foreach (var oid in oids)
                    {
                        var ent = (Entity)transaction.GetObject(oid, OpenMode.ForRead);
                        var blockReference = ent as BlockReference;
                        if (blockReference != null)
                        {
                            if (!raumblockFound)
                            {
                                raumblockFound = true;
                                raumblockName = Globs.GetBlockname(blockReference, transaction);
                            }
                        }
                        else
                        {
                            if (!fgFound)
                            {
                                fgFound = true;
                                fgLayer = ent.Layer;
                            }
                        }

                        if (fgFound && raumblockFound) return true;
                    }

                }
                finally
                {
                    transaction.Commit();
                }
            }

            return false;
        }

        private static string GetErrorLayerNames(out string noFgErrorLayerName)
        {
            if (!GetFromConfig(out var invalidNrRbErrorLayerName, "alx_V:ino_InvalidNrRbLayer"))
                invalidNrRbErrorLayerName = "UngültigeRaumblockAnzahl ";
            noFgErrorLayerName = "KeineFlaechengrenze";
            return invalidNrRbErrorLayerName;
        }

        private static void FehlerLine(string layer, int red, int green, int blue, Point3d label)
        {
            Color col = Color.FromRgb((byte)red, (byte)green, (byte)blue);
            Globs.InsertFehlerLines(new List<Point3d> { label }, layer, 50, Math.PI * 1.25, col);
        }
        private static bool GetFromConfig(out string val, string varName)
        {
            val = null;
            try
            {
                val = TheConfiguration.GetValueString(varName);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

    }
}
