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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace Plan2Ext.Kleinbefehle
{
    public static class AddMissingAttRefsClass
    {
        /// <summary>
        /// From Lisp callable function: Adds Attributereferences to blockrefences (as elements in parameterlist), which are defined in blockdefinition but don't exist yet in blockreference.
        /// </summary>
        /// <param name="rb">(Plan2AddMissingAttRefs (el1 el2 ...))</param>
        /// <remarks>
        /// </remarks>
        /// <returns>true on success</returns>
        [_AcTrx.LispFunction("Plan2AddMissingAttRefs")]
        public static bool Plan2AddMissingAttRefs(_AcDb.ResultBuffer rb)
        {
            _AcEd.Editor ed = _AcAp.Application.DocumentManager.MdiActiveDocument.Editor;
            try
            {
                if (rb == null) return false;
                _AcDb.TypedValue[] values = rb.AsArray();
                if (values == null || values.Length < 3) return false;
                if (values[0].TypeCode != (short)_AcTrx.LispDataType.ListBegin) return false;
                if (values[values.Length - 1].TypeCode != (short)_AcTrx.LispDataType.ListEnd) return false;

                var oids = new List<_AcDb.ObjectId>();
                for (int i = 1; i < values.Length - 1; i++)
                {
                    oids.Add((_AcDb.ObjectId)values[i].Value);
                }
                return AddMissingAttRefs(oids);
            }
            catch (System.Exception ex)
            {
                string msg = string.Format(CultureInfo.CurrentCulture, "Fehler in (Plan2AddMissingAttRefs): {0}", ex.Message);
                ed.WriteMessage("\n" + msg);
                System.Windows.Forms.MessageBox.Show(ex.Message, "Plan2AddMissingAttRefs");
                return false;
            }
        }

        private static bool AddMissingAttRefs(List<_AcDb.ObjectId> oids)
        {
            _AcAp.Document doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            using (var trans = db.TransactionManager.StartTransaction())
            {
                foreach (var oid in oids)
                {
                    var blockRef = trans.GetObject(oid, _AcDb.OpenMode.ForRead) as _AcDb.BlockReference;
                    if (blockRef != null)
                    {
                        AddMissingAttRefs(blockRef, trans);
                    }
                }
                trans.Commit();
            }
            return true;
        }

        private static void AddMissingAttRefs(_AcDb.BlockReference blockRef, _AcDb.Transaction trans)
        {
            var attRefNames = GetAttRefNames(blockRef, trans);
            var attDefs = GetNonConstantAttDefsNotIn(attRefNames, blockRef.BlockTableRecord, trans).Where(x => !attRefNames.Contains(x.Tag)).ToList();
            if (attDefs.Count == 0) return;
            blockRef.UpgradeOpen();
            foreach (var attDef in attDefs)
            {
                AddAttRefFromAttDef(blockRef, attDef, trans);
            }
            blockRef.DowngradeOpen();
        }

        private static void AddAttRefFromAttDef(_AcDb.BlockReference blockRef, _AcDb.AttributeDefinition attDef, _AcDb.Transaction trans)
        {
            var attRef = new _AcDb.AttributeReference();
            attRef.SetAttributeFromBlock(attDef, blockRef.BlockTransform);
            attRef.AdjustAlignment(blockRef.Database);
            blockRef.AttributeCollection.AppendAttribute(attRef);
            trans.AddNewlyCreatedDBObject(attRef, true);
            blockRef.RecordGraphicsModified(true);
        }

        private static List<_AcDb.AttributeDefinition> GetNonConstantAttDefsNotIn(List<string> attRefNames, _AcDb.ObjectId btrOid, _AcDb.Transaction trans)
        {
            var attDefs = new List<_AcDb.AttributeDefinition>();
            var btr = (_AcDb.BlockTableRecord)trans.GetObject(btrOid, _AcDb.OpenMode.ForRead);
            if (btr.HasAttributeDefinitions)
            {
                foreach (var attOid in btr)
                {
                    _AcDb.AttributeDefinition attDef = attOid.GetObject(_AcDb.OpenMode.ForRead) as _AcDb.AttributeDefinition;
                    if (attDef != null && !attDef.Constant) attDefs.Add(attDef);
                }
            }
            return attDefs;
        }

        private static List<string> GetAttRefNames(_AcDb.BlockReference blockRef, _AcDb.Transaction trans)
        {
            var attRefNames = new List<string>();
            foreach (_AcDb.ObjectId attOid in blockRef.AttributeCollection)
            {
                var attRef = trans.GetObject(attOid, _AcDb.OpenMode.ForRead) as _AcDb.AttributeReference;
                if (attRef != null)
                {
                    attRefNames.Add(attRef.Tag);
                }
            }
            return attRefNames;
        }
    }
}
