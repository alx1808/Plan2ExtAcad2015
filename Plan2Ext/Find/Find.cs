#if BRX_APP
using Teigha.DatabaseServices;
using Teigha.Runtime;
using Bricscad.ApplicationServices;
using Bricscad.EditorInput;
#elif ARX_APP
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
#endif
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
// ReSharper disable CommentTypo
// ReSharper disable StringLiteralTypo
// ReSharper disable IdentifierTypo


namespace Plan2Ext.Find
{
    public class Find
    {
        private Transaction _transaction;

        private readonly List<IReplacer> _replacers = new List<IReplacer>
        {
            new BlockReferenceReplacer(new AttributeReferenceReplacer()),
            new AttributeDefinitionReplacer(), // must come before DbTextReplacer
            new DbTextReplacer(),
            new MTextReplacer(),
        };

        [CommandMethod("-Find", CommandFlags.UsePickSet)]
        public void FindCommandLine()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            var editor = doc.Editor;
            try
            {
	            string searchText;
	            string replaceText;
	            if (!GetTextOptions(editor, out searchText, out replaceText)) return;

	            using (_transaction = doc.TransactionManager.StartTransaction())
	            {
		            foreach (var objectId in NextOid())
		            {
			            var dbObject = _transaction.GetObject(objectId, OpenMode.ForRead);
			            var replacer = _replacers.FirstOrDefault(x => x.SetEntityIfApplicable(dbObject));
			            if (replacer == null) continue;
			            dbObject.UpgradeOpen();
			            replacer.Replace(searchText, replaceText);
			            dbObject.DowngradeOpen();
		            }

		            _transaction.Commit();
	            }
            }
            catch (System.Exception ex)
            {
	            string msg = string.Format(CultureInfo.CurrentCulture, "Fehler in (-Find): {0}", ex.Message);
	            editor.WriteMessage("\n" + msg);
            }
            finally
            {
                // needs to be done for bricscad
				editor.SetImpliedSelection(new ObjectId[]{});
            }

        }

        private static bool GetTextOptions(Editor editor, out string searchText, out string replaceText)
        {
            searchText = default(string);
            replaceText = default(string);
            var pso = new PromptStringOptions("\nZu ersetzender Text: ") {AllowSpaces = true};
            var result = editor.GetString(pso);
            if (result.Status != PromptStatus.OK) return false;
            searchText = result.StringResult;
            if (string.IsNullOrEmpty(searchText)) return false;
            pso = new PromptStringOptions("\nNeuer Text: ") {AllowSpaces = true};
            result = editor.GetString(pso);
            if (result.Status != PromptStatus.OK) return false;
            replaceText = result.StringResult;
            return true;
        }

        private IEnumerable<ObjectId> NextOid()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            var editor = doc.Editor;

            var objectIds = editor.PickfirstSelection().ToArray();
            if (objectIds.Any())
            {
                foreach (var objectId in objectIds)
                {
                    yield return objectId;
                }
            }
            else
            {
                var keywords = new[] { "Aktuelles Layout", "Gesamte Zeichnung" };
                var keyword = Globs.AskKeywordFromUser("", keywords, 1);
                if (keyword == null) yield break;

                if (keyword.StartsWith("Aktuelles"))
                {
                    var space = (BlockTableRecord)_transaction.GetObject(db.CurrentSpaceId, OpenMode.ForRead);
                    foreach (var oid in space)
                    {
                        yield return oid;
                    }
                }
                else
                {
                    var blockTable = (BlockTable)_transaction.GetObject(db.BlockTableId, OpenMode.ForRead);
                    foreach (var btroid in blockTable)
                    {
                        var btr = (BlockTableRecord)_transaction.GetObject(btroid, OpenMode.ForRead);
                        foreach (var oid in btr)
                        {
                            yield return oid;
                        }
                    }
                }
            }
        }
    }
}
