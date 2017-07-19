using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plan2Ext.Raumnummern
{
    internal interface ISumRaumnummernHelper
    {
         string FgLayer { get; set; }
         string AfLayer { get; set; }
         string RbName { get; set; }
         string NrAttName { get; set; }
         string M2AttName { get; set; }

    }
}
