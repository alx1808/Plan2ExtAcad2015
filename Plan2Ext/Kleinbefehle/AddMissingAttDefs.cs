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
using System.Collections.Generic;
using System.Globalization;
using Autodesk.AutoCAD.ApplicationServices.Core;

namespace Plan2Ext.Kleinbefehle
{
    public static class AddMissingAttDefsClass
    {

        private static double  _attributeHeight = 1.0;

        /// <summary>
        /// From Lisp callable function: Adds Attributereferences to blockrefences (as elements in parameterlist), which are defined in blockdefinition but don't exist yet in blockreference.
        /// </summary>
        /// <param name="rb">(Plan2AddMissingAttRefs (el1 el2 ...))</param>
        /// <remarks>
        /// </remarks>
        /// <returns>true on success</returns>
        [_AcTrx.LispFunction("Plan2AddMissingAttDefs")]
        public static bool Plan2AddMissingAttDefs(_AcDb.ResultBuffer rb)
        {
            _AcEd.Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            try
            {
                if (rb == null) return false;
                _AcDb.TypedValue[] values = rb.AsArray();
                if (values == null || values.Length < 4) return false;
                if (values[0].TypeCode != (short)_AcTrx.LispDataType.ListBegin) return false;
                if (values[values.Length - 2].TypeCode != (short)_AcTrx.LispDataType.ListEnd) return false;

                if (values[values.Length - 1].TypeCode == (short) _AcTrx.LispDataType.Double)
                {
                    var height = (double) values[values.Length - 1].Value;
                    if (height > 0)
                    {
                        _attributeHeight = height;
                    }
                }

                var oids = new List<_AcDb.ObjectId>();
                for (int i = 1; i < values.Length - 2; i++)
                {
                    oids.Add((_AcDb.ObjectId)values[i].Value);
                }
                return AddMissingAttDefs(oids);
            }
            catch (System.Exception ex)
            {
                string msg = string.Format(CultureInfo.CurrentCulture, "Fehler in (Plan2AddMissingAttDefs): {0}", ex.Message);
                ed.WriteMessage("\n" + msg);
                // ReSharper disable once LocalizableElement
                System.Windows.Forms.MessageBox.Show(ex.Message, "Plan2AddMissingAttDefs");
                return false;
            }
        }

        private static bool AddMissingAttDefs(List<_AcDb.ObjectId> oids)
        {
            _AcAp.Document doc = Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            using (var trans = db.TransactionManager.StartTransaction())
            {
                foreach (var oid in oids)
                {
                    var blockRef = trans.GetObject(oid, _AcDb.OpenMode.ForRead) as _AcDb.BlockReference;
                    if (blockRef != null)
                    {
                        AddMissingAttDefs(blockRef, trans);
                    }
                }
                trans.Commit();
            }
            return true;
        }

        private static void AddMissingAttDefs(_AcDb.BlockReference blockRef, _AcDb.Transaction trans)
        {
            var attDefNames = GetNonConstantAttDefNames(blockRef.BlockTableRecord, trans);
            var attRefs = GetAttRefsNoInAttdefs(blockRef, trans, attDefNames);
            if (attRefs.Count == 0) return;

            var btr = (_AcDb.BlockTableRecord)trans.GetObject(blockRef.BlockTableRecord, _AcDb.OpenMode.ForRead);
            btr.UpgradeOpen();
            var inc = 0 - (_attributeHeight * 1.1);
            var ypos = inc;
            foreach (var attRef in attRefs)
            {
                // Add an attribute definition to the block
                using (var acAttDef = new _AcDb.AttributeDefinition())
                {
                    acAttDef.Position = new _AcGe.Point3d(0, ypos, 0);
                    acAttDef.Verifiable = true;
                    acAttDef.Prompt = attRef.Tag;
                    acAttDef.Tag = attRef.Tag;
                    acAttDef.TextString = "";
                    acAttDef.Height = _attributeHeight;

                    btr.AppendEntity(acAttDef);

                    trans.AddNewlyCreatedDBObject(acAttDef, true);
                }

                ypos += inc;
            }
            btr.DowngradeOpen();
        }

        private static List<_AcDb.AttributeReference> GetAttRefsNoInAttdefs(_AcDb.BlockReference blockRef, _AcDb.Transaction trans, List<string> attDefNames)
        {
            var attRefs = new List<_AcDb.AttributeReference>();
            foreach (_AcDb.ObjectId attOid in blockRef.AttributeCollection)
            {
                var attRef = trans.GetObject(attOid, _AcDb.OpenMode.ForRead) as _AcDb.AttributeReference;
                if (attRef != null)
                {
                    if (!attDefNames.Contains(attRef.Tag))
                        attRefs.Add(attRef);
                }
            }
            return attRefs;
        }

        private static List<string> GetNonConstantAttDefNames(_AcDb.ObjectId btrOid, _AcDb.Transaction trans)
        {
            var names = new List<string>();
            var btr = (_AcDb.BlockTableRecord)trans.GetObject(btrOid, _AcDb.OpenMode.ForRead);
            if (btr.HasAttributeDefinitions)
            {
                foreach (var attOid in btr)
                {
                    _AcDb.AttributeDefinition attDef = attOid.GetObject(_AcDb.OpenMode.ForRead) as _AcDb.AttributeDefinition;
                    if (attDef != null && !attDef.Constant) names.Add(attDef.Tag);
                }
            }
            return names;
        }
    }
}
