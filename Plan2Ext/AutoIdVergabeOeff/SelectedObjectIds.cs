using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
// ReSharper disable IdentifierTypo

namespace Plan2Ext.AutoIdVergabeOeff
{
    internal class SelectedObjectIds
    {
        public List<ObjectId> FensterIds { get; private set; }
        public List<ObjectId> TuerIds { get; private set; }
        public List<ObjectId> RaumBlockIds { get; private set; }
        public List<ObjectId> FlaGrenzIds { get; private set; } 
        public ObjectId ObjectPolygonId { get; set; }

        public SelectedObjectIds()
        {
            Init();
        }

        private void Init()
        {
            FensterIds = new List<ObjectId>();
            TuerIds = new List<ObjectId>();
            RaumBlockIds = new List<ObjectId>();
            FlaGrenzIds = new List<ObjectId>();
        }
    }
}
