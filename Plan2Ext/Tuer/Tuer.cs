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

namespace Plan2Ext.Tuer
{
    public class Tuer
    {
        #region log4net Initialization
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Convert.ToString((typeof(Tuer))));
        static Tuer()
        {
            if (log4net.LogManager.GetRepository(System.Reflection.Assembly.GetExecutingAssembly()).Configured == false)
            {
                log4net.Config.XmlConfigurator.ConfigureAndWatch(new System.IO.FileInfo(System.IO.Path.Combine(new System.IO.FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).DirectoryName, "_log4net.config")));
            }

        }
        #endregion

        static TuerOptionsPalette _TuerOptionsPalette;

        [LispFunction("DotNetTuerOptions")]
        public static object DotNetTuerOptions(ResultBuffer rb)
        {
            log.Debug("--------------------------------------------------------------------------------");
            log.Debug("DotNetTuerOptions");
            try
            {
                Document doc = Application.DocumentManager.MdiActiveDocument;
                log.DebugFormat("Dokumentname: {0}.", doc.Name);

                if (_TuerOptionsPalette == null)
                {
                    _TuerOptionsPalette = new TuerOptionsPalette();
                }

                bool wasOpen = _TuerOptionsPalette.Show();

                if (wasOpen)
                {
                    return true;
                }
                else
                    return false; // returns nil

            }
            catch (System.Exception ex)
            {
                Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in DotNetTuer aufgetreten! {0}", ex.Message));

            }
            finally
            {
                Free();
            }
            return false;
        }

        [LispFunction("DotNetGetTuerOptions")]
        public static ResultBuffer DotNetGetTuerOptions(ResultBuffer rb)
        {
            if (_TuerOptionsPalette == null || _TuerOptionsPalette.TuerOptions == null)
            {
                return null;
            }
            return _TuerOptionsPalette.TuerOptions.AsResultBuffer();
        }

        [LispFunction("DotNetSetTuerOptions")]
        public static ResultBuffer DotNetSetTuerOptions(ResultBuffer rb)
        {
            TuerOptions TuerOptions;
            GetArgs(rb, out TuerOptions);
            _TuerOptionsPalette.SetTuerOptions(TuerOptions);

            return null;
        }

        private static void GetArgs(ResultBuffer rb, out TuerOptions opts)
        {
            TypedValue[] values = rb.AsArray();

            opts = new TuerOptions();
            opts.Breite = (double)values[1].Value;
            opts.Hoehe = (double)values[2].Value;
            opts.Fluegel = (int)(short)values[3].Value;
            opts.TuerArt = TuerOptions.IntToTuerArt((int)(short)values[4].Value);
            opts.TextBlockTyp = TuerOptions.IntToTextBlockTyp((int)(short)values[5].Value);
            opts.StockStaerke = (double)values[6].Value;
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
