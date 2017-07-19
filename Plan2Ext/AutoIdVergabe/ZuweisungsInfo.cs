using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plan2Ext.AutoIdVergabe
{
    internal class ZuweisungsInfo
    {
        public string FromAtt { get; set; }
        public string ToAtt { get; set; }

        public string[] ToArr()
        {
            return new string[] { FromAtt, ToAtt };
        }
    }
}
