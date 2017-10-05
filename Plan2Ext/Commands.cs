using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//using Autodesk.AutoCAD.Runtime;
//using Autodesk.AutoCAD.DatabaseServices;
//using Autodesk.AutoCAD.EditorInput;
using System.Globalization;
using System.Diagnostics;
using Microsoft.Win32.TaskScheduler;

#if BRX_APP
using _AcAp = Bricscad.ApplicationServices;
//using _AcBr = Teigha.BoundaryRepresentation;
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
using _AcLm = Autodesk.AutoCAD.LayerManager;
#endif

namespace Plan2Ext
{
    public class Commands
    {
        #region log4net Initialization
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Convert.ToString((typeof(Commands))));
        #endregion

#if ARX_APP
        /// <summary>
        /// http://adndevblog.typepad.com/autocad/2012/05/creating-and-accessing-layer-filter-information.html
        /// </summary>
        [_AcTrx.CommandMethod("CreateLayerFilter")]
        public void CreateLayerFilter()
        {
            _AcAp.Document doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            _AcDb.Database db = doc.Database;
            _AcEd.Editor ed = doc.Editor;

            _AcLm.LayerFilterTree filterTree = db.LayerFilters;
            _AcLm.LayerFilterCollection filters = filterTree.Root.NestedFilters;
            // Lets find out about the root filter
            ed.WriteMessage(String.Format("\n(Root) Name : {0} Expr : {1}", db.LayerFilters.Root.Name,
                        db.LayerFilters.Root.FilterExpression)
                    );
            // Lets find out about all the filters
            foreach (_AcLm.LayerFilter f in filters)
            {
                ed.WriteMessage(String.Format("\nName : {0} Expr : {1}", f.Name,
                            f.FilterExpression)
                        );
            }
            // Lets find out about the current filter
            if (db.LayerFilters.Current != null)
            {
                ed.WriteMessage(String.Format("\n(Current) Name : {0} Expr : {1}",
                    db.LayerFilters.Current.Name,
                    db.LayerFilters.Current.FilterExpression)
                );
            }

            // Create and add a new layer filter
            _AcLm.LayerFilter layerFilter = new _AcLm.LayerFilter();
            layerFilter.Name = "MyLyFilter";
            layerFilter.FilterExpression = "NAME == \"*Test*\"";
            filters.Add(layerFilter);

            // Set the changed layer filters tree to the database
            db.LayerFilters = filterTree;
        }
#endif

        /// <summary>
        /// Öffnen einer Dwg ohne Editor
        /// </summary>
        //[_AcTrx.CommandMethod("Plan2TestSideDb")]
        static public void Plan2TestSideDb()
        {
            _AcAp.Document doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            _AcEd.Editor ed = doc.Editor;
            // Ask the user to select a file
            _AcEd.PromptResult res = ed.GetString("\nEnter the path of a DWG or DXF file: ");
            if (res.Status == _AcEd.PromptStatus.OK)
            {
                // Create a database and try to load the file
                _AcDb.Database db = new _AcDb.Database(false, true);
                using (db)
                {
                    try
                    {
                        db.ReadDwgFile(res.StringResult, System.IO.FileShare.Read, false, "");
                    }
                    catch (System.Exception)
                    {
                        ed.WriteMessage("\nUnable to read drawing file.");
                        return;
                    }

                    _AcDb.Transaction tr = db.TransactionManager.StartTransaction();
                    using (tr)
                    {
                        // Open the blocktable, get the modelspace
                        _AcDb.BlockTable bt = (_AcDb.BlockTable)tr.GetObject(db.BlockTableId, _AcDb.OpenMode.ForRead);
                        _AcDb.BlockTableRecord btr = (_AcDb.BlockTableRecord)tr.GetObject(bt[_AcDb.BlockTableRecord.ModelSpace], _AcDb.OpenMode.ForRead);

                        // Iterate through it, dumping objects
                        foreach (_AcDb.ObjectId objId in btr)
                        {
                            _AcDb.Entity ent = (_AcDb.Entity)tr.GetObject(objId, _AcDb.OpenMode.ForRead);

                            // Let's get rid of the standard namespace
                            const string prefix = "Autodesk.AutoCAD.DatabaseServices.";
                            string typeString = ent.GetType().ToString();
                            if (typeString.Contains(prefix)) typeString = typeString.Substring(prefix.Length);
                            ed.WriteMessage("\nEntity " + ent.ObjectId.ToString() + " of type " + typeString + " found on layer " +
                                                ent.Layer + " with colour " + ent.Color.ToString());
                        }
                    }
                }
            }
        }

