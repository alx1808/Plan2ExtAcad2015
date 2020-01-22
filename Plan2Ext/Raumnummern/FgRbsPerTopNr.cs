#if BRX_APP
using Bricscad.ApplicationServices;
using Teigha.DatabaseServices;
using Bricscad.EditorInput;
using Bricscad.Runtime;
using Teigha.Runtime;
using Bricscad.Internal;

#elif ARX_APP
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Internal;
using Autodesk.AutoCAD.DatabaseServices;
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plan2Ext.Raumnummern
{
    internal interface IFgRbsPerTopNr
    {
        IEnumerable<AreaEngine.FgRbStructure> GetFgRbStructures(string topNr);
        void RemoveFgRb(AreaEngine.FgRbStructure fgRb, Transaction transaction);
        void AddFgRb(AreaEngine.FgRbStructure fgRb, string topNr, Transaction transaction);
    }

    internal class FgRbsPerTopNr : IFgRbsPerTopNr
    {
        private readonly Dictionary<string, List<AreaEngine.FgRbStructure>> _dictionary = new Dictionary<string, List<AreaEngine.FgRbStructure>>();

        public FgRbsPerTopNr(IEnumerable<AreaEngine.FgRbStructure> fgRbs)
        {
            Build(fgRbs);
        }
        private void Build(IEnumerable<AreaEngine.FgRbStructure> fgRbs)
        {
            _dictionary.Clear();
            var doc = Application.DocumentManager.MdiActiveDocument;
            using (var transaction = doc.Database.TransactionManager.StartTransaction())
            {
                foreach (var value in fgRbs)
                {
                    var topNr = Engine.GetTopNr(value, transaction);
                    if (topNr == null) continue;
                    List<AreaEngine.FgRbStructure> lst;
                    if (!_dictionary.TryGetValue(topNr, out lst))
                    {
                        lst = new List<AreaEngine.FgRbStructure>();
                        _dictionary.Add(topNr, lst);
                    }
                    lst.Add(value);
                }
                transaction.Commit();
            }
        }

        //fgrbs = _FgRbStructs.Values.Where(x => x != fgrb && string.Compare(topNr, GetTopNr(x, transaction)) == 0).ToList();
        public IEnumerable<AreaEngine.FgRbStructure> GetFgRbStructures(string topNr)
        {
            List<AreaEngine.FgRbStructure> lst;
            return _dictionary.TryGetValue(topNr, out lst) ? lst : new List<AreaEngine.FgRbStructure>();
        }

        public void RemoveFgRb(AreaEngine.FgRbStructure fgRb, Transaction transaction)
        {
            var topNr = Engine.GetTopNr(fgRb, transaction);
            if (topNr != null)
            {
                List<AreaEngine.FgRbStructure> lst;
                if (_dictionary.TryGetValue(topNr, out lst))
                {
                    if (lst.Contains(fgRb)) 
                        lst.Remove(fgRb);
                }
            }
        }

        public void AddFgRb(AreaEngine.FgRbStructure fgRb, string topNr, Transaction transaction)
        {
            RemoveFgRb(fgRb, transaction);

            List<AreaEngine.FgRbStructure> lst;
            if (!_dictionary.TryGetValue(topNr, out lst))
            {
                lst = new List<AreaEngine.FgRbStructure>();
                _dictionary.Add(topNr, lst);
            }
            if (!lst.Contains(fgRb))
                lst.Add(fgRb);
        }
    }
}
