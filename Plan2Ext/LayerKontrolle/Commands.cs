using System.Globalization;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
// ReSharper disable StringLiteralTypo
// ReSharper disable IdentifierTypo

namespace Plan2Ext.LayerKontrolle
{
    // ReSharper disable once UnusedMember.Global
    public class Commands
    {
        //#region log4net Initialization
        //private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Convert.ToString((typeof(Commands))));
        //#endregion

        static Palette _Palette;

        [CommandMethod("Plan2LayerKontrolle")]
        // ReSharper disable once UnusedMember.Global
        public void Plan2LayerKontrolle()
        {
            HandleSetLayers(true);
        }
        [CommandMethod("Plan2LayerKontrolleSetLayers")]
        // ReSharper disable once UnusedMember.Global
        public void Plan2LayerKontrolleSetLayers()
        {
            HandleSetLayers(false);
        }

        private void HandleSetLayers(bool initLayers)
        {
            try
            {
                OpenPalette();
                if (initLayers) _Palette.InitLayers();

                var doc = Application.DocumentManager.MdiActiveDocument;
                if (doc == null) return;

                // ReSharper disable once UnusedVariable
                using (var mDoclock = doc.LockDocument())
                {

#if NEWSETFOCUS
                    doc.Window.Focus();
#else
                Autodesk.AutoCAD.Internal.Utils.SetFocusToDwgView(); // previous 2014 AutoCAD - Versions
#endif

                    _Palette.SetLayers();
                }
            }
            catch (System.Exception ex)
            {
                Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture,
                    "Fehler in Plan2LayerKontrolle aufgetreten! {0}", ex.Message));
            }
        }

        [CommandMethod("Plan2LayerKontrolleSelectAlwaysOnLayer")]
        // ReSharper disable once UnusedMember.Global
        public static void Plan2LayerKontrolleSelectAlwaysOnLayer()
        {
            try
            {
                OpenPalette();

                Document doc = Application.DocumentManager.MdiActiveDocument;

                // ReSharper disable once UnusedVariable
                using (var mDoclock = doc.LockDocument())
                {
                    var ed = doc.Editor;
#if NEWSETFOCUS
                    doc.Window.Focus();
#else
                    Autodesk.AutoCAD.Internal.Utils.SetFocusToDwgView(); // previous 2014 AutoCAD - Versions
#endif

                    var per = ed.GetEntity("\nElement wählen, dessen Layer immer angezeigt werden soll: ");
                    if (per.Status == PromptStatus.OK)
                    {
                        using (var tr = doc.TransactionManager.StartTransaction())
                        {
                            Entity entity = tr.GetObject(per.ObjectId, OpenMode.ForRead) as Entity;
                            if (entity == null) return;
                            _Palette.AddAlwaysOnLayer(entity.Layer);
                            tr.Commit();
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2LayerKontrolleSelectAlwaysOnLayer aufgetreten! {0}", ex.Message));
            }
        }


        private static void OpenPalette()
        {

            if (_Palette == null)
            {
                _Palette = new Palette();
            }

            _Palette.Show();
        }
    }
}
