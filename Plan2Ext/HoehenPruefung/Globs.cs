//using Autodesk.AutoCAD.ApplicationServices;
//using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if BRX_APP
using Bricscad.ApplicationServices;
using Teigha.DatabaseServices;
#elif ARX_APP
  using Autodesk.AutoCAD.ApplicationServices;
  using Autodesk.AutoCAD.DatabaseServices;
#endif

namespace Plan2Ext.HoehenPruefung
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
    }

}
