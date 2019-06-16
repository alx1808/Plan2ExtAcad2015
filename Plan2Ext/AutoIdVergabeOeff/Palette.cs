using System;
using Autodesk.AutoCAD.Windows;
// ReSharper disable IdentifierTypo
// ReSharper disable CommentTypo
// ReSharper disable StringLiteralTypo

namespace Plan2Ext.AutoIdVergabeOeff
{
    public interface IPalette
    {
        void SetInvisible();
        int FenNr { get; set; }
        string FenPrefix { get; }
        int TuerNr { get; set; }
        string TuerPrefix { get; }
        bool Show();
    }

    public class Palette : IPalette
    {
        // We cannot derive from PaletteSet
        // so we contain it
        static PaletteSet _Ps;

        // We need to make the textbox available
        // via a static member
        static MyUserControl _UserControl;

        public Palette()
        {
            _UserControl = new MyUserControl();
        }

        public void SetInvisible()
        {
            if (_Ps == null) return;
            if (_Ps.Visible) _Ps.Visible = false;
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

        public int TuerNr
        {
            get
            {
                int nr;
                if (!int.TryParse(_UserControl.txtTuerNummer.Text, out nr))
                {
                    throw new InvalidOperationException("Ungültige Türnummer " + _UserControl.txtTuerNummer.Text);
                }

                return nr;
            }
            set
            {
                _UserControl.txtTuerNummer.Text = value.ToString();
            }
        }


        public string TuerPrefix
        {
            get { return _UserControl.txtTuerPrefix.Text; }
        }

        public bool Show()
        {

            if (_Ps == null)
            {
                _Ps = new PaletteSet("AutoIdVergabeOeffnungen")
                {
                    Style = PaletteSetStyles.NameEditable |
                            PaletteSetStyles.ShowPropertiesMenu |
                            PaletteSetStyles.ShowAutoHideButton |
                            PaletteSetStyles.ShowCloseButton,
                    MinimumSize = new System.Drawing.Size(170, 164)
                };

                _Ps.Add("AutoIdVergabeOeffnungen", _UserControl);

                if (!_Ps.Visible)
                {
                    _Ps.Visible = true;
                }
#if ACAD2013_OR_NEWER
#if ARX_APP
                //ps.SetSize(new System.Drawing.Size(210, 164));
                Globs.SetPaletteDockSettings(_Ps);
#endif
#endif

                return false;
            }
            else
            {
                if (!_Ps.Visible)
                {
                    _Ps.Visible = true;
                    return false;
                }
                return true;

            }
        }

    }
}
