

//using Autodesk.AutoCAD.ApplicationServices;
//using Autodesk.AutoCAD.DatabaseServices;
//using Autodesk.AutoCAD.EditorInput;
//using Autodesk.AutoCAD.Geometry;
//using Autodesk.AutoCAD.Runtime;



//namespace Explosion
//{
//    public class Commands
//    {
//        [CommandMethod("INSERTDWG")]

//        public void InsertDwg()
//        {

//            Document doc = Application.DocumentManager.MdiActiveDocument;
//            Database db = doc.Database;
//            Editor ed = doc.Editor;

//            ObjectId objId;

//            string fname = "D:\\Plan2\\Data\\AutoId\\xref.dwg";
//            using (Transaction tr = db.TransactionManager.StartTransaction())
//            {
//                BlockTable bt = db.BlockTableId.GetObject(OpenMode.ForRead) as BlockTable ;
//                BlockTableRecord btrMs = db.CurrentSpaceId.GetObject(OpenMode.ForWrite) as BlockTableRecord ;
//                using (Database dbInsert = new Database(false,true))
//                {
//                    dbInsert.ReadDwgFile(fname, System.IO.FileShare.Read,true,"");
//                    objId = db.Insert(System.IO.Path.GetFileNameWithoutExtension("xref"), dbInsert, true);
 
//                }
//                Point3d insertPt = new Point3d(0.0, 0.0, 0.0);
//                BlockReference bref = new BlockReference(insertPt, objId);
//                bref.Rotation = 45;
//                btrMs.AppendEntity(bref);
//                tr.AddNewlyCreatedDBObject(bref, true);

                
//                tr.Commit();
//            }

//        }
//        [CommandMethod("BX")]

//        public void BindXrefs()
//        {

//            Document doc = Application.DocumentManager.MdiActiveDocument;

//            Database db = doc.Database;



//            ObjectIdCollection xrefCollection = new ObjectIdCollection();



//            using (XrefGraph xg = db.GetHostDwgXrefGraph(false))
//            {

//                int numOfNodes = xg.NumNodes;

//                for (int cnt = 0; cnt < xg.NumNodes; cnt++)
//                {

//                    XrefGraphNode xNode = xg.GetXrefNode(cnt)

//                                                        as XrefGraphNode;

//                    if (!xNode.Database.Filename.Equals(db.Filename))
//                    {

//                        if (xNode.XrefStatus == XrefStatus.Resolved)
//                        {

//                            xrefCollection.Add(xNode.BlockTableRecordId);

//                        }

//                    }

//                }

//            }



//            if (xrefCollection.Count != 0)

//                db.BindXrefs(xrefCollection, true);



//        }


//        [CommandMethod("EXP", CommandFlags.UsePickSet)]

//        public void ExplodeEntities()
//        {

//            Document doc =

//                Application.DocumentManager.MdiActiveDocument;

//            Database db = doc.Database;

//            Editor ed = doc.Editor;



//            // Ask user to select entities



//            PromptSelectionOptions pso =

//              new PromptSelectionOptions();

//            pso.MessageForAdding = "\nSelect objects to explode: ";

//            pso.AllowDuplicates = false;

//            pso.AllowSubSelections = true;

//            pso.RejectObjectsFromNonCurrentSpace = true;

//            pso.RejectObjectsOnLockedLayers = false;



//            PromptSelectionResult psr = ed.GetSelection(pso);

//            if (psr.Status != PromptStatus.OK)

//                return;



//            // Check whether to erase the original(s)



//            bool eraseOrig = false;



//            if (psr.Value.Count > 0)
//            {

//                PromptKeywordOptions pko =

//                  new PromptKeywordOptions("\nErase original objects?");

//                pko.AllowNone = true;

//                pko.Keywords.Add("Yes");

//                pko.Keywords.Add("No");

//                pko.Keywords.Default = "No";



//                PromptResult pkr = ed.GetKeywords(pko);

//                if (pkr.Status != PromptStatus.OK)

//                    return;



//                eraseOrig = (pkr.StringResult == "Yes");

//            }



//            Transaction tr =

//              db.TransactionManager.StartTransaction();

//            using (tr)
//            {

//                // Collect our exploded objects in a single collection



//                DBObjectCollection objs = new DBObjectCollection();



//                // Loop through the selected objects



//                foreach (SelectedObject so in psr.Value)
//                {

//                    // Open one at a time



//                    Entity ent =

//                      (Entity)tr.GetObject(

//                        so.ObjectId,

//                        OpenMode.ForRead

//                      );



//                    // Explode the object into our collection



//                    ent.Explode(objs);



//                    // Erase the original, if requested



//                    if (eraseOrig)
//                    {

//                        ent.UpgradeOpen();

//                        ent.Erase();

//                    }

//                }



//                // Now open the current space in order to

//                // add our resultant objects



//                BlockTableRecord btr =

//                  (BlockTableRecord)tr.GetObject(

//                    db.CurrentSpaceId,

//                    OpenMode.ForWrite

//                  );



//                // Add each one of them to the current space

//                // and to the transaction



//                foreach (DBObject obj in objs)
//                {

//                    Entity ent = (Entity)obj;

//                    btr.AppendEntity(ent);

//                    tr.AddNewlyCreatedDBObject(ent, true);

//                }



//                // And then we commit



//                tr.Commit();

//            }

//        }

//    }

//}
