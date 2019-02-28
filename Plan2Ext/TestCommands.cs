using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

namespace Plan2Ext
{
    public  class TestCommands
    {


        [CommandMethod("wblockEntity")]

        public static void wblockEntity()
        {

            Document doc = Application.DocumentManager.MdiActiveDocument;

            Database db = doc.Database;

            Editor ed = doc.Editor;



            PromptSelectionResult prRes = ed.GetSelection();



            if (prRes.Status != PromptStatus.OK)

                return;



            ObjectIdCollection objIds = new ObjectIdCollection();

            ObjectId[] objIdArray = prRes.Value.GetObjectIds();



            // Copy objectIds to objectIdCollection

            foreach (ObjectId id in objIdArray)

                objIds.Add(id);



            using (Database newDb = new Database(true, false))
            {

                db.Wblock(newDb, objIds, Point3d.Origin,

                    DuplicateRecordCloning.Ignore);

                string FileName = "C:\\temp\\wblock.dwg";

                newDb.SaveAs(FileName, DwgVersion.Newest);

            }

        }

    }
}
