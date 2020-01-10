#if BRX_APP
using _AcAp = Bricscad.ApplicationServices;
using _AcCm = Teigha.Colors;
using Teigha.DatabaseServices;
using Bricscad.EditorInput;
using Teigha.Geometry;
using _AcIntCom = BricscadDb;
#elif ARX_APP
using _AcAp = Autodesk.AutoCAD.ApplicationServices;
using _AcCm = Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using _AcIntCom = Autodesk.AutoCAD.Interop.Common;
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plan2Ext.Raumnummern
{
    internal interface IEngineParameter
    {
        RnOptions Options { get; set; }
        Dictionary<ObjectId, AreaEngine.FgRbStructure> FgRbs { get; set; }
        List<ObjectId> AllRaumBlocks { get; set; }
        IFgRbsPerTopNr FgRbPerTopNr { get; set; }
    }

    internal class EngineParameter : IEngineParameter
    {
        public RnOptions Options { get; set; }
        public Dictionary<ObjectId, AreaEngine.FgRbStructure> FgRbs { get; set; }
        public List<ObjectId> AllRaumBlocks { get; set; }
        public IFgRbsPerTopNr FgRbPerTopNr { get; set; }
    }
}
