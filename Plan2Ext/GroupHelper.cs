using System.Collections.Generic;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;

namespace Plan2Ext
{
    internal class GroupHelper
    {
        public bool AddToGroup(IEnumerable<ObjectId> oids, string groupName, Document doc, bool deleteExisting)
        {
            using (var transaction = doc.TransactionManager.StartTransaction())
            {
                try
                {

                    var groupDictionary =
                        (DBDictionary)transaction.GetObject(doc.Database.GroupDictionaryId, OpenMode.ForRead);

                    groupDictionary.UpgradeOpen();

                    SymbolUtilityServices.ValidateSymbolName(groupName, false);
                    if (groupDictionary.Contains(groupName))
                    {
                        if (deleteExisting)
                        {
                            var grp = (Group) transaction.GetObject(groupDictionary.GetAt(groupName),
                                OpenMode.ForWrite);
                            grp.Erase();
                            groupDictionary.Remove(groupName);
                        }
                        else
                        {
                            doc.Editor.WriteMessage($"\nDer Gruppenname {groupName} existiert bereits.");
                            return false;
                        }
                    }
                    
                    var group = new Group(groupName, true);
                    groupDictionary.SetAt(groupName, group);
                    transaction.AddNewlyCreatedDBObject(group, true);
                    foreach (var oid in oids)
                    {
                        group.Append(oid);
                    }

                    return true;
                }
                finally
                {
                    transaction.Commit();
                }
            }
        }
    }
}
