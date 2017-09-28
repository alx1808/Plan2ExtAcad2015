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

//            try
//            {

//                Document doc = Application.DocumentManager.MdiActiveDocument;

//                using (DocumentLock m_doclock = doc.LockDocument())
//                {

//#if NEWSETFOCUS
//                    Application.DocumentManager.MdiActiveDocument.Window.Focus();
//#else
//                Autodesk.AutoCAD.Internal.Utils.SetFocusToDwgView(); // previous 2014 AutoCAD - Versions
//#endif

//                }

//            }
//            catch (System.Exception ex)
//            {
//                log.Error(ex.Message, ex);
//                Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2HoePrCalcArea aufgetreten! {0}", ex.Message));
//            }
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
                    Plan2Ext.Flaeche.BereinigFehlerlinienAndRegions();
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
