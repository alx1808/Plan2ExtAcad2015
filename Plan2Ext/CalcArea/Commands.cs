//using Autodesk.AutoCAD.ApplicationServices;
//using Autodesk.AutoCAD.DatabaseServices;
//using Autodesk.AutoCAD.EditorInput;
//using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


#if BRX_APP
using Bricscad.ApplicationServices;
using Teigha.DatabaseServices;
using Bricscad.EditorInput;
using Teigha.Runtime;

#elif ARX_APP
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
#endif

namespace Plan2Ext.CalcArea
{
    public class Commands
    {
        #region log4net Initialization
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Convert.ToString((typeof(Commands))));
        static Commands()
        {
            if (log4net.LogManager.GetRepository(System.Reflection.Assembly.GetExecutingAssembly()).Configured == false)
            {
                log4net.Config.XmlConfigurator.ConfigureAndWatch(new System.IO.FileInfo(System.IO.Path.Combine(new System.IO.FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).DirectoryName, "_log4net.config")));
            }

        }
        #endregion
        [CommandMethod("Plan2CalcArea")]
        static public void Plan2CalcArea()
        {
            try
            {
                var pal = Plan2Ext.Flaeche.TheCalcAreaPalette;
                if (pal == null) return;

                Document doc = Application.DocumentManager.MdiActiveDocument;
                log.DebugFormat("Dokumentname: {0}.", doc.Name);
                if (doc == null) return;

                using (DocumentLock m_doclock = Application.DocumentManager.MdiActiveDocument.LockDocument())
                {
                    Editor ed = doc.Editor;
#if NEWSETFOCUS
                    Application.DocumentManager.MdiActiveDocument.Window.Focus();
#else
                    Autodesk.AutoCAD.Internal.Utils.SetFocusToDwgView(); // previous 2014 AutoCAD - Versions
#endif
                    Plan2Ext.Kleinbefehle.Layers.Plan2SaveLayerStatus();

                    Plan2Ext.Flaeche.AktFlaeche(Application.DocumentManager.MdiActiveDocument,
                        pal.RaumBlockName,
                        pal.AreaAttName,
                        pal.PeriAttName,
                        pal.LayerFg,
                        pal.LayerAg,
                        selectAll: false,
                        layerSchalt: pal.LayerSchaltung

                     );

                }

            }
            catch (System.Exception ex)
            {
                Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2CalcFlaCalVolumes aufgetreten! {0}", ex.Message));
            }
        }

        [CommandMethod("Plan2CalcAreaBulk", CommandFlags.Session)]
        static public void Plan2CalcAreaBulk()
        {
            try
            {
                log.Info("----------------------------------------------------------------------------------");
                log.Info("Plan2CalcAreaBulk");

                var dwgErrors = new Dictionary<string, List<Plan2Ext.Flaeche.AktFlaecheErrorType>>();
                var blockName = string.Empty;
                var attributeName = string.Empty;
                var fgLayer = string.Empty;
                var abzLayer = string.Empty;

                var doc = Application.DocumentManager.MdiActiveDocument;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                if (!GetAttributeInfos(ed, doc, out blockName, out attributeName, out fgLayer, out abzLayer)) return;

                // geht nicht während automated ablauf
                Plan2Ext.Flaeche.InitVariablesFromConfig();

                string dwgName = doc.Name;
                var dwgProposal = System.IO.Path.GetDirectoryName(dwgName);

                string dirName = string.Empty;
                string[] dwgFileNames = null;
                if (!Plan2Ext.Globs.GetMultipleFileNames("AutoCAD-Zeichnung", "Dwg", "Verzeichnis mit Zeichnungen für die Flächenberechnung", "Verzeichnis mit Zeichnungen für die Flächenberechnung", ref dwgFileNames, ref dirName, defaultPath: dwgProposal))
                {
                    return;
                }
                doc.CloseAndDiscard();

                List<string> saveFileNotPossible = new List<string>();
                foreach (var fileName in dwgFileNames)
                {
                    SetReadOnlyAttribute(fileName, false);

                    bool ok = true;

                    log.Info("----------------------------------------------------------------------------------");
                    log.Info(string.Format(CultureInfo.CurrentCulture, "Öffne Zeichnung {0}", fileName));

                    Application.DocumentManager.Open(fileName, false);
                    doc = Application.DocumentManager.MdiActiveDocument;
                    Database db = doc.Database;

                    // Lock the new document
                    using (DocumentLock acLckDoc = doc.LockDocument())
                    {
                        ed = Application.DocumentManager.MdiActiveDocument.Editor;
                        var layersState = Plan2Ext.Globs.SaveLayersState();
                        Plan2Ext.Globs.UnlockAllLayers();

                        // main part
                        Plan2Ext.Flaeche.Modify = true;
                        var errList = Plan2Ext.Flaeche.AktFlaeche(doc,blockName, attributeName, "", fgLayer, abzLayer, selectAll: true, layerSchalt: true, automated: true);
                        dwgErrors[fileName] = errList;
                        Plan2Ext.Flaeche.BereinigRegions(automated: true);
                        Plan2Ext.Globs.RestoreLayersState(layersState);
                    }

                    if (ok)
                    {
                        log.Info(string.Format(CultureInfo.CurrentCulture, "Zeichnung speichern und schließen: {0}", fileName));
                        try
                        {
                            doc.CloseAndSave(fileName);
                        }
                        catch (System.Exception ex)
                        {
                            log.Warn(string.Format(CultureInfo.CurrentCulture, "Zeichnung konnte nicht gespeichert werden! {0}. {1}", fileName, ex.Message));
                            saveFileNotPossible.Add(fileName);
                            doc.CloseAndDiscard();
                        }
                    }
                    else
                    {
                        log.Info(string.Format(CultureInfo.CurrentCulture, "Zeichnung schließen ohne zu speichern: {0}", fileName));
                        doc.CloseAndDiscard();
                    }
                }

                // write errors to excel
                var excelizer = new Excelizer();
                excelizer.ExcelExport(dwgErrors);

                if (saveFileNotPossible.Count > 0)
                {
                    var names = saveFileNotPossible.Select(x => System.IO.Path.GetFileName(x)).ToArray();
                    var allNames = string.Join(", ", names);

                    System.Windows.Forms.MessageBox.Show(string.Format("Folgende Zeichnungen konnen nicht geändert werden: {0}", allNames), "Plan2CalcAreaBulk");
                }
            }
            catch (System.Exception ex)
            {
                string msg = string.Format(CultureInfo.CurrentCulture, "{0}", ex.Message);
                System.Windows.Forms.MessageBox.Show(msg, "Plan2CalcAreaBulk");
            }
        }

        /// <summary>
        /// Sets the read only attribute.
        /// </summary>
        /// <param name="fullName">The full name.</param>
        /// <param name="readOnly">if set to <c>true</c> [read only].</param>
        private static void SetReadOnlyAttribute(string fullName, bool readOnly)
        {
            System.IO.FileInfo filePath = new System.IO.FileInfo(fullName);
            System.IO.FileAttributes attribute;
            if (readOnly)
                attribute = filePath.Attributes | System.IO.FileAttributes.ReadOnly;
            else
            {
                attribute = filePath.Attributes;
                attribute &= ~System.IO.FileAttributes.ReadOnly;
                //attribute = (System.IO.FileAttributes)(filePath.Attributes - System.IO.FileAttributes.ReadOnly);
            }

            System.IO.File.SetAttributes(filePath.FullName, attribute);
        }

        private static bool GetAttributeInfos(Editor ed, Document doc, out string blockName, out string attName, out string fgLayer, out string abzLayer)
        {
            blockName = string.Empty;
            attName = string.Empty;
            fgLayer = string.Empty;
            abzLayer = "$$$ABZUGLAYER$$$";
            PromptNestedEntityResult per = ed.GetNestedEntity("\nM2-Attribut wählen: ");
            if (per.Status == PromptStatus.OK)
            {
                using (var tr = doc.TransactionManager.StartTransaction())
                {
                    DBObject obj = tr.GetObject(per.ObjectId, OpenMode.ForRead);
                    AttributeReference ar = obj as AttributeReference;
                    if (ar != null && !ar.IsConstant)
                    {
                        BlockReference br = Plan2Ext.Globs.GetBlockFromItsSubentity(tr, per);
                        if (br != null)
                        {
                            ed.WriteMessage(string.Format(CultureInfo.CurrentCulture, "\nBlockname: {0}, Attributname: {1}.", Plan2Ext.Globs.GetBlockname(br, tr), ar.Tag));

                            var peo = new PromptEntityOptions("\nFlächengrenze: ");
                            peo.SetRejectMessage("\nDas gewählte Element ist keine Polylinie.");
                            peo.AddAllowedClass(typeof(Polyline), true);
                            peo.AddAllowedClass(typeof(Polyline2d), true);
                            peo.AddAllowedClass(typeof(Polyline3d), true);
                            PromptEntityResult res = ed.GetEntity(peo);
                            if (res.Status == PromptStatus.OK)
                            {
                                Entity ent = (Entity)tr.GetObject(res.ObjectId, OpenMode.ForRead);
                                blockName = Plan2Ext.Globs.GetBlockname(br, tr);
                                attName = ar.Tag;
                                fgLayer = ent.Layer;
                                return true;
                            }
                        }
                    }

                    tr.Commit();
                }
            }
            return false;
        }

        [CommandMethod("Plan2CalcFlaCalVolumes")]
        static public void Plan2CalcFlaCalVolumes()
        {
            try
            {
                var pal = Plan2Ext.Flaeche.TheCalcAreaPalette;
                if (pal == null) return;


                Document doc = Application.DocumentManager.MdiActiveDocument;
                log.DebugFormat("Dokumentname: {0}.", doc.Name);
                if (doc == null) return;

                using (DocumentLock m_doclock = Application.DocumentManager.MdiActiveDocument.LockDocument())
                {
                    DocumentCollection dm = Application.DocumentManager;
                    if (doc == null) return;
                    Editor ed = doc.Editor;


#if NEWSETFOCUS
                    Application.DocumentManager.MdiActiveDocument.Window.Focus();
#else
                    Autodesk.AutoCAD.Internal.Utils.SetFocusToDwgView(); // previous 2014 AutoCAD - Versions
#endif

                    Engine engine = new Engine();
                    engine.CalcVolume(pal.RaumBlockName, pal.AreaAttName, pal.HeightAttName, pal.VolAttName);

                }

            }
            catch (System.Exception ex)
            {
                Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2CalcFlaCalVolumes aufgetreten! {0}", ex.Message));
            }
        }

        [CommandMethod("Plan2FlaBereinig")]
        static public void Plan2FlaBereinig()
        {
            try
            {
                var pal = Plan2Ext.Flaeche.TheCalcAreaPalette;
                if (pal == null) return;


                Document doc = Application.DocumentManager.MdiActiveDocument;
                log.DebugFormat("Plan2FlaBereinig: Dokumentname: {0}.", doc.Name);
                if (doc == null) return;

                using (DocumentLock m_doclock = Application.DocumentManager.MdiActiveDocument.LockDocument())
                {
                    Plan2Ext.Flaeche.BereinigFehlerlinienAndRegions(automated: false);
                }
            }
            catch (System.Exception ex)
            {
                Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2FlaBereinig aufgetreten! {0}", ex.Message));
            }
        }

        [CommandMethod("Plan2CalcFlaSelVolAtts")]
        static public void Plan2CalcFlaSelVolAtts()
        {
            try
            {
                if (Plan2Ext.Flaeche.TheCalcAreaPalette == null) return;


                Document doc = Application.DocumentManager.MdiActiveDocument;
                log.DebugFormat("Dokumentname: {0}.", doc.Name);
                if (doc == null) return;

                using (DocumentLock m_doclock = Application.DocumentManager.MdiActiveDocument.LockDocument())
                {
                    DocumentCollection dm = Application.DocumentManager;
                    if (doc == null) return;
                    Editor ed = doc.Editor;


#if NEWSETFOCUS
                    Application.DocumentManager.MdiActiveDocument.Window.Focus();
#else
                Autodesk.AutoCAD.Internal.Utils.SetFocusToDwgView(); // previous 2014 AutoCAD - Versions
#endif

                    PromptEntityResult per = ed.GetNestedEntity("\nHöhenattribut wählen: ");
                    if (per.Status == PromptStatus.OK)
                    {
                        using (Transaction tr = doc.TransactionManager.StartTransaction())
                        {
                            DBObject obj = tr.GetObject(per.ObjectId, OpenMode.ForRead);
                            AttributeReference ar = obj as AttributeReference;
                            if (ar == null) return;

                            Plan2Ext.Flaeche.TheCalcAreaPalette.SetHeightAttribut(ar.Tag);

                            tr.Commit();

                        }
                    }
                    per = ed.GetNestedEntity("\nVolumsattribut wählen: ");
                    if (per.Status == PromptStatus.OK)
                    {
                        using (Transaction tr = doc.TransactionManager.StartTransaction())
                        {
                            DBObject obj = tr.GetObject(per.ObjectId, OpenMode.ForRead);
                            AttributeReference ar = obj as AttributeReference;
                            if (ar == null) return;

                            Plan2Ext.Flaeche.TheCalcAreaPalette.SetVolAttribut(ar.Tag);

                            tr.Commit();

                        }
                    }
                }

            }
            catch (System.Exception ex)
            {
                Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2CalcFlaSelVolAtts aufgetreten! {0}", ex.Message));
            }
        }

    }
}
