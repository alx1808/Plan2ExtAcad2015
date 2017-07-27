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
using Bricscad.Runtime;
using Teigha.Runtime;

#elif ARX_APP
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
#endif

namespace Plan2Ext.Nummerierung
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

        [CommandMethod("Plan2NummerierungSelTop")]
        static public void Plan2NummerierungSelTop()
        {
            try
            {
                if (!OpenNrPalette()) return;

                var opts = Globs.TheNrOptions;
                Document doc = Application.DocumentManager.MdiActiveDocument;

                using (DocumentLock m_doclock = doc.LockDocument())
                {
                    if (doc == null) return;
                    Editor ed = doc.Editor;
#if NEWSETFOCUS
                    doc.Window.Focus();
#else
                    Autodesk.AutoCAD.Internal.Utils.SetFocusToDwgView(); // previous 2014 AutoCAD - Versions
#endif

                    PromptEntityResult per = ed.GetEntity("\nTop-Text wählen: ");
                    if (per.Status == PromptStatus.OK)
                    {
                        Transaction tr = doc.TransactionManager.StartTransaction();
                        using (tr)
                        {
                            DBObject obj = tr.GetObject(per.ObjectId, OpenMode.ForRead);
                            DBText txt = obj as DBText;
                            if (txt == null) return;

                            opts.SetTop("TOP" + txt.TextString);

                            tr.Commit();
                        }
                    }
                }

            }
            catch (System.Exception ex)
            {
                Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2NummerierungSelTop aufgetreten! {0}", ex.Message));
            }
        }


        [CommandMethod("Plan2NummerierungSelHBlock")]
        static public void Plan2NummerierungSelHBlock()
        {
            try
            {
                if (!OpenNrPalette()) return;

                var opts = Globs.TheNrOptions;
                Document doc = Application.DocumentManager.MdiActiveDocument;

                using (DocumentLock m_doclock = doc.LockDocument())
                {
                    DocumentCollection dm = Application.DocumentManager;
                    if (doc == null) return;
                    Editor ed = doc.Editor;
#if NEWSETFOCUS
                    doc.Window.Focus();
#else
                Autodesk.AutoCAD.Internal.Utils.SetFocusToDwgView(); // previous 2014 AutoCAD - Versions
#endif

                    PromptEntityResult per = ed.GetEntity("\nHöhenblock wählen: ");
                    if (per.Status == PromptStatus.OK)
                    {
                        Transaction tr = doc.TransactionManager.StartTransaction();
                        using (tr)
                        {
                            DBObject obj = tr.GetObject(per.ObjectId, OpenMode.ForRead);
                            BlockReference br = obj as BlockReference;
                            if (br == null) return;

                            opts.SetHBlockname(br.Name);

                            tr.Commit();
                        }
                    }
                }

            }
            catch (System.Exception ex)
            {
                Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2NummerierungSelHBlock aufgetreten! {0}", ex.Message));
            }
        }

        [CommandMethod("Plan2NummerierungSelBlockAndAtt")]
        static public void Plan2NummerierungSelBlockAndAtt()
        {
            try
            {
                if (!OpenNrPalette()) return;

                var opts = Globs.TheNrOptions;
                Document doc = Application.DocumentManager.MdiActiveDocument;

                using (DocumentLock m_doclock = doc.LockDocument())
                {
                    DocumentCollection dm = Application.DocumentManager;
                    if (doc == null) return;
                    Editor ed = doc.Editor;
#if NEWSETFOCUS
                    doc.Window.Focus();
#else
                Autodesk.AutoCAD.Internal.Utils.SetFocusToDwgView(); // previous 2014 AutoCAD - Versions
#endif

                    PromptEntityResult per = ed.GetEntity("\nRaumblock wählen: ");

                    if (per.Status == PromptStatus.OK)
                    {

                        Transaction tr = doc.TransactionManager.StartTransaction();
                        using (tr)
                        {
                            DBObject obj = tr.GetObject(per.ObjectId, OpenMode.ForRead);
                            BlockReference br = obj as BlockReference;
                            if (br == null) return;

                            opts.SetBlockname(br.Name);

                            tr.Commit();
                        }

                        per = ed.GetNestedEntity("\nAttribut wählen: ");

                        if (per.Status == PromptStatus.OK)
                        {

                            tr = doc.TransactionManager.StartTransaction();
                            using (tr)
                            {
                                DBObject obj = tr.GetObject(per.ObjectId, OpenMode.ForRead);
                                AttributeReference ar = obj as AttributeReference;
                                if (ar == null) return;

                                opts.SetAttribname(ar.Tag);

                                tr.Commit();
                            }
                        }
                    }
                }

            }
            catch (System.Exception ex)
            {
                Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2NummerierungSelBlockAndAtt aufgetreten! {0}", ex.Message));
            }
        }

        [CommandMethod("Plan2Nummerierung")]
        static public void Plan2Nummerierung()
        {
            try
            {
                if (!OpenNrPalette()) return;

                var opts = Globs.TheNrOptions;
                Document doc = Application.DocumentManager.MdiActiveDocument;

                using (DocumentLock m_doclock = doc.LockDocument())
                {

                    Engine _Engine = new Engine(opts);

                    while (_Engine.AddNumber()) { };
                }

            }
            catch (System.Exception ex)
            {
                Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2Nummerierung aufgetreten! {0}", ex.Message));
            }
        }




        static NrPalette _NrPalette;

        //[LispFunction("Plan2Nummerierung")]
        //public static object Plan2Nummerierung(ResultBuffer rb)
        //{
        //    log.Debug("--------------------------------------------------------------------------------");
        //    log.Debug("Plan2Nummerierung");
        //    try
        //    {
        //        Document doc = Application.DocumentManager.MdiActiveDocument;
        //        log.DebugFormat("Dokumentname: {0}.", doc.Name);

        //        if (_NrPalette == null)
        //        {
        //            _NrPalette = new NrPalette();
        //        }

        //        bool wasOpen = _NrPalette.Show();

        //        if (wasOpen)
        //        {
        //            return true;
        //        }
        //        else
        //            return false; // returns nil
        //    }
        //    catch (System.Exception ex)
        //    {
        //        Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2Nummerierung aufgetreten! {0}", ex.Message));
        //    }
        //    finally
        //    {
        //        //Free();
        //    }
        //    return false;
        //}

        private static bool OpenNrPalette()
        {
            if (_NrPalette == null)
            {
                _NrPalette = new NrPalette();
            }

            bool wasOpen = _NrPalette.Show();
            if (!wasOpen) return false;

            Document doc = Application.DocumentManager.MdiActiveDocument;
            if (doc == null) return false;
            log.DebugFormat("Dokumentname: {0}.", doc.Name);

            if (Globs.TheNrOptions == null) return false;
            else return true;
        }


    }
}
