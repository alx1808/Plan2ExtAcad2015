using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Windows;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

// ReSharper disable IdentifierTypo
// ReSharper disable StringLiteralTypo

namespace Plan2Ext.LayerKontrolle
{
    public class Palette
    {
        private static PaletteSet _PaletteSet;
        private static LayerKontrolleControl _Control;

        public Palette()
        {
            _Control = new LayerKontrolleControl();
            Application.DocumentManager.DocumentActivated += DocumentManager_DocumentActivated;
        }

        void DocumentManager_DocumentActivated(object sender, DocumentCollectionEventArgs e)
        {
            if (!_PaletteSet.Visible) return;
            if (e.Document == null) _Control.ClearLayerList();
            else _Control.InitLayers(ignoreSetLayers: true);
        }

        private string CurrentLayerName
        {
            get { return _Control.CurrentLayerName; }
        }

        public void Show()
        {
            if (_PaletteSet == null)
            {
                _PaletteSet = new PaletteSet("LayerKontrolle")
                {
                    Style = PaletteSetStyles.NameEditable |
                            PaletteSetStyles.ShowPropertiesMenu |
                            PaletteSetStyles.ShowAutoHideButton |
                            PaletteSetStyles.ShowCloseButton,
                    MinimumSize = new System.Drawing.Size(170, 164)
                };
#if ACAD2013_OR_NEWER
                _PaletteSet.SetSize(new System.Drawing.Size(210, 164));
#endif
                _PaletteSet.Add("LayerKontrolle", _Control);

                if (!_PaletteSet.Visible)
                {
                    _PaletteSet.Visible = true;
                }

            }
            else
            {
                if (!_PaletteSet.Visible)
                {
                    _PaletteSet.Visible = true;
                }
            }

        }

        internal void InitLayers(bool ignoreSetLayers)
        {
            _Control.InitLayers(ignoreSetLayers);
        }

        internal void SetLayers()
        {

            var doc = Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            using (var trans = doc.TransactionManager.StartTransaction())
            {
                var layTb = (LayerTable)trans.GetObject(db.LayerTableId, OpenMode.ForRead);
                foreach (var ltrOid in layTb)
                {
                    bool dontFreeze = (ltrOid == db.Clayer);
                    var ltr = (LayerTableRecord)trans.GetObject(ltrOid, OpenMode.ForRead);
                    var name = ltr.Name;
                    var on = IsAlwaysOn(name) || (name == CurrentLayerName);
                    SetLayer(ltr, !on, dontFreeze);
                }
                trans.Commit();
            }
        }

        private void SetLayer(LayerTableRecord ltr, bool off, bool dontFreeze)
        {
            if (!ltr.IsFrozen && ltr.IsOff == off) return;

            ltr.UpgradeOpen();
            if (!dontFreeze) ltr.IsFrozen = false;
            ltr.IsOff = off;
        }

        public void AddAlwaysOnLayer(string entityLayer)
        {
            _Control.AddAlwaysOnLayer(entityLayer);
        }

        private bool IsAlwaysOn(string name)
        {
            return _Control.IsAlwaysOn(name);
        }
    }
}