        [_AcTrx.CommandMethod("Plan2DynamicBlockProps")]
        static public void DynamicBlockProps()
        {

            _AcAp.Document doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            _AcDb.Database db = doc.Database;
            _AcEd.Editor ed = doc.Editor;

            _AcEd.PromptStringOptions pso = new _AcEd.PromptStringOptions("\nEnter dynamic block name or enter to select: ");
            pso.AllowSpaces = true;
            _AcEd.PromptResult pr = ed.GetString(pso);

            if (pr.Status != _AcEd.PromptStatus.OK) return;

            _AcDb.Transaction tr = db.TransactionManager.StartTransaction();
            using (tr)
            {
                _AcDb.BlockReference br = null;
                // If a null string was entered allow entity selection
                if (pr.StringResult == "")
                {
                    // Select a block reference
                    _AcEd.PromptEntityOptions peo = new _AcEd.PromptEntityOptions("\nSelect dynamic block reference: ");
                    peo.SetRejectMessage("\nEntity is not a block.");
                    peo.AddAllowedClass(typeof(_AcDb.BlockReference), false);

                    _AcEd.PromptEntityResult per = ed.GetEntity(peo);
                    if (per.Status != _AcEd.PromptStatus.OK) return;

                    // Access the selected block reference
                    br = tr.GetObject(per.ObjectId, _AcDb.OpenMode.ForRead) as _AcDb.BlockReference;
                }

                else
                {
                    // Otherwise we look up the block by name
                    _AcDb.BlockTable bt = tr.GetObject(db.BlockTableId, _AcDb.OpenMode.ForRead) as _AcDb.BlockTable;
                    if (!bt.Has(pr.StringResult))
                    {
                        ed.WriteMessage("\nBlock \"" + pr.StringResult + "\" does not exist.");
                        return;

                    }

                    // Create a new block reference referring to the block
                    br = new _AcDb.BlockReference(new _AcGe.Point3d(), bt[pr.StringResult]);
                }

                _AcDb.BlockTableRecord btr = (_AcDb.BlockTableRecord)tr.GetObject(br.DynamicBlockTableRecord, _AcDb.OpenMode.ForRead);

                // Call our function to display the block properties
                DisplayDynBlockProperties(ed, br, btr.Name);

                // Committing is cheaper than aborting
                tr.Commit();
            }
        }

        private static void DisplayDynBlockProperties(_AcEd.Editor ed, _AcDb.BlockReference br, string name)
        {
            // Only continue is we have a valid dynamic block
            if (br != null && br.IsDynamicBlock)
            {
                ed.WriteMessage("\nDynamic properties for \"{0}\"\n", name);

                // Get the dynamic block's property collection
                _AcDb.DynamicBlockReferencePropertyCollection pc = br.DynamicBlockReferencePropertyCollection;

                // Loop through, getting the info for each property
                foreach (_AcDb.DynamicBlockReferenceProperty prop in pc)
                {
                    // Start with the property name, type and description
                    ed.WriteMessage("\nProperty: \"{0}\" : {1}", prop.PropertyName, prop.UnitsType);
                    if (prop.Description != "") ed.WriteMessage("\n  Description: {0}", prop.Description);

                    // Is it read-only?
                    if (prop.ReadOnly) ed.WriteMessage(" (Read Only)");

                    // Get the allowed values, if it's constrained
                    bool first = true;
                    foreach (object value in prop.GetAllowedValues())
                    {
                        ed.WriteMessage((first ? "\n  Allowed values: [" : ", "));
                        ed.WriteMessage("\"{0}\"", value);
                        first = false;
                    }

                    if (!first) ed.WriteMessage("]");

                    // And finally the current value
                    ed.WriteMessage("\n  Current value: \"{0}\"\n", prop.Value);
                }
            }
        }

        [_AcTrx.CommandMethod("ViewTest")]
        public static void ViewTest()
        {
            Globs.SetWorldView();
        }
        //[_AcTrx.CommandMethod("SaveView")]
        //public static void SaveView()
        //{
        //    Globs.SaveView("Test");
        //}
        //[_AcTrx.CommandMethod("RestoreView")]
        //public static void RestoreView()
        //{
        //    Globs.RestoreView("Test");
        //}        

        //[_AcTrx.CommandMethod("TestMakeLayer")]
        public void TestMakeLayer()
        {
            var doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            var layerOid = CreateNewLayer(doc, db);

            using (_AcDb.Transaction trans = doc.TransactionManager.StartTransaction())
            {
                _AcDb.BlockTable blkTable = (_AcDb.BlockTable)trans.GetObject(db.BlockTableId, _AcDb.OpenMode.ForRead);
                foreach (var id in blkTable)
                {
                    _AcDb.BlockTableRecord btRecord = (_AcDb.BlockTableRecord)trans.GetObject(id, _AcDb.OpenMode.ForRead);
                    //if (!btRecord.IsLayout)
                    //{
                    //Access to the block (not model/paper space)
                    System.Diagnostics.Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Block: {0}", btRecord.Name));
                    //MgdAcApplication.DocumentManager.MdiActiveDocument.Editor.WriteMessage(string.Format("\nBlock name: {0}", btRecord.Name));

                    btRecord.UpgradeOpen();

                    foreach (var entId in btRecord)
                    {
                        _AcDb.Entity entity = (_AcDb.Entity)trans.GetObject(entId, _AcDb.OpenMode.ForRead);

                        entity.UpgradeOpen();

                        //Access to the entity
                        //MgdAcApplication.DocumentManager.MdiActiveDocument.Editor.WriteMessage(string.Format("\nHandle: {0}", entity.Handle));
                        //System.Diagnostics.Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Handle: {0}, Layer: {1}", entity.Handle, entity.Layer));
                        entity.Layer = "MyTest";

                        _AcDb.BlockReference block = entity as _AcDb.BlockReference;
                        if (block != null)
                        {
                            foreach (var att in block.AttributeCollection)
                            {
                                _AcDb.ObjectId attOid = (_AcDb.ObjectId)att;
                                _AcDb.AttributeReference attrib = trans.GetObject(attOid, _AcDb.OpenMode.ForWrite) as _AcDb.AttributeReference;
                                if (attrib != null)
                                {
                                    //attrib.UpgradeOpen();
                                    attrib.Layer = "MyTest";
                                }
                            }

                        }
                    }

                    //}

                }

                // Layouts iterieren
                _AcDb.DBDictionary layoutDict = (_AcDb.DBDictionary)trans.GetObject(db.LayoutDictionaryId, _AcDb.OpenMode.ForRead);
                foreach (var loEntry in layoutDict)
                {
                    if (loEntry.Key.ToUpperInvariant() == "MODEL") continue;
                    _AcDb.Layout lo = (_AcDb.Layout)trans.GetObject(loEntry.Value, _AcDb.OpenMode.ForRead, false);
                    if (lo == null) continue;
                    System.Diagnostics.Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Layout: {0}", lo.LayoutName));

                    _AcDb.BlockTableRecord btRecord = (_AcDb.BlockTableRecord)trans.GetObject(lo.BlockTableRecordId, _AcDb.OpenMode.ForRead);
                    foreach (var entId in btRecord)
                    {
                        _AcDb.Entity entity = (_AcDb.Entity)trans.GetObject(entId, _AcDb.OpenMode.ForRead);

                        entity.UpgradeOpen();

                        //Access to the entity
                        //MgdAcApplication.DocumentManager.MdiActiveDocument.Editor.WriteMessage(string.Format("\nHandle: {0}", entity.Handle));
                        //System.Diagnostics.Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Handle: {0}, Layer: {1}", entity.Handle, entity.Layer));
                        entity.Layer = "MyTest";

                        _AcDb.BlockReference block = entity as _AcDb.BlockReference;
                        if (block != null)
                        {
                            foreach (var att in block.AttributeCollection)
                            {
                                _AcDb.ObjectId attOid = (_AcDb.ObjectId)att;
                                _AcDb.AttributeReference attrib = trans.GetObject(attOid, _AcDb.OpenMode.ForWrite) as _AcDb.AttributeReference;
                                if (attrib != null)
                                {
                                    //attrib.UpgradeOpen();
                                    attrib.Layer = "MyTest";
                                }
                            }

                        }

                    }

                }



                trans.Commit();
            }

        }

