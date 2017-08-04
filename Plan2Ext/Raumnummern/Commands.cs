
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
using Autodesk.AutoCAD.Internal;
#endif

namespace Plan2Ext.Raumnummern
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

        internal const string PFEILBLOCKNAME = "AL_TOP";
        internal const string TOPBLOCKNAME = "AL_TOP_NFL";
        internal const string PROPOTYP_DWG_NAME = "PROTO_50.dwg";
        internal const string TOPBLOCK_TOPNR_ATTNAME = "TOP";
        internal const string PFEILBLOCK_TOPNR_ATTNAME = "TOP";
        internal const string TOPBLOCK_M2_ATTNAME = "M2";

        [LispFunction("alx_F:ino_RaumnummernGetTopNr")]
        public static string LispRaumnummernGetTopNr(ResultBuffer rb)
        {
            if (!OpenRnPalette()) return "";
            else return Globs.TheRnOptions.TopNr;
        }

        [LispFunction("alx_F:ino_RaumnummernIncrementTopNr")]
        public static void LispRaumnummernIncrementTopNr(ResultBuffer rb)
        {
            if (!OpenRnPalette()) return;
            IncrementTopNr();
        }

        [CommandMethod("Plan2Raumnummern")]
        static public void Plan2Raumnummern()
        {
            try
            {
                if (!OpenRnPalette()) return;

                var opts = Globs.TheRnOptions;
                Document doc = Application.DocumentManager.MdiActiveDocument;

                using (DocumentLock m_doclock = doc.LockDocument())
                {
                    var blockOids = new List<ObjectId>();
                    Engine _Engine = new Engine(opts);

                    Plan2Ext.Globs.LayerOffRegex(new List<string> { "^X", "^A_BM_", "^A_BE_TXT", "^A_BE_HÖHE$" });
                    Plan2Ext.Globs.LayerOnAndThawRegex(new List<string> { "^" + opts.FlaechenGrenzeLayerName + "$", "^" + opts.AbzFlaechenGrenzeLayerName + "$" });

                    while (_Engine.AddNumber(blockOids)) { };

                    if (opts.UseHiddenAttribute)
                    {
                        _Engine.MoveNrToInfoAttribute(blockOids);
                    }
                }

            }
            catch (System.Exception ex)
            {
                Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2Raumnummern aufgetreten! {0}", ex.Message));
            }
        }

        [CommandMethod("Plan2RaumnummernSum")]
        static public void Plan2RaumnummernSum()
        {
            try
            {
                if (!OpenRnPalette()) return;

                var opts = Globs.TheRnOptions;
                Document doc = Application.DocumentManager.MdiActiveDocument;

                using (DocumentLock m_doclock = doc.LockDocument())
                {

                    Engine _Engine = new Engine(opts);
                    _Engine.SumTops();
                }
            }
            catch (System.Exception ex)
            {
                Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2RaumnummernSum aufgetreten! {0}", ex.Message));
            }
        }


