// ReSharper disable CommentTypo
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
using _AcIntCom = BricscadDb;
using _AcInt = BricscadApp;
using Bricscad.Windows;
#elif ARX_APP
using _AcAp = Autodesk.AutoCAD.ApplicationServices;
using _AcCm = Autodesk.AutoCAD.Colors;
using _AcDb = Autodesk.AutoCAD.DatabaseServices;
using _AcGe = Autodesk.AutoCAD.Geometry;
using _AcGi = Autodesk.AutoCAD.GraphicsInterface;
using _AcGs = Autodesk.AutoCAD.GraphicsSystem;
using _AcPl = Autodesk.AutoCAD.PlottingServices;
using _AcBrx = Autodesk.AutoCAD.Runtime;
using _AcTrx = Autodesk.AutoCAD.Runtime;
using _AcWnd = Autodesk.AutoCAD.Windows;
using _AcIntCom = Autodesk.AutoCAD.Interop.Common;
using _AcInt = Autodesk.AutoCAD.Interop;
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
// ReSharper disable StringLiteralTypo

// ReSharper disable IdentifierTypo

namespace Plan2Ext.Vorauswahl
{
    public partial class VorauswahlControl : UserControl
    {
        public VorauswahlControl()
        {
            InitializeComponent();
        }

        internal class EntityItem
        {
            private static Dictionary<string, string> _GermanNameForTypeName = new Dictionary<string, string>()
            {
                {"Arc", "Bogen"},
                {"BlockReference", "Blockreferenz"},
                {"Circle", "Kreis"},
                {"DBPoint", "Punkt"},
                {"DBText", "Text"},
                {"Hatch", "Schraffur"},
                {"Line", "Linie"},
                {"MText", "M-Text"},
                {"MLine", "M-Linie"},
                {"Polyline", "Polylinie"},
                {"Polyline2d", "2D-Polylinie"},
                {"Polyline3d", "3D-Polylinie"},
                {"RotatedDimension", "Gedrehte Bemaßung"},
                {"AlignedDimension", "Ausgerichtete Bemaßung"},
            };

            public Type Type { get; set; }
            public override string ToString()
            {
                return Type != null ? TypeName() : "";
            }

            private string TypeName()
            {
                string name;
                return _GermanNameForTypeName.TryGetValue(Type.Name, out name) ? name : Type.Name;
            }
        }

        private void FillEntityTypesCombobox()
        {
            cmbEntityTypes.Items.Clear();
            foreach (var entityType in GetAllEntityTypesInModelSpace())
            {
                cmbEntityTypes.Items.Add(new EntityItem() { Type = entityType });
            }
        }

        private IEnumerable<Type> GetAllEntityTypesInModelSpace()
        {
            var entityTypes = new HashSet<Type>();
            _AcAp.Document doc = Application.DocumentManager.MdiActiveDocument;
            _AcDb.Database db = doc.Database;
            using (var trans = db.TransactionManager.StartTransaction())
            {
                var frozenLayerIds = new List<_AcDb.ObjectId>();
                _AcDb.LayerTable layTb = (_AcDb.LayerTable)trans.GetObject(db.LayerTableId, _AcDb.OpenMode.ForRead);
                foreach (var ltrOid in layTb)
                {
                    _AcDb.LayerTableRecord ltr = (_AcDb.LayerTableRecord)trans.GetObject(ltrOid, _AcDb.OpenMode.ForRead);
                    if (ltr.IsFrozen) frozenLayerIds.Add(ltrOid);
                }

                _AcDb.BlockTable bt = (_AcDb.BlockTable)trans.GetObject(db.BlockTableId, _AcDb.OpenMode.ForRead);
                _AcDb.BlockTableRecord btr = (_AcDb.BlockTableRecord)trans.GetObject(bt[_AcDb.BlockTableRecord.ModelSpace], _AcDb.OpenMode.ForRead);

                // Iterate through it, dumping objects
                foreach (_AcDb.ObjectId oid in btr)
                {
                    var dbObject = trans.GetObject(oid, _AcDb.OpenMode.ForRead);
                    var ent = dbObject as _AcDb.Entity;
                    if (ent == null) continue;
                    if (frozenLayerIds.Contains(ent.LayerId)) continue;
                    entityTypes.Add(dbObject.GetType());
                }
                trans.Commit();
            }

            return entityTypes;
        }

        // ReSharper disable once UnusedMember.Local
        private IEnumerable<Type> GetAllEntityTypes()
        {
            var assembly = System.Reflection.Assembly.GetAssembly(typeof(_AcDb.Entity));
            return assembly.GetTypes().Where(x => x.IsSubclassOf(typeof(_AcDb.Entity))).OrderBy(x => x.Name);
        }

        private bool _selBlocknamenShield;
        private void btnSelBlocknamen_Click(object sender, EventArgs e)
        {
            if (_selBlocknamenShield) return;
            try
            {
                _selBlocknamenShield = true;

                Globs.CancelCommand();

                Application.DocumentManager.MdiActiveDocument.SendStringToExecute("Plan2VorauswahlSelBlocknamen ", true, false, false);
            }
            catch (Exception ex)
            {
                Application.ShowAlertDialog(ex.Message);
            }
            finally
            {
                _selBlocknamenShield = false;
            }
        }

        private bool _selLayerShield;
        private void btnSelLayer_Click(object sender, EventArgs e)
        {
            if (_selLayerShield) return;
            try
            {
                _selLayerShield = true;

                Globs.CancelCommand();

                Application.DocumentManager.MdiActiveDocument.SendStringToExecute("Plan2VorauswahlSelLayer ", true, false, false);
            }
            catch (Exception ex)
            {
                Application.ShowAlertDialog(ex.Message);
            }
            finally
            {
                _selLayerShield = false;
            }
        }

        private bool _selectShield;
        private void btnSelect_Click(object sender, EventArgs e)
        {
            if (_selectShield) return;
            try
            {
                _selectShield = true;

                //Globs.CancelCommand(); removes pickfirst selection, so disabled

                Application.DocumentManager.MdiActiveDocument.SendStringToExecute("Plan2VorauswahlSelect ", true, false, false);
            }
            catch (Exception ex)
            {
                Application.ShowAlertDialog(ex.Message);
            }
            finally
            {
                _selectShield = false;
            }
        }

        private void lstBlocknamen_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.KeyCode == Keys.Delete)
                {
                    var selItem = lstBlocknamen.SelectedItem;
                    if (selItem == null) return;
                    lstBlocknamen.Items.Remove(selItem);
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void lstLayer_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.KeyCode == Keys.Delete)
                {
                    var selItem = lstLayer.SelectedItem;
                    if (selItem == null) return;
                    lstLayer.Items.Remove(selItem);
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void cmbEntityTypes_SelectedIndexChanged(object sender, EventArgs e)
        {
            var entityItem = (EntityItem)cmbEntityTypes.SelectedItem;
            if (entityItem == null) return;
            if (lstEntityTypes.Items.Contains(entityItem)) return;
            lstEntityTypes.Items.Add(entityItem);
            cmbEntityTypes.SelectedItem = null;
        }

        private void lstEntityTypes_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.KeyCode == Keys.Delete)
                {
                    var selItem = lstEntityTypes.SelectedItem;
                    if (selItem == null) return;
                    lstEntityTypes.Items.Remove(selItem);
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void cmbEntityTypes_DropDown(object sender, EventArgs e)
        {
            FillEntityTypesCombobox();
        }
    }
}
