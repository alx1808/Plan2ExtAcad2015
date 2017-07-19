//using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if BRX_APP
using Teigha.DatabaseServices;
#elif ARX_APP
  using Autodesk.AutoCAD.DatabaseServices;
#endif

namespace Plan2Ext.Raumnummern
{

    internal class Summarizer
    {
        private ISumRaumnummernHelper _SumHelper = null;
        private AreaEngine _AreaEngine = null;

        public Summarizer(ISumRaumnummernHelper sumHelper)
        {
            _SumHelper = sumHelper;
            _AreaEngine = new AreaEngine();
        }

        public void Start()
        {
            List<RaumInfo> raumInfos = GetAllRaumInfos();
            if (raumInfos == null) return;

            List<TopInfo> topInfos = new List<TopInfo>();
            var query = raumInfos.GroupBy(x => x.TopNr);
            foreach (var topGroup in query)
            {
                string topNr = topGroup.Key;
                double rbM2 = 0.0;
                double calcM2 = 0.0;
                foreach (var ri in topGroup)
                {
                    rbM2 += ri.RaumblockM2;
                    calcM2 += ri.CalculatedM2;
                }

                topInfos.Add(new TopInfo() { TopName = topNr, RaumblockM2 = rbM2, CalculatedM2 = calcM2 });

                // todo: create hatch per top 

                // todo: insert SumBlock

            }
        }

        private List<RaumInfo> GetAllRaumInfos()
        {
            // todo:
            List<ObjectId> flaechenGrenzen = new List<ObjectId>();
            List<ObjectId> raumBloecke = new List<ObjectId>();
            if (!_AreaEngine.SelectFgAndRb(flaechenGrenzen, raumBloecke, _SumHelper.FgLayer, _SumHelper.RbName)) return null;

            throw new NotImplementedException();
        }

        private class TopInfo
        {
            public string TopName { get; set; }
            public double RaumblockM2 { get; set; }
            public double CalculatedM2 { get; set; }
        }

        private class RaumInfo
        {
            public string TopNr { get; set; }
            public ObjectId RaumBlockId { get; set; }
            public double RaumblockM2 { get; set; }
            public double CalculatedM2 { get; set; }
            public ObjectId FlaechenGrenze { get; set; }
            public List<ObjectId> Abzugsflaechen { get; set; }
        }

    }
}
