
using System;
#if BRX_APP
using Bricscad.ApplicationServices;
using Teigha.Runtime;
#elif ARX_APP
using Autodesk.AutoCAD.Runtime;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Autodesk.AutoCAD.ApplicationServices;
#endif
using Plan2Ext.CalcArea;

namespace Plan2Ext
{
    public class Plan2Application : IExtensionApplication
    {
        public void Initialize()
        {
            Globs.TheApplication = this;
            Flaeche.TheCalcAreaPalette = new CalcAreaPalette();
        }
		public void Terminate()
        {
        }
    }
}
