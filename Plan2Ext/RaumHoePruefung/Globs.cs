//using Autodesk.AutoCAD.ApplicationServices;
//using Autodesk.AutoCAD.DatabaseServices;
//using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if BRX_APP
using Bricscad.ApplicationServices;
using Teigha.DatabaseServices;
using Teigha.Geometry;
#elif ARX_APP
  using Autodesk.AutoCAD.ApplicationServices;
  using Autodesk.AutoCAD.DatabaseServices;
  using Autodesk.AutoCAD.Geometry;
#endif


namespace Plan2Ext.RaumHoePruefung
{
    internal class Globs
    {
        public static HoePrOptions TheHoePrOptions = null;

        public static void CancelCommand()
        {
            Application.DocumentManager.MdiActiveDocument.SendStringToExecute("\x1B", true, false, true);
            Application.DocumentManager.MdiActiveDocument.SendStringToExecute("\x1B", true, false, true);
        }

        public static void SetXrecord(ObjectId id, string key, ResultBuffer resbuf)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                Entity ent = tr.GetObject(id, OpenMode.ForRead) as Entity;
                if (ent != null)
                {
                    ent.UpgradeOpen();
                    if (ent.ExtensionDictionary == default(ObjectId)) ent.CreateExtensionDictionary();
                    DBDictionary xDict = (DBDictionary)tr.GetObject(ent.ExtensionDictionary, OpenMode.ForWrite);
                    Xrecord xRec = new Xrecord();
                    xRec.Data = resbuf;
                    xDict.SetAt(key, xRec);
                    tr.AddNewlyCreatedDBObject(xRec, true);
                }
                tr.Commit();
            }
        }

        public static ResultBuffer GetXrecord(ObjectId id, string key)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            ResultBuffer result = new ResultBuffer();
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                Xrecord xRec = new Xrecord();
                Entity ent = tr.GetObject(id, OpenMode.ForRead, false) as Entity;
                if (ent != null)
                {
                    try
                    {
                        if (ent.ExtensionDictionary == default(ObjectId)) return null;
                        DBDictionary xDict = (DBDictionary)tr.GetObject(ent.ExtensionDictionary, OpenMode.ForRead, false);
                        xRec = (Xrecord)tr.GetObject(xDict.GetAt(key), OpenMode.ForRead, false);
                        return xRec.Data;
                    }
                    catch
                    {
                        return null;
                    }
                }
                else
                    return null;
            }
        }

        public static bool GetFirstDoubleInText(string txt, out double dblVal)
        {
            bool inNr = false;
            bool comma = false;
            StringBuilder sb = new StringBuilder();
            foreach (var c in txt.ToArray())
            {
                if (!inNr)
                {
                    // not in nr
                    if (IsNumeric(c))
                    {
                        sb.Append(c);
                        inNr = true;
                    }
                }
                else
                {
                    // in nr
                    if (IsNumeric(c))
                    {
                        sb.Append(c);
                    }
                    else if (c == '.' || c == ',')
                    {
                        if (comma) break; // second comma -> break
                        sb.Append('.');
                        comma = true;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            string sNr = sb.ToString();
            if (!double.TryParse(sNr, NumberStyles.Any, CultureInfo.InvariantCulture, out dblVal)) return false;
            else return true;
        }

        public static bool IsNumeric(char c)
        {
            return c >= '0' && c <= '9';
        }

        public static Point3d GetCenter(Entity ent)
        {
            var bounds = ent.Bounds;
            if (!bounds.HasValue) return default(Point3d);

            var minpt = bounds.Value.MinPoint;
            var maxpt = bounds.Value.MaxPoint;

            return new Point3d((minpt.X + maxpt.X) / 2.0, (minpt.Y + maxpt.Y) / 2.0, 0);
                        

        }

    }

}
