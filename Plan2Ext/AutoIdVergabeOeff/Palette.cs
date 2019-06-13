using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Windows;
using Plan2Ext.RaumHoePruefung;

namespace Plan2Ext.AutoIdVergabeOeff
{
    public interface IPalette
    {
        void SetInvisible();
        int FenNr { get; set; }
        string FenPrefix { get; }
        bool Show();
    }

    public class Palette : IPalette
    {
        // We cannot derive from PaletteSet
        // so we contain it
        static PaletteSet ps;

        // We need to make the textbox available
        // via a static member
        static MyUserControl _UserControl;

        public Palette()
        {
            _UserControl = new MyUserControl();
        }

        public void SetInvisible()
        {
            if (ps == null) return;
            if (ps.Visible) ps.Visible = false;
        }

        public int FenNr
        {
            get
            {
                int nr;
                if (!int.TryParse(_UserControl.txtFenNummer.Text, out nr))
                {
                    throw new InvalidOperationException("Ungültige Fensternummer " + _UserControl.txtFenNummer.Text);
                }

                return nr;
            }
            set
            {
                _UserControl.txtFenNummer.Text = value.ToString();
            }
        }

        public string FenPrefix
        {
            get { return _UserControl.txtFenPrefix.Text; }
        }


        public bool Show()
        {

            if (ps == null)
            {
                ps = new PaletteSet("AutoIdVergabeOeffnungen")
                {
                    Style = PaletteSetStyles.NameEditable |
                            PaletteSetStyles.ShowPropertiesMenu |
                            PaletteSetStyles.ShowAutoHideButton |
                            PaletteSetStyles.ShowCloseButton,
                    MinimumSize = new System.Drawing.Size(170, 164)
                };

                ps.Add("AutoIdVergabeOeffnungen", _UserControl);

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