        private _AcDb.ObjectId CreateNewLayer(_AcAp.Document doc, _AcDb.Database db)
        {
            using (_AcDb.Transaction trans = doc.TransactionManager.StartTransaction())
            {
                try
                {
                    string layerName = "MyTest";
                    _AcDb.LayerTable layTb = trans.GetObject(db.LayerTableId, _AcDb.OpenMode.ForRead) as _AcDb.LayerTable;
                    using (_AcDb.LayerTableRecord acLyrTblRec = new _AcDb.LayerTableRecord())
                    {
                        // Assign the layer a name
                        acLyrTblRec.Name = layerName;

                        // Upgrade the Layer table for write
                        layTb.UpgradeOpen();


                        // Append the new layer to the Layer table and the transaction
                        layTb.Add(acLyrTblRec);
                        trans.AddNewlyCreatedDBObject(acLyrTblRec, true);


                        int transparenz = 10;

                        Byte alpha = TransparenzToAlpha(transparenz);
                        _AcCm.Transparency tr = new _AcCm.Transparency(alpha);
                        acLyrTblRec.Transparency = tr;

                        _AcCm.Color col = _AcCm.Color.FromColorIndex(_AcCm.ColorMethod.ByColor, 2);
                        //_AcCm.Color col = _AcCm.Color.FromRgb(10, 20, 30);
                        acLyrTblRec.Color = col;

                        _AcDb.ObjectId ltOid = GetLinetypeFromName("Continuous", trans, db);
                        if (!ltOid.IsNull)
                        {
                            acLyrTblRec.LinetypeObjectId = ltOid;
                        }

                        _AcDb.LineWeight lw = _AcDb.LineWeight.LineWeight030;
                        acLyrTblRec.LineWeight = lw;

                        // ???
                        //acLyrTblRec.PlotStyleName = "hugo";

                        acLyrTblRec.Description = "My new Layer";

                        return acLyrTblRec.ObjectId;

                    }
                }
                finally
                {
                    trans.Commit();
                }
            }
        }

        private _AcDb.ObjectId GetLinetypeFromName(string name, _AcDb.Transaction trans, _AcDb.Database db)
        {
            _AcDb.LinetypeTable acLinTbl;
            acLinTbl = trans.GetObject(db.LinetypeTableId,
                                            _AcDb.OpenMode.ForRead) as _AcDb.LinetypeTable;

            if (acLinTbl.Has(name)) return acLinTbl[name];
            else return default(_AcDb.ObjectId);
        }


        private byte TransparenzToAlpha(int transparenz)
        {
            return (Byte)(255 * (100 - transparenz) / 100);
        }


        [_AcTrx.LispFunction("Plan2FinishPlot")]
        public static bool Plan2FinishPlot(_AcDb.ResultBuffer rb)
        {
            _AcEd.Editor ed = _AcAp.Application.DocumentManager.MdiActiveDocument.Editor;
            try
            {
                if (rb == null) return false;
                _AcDb.TypedValue[] values = rb.AsArray();
                if (values == null || values.Length < 1) return false;
                if (values[0].Value == null) return false;
                string dirName = values[0].Value.ToString();
                if (string.IsNullOrEmpty(dirName)) return false;
                if (!System.IO.Directory.Exists(dirName)) return false;

                return Plan2FinishPlot(dirName);

            }
            catch (System.Exception ex)
            {
                string msg = string.Format(CultureInfo.CurrentCulture, "Fehler in (Plan2FinishPlot): {0}", ex.Message);
                ed.WriteMessage("\n" + msg);
                System.Windows.Forms.MessageBox.Show(ex.Message, "Plan2FinishPlot");
                return false;
            }
        }

