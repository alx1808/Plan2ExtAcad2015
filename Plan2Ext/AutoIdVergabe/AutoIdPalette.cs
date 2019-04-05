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

namespace Plan2Ext.AutoIdVergabe
{
    public class AutoIdPalette
    {

        private AutoIdOptions _AutoIdOptions = null;
        internal AutoIdOptions AutoIdOptions
        {
            get { return _AutoIdOptions; }
        }

        // We cannot derive from PaletteSet
        // so we contain it
        static PaletteSet ps;

        // We need to make the textbox available
        // via a static member
        static AutoIdControl userControl;

        public AutoIdPalette()
        {
            _AutoIdOptions = new AutoIdOptions();
            userControl = new AutoIdControl(_AutoIdOptions);
        }

        public bool Show()
        {

            if (ps == null)
            {
                ps = new PaletteSet("Auto-Id-Vergabe")
                {
                    Style = PaletteSetStyles.NameEditable |
                            PaletteSetStyles.ShowPropertiesMenu |
                            PaletteSetStyles.ShowAutoHideButton |
                            PaletteSetStyles.ShowCloseButton,
                    MinimumSize = new System.Drawing.Size(170, 164)
                };

                ps.Add("AutoIdVergabe", userControl);

                if (!ps.Visible)
                {
                    ps.Visible = true;
                }
#if ACAD2013_OR_NEWER
#if ARX_APP
                // moved here since bug in acad 2016 and 2017 (must be visible before setting dock)
                // https://forums.autodesk.com/t5/net/paletteset-docking-via-c-not-working-in-autocad-2016/td-p/5568372
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

        internal void SetLvZuweisungen()
        {
            userControl.SetLvZuweisungen();
        }
    }
}