#if !OLDER_THAN_2015
        [CommandMethod("Plan2RaumnummernInsertTop")]
        async public void Plan2RaumnummernInsertTop()
        {
            var oldAttReq = Application.GetSystemVariable("ATTREQ");
            var curLayer = Application.GetSystemVariable("CLAYER").ToString();
            try
            {
                if (!OpenRnPalette()) return;
                var opts = Globs.TheRnOptions;

                var doc = Application.DocumentManager.MdiActiveDocument;
                var ed = doc.Editor;

                var blockLayer = "A_RA_NUMMER";

                Plan2Ext.Globs.VerifyLayerExists(blockLayer, null);
                Plan2Ext.Globs.SetLayerCurrent(blockLayer);

                if (!Plan2Ext.Globs.BlockExists(PFEILBLOCKNAME))
                {
                    if (!Plan2Ext.Globs.InsertFromPrototype(PFEILBLOCKNAME, PROPOTYP_DWG_NAME))
                    {
                        ed.WriteMessage(string.Format(CultureInfo.CurrentCulture, "\nBlock '{0}' existiert nicht!", PFEILBLOCKNAME));
                        return;
                    }
                }
                if (!Plan2Ext.Globs.BlockExists(TOPBLOCKNAME))
                {
                    if (!Plan2Ext.Globs.InsertFromPrototype(TOPBLOCKNAME, PROPOTYP_DWG_NAME))
                    {
                        ed.WriteMessage(string.Format(CultureInfo.CurrentCulture, "\nBlock '{0}' existiert nicht!", TOPBLOCKNAME));
                        return;
                    }
                }

                var oidL = Utils.EntLast();
                Application.SetSystemVariable("ATTREQ", 0);
                bool userBreak = await Plan2Ext.Globs.CallCommandAsync("_.INSERT", PFEILBLOCKNAME, Editor.PauseToken, 1, 1, Editor.PauseToken);
                if (userBreak) return;
                var oid = Utils.EntLast();
                var topNr = opts.TopNr;
                SetTopNr(doc.Database, oid, topNr, "TOP");

                var vctrU = Plan2Ext.Globs.GetViewCtrW();
                //var vctrU = Plan2Ext.Globs.TransWcsUcs(vctr);
                ed.Command("_.INSERT", TOPBLOCKNAME, vctrU, 1, 1, 0.0);
                if (userBreak) return;
                oid = Utils.EntLast();
                SetTopBlockNr(doc.Database, oid, topNr, TOPBLOCK_TOPNR_ATTNAME);
                userBreak = await Plan2Ext.Globs.CallCommandAsync("_.MOVE", "_L", "", vctrU, Editor.PauseToken);
                IncrementTopNr();

            }
            catch (System.Exception ex)
            {
                Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2RaumnummernInsertTop aufgetreten! {0}", ex.Message));
            }
            finally
            {
                Application.SetSystemVariable("ATTREQ", oldAttReq);
                Plan2Ext.Globs.SetLayerCurrent(curLayer);
            }
        }

        private static bool SetTopBlockNr(Database db, ObjectId blockOid, string topNr, string attName)
        {
            bool ok = true;
            try
            {
                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    var blockRef = (BlockReference)tr.GetObject(blockOid, OpenMode.ForRead);

                    var atts = GetAttributEntities(blockRef, tr);
                    foreach (var att in atts)
                    {
                        if (att.Tag == attName)
                        {
                            att.UpgradeOpen();
                            att.TextString = "TOP " + topNr;
                        }
                    }
                    tr.Commit();
                }
            }
            catch (System.Exception ex)
            {
                log.Error(string.Format(CultureInfo.CurrentCulture, "Fehler beim Ändern der Attribute: {0}", ex.Message), ex);
                ok = false;

            }
            return ok;
        }
        private static bool SetTopNr(Database db, ObjectId blockOid, string topNr, string attName)
        {
            bool ok = true;
            try
            {
                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    var blockRef = (BlockReference)tr.GetObject(blockOid, OpenMode.ForRead);

                    var atts = GetAttributEntities(blockRef, tr);
                    foreach (var att in atts)
                    {
                        if (att.Tag == attName)
                        {
                            att.UpgradeOpen();
                            att.TextString = topNr;

                            var tok = Plan2Ext.Globs.IsTextAngleOk(att.Rotation, Plan2Ext.Globs.GetRadRotationTolerance());
                            if (!tok)
                            {
                                att.Rotation += Math.PI;
                            }
                        }
                    }
                    tr.Commit();
                }
            }
            catch (System.Exception ex)
            {
                log.Error(string.Format(CultureInfo.CurrentCulture, "Fehler beim Ändern der Attribute: {0}", ex.Message), ex);
                ok = false;

            }
            return ok;
        }

        public static List<AttributeReference> GetAttributEntities(BlockReference blockRef, Transaction tr)
        {
            var atts = new List<AttributeReference>();
            foreach (ObjectId attId in blockRef.AttributeCollection)
            {
                var anyAttRef = tr.GetObject(attId, OpenMode.ForRead) as AttributeReference;
                if (anyAttRef != null)
                {
                    atts.Add(anyAttRef);
                }
            }
            return atts;
        }

