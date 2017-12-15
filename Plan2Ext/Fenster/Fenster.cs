//using Autodesk.AutoCAD.ApplicationServices;
//using Autodesk.AutoCAD.DatabaseServices;
//using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if BRX_APP
using Bricscad.ApplicationServices;
using Teigha.DatabaseServices;
using Teigha.Runtime;

#elif ARX_APP
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
#endif


namespace Plan2Ext.Fenster
{
    public class Fenster
    {
        #region log4net Initialization
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Convert.ToString((typeof(Fenster))));
        static Fenster()
        {
            if (log4net.LogManager.GetRepository(System.Reflection.Assembly.GetExecutingAssembly()).Configured == false)
            {
                log4net.Config.XmlConfigurator.ConfigureAndWatch(new System.IO.FileInfo(System.IO.Path.Combine(new System.IO.FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).DirectoryName, "_log4net.config")));
            }

        }
        #endregion

        static FensterOptionsPalette _FensterOptionsPalette;

        [CommandMethod("Plan2CheckFenWidth")]
        public static void Plan2CheckFenWidth()
        {
            var examiner = new Examiner();
            examiner.CheckWindowWidth();
        }

        [LispFunction("DotNetFensterOptions")]
        public static object DotNetFensterOptions(ResultBuffer rb)
        {
            log.Debug("--------------------------------------------------------------------------------");
            log.Debug("DotNetFensterOptions");
            try
            {
                Document doc = Application.DocumentManager.MdiActiveDocument;
                log.DebugFormat("Dokumentname: {0}.", doc.Name);

                if (_FensterOptionsPalette == null)
                {
                    _FensterOptionsPalette = new FensterOptionsPalette();
                }

                bool wasOpen = _FensterOptionsPalette.Show();

                if (wasOpen)
                {
                    return true;
                }
                else
                    return false; // returns nil

            }
            catch (System.Exception ex)
            {
                Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in DotNetFenster aufgetreten! {0}", ex.Message));

            }
            finally
            {
                Free();
            }
            return false;
        }

        [LispFunction("DotNetGetFensterOptions")]
        public static ResultBuffer DotNetGetFensterOptions(ResultBuffer rb)
        {
            if (_FensterOptionsPalette == null || _FensterOptionsPalette.FensterOptions == null)
            {
                return null;
            }
            return _FensterOptionsPalette.FensterOptions.AsResultBuffer();
        }

        [LispFunction("DotNetSetFensterOptions")]
        public static ResultBuffer DotNetSetFensterOptions(ResultBuffer rb)
        {
            FensterOptions fensterOptions;
            GetArgs(rb, out fensterOptions);
            _FensterOptionsPalette.SetFensterOptions(fensterOptions);

            return null;
        }


        private static void GetArgs(ResultBuffer rb, out FensterOptions opts)
        {
            TypedValue[] values = rb.AsArray();

            opts = new FensterOptions();
            opts.Breite = (double)values[1].Value;
            opts.Hoehe = (double)values[2].Value;
            opts.Parapet = (double)values[3].Value;
            opts.OlAb = (double)values[4].Value;
            opts.Staerke = (double)values[5].Value;
            opts.Stock = (double)values[6].Value;
            opts.SprossenBreite = (double)values[7].Value;
            opts.Sprossen = (int)(short)values[8].Value;
            opts.TextAbstand = (double)values[9].Value;
            opts.FluegelStaerke = (double)values[10].Value;
            opts.FensterArt = FensterOptions.StringToFenArt((string)values[11].Value);
        }

        private static void Free()
        {
            //_FlaechenGrenzen.Clear();
            //_Raumbloecke.Clear();
            //FreeSelectionSet(ref _ssFG);
            //FreeSelectionSet(ref _ssRB);
        }

    }
}