        private static bool Plan2FinishPlot(string dirName)
        {
            //dirName = System.IO.Path.Combine(dirName, "PDF");

            const string prefix = "PDF";
            string targetDir = System.IO.Path.Combine(dirName, prefix);
            if (!System.IO.Directory.Exists(targetDir))
            {
                System.IO.Directory.CreateDirectory(targetDir);
            }

            var subdirs = System.IO.Directory.GetDirectories(targetDir, prefix + "*", System.IO.SearchOption.TopDirectoryOnly);

            foreach (var tempDir in subdirs)
            {
                string fName = System.IO.Path.GetFileName(tempDir);
                if (!fName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) continue; // ignore other dirs

                var pdfFiles = System.IO.Directory.GetFiles(tempDir, "*.pdf", System.IO.SearchOption.TopDirectoryOnly);
                foreach (var pdf in pdfFiles)
                {
                    System.IO.File.Copy(pdf, System.IO.Path.Combine(targetDir, System.IO.Path.GetFileName(pdf)), true);
                    System.IO.File.Delete(pdf);
                }

                RecRemoveEmptyDir(tempDir);

            }

            return true;
        }

        private static void RecRemoveEmptyDir(string tempDir)
        {
            var subdirs = System.IO.Directory.GetDirectories(tempDir, "*.*", System.IO.SearchOption.TopDirectoryOnly);
            foreach (var sd in subdirs)
            {
                RecRemoveEmptyDir(sd);
            }

            if (System.IO.Directory.GetFiles(tempDir, "*.*", System.IO.SearchOption.TopDirectoryOnly).Length == 0 &&
                System.IO.Directory.GetDirectories(tempDir, "*.*", System.IO.SearchOption.TopDirectoryOnly).Length == 0)
            {
                System.IO.Directory.Delete(tempDir);
            }
        }

        /// <summary>
        /// (Layer, Length?, Angle?, Colorindex?, LB, p1, p2, ..., LE)
        /// </summary>
        /// <param name="rb"></param>
        /// <remarks>
        /// Length, Angle, Colorindex not supported yet.
        /// </remarks>

        [_AcTrx.LispFunction("Plan2InsertFehlerLines")]
        public static void Plan2InsertFehlerLines(_AcDb.ResultBuffer rb)
        {
            var arr = rb.AsArray();
            if (arr.Length < 6) return;
            string layer = arr[0].Value.ToString();
            // length 1
            // angle 2
            // colorindex 3
            // LB 4

            List<_AcGe.Point3d> pts = new List<_AcGe.Point3d>();
            for (int i = 5; i < arr.Length - 1; i++)
            {
                pts.Add((_AcGe.Point3d)arr[i].Value);
            }
            Globs.InsertFehlerLines(pts, layer);
        }

        [_AcTrx.LispFunction("InoNewGuid")]
        public static _AcDb.ResultBuffer InoGuid(_AcDb.ResultBuffer rb)
        {
            string guidStr = System.Guid.NewGuid().ToString();
            return new _AcDb.ResultBuffer(new _AcDb.TypedValue((int)_AcBrx.LispDataType.Text, guidStr));
        }

