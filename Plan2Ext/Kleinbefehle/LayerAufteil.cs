using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
using System.Globalization;
using System.Text.RegularExpressions;
#endif

namespace Plan2Ext.Kleinbefehle
{
    public class LayerAufteil
    {
        #region log4net Initialization
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Convert.ToString((typeof(LayerAufteil))));
        #endregion

        private static _AcDb.Transaction _Tr = null;
        private static _AcDb.Database _Db = null;
        [_AcTrx.CommandMethod("Plan2LayerAufteil")]
        public static void Plan2LayerAufteil()
        {
            var acadApp = (Autodesk.AutoCAD.Interop.AcadApplication)_AcAp.Application.AcadApplication;

            _AcAp.Document doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            _Db = doc.Database;
            _AcEd.Editor ed = doc.Editor;
            try
            {
                using (_Tr = _Db.TransactionManager.StartTransaction())
                {
                    var oids = new List<_AcDb.ObjectId>();

                    _AcEd.PromptSelectionOptions selOpts = new _AcEd.PromptSelectionOptions();
                    selOpts.MessageForAdding = "Elemente wählen <Return für alle>: ";
                    var res = ed.GetSelection(selOpts);
                    if (res.Status == _AcEd.PromptStatus.Cancel) return;
                    if (res.Status == _AcEd.PromptStatus.OK)
                    {
#if BRX_APP
                        _AcEd.SelectionSet ss = res.Value;
#else
                        using (_AcEd.SelectionSet ss = res.Value)
#endif
                        {
                            oids.AddRange(ss.GetObjectIds());
                        }
                    }
                    else
                    {
                        GetAllObjectIds(oids);
                    }

                    foreach (_AcDb.ObjectId objId in oids)
                    {
                        _AcDb.Entity ent = (_AcDb.Entity)_Tr.GetObject(objId, _AcDb.OpenMode.ForRead);
                        if (ent.GetType() == typeof(_AcDb.AttributeDefinition) ||
                            ent.GetType() == typeof(_AcDb.DBText) ||
                            ent.GetType() == typeof(_AcDb.MText) ||
                            ent.GetType() == typeof(_AcDb.Leader) ||
                            Regex.IsMatch(ent.GetType().Name, "Dimension", RegexOptions.IgnoreCase)
                            )
                        {
                            Correction(ent, suffix: "_T", isContinuous: true);
                        }
                        else
                        {
                            var blockRef = ent as _AcDb.BlockReference;
                            if (blockRef != null)
                            {
                                Correction(ent, suffix: "_B", isContinuous: true);
                            }
                            else
                            {
                                var hatch = ent as _AcDb.Hatch;
                                if (hatch != null)
                                {
                                    if (Regex.IsMatch(hatch.PatternName, "SOLID", RegexOptions.IgnoreCase))
                                    {
                                        Correction(ent, suffix: "_F", isContinuous: true);
                                    }
                                    else
                                    {
                                        Correction(ent, suffix: "_S", isContinuous: true);
                                    }
                                }
                                else
                                {
                                    var solid = ent as _AcDb.Solid;
                                    if (solid != null)
                                    {
                                        Correction(ent, suffix: "_F", isContinuous: true);
                                    }
                                    else
                                    {
                                        CorrectionRest(ent);
                                    }
                                }
                            }
                        }
                    }

                    _Tr.Commit();
                }
            }
            catch (System.Exception ex)
            {
                string msg = string.Format(CultureInfo.CurrentCulture, "Fehler in (Plan2LayerAufteil): {0}", ex.Message);
                ed.WriteMessage("\n" + msg);
                System.Windows.Forms.MessageBox.Show(ex.Message, "Plan2LayerAufteil");
            }
        }

        private static void GetAllObjectIds(List<_AcDb.ObjectId> oids)
        {
            _AcDb.BlockTable bt = (_AcDb.BlockTable)_Tr.GetObject(_Db.BlockTableId, _AcDb.OpenMode.ForRead);
            foreach (var btrOid in bt)
            {
                _AcDb.BlockTableRecord btr = (_AcDb.BlockTableRecord)_Tr.GetObject(btrOid, _AcDb.OpenMode.ForRead);
                log.InfoFormat("Blocktable: {0}", btr.Name);
                foreach (_AcDb.ObjectId objId in btr)
                {
                    oids.Add(objId);
                }
            }
        }

