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

namespace Plan2Ext.HoehenPruefung
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

        static HoePrPalette _HoePrPalette;
        internal static HoePrPalette Palette { get { return _HoePrPalette; } }


        [LispFunction("Plan2HoePrFehlerLinienLayer")]
        public static string Plan2HoePrFehlerLinienLayer(ResultBuffer rb)
        {
            return Engine.FEHLER_LINE_LAYER;
        }


        [LispFunction("Plan2HoePrMarkAsOk")]
        public static bool Plan2HoePrMarkAsOk(ResultBuffer rb)
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            try
            {

                if (rb == null)
                {
                    ed.WriteMessage("\nAufruf: (Plan2HoePrMarkAsOk Entity YesNo)");
                    return false;
                }
                TypedValue[] values = rb.AsArray();
                if (values == null || values.Length < 2 || values[0].Value == null)
                {
                    ed.WriteMessage("\nAufruf: (Plan2HoePrMarkAsOk Entity YesNo)");
                    return false;
                }

                var val = values[0].Value;
                ObjectId oid = (ObjectId)val;


                var val2 = values[1].Value;
                bool markIt = (int)values[1].TypeCode == (int)LispDataType.T_atom;

                ResultBuffer resbuf = new ResultBuffer(new TypedValue((int)DxfCode.Bool, markIt));
                Globs.SetXrecord(oid, Engine.HOEPR_OK_DICT_NAME, resbuf);

                return true;

            }
            catch (System.Exception ex)
            {

                ed.WriteMessage(string.Format(CultureInfo.CurrentCulture, "\nFehler in Plan2HoePrMarkAsOk! {0}'", ex.Message));
            }
            return false;
        }


        [CommandMethod("Plan2HoehenPruefung")]
        static public void Plan2HoehenPruefung()
        {
            try
            {
                bool ignoreFirstCancel = false;
                ignoreFirstCancel = !OpenHoePrPalette();

                var opts = Globs.TheHoePrOptions;
                Document doc = Application.DocumentManager.MdiActiveDocument;

                using (DocumentLock m_doclock = doc.LockDocument())
                {

                    Engine engine = new Engine(opts);
                    engine.CheckFb(ignoreFirstCancel);
                }

            }
            catch (System.Exception ex)
            {
                Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2HoehenPruefung aufgetreten! {0}", ex.Message));
            }
        }

        [CommandMethod("Plan2HoePrResetCheck")]
        static public void Plan2HoePrResetCheck()
        {
            const int RTNORM = 5100;
            try
            {
                //if (!OpenHoePrPalette()) return;

                //var opts = Globs.TheHoePrOptions;
                Document doc = Application.DocumentManager.MdiActiveDocument;

                using (DocumentLock m_doclock = doc.LockDocument())
                {
                    using (var rb = new ResultBuffer(new TypedValue((int)LispDataType.Text, "c:Plan2HoePrReset")))
                    {
                        int stat = 0;
                        ResultBuffer res = CADDZone.AutoCAD.Samples.AcedInvokeSample.InvokeLisp(rb, ref stat);
                        if (stat == RTNORM && res != null)
                        {
                            res.Dispose();
                        }
                    }

                }

            }
            catch (System.Exception ex)
            {
                Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2HoePrResetCheck aufgetreten! {0}", ex.Message));
            }
        }

        [CommandMethod("Plan2HoePrSelHkBlockAndAtt")]
        static public void Plan2HoePrSelHkBlockAndAtt()
        {
            try
            {
                if (!OpenHoePrPalette()) return;

                var opts = Globs.TheHoePrOptions;
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

                    PromptNestedEntityResult per = ed.GetNestedEntity("\nHöhenkotenblock wählen: ");

                    if (per.Status == PromptStatus.OK)
                    {

                        using (var tr = doc.TransactionManager.StartTransaction())
                        {
                            DBObject obj = tr.GetObject(per.ObjectId, OpenMode.ForRead);
                            BlockReference br = obj as BlockReference;
                            if (br == null)
                            {
                                br = Plan2Ext.Globs.GetBlockFromItsSubentity(tr, per);
                                if (br == null) return;
                            }

                            opts.SetHKBlockname(br.Name);

                            tr.Commit();
                        }

                        per = ed.GetNestedEntity("\nHöhen-Attribut wählen: ");
                        if (per.Status != PromptStatus.OK) return;
                        using (var tr = doc.TransactionManager.StartTransaction())
                        {
                            DBObject obj = tr.GetObject(per.ObjectId, OpenMode.ForRead);
                            AttributeReference ar = obj as AttributeReference;
                            if (ar == null) return;

                            opts.SetHoehenAtt(ar.Tag);

                            tr.Commit();
                        }


                    }
                }

            }
            catch (System.Exception ex)
            {
                Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2HoePrSelHkBlockAndAtt aufgetreten! {0}", ex.Message));
            }
        }

        [CommandMethod("Plan2HoePrCheckFb")]
        static public void Plan2HoePrCheckFb()
        {
            try
            {
                if (!OpenHoePrPalette()) return;

                var opts = Globs.TheHoePrOptions;
                Document doc = Application.DocumentManager.MdiActiveDocument;

                using (DocumentLock m_doclock = doc.LockDocument())
                {

                    Engine engine = new Engine(opts);
                    engine.CheckFb();
                }

            }
            catch (System.Exception ex)
            {
                log.Error(ex.Message, ex);
                Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2HoePrCheckFb aufgetreten! {0}", ex.Message));
            }
        }

        [CommandMethod("Plan2HoePrSelPolygonLayer")]
        static public void Plan2HoePrSelPolygonLayer()
        {
            try
            {
                if (!OpenHoePrPalette()) return;

                var opts = Globs.TheHoePrOptions;
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

                    PromptNestedEntityOptions peo = new PromptNestedEntityOptions("\nPolylinie wählen: ");
                    //peo.SetRejectMessage("\nDas gewählte Element ist keine Polylinie.");
                    //peo.AddAllowedClass(typeof(Polyline), true);
                    //peo.AddAllowedClass(typeof(Polyline2d), true);
                    //peo.AddAllowedClass(typeof(Polyline3d), true);

                    PromptEntityResult per = ed.GetNestedEntity(peo);

                    if (per.Status == PromptStatus.OK)
                    {

                        using (var tr = doc.TransactionManager.StartTransaction())
                        {
                            DBObject obj = tr.GetObject(per.ObjectId, OpenMode.ForRead);
                            Entity ent = obj as Entity;
                            if (ent != null)
                            {
                                if (ent is Polyline || ent is Polyline2d || ent is Polyline3d)
                                {
                                    opts.SetPolygonLayer(ent.Layer);
                                }
                                else
                                {
                                    ed.WriteMessage("\nDas gewählte Element ist keine Polylinie.");
                                }
                            }

                            tr.Commit();
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2HoePrSelPolygonLayer aufgetreten! {0}", ex.Message));
            }
        }

        private static bool OpenHoePrPalette()
        {
            // raumhöhenprüfung schließen
            if (Plan2Ext.RaumHoePruefung.Commands.Palette != null)
            {
                Plan2Ext.RaumHoePruefung.Commands.Palette.SetInvisible();
            }

            if (_HoePrPalette == null)
            {
                _HoePrPalette = new HoePrPalette();
            }

            bool wasOpen = _HoePrPalette.Show();
            if (!wasOpen) return false;

            Document doc = Application.DocumentManager.MdiActiveDocument;
            if (doc == null) return false;
            log.DebugFormat("Dokumentname: {0}.", doc.Name);

            if (Globs.TheHoePrOptions == null) return false;
            else return true;
        }

    }
}
