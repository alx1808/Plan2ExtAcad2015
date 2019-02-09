using System.Collections.Generic;

namespace Plan2Ext.BlockInfo
{
    internal class LayoutBlockRowProvider : IRowProvider
    {
        public string LayoutName { get; set; }
        public string BlockName { get; set; }
        public IEnumerable<string> RowValues()
        {
            return new[] {LayoutName, BlockName};
        }
    }
}
