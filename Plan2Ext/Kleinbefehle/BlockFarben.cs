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
using System.Globalization;

// ReSharper disable AccessToStaticMemberViaDerivedType

namespace Plan2Ext.Kleinbefehle
{
    public class BlockFarben
    {
        #region log4net Initialization
        // ReSharper disable once InconsistentNaming
        // ReSharper disable once UnusedMember.Local
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(Convert.ToString((typeof(BlockFarben))));
        #endregion

        [_AcTrx.CommandMethod("Plan2BlockFarben")]
        public static void Plan2BlockFarben()
        {
            var doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            _AcEd.Editor ed = _AcAp.Application.DocumentManager.MdiActiveDocument.Editor;
            try
            {
                // Alle Layer tauen, ein, entsperren.
                Globs.UnlockAllLayers();

                short fromColorIndex;
                short toColorIndex;
                if (!GetColorNumbers(ed, out fromColorIndex, out toColorIndex)) return;

                _AcEd.PromptSelectionOptions selOpts =
                    new _AcEd.PromptSelectionOptions {MessageForAdding = "Elemente für Farbänderung wählen: "};
                var res = ed.GetSelection(selOpts);
                if (res.Status != _AcEd.PromptStatus.OK)
                {
                    ed.WriteMessage("\nAuswahl wurde abgebrochen.");
                    return;
                }

                _AcDb.ObjectId[] idArray;
#if BRX_APP
            _AcEd.SelectionSet ss = res.Value;
#else
                using (_AcEd.SelectionSet ss = res.Value)
#endif
                {
                    idArray = ss.GetObjectIds();
                }

                using (_AcDb.Transaction myT = db.TransactionManager.StartTransaction())
                {
                    int level = 0;

                    RecSetColor(fromColorIndex, toColorIndex, idArray, myT, level);
                    myT.Commit();
                }

                ed.Regen();
            }
            catch (Exception ex)
            {
                string msg = string.Format(CultureInfo.CurrentCulture, "Fehler in (Plan2BlockFarben): {0}", ex.Message);
                ed.WriteMessage("\n" + msg);
                // ReSharper disable once LocalizableElement
                System.Windows.Forms.MessageBox.Show(ex.Message, "Plan2BlockFarben");
            }
        }

        private static bool GetColorNumbers(_AcEd.Editor ed, out short fromColorIndex, out short toColorIndex)
        {
            fromColorIndex = 1;
            toColorIndex = 1;

            var inputOptions = new _AcEd.PromptIntegerOptions("")
            {
                Message = "Von Farbe (Nummer): ",
                AllowZero = false,
                AllowNegative = false
            };

            _AcEd.PromptIntegerResult intRes;
            if (!GetColorValue(ed, inputOptions, out intRes)) return false;
            fromColorIndex = (short) intRes.Value;

            inputOptions.Message = "Nach Farbe (Nummer): ";
            if (!GetColorValue(ed, inputOptions, out intRes)) return false;
            toColorIndex = (short) intRes.Value;

            return true;
        }

        private static bool GetColorValue(_AcEd.Editor ed, _AcEd.PromptIntegerOptions inputOptions, out _AcEd.PromptIntegerResult intRes)
        {
            intRes = ed.GetInteger(inputOptions);
            if (!CheckGetIntegerforColorResult(ed, intRes)) return false;
            return true;
        }

        private static bool CheckGetIntegerforColorResult(_AcEd.Editor ed, _AcEd.PromptIntegerResult intRes)
        {
            if (intRes.Status != _AcEd.PromptStatus.OK) return false;
            if (!CheckColorValidity(ed, intRes)) return false;
            return true;
        }

        private static bool CheckColorValidity(_AcEd.Editor ed, _AcEd.PromptIntegerResult intRes)
        {
            if (intRes.Value > 255)
            {
                ed.WriteMessage("\nUngültiger Farbwert.");
                return false;
            }

            return true;
        }

        private static void RecSetColor(short fromColorIndex, short toColorIndex, _AcDb.ObjectId[] idArray, _AcDb.Transaction myT, int level)
        {
            if (level >= 10) return;
            level++;

            foreach (var oid in idArray)
            {
                _AcDb.Entity ent = myT.GetObject(oid, _AcDb.OpenMode.ForRead) as _AcDb.Entity;
                if (ent == null) continue;
                ChangeColor(fromColorIndex, toColorIndex, ent);

                _AcDb.BlockReference blockRef = ent as _AcDb.BlockReference;
                if (blockRef != null)
                {
                    var blockTableRecord = (_AcDb.BlockTableRecord)myT.GetObject(blockRef.BlockTableRecord, _AcDb.OpenMode.ForRead);
                    if (blockTableRecord.IsFromExternalReference)
                    {
                        continue;
                    }

                    var attributes = Globs.GetAttributEntities(blockRef, myT);
                    foreach (var attributeReference in attributes)
                    {
                        ChangeColor(fromColorIndex, toColorIndex, attributeReference);
                    }


                    var innerIdArray = GetOidsAsArray(blockTableRecord);

                    RecSetColor(fromColorIndex, toColorIndex, innerIdArray, myT, level);
                }
            }
        }

        private static _AcDb.ObjectId[] GetOidsAsArray(_AcDb.BlockTableRecord bd)
        {
            var innerIdList = new List<_AcDb.ObjectId>();
            foreach (_AcDb.ObjectId innerOid in bd)
            {
                innerIdList.Add(innerOid);
            }

            var innerIdArray = innerIdList.ToArray();
            return innerIdArray;
        }

        private static void ChangeColor(short fromColorIndex, short toColorIndex, _AcDb.Entity ent)
        {
            var col = ent.Color;
            switch (col.ColorMethod)
            {
                case Autodesk.AutoCAD.Colors.ColorMethod.ByAci:
                    if (col.ColorIndex == fromColorIndex)
                    {
                        ent.UpgradeOpen();
                        var newColour = _AcCm.Color.FromColorIndex(_AcCm.ColorMethod.ByAci, toColorIndex);
                        ent.Color = newColour;
                        ent.DowngradeOpen();
                    }

                    break;
            }
        }
    }
}
