using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Interop;
using Autodesk.AutoCAD.Interop.Common;
using Autodesk.AutoCAD.Windows;

namespace Plan2Ext.CalcArea
{
  public class CalcAreaPalette
  {
    // We cannot derive from PaletteSet
    // so we contain it
    static PaletteSet ps;

    // We need to make the textbox available
    // via a static member
    static CalcAreaControl userControl;

    public CalcAreaPalette()
    {
      userControl = new CalcAreaControl();
    }

    public void Show()
    {
      if (ps == null)
      {
        ps = new PaletteSet("Flächenberechnung");
        ps.Style =
          PaletteSetStyles.NameEditable |
          PaletteSetStyles.ShowPropertiesMenu |
          PaletteSetStyles.ShowAutoHideButton |
          PaletteSetStyles.ShowCloseButton;
        ps.MinimumSize =
          new System.Drawing.Size(300, 300);
        ps.Add("CalcArea1", userControl);
        //ps.Add("Type Viewer 1", tvc);
      }
      ps.Visible = true;
    }

    public void SetObjectText(string text)
    {
      userControl.typeTextBox.Text = text;
    }


    internal void Show(string _RaumblockName, string _FlAttrib, string _FgLayer, string _AfLayer, Flaeche.AktFlaecheDelegate aktFlaecheDelegate)
    {
        if (ps == null)
        {
            ps = new PaletteSet("Flächenberechnung");
            ps.Style =
              PaletteSetStyles.NameEditable |
              PaletteSetStyles.ShowPropertiesMenu |
              PaletteSetStyles.ShowAutoHideButton |
              PaletteSetStyles.ShowCloseButton;
            ps.MinimumSize =
              new System.Drawing.Size(300, 300);
            ps.Add("CalcArea1", userControl);
            userControl.SetAktFlaecheDelegate(aktFlaecheDelegate);
            //ps.Add("Type Viewer 1", tvc);
        }
        userControl.txtBlockname.Text = _RaumblockName;
        userControl.txtAttribute.Text = _FlAttrib;
        userControl.txtFG.Text = _FgLayer;
        userControl.txtAG.Text = _AfLayer;
        

        ps.Visible = true;

    }

  }
}