        [_AcTrx.LispFunction("GetNonPlottableLayers")]
        public static _AcDb.ResultBuffer GetNonPlottableLayers(_AcDb.ResultBuffer rb)
        {
            List<string> layerNames = new List<string>();
            Globs.GetNonPlottableLayers(layerNames);

            if (layerNames.Count > 0)
            {
                _AcDb.ResultBuffer rbRet = new _AcDb.ResultBuffer();
                foreach (var name in layerNames)
                {
                    rbRet.Add(new _AcDb.TypedValue((int)_AcBrx.LispDataType.Text, name));
                }
                return rbRet;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Calls Process with Arguments
        /// </summary>
        /// <param name="rb">
        /// First param is the filename of the command.
        /// Second param are the arguments separated by whitespace. Can be missing or empty.
        /// </param>
        /// <example>
        /// Lispcall: (inocallprocess "d:\\temp\\plan²\\cop.bat" "d:\\temp\\plan²\\alx.txt d:\\temp\\plan²\\alx3.txt")
        /// </example>
        [_AcTrx.LispFunction("InoCallProcess")]
        public static void InoCallProcess(_AcDb.ResultBuffer rb)
        {
            try
            {
                if (rb == null) return;
                _AcDb.TypedValue[] values = rb.AsArray();
                if (values == null || values.Length < 1) return;
                if (values[0].Value == null) return;
                string fileName = values[0].Value.ToString();
                if (string.IsNullOrEmpty(fileName)) return;

                string args = string.Empty;
                if (values.Length > 1)
                {
                    args = values[1].Value.ToString();
                }

                // Simple variante
                //if (string.IsNullOrEmpty(args))
                //{
                //    System.Diagnostics.Process.Start(fileName);
                //}
                //else
                //{
                //    System.Diagnostics.Process.Start(fileName, args);
                //}

                _AcEd.Editor ed = _AcAp.Application.DocumentManager.MdiActiveDocument.Editor;

                // Variante with output
                int exitCode = -1;
                // -> async writings to editor are ignored
                //ExecuteCommandAsyncOut(fileName, args, ref exitCode, ed); 
                string error = string.Empty;
                string output = string.Empty;

                log.InfoFormat(CultureInfo.InvariantCulture, "InoCallProcess: args='{0}', filename='{1}'.", args, fileName);

                ExecuteCommandSyncOut(fileName, args, ref error, ref output, ref exitCode);
                if (!string.IsNullOrEmpty(output))
                {
                    ed.WriteMessage("\n" + output);
                    log.Info(output);
                }
                if (!string.IsNullOrEmpty(error))
                {
                    ed.WriteMessage("\nError>>" + error);
                    log.Warn("\nError>>" + error);
                }

            }
            catch (System.Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(string.Format(CultureInfo.CurrentCulture, "Fehler in CallProcess: {0}", ex.Message), "CallProcess");
            }
        }

        private static void ExecuteCommandAsyncOut(string command, string args, ref int exitCode, _AcEd.Editor ed)
        {
            ProcessStartInfo ProcessInfo;
            Process process;

            //string sap = Application.StartupPath;

            ProcessInfo = new ProcessStartInfo(command, args);
            ProcessInfo.CreateNoWindow = true;
            ProcessInfo.UseShellExecute = false;
            //ProcessInfo.WorkingDirectory = Application.StartupPath + "\\txtmanipulator";
            // *** Redirect the output ***
            ProcessInfo.RedirectStandardError = true;
            ProcessInfo.RedirectStandardOutput = true;

            using (process = Process.Start(ProcessInfo))
            {
                process.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data)) ed.WriteMessage("\n" + e.Data);
                };
                process.BeginOutputReadLine();

                process.ErrorDataReceived += (object sender, DataReceivedEventArgs e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data)) ed.WriteMessage("\nError>>" + e.Data);
                };
                process.BeginErrorReadLine();

                process.WaitForExit();

                exitCode = process.ExitCode;

            }
        }

        private static void ExecuteCommandSyncOut(string command, string args, ref string error, ref string output, ref int exitCode)
        {
            ProcessStartInfo ProcessInfo;
            Process process;

            //string sap = Application.StartupPath;

            ProcessInfo = new ProcessStartInfo(command, args);
            ProcessInfo.CreateNoWindow = true;
            ProcessInfo.UseShellExecute = false;
            //ProcessInfo.WorkingDirectory = Application.StartupPath + "\\txtmanipulator";
            // *** Redirect the output ***
            ProcessInfo.RedirectStandardError = true;
            ProcessInfo.RedirectStandardOutput = true;

            using (process = Process.Start(ProcessInfo))
            {
                process.WaitForExit();

                // *** Read the streams ***
                output = process.StandardOutput.ReadToEnd();
                error = process.StandardError.ReadToEnd();

                exitCode = process.ExitCode;

                //MessageBox.Show("output>>" + (String.IsNullOrEmpty(output) ? "(none)" : output));
                //MessageBox.Show("error>>" + (String.IsNullOrEmpty(error) ? "(none)" : error));
                //MessageBox.Show("ExitCode: " + xitCode.ToString(), "ExecuteCommand");
                //process.Close();
            }
        }

        [_AcTrx.LispFunction("CreateCallCropBat")]
        public static bool CreateCallCropBat(_AcDb.ResultBuffer rb)
        {
            const string cropBatName = "CallCrop.BAT";
            _AcEd.Editor ed = _AcAp.Application.DocumentManager.MdiActiveDocument.Editor;


            try
            {

                if (rb == null)
                {
                    ed.WriteMessage("\nAufruf: (CreateCallCropBat CropBatDir PdfDir)");
                    return false;
                }
                _AcDb.TypedValue[] values = rb.AsArray();
                if (values == null || values.Length < 2 || values[0].Value == null || values[1] == null)
                {
                    ed.WriteMessage("\nAufruf: (CreateCallCropBat CropBatDir PdfDir)");
                    return false;
                }

                string cropBatDir = values[0].Value.ToString();
                string pdfDir = values[1].Value.ToString();
                if (string.IsNullOrEmpty(cropBatDir) || !System.IO.Directory.Exists(cropBatDir))
                {
                    ed.WriteMessage(string.Format(CultureInfo.CurrentCulture, "\n(CreateCallCropBat '{0}' '{1}'): Verzeichnis '{0}' existiert nicht!", cropBatDir, pdfDir));
                    return false;
                }
                if (string.IsNullOrEmpty(pdfDir) || !System.IO.Directory.Exists(pdfDir))
                {
                    ed.WriteMessage(string.Format(CultureInfo.CurrentCulture, "\n(CreateCallCropBat '{0}' '{1}'): Verzeichnis '{1}' existiert nicht!", cropBatDir, pdfDir));
                    return false;
                }

                string cropBatFileName = System.IO.Path.Combine(cropBatDir, cropBatName);
                try
                {
                    if (System.IO.File.Exists(cropBatFileName)) System.IO.File.Delete(cropBatFileName);
                }
                catch (System.Exception)
                {
                }
                if (System.IO.File.Exists(cropBatFileName))
                {
                    ed.WriteMessage(string.Format(CultureInfo.CurrentCulture, "\nFehler in CreateCallCropBat: Konnte Datei '{0}' nicht löschen!", cropBatFileName));
                    return false;
                }
                if (!pdfDir.ToUpperInvariant().Contains("T:"))
                {
                    ed.WriteMessage(string.Format(CultureInfo.CurrentCulture, "\nFehler in CreateCallCropBat: PdfDir '{0}' muss auf T: liegen!", pdfDir));
                    return false;
                }

                //pdfDir = pdfDir.ToUpperInvariant().Replace("T:", "E:");

                string txt = string.Format(CultureInfo.InvariantCulture, "call c:\\pdftemp\\crop.bat \"{0}\" \"{0}\"\n", pdfDir);
                System.IO.File.WriteAllText(cropBatFileName, txt);

                return true;

            }
            catch (System.Exception ex)
            {

                ed.WriteMessage(string.Format(CultureInfo.CurrentCulture, "\nFehler in CreateCallCropBat! {0}'", ex.Message));
            }
            return false;
        }

        [_AcTrx.LispFunction("CreateCallCropRotateBat")]
        public static bool CreateCallCropRotateBat(_AcDb.ResultBuffer rb)
        {
            const string cropBatName = "CallCrop.BAT";
            _AcEd.Editor ed = _AcAp.Application.DocumentManager.MdiActiveDocument.Editor;


            try
            {

                if (rb == null)
                {
                    ed.WriteMessage("\nAufruf: (CreateCallCropRotateBat CropBatDir PdfDir)");
                    return false;
                }
                _AcDb.TypedValue[] values = rb.AsArray();
                if (values == null || values.Length < 2 || values[0].Value == null || values[1] == null)
                {
                    ed.WriteMessage("\nAufruf: (CreateCallCropRotateBat CropBatDir PdfDir)");
                    return false;
                }

                string cropBatDir = values[0].Value.ToString();
                string pdfDir = values[1].Value.ToString();
                if (string.IsNullOrEmpty(cropBatDir) || !System.IO.Directory.Exists(cropBatDir))
                {
                    ed.WriteMessage(string.Format(CultureInfo.CurrentCulture, "\n(CreateCallCropRotateBat '{0}' '{1}'): Verzeichnis '{0}' existiert nicht!", cropBatDir, pdfDir));
                    return false;
                }
                if (string.IsNullOrEmpty(pdfDir) || !System.IO.Directory.Exists(pdfDir))
                {
                    ed.WriteMessage(string.Format(CultureInfo.CurrentCulture, "\n(CreateCallCropRotateBat '{0}' '{1}'): Verzeichnis '{1}' existiert nicht!", cropBatDir, pdfDir));
                    return false;
                }

                string cropBatFileName = System.IO.Path.Combine(cropBatDir, cropBatName);
                try
                {
                    if (System.IO.File.Exists(cropBatFileName)) System.IO.File.Delete(cropBatFileName);
                }
                catch (System.Exception)
                {
                }
                if (System.IO.File.Exists(cropBatFileName))
                {
                    ed.WriteMessage(string.Format(CultureInfo.CurrentCulture, "\nFehler in CreateCallCropRotateBat: Konnte Datei '{0}' nicht löschen!", cropBatFileName));
                    return false;
                }
                if (!pdfDir.ToUpperInvariant().Contains("T:"))
                {
                    ed.WriteMessage(string.Format(CultureInfo.CurrentCulture, "\nFehler in CreateCallCropRotateBat: PdfDir '{0}' muss auf T: liegen!", pdfDir));
                    return false;
                }

                //pdfDir = pdfDir.ToUpperInvariant().Replace("T:", "E:");

                string txt = string.Format(CultureInfo.InvariantCulture, "call c:\\pdftemp\\cropRotate.bat \"{0}\" \"{0}\"\n", pdfDir);
                System.IO.File.WriteAllText(cropBatFileName, txt);

                return true;

            }
            catch (System.Exception ex)
            {

                ed.WriteMessage(string.Format(CultureInfo.CurrentCulture, "\nFehler in CreateCallCropRotateBat! {0}'", ex.Message));
            }
            return false;
        }

        [_AcTrx.LispFunction("CreateCallRotateBat")]
        public static bool CreateCallRotateBat(_AcDb.ResultBuffer rb)
        {
            const string cropBatName = "CallCrop.BAT";
            _AcEd.Editor ed = _AcAp.Application.DocumentManager.MdiActiveDocument.Editor;


            try
            {

                if (rb == null)
                {
                    ed.WriteMessage("\nAufruf: (CreateCallRotateBat CropBatDir PdfDir)");
                    return false;
                }
                _AcDb.TypedValue[] values = rb.AsArray();
                if (values == null || values.Length < 2 || values[0].Value == null || values[1] == null)
                {
                    ed.WriteMessage("\nAufruf: (CreateCallRotateBat CropBatDir PdfDir)");
                    return false;
                }

                string cropBatDir = values[0].Value.ToString();
                string pdfDir = values[1].Value.ToString();
                if (string.IsNullOrEmpty(cropBatDir) || !System.IO.Directory.Exists(cropBatDir))
                {
                    ed.WriteMessage(string.Format(CultureInfo.CurrentCulture, "\n(CreateCallRotateBat '{0}' '{1}'): Verzeichnis '{0}' existiert nicht!", cropBatDir, pdfDir));
                    return false;
                }
                if (string.IsNullOrEmpty(pdfDir) || !System.IO.Directory.Exists(pdfDir))
                {
                    ed.WriteMessage(string.Format(CultureInfo.CurrentCulture, "\n(CreateCallRotateBat '{0}' '{1}'): Verzeichnis '{1}' existiert nicht!", cropBatDir, pdfDir));
                    return false;
                }

                string cropBatFileName = System.IO.Path.Combine(cropBatDir, cropBatName);
                try
                {
                    if (System.IO.File.Exists(cropBatFileName)) System.IO.File.Delete(cropBatFileName);
                }
                catch (System.Exception)
                {
                }
                if (System.IO.File.Exists(cropBatFileName))
                {
                    ed.WriteMessage(string.Format(CultureInfo.CurrentCulture, "\nFehler in CreateCallRotateBat: Konnte Datei '{0}' nicht löschen!", cropBatFileName));
                    return false;
                }
                if (!pdfDir.ToUpperInvariant().Contains("T:"))
                {
                    ed.WriteMessage(string.Format(CultureInfo.CurrentCulture, "\nFehler in CreateCallRotateBat: PdfDir '{0}' muss auf T: liegen!", pdfDir));
                    return false;
                }

                //pdfDir = pdfDir.ToUpperInvariant().Replace("T:", "E:");

                string txt = string.Format(CultureInfo.InvariantCulture, "call c:\\pdftemp\\Rotate.bat \"{0}\" \"{0}\"\n", pdfDir);
                System.IO.File.WriteAllText(cropBatFileName, txt);

                return true;

            }
            catch (System.Exception ex)
            {

                ed.WriteMessage(string.Format(CultureInfo.CurrentCulture, "\nFehler in CreateCallRotateBat! {0}'", ex.Message));
            }
            return false;
        }

        [_AcTrx.LispFunction("CallCropBat")]
        public static bool CallCropBat(_AcDb.ResultBuffer rb)
        {
            _AcEd.Editor ed = _AcAp.Application.DocumentManager.MdiActiveDocument.Editor;
            string taskName = "CallCropBat";

            try
            {
                using (TaskService tasksrvc = new TaskService("server", "pdfcrop", "workgroup", "pdf"))
                {
                    Task task = tasksrvc.FindTask(taskName);
                    if (task != null)
                    {
                        var t1 = task.Run();
                        ed.WriteMessage(string.Format("\nTask '{0}' ausgeführt.", taskName));
                        return true;
                    }
                    else
                    {
                        ed.WriteMessage(string.Format("\nTask '{0}' nicht gefunden!", taskName));
                    }
                }
            }
            catch (System.Exception ex)
            {
                ed.WriteMessage(string.Format(CultureInfo.CurrentCulture, "\nFehler in CallCropBat! {0}'", ex.Message));
            }
            return false;
        }

        [_AcTrx.LispFunction("TextToClipBoard")]
        public static void TextToClipBoard(_AcDb.ResultBuffer rb)
        {
            try
            {
                if (rb == null) return;
                _AcDb.TypedValue[] values = rb.AsArray();
                if (values == null || values.Length < 1) return;
                if (values[0].Value == null) return;

                string clipBoardText = values[0].Value.ToString();
                if (string.IsNullOrEmpty(clipBoardText)) return;

                System.Windows.Clipboard.SetText(clipBoardText);

            }
            catch (System.Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(string.Format(CultureInfo.CurrentCulture, "Fehler in TextToClipBoard: {0}", ex.Message), "TextToClipBoard");
            }
        }

        [_AcTrx.CommandMethod("HelloWorld")]
        public static void HelloWorld()
        {
            System.Windows.Forms.MessageBox.Show("Hello World!");
        }
        [_AcTrx.LispFunction("helloWorld")]
        public static bool helloWorld(_AcDb.ResultBuffer args)
        {
            _AcEd.Editor ed = _AcAp.Application.DocumentManager.MdiActiveDocument.Editor; ed.WriteMessage('\n' + "Hello World!" + '\n');
            return true;
        }

        private int _test = -1;

        [_AcTrx.CommandMethod("GEOIN_DN")]
        public void CallLisp()
        {
            CADDZone.AutoCAD.Samples.AcedInvokeSample.InvokeCGeoin();
            //CADDZone.AutoCAD.Samples.AcedInvokeSample.TestInvokeLisp();
        }

        [_AcTrx.LispFunction("Plan2AddHyperLink")]
        public static bool Plan2AddHyperLink(_AcDb.ResultBuffer rb)
        {
            var doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            _AcDb.Database db = _AcAp.Application.DocumentManager.MdiActiveDocument.Database;
            _AcDb.TransactionManager dbTrans = db.TransactionManager;
            _AcEd.Editor ed = doc.Editor;

            try
            {
                _AcDb.ObjectId oid;
                string description, name, sublocation;
                if (rb == null)
                {
                    ed.WriteMessage("Aufruf: (Plan2AddHyperLink el description name sublocation)");
                    return false;
                }

                var typedValues = rb.AsArray();
                if (typedValues == null || typedValues.Length != 4)
                {
                    ed.WriteMessage("Aufruf: (Plan2AddHyperLink el description name sublocation)");
                    return false;
                }

                if (typedValues[0].TypeCode != (short)_AcBrx.LispDataType.ObjectId)
                {
                    ed.WriteMessage("Aufruf: (Plan2AddHyperLink el description name sublocation)");
                    return false;
                }

                oid = (_AcDb.ObjectId)typedValues[0].Value;

                if (typedValues[1].TypeCode != (short)_AcBrx.LispDataType.Text)
                {
                    ed.WriteMessage("Aufruf: (Plan2AddHyperLink el description name sublocation)");
                    return false;
                }
                description = typedValues[1].Value.ToString();

                if (typedValues[2].TypeCode != (short)_AcBrx.LispDataType.Text)
                {
                    ed.WriteMessage("Aufruf: (Plan2AddHyperLink el description name sublocation)");
                    return false;
                }
                name = typedValues[2].Value.ToString();

                if (typedValues[3].TypeCode != (short)_AcBrx.LispDataType.Text)
                {
                    ed.WriteMessage("Aufruf: (Plan2AddHyperLink el description name sublocation)");
                    return false;
                }
                sublocation = typedValues[3].Value.ToString();


                using (_AcDb.Transaction trans = dbTrans.StartTransaction())
                {
                    _AcDb.Entity ent = trans.GetObject((_AcDb.ObjectId)typedValues[0].Value, _AcDb.OpenMode.ForWrite) as _AcDb.Entity;
                    if (ent != null)
                    {
                        _AcDb.HyperLink hyperLink = new _AcDb.HyperLink();
                        hyperLink.Description = description;
                        hyperLink.Name = name;
                        hyperLink.SubLocation = sublocation;

                        ent.Hyperlinks.Add(hyperLink);
                    }
                    trans.Commit();
                }
                return true;

            }
            catch (Exception ex)
            {
                ed.WriteMessage("Aufruf: (Plan2RemoveHyperLinks el)");
            }
            return false;

        }

        [_AcTrx.LispFunction("Plan2RemoveHyperLinks")]
        public static bool Plan2RemoveHyperLinks(_AcDb.ResultBuffer rb)
        {
            var doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            _AcDb.Database db = _AcAp.Application.DocumentManager.MdiActiveDocument.Database;
            _AcDb.TransactionManager dbTrans = db.TransactionManager;
            _AcEd.Editor ed = doc.Editor;

            try
            {
                if (rb == null)
                {
                    ed.WriteMessage("Aufruf: (Plan2RemoveHyperLinks el)");
                    return false;
                }

                var typedValues = rb.AsArray();
                if (typedValues == null || typedValues.Length != 1)
                {
                    ed.WriteMessage("Aufruf: (Plan2RemoveHyperLinks el)");
                    return false;
                }

                if (typedValues[0].TypeCode != (short)_AcBrx.LispDataType.ObjectId)
                {
                    ed.WriteMessage("Aufruf: (Plan2RemoveHyperLinks el)");
                    return false;
                }

                using (_AcDb.Transaction trans = dbTrans.StartTransaction())
                {
                    _AcDb.Entity ent = trans.GetObject((_AcDb.ObjectId)typedValues[0].Value, _AcDb.OpenMode.ForWrite) as _AcDb.Entity;
                    if (ent != null)
                    {
                        //ent.Hyperlinks.Clear(); // -> crashes
                        while (ent.Hyperlinks.Count > 0)
                        {
                            ent.Hyperlinks.RemoveAt(0);
                        }
                    }
                    trans.Commit();
                }
                return true;

            }
            catch (Exception ex)
            {
                ed.WriteMessage("Aufruf: (Plan2RemoveHyperLinks el)");
            }
            return false;

        }
        // instance method
        [_AcTrx.CommandMethod("TestTrans")]
        public void TestTrans()
        {
            try
            {
                //System.Windows.Forms.MessageBox.Show(string.Format(CultureInfo.InvariantCulture, "_test = {0}", _test));
                //_test = 0;



                _AcDb.Database db = _AcAp.Application.DocumentManager.MdiActiveDocument.Database;
                _AcDb.TransactionManager dbTrans = db.TransactionManager;
                using (_AcDb.Transaction trans = dbTrans.StartTransaction())
                {
                    // create a line
                    _AcDb.Line ln = new _AcDb.Line(new _AcGe.Point3d(0.0, 0.0, 0.0), new _AcGe.Point3d(1.0, 1.0, 0.0));
                    _AcDb.BlockTable bt = dbTrans.GetObject(db.BlockTableId, _AcDb.OpenMode.ForRead, false) as _AcDb.BlockTable;
                    if (bt == null) return;
                    //ObjectId id = bt[BlockTableRecord.ModelSpace];
                    _AcDb.BlockTableRecord btr = dbTrans.GetObject(bt[_AcDb.BlockTableRecord.ModelSpace], _AcDb.OpenMode.ForWrite, false) as _AcDb.BlockTableRecord;
                    if (btr == null) return;
                    //Add it to the model space block table record.
                    btr.AppendEntity(ln);
                    //Make sure that the transaction knows about this new object.    tm.AddNewlyCreatedDBObject(line, True)
                    dbTrans.AddNewlyCreatedDBObject(ln, true);


                    //'Add some hyperlinks.    Dim hyper As New HyperLink()    hyper.Name = "www.autodesk.com"    line.Hyperlinks.Add(hyper)   
                    _AcDb.HyperLink hyper = new _AcDb.HyperLink();
                    hyper.Name = "www.facebook.com";
                    ln.Hyperlinks.Add(hyper);
                    if (ln.Hyperlinks.Contains(hyper))
                    {
                        hyper.Name = "www.gotdotnet.com";
                    }
                    ln.Hyperlinks.Add(hyper);
                    foreach (var hl in ln.Hyperlinks)
                    {
                        System.Diagnostics.Debug.WriteLine(hl.ToString());
                    }
                    trans.Commit();
                }
            }
            catch (System.Exception ex)
            {

                System.Windows.Forms.MessageBox.Show(ex.Message);

            }

        }

    }
}
