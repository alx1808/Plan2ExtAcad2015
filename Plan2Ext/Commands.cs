﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Globalization;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Office.Interop.Excel;
//using Autodesk.AutoCAD.ApplicationServices.Core;
#if !ACAD2020
using Microsoft.Win32.TaskScheduler;
#endif

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
// ReSharper disable StringLiteralTypo
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
// ReSharper disable IdentifierTypo
#endif

namespace Plan2Ext
{
    public class Commands
    {
        #region log4net Initialization
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Convert.ToString((typeof(Commands))));
        #endregion



        private static bool inCommand;


        [_AcTrx.LispFunction("SetWorkingDirx")]
        public static bool SetWorkingDir(_AcDb.ResultBuffer rb)
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
                if (!Directory.Exists(dirName)) return false;

                Directory.SetCurrentDirectory(dirName);

                return true;
            }
            catch (Exception ex)
            {
                string msg = string.Format(CultureInfo.CurrentCulture, "Fehler in (SetWorkingDir): {0}", ex.Message);
                ed.WriteMessage("\n" + msg);
                return false;
            }
        }


        /*
        //[_AcTrx.CommandMethod("Alx")]
        public static void Alx()
        {


	        _AcAp.Document doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
	        _AcEd.Editor ed = doc.Editor;
	        _AcDb.Database db = doc.Database;



	        var oid = EditorHelper.Entlast();
	        ed.WriteMessage(oid == _AcDb.ObjectId.Null ? "\nNot found" : oid.Handle.ToString());

	        var p3d = new _AcGe.Point3d(109708.402928399, 329353.555420752, 0);
	        //var p3d = new _AcGe.Point3d(2439.05, 1298.36, 0.0);

	        //ed.Command("_.bpoly","2439.05,1298.36,0.0","" );
	        ed.Command("_.bpoly", p3d, "");

	        ////var cmd = "(vl-cmdf \"_.bpoly\" '(2439.05 1298.36 0.0) \" \")";
	        ////var cmd = "(vl-cmdf \"_.bpoly\" p \"\") ";
	        //var cmd = "(vl-cmdf \"_.bpoly\" '(2439.05 1298.36 0.0) \"\") ";
	        //doc.CommandEnded += doc_CommandEnded;
	        //doc.CommandCancelled += doc_CommandEnded;
	        //inCommand = true;
	        //ed.Document.SendStringToExecute(cmd, true, false, true);
	        //while (inCommand)
	        //{
	        //    var active = Application.GetSystemVariable("CmdActive");
	        //}
	        //doc.CommandEnded -= doc_CommandEnded;


	        var oid2 = EditorHelper.Entlast();
	        var ok = (oid != oid2);
	        ed.WriteMessage("Is ok: " + ok);

	        //var res = ed.SelectLast();
	        //ed.WriteMessage("\n" + res.Status);

	        //var ss = res.Value;
	        //var cnt = ss.Count;
	        //var oids = ss.GetObjectIds();
	        //var oid = oids[0];



        }

        static void doc_CommandEnded(object sender, _AcAp.CommandEventArgs e)
        {
	        inCommand = false;
        }
        */
        public static void ImportDrawing(string drawingPath)
        {
            var doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            using (var trans = db.TransactionManager.StartTransaction())
            {
                try
                {
                    var objectIds = new _AcDb.ObjectIdCollection();
                    var blockTable = (_AcDb.BlockTable)trans.GetObject(db.BlockTableId, _AcDb.OpenMode.ForRead);
                    var extDb = new _AcDb.Database();
                    extDb.ReadDwgFile(drawingPath, System.IO.FileShare.Read, false, null);
                    using (var extTrans = extDb.TransactionManager.StartTransaction())
                    {
                        var extBlockTable =
                            (_AcDb.BlockTable)extTrans.GetObject(
                                extDb.BlockTableId, _AcDb.OpenMode.ForRead);
                        var extModelSpace =
                            (_AcDb.BlockTableRecord)extTrans.GetObject(
                                extBlockTable[_AcDb.BlockTableRecord.ModelSpace], _AcDb.OpenMode.ForRead);
                        foreach (var id in extModelSpace)
                        {
                            var ent = extTrans.GetObject(id, _AcDb.OpenMode.ForRead, true);
                            if (!ent.IsErased) objectIds.Add(id);
                        }
                    }
                    extDb.CloseInput(true);
                    var idMapping = new _AcDb.IdMapping();
                    db.WblockCloneObjects(
                        objectIds,
                        blockTable[_AcDb.BlockTableRecord.ModelSpace],
                        idMapping,
                        _AcDb.DuplicateRecordCloning.Replace,
                        false);
                    trans.Commit();
                }
                catch (System.Exception)
                {
                }
            }
            //string drawingPath = @"C:\Temp\StandardTemplate.dwg";
            //var destDb = _AcAp.Application.DocumentManager.MdiActiveDocument.Database;
            //using (var sourceDb = new _AcDb.Database(buildDefaultDrawing: false, noDocument: true))
            //{
            //    sourceDb.ReadDwgFile(drawingPath, System.IO.FileShare.Read, false, null);
            //    var SourceObjectIds = new _AcDb.ObjectIdCollection();
            //    Autodesk.AutoCAD.DatabaseServices.TransactionManager SourceTM = sourceDb.TransactionManager;
            //    using (var trans = destDb.TransactionManager.StartTransaction())
            //    {
            //        try
            //        {
            //            using (var extTrans = sourceDb.TransactionManager.StartTransaction())
            //            {
            //                var extBlockTable = (_AcDb.BlockTable)extTrans.GetObject(sourceDb.BlockTableId, _AcDb.OpenMode.ForRead);
            //                var extModelSpace = (_AcDb.BlockTableRecord)extTrans.GetObject(extBlockTable[_AcDb.BlockTableRecord.ModelSpace], _AcDb.OpenMode.ForRead);
            //                foreach (var id in extModelSpace)
            //                {
            //                    SourceObjectIds.Add(id);
            //                }
            //            }
            //            sourceDb.CloseInput(true);
            //            var idMapping = new _AcDb.IdMapping();
            //            destDb.WblockCloneObjects(SourceObjectIds, destDb.BlockTableId, idMapping, _AcDb.DuplicateRecordCloning.Replace, false);
            //            trans.Commit();
            //        }
            //        catch (System.Exception ex)
            //        {
            //            _AcAp.Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nError " + ex.Message);
            //        }
            //    }
            //}
        }




        //[_AcTrx.CommandMethod("alxinsert", _AcTrx.CommandFlags.NoTileMode)]
        //public static void alxinsert()
        //{
        //    _AcAp.Document doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
        //    _AcEd.Editor ed = doc.Editor;
        //    _AcDb.Database db = doc.Database;

        //    string blockName = "PLK_FW_BA_STANDORT";
        //    string protoTypeDwg = "FW_Legende.dwg";

        //    if (Globs.BlockExists(blockName) || Globs.InsertFromPrototype(blockName, protoTypeDwg))
        //    {
        //        using (var transaction = doc.TransactionManager.StartTransaction())
        //        {
        //            var blockTable = (_AcDb.BlockTable)transaction.GetObject(db.BlockTableId, _AcDb.OpenMode.ForRead);
        //            var oid = blockTable[blockName];
        //            using (var bref = new _AcDb.BlockReference(new _AcGe.Point3d(-200, 0, 0), oid))
        //            {
        //                var acCurSpaceBlkTblRec = (_AcDb.BlockTableRecord)transaction.GetObject(db.CurrentSpaceId, _AcDb.OpenMode.ForWrite);
        //                acCurSpaceBlkTblRec.AppendEntity(bref);
        //                transaction.AddNewlyCreatedDBObject(bref, true);

        //                _AcDb.DBObjectCollection objs = new _AcDb.DBObjectCollection();
        //                bref.Explode(objs);
        //                _AcDb.ObjectId blockRefTableId = bref.BlockTableRecord;
        //                foreach (_AcDb.DBObject obj in objs)
        //                {
        //                    _AcDb.Entity ent = (_AcDb.Entity)obj;
        //                    acCurSpaceBlkTblRec.AppendEntity(ent);
        //                    transaction.AddNewlyCreatedDBObject(ent, true);
        //                }

        //                bref.UpgradeOpen();
        //                bref.Erase();
        //            }

        //            transaction.Commit();
        //        }
        //    }
        //}


        ///// <summary>
        ///// Paperspace coords to Modelspace (siehe: PaperSpaceHelper.cs)
        ///// </summary>
        //[_AcTrx.CommandMethod("ps2ms", _AcTrx.CommandFlags.NoTileMode)]
        //public static void ps2ms()
        //{
        //    _AcAp.Document doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
        //    _AcEd.Editor ed = doc.Editor;
        //    _AcDb.Database db = doc.Database;

        //    var vpResult = ed.GetEntity("Viewport");
        //    if (vpResult.Status != _AcEd.PromptStatus.OK) return;
        //    var vpOid = vpResult.ObjectId;

        //    var points = new List<_AcGe.Point3d>();
        //    _AcEd.PromptPointResult resultPoint = null;
        //    do
        //    {
        //        resultPoint = ed.GetPoint("Pick Model Space Point");
        //        if (resultPoint.Status == _AcEd.PromptStatus.OK)
        //        {
        //            points.Add(resultPoint.Value);
        //        }

        //    } while (resultPoint.Status == _AcEd.PromptStatus.OK);

        //    //SetActivePaperspaceViewport(vpOid, true);
        //    var wcsPoints = new List<_AcGe.Point3d>();
        //    PaperSpaceHelper.ConvertPaperSpaceCoordinatesToModelSpaceWcs(vpOid,points, wcsPoints);

        //    // create a new DBPoint and add to model space to show where we picked
        //    foreach (var wcsPoint in wcsPoints)
        //    {
        //        using (var pnt = new _AcDb.DBPoint(new _AcGe.Point3d(wcsPoint.ToArray())))
        //        using (var bt = ed.Document.Database.BlockTableId.Open(_AcDb.OpenMode.ForRead) as _AcDb.BlockTable)
        //        using (var ms = bt[_AcDb.BlockTableRecord.ModelSpace].Open(_AcDb.OpenMode.ForWrite) as _AcDb.BlockTableRecord)
        //            ms.AppendEntity(pnt);
        //    }
        //}

        //[_AcTrx.CommandMethod("CheckDimensions", _AcTrx.CommandFlags.UsePickSet)]
        //public void CheckDimensions()
        //{
        //    _AcAp.Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
        //    _AcDb.Database db = doc.Database;
        //    _AcEd.Editor ed = doc.Editor;

        //    _AcDb.Transaction tr = doc.TransactionManager.StartTransaction();
        //    using (tr)
        //    {
        //        _AcEd.PromptEntityOptions peo = new _AcEd.PromptEntityOptions("\nSelect dimension >>");
        //        peo.SetRejectMessage("\nSelect dimension only >>");
        //        peo.AddAllowedClass(typeof(_AcDb.Dimension), false);
        //        _AcEd.PromptEntityResult res;
        //        res = ed.GetEntity(peo);
        //        if (res.Status != _AcEd.PromptStatus.OK)
        //            return;

        //        _AcDb.Entity ent = (_AcDb.Entity)tr.GetObject(res.ObjectId, _AcDb.OpenMode.ForRead);

        //        if (ent == null)
        //            return;

        //        _AcDb.Dimension dim = (_AcDb.Dimension)ent as _AcDb.Dimension;

        //        if (dim != null)
        //        {

        //            ed.WriteMessage("\nDim measurement:\t{0}", dim.Measurement);

        //            if (dim.DimensionText != "") ed.WriteMessage("\nHas overriden dim text:\t{0}", dim.DimensionText);
        //            var pos = dim.TextPosition;
        //            var pos2 = pos + new _AcGe.Vector3d(1.0, 1.0, 0.0);

        //            var textStyleId = dim.TextStyleId;
        //            var textStyle = tr.GetObject(textStyleId, _AcDb.OpenMode.ForRead);
        //            var textStyleTableRecord = (_AcDb.TextStyleTableRecord) textStyle;
        //            var dimStyleOid = dim.DimensionStyle;
        //            var dimStyle = (_AcDb.DimStyleTableRecord)tr.GetObject(dimStyleOid, _AcDb.OpenMode.ForRead);


        //            dim.UpgradeOpen();
        //            dim.UsingDefaultTextPosition = false;
        //            dim.TextPosition = pos2;
        //            dim.DowngradeOpen();


        //        }

        //        tr.Commit();
        //    }//end using transaction
        //}

        //[_AcTrx.CommandMethod("Plan2Test")]
        //static public void Plan2Test()
        //{
        //    _AcAp.Document doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
        //    var db = doc.Database;
        //    _AcEd.Editor ed = doc.Editor;
        //    var res = ed.GetEntity("Show rotated dimension: ");
        //    if (res.Status != _AcEd.PromptStatus.OK) return;
        //    var oid = res.ObjectId;

        //    using (var trans = doc.TransactionManager.StartTransaction())
        //    {
        //        var dim = trans.GetObject(oid, _AcDb.OpenMode.ForRead) as _AcDb.RotatedDimension;
        //        if (dim != null)
        //        {
        //            var deg = dim.Rotation * 180 / Math.PI;
        //            ed.WriteMessage(string.Format("\nRotation: {0}, Measurement: {1}", deg.ToString(), dim.Measurement.ToString()));
        //        }
        //        trans.Commit();
        //    }
        //}



        [_AcTrx.CommandMethod("Plan2AttToBlockdef")]
        public static void Plan2AttToBlockdef()
        {
            try
            {
                var doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
                var db = doc.Database;
                var editor = _AcAp.Application.DocumentManager.MdiActiveDocument.Editor;

                var result = editor.GetEntity("\nBlock mit Attributen zeigen: ");
                if (result.Status != _AcEd.PromptStatus.OK) return;

                var nrOfCopiedAttributes = 0;
                using (var trans = db.TransactionManager.StartTransaction())
                {
                    var d = trans.GetObject(result.ObjectId, _AcDb.OpenMode.ForRead) as _AcDb.BlockReference;
                    if (d == null)
                    {
                        trans.Commit();
                        return;
                    }
                    var kw = Globs.AskKeywordFromUser("Sollen die Attribute neu positioniert werden?", new[] { "Ja", "Nein" }, 1);
                    if (string.IsNullOrEmpty(kw)) return;
                    var newPosition = kw.Equals("Ja");

                    var btr = (_AcDb.BlockTableRecord)trans.GetObject(d.BlockTableRecord, _AcDb.OpenMode.ForRead);
                    var existingAttributes = new HashSet<string>();
                    foreach (_AcDb.ObjectId oid in btr)
                    {
                        var att = trans.GetObject(oid, _AcDb.OpenMode.ForRead) as _AcDb.AttributeDefinition;
                        if (att != null) existingAttributes.Add(att.Tag.ToUpperInvariant());
                    }

                    var nextPos = btr.Origin;

                    btr.UpgradeOpen();
                    var mat = d.BlockTransform.Inverse();
                    foreach (_AcDb.ObjectId attId in d.AttributeCollection)
                    {
                        var attRef = (_AcDb.AttributeReference)trans.GetObject(attId, _AcDb.OpenMode.ForRead);
                        if (existingAttributes.Contains(attRef.Tag.ToUpperInvariant())) continue;

                        var pos = attRef.Position;
                        var al = attRef.AlignmentPoint;
                        var rot = attRef.Rotation;
                        var tag = attRef.Tag;
                        var c = attRef.IsConstant;
                        var vis = attRef.Visible;

                        var attDef = new _AcDb.AttributeDefinition();
                        attDef.SetDatabaseDefaults(btr.Database);
                        attDef.Tag = tag;
                        attDef.Prompt = tag;
                        attDef.Constant = c;
                        attDef.Visible = vis;
                        attDef.Invisible = !vis;
                        attDef.Layer = attRef.Layer;
                        attDef.Height = attRef.Height;
                        attDef.WidthFactor = attRef.WidthFactor;
                        attDef.TextStyleId = attRef.TextStyleId;

                        if (newPosition)
                        {
                            attDef.Position = nextPos;
                            attDef.Rotation = 0;
                            nextPos = new _AcGe.Point3d(nextPos.X, nextPos.Y - (attDef.Height * 1.1), nextPos.Z);
                        }
                        else
                        {
                            attDef.Position = pos;
                            attDef.Rotation = rot;
                            attDef.TransformBy(mat);
                        }
                        btr.AppendEntity(attDef);
                        trans.AddNewlyCreatedDBObject(attDef, true);
                        nrOfCopiedAttributes++;
                    }

                    trans.Commit();
                }
                editor.WriteMessage($"\nAnzahl kopierter Attribute: {nrOfCopiedAttributes}");
            }
            catch (Exception ex)
            {
                _AcAp.Application.ShowAlertDialog(ex.Message);
            }

        }


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
                                if (attOid.IsErased) continue;
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
                                if (attOid.IsErased) continue;
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
                if (!Directory.Exists(dirName)) return false;

                return Plan2FinishPlot(dirName);

            }
            catch (Exception ex)
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

            Plan2FinishPlot(dirName, "PDF");
            Plan2FinishPlot(dirName, "PLT");
            RecRemoveEmptyDir(Path.Combine(dirName, "PLT"));

            return true;
        }

        private static void Plan2FinishPlot(string dirName, string prefix)
        {
            string targetDir = Path.Combine(dirName, prefix);
            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }

            var subdirs = Directory.GetDirectories(targetDir, prefix + "*", System.IO.SearchOption.TopDirectoryOnly);

            foreach (var tempDir in subdirs)
            {
                string fName = Path.GetFileName(tempDir);
                if (fName == null) continue;
                if (!fName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) continue; // ignore other dirs

                var pdfFiles = Directory.GetFiles(tempDir, "*." + prefix, System.IO.SearchOption.TopDirectoryOnly);
                foreach (var pdf in pdfFiles)
                {
                    // ReSharper disable once AssignNullToNotNullAttribute
                    File.Copy(pdf, Path.Combine(targetDir, Path.GetFileName(pdf)), true);
                    File.Delete(pdf);
                }

                RecRemoveEmptyDir(tempDir);
            }
        }

        private static void RecRemoveEmptyDir(string tempDir)
        {
            var subdirs = Directory.GetDirectories(tempDir, "*.*", System.IO.SearchOption.TopDirectoryOnly);
            foreach (var sd in subdirs)
            {
                RecRemoveEmptyDir(sd);
            }

            if (Directory.GetFiles(tempDir, "*.*", System.IO.SearchOption.TopDirectoryOnly).Length == 0 &&
                Directory.GetDirectories(tempDir, "*.*", System.IO.SearchOption.TopDirectoryOnly).Length == 0)
            {
                Directory.Delete(tempDir);
            }
        }


        [_AcTrx.LispFunction("Plan2MatchCodeCorrection")]
        public static string Plan2MatchCodeCorrection(_AcDb.ResultBuffer rb)
        {
            if (rb == null) return null;
            var arr = rb.AsArray();
            if (arr.Length < 1) return null;
            if (arr[0].TypeCode != (short)_AcBrx.LispDataType.Text) return null;
            var val = arr[0].Value;
            if (val == null) return null;
            return Globs.MatchCodeCorrection(val.ToString());
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

            var length = GetDoubleValue(arr[1].Value);
            // angle 2c
            // colorindex 3
            // LB 4

            List<_AcGe.Point3d> pts = new List<_AcGe.Point3d>();
            for (int i = 5; i < arr.Length - 1; i++)
            {
                pts.Add((_AcGe.Point3d)arr[i].Value);
            }
            if (length.HasValue)
                Globs.InsertFehlerLines(pts, layer, length.Value);
            else Globs.InsertFehlerLines(pts, layer);
        }

        private static double? GetDoubleValue(object o)
        {
            if (o == null) return null;
            if (o is double || o is float || o is int || o is Int16) return Convert.ToDouble(o);
            return null;
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
            var layerNames = LayerManager.GetNamesOfNonPlottableLayers().ToList();

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


        [_AcTrx.LispFunction("InoWaitForPlotPdfFile")]
        public static string InoWaitForPlotPdfFile(_AcDb.ResultBuffer rb)
        {
            _AcEd.Editor ed = _AcAp.Application.DocumentManager.MdiActiveDocument.Editor;
            if (rb == null)
            {
                ShowCallingInfoForWaitForPlotFile(ed);
                return null;
            }

            _AcDb.TypedValue[] values = rb.AsArray();
            if (values == null || values.Length < 2 || values[0].Value == null || values[1].Value == null)
            {
                ShowCallingInfoForWaitForPlotFile(ed);
                return null;
            }

            var dir = values[0].Value.ToString();
            if (!Directory.Exists(dir))
            {
                ed.WriteMessage(string.Format(CultureInfo.CurrentCulture, "\nVerzeichnis {0} existiert nicht!", dir));
                return null;
            }

            int timeout;
            if (!int.TryParse(values[1].Value.ToString(), out timeout))
            {
                ed.WriteMessage(string.Format(CultureInfo.CurrentCulture, "\nTimeout is ungültig: {0}!", values[1].Value.ToString()));
                return null;
            }

            ed.WriteMessage("\nWarte auf Plotdatei");

            var nrOfAgeSeconds = 60;
            var start = Environment.TickCount;
            var endTime = start + timeout;
            string theFile = null;
            while (Environment.TickCount < endTime)
            {
                if (theFile == null)
                {
                    var files = Directory.GetFiles(dir, "*.pdf").Where(x => NotOlderThan(x, nrOfAgeSeconds)).OrderBy(File.GetLastAccessTime).Reverse().ToArray();
                    if (files.Length > 0)
                    {
                        theFile = files[0];
                    }
                }

                if (theFile != null)
                {
                    if (!IsFileLocked(new FileInfo(theFile)))
                    {
                        return theFile;
                    }
                }

                Thread.Sleep(1000);
            }
            if (theFile == null)
            {
                ed.WriteMessage(string.Format(CultureInfo.CurrentCulture, "\nKeine neue Plotdatei in {0} gefunden", dir));
            }
            else
            {
                ed.WriteMessage(string.Format(CultureInfo.CurrentCulture, "\nPlotfile {0} noch gesperrt nach Wartezeit {1}!", theFile, timeout));
            }
            return null;
        }

        private static bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;
            try
            {
                stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return false;
        }

        private static bool NotOlderThan(string fileName, int seconds)
        {
            var now = DateTime.Now;
            var diff = now - File.GetLastAccessTime(fileName);
            return (diff.TotalSeconds < seconds);
        }

        private static void ShowCallingInfoForWaitForPlotFile(_AcEd.Editor ed)
        {
            ed.WriteMessage("\nAufruf: (InoWaitForPlotPdfFile Directory Timeout)");
        }


        [_AcTrx.LispFunction("InoKillAllProcessesWithName")]
        public static bool InoKillAllProcessesWithName(_AcDb.ResultBuffer rb)
        {
            _AcEd.Editor ed = _AcAp.Application.DocumentManager.MdiActiveDocument.Editor;
            if (rb == null)
            {
                ShowCallingInfoForKillAllProcessesWithName(ed);
                return false;
            }
            _AcDb.TypedValue[] values = rb.AsArray();
            if (values == null || values.Length < 1)
            {
                ShowCallingInfoForKillAllProcessesWithName(ed);
                return false;
            }

            var processName = values[0].Value.ToString();
            var processes = Process.GetProcessesByName(processName);
            foreach (var process in processes)
            {
                process.Kill();
            }
            return true;
        }

        /// <summary>
        /// String.Replace from c# for Lisp
        /// </summary>
        /// <param name="rb"></param>
        /// <returns></returns>
        [_AcTrx.LispFunction("InoStringReplace")]
        public static string InoStringReplace(_AcDb.ResultBuffer rb)
        {
            _AcEd.Editor ed = _AcAp.Application.DocumentManager.MdiActiveDocument.Editor;
            if (rb == null)
            {
                ed.WriteMessage("\nAufruf (InoStringReplace Completestring StringToReplace NewString");
                return null;
            }

            _AcDb.TypedValue[] values = rb.AsArray();
            if (values == null || values.Length < 3)
            {
                ed.WriteMessage("\nAufruf (InoStringReplace Completestring StringToReplace NewString");
                return null;
            }

            if (values[0].TypeCode != (short)_AcBrx.LispDataType.Text)
            {
                ed.WriteMessage("\nAufruf (InoStringReplace Completestring StringToReplace NewString");
                return null;
            }
            var completeString = values[0].Value.ToString();
            if (values[1].TypeCode != (short)_AcBrx.LispDataType.Text)
            {
                ed.WriteMessage("\nAufruf (InoStringReplace Completestring StringToReplace NewString");
                return null;
            }
            var stringToReplace = values[1].Value.ToString();
            if (values[2].TypeCode != (short)_AcBrx.LispDataType.Text)
            {
                ed.WriteMessage("\nAufruf (InoStringReplace Completestring StringToReplace NewString");
                return null;
            }
            var newString = values[2].Value.ToString();

            return completeString.Replace(stringToReplace, newString);
        }



        /// <summary>
        /// Calls Process with arguments and environment variables
        /// </summary>
        /// <param name="rb"></param>
        /// <remarks>
        /// (InoCallProcessWithEnv exename (arglist) timeoutInMs ((envkey envval)(envkey envval)))
        /// timeoutInMs: -1 = infinite
        /// </remarks>
        /// <returns>
        /// True, if Process-Call was successful and no timeout
        /// </returns>
        [_AcTrx.LispFunction("InoCallProcessWithEnv")]
        public static bool InoCallProcessWithEnv(_AcDb.ResultBuffer rb)
        {
            _AcEd.Editor ed = _AcAp.Application.DocumentManager.MdiActiveDocument.Editor;
            if (rb == null) return false;
            _AcDb.TypedValue[] values = rb.AsArray();
            if (values == null || values.Length < 4)
            {
                ShowCallingInfoForProcessWithEnv(ed);
                return false;
            }
            if (values[0].Value == null) return false;
            string fileName = values[0].Value.ToString();

            // get args
            var args = new List<string>();
            int i = 1;
            if (values[i].TypeCode != (short)_AcBrx.LispDataType.Nil)
            {
                if (values[i].TypeCode != (short)_AcBrx.LispDataType.ListBegin)
                {
                    ShowCallingInfoForProcessWithEnv(ed);
                    return false;
                }
                i++;
                while (i < values.Length && (values[i].TypeCode != (short)_AcBrx.LispDataType.ListEnd))
                {
                    if (values[i].TypeCode == (short)_AcBrx.LispDataType.ListBegin)
                    {
                        ShowCallingInfoForProcessWithEnv(ed);
                        return false;
                    }
                    args.Add(values[i].Value.ToString());
                    i++;
                }
            }

            i++;
            // timeout
            int timeout;
            try
            {
                timeout = int.Parse(values[i].Value.ToString());
                i++;
            }
            catch (Exception e)
            {
                ed.WriteMessage(string.Format(CultureInfo.CurrentCulture, "Fehler bei timeout parameter='{0}': {1}", values[i].Value, e));
                ShowCallingInfoForProcessWithEnv(ed);
                return false;
            }

            // get envs
            var envs = new List<EnvPair>();
            if (values[i].TypeCode != (short)_AcBrx.LispDataType.Nil)
            {
                if (values[i].TypeCode != (short)_AcBrx.LispDataType.ListBegin)
                {
                    ShowCallingInfoForProcessWithEnv(ed);
                    return false;
                }

                for (int j = i + 1; j < values.Length - 4; j += 4)
                {
                    if (values[j].TypeCode != (short)_AcBrx.LispDataType.ListBegin)
                    {
                        ShowCallingInfoForProcessWithEnv(ed);
                        return false;
                    }

                    var key = values[j + 1].Value.ToString();
                    var val = values[j + 2].Value.ToString();
                    envs.Add(new EnvPair()
                    {
                        Key = key,
                        Val = val
                    });

                    if (values[j + 3].TypeCode != (short)_AcBrx.LispDataType.ListEnd)
                    {
                        ShowCallingInfoForProcessWithEnv(ed);
                        return false;
                    }
                }
            }

            // call process
            foreach (var envPair in envs)
            {
                ed.WriteMessage(string.Format(CultureInfo.CurrentCulture, "\n{0}:{1}", envPair.Key, envPair.Val));
                System.Environment.SetEnvironmentVariable(envPair.Key, envPair.Val);
            }

            var allArgs = string.Join(" ", args);
            ed.WriteMessage(string.Format(CultureInfo.CurrentCulture, "\nCalling {0} {1}", fileName, allArgs));

            try
            {
                using (var exeProcess = Process.Start(fileName, allArgs))
                {
                    if (exeProcess == null)
                    {
                        ed.WriteMessage(string.Format(CultureInfo.CurrentCulture, "Konnte Prozess {0} nicht starten!", fileName));
                        return false;
                    }
                    exeProcess.WaitForExit(timeout);
                    if (!exeProcess.HasExited)
                    {
                        ed.WriteMessage("Timeout überschritten. Der Prozess wird beendet.");
                        exeProcess.Kill();
                        return false;
                    }
                }
            }
            catch (Exception e)
            {
                ed.WriteMessage(string.Format(CultureInfo.CurrentCulture, "\nFehler beim Starten von Prozess {1}: {0}", e, fileName));
                return false;
            }

            return true;
        }

        private static void ShowCallingInfoForKillAllProcessesWithName(_AcEd.Editor ed)
        {
            ed.WriteMessage(string.Format(CultureInfo.CurrentCulture,
                "Aufruf: (InoKillAllProcessesWithName {{friendly process name}})"));
        }

        private static void ShowCallingInfoForProcessWithEnv(_AcEd.Editor ed)
        {
            ed.WriteMessage(string.Format(CultureInfo.CurrentCulture,
                "Aufruf: (InoCallProcessWithEnv exename (arglist) timeoutInMs ((envkey envval)(envkey envval)))"));
        }

        private class EnvPair
        {
            public string Key { get; set; }
            public string Val { get; set; }
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
                //process.WaitForExit();

                //// *** Read the streams ***
                //output = process.StandardOutput.ReadToEnd();
                //error = process.StandardError.ReadToEnd();

                //exitCode = process.ExitCode;
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
#if !ACAD2020

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
#endif
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

        //[_AcTrx.CommandMethod("HelloWorld")]
        //public static void HelloWorld()
        //{
        //    System.Windows.Forms.MessageBox.Show("Hello World!");
        //}
        //[_AcTrx.LispFunction("helloWorld")]
        //public static bool helloWorld(_AcDb.ResultBuffer args)
        //{
        //    _AcEd.Editor ed = _AcAp.Application.DocumentManager.MdiActiveDocument.Editor; ed.WriteMessage('\n' + "Hello World!" + '\n');
        //    return true;
        //}

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
            catch (Exception)
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
            catch (Exception)
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
