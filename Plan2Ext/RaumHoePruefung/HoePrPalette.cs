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

namespace Plan2Ext.RaumHoePruefung
{
    public class HoePrPalette
    {
        private HoePrOptions _HoePrOptions = null;
        internal HoePrOptions HoePrOptions
        {
            get { return _HoePrOptions; }
        }

        // We cannot derive from PaletteSet
        // so we contain it
        static PaletteSet ps;

        // We need to make the textbox available
        // via a static member
        static HoePrControl userControl;

        public HoePrPalette()
        {
            _HoePrOptions = new HoePrOptions();
            userControl = new HoePrControl(_HoePrOptions);
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
                ps = new PaletteSet("Raumhöhenprüfung");
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

                ps.Add("RaumhoehenPruefung", userControl);

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