        [_AcTrx.CommandMethod("Plan2LayerAufteilFarbe")]
        public static void Plan2LayerAufteilFarbe()
        {
            var acadApp = (Autodesk.AutoCAD.Interop.AcadApplication)_AcAp.Application.AcadApplication;

            _AcAp.Document doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            _Db = doc.Database;
            _AcEd.Editor ed = doc.Editor;
            try
            {
                using (_Tr = _Db.TransactionManager.StartTransaction())
                {
                    var oids = new List<_AcDb.ObjectId>();

                    _AcEd.PromptSelectionOptions selOpts = new _AcEd.PromptSelectionOptions();
                    selOpts.MessageForAdding = "Elemente wählen <Return für alle>: ";
                    var res = ed.GetSelection(selOpts);
                    if (res.Status == _AcEd.PromptStatus.Cancel) return;
                    if (res.Status == _AcEd.PromptStatus.OK)
                    {
#if BRX_APP
                        _AcEd.SelectionSet ss = res.Value;
#else
                        using (_AcEd.SelectionSet ss = res.Value)
#endif
                        {
                            oids.AddRange(ss.GetObjectIds());
                        }
                    }
                    else
                    {
                        GetAllObjectIds(oids);
                    }

                    foreach (_AcDb.ObjectId objId in oids)
                    {
                        _AcDb.Entity ent = (_AcDb.Entity)_Tr.GetObject(objId, _AcDb.OpenMode.ForRead);
                        var col = ent.Color;
                        string newLayerName = ent.Layer;
                        _AcCm.Color layerColor = null;
                        switch (col.ColorMethod)
                        {
                            case Autodesk.AutoCAD.Colors.ColorMethod.ByAci:
                                newLayerName += "_" + col.ColorIndex.ToString();
                                layerColor = col;
                                break;
                            case Autodesk.AutoCAD.Colors.ColorMethod.ByBlock:
                                newLayerName += "_" + col.ColorNameForDisplay;
                                break;
                            case Autodesk.AutoCAD.Colors.ColorMethod.ByColor:
                                newLayerName += "_" + col.ColorNameForDisplay.Replace(',', '-').Replace(" ", ""); // 255,255,255 -> 255-255-255; DIC 4 -> DIC4
                                layerColor = col;
                                break;
                            case Autodesk.AutoCAD.Colors.ColorMethod.ByLayer:
                                //newLayerName += "_" + col.ColorNameForDisplay;
                                //layerColor = Plan2Ext.Globs.GetLayerColor(ent.Layer);
                                continue;
                                break;
                            case Autodesk.AutoCAD.Colors.ColorMethod.ByPen:
                            case Autodesk.AutoCAD.Colors.ColorMethod.Foreground:
                            case Autodesk.AutoCAD.Colors.ColorMethod.LayerFrozen:
                            case Autodesk.AutoCAD.Colors.ColorMethod.LayerOff:
                            case Autodesk.AutoCAD.Colors.ColorMethod.None:
                                newLayerName += "_" + col.ColorNameForDisplay;
                                break;
                            default:
                                break;
                        }

                        Plan2Ext.Globs.CreateLayer(newLayerName, layerColor);

                        ent.UpgradeOpen();
                        ent.Layer = newLayerName;
                        ent.Color = _AcCm.Color.FromColorIndex(_AcCm.ColorMethod.ByLayer, 256);
                        ent.DowngradeOpen();
                    }
                    _Tr.Commit();
                }
            }
            catch (System.Exception ex)
            {
                string msg = string.Format(CultureInfo.CurrentCulture, "Fehler in (Plan2LayerAufteilFarbe): {0}", ex.Message);
                ed.WriteMessage("\n" + msg);
                System.Windows.Forms.MessageBox.Show(ex.Message, "Plan2LayerAufteilFarbe");
            }
        }

        private static bool CorrectionRest(_AcDb.Entity ent)
        {
            string suffix = "";

            var ltr = (_AcDb.LayerTableRecord)_Tr.GetObject(ent.LayerId, _AcDb.OpenMode.ForRead);
            var li = new LayerInfo(ltr, _Tr);
            if (IsActuallyContintuous(ent, li))
            {
                suffix = "_L";
                li.NewLineType = "Continuous";
            }
            else
            {
                suffix = "_V";
                li.NewLineType = "Verdeckt";
            }

            if (ent.Layer.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)) return false; // already handled

            li.NewLayer = ent.Layer + suffix;
            li.CreateNewLayer();

            li.UnlockOldLayer();
            ent.UpgradeOpen();
            ent.Layer = li.NewLayer;
            ent.LinetypeId = LayerInfo.GetLinetypeFromName("ByLayer", _Tr, _Db);

