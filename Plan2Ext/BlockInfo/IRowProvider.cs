using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plan2Ext.BlockInfo
{
    interface IRowProvider
    {
        IEnumerable<string> RowValues();
    }
}
