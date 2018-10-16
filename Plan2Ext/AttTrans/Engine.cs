using System;
using System.Collections.Generic;
using System.Reflection;
using System.Globalization;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices.Core;
using StringComparison = System.StringComparison;
#if BRX_APP
using _AcAp = Bricscad.ApplicationServices;
using _AcCm = Teigha.Colors;
using _AcDb = Teigha.DatabaseServices;
using _AcEd = Bricscad.EditorInput;
using _AcGe = Teigha.Geometry;
using _AcGi = Teigha.GraphicsInterface;
using _AcGs = Teigha.GraphicsSystem;
using _AcPl = Bricscad.PlottingServices;
using _AcBrx = Bricscad.Runtime;
using _AcTrx = Teigha.Runtime;
using _AcWnd = Bricscad.Windows;
using _AcIntCom = BricscadDb;
using _AcInt = BricscadApp;
#elif ARX_APP
using _AcAp = Autodesk.AutoCAD.ApplicationServices;
using _AcBr = Autodesk.AutoCAD.BoundaryRepresentation;
using _AcCm = Autodesk.AutoCAD.Colors;
using _AcDb = Autodesk.AutoCAD.DatabaseServices;
using _AcEd = Autodesk.AutoCAD.EditorInput;
using _AcGe = Autodesk.AutoCAD.Geometry;
using _AcGi = Autodesk.AutoCAD.GraphicsInterface;
using _AcGs = Autodesk.AutoCAD.GraphicsSystem;
using _AcPl = Autodesk.AutoCAD.PlottingServices;
using _AcBrx = Autodesk.AutoCAD.Runtime;
using _AcTrx = Autodesk.AutoCAD.Runtime;
using _AcWnd = Autodesk.AutoCAD.Windows;
using _AcIntCom = Autodesk.AutoCAD.Interop.Common;
using _AcInt = Autodesk.AutoCAD.Interop;
using Excel = Microsoft.Office.Interop.Excel;
// ReSharper disable CommentTypo
// ReSharper disable StringLiteralTypo
// ReSharper disable IdentifierTypo


namespace Plan2Ext.AttTrans
{
    internal class Engine
    {
        #region log4net Initialization
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(Convert.ToString((typeof(LayTrans.Engine))));
        #endregion

        public readonly List<string> Errors = new List<string>();

        // ReSharper disable once StringLiteralTypo
        private readonly List<string> _header = new List<string>() { "Blockname", "Alter Name", "Neuer Name", "Alte Eingabe", "Neue Eingabe" };

        private readonly List<AttInfo> _attInfos;

        public Engine() { }

        public Engine(string fileName)
        {
            _attInfos = ExcelImport(fileName).Where(TagOrPromptChanges).ToList();
            if (_attInfos == null) throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Fehler beim Auslesen der Exceldatei!"));

        }

        private bool TagOrPromptChanges(AttInfo attInfo)
        {
            return (string.Compare(attInfo.OldAttTag, attInfo.NewAttTag, StringComparison.OrdinalIgnoreCase) != 0) ||
                   (string.Compare(attInfo.OldAttPrompt, attInfo.NewAttPrompt, StringComparison.OrdinalIgnoreCase) != 0);
        }

        internal bool AttTrans()
        {
            if (_attInfos == null) return false;

            Globs.UnlockAllLayers();

            Errors.Clear();
            var blockInfos = _attInfos.GroupBy(x => x.BlockName.ToUpperInvariant()).Where(IsValidBlockInfo).ToList();
            foreach (var err in Errors)
            {
                Log.Warn(err);
            }

            var doc = Application.DocumentManager.MdiActiveDocument;
            foreach (var blockInfo in blockInfos)
            {
                try
                {
                    var blockName = blockInfo.Key;
                    if (ChangeAttsInBlockDefinition(blockName, doc, blockInfo))
                    {
                        CheckAttsInBlockreferences(doc, blockName, blockInfo);
                    }
                }
                catch (Exception ex)
                {
                    var msg = string.Format(CultureInfo.CurrentCulture, "Fehler bei Block '{0}'! {1}", blockInfo.Key,
                        ex.Message);
                    Errors.Add(msg);
                    Log.WarnFormat(msg);
                }
            }

            return Errors.Count == 0;
        }

