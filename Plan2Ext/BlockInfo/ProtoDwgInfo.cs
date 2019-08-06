using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Exception = Autodesk.AutoCAD.Runtime.Exception;
// ReSharper disable IdentifierTypo
// ReSharper disable StringLiteralTypo

namespace Plan2Ext.BlockInfo
{
    internal interface IProtoDwgInfo
    {
        List<string> GetOrderedBlocknames(string protoDwgName);
    }

    internal class ProtoDwgInfo : IProtoDwgInfo
    {
        public List<string> GetOrderedBlocknames(string protoDwgName)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            string protoDwgFullPath;
            try
            {
                protoDwgFullPath = HostApplicationServices.Current.FindFile(protoDwgName, doc.Database, FindFileHint.Default);
            }
            catch (Exception)
            {
                doc.Editor.WriteMessage(string.Format(CultureInfo.CurrentCulture, "\nKonnte Prototypzeichnung '{0}' nicht finden!", protoDwgName));
                return null;
            }

            using (var openDb = new Database(buildDefaultDrawing: false, noDocument: true))
            {
                openDb.ReadDwgFile(protoDwgFullPath, System.IO.FileShare.ReadWrite, allowCPConversion: true, password: "");
                using (var tr = openDb.TransactionManager.StartTransaction())
                {
                    var btr = (BlockTableRecord)tr.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(openDb),
                        OpenMode.ForRead);
                    var blockList = new List<BlockReference>();
                    foreach (var oid in btr)
                    {
                        var blockReference = tr.GetObject(oid, OpenMode.ForRead) as BlockReference;
                        if (blockReference != null) blockList.Add(blockReference);
                    }

                    tr.Commit();

                    var orderedBlocks = blockList.OrderBy(x => x.Position.Y).Reverse();
                    return orderedBlocks.Select(x => Globs.GetBlockname(x, tr)).Distinct().ToList();
                }
            }
        }
    }
}
