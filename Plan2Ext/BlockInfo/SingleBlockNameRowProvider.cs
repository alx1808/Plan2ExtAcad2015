using System.Collections.Generic;
// ReSharper disable IdentifierTypo

namespace Plan2Ext.BlockInfo
{
    internal class SingleBlockNameRowProvider : IRowProvider
    {
        public string Blockname { get; set; }
        public IEnumerable<string> RowValues()
        {
            return new[] { Blockname };
        }
    }
}
