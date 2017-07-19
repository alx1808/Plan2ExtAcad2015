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

namespace Plan2Ext.Tuer
{
    public class TuerOptionsPalette
    {
        private TuerOptions _TuerOptions = null;
        internal TuerOptions TuerOptions
        {
            get { return _TuerOptions; }
        }

        // We cannot derive from PaletteSet
        // so we contain it
        static PaletteSet ps;

        static TuerOptionsControl userControl;

        public TuerOptionsPalette()
        {
            _TuerOptions = new TuerOptions();
            userControl = new TuerOptionsControl(_TuerOptions);
        }

        internal void SetTuerOptions(TuerOptions TuerOptions)
        {
            _TuerOptions = TuerOptions;
            userControl.SetTuerOptions(TuerOptions);
        }

        public bool Show()
        {

            if (ps == null)
            {
                ps = new PaletteSet("Türoptionen");
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

                ps.Add("TuerOptions", userControl);

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
