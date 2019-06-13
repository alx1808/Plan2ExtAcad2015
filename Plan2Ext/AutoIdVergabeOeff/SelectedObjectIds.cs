using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;

namespace Plan2Ext.AutoIdVergabeOeff
{
    internal class SelectedObjectIds
    {
        public List<ObjectId> FensterIds { get; set; }
        public List<ObjectId> TuerIds { get; set; } 
        public ObjectId ObjectPolygonId { get; set; }

        public SelectedObjectIds()
        {
            Init();
        }

        private void Init()
        {
            FensterIds = new List<ObjectId>();
            TuerIds = new List<ObjectId>();
        }
    }
}