            return true;
        }

        private static bool IsActuallyContintuous(_AcDb.Entity ent, LayerInfo li)
        {
            if (string.Compare(ent.Linetype, "ByBlock", true) == 0) return true;
            if (string.Compare(ent.Linetype, "ByLayer", true) == 0)
            {
                if (IsContinuous(li.LineType)) return true;
                else return false;
            }
            if (IsContinuous(ent.Linetype)) return true;
            else return false;
        }

        private static bool IsContinuous(string ltName)
        {
            if (Regex.IsMatch(ltName, "Conti", RegexOptions.IgnoreCase)) return true;
            if (Regex.IsMatch(ltName, "Ausgez", RegexOptions.IgnoreCase)) return true;
            return false;
        }

        private static bool Correction(_AcDb.Entity ent, string suffix, bool isContinuous)
        {
            if (ent.Layer.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)) return false; // already handled

            var ltr = (_AcDb.LayerTableRecord)_Tr.GetObject(ent.LayerId, _AcDb.OpenMode.ForRead);
            var li = new LayerInfo(ltr, _Tr);
            if (isContinuous) li.NewLineType = "Continuous";
            li.NewLayer = ent.Layer + suffix;
            li.CreateNewLayer();
            li.UnlockOldLayer();
            ent.UpgradeOpen();
            ent.Layer = li.NewLayer;



            //const string prefix = "Autodesk.AutoCAD.DatabaseServices.";
            //string typeString = ent.GetType().ToString();
            //if (typeString.Contains(prefix)) typeString = typeString.Substring(prefix.Length);
            //log.Info("\nEntity " + ent.ObjectId.ToString() + " of type " + typeString + " found on layer " +
            //                    ent.Layer + " with colour " + ent.Color.ToString());




            return true;

        }

        private class LayerInfo
        {
            //List<string> HEADER = new List<string>() { "Alter Layer", "Neuer Layer", "Farbe", "Linientyp", "Linienstärke", "Transparenz", "Beschreibung" };
            private _AcDb.LayerTableRecord _Ltr = null;


            public LayerInfo()
            {
            }
            public LayerInfo(_AcDb.LayerTableRecord ltr, _AcDb.Transaction trans)
            {
                _Ltr = ltr;
                OldLayer = ltr.Name;
                NewLayer = "";
                _ColorO = ltr.Color;
                Color = ColorToString();
                _LineTypeO = ltr.LinetypeObjectId;
                _LineType = GetNameFromLinetypeOid(ltr.LinetypeObjectId, trans);
                _LineWeightO = ltr.LineWeight;
                LineWeight = LineWeightToString();
                _TransparencyO = ltr.Transparency;
                if (_TransparencyO != default(_AcCm.Transparency))
                {
                    Transparency = AlphaToTransparenz(_TransparencyO.Alpha).ToString(CultureInfo.InvariantCulture);
                }
                else
                {
                    Transparency = string.Empty;
                }
                if (ltr.IsPlottable) Plot = "Ja";
                else Plot = "Nein";

                Description = ltr.Description;
            }

            private string _NewLineType = "Continuous";
            public string NewLineType
            {
                get { return _NewLineType; }
                set { _NewLineType = value; }
            }

            public void UnlockOldLayer()
            {
                _Ltr.UpgradeOpen();
                _Ltr.IsLocked = false;
            }

            private string _Errors = string.Empty;
            public string Errors { get { return _Errors; } }

            public string OldLayer { get; set; }
            private string _NewLayer = string.Empty;
            public string NewLayer
            {
                get { return _NewLayer; }
                set
                {
                    _NewLayer = value;
                    if (!string.IsNullOrEmpty(OldLayer) && !String.IsNullOrEmpty(_NewLayer)) _Ok = true;
                    else
                    {
                        _Errors = _Errors + string.Format(CultureInfo.CurrentCulture, "\nKein neuer Layer für Layer '{0}'", OldLayer);
                    }
                }
            }
            private _AcCm.Color _ColorO = null;
            private string _Color = string.Empty;
            public string Color
            {
                get { return _Color; }
                set
                {
                    _Color = value;

                    StringToColor();
                }
            }

            private _AcDb.ObjectId _LineTypeO;
            private string _LineType = string.Empty;
            public void SetLineType(string lt, _AcDb.Transaction trans, _AcDb.Database db)
            {
                _LineType = lt;
                var lto = GetLinetypeFromName(_LineType, trans, db);
                if (lto == default(_AcDb.ObjectId))
                {
                    _Errors = _Errors + string.Format(CultureInfo.CurrentCulture, "\nUngültiger Linientyp '{0}' für Layer '{1}'", _LineType, OldLayer);
                    return;
                }

                _LineTypeO = lto;
            }
            public string LineType { get { return _LineType; } }
            private _AcDb.LineWeight _LineWeightO;
            private string _LineWeight = string.Empty;
            public string LineWeight
            {
                get { return _LineWeight; }
                set
                {
                    _LineWeight = value;
                    SetLineWeight();
                }
            }

            private void SetLineWeight()
            {
                _LineWeightO = _AcDb.LineWeight.ByLineWeightDefault;
                if (string.IsNullOrEmpty(_LineWeight))
                {
                    return;
                }
                double d;
                if (!double.TryParse(_LineWeight, NumberStyles.Any, CultureInfo.InvariantCulture, out d))
                {
                    _Errors = _Errors + string.Format(CultureInfo.CurrentCulture, "\nUngültiger Eintrag für Linienstärke für Layer '{0}': {1}", OldLayer, _LineWeight);
                    return;
                }

                d = d * 100.0;
                int val = (int)Math.Floor(d);
                string cmpVal = "LineWeight" + val.ToString().PadLeft(3, '0');

                foreach (var e in Enum.GetValues(typeof(_AcDb.LineWeight)))
                {
                    if (cmpVal == e.ToString())
                    {
                        _LineWeightO = (_AcDb.LineWeight)e;
                        return;
                    }
                }

                _Errors = _Errors + string.Format(CultureInfo.CurrentCulture, "\nUngültiger Eintrag für Linienstärke für Layer '{0}': {1}", OldLayer, _LineWeight);

            }
            private _AcCm.Transparency _TransparencyO;
            private string _Transparency = string.Empty;
            public string Transparency
            {
                get { return _Transparency; }
                set
                {
                    _Transparency = value;
                    SetTransparency();

                }
            }

            private void SetTransparency()
            {
                if (string.IsNullOrEmpty(_Transparency)) return;
                int t;
                if (!int.TryParse(_Transparency, out t))
                {
                    _Errors = _Errors + string.Format(CultureInfo.CurrentCulture, "\nUngültiger Eintrag für Transparenz für Layer '{0}': {1}", OldLayer, _Transparency);
                    return;
                }
                if (t < 0 || t > 90)
                {
                    _Errors = _Errors + string.Format(CultureInfo.CurrentCulture, "\nUngültiger Eintrag für Transparenz für Layer '{0}': {1}", OldLayer, _Transparency);
                    return;
                }
                Byte alpha = TransparenzToAlpha(t);
                _TransparencyO = new _AcCm.Transparency(alpha);
            }

            private bool _IsPlottable = false;
            private string _Plot = string.Empty;
            public string Plot
            {
                get { return _Plot; }
                set
                {
                    _Plot = value;
                    SetPlot();
                }
            }

            private void SetPlot()
            {
                _IsPlottable = false;
                if (string.IsNullOrEmpty(_Plot)) return;

                if (string.Compare(_Plot.Trim(), "Ja", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    _IsPlottable = true;
                }
            }

            public string Description { get; set; }

            private bool _Ok = false;
            public bool Ok { get { return _Ok; } }

            private string LineWeightToString()
            {
                int lw = (int)_LineWeightO;
                if (lw < 0) return "";

                double d = lw / 100.0;
                return d.ToString("F", CultureInfo.InvariantCulture);
            }

            private void StringToColor()
            {
                if (string.IsNullOrEmpty(_Color))
                {
                    _Errors = _Errors + string.Format(CultureInfo.CurrentCulture, "\nFehler in Eintrag für Layer '{0}': Es ist keine Farbe festgelegt!", OldLayer);
                    return;
                }

                var vals = _Color.Split(new char[] { '/' });
                if (vals.Length != 1 && vals.Length != 3)
                {
                    _Errors = _Errors + string.Format(CultureInfo.CurrentCulture, "\nUngültiger Eintrag für Farbe für Layer '{0}': {1}", OldLayer, _Color);
                    return;
                }

                if (vals.Length == 1)
                {
                    byte index;
                    if (!GetColorInt(vals[0], out index)) return;

                    _ColorO = _AcCm.Color.FromColorIndex(_AcCm.ColorMethod.ByColor, index);
                }
                else
                {
                    // rgb
                    byte rIndex, gIndex, bIndex;
                    if (!GetColorInt(vals[0], out rIndex)) return;
                    if (!GetColorInt(vals[1], out gIndex)) return;
                    if (!GetColorInt(vals[2], out bIndex)) return;

                    _ColorO = _AcCm.Color.FromRgb(rIndex, gIndex, bIndex);
                }
            }

            private bool GetColorInt(string val, out byte index)
            {
                if (!byte.TryParse(val, out index))
                {
                    _Errors = _Errors + string.Format(CultureInfo.CurrentCulture, "\nUngültiger Wert für Farbe für Layer '{0}': {1}", OldLayer, _Color);
                    return false;
                }
                if (index < 0 || index > 256)
                {
                    _Errors = _Errors + string.Format(CultureInfo.CurrentCulture, "\nUngültiger Wert für Farbe für Layer '{0}': {1}", OldLayer, _Color);
                    return false;
                }
                return true;
            }


            private string ColorToString()
            {
                if (_ColorO.IsByAci)
                {
                    return _ColorO.ColorIndex.ToString(CultureInfo.InvariantCulture);
                }
                else
                {
                    return string.Format(CultureInfo.InvariantCulture, "{0}/{1}/{2}",
                        _ColorO.Red.ToString(CultureInfo.InvariantCulture),
                        _ColorO.Green.ToString(CultureInfo.InvariantCulture),
                        _ColorO.Blue.ToString(CultureInfo.InvariantCulture));
                }
            }

            internal List<string> RowAsList()
            {
                return new List<string>() { OldLayer, NewLayer, Color, LineType, LineWeight, Transparency, Plot, Description };
            }


            internal void ModifyLayer(_AcDb.LayerTableRecord ltr, _AcDb.Transaction trans, _AcDb.Database db)
            {

                if (_ColorO != null)
                {
                    ltr.Color = _ColorO;
                }

                if (_LineTypeO != null && !_LineTypeO.IsNull)
                {
                    ltr.LinetypeObjectId = _LineTypeO;
                }

                ltr.LineWeight = _LineWeightO;

                if (_TransparencyO != null && _TransparencyO != default(_AcCm.Transparency))
                {
                    ltr.Transparency = _TransparencyO;
                }
                else
                {
                    ltr.Transparency = default(_AcCm.Transparency);
                }

                if (!string.IsNullOrEmpty(Description)) ltr.Description = Description;

                ltr.IsPlottable = _IsPlottable;

            }

            public static _AcDb.ObjectId GetLinetypeFromName(string name, _AcDb.Transaction trans, _AcDb.Database db)
            {
                _AcDb.LinetypeTable acLinTbl;
                acLinTbl = trans.GetObject(db.LinetypeTableId,
                                                _AcDb.OpenMode.ForRead) as _AcDb.LinetypeTable;

                if (acLinTbl.Has(name)) return acLinTbl[name];
                else return default(_AcDb.ObjectId);
            }
            private static string GetNameFromLinetypeOid(_AcDb.ObjectId oid, _AcDb.Transaction trans)
            {
                _AcDb.LinetypeTableRecord ltr = (_AcDb.LinetypeTableRecord)trans.GetObject(oid, _AcDb.OpenMode.ForRead);
                return ltr.Name;

            }
            private static byte TransparenzToAlpha(int transparenz)
            {
                return (Byte)(255 * (100 - transparenz) / 100);
            }
            private static int AlphaToTransparenz(byte alpha)
            {
                return 100 - (100 * alpha / 255);
            }


            internal void CreateNewLayer()
            {
                _AcDb.LayerTable lt = (_AcDb.LayerTable)_Tr.GetObject(_Db.LayerTableId, _AcDb.OpenMode.ForRead, false);
                if (!lt.Has(_NewLayer))
                {
                    _AcDb.LayerTableRecord ltRec = new _AcDb.LayerTableRecord();
                    ltRec.Name = _NewLayer;
                    lt.UpgradeOpen();
                    lt.Add(ltRec);
                    ltRec.LinetypeObjectId = GetLinetypeFromName(_NewLineType, _Tr, _Db);
                    if (ltRec.LinetypeObjectId == default(_AcDb.ObjectId))
                    {
                        log.WarnFormat("Linientyp '{0}' existiert nicht und wird daher Layer '{1}' nicht zugeordnet!", _NewLineType, _NewLayer);
                    }
                    ltRec.Color = _ColorO;
                    _Tr.AddNewlyCreatedDBObject(ltRec, true);
                }
            }


        }
    }
}
