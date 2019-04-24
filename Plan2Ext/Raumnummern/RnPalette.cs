//using Autodesk.AutoCAD.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if BRX_APP
using Bricscad.Windows;
#elif ARX_APP
  using Autodesk.AutoCAD.Windows;
#endif

namespace Plan2Ext.Raumnummern
{
    public class RnPalette
    {

        private RnOptions _RnOptions = null;
        internal RnOptions RnOptions
        {
            get { return _RnOptions; }
        }

        // We cannot derive from PaletteSet
        // so we contain it
        static PaletteSet ps;

        // We need to make the textbox available
        // via a static member
        static RnControl userControl;

        public RnPalette()
        {
            _RnOptions = new RnOptions();
            userControl = new RnControl(_RnOptions) { SelectAllOnFocus = true };
        }

        public bool Show()
        {

            if (ps == null)
            {
                ps = new PaletteSet("Raumnummern")
                {
                    Style = PaletteSetStyles.NameEditable |
                            PaletteSetStyles.ShowPropertiesMenu |
                            PaletteSetStyles.ShowAutoHideButton |
                            PaletteSetStyles.ShowCloseButton,
                    MinimumSize = new System.Drawing.Size(170, 164)
                };

                ps.Add("Raumnummern", userControl);

                if (!ps.Visible)
                {
                    ps.Visible = true;
                }
#if ACAD2013_OR_NEWER
#if ARX_APP
                //ps.SetSize(new System.Drawing.Size(210, 164));
                Plan2Ext.Globs.SetPaletteDockSettings(ps);
#endif
#endif

                return false;
            }
            else
            {
                if (!ps.Visible)
                {
                    ps.Visible = true;
                    return false;
                }
                return true;

            }
        }
    }
}
