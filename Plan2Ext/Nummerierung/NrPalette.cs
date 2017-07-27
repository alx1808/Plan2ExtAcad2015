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

namespace Plan2Ext.Nummerierung
{
    public class NrPalette
    {

        private NrOptions _NrOptions = null;
        internal NrOptions NrOptions
        {
            get { return _NrOptions; }
        }

        // We cannot derive from PaletteSet
        // so we contain it
        static PaletteSet ps;

        // We need to make the textbox available
        // via a static member
        static NrControl userControl;

        public NrPalette()
        {
            _NrOptions = new NrOptions();
            userControl = new NrControl(_NrOptions) { SelectAllOnFocus = true };
        }

        public bool Show()
        {

            if (ps == null)
            {
                ps = new PaletteSet("Nummerierung");
                ps.Style =
                  PaletteSetStyles.NameEditable |
                  PaletteSetStyles.ShowPropertiesMenu |
                  PaletteSetStyles.ShowAutoHideButton |
                  PaletteSetStyles.ShowCloseButton;
                ps.MinimumSize =
                  new System.Drawing.Size(170, 164);
#if ACAD2013_OR_NEWER
#if ARX_APP
                ps.SetSize(new System.Drawing.Size(210, 164));
#endif
#endif

                ps.Add("Nummerierung", userControl);

                if (!ps.Visible)
                {
                    ps.Visible = true;
                }

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
