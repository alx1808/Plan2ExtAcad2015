//using Autodesk.AutoCAD.ApplicationServices;
//using Autodesk.AutoCAD.DatabaseServices;
//using Autodesk.AutoCAD.EditorInput;
//using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections.Generic;
using System.Globalization;
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
#endif


namespace Plan2Ext.AutoIdVergabe
{
    public class Commands
    {
        #region log4net Initialization
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Convert.ToString((typeof(Commands))));
        static Commands()
        {
            if (log4net.LogManager.GetRepository(System.Reflection.Assembly.GetExecutingAssembly()).Configured == false)
            {
                log4net.Config.XmlConfigurator.ConfigureAndWatch(new System.IO.FileInfo(System.IO.Path.Combine(new System.IO.FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).DirectoryName, "_log4net.config")));
            }

        }
        #endregion

        static AutoIdPalette _AutoIdPalette;

        [_AcTrx.CommandMethod("Plan2AutoIdExcelExport")]
        static public void Plan2AutoIdExcelExport()
        {
            try
            {
#if BRX_APP
                return;
#else

                if (!OpenAutoIdPalette()) return;

                var opts = Globs.TheAutoIdOptions;
                _AcAp.Document doc = _AcAp.Application.DocumentManager.MdiActiveDocument;

                using (_AcAp.DocumentLock m_doclock = doc.LockDocument())
                {

                    Engine engine = new Engine(opts);
                    var ok = engine.ExcelExport();
                    if (!ok)
                    {
                        _AcAp.Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Nicht alle Blöcke haben die gleichen Attribute (siehe %temp%\\plan2.log)"));
                    }
                    else
                    {
                        string msg = string.Format(CultureInfo.CurrentCulture, "Der Excel-Export wurde erfolgreich beendet.");
                        _AcAp.Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage(msg);
                        log.Info(msg);
                    }

                }
#endif

            }
            catch (System.Exception ex)
            {
                log.Error(ex.Message, ex);
                _AcAp.Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2AutoIdExcelExport aufgetreten! {0}", ex.Message));
            }
        }

        [_AcTrx.CommandMethod("Plan2AutoIdExcelImport")]
        static public void Plan2AutoIdExcelImport()
        {
            try
            {
#if BRX_APP
                return;
#else
                if (!OpenAutoIdPalette()) return;

                var opts = Globs.TheAutoIdOptions;
                _AcAp.Document doc = _AcAp.Application.DocumentManager.MdiActiveDocument;

                using (_AcAp.DocumentLock m_doclock = doc.LockDocument())
                {

                    Engine engine = new Engine(opts);
                    var ok = engine.ExcelImport();
                    if (!ok)
                    {
                        _AcAp.Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Es sind Fehler aufgetreten. (siehe %temp%\\plan2.log)"));
                    }
                    else
                    {
                        string msg = string.Format(CultureInfo.CurrentCulture, "Der Excel-Import wurde erfolgreich beendet.");
                        _AcAp.Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage(msg);
                        log.Info(msg);
                    }

                }
#endif
            }
            catch (System.Exception ex)
            {
                log.Error(ex.Message, ex);
                _AcAp.Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2AutoIdExcelImport aufgetreten! {0}", ex.Message));
            }
        }

        [_AcTrx.CommandMethod("Plan2AutoIdEindeutigkeit")]
        static public void Plan2AutoIdEindeutigkeit()
        {
            try
            {
                if (!OpenAutoIdPalette()) return;

                var opts = Globs.TheAutoIdOptions;
                _AcAp.Document doc = _AcAp.Application.DocumentManager.MdiActiveDocument;

                using (_AcAp.DocumentLock m_doclock = doc.LockDocument())
                {

                    Engine engine = new Engine(opts);
                    engine.CheckUniqueness();
                }

            }
            catch (System.Exception ex)
            {
                log.Error(ex.Message, ex);
                _AcAp.Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2AutoIdEindeutigkeit aufgetreten! {0}", ex.Message));
            }
        }

        [_AcTrx.CommandMethod("Plan2AutoIdVergabe")]
        static public void Plan2AutoIdVergabe()
        {
            try
            {
                if (!OpenAutoIdPalette()) return;

                var opts = Globs.TheAutoIdOptions;
                _AcAp.Document doc = _AcAp.Application.DocumentManager.MdiActiveDocument;

                using (_AcAp.DocumentLock m_doclock = doc.LockDocument())
                {

                    Engine engine = new Engine(opts);
                    engine.AssignIds();
                }

            }
            catch (System.Exception ex)
            {
                _AcAp.Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2AutoIdVergabe aufgetreten! {0}", ex.Message));
            }
        }

        [_AcTrx.CommandMethod("Plan2AutoIdSelBlockAndAtt")]
        static public void Plan2AutoIdSelBlockAndAtt()
        {
            try
            {
                if (!OpenAutoIdPalette()) return;

                var opts = Globs.TheAutoIdOptions;
                _AcAp.Document doc = _AcAp.Application.DocumentManager.MdiActiveDocument;

                using (_AcAp.DocumentLock m_doclock = doc.LockDocument())
                {
                    _AcAp.DocumentCollection dm = _AcAp.Application.DocumentManager;
                    if (doc == null) return;
                    _AcEd.Editor ed = doc.Editor;
#if NEWSETFOCUS
                    doc.Window.Focus();
#else
                Autodesk.AutoCAD.Internal.Utils.SetFocusToDwgView(); // previous 2014 AutoCAD - Versions
#endif

                    _AcEd.PromptNestedEntityResult per = ed.GetNestedEntity("\nRaumblock wählen: ");

                    if (per.Status == _AcEd.PromptStatus.OK)
                    {

                        using (var tr = doc.TransactionManager.StartTransaction())
                        {
                            _AcDb.DBObject obj = tr.GetObject(per.ObjectId, _AcDb.OpenMode.ForRead);
                            _AcDb.BlockReference br = obj as _AcDb.BlockReference;
                            if (br == null)
                            {
                                br = Plan2Ext.Globs.GetBlockFromItsSubentity(tr, per);
                                if (br == null) return;
                            }

                            string blockName = Plan2Ext.Globs.GetBlockname(br, tr);
                            opts.SetBlockname(Engine.RemoveXRefPart(blockName));

                            tr.Commit();
                        }

                        per = ed.GetNestedEntity("\nTürschild-Attribut wählen: ");
                        if (per.Status != _AcEd.PromptStatus.OK) return;
                        using (var tr = doc.TransactionManager.StartTransaction())
                        {
                            _AcDb.DBObject obj = tr.GetObject(per.ObjectId, _AcDb.OpenMode.ForRead);
                            _AcDb.AttributeReference ar = obj as _AcDb.AttributeReference;
                            if (ar == null) return;

                            opts.SetTuerschildAtt(ar.Tag);

                            tr.Commit();
                        }

                        per = ed.GetNestedEntity("ID-Attribut wählen: ");
                        if (per.Status != _AcEd.PromptStatus.OK) return;
                        using (var tr = doc.TransactionManager.StartTransaction())
                        {
                            _AcDb.DBObject obj = tr.GetObject(per.ObjectId, _AcDb.OpenMode.ForRead);
                            _AcDb.AttributeReference ar = obj as _AcDb.AttributeReference;
                            if (ar == null) return;

                            opts.SetIdAtt(ar.Tag);

                            tr.Commit();
                        }

                    }
                }

            }
            catch (System.Exception ex)
            {
                _AcAp.Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2AutoIdSelBlockAndAtt aufgetreten! {0}", ex.Message));
            }
        }

        [_AcTrx.CommandMethod("Plan2AutoIdSelPolygonLayer")]
        static public void Plan2AutoIdSelPolygonLayer()
        {
            try
            {
                if (!OpenAutoIdPalette()) return;

                var opts = Globs.TheAutoIdOptions;
                _AcAp.Document doc = _AcAp.Application.DocumentManager.MdiActiveDocument;

                using (_AcAp.DocumentLock m_doclock = doc.LockDocument())
                {
                    _AcAp.DocumentCollection dm = _AcAp.Application.DocumentManager;
                    if (doc == null) return;
                    _AcEd.Editor ed = doc.Editor;
#if NEWSETFOCUS
                    doc.Window.Focus();
#else
                Autodesk.AutoCAD.Internal.Utils.SetFocusToDwgView(); // previous 2014 AutoCAD - Versions
#endif

                    _AcEd.PromptNestedEntityOptions peo = new _AcEd.PromptNestedEntityOptions("\nPolylinie wählen: ");
                    //peo.SetRejectMessage("\nDas gewählte Element ist keine Polylinie.");
                    //peo.AddAllowedClass(typeof(Polyline), true);
                    //peo.AddAllowedClass(typeof(Polyline2d), true);
                    //peo.AddAllowedClass(typeof(Polyline3d), true);

                    _AcEd.PromptEntityResult per = ed.GetNestedEntity(peo);

                    if (per.Status == _AcEd.PromptStatus.OK)
                    {

                        using (var tr = doc.TransactionManager.StartTransaction())
                        {
                            _AcDb.DBObject obj = tr.GetObject(per.ObjectId, _AcDb.OpenMode.ForRead);
                            _AcDb.Entity ent = obj as _AcDb.Entity;
                            if (ent != null)
                            {
                                if (ent is _AcDb.Polyline || ent is _AcDb.Polyline2d || ent is _AcDb.Polyline3d)
                                {
                                    opts.SetPolygonLayer(Engine.RemoveXRefPart(ent.Layer));
                                }
                                else
                                {
                                    ed.WriteMessage("\nDas gewählte Element ist keine Polylinie.");
                                }
                            }

                            tr.Commit();
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                _AcAp.Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2AutoIdSelPolygonLayer aufgetreten! {0}", ex.Message));
            }
        }

        [_AcTrx.CommandMethod("Plan2AutoIdAssignments")]
        static public void Plan2AutoIdAssignments()
        {
            try
            {
                if (!OpenAutoIdPalette()) return;

                var opts = Globs.TheAutoIdOptions;
                _AcAp.Document doc = _AcAp.Application.DocumentManager.MdiActiveDocument;

                using (_AcAp.DocumentLock m_doclock = doc.LockDocument())
                {
                    _AcAp.DocumentCollection dm = _AcAp.Application.DocumentManager;
                    if (doc == null) return;
                    _AcEd.Editor ed = doc.Editor;
#if NEWSETFOCUS
                    doc.Window.Focus();
#else
                Autodesk.AutoCAD.Internal.Utils.SetFocusToDwgView(); // previous 2014 AutoCAD - Versions
#endif

                    SetAssignmentsDict();
                    _AutoIdPalette.SetLvZuweisungen();

                    //bool attSelected = false;
                    //while (!attSelected)
                    //{
                    //    PromptNestedEntityOptions peo = new PromptNestedEntityOptions("\nZu-Raum-ID-Attribut wählen: ");
                    //    PromptNestedEntityResult per = ed.GetNestedEntity(peo);

                    //    if (per.Status == _AcEd.PromptStatus.OK)
                    //    {

                    //        using (var tr = doc.TransactionManager.StartTransaction())
                    //        {
                    //            DBObject obj = tr.GetObject(per.ObjectId, _AcDb.OpenMode.ForRead);
                    //            _AcDb.AttributeReference attRef = obj as _AcDb.AttributeReference;
                    //            if (attRef != null)
                    //            {
                    //                attSelected = true;
                    //                opts.SetZuRaumIdAtt(attRef.Tag);
                    //            }
                    //            else
                    //            {
                    //                ed.WriteMessage("\nDas gewählte Element ist kein Attribut.");
                    //            }

                    //            tr.Commit();
                    //        }
                    //    }
                    //    else
                    //    {
                    //        break;
                    //    }
                    //}
                }
            }
            catch (System.Exception ex)
            {
                _AcAp.Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2AutoIdAssignments aufgetreten! {0}", ex.Message));
            }
        }

        private const int RTNORM = 5100;
        private static void SetAssignmentsDict()
        {
            using (var rb = new _AcDb.ResultBuffer(new _AcDb.TypedValue((int)_AcBrx.LispDataType.Text, "c:Plan2AutoIdVergabeSetAssignments")))
            {
                int stat = 0;
                _AcDb.ResultBuffer res = CADDZone.AutoCAD.Samples.AcedInvokeSample.InvokeLisp(rb, ref stat);
                if (stat == RTNORM && res != null)
                {
                    res.Dispose();
                }
            }
        }

        [_AcTrx.CommandMethod("Plan2AutoIdZuRaumIdVergabe")]
        static public void Plan2AutoIdZuRaumIdVergabe()
        {
            try
            {
                if (!OpenAutoIdPalette()) return;

                var opts = Globs.TheAutoIdOptions;
                _AcAp.Document doc = _AcAp.Application.DocumentManager.MdiActiveDocument;

                using (_AcAp.DocumentLock m_doclock = doc.LockDocument())
                {

                    Engine engine = new Engine(opts);
                    engine.ZuRaumIdVergabe();
                }

            }
            catch (System.Exception ex)
            {
                log.Error(ex.Message, ex);
                _AcAp.Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2AutoIdZuRaumIdVergabe aufgetreten! {0}", ex.Message));
            }
        }

        [_AcTrx.CommandMethod("Plan2AutoIdZuRaumIdVergabeAttribut")]
        static public void Plan2AutoIdZuRaumIdVergabeAttribut()
        {
            try
            {
                if (!OpenAutoIdPalette()) return;

                var opts = Globs.TheAutoIdOptions;
                _AcAp.Document doc = _AcAp.Application.DocumentManager.MdiActiveDocument;

                using (_AcAp.DocumentLock m_doclock = doc.LockDocument())
                {

                    Engine engine = new Engine(opts);
                    engine.ZuRaumIdVergabeAttribut();
                }
            }
            catch (System.Exception ex)
            {
                log.Error(ex.Message, ex);
                _AcAp.Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2AutoIdZuRaumIdVergabeAttribut aufgetreten! {0}", ex.Message));
            }
        }

        // Just for Testpurpose
        private static void GetAssignmentsDict()
        {
            var doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            try
            {
                using (_AcDb.Transaction trans =

                                  db.TransactionManager.StartTransaction())
                {

                    // Find the NOD in the database

                    _AcDb.DBDictionary nod = (_AcDb.DBDictionary)trans.GetObject(

                                db.NamedObjectsDictionaryId, _AcDb.OpenMode.ForWrite);



                    //// We use Xrecord class to store data in Dictionaries

                    //Xrecord myXrecord = new Xrecord();

                    //myXrecord.Data = new _AcDb.ResultBuffer(

                    //        new _AcDb.TypedValue((int)DxfCode.Int16, 1234),

                    //        new _AcDb.TypedValue((int)DxfCode.Text,

                    //                        "This drawing has been processed"));



                    //// Create the entry in the Named Object Dictionary

                    //nod.SetAt("MyData", myXrecord);

                    //trans.AddNewlyCreatedDBObject(myXrecord, true);



                    // Now let's read the data back and print them out

                    //  to the Visual Studio's Output window

                    _AcDb.ObjectId myDataId = nod.GetAt("XRECLIST");

                    var readBack = trans.GetObject(

                                                  myDataId, _AcDb.OpenMode.ForRead);

                    var readBack2 = readBack as _AcDb.Xrecord;
                    if (readBack2 != null)
                    {

                        foreach (_AcDb.TypedValue value in readBack2.Data)

                            System.Diagnostics.Debug.Print(

                                      "===== OUR DATA: " + value.TypeCode.ToString()

                                      + ". " + value.Value.ToString());



                        trans.Commit();
                    }


                } // using



                db.SaveAs(@"C:\Temp\Test.dwg", _AcDb.DwgVersion.Current);



            }

            catch (System.Exception e)
            {

                System.Diagnostics.Debug.Print(e.ToString());

            }

        }



        private static bool OpenAutoIdPalette()
        {
            if (_AutoIdPalette == null)
            {
                _AutoIdPalette = new AutoIdPalette();
            }

            bool wasOpen = _AutoIdPalette.Show();
            if (!wasOpen) return false;

            _AcAp.Document doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
            if (doc == null) return false;
            log.DebugFormat("Dokumentname: {0}.", doc.Name);

            if (Globs.TheAutoIdOptions == null) return false;
            else return true;
        }

    }
}