        private bool IsValidBlockInfo(IGrouping<string, AttInfo> attInfos)
        {
            var nrElements = attInfos.Count();
            var nrNewTags = attInfos.Select(x => x.NewAttTag.ToUpperInvariant()).Distinct().Count();
            if (nrElements != nrNewTags)
            {
                Errors.Add(string.Format(CultureInfo.CurrentCulture, "Gleiche neue Attributnamen in Definition für Block '{0}'.", attInfos.Key));
                return false;
            }
            return true;
        }

        private static void CheckAttsInBlockreferences(_AcAp.Document doc, string blockName, IGrouping<string, AttInfo> blockInfo)
        {
            using (_AcDb.Transaction trans = doc.TransactionManager.StartTransaction())
            {
                _AcDb.BlockTable blockTable = (_AcDb.BlockTable)trans.GetObject(doc.Database.BlockTableId, _AcDb.OpenMode.ForRead);
                var oid = blockTable[blockName];
                _AcDb.BlockTableRecord blockTableRecord =
                    (_AcDb.BlockTableRecord)trans.GetObject(oid, _AcDb.OpenMode.ForWrite);


                var references = GetBlockReferences(trans, blockTable, blockName);
                foreach (var blockReference in references)
                {
                    foreach (var oid2 in blockTableRecord)
                    {
                        var attDef = trans.GetObject(oid2, _AcDb.OpenMode.ForRead) as _AcDb.AttributeDefinition;
                        if (attDef != null)
                        {
                            if (!attDef.Constant)
                            {
                                AttInfo theAttInfo = null;
                                foreach (var attInfo in blockInfo)
                                {
                                    if (string.Compare(attInfo.NewAttTag, attDef.Tag,
                                            StringComparison.OrdinalIgnoreCase) == 0)
                                    {
                                        theAttInfo = attInfo;
                                        break;
                                    }
                                }

                                if (theAttInfo == null) continue;

                                _AcDb.AttributeReference theAttRef = null;
                                // Attribute neu erzeugen, Eigenschaften von alten Attributen kopieren und alte Attribute löschen.
                                foreach (_AcDb.ObjectId attId in blockReference.AttributeCollection)
                                {
                                    var anyAttRef = trans.GetObject(attId, _AcDb.OpenMode.ForRead) as _AcDb.AttributeReference;
                                    if (anyAttRef != null)
                                    {
                                        if (string.Compare(anyAttRef.Tag, theAttInfo.OldAttTag,
                                                StringComparison.OrdinalIgnoreCase) == 0)
                                        {
                                            theAttRef = anyAttRef;
                                            break;
                                        }
                                    }
                                }

                                if (theAttRef == null) continue;


                                using (_AcDb.AttributeReference attRef = new _AcDb.AttributeReference())
                                {
                                    blockReference.UpgradeOpen();

                                    attRef.SetAttributeFromBlock(attDef, blockReference.BlockTransform);
                                    attRef.Position =
                                        theAttRef.Position; //  attRef.Position.TransformBy(blockReference.BlockTransform);
                                    attRef.Rotation = theAttRef.Rotation;
                                    attRef.Height = theAttRef.Height;
                                    attRef.HorizontalMode = theAttRef.HorizontalMode;
                                    attRef.Justify = theAttRef.Justify;
                                    attRef.VerticalMode = theAttRef.VerticalMode;


                                    attRef.TextString = theAttRef.TextString; // attRef.TextString;

                                    blockReference.AttributeCollection.AppendAttribute(attRef);

                                    trans.AddNewlyCreatedDBObject(attRef, true);

                                    blockReference.DowngradeOpen();

                                    theAttRef.UpgradeOpen();
                                    theAttRef.Erase(true);
                                    theAttRef.DowngradeOpen();
                                }
                            }
                        }
                    }
                }

                trans.Commit();
            }
        }


