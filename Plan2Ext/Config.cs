using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using Autodesk.AutoCAD.Runtime;
//using Autodesk.AutoCAD.DatabaseServices;
//using Autodesk.AutoCAD.ApplicationServices;
using System.Globalization;
using Plan2Ext.Configuration;

#if BRX_APP
using _AcAp = Bricscad.ApplicationServices;
//using _AcBr = Teigha.BoundaryRepresentation;
using _AcCm = Teigha.Colors;
using _AcDb = Teigha.DatabaseServices;
using _AcEd = Bricscad.EditorInput;
using _AcGe = Teigha.Geometry;
using _AcGi = Teigha.GraphicsInterface;
using _AcGs = Teigha.GraphicsSystem;
using _AcPl = Bricscad.PlottingServices;
using _AcBrx = Bricscad.Runtime;
using _AcTrx = Teigha.Runtime;
using _AcWnd = Bricscad.Windows;
#elif ARX_APP
  using _AcAp = Autodesk.AutoCAD.ApplicationServices;
  using _AcBr = Autodesk.AutoCAD.BoundaryRepresentation;
  using _AcCm = Autodesk.AutoCAD.Colors;
  using _AcDb = Autodesk.AutoCAD.DatabaseServices;
  using _AcEd = Autodesk.AutoCAD.EditorInput;
  using _AcGe = Autodesk.AutoCAD.Geometry;
  using _AcGi = Autodesk.AutoCAD.GraphicsInterface;
  using _AcGs = Autodesk.AutoCAD.GraphicsSystem;
  using _AcPl = Autodesk.AutoCAD.PlottingServices;
  using _AcBrx = Autodesk.AutoCAD.Runtime;
  using _AcTrx = Autodesk.AutoCAD.Runtime;
  using _AcWnd = Autodesk.AutoCAD.Windows;
#endif


namespace Plan2Ext
{
    public class Config
    {

        #region log4net Initialization
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Convert.ToString((typeof(Config))));
        static Config()
        {
            if (log4net.LogManager.GetRepository(System.Reflection.Assembly.GetExecutingAssembly()).Configured == false)
            {
                log4net.Config.XmlConfigurator.ConfigureAndWatch(new System.IO.FileInfo(System.IO.Path.Combine(new System.IO.FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).DirectoryName, "_log4net.config")));
            }

        }
        #endregion

        #region Member Variables
        private static string _FileName = "";
        #endregion

        [_AcTrx.LispFunction("NetSetPlan2Config")]
        public static _AcDb.ResultBuffer NetSetPlan2Config(_AcDb.ResultBuffer rb)
        {
            try
            {
                TheConfiguration.Loaded = false;

                List<string> existingConfigs = new List<string>();
                string current = "";
                if (!GetArgs2(rb, ref current, existingConfigs))
                {
                    log.Error("No valid configlist!");

                }

                using (SetConfigForm frm = new SetConfigForm(current, existingConfigs))
                {
                    System.Windows.Forms.DialogResult res = _AcAp.Application.ShowModalDialog(frm);
                    if (res == System.Windows.Forms.DialogResult.Cancel)
                    {
                        return null;
                    }
                    else
                    {
                        return new _AcDb.ResultBuffer(new _AcDb.TypedValue((int)_AcBrx.LispDataType.Text, frm.Configuration));
                    }

                }

            }
            catch (System.Exception ex)
            {
                _AcAp.Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in NetSetPlan2Config aufgetreten!\n{0}", ex.Message));

            }
            return null;
        }

        private static bool GetArgs2(_AcDb.ResultBuffer rb, ref string current, List<string> configs)
        {
            if (rb == null) return false;
            _AcDb.TypedValue[] values = rb.AsArray();
            if (values.Length <= 2) return false;

            current = values[1].Value.ToString();

            for (int i = 2; i < (values.Length - 1); i++)
            {
                var v = values[i];
                configs.Add(v.Value.ToString());
            }

            return true;

        }


        [_AcTrx.LispFunction("ConfigPlan2")]
        public static _AcDb.ResultBuffer ConfigPlan2(_AcDb.ResultBuffer rb)
        {
            try
            {
                TheConfiguration.Loaded = false;

                GetArgs(rb);

                using (ConfigForm frm = new ConfigForm(_FileName))
                {
                    _AcAp.Application.ShowModalDialog(frm);
                }

            }
            catch (System.Exception ex)
            {
                _AcAp.Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in ConfigPlan2 aufgetreten!\n{0}", ex.Message));

            }
            return null;
        }

        private static void GetArgs(_AcDb.ResultBuffer rb)
        {
            _AcDb.TypedValue[] values = rb.AsArray();
            _FileName = values[0].Value.ToString();
        }

    }



}
