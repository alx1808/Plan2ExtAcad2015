using Autodesk.AutoCAD.Windows;
// ReSharper disable StringLiteralTypo

// ReSharper disable once IdentifierTypo
namespace Plan2Ext.LayerNummerierung
{
    public class NrPalette
    {
        // We cannot derive from PaletteSet
        // so we contain it
        static PaletteSet _ps;

        static NrControl _userControl;

        public NrPalette()
        {
            var nrOptions = new NrOptions();
            _userControl = new NrControl(nrOptions);
        }

        public bool Show()
        {

            if (_ps == null)
            {
                _ps = new PaletteSet("LayerNummerierung")
                {
                    Style = PaletteSetStyles.NameEditable |
                            PaletteSetStyles.ShowPropertiesMenu |
                            PaletteSetStyles.ShowAutoHideButton |
                            PaletteSetStyles.ShowCloseButton,
                    MinimumSize = new System.Drawing.Size(170, 164)
                };
#if ACAD2013_OR_NEWER
#if ARX_APP
                _ps.SetSize(new System.Drawing.Size(210, 164));
#endif
#endif

                _ps.Add("LayerNummerierung", _userControl);

                if (!_ps.Visible)
                {
                    _ps.Visible = true;
                }

                return false;
            }
            else
            {
                if (!_ps.Visible)
                {
                    _ps.Visible = true;
                    return false;
                }
                return true;

            }
        }
    }
}
