using Autodesk.AutoCAD.Windows;
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
        }

        public bool Show()
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

                return false;
            }
            else
            {
                if (!_PaletteSet.Visible)
                {
                    _PaletteSet.Visible = true;
                    return false;
                }
                return true;

            }
        }

        public bool IsAlwaysOn(string name)
        {
            // todo: get layers always on from control
            return false;
        }

        public bool IsCurrentLayer(string name)
        {
            // todo: get currentActiveLayer from control
            return false;
        }

    }
}
