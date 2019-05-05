using System.Collections.Generic;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

// ReSharper disable IdentifierTypo

namespace Plan2Ext
{
    internal static class XrefManager
    {
        public static IEnumerable<ObjectId> GetAllFirstLevelXrefIds(Database db)
        {
            var xrefOids = new List<ObjectId>();
            using (var transaction = db.TransactionManager.StartTransaction())
            {
                //db.ResolveXrefs(true, false);
                var xrefGraph = db.GetHostDwgXrefGraph(true);
                var graphNode = xrefGraph.RootNode;
                for (int i = 0; i < graphNode.NumOut; i++)
                {
                    var xrefGraphNode = graphNode.Out(i) as XrefGraphNode;
                    if (xrefGraphNode == null) continue;
                    if (xrefGraphNode.XrefStatus == XrefStatus.Resolved)
                    {
                        xrefOids.Add(xrefGraphNode.BlockTableRecordId);
                    }
                }

                transaction.Commit();
            }

            return xrefOids;
        }


        [CommandMethod("XrefGraph")]

        // ReSharper disable once UnusedMember.Global
        public static void XrefGraph()
        {

            Document doc = Application.DocumentManager.MdiActiveDocument;

            Database db = doc.Database;

            Editor ed = doc.Editor;

            // ReSharper disable once UnusedVariable
            using (Transaction tx = db.TransactionManager.StartTransaction())
            {

                ed.WriteMessage("\n---Resolving the XRefs------------------");

                db.ResolveXrefs(true, false);

                XrefGraph xg = db.GetHostDwgXrefGraph(true);

                ed.WriteMessage("\n---XRef's Graph-------------------------");

                ed.WriteMessage("\nCURRENT DRAWING");

                GraphNode root = xg.RootNode;

                PrintChildren(root, "|-------", ed);

                ed.WriteMessage("\n----------------------------------------\n");

            }

        }



        // Recursively prints out information about the XRef's hierarchy

        private static void PrintChildren(GraphNode iRoot, string iIndent,

            Editor iEd)
        {

            for (int o = 0; o < iRoot.NumOut; o++)
            {

                XrefGraphNode child = iRoot.Out(o) as XrefGraphNode;

                // ReSharper disable once PossibleNullReferenceException
                if (child.XrefStatus == XrefStatus.Resolved)
                {

                    //BlockTableRecord bl =

                    //    iTx.GetObject(child.BlockTableRecordId, OpenMode.ForRead)

                    //        as BlockTableRecord;

                    iEd.WriteMessage("\n" + iIndent + child.Database.Filename);

                    // Name of the Xref (found name)

                    // You can find the original path too:

                    //if (bl.IsFromExternalReference == true)

                    // i_ed.WriteMessage("\n" + i_indent + "Xref path name: "

                    //                      + bl.PathName);

                    PrintChildren(child, "| " + iIndent, iEd);

                }

            }

        }
    }
}
