using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

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

#endif


namespace Plan2Ext.CenterBlock
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

        static Palette _Palette;
        internal static Palette ThePalette { get { return _Palette; } }
        [_AcTrx.CommandMethod("Plan2CenterBlock")]
        public void Plan2CenterBlock()
        {
            try
            {
                if (!OpenPalette()) return;

                var opts = Globs.TheOptions;
                _AcAp.Document doc = _AcAp.Application.DocumentManager.MdiActiveDocument;

                using (_AcAp.DocumentLock m_doclock = doc.LockDocument())
                {
                    CenterRaumBlock crb = new CenterRaumBlock();
                    crb.DoIt(doc, opts.Blockname, opts.LayerName, opts.UseXRefs);

                }

            }
            catch (System.Exception ex)
            {
                _AcAp.Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2CenterBlock aufgetreten! {0}", ex.Message));
            }
        }

        [_AcTrx.CommandMethod("Plan2CenterBlockDelErrSyms")]
        public void Plan2CenterBlockDelErrSyms()
        {
            try
            {
                if (!OpenPalette()) return;

                var opts = Globs.TheOptions;
                _AcAp.Document doc = _AcAp.Application.DocumentManager.MdiActiveDocument;

                using (_AcAp.DocumentLock m_doclock = doc.LockDocument())
                {
                    CenterRaumBlock crb = new CenterRaumBlock();
                    crb.DelErrSyms(doc);

                }

            }
            catch (System.Exception ex)
            {
                _AcAp.Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2CenterBlockDelErrSyms aufgetreten! {0}", ex.Message));
            }
        }

        [_AcTrx.CommandMethod("Plan2CenterBlockSelBlock")]
        static public void Plan2CenterBlockSelBlock()
        {
            try
            {
                if (!OpenPalette()) return;

                var opts = Globs.TheOptions;
                _AcAp.Document doc = _AcAp.Application.DocumentManager.MdiActiveDocument;

                using (_AcAp.DocumentLock m_doclock = doc.LockDocument())
                {
                    _AcAp.DocumentCollection dm = _AcAp.Application.DocumentManager;
                    if (doc == null) return;
                    _AcEd.Editor ed = doc.Editor;
#if NEWSETFOCUS
                    doc.Window.Focus();
#else
                Autodesk.AutoCAD.Internal.Utils.SetFocusToDwgView(); // previous 2014 AutoCAD - Versions
#endif

                    _AcEd.PromptNestedEntityResult per = ed.GetNestedEntity("\nBlock wählen: ");

                    if (per.Status == _AcEd.PromptStatus.OK)
                    {

                        using (var tr = doc.TransactionManager.StartTransaction())
                        {
                            _AcDb.DBObject obj = tr.GetObject(per.ObjectId, _AcDb.OpenMode.ForRead);
                            _AcDb.BlockReference br = obj as _AcDb.BlockReference;
                            if (br == null)
                            {
                                br = Plan2Ext.Globs.GetBlockFromItsSubentity(tr, per);
                                if (br == null) return;
                            }

                            opts.SetBlockname(br.Name);

                            tr.Commit();
                        }
                    }
                }

            }
            catch (System.Exception ex)
            {
                _AcAp.Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2CenterBlockSelBlock aufgetreten! {0}", ex.Message));
            }
        }

        private static bool OpenPalette()
        {

            if (_Palette == null)
            {
                _Palette = new Palette();
            }

            bool wasOpen = _Palette.Show();
            if (!wasOpen) return false;

            _AcAp.Document doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            if (doc == null) return false;
            log.DebugFormat("Dokumentname: {0}.", doc.Name);

            if (Globs.TheOptions == null) return false;
            else return true;
        }
    }
}
