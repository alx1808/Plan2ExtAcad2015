using System.Collections.Generic;

namespace Plan2Ext.BlockInfo
{
    internal static class LayoutBlockNameFactory
    {
        public static IEnumerable<IRowProvider> CreateRowProviders(string layoutName, List<string> blockNames)
        {
            var rowProviders = new List<IRowProvider>();
            for (var i = 0; i < blockNames.Count; i++)
            {
                var ln = i == 0 ? layoutName : "";
                rowProviders.Add(new LayoutBlockRowProvider() { BlockName = blockNames[i], LayoutName = ln });
            }

            return rowProviders;
        }
    }
}
