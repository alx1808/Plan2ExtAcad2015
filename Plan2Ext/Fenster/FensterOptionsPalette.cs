//using Autodesk.AutoCAD.Windows;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if BRX_APP
using Bricscad.Windows;
#elif ARX_APP
  using Autodesk.AutoCAD.Windows;
#endif


namespace Plan2Ext.Fenster
{
    public class FensterOptionsPalette
    {

        private FensterOptions _FensterOptions = null;
        internal FensterOptions FensterOptions
        {
            get { return _FensterOptions; }
        }

        // We cannot derive from PaletteSet
        // so we contain it
        static PaletteSet ps;

        // We need to make the textbox available
        // via a static member
        static FensterOptionsControl userControl;

        public FensterOptionsPalette()
        {
            _FensterOptions = new FensterOptions();
            userControl = new FensterOptionsControl(_FensterOptions );
        }

        internal void SetFensterOptions(FensterOptions fensterOptions)
        {
            _FensterOptions = fensterOptions;
            userControl.SetFensterOptions(fensterOptions );
        }


        public bool Show()
        {

            if (ps == null)
            {
                ps = new PaletteSet("Fensteroptionen")
                {
                    Style = PaletteSetStyles.NameEditable |
                            PaletteSetStyles.ShowPropertiesMenu |
                            PaletteSetStyles.ShowAutoHideButton |
                            PaletteSetStyles.ShowCloseButton,
                    MinimumSize = new System.Drawing.Size(170, 164)
                };

                ps.Add("FensterOptions", userControl);
                //ps.Add("Type Viewer 1", tvc);

                if (!ps.Visible)
                {
                    ps.Visible = true;
                }
#if ACAD2013_OR_NEWER
#if ARX_APP
                //ps.SetSize(new System.Drawing.Size(210, 164));
                ps.Visible = true;
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



        internal void Show(FensterOptions opts)
        {
            this.Show();
        }

        //private void SetOpts(FensterOptions opts)
        //{
        //    if (userControl == null) return;
        //    userControl.txtWidth.Text = opts.BreiteString;
        //    userControl.txtHeight.Text = opts.HoeheString;
        //    userControl.txtParapet.Text = opts.ParapetString;
        //    userControl.txtOlAb.Text = opts.OlAbString;
        //    userControl.txtStaerke.Text = opts.StaerkeString;
        //    userControl.txtStock.Text = opts.StockString;
        //}

        public double Breite { get { return _FensterOptions.Breite; } }
    }
}