#endif

        private static void IncrementTopNr()
        {
            var topNr = Globs.TheRnOptions.TopNr;
            var newTopNr = IncrementNumberInString(topNr);
            Globs.TheRnOptions.SetTopNr(newTopNr);
        }

        private static string IncrementNumberInString(string s)
        {
            string prefix, suffix;
            int? i = Plan2Ext.Globs.GetFirstIntInString(s, out prefix, out suffix);
            if (i.HasValue)
            {
                int origIntLen = s.Length - (prefix.Length + suffix.Length);
                var incI = i.Value + 1;
                var iString = incI.ToString().PadLeft(origIntLen, '0');
                return prefix + iString + suffix;
            }
            else
            {
                return prefix + suffix;
            }
        }

        [CommandMethod("Plan2RaumnummerSelTop")]
        static public void Plan2RaumnummerSelTop()
        {
            try
            {
                if (!OpenRnPalette()) return;

                var opts = Globs.TheRnOptions;
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

                    PromptEntityResult per = ed.GetEntity("\nTopblock oder Pfeilblock wählen: ");
                    if (per.Status == PromptStatus.OK)
                    {
                        using (var tr = doc.TransactionManager.StartTransaction())
                        {
                            DBObject obj = tr.GetObject(per.ObjectId, OpenMode.ForRead);
                            var bref = obj as BlockReference;
                            if (bref != null)
                            {
                                var attribs = Plan2Ext.Globs.GetAttributes(bref);
                                if (bref.Name == PFEILBLOCKNAME)
                                {
                                    string tops;
                                    if (attribs.TryGetValue(PFEILBLOCK_TOPNR_ATTNAME, out tops))
                                    {
                                        opts.SetTop(tops);
                                    }
                                }
                                else if (bref.Name == TOPBLOCKNAME)
                                {
                                    string tops;
                                    if (attribs.TryGetValue(TOPBLOCK_TOPNR_ATTNAME, out tops))
                                    {
                                        if (tops.StartsWith(Engine.TOP_PREFIX, StringComparison.OrdinalIgnoreCase))
                                        {
                                            tops = tops.Remove(0, Engine.TOP_PREFIX.Length).Trim();
                                            opts.SetTop(tops);
                                        }
                                    }
                                }
                            }
                            tr.Commit();
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2MoveFbhWithNumber aufgetreten! {0}", ex.Message));
            }
        }

        [CommandMethod("Plan2RaumnummerSelHBlock")]
        static public void Plan2RaumnummerSelHBlock()
        {
            try
            {
                if (!OpenRnPalette()) return;

                var opts = Globs.TheRnOptions;
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
                Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2MoveFbhWithNumber aufgetreten! {0}", ex.Message));
            }
        }

        [CommandMethod("Plan2RaumnummerSelBlockAndAtt")]
        static public void Plan2RaumnummerSelBlockAndAtt()
        {
            try
            {
                if (!OpenRnPalette()) return;

                var opts = Globs.TheRnOptions;
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

                        per = ed.GetNestedEntity("\nNummer-Attribut wählen: ");
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
                        per = ed.GetNestedEntity("\nFlächen-Attribut wählen: ");
                        if (per.Status == PromptStatus.OK)
                        {
                            tr = doc.TransactionManager.StartTransaction();
                            using (tr)
                            {
                                DBObject obj = tr.GetObject(per.ObjectId, OpenMode.ForRead);
                                AttributeReference ar = obj as AttributeReference;
                                if (ar == null) return;

                                opts.SetFlaechenAttributName(ar.Tag);

                                tr.Commit();
                            }
                        }
                    }
                }

            }
            catch (System.Exception ex)
            {
                Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2MoveFbhWithNumber aufgetreten! {0}", ex.Message));
            }
        }

        [CommandMethod("Plan2RaumnummerSelFgLayer")]
        static public void Plan2RaumnummerSelFgLayer()
        {
            try
            {
                if (!OpenRnPalette()) return;

                var opts = Globs.TheRnOptions;
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

                    PromptEntityResult per = ed.GetEntity("\nFlächengrenze wählen: ");
                    if (per.Status == PromptStatus.OK)
                    {
                        Transaction tr = doc.TransactionManager.StartTransaction();
                        using (tr)
                        {
                            string layer = string.Empty;
                            DBObject obj = tr.GetObject(per.ObjectId, OpenMode.ForRead);
                            layer = GetPolylineLayer(obj);
                            if (string.IsNullOrEmpty(layer)) return;

                            if (string.Compare(opts.AbzFlaechenGrenzeLayerName, layer, StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Die Layer für Flächengrenze und Abzugsfläche müssen unterschiedlich sein."));
                                return;
                            }

                            opts.SetFlaechenGrenzeLayerName(layer);
                            tr.Commit();
                        }
                    }
                }

            }
            catch (System.Exception ex)
            {
                Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2RaumnummerSelFgLayer aufgetreten! {0}", ex.Message));
            }
        }

        [CommandMethod("Plan2RaumnummerSelAbzFgLayer")]
        static public void Plan2RaumnummerSelAbzFgLayer()
        {
            try
            {
                if (!OpenRnPalette()) return;

                var opts = Globs.TheRnOptions;
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

                    PromptEntityResult per = ed.GetEntity("\nFlächengrenze wählen: ");
                    if (per.Status == PromptStatus.OK)
                    {
                        Transaction tr = doc.TransactionManager.StartTransaction();
                        using (tr)
                        {
                            string layer = string.Empty;
                            DBObject obj = tr.GetObject(per.ObjectId, OpenMode.ForRead);
                            layer = GetPolylineLayer(obj);
                            if (string.IsNullOrEmpty(layer)) return;

                            if (string.Compare(opts.FlaechenGrenzeLayerName, layer, StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Die Layer für Flächengrenze und Abzugsfläche müssen unterschiedlich sein."));
                                return;
                            }

                            opts.SetAbzFlaechenGrenzeLayerName(layer);
                            tr.Commit();
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2RaumnummerSelAabzFgLayer aufgetreten! {0}", ex.Message));
            }
        }

        private static string GetPolylineLayer(DBObject obj)
        {
            Polyline pline = obj as Polyline;
            if (pline != null)
            {
                return pline.Layer;
            }
            else
            {
                Polyline2d pl = obj as Polyline2d;
                if (pl != null) return pl.Layer;

            }
            return string.Empty;
        }

        [CommandMethod("Plan2MoveFbhWithNumber")]
        static public void Plan2MoveFbhWithNumber()
        {
            string distVar = "alx_V:ino_rb_fbhYDistWithNr";

            Plan2MoveFb(distVar);
        }

        [CommandMethod("Plan2MoveFbhWithOutNumber")]
        static public void Plan2MoveFbhWithOutNumber()
        {
            string distVar = "alx_V:ino_rb_fbhYDistNoNr";

            Plan2MoveFb(distVar);
        }

        private static void Plan2MoveFb(string distVar)
        {
            try
            {
                if (!OpenRnPalette()) return;

                var opts = Globs.TheRnOptions;
                Document doc = Application.DocumentManager.MdiActiveDocument;

                using (DocumentLock m_doclock = doc.LockDocument())
                {

                    Engine _Engine = new Engine(opts);

                    string sDist = TheConfiguration.GetValueString(distVar);
                    double dist = double.Parse(sDist, CultureInfo.InvariantCulture);

                    _Engine.MoveFbh(0.0, dist);
                }

            }
            catch (System.Exception ex)
            {
                Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2MoveFbhWithNumber aufgetreten! {0}", ex.Message));
            }
        }

        [CommandMethod("Plan2RaumnummernRemoveRaum")]
        static public void Plan2RaumnummernRemoveRaum()
        {
            try
            {
                if (!OpenRnPalette()) return;

                var opts = Globs.TheRnOptions;
                Document doc = Application.DocumentManager.MdiActiveDocument;

                using (DocumentLock m_doclock = doc.LockDocument())
                {

                    Engine _Engine = new Engine(opts);

                    while (_Engine.RemoveRaum()) { };
                }

            }
            catch (System.Exception ex)
            {
                Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2RaumnummernRemoveRaum aufgetreten! {0}", ex.Message));
            }
        }

        [CommandMethod("Plan2RaumnummernCalcArea")]
        static public void Plan2RaumnummernCalcArea()
        {
            try
            {
                if (!OpenRnPalette()) return;

                var opts = Globs.TheRnOptions;
                Document doc = Application.DocumentManager.MdiActiveDocument;

                using (DocumentLock m_doclock = doc.LockDocument())
                {
                    Plan2Ext.Flaeche.Modify = true;
                    Plan2Ext.Flaeche.AktFlaeche(
                        Application.DocumentManager.MdiActiveDocument,
                        opts.Blockname, opts.FlaechenAttributName, opts.UmfangAttributName, opts.FlaechenGrenzeLayerName, opts.AbzFlaechenGrenzeLayerName
                        );
                }

            }
            catch (System.Exception ex)
            {
                Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2RaumnummernCalcArea aufgetreten! {0}", ex.Message));
            }
        }

        static RnPalette _RnPalette;

        [LispFunction("Plan2Raumnummern")]
        public static object Plan2Raumnummern(ResultBuffer rb)
        {
            log.Debug("--------------------------------------------------------------------------------");
            log.Debug("Plan2Raumnummern");
            try
            {
                Document doc = Application.DocumentManager.MdiActiveDocument;
                log.DebugFormat("Dokumentname: {0}.", doc.Name);

                if (_RnPalette == null)
                {
                    _RnPalette = new RnPalette();
                }

                bool wasOpen = _RnPalette.Show();

                if (wasOpen)
                {
                    return true;
                }
                else
                    return false; // returns nil
            }
            catch (System.Exception ex)
            {
                Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2Raumnummern aufgetreten! {0}", ex.Message));
            }
            finally
            {
                //Free();
            }
            return false;
        }

        private static bool OpenRnPalette()
        {
            if (_RnPalette == null)
            {
                _RnPalette = new RnPalette();
            }

            bool wasOpen = _RnPalette.Show();
            if (!wasOpen) return false;

            Document doc = Application.DocumentManager.MdiActiveDocument;
            if (doc == null) return false;
            log.DebugFormat("Dokumentname: {0}.", doc.Name);

            if (Globs.TheRnOptions == null) return false;
            else return true;
        }


    }
}
