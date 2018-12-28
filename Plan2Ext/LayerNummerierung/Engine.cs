using Autodesk.AutoCAD.ApplicationServices.Core;
using _AcAp = Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using _AcIntCom = Autodesk.AutoCAD.Interop.Common;
using Autodesk.AutoCAD.Colors;
// ReSharper disable StringLiteralTypo

// ReSharper disable once IdentifierTypo
namespace Plan2Ext.LayerNummerierung
{
    internal class Engine
    {
        #region log4net Initialization
        // ReSharper disable once UnusedMember.Local
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Convert.ToString((typeof(Engine))));
        #endregion

        private readonly NrOptions _opts;
        
        public Engine(NrOptions opts)
        {
            _opts = opts;
        }

        #region Internal

        internal bool AddNumber()
        {
            bool result = false;
            var db = Application.DocumentManager.MdiActiveDocument.Database;
            var ed = Application.DocumentManager.MdiActiveDocument.Editor;

            using (var transaction =  db.TransactionManager.StartTransaction())
            {
                var getEntResult = ed.GetEntity("Element wählen: ");
                if (getEntResult.Status == PromptStatus.OK)
                {
                    var layerName = GetNextLayerName(_opts.Number);
                    var col = Color.FromColorIndex(ColorMethod.ByAci, 10);
                    Plan2Ext.Globs.CreateLayer(layerName, col);
                    
                    var ent = (Entity) transaction.GetObject(getEntResult.ObjectId, OpenMode.ForWrite);
                    ent.Layer = layerName;

                    var newNr = Increment(_opts.Number);
                    _opts.SetNumber(newNr);

                    result = true;
                }
                transaction.Commit();
            }
            return result;
        }


        #endregion

        #region Private
        private string Increment(string number)
        {
            int len = number.Length;
            int i = int.Parse(number);
            i++;
            string s = i.ToString();
            return s.PadLeft(len, '0');
        }

        private string GetNextLayerName(string curNumber)
        {
            return _opts.Prefix + curNumber + _opts.Suffix;
        }
        #endregion
    }
}