        private static List<_AcDb.BlockReference> GetBlockReferences(_AcDb.Transaction trans, _AcDb.BlockTable blockTable, string blockName)
        {
            List<_AcDb.BlockReference> references = new List<_AcDb.BlockReference>();
            _AcDb.BlockTableRecord btr =
                (_AcDb.BlockTableRecord)trans.GetObject(blockTable[_AcDb.BlockTableRecord.ModelSpace], _AcDb.OpenMode.ForRead);
            foreach (_AcDb.ObjectId objId in btr)
            {
                _AcDb.BlockReference br = trans.GetObject(objId, _AcDb.OpenMode.ForWrite) as _AcDb.BlockReference;
                if (br != null)
                {
                    var bn = Globs.GetBlockname(br, trans);
                    if (string.Compare(bn, blockName, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        references.Add(br);
                    }
                }
            }

            return references;
        }

        private bool ChangeAttsInBlockDefinition(string blockName, _AcAp.Document doc,
            IGrouping<string, AttInfo> blockInfo)
        {

            using (_AcDb.Transaction trans = doc.TransactionManager.StartTransaction())
            {
                _AcDb.BlockTable blockTable = (_AcDb.BlockTable)trans.GetObject(doc.Database.BlockTableId, _AcDb.OpenMode.ForRead);
                if (!blockTable.Has(blockName))
                {
                    trans.Abort();
                    return false;
                }

                var oid = blockTable[blockName];
                _AcDb.BlockTableRecord blockTableRecord = (_AcDb.BlockTableRecord)trans.GetObject(oid, _AcDb.OpenMode.ForRead);

                // check if valid change of attributes
                var origNames = new List<string>();
                var newNames = new List<string>();
                foreach (var attOid in blockTableRecord)
                {
                    _AcDb.AttributeDefinition attDef = attOid.GetObject(_AcDb.OpenMode.ForRead) as _AcDb.AttributeDefinition;
                    if (attDef != null)
                    {
                        origNames.Add(attDef.Tag.ToUpperInvariant());
                        var attInfo = blockInfo.FirstOrDefault(x =>
                            string.Compare(x.OldAttTag, attDef.Tag, StringComparison.OrdinalIgnoreCase) == 0);
                        if (attInfo != null) newNames.Add(attInfo.NewAttTag.ToUpperInvariant());
                        else newNames.Add(attDef.Tag.ToUpperInvariant());
                    }
                }
                if (origNames.Count != newNames.Distinct().Count())
                {
                    Errors.Add(String.Format(CultureInfo.CurrentCulture, "Ungültige Attributzuordnung für Block '{0}'!", blockName));
                    trans.Abort();
                    return false;
                }


                foreach (var attOid in blockTableRecord)
                {
                    _AcDb.AttributeDefinition attDef = attOid.GetObject(_AcDb.OpenMode.ForRead) as _AcDb.AttributeDefinition;
                    if (attDef != null)
                    {
                        foreach (var attInfo in blockInfo)
                        {
                            if (string.Compare(attInfo.OldAttTag, attDef.Tag, StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                attDef.UpgradeOpen();
                                attDef.Tag = attInfo.NewAttTag;
                                attDef.Prompt = attInfo.NewAttPrompt;
                                attDef.DowngradeOpen();
                                break;
                            }
                        }
                    }
                }
                trans.Commit();
            }

            return true;
        }

        private List<AttInfo> ExcelImport(string fileName)
        {
            Excel.Application myApp = new Excel.Application();
            Excel.Workbook workBook = null;
            Excel.Worksheet sheet = null;
            try
            {
                workBook = myApp.Workbooks.Open(fileName, Missing.Value, true); //, 5, "", "", true, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);
                // ReSharper disable once UseIndexedProperty
                sheet = workBook.Worksheets.get_Item(1);

                var biis = GetAttInfos(sheet);

                workBook.Close(false, Missing.Value, Missing.Value);
                myApp.Quit();

                return biis;

            }
            finally
            {
                ReleaseObject(sheet);
                ReleaseObject(workBook);
                ReleaseObject(myApp);
            }
        }

        private List<AttInfo> GetAttInfos(Excel.Worksheet sheet)
        {
            // test import
            int nrRows;
            var nrCols = _header.Count;
            GetNrRows(sheet, out nrRows);
            var b1 = GetCellBez(0, 0);
            var b2 = GetCellBez(nrRows, nrCols);
            var range = sheet.Range[b1, b2];
            // ReSharper disable once UseIndexedProperty
            object[,] impMatrix = range.get_Value(Excel.XlRangeValueDataType.xlRangeValueDefault);

            var doc = Application.DocumentManager.MdiActiveDocument;

            List<AttInfo> biis = new List<AttInfo>();
            using (_AcDb.Transaction trans = doc.TransactionManager.StartTransaction())
            {
                try
                {
                    for (int r = 2; r <= nrRows; r++)
                    {
                        for (int c = 1; c <= nrCols; c++)
                        {
                            if (impMatrix[r, c] == null) impMatrix[r, c] = "";
                        }
                    }
                    for (int r = 2; r <= nrRows; r++)
                    {
                        AttInfo attInfo = new AttInfo
                        {
                            BlockName = impMatrix[r, 1].ToString(),
                            OldAttTag = impMatrix[r, 2].ToString(),
                            NewAttTag = impMatrix[r, 3].ToString(),
                            OldAttPrompt = impMatrix[r, 4].ToString(),
                            NewAttPrompt = impMatrix[r, 5].ToString()
                        };

                        if (attInfo.Ok)
                        {
                            biis.Add(attInfo);
                        }
                        if (!string.IsNullOrEmpty(attInfo.Errors))
                        {
                            Errors.Add(attInfo.Errors);
                        }
                    }
                }
                finally
                {
                    trans.Commit();
                }
            }

            return biis;
        }

        internal bool ExcelExport()
        {
            Excel.Application myApp = null;
            Excel.Workbook workBook = null;
            Excel.Worksheet sheet = null;

            try
            {
                var attInfos = GetAttInfos();
                if (attInfos == null) return true;

                myApp = new Excel.Application();

                workBook = myApp.Workbooks.Add(Missing.Value);
                sheet = workBook.ActiveSheet;

                Excel.Range cells = sheet.Cells;
                cells.NumberFormat = "@";
                var b1 = GetCellBez(0, 0);
                var b2 = GetCellBez(0, _header.Count);
                var range = sheet.Range[b1, b2];
                range.Font.Bold = true;
                range.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;

                int rowCount = 1 + attInfos.Count;
                int colCount = _header.Count;
                b2 = GetCellBez(rowCount - 1, colCount - 1);
                range = sheet.Range[b1, b2];

                string[,] indexMatrix = new string[rowCount, colCount];
                for (int i = 0; i < _header.Count; i++)
                {
                    indexMatrix[0, i] = _header[i];
                }
                for (int r = 1; r <= attInfos.Count; r++)
                {
                    var blockInfo = attInfos[r - 1];
                    List<string> values = blockInfo.RowAsList();
                    for (int i = 0; i < values.Count; i++)
                    {
                        indexMatrix[r, i] = values[i];
                    }
                }

                // ReSharper disable once UseIndexedProperty
                range.set_Value(Excel.XlRangeValueDataType.xlRangeValueDefault, indexMatrix);

                range.Font.Name = "Arial";
                range.Columns.AutoFit();
            }
            finally
            {
                if (myApp != null)
                {
                    myApp.Visible = true;
                    myApp.ScreenUpdating = true;
                }

                ReleaseObject(sheet);
                ReleaseObject(workBook);
                ReleaseObject(myApp);
            }

            return true;
        }

        private const int Maxrows = 3000;
        private void GetNrRows(Excel.Worksheet sheet, out int nrRows)
        {
            nrRows = 0;

            var b1 = GetCellBez(0, 0);
            var b2 = GetCellBez(Maxrows, 0);
            Log.DebugFormat("B1 '{0}' und B2 '{1}", b1, b2);
            var range = sheet.Range[b1, b2];
            Log.DebugFormat("Nach getrange!");

            // ReSharper disable once UseIndexedProperty
            object[,] indexMatrix = range.get_Value(Excel.XlRangeValueDataType.xlRangeValueDefault);

            for (int i = 1; i <= Maxrows; i++)
            {
                var v1 = indexMatrix[i, 1];
                if (v1 == null) break;
                nrRows++;
            }
        }

        private List<AttInfo> GetAttInfos()
        {
            var blockNames = SelectBlocks();
            if (blockNames == null) return null;

            List<AttInfo> attInfos = new List<AttInfo>();
            var doc = Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;

            using (_AcDb.Transaction trans = doc.TransactionManager.StartTransaction())
            {
                try
                {
                    _AcDb.BlockTable blockTable = trans.GetObject(db.BlockTableId, _AcDb.OpenMode.ForRead) as _AcDb.BlockTable;
                    if (blockTable != null)
                    {
                        foreach (var blockOid in blockTable)
                        {
                            _AcDb.BlockTableRecord blockTableRecord = (_AcDb.BlockTableRecord)trans.GetObject(blockOid, _AcDb.OpenMode.ForRead);
                            if (blockTableRecord.IsAnonymous || blockTableRecord.IsFromExternalReference || blockTableRecord.IsDependent || blockTableRecord.IsLayout) continue;
                            if (blockNames.Contains(blockTableRecord.Name) && blockTableRecord.HasAttributeDefinitions)
                            {
                                foreach (var attOid in blockTableRecord)
                                {
                                    _AcDb.AttributeDefinition attDef = attOid.GetObject(_AcDb.OpenMode.ForRead) as _AcDb.AttributeDefinition;
                                    if (attDef != null)
                                    {
                                        attInfos.Add(new AttInfo(blockTableRecord, attDef));
                                    }
                                }
                            }
                        }
                    }
                }
                finally
                {
                    trans.Commit();
                }
            }

            return attInfos;
        }

        private static void ReleaseObject(object obj)
        {
            try
            {
                if (obj != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(obj);
            }
            catch (Exception)
            {
                // ignored
            }
            finally
            {
                GC.Collect();
            }
        }

        private static string GetCellBez(int rowIndex, int colIndex)
        {
            return TranslateColumnIndexToName(colIndex) + (rowIndex + 1).ToString(CultureInfo.InvariantCulture);
        }

        private static String TranslateColumnIndexToName(int index)
        {
            int quotient = (index) / 26;

            if (quotient > 0)
            {
                return TranslateColumnIndexToName(quotient - 1) + (char)((index % 26) + 65);
            }
            else
            {
                return "" + (char)((index % 26) + 65);
            }
        }

        private sealed class AttInfo
        {
            #region Lifecycle

            public AttInfo()
            {
                Ok = true;
            }
            public AttInfo(_AcDb.BlockTableRecord blockTableRecord, _AcDb.AttributeDefinition attributeDefinition)
            {
                Ok = true;
                BlockName = blockTableRecord.Name;
                OldAttTag = attributeDefinition.Tag;
                NewAttTag = attributeDefinition.Tag;
                OldAttPrompt = attributeDefinition.Prompt;
                NewAttPrompt = attributeDefinition.Prompt;
            }

            #endregion

            #region Internal
            internal List<string> RowAsList()
            {
                return new List<string>() { BlockName, OldAttTag, NewAttTag, OldAttPrompt, NewAttPrompt };
            }

            #endregion

            #region Properties
            private string _errors = string.Empty;
            public string Errors { get { return _errors; } }


            public string BlockName { get; set; }

            private string _oldAttTag = string.Empty;
            public string OldAttTag
            {
                get { return _oldAttTag; }
                set
                {
                    _oldAttTag = value;
                    if (String.IsNullOrEmpty(_oldAttTag))
                    {
                        Ok = false;
                        // ReSharper disable once StringLiteralTypo
                        _errors = _errors + string.Format(CultureInfo.CurrentCulture, "\nKein bestehender Attributname '{0}'", _oldAttTag);
                    }
                }
            }
            private string _newAttTag = string.Empty;
            public string NewAttTag
            {
                get { return _newAttTag; }
                set
                {
                    _newAttTag = value;
                    if (String.IsNullOrEmpty(_newAttTag))
                    {
                        Ok = false;
                        // ReSharper disable once StringLiteralTypo
                        _errors = _errors + string.Format(CultureInfo.CurrentCulture, "\nKein neuer Name für Attribut '{0}'", OldAttTag);
                    }
                }
            }

            private string _oldAttPrompt = string.Empty;
            public string OldAttPrompt
            {
                get { return _oldAttPrompt; }
                set
                {
                    _oldAttPrompt = value;
                }
            }
            private string _newAttPrompt = string.Empty;
            public string NewAttPrompt
            {
                get { return _newAttPrompt; }
                set
                {
                    _newAttPrompt = value;
                }
            }

            public bool Ok { get; private set; }

            #endregion
        }

        private List<string> SelectBlocks()
        {
            var blockNames = new List<string>();

            var ed = Application.DocumentManager.MdiActiveDocument.Editor;
            _AcEd.SelectionFilter filter = new _AcEd.SelectionFilter(new[] { 
                new _AcDb.TypedValue((int)_AcDb.DxfCode.Start,"INSERT" ),
            });

            _AcEd.PromptSelectionResult res = ed.GetSelection(filter); // ed.SelectAll(filter);
            if (res.Status != _AcEd.PromptStatus.OK) return null;

            List<_AcDb.ObjectId> selectedBlocks = new List<_AcDb.ObjectId>();
#if BRX_APP
            _AcEd.SelectionSet ss = res.Value;
#else
            using (_AcEd.SelectionSet ss = res.Value)
#endif
            {
                selectedBlocks.AddRange(ss.GetObjectIds());
            }

            var doc = Application.DocumentManager.MdiActiveDocument;
            using (var trans = doc.TransactionManager.StartTransaction())
            {
                var nonXrefs = selectedBlocks.Where(oid => !IsXRef(oid, trans)).ToList();
                foreach (var oid in nonXrefs)
                {
                    var blockRef = trans.GetObject(oid, _AcDb.OpenMode.ForRead) as _AcDb.BlockReference;
                    if (blockRef != null)
                    //if (blockRef != null && !blockRef.IsDynamicBlock)
                    {
                        var name = Globs.GetBlockname(blockRef, trans);
                        if (!blockNames.Contains(name))
                        {
                            blockNames.Add(name);
                        }
                    }
                }

                trans.Commit();
            }

            return blockNames;
        }
        private bool IsXRef(_AcDb.ObjectId oid, _AcDb.Transaction tr)
        {
            var br = tr.GetObject(oid, _AcDb.OpenMode.ForRead) as _AcDb.BlockReference;
            if (br != null)
            {
                var bd = (_AcDb.BlockTableRecord)tr.GetObject(br.BlockTableRecord, _AcDb.OpenMode.ForRead);
                if (bd.IsFromExternalReference) return true;
            }
            return false;
        }

    }
}
#endif
