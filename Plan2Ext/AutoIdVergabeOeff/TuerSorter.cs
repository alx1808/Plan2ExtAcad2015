using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;

namespace Plan2Ext.AutoIdVergabeOeff
{
    internal class TuerSorter : Sorter
    {
        private readonly IConfigurationHandler _configurationHandler;
        private readonly IPalette _palette;
        private int _currentNr;

        public TuerSorter(ConfigurationHandler configurationHandler, IPalette palette)
        {
            _configurationHandler = configurationHandler;
            _palette = palette;
        }

        public void Sort(IEnumerable<ITuerInfo> tuerInfos)
        {
            _currentNr = _palette.TuerNr;
            var arr = tuerInfos.ToArray();
            SortTuerenInRaum(arr);
            try
            {
                UcsToAnsicht();
                // todo:
                //SortFromLeftToRight(arr);
            }
            finally
            {
                UcsRestore();
            }

            _palette.TuerNr = _currentNr + 1;


        }

        private void SortTuerenInRaum(ITuerInfo[] tuerInfos)
        {
            var raumTuerInfos = tuerInfos.Where(x => !x.RaumblockId.IsNull).ToArray();
            var doc = Application.DocumentManager.MdiActiveDocument;
            using (var transaction = doc.TransactionManager.StartTransaction())
            {


                //var bounds = curve.Bounds;
                //var startPoint = new Point3d(bounds.Value.MinPoint.X, bounds.Value.MaxPoint.Y, 0.0);
                var ordered = raumTuerInfos.OrderBy(x => GetCompareValueFromRaumblock(x.RaumblockId,transaction)).ToArray();
                Renumber(ordered, transaction);

                transaction.Commit();
            }
        }

        private string GetCompareValueFromRaumblock(ObjectId argRaumblockId, Transaction transaction)
        {
            var raumblockRef = (BlockReference) transaction.GetObject(argRaumblockId, OpenMode.ForRead);
            var nrAtt = Globs.GetAttributEntities(raumblockRef, transaction).FirstOrDefault(x =>
                string.Compare(x.Tag, _configurationHandler.RaumIdAttName, StringComparison.OrdinalIgnoreCase) == 0);
            if (nrAtt == null)
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture,
                    "Raumblock mit Handle {0} hat kein Attribut {1}!", raumblockRef.Handle.ToString(),
                    _configurationHandler.RaumIdAttName));

            return nrAtt.TextString;
        }

        private void Renumber(ITuerInfo[] ordered, Transaction transaction)
        {
            foreach (var tuerInfo in ordered)
            {
                var blockReference = (BlockReference)transaction.GetObject(tuerInfo.Oid, OpenMode.ForRead);
                var nrAtt = Globs.GetAttributEntities(blockReference, transaction).FirstOrDefault(x =>
                    string.Compare(x.Tag, _configurationHandler.TuerNrAttName, StringComparison.OrdinalIgnoreCase) == 0);
                if (nrAtt == null)
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture,
                        "Türblock mit Handle {0} hat kein Attribut {1}!", blockReference.Handle.ToString(),
                        _configurationHandler.TuerNrAttName));
                nrAtt.UpgradeOpen();
                nrAtt.TextString = _palette.TuerPrefix + _currentNr.ToString().PadLeft(3, '0');
                nrAtt.DowngradeOpen();
                _currentNr++;
            }
        }

    }
}
