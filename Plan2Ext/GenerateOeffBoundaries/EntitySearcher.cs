using System;
using System.Collections.Generic;
using System.Globalization;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using log4net;
// ReSharper disable StringLiteralTypo
// ReSharper disable IdentifierTypo

namespace Plan2Ext.GenerateOeffBoundaries
{
    internal class EntitySearcher
    {
        #region log4net Initialization
        private static readonly ILog Log = LogManager.GetLogger(Convert.ToString((typeof(EntitySearcher))));
        #endregion

        private readonly List<string> _configuredBlockNames = new List<string>();
        private readonly List<string> _configuredBlockNamesVariables = new List<string>()
        {
            "alx_V:ino_fenster_Block_Oben",
            "alx_V:ino_fenster_Block_Unten",
            "alx_V:ino_fenster_Block_Links",
            "alx_V:ino_fenster_Block_Rechts",
            "alx_V:ino_tuerBlock_Oben",
            "alx_V:ino_tuerBlock_Unten",
            "alx_V:ino_tuerBlock_Links",
            "alx_V:ino_tuerBlock_Rechts",
        };

        public EntitySearcher()
        {
            ReadConfiguration();
        }

        public IEnumerable<Point3d> GetInsertPointsInMs()
        {
            var points = new List<Point3d>();
            var doc = Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            using (var transaction = doc.TransactionManager.StartTransaction())
            {
                var blockTable = (BlockTable)transaction.GetObject(db.BlockTableId, OpenMode.ForRead);
                var blockTableRecord = (BlockTableRecord)transaction.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForRead);
                foreach (var oid in blockTableRecord)
                {
                    var blockReference = transaction.GetObject(oid, OpenMode.ForRead) as BlockReference;
                    if (blockReference != null)
                    {
                        if (_configuredBlockNames.Contains(blockReference.Name))
                        {
                            points.Add(blockReference.Position);
                        }
                    }

                }
                transaction.Commit();
            }

            return points;
        }

        private void ReadConfiguration()
        {
            foreach (var configuredBlockNamesVariable in _configuredBlockNamesVariables)
            {
                string val;
                if (GetFromConfig(out val, configuredBlockNamesVariable))
                {
                    var valUc = val.ToUpperInvariant();
                    if (!_configuredBlockNames.Contains(valUc)) _configuredBlockNames.Add(valUc);
                }
                else Log.Warn(string.Format(CultureInfo.CurrentCulture, "Variable {0} ist nicht konfiguriert!", configuredBlockNamesVariable));

            }
        }
        private static bool GetFromConfig(out string val, string varName)
        {
            val = null;
            try
            {
                val = TheConfiguration.GetValueString(varName);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
