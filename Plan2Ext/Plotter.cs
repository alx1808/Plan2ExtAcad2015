using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace Plan2Ext
{
    internal static class Plotter
    {
        public static List<string> GetDeviceList(Database db)
        {
            var devices = new List<string>();
            using (var trans = db.TransactionManager.StartTransaction())
            {
                var plotSetVal = PlotSettingsValidator.Current;
                var layouts = (DBDictionary)trans.GetObject(db.LayoutDictionaryId, OpenMode.ForRead);
                foreach (var layoutDe in layouts)
                {
                    var layoutId = layoutDe.Value;
                    var layoutObj = (Layout)trans.GetObject(layoutId, OpenMode.ForRead);
                    if (layoutObj.LayoutName != "Model") continue;
                    
                    layoutObj.UpgradeOpen();
                    plotSetVal.RefreshLists(layoutObj);
                    layoutObj.DowngradeOpen();
                    System.Collections.Specialized.StringCollection deviceList = plotSetVal.GetPlotDeviceList();
                    foreach (var dev in deviceList)
                    {
                        devices.Add(dev);
                    }
                    break;
                }
                trans.Commit();
            }

            return devices;
        }
    }
}
