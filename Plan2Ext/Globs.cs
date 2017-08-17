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
#elif ARX_APP
using _AcAp = Autodesk.AutoCAD.ApplicationServices;
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
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Plan2Ext
{

    internal static class Globs
    {
        #region log4net Initialization
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Convert.ToString((typeof(Globs))));
        static Globs()
        {
            if (log4net.LogManager.GetRepository(System.Reflection.Assembly.GetExecutingAssembly()).Configured == false)
            {
                log4net.Config.XmlConfigurator.ConfigureAndWatch(new System.IO.FileInfo(System.IO.Path.Combine(new System.IO.FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).DirectoryName, "_log4net.config")));
            }

        }
        #endregion

        #region Constants
        internal const string FEHLERBLOCKNAME = "UPDFLA_FEHLER";
        #endregion

        public static int? GetFirstIntInString(string s, out string prefix, out string suffix)
        {
            var cArr = s.ToCharArray();
            var startIndex = s.IndexOfAny(new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' });
            if (startIndex == -1)
            {
                prefix = s; suffix = "";
                return null;
            }
            prefix = s.Substring(0, startIndex);
            int endIndex = startIndex + 1;
            while (endIndex < cArr.Length)
            {
                if (!Char.IsDigit(cArr[endIndex]))
                {
                    suffix = s.Substring(endIndex);
                    return int.Parse(s.Substring(startIndex, endIndex - startIndex));
                }
                endIndex++;
            }
            suffix = "";
            return int.Parse(s.Substring(startIndex));
        }

        public static double? GetFirstDoubleInString(string s, out string prefix, out string suffix)
        {
            var startIndex = s.IndexOfAny(new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' });

            if (startIndex == -1)
            {
                prefix = s; suffix = "";
                return null;
            }
            var cArr = s.ToCharArray();

            if (startIndex > 0)
            {
                if (cArr[startIndex - 1] == '-')
                {
                    startIndex--;
                }
            }

            prefix = s.Substring(0, startIndex);
            int endIndex = startIndex + 1;
            bool comma = false;
            while (endIndex < cArr.Length)
            {
                var c = cArr[endIndex];
                if (c == '.' || c == ',')
                {
                    if (comma)
                    {
                        // comma already happened
                        suffix = s.Substring(endIndex);
                        return double.Parse(s.Substring(startIndex, endIndex - startIndex).Replace(",", "."), CultureInfo.InvariantCulture);
                    }
                    else comma = true;
                }
                else if (!Char.IsDigit(cArr[endIndex]))
                {
                    suffix = s.Substring(endIndex);
                    return double.Parse(s.Substring(startIndex, endIndex - startIndex).Replace(",", "."), CultureInfo.InvariantCulture);
                }
                endIndex++;
            }
            suffix = "";
            return double.Parse(s.Substring(startIndex).Replace(",", "."), CultureInfo.InvariantCulture);
        }
        public static bool InsertFromPrototype(string blockName, string protoDwgName)
        {
            _AcAp.Document doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            string protoDwgFullPath = string.Empty;
            try
            {
                protoDwgFullPath = _AcDb.HostApplicationServices.Current.FindFile(protoDwgName, doc.Database, _AcDb.FindFileHint.Default);
            }
            catch (Exception ex)
            {
                doc.Editor.WriteMessage(string.Format(CultureInfo.CurrentCulture, "\nKonnte Prototypzeichnung '{0}' nicht finden!", protoDwgName));
                return false;
            }
            bool ret = false;
            using (var OpenDb = new _AcDb.Database(false, true))
            {
                OpenDb.ReadDwgFile(protoDwgFullPath, System.IO.FileShare.ReadWrite, true, "");
                var ids = new _AcDb.ObjectIdCollection();

                using (var tr = OpenDb.TransactionManager.StartTransaction())
                {
                    var bt = (_AcDb.BlockTable)tr.GetObject(OpenDb.BlockTableId, _AcDb.OpenMode.ForRead);
                    if (bt.Has(blockName))
                    {
                        ids.Add(bt[blockName]);
                    }
                    else
                    {
                        doc.Editor.WriteMessage(string.Format(CultureInfo.CurrentCulture, "\nDie Prototypzeichnung '{0}' hat keinen Block namens '{1}'!", protoDwgFullPath, blockName));
                    }
                    tr.Commit();
                }

                if (ids.Count > 0)
                {
                    //get the current drawing database
                    var destdb = doc.Database;
                    var iMap = new _AcDb.IdMapping();
                    destdb.WblockCloneObjects(ids, destdb.BlockTableId, iMap, _AcDb.DuplicateRecordCloning.Ignore, deferTranslation: false);
                    ret = true;
                }
            }
            return ret;
        }

        public static _AcDb.ObjectId HandleStringToObjectId(_AcDb.ObjectId oid, string handleString)
        {
            long ln = Convert.ToInt64(handleString, 16);
            var handle = new _AcDb.Handle(ln);
            oid = _AcAp.Application.DocumentManager.MdiActiveDocument.Database.GetObjectId(false, handle, 0);
            return oid;
        }

        public static void DelXrecord(_AcDb.ObjectId id, string key)
        {
            _AcAp.Document doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            _AcDb.Database db = doc.Database;
            using (_AcDb.Transaction tr = db.TransactionManager.StartTransaction())
            {
                _AcDb.Entity ent = tr.GetObject(id, _AcDb.OpenMode.ForRead) as _AcDb.Entity;
                if (ent != null)
                {
                    if (ent.ExtensionDictionary != default(_AcDb.ObjectId))
                    {
                        ent.UpgradeOpen();
                        _AcDb.DBDictionary xDict = (_AcDb.DBDictionary)tr.GetObject(ent.ExtensionDictionary, _AcDb.OpenMode.ForWrite);
                        if (HasKey(xDict,key))
                        {
                            xDict.Remove(key);
                        }
                    }
                }
                tr.Commit();
            }
        }

        private static bool HasKey(_AcDb.DBDictionary xDict, string key)
        {
            foreach (var val in xDict)
            {
                if (val.Key == key) return true;
            }
            return false;
        }

        public static void SetXrecord(_AcDb.ObjectId id, string key, _AcDb.ResultBuffer resbuf)
        {
            _AcAp.Document doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            _AcDb.Database db = doc.Database;
            using (_AcDb.Transaction tr = db.TransactionManager.StartTransaction())
            {
                _AcDb.Entity ent = tr.GetObject(id, _AcDb.OpenMode.ForRead) as _AcDb.Entity;
                if (ent != null)
                {
                    ent.UpgradeOpen();
                    if (ent.ExtensionDictionary == default(_AcDb.ObjectId)) ent.CreateExtensionDictionary();
                    _AcDb.DBDictionary xDict = (_AcDb.DBDictionary)tr.GetObject(ent.ExtensionDictionary, _AcDb.OpenMode.ForWrite);
                    _AcDb.Xrecord xRec = new _AcDb.Xrecord();
                    xRec.Data = resbuf;
                    xDict.SetAt(key, xRec);
                    tr.AddNewlyCreatedDBObject(xRec, true);
                }
                tr.Commit();
            }
        }

        public static _AcDb.ResultBuffer GetXrecord(_AcDb.ObjectId id, string key)
        {
            _AcAp.Document doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            _AcDb.Database db = doc.Database;
            _AcDb.ResultBuffer result = new _AcDb.ResultBuffer();
            using (_AcDb.Transaction tr = db.TransactionManager.StartTransaction())
            {
                _AcDb.Xrecord xRec = new _AcDb.Xrecord();
                _AcDb.Entity ent = tr.GetObject(id, _AcDb.OpenMode.ForRead, false) as _AcDb.Entity;
                if (ent != null)
                {
                    try
                    {
                        if (ent.ExtensionDictionary == default(_AcDb.ObjectId)) return null;
                        _AcDb.DBDictionary xDict = (_AcDb.DBDictionary)tr.GetObject(ent.ExtensionDictionary, _AcDb.OpenMode.ForRead, false);
                        xRec = (_AcDb.Xrecord)tr.GetObject(xDict.GetAt(key), _AcDb.OpenMode.ForRead, false);
                        return xRec.Data;
                    }
                    catch
                    {
                        return null;
                    }
                }
                else
                    return null;
            }
        }

        public static double GetRadRotationTolerance()
        {
            string gradToleranceS;
            if (!Plan2Ext.Globs.GetFromConfig(out gradToleranceS, "alx_V:ino_tuer_90GradTolForBlockRotation")) gradToleranceS = "0.0";

            double gradTolerance;
            if (double.TryParse(gradToleranceS, out gradTolerance))
            {
                return gradTolerance * Math.PI / 180.0;
            }
            return 0.0;
        }


        public static bool IsTextAngleOk(double radAngW, double radTolerance)
        {
            var radAngU = WcsAngToUcsAng(radAngW);
            if (radAngU <= (radTolerance + (Math.PI * 0.5)) || radAngU > (radTolerance + (Math.PI * 1.5))) return true;
            else return false;
        }

        public static double WcsAngToUcsAng(double radAngW)
        {
            var ucsAng = GetUcsDirection();
            var radAngU = radAngW - ucsAng;
            var pid = Math.PI * 2.0;
            while (radAngU < 0.0)
            {
                radAngU += pid;
            }
            while (radAngU > pid)
            {
                radAngU -= pid;
            }
            return radAngU;
        }

        public static double GetUcsDirection()
        {
            var ed = _AcAp.Application.DocumentManager.MdiActiveDocument.Editor;
            _AcGe.Matrix3d ucsCur = ed.CurrentUserCoordinateSystem;
            _AcGe.CoordinateSystem3d cs = ucsCur.CoordinateSystem3d;
            return cs.Xaxis.AngleOnPlane(new _AcGe.Plane(_AcGe.Point3d.Origin, _AcGe.Vector3d.ZAxis));
        }
#if !OLDER_THAN_2015
        public static bool CallCommand(params object[] parameter)
        {
            bool userBreak = false;
            try
            {
                var ed = _AcAp.Application.DocumentManager.MdiActiveDocument.Editor;
                ed.Command(parameter);
            }
            catch (System.Exception ex)
            {
                if (ex.Message == "eUserBreak")
                {
                    userBreak = true;
                }
            }
            return userBreak;
        }

        public static async Task<bool> CallCommandAsync(params object[] parameter)
        {
            bool userBreak = false;
            try
            {
                await CallAsyncCommandSub(parameter);
            }
            catch (System.Exception ex)
            {
                if (ex.Message == "eUserBreak")
                {
                    userBreak = true;
                }
            }
            return userBreak;
        }

        private static async Task CallAsyncCommandSub(params object[] parameter)
        {
            var ed = _AcAp.Application.DocumentManager.MdiActiveDocument.Editor;
            await ed.CommandAsync(parameter);
        }
#endif

        public static bool GetFromConfig(out string val, string varName)
        {
            val = null;
            try
            {
                val = TheConfiguration.GetValueString(varName);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public static _AcGe.Point3d GetViewCtrW()
        {
            var vcS = _AcAp.Application.GetSystemVariable("VIEWCTR").ToString();
            vcS = vcS.Remove(0, 1);
            vcS = vcS.Remove(vcS.Length - 1, 1);
            var arr = vcS.Split(new char[] { ',' });
            var coords = arr.Select(x => double.Parse(x.Trim())).ToArray();
            return new _AcGe.Point3d(coords);
        }

        public static _AcGe.Point3d TransWcsUcs(_AcGe.Point3d point)
        {
            _AcAp.Document doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            _AcDb.Database db = doc.Database;
            _AcEd.Editor ed = doc.Editor;

            _AcGe.Matrix3d newMatrix;
            var ucs = ed.CurrentUserCoordinateSystem;
            var cs = ucs.CoordinateSystem3d;
            newMatrix = _AcGe.Matrix3d.AlignCoordinateSystem(
                cs.Origin,
                cs.Xaxis,
                cs.Yaxis,
                cs.Zaxis,
                _AcGe.Point3d.Origin,
                _AcGe.Vector3d.XAxis,
                _AcGe.Vector3d.YAxis,
                _AcGe.Vector3d.ZAxis
                );


            return point.TransformBy(newMatrix);
        }

        public static _AcGe.Point3d TransUcsWcs(_AcGe.Point3d point)
        {
            _AcAp.Document doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            _AcDb.Database db = doc.Database;
            _AcEd.Editor ed = doc.Editor;

            return point.TransformBy(ed.CurrentUserCoordinateSystem);
        }

        public static void HightLight(_AcDb.ObjectId oid, bool onOff)
        {
            _AcAp.Document doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            _AcDb.Database db = doc.Database;

            using (var trans = doc.TransactionManager.StartTransaction())
            {
                var ent = trans.GetObject(oid, _AcDb.OpenMode.ForRead) as _AcDb.Entity;
                if (onOff) ent.Highlight();
                else ent.Unhighlight();
                trans.Commit();
            }
        }

        public static void CancelCommand()
        {
            _AcAp.Application.DocumentManager.MdiActiveDocument.SendStringToExecute("\x1B", true, false, true);
            _AcAp.Application.DocumentManager.MdiActiveDocument.SendStringToExecute("\x1B", true, false, true);
        }

        public static void ZoomExtents()
        {

            //using InvokeMember to support .NET 3.5

            var acadObject = _AcAp.Application.AcadApplication;

            acadObject.GetType().InvokeMember("ZoomExtents", System.Reflection.BindingFlags.InvokeMethod, null, acadObject, null);

        }

        #region VIEW Save Restore Delete

        public static void SetWorldView()
        {
            {
                // Get the current database
                _AcAp.Document acDoc = _AcAp.Application.DocumentManager.MdiActiveDocument;
                _AcDb.Database acCurDb = acDoc.Database;

                // Start a transaction
                using (_AcDb.Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
                {

                    _AcDb.ViewTableRecord acViewTblRec = new _AcDb.ViewTableRecord();

                    acDoc.Editor.CurrentUserCoordinateSystem = _AcGe.Matrix3d.Identity;

                    acDoc.Editor.SetCurrentView(acViewTblRec);

                    acTrans.Commit();
                }
            }
        }

        public static void SaveView(string viewName)
        {
            if (ViewExists(viewName))
            {
                DeleteView(viewName);
            }
            SaveCurrentView(viewName);
        }

        public static void RestoreView(string viewName)
        {
            {
                // Get the current database
                _AcAp.Document acDoc = _AcAp.Application.DocumentManager.MdiActiveDocument;
                _AcDb.Database acCurDb = acDoc.Database;

                // Start a transaction
                using (_AcDb.Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
                {
                    // Open the View table for read
                    _AcDb.ViewTable acViewTbl;
                    acViewTbl = acTrans.GetObject(acCurDb.ViewTableId,
                                                    _AcDb.OpenMode.ForRead) as _AcDb.ViewTable;

                    // Check to see if the named view 'View1' exists
                    if (acViewTbl.Has(viewName))
                    {

                        _AcDb.ViewTableRecord acViewTblRec = (_AcDb.ViewTableRecord)acTrans.GetObject(acViewTbl[viewName],
                                                            _AcDb.OpenMode.ForWrite);


                        acDoc.Editor.SetCurrentView(acViewTblRec);
                        _AcGe.Matrix3d mat =
                            _AcGe.Matrix3d.AlignCoordinateSystem(
                            new _AcGe.Point3d(0.0, 0.0, 0.0),
                            new _AcGe.Vector3d(1.0, 0, 0),
                            new _AcGe.Vector3d(0, 1.0, 0),
                            new _AcGe.Vector3d(0.0, 0.0, 1.0),
                            acViewTblRec.Ucs.Origin,
                            acViewTblRec.Ucs.Xaxis,
                            acViewTblRec.Ucs.Yaxis,
                            acViewTblRec.Ucs.Zaxis
                            );

                        acDoc.Editor.CurrentUserCoordinateSystem = mat;

                        acTrans.Commit();
                    }
                }
            }
        }

        private static void SaveCurrentView(string viewName)
        {
            {
                // Get the current database
                _AcAp.Document acDoc = _AcAp.Application.DocumentManager.MdiActiveDocument;
                _AcDb.Database acCurDb = acDoc.Database;

                // Start a transaction
                using (_AcDb.Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
                {
                    // Open the View table for read
                    _AcDb.ViewTable acViewTbl;
                    acViewTbl = acTrans.GetObject(acCurDb.ViewTableId,
                                                    _AcDb.OpenMode.ForRead) as _AcDb.ViewTable;

                    // Check to see if the named view 'View1' exists
                    if (acViewTbl.Has(viewName) == false)
                    {
                        // Open the View table for write
                        acViewTbl.UpgradeOpen();


                        _AcDb.ViewTableRecord acViewTblRec = acDoc.Editor.GetCurrentView();
                        acViewTblRec.Name = viewName;
                        // Add the new View table record to the View table and the transaction
                        acViewTbl.Add(acViewTblRec);
                        acTrans.AddNewlyCreatedDBObject(acViewTblRec, true);

                        // Commit the changes
                        acTrans.Commit();
                    }

                }
            }
        }

        public static void DeleteView(string viewName)
        {
            // Get the current database
            _AcAp.Document acDoc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            _AcDb.Database acCurDb = acDoc.Database;

            // Start a transaction
            using (_AcDb.Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the View table for read
                _AcDb.ViewTable acViewTbl;
                acViewTbl = acTrans.GetObject(acCurDb.ViewTableId,
                                                _AcDb.OpenMode.ForRead) as _AcDb.ViewTable;

                // Check to see if the named view 'View1' exists
                if (acViewTbl.Has(viewName))
                {
                    // Open the View table for write
                    acViewTbl.UpgradeOpen();

                    // Get the named view
                    var acViewTblRec = acTrans.GetObject(acViewTbl[viewName],
                                                        _AcDb.OpenMode.ForWrite);

                    // Remove the named view from the View table
                    acViewTblRec.Erase();

                    // Commit the changes
                    acTrans.Commit();
                }

            }
        }

        public static bool ViewExists(string viewName)
        {
            _AcAp.Document acDoc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            _AcDb.Database acCurDb = acDoc.Database;

            bool exists = false;

            using (_AcDb.Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                try
                {
                    _AcDb.ViewTable viewTable = (_AcDb.ViewTable)acTrans.GetObject(acCurDb.ViewTableId, _AcDb.OpenMode.ForRead);
                    return viewTable.Has(viewName);
                }
                finally
                {
                    acTrans.Commit();
                }

            }
        }

        #endregion


        public static void SetLayerCurrent(string layerName)
        {
            // Get the current document and database
            _AcAp.Document acDoc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            _AcDb.Database acCurDb = acDoc.Database;

            // Start a transaction
            using (_AcDb.Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Layer table for read
                _AcDb.LayerTable acLyrTbl;
                acLyrTbl = acTrans.GetObject(acCurDb.LayerTableId,
                                                _AcDb.OpenMode.ForRead) as _AcDb.LayerTable;

                if (acLyrTbl.Has(layerName) == true)
                {
                    // Set the layer Center current

                    var acLyrTblRec = acTrans.GetObject(acLyrTbl[layerName], _AcDb.OpenMode.ForWrite) as _AcDb.LayerTableRecord;
                    if (acLyrTblRec.IsFrozen)
                    {
                        acLyrTblRec.UpgradeOpen();
                        acLyrTblRec.IsFrozen = false;
                    }

                    acCurDb.Clayer = acLyrTbl[layerName];

                    // Save the changes
                    acTrans.Commit();
                }
            }
        }

        public static void UnlockAllLayers()
        {
            var doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            using (_AcDb.Transaction trans = doc.TransactionManager.StartTransaction())
            {
                _AcDb.LayerTable layTb = trans.GetObject(db.LayerTableId, _AcDb.OpenMode.ForRead) as _AcDb.LayerTable;
                foreach (var ltrOid in layTb)
                {
                    _AcDb.LayerTableRecord ltr = (_AcDb.LayerTableRecord)trans.GetObject(ltrOid, _AcDb.OpenMode.ForRead);
                    if (ltr.IsLocked)
                    {
                        ltr.UpgradeOpen();
                        ltr.IsLocked = false;
                    }
                }
                trans.Commit();
            }
        }

        public static void GetNonPlottableLayers(List<string> layerNames)
        {
            var doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            using (_AcDb.Transaction trans = doc.TransactionManager.StartTransaction())
            {
                _AcDb.LayerTable layTb = trans.GetObject(db.LayerTableId, _AcDb.OpenMode.ForRead) as _AcDb.LayerTable;
                List<_AcDb.LayerTableRecord> ltrs = new List<_AcDb.LayerTableRecord>();
                foreach (var ltrOid in layTb)
                {
                    _AcDb.LayerTableRecord ltr = (_AcDb.LayerTableRecord)trans.GetObject(ltrOid, _AcDb.OpenMode.ForRead);
                    if (!ltr.IsPlottable)
                    {
                        layerNames.Add(ltr.Name);
                    }
                }
                trans.Commit();
            }
        }

        public static void LayersOnRestOffAllThawIC(List<string> layerNames)
        {
            var doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            bool needsRegen = false;
            using (_AcDb.Transaction trans = doc.TransactionManager.StartTransaction())
            {
                try
                {
                    _AcDb.LayerTable layTb = trans.GetObject(db.LayerTableId, _AcDb.OpenMode.ForRead) as _AcDb.LayerTable;
                    List<_AcDb.LayerTableRecord> ltrsOnThaw = new List<_AcDb.LayerTableRecord>();
                    List<_AcDb.LayerTableRecord> ltrsOff = new List<_AcDb.LayerTableRecord>();
                    foreach (var ltrOid in layTb)
                    {
                        _AcDb.LayerTableRecord ltr = (_AcDb.LayerTableRecord)trans.GetObject(ltrOid, _AcDb.OpenMode.ForRead);
                        string ok = layerNames.FirstOrDefault(x => string.Compare(ltr.Name, x, StringComparison.OrdinalIgnoreCase) == 0);
                        if (!string.IsNullOrEmpty(ok))
                        {
                            ltrsOnThaw.Add(ltr);
                        }
                        else
                        {
                            ltrsOff.Add(ltr);
                        }
                    }

                    foreach (var ltr in ltrsOnThaw)
                    {
                        log.InfoFormat("Taue und schalte Layer {0} ein.", ltr.Name);
                        ltr.UpgradeOpen();
                        ltr.IsOff = false;
                        if (string.Compare(_AcAp.Application.GetSystemVariable("CLAYER").ToString(), ltr.Name, StringComparison.OrdinalIgnoreCase) != 0)
                        {
                            if (ltr.IsFrozen) needsRegen = true;
                            ltr.IsFrozen = false;
                        }
                    }
                    foreach (var ltr in ltrsOff)
                    {
                        log.InfoFormat("Taue und schalte Layer {0} aus.", ltr.Name);
                        ltr.UpgradeOpen();
                        ltr.IsOff = true;
                        if (string.Compare(_AcAp.Application.GetSystemVariable("CLAYER").ToString(), ltr.Name, StringComparison.OrdinalIgnoreCase) != 0)
                        {
                            if (ltr.IsFrozen) needsRegen = true;
                            ltr.IsFrozen = false;
                        }
                    }
                }
                finally
                {
                    trans.Commit();
                    if (needsRegen)
                    {
                        doc.Editor.Regen();
                    }
                }
            }
        }

        public static bool LayerOnAndThaw(string layerName)
        {
            var doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            bool needsRegen = false;
            using (_AcDb.Transaction trans = doc.TransactionManager.StartTransaction())
            {
                try
                {
                    _AcDb.LayerTable layTb = trans.GetObject(db.LayerTableId, _AcDb.OpenMode.ForRead) as _AcDb.LayerTable;
                    if (!layTb.Has(layerName)) return false;
                    var layId = layTb[layerName];
                    _AcDb.LayerTableRecord ltr = trans.GetObject(layId, _AcDb.OpenMode.ForRead) as _AcDb.LayerTableRecord;
                    log.InfoFormat("Taue und schalte Layer {0} ein.", ltr.Name);
                    ltr.UpgradeOpen();
                    ltr.IsOff = false;
                    if (string.Compare(_AcAp.Application.GetSystemVariable("CLAYER").ToString(), ltr.Name, StringComparison.OrdinalIgnoreCase) != 0)
                    {
                        if (ltr.IsFrozen) needsRegen = true;
                        ltr.IsFrozen = false;
                    }
                    return true;
                }
                finally
                {
                    trans.Commit();
                    if (needsRegen)
                    {
                        doc.Editor.Regen();
                    }
                }
            }
        }

        public static void DrawOrderBottom(_AcDb.ObjectIdCollection ids)
        {
            if (ids.Count == 0) return;
            var doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            using (_AcDb.Transaction tr = doc.TransactionManager.StartTransaction())
            {
                _AcDb.BlockTable bt = (_AcDb.BlockTable)tr.GetObject(db.BlockTableId, _AcDb.OpenMode.ForRead);
                _AcDb.BlockTableRecord btr = (_AcDb.BlockTableRecord)tr.GetObject(bt[_AcDb.BlockTableRecord.ModelSpace], _AcDb.OpenMode.ForRead);

                var dot = (_AcDb.DrawOrderTable)tr.GetObject(btr.DrawOrderTableId, _AcDb.OpenMode.ForWrite);
                dot.MoveToBottom(ids);

                tr.Commit();
            }

        }

        public static void LayerOffRegex(List<string> layerNames)
        {
            var doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            bool needsRegen = false;
            using (_AcDb.Transaction trans = doc.TransactionManager.StartTransaction())
            {
                try
                {
                    _AcDb.LayerTable layTb = trans.GetObject(db.LayerTableId, _AcDb.OpenMode.ForRead) as _AcDb.LayerTable;
                    List<_AcDb.LayerTableRecord> ltrs = new List<_AcDb.LayerTableRecord>();
                    foreach (var ltrOid in layTb)
                    {
                        _AcDb.LayerTableRecord ltr = (_AcDb.LayerTableRecord)trans.GetObject(ltrOid, _AcDb.OpenMode.ForRead);
                        string ok = layerNames.FirstOrDefault(x => Regex.IsMatch(ltr.Name, x, RegexOptions.IgnoreCase));
                        if (!string.IsNullOrEmpty(ok))
                        {
                            ltrs.Add(ltr);
                        }
                    }

                    foreach (var ltr in ltrs)
                    {
                        if (string.Compare(_AcAp.Application.GetSystemVariable("CLAYER").ToString(), ltr.Name, StringComparison.OrdinalIgnoreCase) != 0)
                        {
                            log.InfoFormat("Schalte Layer {0} aus.", ltr.Name);
                            ltr.UpgradeOpen();
                            ltr.IsOff = true;
                        }
                    }
                }
                finally
                {
                    trans.Commit();
                    if (needsRegen)
                    {
                        doc.Editor.Regen();
                    }
                }
            }

        }

        public static void LayerOnAndThawRegex(List<string> layerNames)
        {
            var doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            bool needsRegen = false;
            using (_AcDb.Transaction trans = doc.TransactionManager.StartTransaction())
            {
                try
                {
                    _AcDb.LayerTable layTb = trans.GetObject(db.LayerTableId, _AcDb.OpenMode.ForRead) as _AcDb.LayerTable;
                    List<_AcDb.LayerTableRecord> ltrs = new List<_AcDb.LayerTableRecord>();
                    foreach (var ltrOid in layTb)
                    {
                        _AcDb.LayerTableRecord ltr = (_AcDb.LayerTableRecord)trans.GetObject(ltrOid, _AcDb.OpenMode.ForRead);
                        string ok = layerNames.FirstOrDefault(x => Regex.IsMatch(ltr.Name, x, RegexOptions.IgnoreCase));
                        if (!string.IsNullOrEmpty(ok))
                        {
                            ltrs.Add(ltr);
                        }
                    }

                    foreach (var ltr in ltrs)
                    {
                        log.InfoFormat("Taue und schalte Layer {0} ein.", ltr.Name);
                        ltr.UpgradeOpen();
                        ltr.IsOff = false;
                        if (string.Compare(_AcAp.Application.GetSystemVariable("CLAYER").ToString(), ltr.Name, StringComparison.OrdinalIgnoreCase) != 0)
                        {
                            if (ltr.IsFrozen) needsRegen = true;
                            ltr.IsFrozen = false;
                        }
                    }
                }
                finally
                {
                    trans.Commit();
                    if (needsRegen)
                    {
                        doc.Editor.Regen();
                    }
                }
            }

        }

        public static void PurgeLayer(List<string> layerNames)
        {
            // Get the current document and database
            _AcAp.Document acDoc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            _AcDb.Database acCurDb = acDoc.Database;

            // Start a transaction
            using (_AcDb.Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Layer table for read
                _AcDb.LayerTable acLyrTbl;
                acLyrTbl = acTrans.GetObject(acCurDb.LayerTableId,
                                                _AcDb.OpenMode.ForRead) as _AcDb.LayerTable;
                _AcDb.ObjectIdCollection acObjIdColl = new _AcDb.ObjectIdCollection();
                foreach (var layerName in layerNames)
                {
                    if (acLyrTbl.Has(layerName) == true)
                    {
                        acObjIdColl.Add(acLyrTbl[layerName]);

                    }
                }

                if (acObjIdColl.Count > 0)
                {
                    // Check to see if it is safe to erase layer
                    acCurDb.Purge(acObjIdColl);

                    if (acObjIdColl.Count > 0)
                    {
                        _AcDb.LayerTableRecord acLyrTblRec;

                        foreach (_AcDb.ObjectId oidLayer in acObjIdColl)
                        {
                            acLyrTblRec = acTrans.GetObject(oidLayer, _AcDb.OpenMode.ForWrite) as _AcDb.LayerTableRecord;
                            acLyrTblRec.Erase(true);
                        }
                    }
                }

                // Save the changes and dispose of the transaction
                acTrans.Commit();

            }
        }

        public static Dictionary<string, string> GetAttributes(_AcDb.BlockReference blockRef)
        {
            Dictionary<string, string> valuePerTag = new Dictionary<string, string>();

            _AcAp.Document doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            _AcDb.Database db = doc.Database;
            using (var trans = db.TransactionManager.StartTransaction())
            {

                foreach (_AcDb.ObjectId attId in blockRef.AttributeCollection)
                {
                    var anyAttRef = trans.GetObject(attId, _AcDb.OpenMode.ForRead) as _AcDb.AttributeReference;
                    if (anyAttRef != null)
                    {
                        valuePerTag[anyAttRef.Tag] = anyAttRef.TextString;
                    }
                }
            }
            return valuePerTag;
        }

        public static List<_AcDb.AttributeReference> GetAttributEntities(_AcDb.BlockReference blockRef, _AcDb.Transaction tr)
        {
            var atts = new List<_AcDb.AttributeReference>();

            foreach (_AcDb.ObjectId attId in blockRef.AttributeCollection)
            {
                var anyAttRef = tr.GetObject(attId, _AcDb.OpenMode.ForRead) as _AcDb.AttributeReference;
                if (anyAttRef != null)
                {
                    atts.Add(anyAttRef);
                }
            }
            return atts;
        }

        public static List<string> GetBlockAttributeLayers(string blockName)
        {
            List<string> attLayers = new List<string>();

            _AcAp.Document doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            _AcDb.Database db = doc.Database;
            using (var trans = db.TransactionManager.StartTransaction())
            {
                _AcDb.BlockTable bt = db.BlockTableId.GetObject(_AcDb.OpenMode.ForRead) as _AcDb.BlockTable;
                if (!bt.Has(blockName))
                {
                    log.WarnFormat(CultureInfo.CurrentCulture, "Es existiert kein Block namens '{0}'!", blockName);
                }
                else
                {
                    var oid = bt[blockName];
                    _AcDb.BlockTableRecord btr = (_AcDb.BlockTableRecord)trans.GetObject(oid, _AcDb.OpenMode.ForRead);
                    if (btr.HasAttributeDefinitions)
                    {
                        foreach (var attOid in btr)
                        {
                            _AcDb.AttributeDefinition attDef = attOid.GetObject(_AcDb.OpenMode.ForRead) as _AcDb.AttributeDefinition;
                            if (attDef != null)
                            {
                                var layName = attDef.Layer;
                                if (!attLayers.Contains(layName)) attLayers.Add(layName);
                            }
                        }
                    }
                }
                trans.Commit();
            }
            return attLayers;
        }

        public static List<string> GetBlockAttributeNames(string blockName)
        {
            List<string> attNames = new List<string>();

            _AcAp.Document doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            _AcDb.Database db = doc.Database;
            using (var trans = db.TransactionManager.StartTransaction())
            {
                _AcDb.BlockTable bt = db.BlockTableId.GetObject(_AcDb.OpenMode.ForRead) as _AcDb.BlockTable;
                if (!bt.Has(blockName))
                {
                    log.WarnFormat(CultureInfo.CurrentCulture, "Es existiert kein Block namens '{0}'!", blockName);
                }
                else
                {
                    var oid = bt[blockName];
                    _AcDb.BlockTableRecord btr = (_AcDb.BlockTableRecord)trans.GetObject(oid, _AcDb.OpenMode.ForRead);
                    if (btr.HasAttributeDefinitions)
                    {
                        foreach (var attOid in btr)
                        {
                            _AcDb.AttributeDefinition attDef = attOid.GetObject(_AcDb.OpenMode.ForRead) as _AcDb.AttributeDefinition;
                            if (attDef != null) attNames.Add(attDef.Tag);
                        }
                    }
                }

                trans.Commit();
            }

            return attNames;
        }
        public static bool BlockHasAttribute(string blockName, string attributName)
        {
            bool ok = false;

            _AcAp.Document doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            _AcDb.Database db = doc.Database;
            using (var trans = db.TransactionManager.StartTransaction())
            {
                _AcDb.BlockTable bt = db.BlockTableId.GetObject(_AcDb.OpenMode.ForRead) as _AcDb.BlockTable;
                if (!bt.Has(blockName)) throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Es existiert kein Block namens '{0}'!", blockName));

                var oid = bt[blockName];
                _AcDb.BlockTableRecord btr = (_AcDb.BlockTableRecord)trans.GetObject(oid, _AcDb.OpenMode.ForRead);
                if (!btr.HasAttributeDefinitions) return false;

                foreach (var attOid in btr)
                {
                    _AcDb.AttributeDefinition attDef = attOid.GetObject(_AcDb.OpenMode.ForRead) as _AcDb.AttributeDefinition;
                    if (attDef != null)
                    {
                        if (string.Compare(attDef.Tag, attributName, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            ok = true;
                            break;
                        }
                    }
                }

                trans.Commit();
            }

            return ok;
        }



        public static _AcDb.ObjectId InsertDwg(string fname, _AcGe.Point3d insertPt, double rotation, string blockName)
        {

            log.Debug("InsertDwg");
            _AcAp.Document doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            _AcDb.Database db = doc.Database;
            _AcEd.Editor ed = doc.Editor;

            _AcDb.ObjectId objId;
            _AcDb.ObjectId ret;
            using (_AcDb.Transaction tr = db.TransactionManager.StartTransaction())
            {
                log.Debug("GetBlockTable");
                _AcDb.BlockTable bt = db.BlockTableId.GetObject(_AcDb.OpenMode.ForRead) as _AcDb.BlockTable;
                log.Debug("Get Modelspace");
                _AcDb.BlockTableRecord btrMs = db.CurrentSpaceId.GetObject(_AcDb.OpenMode.ForWrite) as _AcDb.BlockTableRecord;
                log.Debug("Create new Database");
                using (_AcDb.Database dbInsert = new _AcDb.Database(false, true))
                {
                    log.Debug("ReadDwgFile");
                    dbInsert.ReadDwgFile(fname, System.IO.FileShare.Read, true, "");
                    log.Debug("Insert to Db");
                    objId = db.Insert(blockName, dbInsert, true);

                }
                log.Debug("New Blockreference");
                _AcDb.BlockReference bref = new _AcDb.BlockReference(insertPt, objId);
                bref.Rotation = rotation;
                log.Debug("Append to Modelspace");
                btrMs.AppendEntity(bref);
                log.Debug("AddNewlyCreatedDBObject");
                tr.AddNewlyCreatedDBObject(bref, true);

                ret = bref.ObjectId;
                log.Debug("Commit");
                tr.Commit();
            }

            log.Debug("Return InsertDwg");
            return ret;
        }

        public static _AcDb.BlockReference GetBlockFromItsSubentity(_AcDb.Transaction tr, _AcEd.PromptNestedEntityResult nres)
        {
            _AcDb.ObjectId selId = nres.ObjectId;

            List<_AcDb.ObjectId> objIds = new List<_AcDb.ObjectId>(nres.GetContainers());

            objIds.Add(selId);

            objIds.Reverse();

            // following lines needed?

            // Retrieve the sub-entity path for this entity

            //SubentityId subEnt = new SubentityId(SubentityType.Null, 0);

            //FullSubentityPath path = new FullSubentityPath(objIds.ToArray(), subEnt);

            // Open the outermost container, relying on the open

            // transaction...

            _AcDb.Entity subent = tr.GetObject(objIds[0], _AcDb.OpenMode.ForRead, false) as _AcDb.Entity;

            _AcDb.ObjectId eid = subent.OwnerId;

            _AcDb.DBObject bowner = tr.GetObject(eid, _AcDb.OpenMode.ForRead, false) as _AcDb.DBObject;

            return bowner as _AcDb.BlockReference;

        }

        internal static void VerifyLayerExists(string layerName, _AcCm.Color col)
        {
            _AcAp.Document doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            _AcDb.Database db = doc.Database;
            _AcEd.Editor ed = doc.Editor;
            using (_AcDb.Transaction tr = db.TransactionManager.StartTransaction())
            {
                _AcDb.LayerTable lt = (_AcDb.LayerTable)tr.GetObject(db.LayerTableId, _AcDb.OpenMode.ForRead);

                _AcDb.SymbolUtilityServices.ValidateSymbolName(layerName, false);

                if (lt.Has(layerName)) return;

                _AcDb.LayerTableRecord ltr = new _AcDb.LayerTableRecord();
                ltr.Name = layerName;
                if (col != null)
                {
                    ltr.Color = col;
                }

                lt.UpgradeOpen();
                _AcDb.ObjectId ltId = lt.Add(ltr);
                tr.AddNewlyCreatedDBObject(ltr, true);

                tr.Commit();
            }

        }

        internal static void HatchPoly(_AcDb.ObjectId oid, string layer, _AcCm.Color layerCol)
        {
            VerifyLayerExists(layer, layerCol);

            _AcAp.Document doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            _AcDb.Database db = doc.Database;
            _AcEd.Editor ed = doc.Editor;

            string patternName = "_SOLID";
            bool bAssociativity = false;

            using (_AcDb.Transaction myT = db.TransactionManager.StartTransaction())
            {
                _AcDb.ObjectId ModelSpaceId = _AcDb.SymbolUtilityServices.GetBlockModelSpaceId(db);
                _AcDb.BlockTableRecord btr = myT.GetObject(ModelSpaceId, _AcDb.OpenMode.ForWrite) as _AcDb.BlockTableRecord;

                _AcDb.ObjectIdCollection ObjIds = new _AcDb.ObjectIdCollection();
                ObjIds.Add(oid);

                _AcDb.Hatch oHatch = new _AcDb.Hatch();
                _AcGe.Vector3d normal = new _AcGe.Vector3d(0.0, 0.0, 1.0);
                oHatch.Normal = normal;
                oHatch.SetHatchPattern(_AcDb.HatchPatternType.PreDefined, patternName);
                oHatch.Layer = layer;

                btr.AppendEntity(oHatch);
                myT.AddNewlyCreatedDBObject(oHatch, true);

                oHatch.Associative = bAssociativity;
                oHatch.AppendLoop((int)_AcDb.HatchLoopTypes.Default, ObjIds);

                oHatch.EvaluateHatch(true);
                myT.Commit();
            }
        }

        internal static _AcDb.ObjectId HatchPoly(Dictionary<_AcDb.ObjectId, List<_AcDb.ObjectId>> outerInner, string layer, _AcIntCom.AcadAcCmColor col, _AcDb.TransactionManager tm)
        {
            string patternName = "_SOLID";
            bool bAssociativity = false;

            _AcInt.AcadApplication app = (_AcInt.AcadApplication)_AcAp.Application.AcadApplication;
            _AcIntCom.AcadHatch hatchObj = app.ActiveDocument.ModelSpace.AddHatch(0, patternName, bAssociativity, 0);
            hatchObj.TrueColor = col;

            //var outerInnerEnts = new Dictionary<_AcIntCom.AcadEntity, List<_AcIntCom.AcadEntity>>();
            var polysToDelete = new List<_AcIntCom.AcadEntity>();
            foreach (var kvp in outerInner)
            {
                var oid = kvp.Key;
                var inner = kvp.Value;
                _AcIntCom.AcadEntity oCopiedPoly = CopyPoly(oid, tm);
                List<_AcIntCom.AcadEntity> innerPolys = inner.Select(x => CopyPoly(x, tm)).ToList();
                polysToDelete.Add(oCopiedPoly);
                polysToDelete.AddRange(innerPolys);

                //' Create the non associative Hatch object in model space
                _AcIntCom.AcadEntity[] outerLoop = new _AcIntCom.AcadEntity[] { oCopiedPoly };
                hatchObj.AppendOuterLoop(outerLoop);
                try
                {
                    if (innerPolys.Count > 0)
                    {
                        foreach (var innerPoly in innerPolys)
                        {
                            _AcIntCom.AcadEntity[] innerLoop = new _AcIntCom.AcadEntity[] { innerPoly };
                            hatchObj.AppendInnerLoop(innerLoop);
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Warn(ex.Message);
                }
            }

            hatchObj.Evaluate();

            SetLayer((_AcIntCom.AcadEntity)hatchObj, layer);

            foreach (var poly in polysToDelete)
            {
                poly.Delete();
            }

            return new _AcDb.ObjectId((IntPtr)hatchObj.ObjectID);
        }

        internal static _AcDb.ObjectId HatchPoly(_AcDb.ObjectId oid, List<_AcDb.ObjectId> inner, string layer, _AcIntCom.AcadAcCmColor col, _AcDb.TransactionManager tm)
        {
            string patternName = "_SOLID";
            bool bAssociativity = false;

            _AcIntCom.AcadEntity oCopiedPoly = CopyPoly(oid, tm);
            List<_AcIntCom.AcadEntity> innerPolys = inner.Select(x => CopyPoly(x, tm)).ToList();

            //' Create the non associative Hatch object in model space
            _AcInt.AcadApplication app = (_AcInt.AcadApplication)_AcAp.Application.AcadApplication;
            _AcIntCom.AcadHatch hatchObj = app.ActiveDocument.ModelSpace.AddHatch(0, patternName, bAssociativity, 0);
            hatchObj.TrueColor = col;
            _AcIntCom.AcadEntity[] outerLoop = new _AcIntCom.AcadEntity[] { oCopiedPoly };
            hatchObj.AppendOuterLoop(outerLoop);
            try
            {
                if (innerPolys.Count > 0)
                {
                    foreach (var innerPoly in innerPolys)
                    {
                        _AcIntCom.AcadEntity[] innerLoop = new _AcIntCom.AcadEntity[] { innerPoly };
                        hatchObj.AppendInnerLoop(innerLoop);
                    }
                    //_AcIntCom.AcadEntity[] innerLoop = innerPolys.ToArray();
                    //hatchObj.AppendInnerLoop(innerLoop);
                }
            }
            catch (Exception ex)
            {
                log.Warn(ex.Message);
            }
            hatchObj.Evaluate();
            SetLayer((_AcIntCom.AcadEntity)hatchObj, layer);
            if (oCopiedPoly != null) oCopiedPoly.Delete();
            foreach (var poly in innerPolys)
            {
                poly.Delete();
            }

            return new _AcDb.ObjectId((IntPtr)hatchObj.ObjectID);
        }

        private static _AcIntCom.AcadEntity CopyPoly(_AcDb.ObjectId oid, _AcDb.TransactionManager tm)
        {
            _AcIntCom.AcadEntity oPoly = Globs.ObjectIdToAcadEntity(oid, tm);
            _AcIntCom.AcadEntity oCopiedPoly = null;
            if (oPoly is _AcIntCom.AcadPolyline)
            {
                _AcIntCom.AcadPolyline poly1 = (_AcIntCom.AcadPolyline)oPoly;
                oCopiedPoly = (_AcIntCom.AcadEntity)poly1.Copy();
                ((_AcIntCom.AcadPolyline)oCopiedPoly).Closed = true;
            }
            else if (oPoly is _AcIntCom.AcadLWPolyline)
            {
                _AcIntCom.AcadLWPolyline poly2 = (_AcIntCom.AcadLWPolyline)oPoly;
                oCopiedPoly = (_AcIntCom.AcadEntity)poly2.Copy();
                ((_AcIntCom.AcadLWPolyline)oCopiedPoly).Closed = true;
            }
            else // 3dpoly
            {
                _AcIntCom.Acad3DPolyline poly2 = (_AcIntCom.Acad3DPolyline)oPoly;
                oCopiedPoly = (_AcIntCom.AcadEntity)poly2.Copy();
                ((_AcIntCom.Acad3DPolyline)oCopiedPoly).Closed = true;
            }
            return oCopiedPoly;
        }

        private static void SetLayer(_AcIntCom.AcadEntity oCopiedPoly, string layerName)
        {
            Globs.CreateLayer(layerName);
            oCopiedPoly.Layer = layerName;

        }



        internal static void AddFehlerBlock()
        {
            if (Globs.BlockExists(FEHLERBLOCKNAME)) return;

            log.Info("AddFehlerBlock");

            _AcInt.AcadApplication app = (_AcInt.AcadApplication)_AcAp.Application.AcadApplication;
            double[] ip = new double[] { 0.0, 0.0, 0.0 };
            _AcIntCom.IAcadBlock pAcadBlock = app.ActiveDocument.Blocks.Add(ip, FEHLERBLOCKNAME);
            pAcadBlock.AddCircle(ip, 1.0);
            ip[1] = -0.46;
            pAcadBlock.AddCircle(ip, 0.05);
            ip[1] = 0.7;
            double[] ip2 = new double[] { 0.0, -0.19, 0.0 };
            pAcadBlock.AddLine(ip, ip2);
        }

        /// <summary>
        /// Gets centroid even if it lies outside
        /// </summary>
        /// <param name="polyline"></param>
        /// <returns></returns>
        internal static _AcGe.Point3d? GetCentroid(_AcDb.ObjectId polyline)
        {
            _AcGe.Point3d? ret = null;

            var doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            using (var trans = db.TransactionManager.StartTransaction())
            {
                var dbo = trans.GetObject(polyline, _AcDb.OpenMode.ForRead);
                var region = CreateAcadRegion(polyline, db.TransactionManager);
                if (region != null)
                {
                    double[] pt = (double[])region.Centroid;
                    var centroid = new _AcGe.Point3d(pt[0], pt[1], 0.0);
                    ret = centroid;
                    region.Delete();
                }
                //// doesn't work with 12 and 13
                //using (var region = CreateRegion(dbo))
                //{
                //    if (region != null)
                //    {
                //        var centroid = region.GetCentroid((_AcDb.Curve) dbo );
                //        if (centroid.HasValue && region.ContainsPoint(centroid.Value))
                //        {
                //                ret  = centroid.Value;
                //        }
                //    }
                //}
                trans.Commit();
            }

            return ret;
        }

        /// <summary>
        /// Gets centroid unless it lies outside, then null
        /// </summary>
        /// <param name="polyline"></param>
        /// <returns></returns>
        internal static _AcGe.Point3d? GetLabelPoint(_AcDb.ObjectId polyline)
        {
            _AcGe.Point3d? ret = null;

            var doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            using (var trans = db.TransactionManager.StartTransaction())
            {
                var dbo = trans.GetObject(polyline, _AcDb.OpenMode.ForRead);
                var region = CreateAcadRegion(polyline, db.TransactionManager);
                if (region != null)
                {
                    double[] pt = (double[])region.Centroid;
                    var centroid = new _AcGe.Point3d(pt[0], pt[1], 0.0);
                    if (AreaEngine.InPoly(centroid, dbo as _AcDb.Entity))
                    {
                        ret = centroid;
                    }
                    region.Delete();
                }
                //// doesn't work with 12 and 13
                //using (var region = CreateRegion(dbo))
                //{
                //    if (region != null)
                //    {
                //        var centroid = region.GetCentroid((_AcDb.Curve) dbo );
                //        if (centroid.HasValue && region.ContainsPoint(centroid.Value))
                //        {
                //                ret  = centroid.Value;
                //        }
                //    }
                //}
                trans.Commit();
            }

            return ret;
        }

        /// <summary>
        /// Gets centroid unless it lies outside oder Region can't be created, in this case the startpoint of the curve will be returned
        /// </summary>
        /// <param name="polyline"></param>
        /// <returns></returns>
        internal static _AcGe.Point3d? GetLabelOrStartPoint(_AcDb.ObjectId polyline)
        {
            _AcGe.Point3d? ret = null;

            var doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            using (var trans = db.TransactionManager.StartTransaction())
            {
                var dbo = trans.GetObject(polyline, _AcDb.OpenMode.ForRead);
                var region = CreateAcadRegion(polyline, db.TransactionManager);
                if (region != null)
                {
                    double[] pt = (double[])region.Centroid;
                    var centroid = new _AcGe.Point3d(pt[0], pt[1], 0.0);
                    if (AreaEngine.InPoly(centroid, dbo as _AcDb.Entity))
                    {
                        ret = centroid;
                    }
                    region.Delete();
                }

                if (ret == null)
                {
                    _AcDb.Curve curve = dbo as _AcDb.Curve;
                    if (curve != null)
                    {
                        var startParam = curve.StartParam;
                        var endParam = curve.EndParam;
                        var length = curve.GetDistanceAtParameter(endParam) - curve.GetDistanceAtParameter(startParam);
                        ret = curve.GetPointAtDist(length / 2.0);
                    }
                }

                trans.Commit();
            }

            return ret;
        }

        internal static _AcGe.Point3d? GetMiddlePoint(_AcDb.ObjectId polyline)
        {
            _AcGe.Point3d? ret = null;

            var doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            using (var trans = db.TransactionManager.StartTransaction())
            {
                _AcDb.Curve curve = (_AcDb.Curve)trans.GetObject(polyline, _AcDb.OpenMode.ForRead); ;
                {
                    var startParam = curve.StartParam;
                    var endParam = curve.EndParam;
                    var length = curve.GetDistanceAtParameter(endParam) - curve.GetDistanceAtParameter(startParam);
                    ret = curve.GetPointAtDist(length / 2.0);
                }

                trans.Commit();
            }

            return ret;
        }

        internal static _AcGe.Point3d? GetStartPoint(_AcDb.ObjectId polyline)
        {
            _AcGe.Point3d? ret = null;

            var doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            using (var trans = db.TransactionManager.StartTransaction())
            {
                _AcDb.Curve curve = (_AcDb.Curve)trans.GetObject(polyline, _AcDb.OpenMode.ForRead); ;
                {
                    ret = curve.GetPointAtDist(0.0);
                }

                trans.Commit();
            }

            return ret;
        }

        // doesn't work with acad12 and acad13 (no AreaProperties)
        //internal static _AcGe.Point3d? GetCentroid(this Region reg, Curve cur = null)
        //{
        //    if (cur == null)
        //    {
        //        var idc = new _AcDb.DBObjectCollection();
        //        reg.Explode(idc);
        //        if (idc.Count == 0)
        //            return null;
        //        cur = idc[0] as Curve;
        //    }

        //    if (cur == null)
        //        return null;

        //    var cs = cur.GetPlane().GetCoordinateSystem();

        //    //var cs = reg.GetPlane().GetCoordinateSystem();

        //    var o = cs.Origin;
        //    var x = cs.Xaxis;
        //    var y = cs.Yaxis;

        //    var a = reg.AreaProperties(ref o, ref x, ref y);
        //    var pl = new Plane(o, x, y);
        //    return pl.EvaluatePoint(a.Centroid);
        //}

        internal static _AcIntCom.AcadRegion CreateAcadRegion(_AcDb.ObjectId elFG, _AcDb.TransactionManager tm)
        {
            _AcIntCom.AcadRegion region = default(_AcIntCom.AcadRegion);
            _AcInt.AcadApplication app = (_AcInt.AcadApplication)_AcAp.Application.AcadApplication;
            _AcIntCom.AcadEntity oldCurve = Globs.ObjectIdToAcadEntity(elFG, (_AcDb.TransactionManager)tm);

            _AcIntCom.AcadEntity[] curves = new _AcIntCom.AcadEntity[] { oldCurve };
            try
            {
#if BRX_APP
                System.Object[] objs = (System.Object[])app.ActiveDocument.database.ModelSpace.AddRegion(curves);
#else
                System.Object[] objs = (System.Object[])app.ActiveDocument.Database.ModelSpace.AddRegion(curves);
#endif
                if (objs == null || objs.Length == 0) return null;
                region = (_AcIntCom.AcadRegion)objs[0];
                return region;
            }
            catch (System.Exception)
            {
                return null;
            }

        }

        internal static _AcDb.Region CreateRegion(_AcDb.DBObject dbo)
        {
            _AcDb.DBObjectCollection acDBObjColl = new _AcDb.DBObjectCollection();
            acDBObjColl.Add(dbo);

            try
            {
                var objColl = _AcDb.Region.CreateFromCurves(acDBObjColl);
                if (objColl.Count > 0)
                {
                    return objColl[0] as _AcDb.Region;
                }
            }
            catch (Exception)
            {
            }

            return null;

        }

        internal static void InsertFehlerLines(List<object> insPoints, string layerName, double length = 50, double ang = Math.PI * 1.25, _AcCm.Color col = null)
        {
            var insp = insPoints.Select(c =>
            {
                double[] p = (double[])c;
                return new _AcGe.Point3d(p[0], p[1], p[2]);

            }).ToList();
            InsertFehlerLines(insp, layerName, length, ang, col);
        }

        internal static void InsertFehlerLines(List<_AcGe.Point3d> insPoints, string layerName, double length = 50, double ang = Math.PI * 1.25, _AcCm.Color col = null)
        {
            log.Info("InsertFehlerLines");

            if (insPoints.Count == 0) return;

            Globs.CreateLayer(layerName);

            var doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;

            ang += GetUcsAngle();

            using (var tr = db.TransactionManager.StartTransaction())
            {
                _AcDb.BlockTable blockTable = (_AcDb.BlockTable)db.BlockTableId.GetObject(_AcDb.OpenMode.ForRead, false);
                _AcDb.BlockTableRecord btr = (_AcDb.BlockTableRecord)blockTable[_AcDb.BlockTableRecord.ModelSpace].GetObject(_AcDb.OpenMode.ForWrite, false);
                foreach (var ip in insPoints)
                {
                    _AcGe.Vector3d v3d = new _AcGe.Vector3d(length, 0.0, 0.0);
                    v3d = v3d.RotateBy(ang, new _AcGe.Vector3d(0.0, 0.0, 1.0));
                    var p2 = ip.Add(v3d);

                    _AcDb.Line line = new _AcDb.Line(ip, p2);
                    if (col != null) line.Color = col;
                    btr.AppendEntity(line);
                    tr.AddNewlyCreatedDBObject(line, true);

                    line.Layer = layerName;

                }
                tr.Commit();
            }
        }

        /// <summary>
        /// If applicable returns UCS-Angle otherwise 0.0
        /// </summary>
        /// <returns></returns>
        private static double GetUcsAngle()
        {
            var doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;


            double ucsAngle = 0.0;
            if (db.Ucsxdir.Y != 0.0)
            {
                ucsAngle = Math.Atan2(db.Ucsxdir.Y, db.Ucsxdir.X);
            }
            return ucsAngle;
        }

        internal static void InsertFehlerBlocks(List<object> insPoints, string layerName)
        {
            log.Info("InsertFehlerBlocks");

            AddFehlerBlock();
            _AcInt.AcadApplication app = (_AcInt.AcadApplication)_AcAp.Application.AcadApplication;
            Globs.CreateLayer(layerName);
            foreach (var ip in insPoints)
            {
                _AcIntCom.AcadBlockReference fehlerBlockObj = app.ActiveDocument.ModelSpace.InsertBlock(ip, FEHLERBLOCKNAME, 1.0, 1.0, 1.0, 0.0, Type.Missing);
                fehlerBlockObj.Layer = layerName;
            }

        }

        internal static void DeleteHatches(string layerName)
        {
            log.InfoFormat(CultureInfo.CurrentCulture, "DeleteHatches", layerName);

            var hatches = SelectHatches(layerName);
            if (hatches.Count == 0) return;

            _AcDb.TransactionManager tm = _AcAp.Application.DocumentManager.MdiActiveDocument.Database.TransactionManager;
            using (_AcDb.Transaction myT = tm.StartTransaction())
            {

                foreach (var id in hatches)
                {
                    using (_AcDb.DBObject obj = tm.GetObject(id, _AcDb.OpenMode.ForWrite, false))
                    {
                        obj.Erase();
                    }
                }

                myT.Commit();
            }

        }

        internal static List<_AcDb.ObjectId> SelectHatches(string layerName)
        {
            List<_AcDb.ObjectId> hatches = new List<_AcDb.ObjectId>();

            var ed = _AcAp.Application.DocumentManager.MdiActiveDocument.Editor;
            _AcEd.SelectionFilter filter = new _AcEd.SelectionFilter(new _AcDb.TypedValue[] { 
                new _AcDb.TypedValue((int)_AcDb.DxfCode.Start,"HATCH" ),
                new _AcDb.TypedValue((int)_AcDb.DxfCode.LayerName, layerName)
            });

            _AcEd.PromptSelectionResult res = ed.SelectAll(filter);
            if (res.Status == _AcEd.PromptStatus.OK)
            {
#if BRX_APP
                _AcEd.SelectionSet ss = res.Value;
#else
                using (_AcEd.SelectionSet ss = res.Value)
#endif
                {
                    hatches.AddRange(ss.GetObjectIds().ToList());
                }
            }

            return hatches;

        }

        internal static void DeleteFehlerLines(string layerName)
        {
            log.Info("DeleteFehlerLines");

            var fehlerLines = SelectFehlerLines(layerName);
            if (fehlerLines.Count == 0) return;

            _AcDb.TransactionManager tm = _AcAp.Application.DocumentManager.MdiActiveDocument.Database.TransactionManager;
            using (_AcDb.Transaction myT = tm.StartTransaction())
            {

                foreach (var id in fehlerLines)
                {
                    using (_AcDb.DBObject obj = tm.GetObject(id, _AcDb.OpenMode.ForWrite, false))
                    {
                        obj.Erase();
                    }
                }

                myT.Commit();
            }

        }

        internal static List<_AcDb.ObjectId> SelectFehlerLines(string layerName)
        {
            List<_AcDb.ObjectId> fehlerLines = new List<_AcDb.ObjectId>();

            var ed = _AcAp.Application.DocumentManager.MdiActiveDocument.Editor;
            _AcEd.SelectionFilter filter = new _AcEd.SelectionFilter(new _AcDb.TypedValue[] { 
                new _AcDb.TypedValue((int)_AcDb.DxfCode.Start,"LINE" ),
                new _AcDb.TypedValue((int)_AcDb.DxfCode.LayerName, layerName)
            });

            _AcEd.PromptSelectionResult res = ed.SelectAll(filter);
            if (res.Status == _AcEd.PromptStatus.OK)
            {
#if BRX_APP
                _AcEd.SelectionSet ss = res.Value;
#else
                using (_AcEd.SelectionSet ss = res.Value)
#endif
                {
                    fehlerLines.AddRange(ss.GetObjectIds().ToList());
                }
            }
            return fehlerLines;
        }

        internal static void DeleteFehlerBlocks(string layerName)
        {
            log.Info("DeleteFehlerBlocks");

            var fehlerBlocks = SelectFehlerBlocks(layerName);
            if (fehlerBlocks.Count == 0) return;

            _AcDb.TransactionManager tm = _AcAp.Application.DocumentManager.MdiActiveDocument.Database.TransactionManager;
            using (_AcDb.Transaction myT = tm.StartTransaction())
            {

                foreach (var id in fehlerBlocks)
                {
                    using (_AcDb.DBObject obj = tm.GetObject(id, _AcDb.OpenMode.ForWrite, false))
                    {
                        obj.Erase();
                    }
                }

                myT.Commit();
            }

        }

        internal static List<_AcDb.ObjectId> SelectFehlerBlocks(string layerName)
        {
            List<_AcDb.ObjectId> fehlerBlocks = new List<_AcDb.ObjectId>();

            string hkBlockName = FEHLERBLOCKNAME;
            var ed = _AcAp.Application.DocumentManager.MdiActiveDocument.Editor;
            _AcEd.SelectionFilter filter = new _AcEd.SelectionFilter(new _AcDb.TypedValue[] { 
                new _AcDb.TypedValue((int)_AcDb.DxfCode.Start,"INSERT" ),
                new _AcDb.TypedValue((int)_AcDb.DxfCode.BlockName,hkBlockName),
                new _AcDb.TypedValue((int)_AcDb.DxfCode.LayerName, layerName)
            });

            _AcEd.PromptSelectionResult res = ed.SelectAll(filter);
            if (res.Status == _AcEd.PromptStatus.OK)
            {
#if BRX_APP
                _AcEd.SelectionSet ss = res.Value;
#else
                using (_AcEd.SelectionSet ss = res.Value)
#endif

                {
                    fehlerBlocks.AddRange(ss.GetObjectIds().ToList());
                }
            }

            return fehlerBlocks;

        }

        internal static List<_AcDb.ObjectId> UserSelectFehlerBlocks(string layerName)
        {
            List<_AcDb.ObjectId> fehlerBlocks = new List<_AcDb.ObjectId>();

            string hkBlockName = FEHLERBLOCKNAME;
            var ed = _AcAp.Application.DocumentManager.MdiActiveDocument.Editor;
            _AcEd.SelectionFilter filter = new _AcEd.SelectionFilter(new _AcDb.TypedValue[] { 
                new _AcDb.TypedValue((int)_AcDb.DxfCode.Start,"INSERT" ),
                new _AcDb.TypedValue((int)_AcDb.DxfCode.BlockName,hkBlockName),
                new _AcDb.TypedValue((int)_AcDb.DxfCode.LayerName, layerName)
            });

            _AcEd.PromptSelectionResult res = ed.GetSelection(filter);
            if (res.Status == _AcEd.PromptStatus.OK)
            {
#if BRX_APP
                _AcEd.SelectionSet ss = res.Value;
#else
                using (_AcEd.SelectionSet ss = res.Value)
#endif

                {
                    fehlerBlocks.AddRange(ss.GetObjectIds().ToList());
                }
            }

            return fehlerBlocks;

        }

        internal static bool BlockExists(string blockName)
        {
            log.Info(string.Format(CultureInfo.InvariantCulture, "BlockExists: {0}", blockName));

            _AcDb.Database db = _AcAp.Application.DocumentManager.MdiActiveDocument.Database;
            _AcDb.TransactionManager tm = db.TransactionManager;
            using (_AcDb.Transaction myT = tm.StartTransaction())
            {
                using (_AcDb.BlockTable bt = (_AcDb.BlockTable)tm.GetObject(db.BlockTableId, _AcDb.OpenMode.ForRead, false))
                {
                    return (bt.Has(blockName));
                }
            }
        }

#if ARX_APP
        internal static object GetLispVariable(string name)
        {
            _AcDb.ResultBuffer rbInvoke = new _AcDb.ResultBuffer();
            rbInvoke.Add(new _AcDb.TypedValue((int)(_AcBrx.LispDataType.Text), "alx_F:ino_EvalLispVariable"));
            rbInvoke.Add(new _AcDb.TypedValue((int)(_AcBrx.LispDataType.Text), name));
            _AcDb.ResultBuffer resInvoke = _AcAp.Application.Invoke(rbInvoke);
            rbInvoke.Dispose();

            object o = GetRbValues(resInvoke);

            resInvoke.Dispose();

            return o;

        }

        internal static object LispFindFile(string configName)
        {
            _AcDb.ResultBuffer rbInvoke = new _AcDb.ResultBuffer();
            rbInvoke.Add(new _AcDb.TypedValue((int)(_AcBrx.LispDataType.Text), "alx_F:ino_LispFindFile"));
            rbInvoke.Add(new _AcDb.TypedValue((int)(_AcBrx.LispDataType.Text), configName));
            _AcDb.ResultBuffer resInvoke = _AcAp.Application.Invoke(rbInvoke);
            rbInvoke.Dispose();

            object o = GetRbValues(resInvoke);

            resInvoke.Dispose();

            return o;

        }

        internal static object LispTryLoadGlobs(string configName)
        {
            _AcDb.ResultBuffer rbInvoke = new _AcDb.ResultBuffer();
            rbInvoke.Add(new _AcDb.TypedValue((int)(_AcBrx.LispDataType.Text), "alx_F:ino_TryLoadGlobs"));
            rbInvoke.Add(new _AcDb.TypedValue((int)(_AcBrx.LispDataType.Text), configName));
            _AcDb.ResultBuffer resInvoke = _AcAp.Application.Invoke(rbInvoke);
            rbInvoke.Dispose();

            object o = GetRbValues(resInvoke);

            resInvoke.Dispose();

            return o;

        }
#endif
        internal static object GetRbValues(_AcDb.ResultBuffer resInvoke)
        {
            if (resInvoke == null)
            {
                log.Error("resInvoke == null!");
                return null;
            }

            List<_AcDb.TypedValue> vals = new List<_AcDb.TypedValue>();
            foreach (_AcDb.TypedValue val in resInvoke)
            {
                vals.Add(val);
            }
            if (vals.Count == 0) return null;
            else if (vals.Count == 1)
            {
                return vals[0].Value;
            }
            else
            {
                return vals.Select(x => x.Value);
            }

        }

        //http://through-the-interface.typepad.com/through_the_interface/2008/06/zooming-to-a-wi.html
        internal static void ZoomWin(_AcEd.Editor ed, _AcGe.Point3d min, _AcGe.Point3d max)
        {
            // funkt nicht 
            _AcGe.Point2d min2d = new _AcGe.Point2d(min.X, min.Y);
            _AcGe.Point2d max2d = new _AcGe.Point2d(max.X, max.Y);
            _AcDb.ViewTableRecord view =
              new _AcDb.ViewTableRecord();
            view.CenterPoint = min2d + ((max2d - min2d) / 2.0);
            view.Height = max2d.Y - min2d.Y;
            view.Width = max2d.X - min2d.X;
            ed.SetCurrentView(view);

        }

        internal static void ZoomWin2(_AcEd.Editor ed, _AcGe.Point3d min, _AcGe.Point3d max)
        {
            // funkt nicht
            _AcInt.AcadApplication app = (_AcInt.AcadApplication)_AcAp.Application.AcadApplication;
            double[] lower = new double[3] { min.X, min.Y, min.Z };
            double[] upper = new double[3] { max.X, max.Y, max.Z };
            app.ZoomWindow(lower, upper);
        }


        internal static void ZoomWin3(_AcEd.Editor ed, _AcGe.Point3d min, _AcGe.Point3d max)
        {

            string lower = min.ToString(CultureInfo.InvariantCulture);
            lower = lower.Substring(1, lower.Length - 2);
            string upper = max.ToString(CultureInfo.InvariantCulture);
            upper = upper.Substring(1, lower.Length - 2);


            string cmd = "_.ZOOM _W " + lower + " " + upper + " ";

            ed.Document.SendStringToExecute(cmd, true, false, true);

        }








        //        The parameters of the Zoom procedure are: 

        //Minimum point - 3D point used to define the lower-left corner of the area to display. 
        //Maximum point - 3D point used to define the upper-right corner of the area to display. 
        //Center point - 3D point used to define the center of a view. 
        //Scale factor - Real number used to specify the scale to increase or decrease the size of a view. 
        //http://docs.autodesk.com/ACD/2010/ENU/AutoCAD%20.NET%20Developer's%20Guide/index.html?url=WS1a9193826455f5ff2566ffd511ff6f8c7ca-433b.htm,topicNumber=d0e5984
        internal static void Zoom(_AcGe.Point3d pMin, _AcGe.Point3d pMax, _AcGe.Point3d pCenter, double dFactor)
        {
            // funkt nicht
            // Get the current document and database
            _AcAp.Document acDoc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            _AcDb.Database acCurDb = acDoc.Database;

            int nCurVport = System.Convert.ToInt32(_AcAp.Application.GetSystemVariable("CVPORT"));

            // Get the extents of the current space no points 
            // or only a center point is provided
            // Check to see if Model space is current
            if (acCurDb.TileMode == true)
            {
                if (pMin.Equals(new _AcGe.Point3d()) == true &&
                    pMax.Equals(new _AcGe.Point3d()) == true)
                {
                    pMin = acCurDb.Extmin;
                    pMax = acCurDb.Extmax;
                }
            }
            else
            {
                // Check to see if Paper space is current
                if (nCurVport == 1)
                {
                    // Get the extents of Paper space
                    if (pMin.Equals(new _AcGe.Point3d()) == true &&
                        pMax.Equals(new _AcGe.Point3d()) == true)
                    {
                        pMin = acCurDb.Pextmin;
                        pMax = acCurDb.Pextmax;
                    }
                }
                else
                {
                    // Get the extents of Model space
                    if (pMin.Equals(new _AcGe.Point3d()) == true &&
                        pMax.Equals(new _AcGe.Point3d()) == true)
                    {
                        pMin = acCurDb.Extmin;
                        pMax = acCurDb.Extmax;
                    }
                }
            }

            // Start a transaction
            using (_AcDb.Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Get the current view
                using (_AcDb.ViewTableRecord acView = acDoc.Editor.GetCurrentView())
                {
                    _AcDb.Extents3d eExtents;

                    // Translate WCS coordinates to DCS
                    _AcGe.Matrix3d matWCS2DCS;
                    matWCS2DCS = _AcGe.Matrix3d.PlaneToWorld(acView.ViewDirection);
                    matWCS2DCS = _AcGe.Matrix3d.Displacement(acView.Target - _AcGe.Point3d.Origin) * matWCS2DCS;
                    matWCS2DCS = _AcGe.Matrix3d.Rotation(-acView.ViewTwist,
                                                   acView.ViewDirection,
                                                   acView.Target) * matWCS2DCS;

                    // If a center point is specified, define the min and max 
                    // point of the extents
                    // for Center and Scale modes
                    if (pCenter.DistanceTo(_AcGe.Point3d.Origin) != 0)
                    {
                        pMin = new _AcGe.Point3d(pCenter.X - (acView.Width / 2),
                                           pCenter.Y - (acView.Height / 2), 0);

                        pMax = new _AcGe.Point3d((acView.Width / 2) + pCenter.X,
                                           (acView.Height / 2) + pCenter.Y, 0);
                    }

                    // Create an extents object using a line
                    using (_AcDb.Line acLine = new _AcDb.Line(pMin, pMax))
                    {
                        eExtents = new _AcDb.Extents3d(acLine.Bounds.Value.MinPoint,
                                                 acLine.Bounds.Value.MaxPoint);
                    }

                    // Calculate the ratio between the width and height of the current view
                    double dViewRatio;
                    dViewRatio = (acView.Width / acView.Height);

                    // Tranform the extents of the view
                    matWCS2DCS = matWCS2DCS.Inverse();
                    eExtents.TransformBy(matWCS2DCS);

                    double dWidth;
                    double dHeight;
                    _AcGe.Point2d pNewCentPt;

                    // Check to see if a center point was provided (Center and Scale modes)
                    if (pCenter.DistanceTo(_AcGe.Point3d.Origin) != 0)
                    {
                        dWidth = acView.Width;
                        dHeight = acView.Height;

                        if (dFactor == 0)
                        {
                            pCenter = pCenter.TransformBy(matWCS2DCS);
                        }

                        pNewCentPt = new _AcGe.Point2d(pCenter.X, pCenter.Y);
                    }
                    else // Working in Window, Extents and Limits mode
                    {
                        // Calculate the new width and height of the current view
                        dWidth = eExtents.MaxPoint.X - eExtents.MinPoint.X;
                        dHeight = eExtents.MaxPoint.Y - eExtents.MinPoint.Y;

                        // Get the center of the view
                        pNewCentPt = new _AcGe.Point2d(((eExtents.MaxPoint.X + eExtents.MinPoint.X) * 0.5),
                                                 ((eExtents.MaxPoint.Y + eExtents.MinPoint.Y) * 0.5));
                    }

                    // Check to see if the new width fits in current window
                    if (dWidth > (dHeight * dViewRatio)) dHeight = dWidth / dViewRatio;

                    // Resize and scale the view
                    if (dFactor != 0)
                    {
                        acView.Height = dHeight * dFactor;
                        acView.Width = dWidth * dFactor;
                    }

                    // Set the center of the view
                    acView.CenterPoint = pNewCentPt;

                    // Set the current view
                    acDoc.Editor.SetCurrentView(acView);
                }

                // Commit the changes
                acTrans.Commit();

                acDoc.Editor.UpdateScreen();
            }
        }



        //[CommandMethod("NewUCS")]
        public static void NewUCS()
        {
            // Get the current document and database, and start a transaction
            _AcAp.Document acDoc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            _AcDb.Database acCurDb = acDoc.Database;

            using (_AcDb.Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the UCS table for read
                _AcDb.UcsTable acUCSTbl;
                acUCSTbl = acTrans.GetObject(acCurDb.UcsTableId,
                                             _AcDb.OpenMode.ForRead) as _AcDb.UcsTable;

                _AcDb.UcsTableRecord acUCSTblRec;

                // Check to see if the "New_UCS" UCS table record exists
                if (acUCSTbl.Has("New_UCS") == false)
                {
                    acUCSTblRec = new _AcDb.UcsTableRecord();
                    acUCSTblRec.Name = "New_UCS";

                    // Open the UCSTable for write
                    acUCSTbl.UpgradeOpen();

                    // Add the new UCS table record
                    acUCSTbl.Add(acUCSTblRec);
                    acTrans.AddNewlyCreatedDBObject(acUCSTblRec, true);
                }
                else
                {
                    acUCSTblRec = acTrans.GetObject(acUCSTbl["New_UCS"],
                                                    _AcDb.OpenMode.ForWrite) as _AcDb.UcsTableRecord;
                }

                acUCSTblRec.Origin = new _AcGe.Point3d(4, 5, 3);
                acUCSTblRec.XAxis = new _AcGe.Vector3d(1, 0, 0);
                acUCSTblRec.YAxis = new _AcGe.Vector3d(0, 1, 0);

                // Open the active viewport
                _AcDb.ViewportTableRecord acVportTblRec;
                acVportTblRec = acTrans.GetObject(acDoc.Editor.ActiveViewportId,
                                                  _AcDb.OpenMode.ForWrite) as _AcDb.ViewportTableRecord;

                // Display the UCS Icon at the origin of the current viewport
                acVportTblRec.IconAtOrigin = true;
                acVportTblRec.IconEnabled = true;

                // Set the UCS current
                acVportTblRec.SetUcs(acUCSTblRec.ObjectId);
                acDoc.Editor.UpdateTiledViewportsFromDatabase();

                // Display the name of the current UCS
                _AcDb.UcsTableRecord acUCSTblRecActive;
                acUCSTblRecActive = acTrans.GetObject(acVportTblRec.UcsName,
                                                      _AcDb.OpenMode.ForRead) as _AcDb.UcsTableRecord;

                _AcAp.Application.ShowAlertDialog("The current UCS is: " +
                                            acUCSTblRecActive.Name);

                _AcEd.PromptPointResult pPtRes;
                _AcEd.PromptPointOptions pPtOpts = new _AcEd.PromptPointOptions("");

                // Prompt for a point
                pPtOpts.Message = "\nEnter a point: ";
                pPtRes = acDoc.Editor.GetPoint(pPtOpts);

                _AcGe.Point3d pPt3dWCS;
                _AcGe.Point3d pPt3dUCS;

                // If a point was entered, then translate it to the current UCS
                if (pPtRes.Status == _AcEd.PromptStatus.OK)
                {
                    pPt3dWCS = pPtRes.Value;
                    pPt3dUCS = pPtRes.Value;

                    // Translate the point from the current UCS to the WCS
                    _AcGe.Matrix3d newMatrix = new _AcGe.Matrix3d();
                    newMatrix = _AcGe.Matrix3d.AlignCoordinateSystem(_AcGe.Point3d.Origin,
                                                               _AcGe.Vector3d.XAxis,
                                                               _AcGe.Vector3d.YAxis,
                                                               _AcGe.Vector3d.ZAxis,
                                                               acVportTblRec.Ucs.Origin,
                                                               acVportTblRec.Ucs.Xaxis,
                                                               acVportTblRec.Ucs.Yaxis,
                                                               acVportTblRec.Ucs.Zaxis);

                    pPt3dWCS = pPt3dWCS.TransformBy(newMatrix);

                    _AcAp.Application.ShowAlertDialog("The WCS coordinates are: \n" +
                                                pPt3dWCS.ToString() + "\n" +
                                                "The UCS coordinates are: \n" +
                                                pPt3dUCS.ToString());
                }

                // Save the new objects to the database
                acTrans.Commit();
            }
        }



        internal static void SetWorldUCS()
        {
            _AcInt.AcadApplication app = (_AcInt.AcadApplication)_AcAp.Application.AcadApplication;
            if (app.ActiveDocument.GetVariable("WORLDUCS").ToString() == "0")
            {
                string cmd = "_.UCS _w ";
                _AcAp.Application.DocumentManager.MdiActiveDocument.Editor.Document.SendStringToExecute(cmd, true, false, false);
                //app.ActiveDocument.SendCommand(cmd);
            }
        }


        // dsicht, _plan
        internal static void SetPlanToWCS()
        {
            _AcInt.AcadApplication app = (_AcInt.AcadApplication)_AcAp.Application.AcadApplication;
            string cmd = "_.PLAN _W ";
            _AcAp.Application.DocumentManager.MdiActiveDocument.Editor.Document.SendStringToExecute(cmd, true, true, true);
        }

        internal static void CreateLayer(string layerName, _AcCm.Color color = null)
        {

            _AcDb.Database db = _AcAp.Application.DocumentManager.MdiActiveDocument.Database;
            _AcDb.TransactionManager tm = db.TransactionManager;
            using (var ta = tm.StartTransaction())
            {
                _AcDb.LayerTable lt = (_AcDb.LayerTable)ta.GetObject(db.LayerTableId, _AcDb.OpenMode.ForRead, false);
                if (!lt.Has(layerName))
                {
                    _AcDb.LayerTableRecord ltRec = new _AcDb.LayerTableRecord();
                    ltRec.Name = layerName;
                    if (color != null)
                    {
                        ltRec.Color = color;
                    }
                    lt.UpgradeOpen();
                    lt.Add(ltRec);
                    tm.AddNewlyCreatedDBObject(ltRec, true);
                }
                ta.Commit();
            }
        }

        internal static _AcCm.Color GetLayerColor(string layerName)
        {
            _AcCm.Color color = null;
            _AcDb.Database db = _AcAp.Application.DocumentManager.MdiActiveDocument.Database;
            _AcDb.TransactionManager tm = db.TransactionManager;
            using (var ta = tm.StartTransaction())
            {
                _AcDb.LayerTable lt = (_AcDb.LayerTable)ta.GetObject(db.LayerTableId, _AcDb.OpenMode.ForRead, false);
                if (lt.Has(layerName))
                {
                    var layerId = lt[layerName];
                    var layer = (_AcDb.LayerTableRecord)ta.GetObject(layerId, _AcDb.OpenMode.ForRead);
                    color = layer.Color;
                }
                ta.Commit();
            }
            return color;
        }

        internal static _AcIntCom.AcadEntity ObjectIdToAcadEntity(_AcDb.ObjectId objectId, _AcDb.TransactionManager tm)
        {
            using (_AcDb.Entity ent = (_AcDb.Entity)tm.GetObject(objectId, _AcDb.OpenMode.ForRead, false))
                return ent.AcadObject as _AcIntCom.AcadEntity;
        }

        internal static _AcGe.Point2d PolarPoints(_AcGe.Point2d pPt, double dAng, double dDist)
        {
            return new _AcGe.Point2d(pPt.X + dDist * Math.Cos(dAng),
                               pPt.Y + dDist * Math.Sin(dAng));
        }

        internal static _AcGe.Point3d PolarPoints(_AcGe.Point3d pPt, double dAng, double dDist)
        {
            return new _AcGe.Point3d(pPt.X + dDist * Math.Cos(dAng),
                               pPt.Y + dDist * Math.Sin(dAng),
                               pPt.Z);
        }

        internal static void Stern(_AcGe.Point3d p, double radius, int segments, int color, bool highLighted)
        {
            var ed = _AcAp.Application.DocumentManager.MdiActiveDocument.Editor;
            double inc = (Math.PI * 2.0) / segments;
            double ang = 0.0;
            for (int i = 0; i < segments; i++)
            {
                ed.DrawVector(p, PolarPoints(p, ang, radius), color, highLighted);
                ang += inc;
            }
        }

        internal static string GetBlockname(_AcDb.BlockReference br, _AcDb.Transaction tr)
        {
            try
            {
                if (br == null) return default(string);
                if (br.IsDynamicBlock)
                {
                    _AcDb.BlockTableRecord btr = (_AcDb.BlockTableRecord)tr.GetObject(br.DynamicBlockTableRecord, _AcDb.OpenMode.ForRead);
                    return btr.Name;
                }
                else
                {
                    return br.Name;
                }
            }
            catch (Exception)
            {
                log.WarnFormat("Invalid block with handle {0}!", br.Handle);
            }
            return "$$$InvalidBlock$$$";
        }
    }
}
