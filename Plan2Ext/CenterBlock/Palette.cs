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

namespace Plan2Ext.CenterBlock
{
    public class Palette
    {
        private Options _Options = null;
        internal Options Options
        {
            get { return _Options; }
        }

        // We cannot derive from PaletteSet
        // so we contain it
        static PaletteSet ps;

        // We need to make the textbox available
        // via a static member
        static CenterBlockControl userControl;

        public Palette()
        {
            _Options = new Options();
            userControl = new CenterBlockControl(_Options);
        }

        public void SetInvisible()
        {
            if (ps == null) return;
            if (ps.Visible) ps.Visible = false;
        }

        public bool Show()
        {

            if (ps == null)
            {
                ps = new PaletteSet("Blockzentrierung")
                {
                    Style = PaletteSetStyles.NameEditable |
                            PaletteSetStyles.ShowPropertiesMenu |
                            PaletteSetStyles.ShowAutoHideButton |
                            PaletteSetStyles.ShowCloseButton,
                    MinimumSize = new System.Drawing.Size(170, 164)
                };

                ps.Add("Blockzentrierung", userControl);

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
