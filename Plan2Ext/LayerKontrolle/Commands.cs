using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using Plan2Ext.CenterBlock;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
// ReSharper disable StringLiteralTypo
// ReSharper disable IdentifierTypo

namespace Plan2Ext.LayerKontrolle
{
    public class Commands
    {
        #region log4net Initialization
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Convert.ToString((typeof(Commands))));
        #endregion

        static Palette _Palette;

        [CommandMethod("Plan2LayerKontrolle")]
        public void Plan2LayerKontrolle()
        {
            try
            {
                OpenPalette();

                var doc = Application.DocumentManager.MdiActiveDocument;
                if (doc == null) return;

                using (var m_doclock = doc.LockDocument())
                {
                    SetLayers();
                }

            }
            catch (System.Exception ex)
            {
                Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2CenterBlock aufgetreten! {0}", ex.Message));
            }
        }

        private void SetLayers()
        {

            var doc = Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            using (var trans = doc.TransactionManager.StartTransaction())
            {
                var layTb = (LayerTable)trans.GetObject(db.LayerTableId, OpenMode.ForRead);
                foreach (var ltrOid in layTb)
                {
                    var ltr = (LayerTableRecord)trans.GetObject(ltrOid, OpenMode.ForRead);
                    var name = ltr.Name;
                    var on = _Palette.IsCurrentLayer(name) || _Palette.IsAlwaysOn(name);
                    SetLayer(ltr, !on);
                }
                trans.Commit();
            }
        }

        private void SetLayer(LayerTableRecord ltr, bool off)
        {
            if (!ltr.IsFrozen && ltr.IsOff == off) return;

            ltr.UpgradeOpen();
            ltr.IsFrozen = false;
            ltr.IsOff = off;
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
