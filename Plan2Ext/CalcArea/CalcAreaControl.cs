using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Globalization;

//using Autodesk.AutoCAD.DatabaseServices;
//using Autodesk.AutoCAD.ApplicationServices;
//using Autodesk.AutoCAD.EditorInput;

#if BRX_APP
using _AcAp = Bricscad.ApplicationServices;
using Teigha.DatabaseServices;
using Bricscad.EditorInput;
using _AcBrx = Bricscad.Runtime;
#elif ARX_APP
  using _AcAp = Autodesk.AutoCAD.ApplicationServices;
  using Autodesk.AutoCAD.DatabaseServices;
  using Autodesk.AutoCAD.EditorInput;
  using _AcBrx = Autodesk.AutoCAD.Runtime;
#endif


namespace Plan2Ext.CalcArea
{
    public partial class CalcAreaControl : UserControl
    {
        #region log4net Initialization
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Convert.ToString((typeof(CalcAreaControl))));
        #endregion

        #region Lifecycle
        public CalcAreaControl()
        {
            InitializeComponent();
        }
        #endregion

        #region Control Events

        private bool _CalcAreaShield = false;
        private void btnCalcArea_Click(object sender, EventArgs e)
        {
            if (_CalcAreaShield) return;
            try
            {
                _CalcAreaShield = true;

                //Globs.CancelCommand();

                _AcAp.Application.DocumentManager.MdiActiveDocument.SendStringToExecute("Plan2CalcArea ", true, false, false);
            }
            catch (Exception ex)
            {
                _AcAp.Application.ShowAlertDialog(ex.Message);
            }
            finally
            {
                _CalcAreaShield = false;
            }
        }

        private bool _FlaBereinigShield = false;
        private void btnFlaBereinig_Click(object sender, EventArgs e)
        {
            if (_FlaBereinigShield) return;
            try
            {
                _FlaBereinigShield = true;

                Globs.CancelCommand();

                _AcAp.Application.DocumentManager.MdiActiveDocument.SendStringToExecute("Plan2FlaBereinig ", true, false, false);
            }
            catch (Exception ex)
            {
                _AcAp.Application.ShowAlertDialog(ex.Message);
            }
            finally
            {
                _FlaBereinigShield = false;
            }
        }

        private void btnCalcArea_Click2(object sender, EventArgs e)
        {
            try
            {
#if NEWSETFOCUS
                _AcAp.Application.DocumentManager.MdiActiveDocument.Window.Focus();
#else
                Autodesk.AutoCAD.Internal.Utils.SetFocusToDwgView(); // previous 2014 AutoCAD - Versions
#endif

                Plan2Ext.Flaeche.AktFlaeche(_AcAp.Application.DocumentManager.MdiActiveDocument,
                    txtBlockname.Text,
                    txtAttribute.Text,
                    txtPeriAtt.Text,
                    txtFG.Text,
                    txtAG.Text
                  );
            }
            catch (Exception ex)
            {
                _AcAp.Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler aufgetreten: '{0}'", ex.Message));
            }

        }

        private void btnSelectBlock_Click(object sender, EventArgs e)
        {
            try
            {
                _AcAp.DocumentCollection dm = _AcAp.Application.DocumentManager;
                _AcAp.Document doc = dm.MdiActiveDocument;
                Editor ed = doc.Editor;

#if NEWSETFOCUS
                _AcAp.Application.DocumentManager.MdiActiveDocument.Window.Focus();
#else
                Autodesk.AutoCAD.Internal.Utils.SetFocusToDwgView(); // previous 2014 AutoCAD - Versions
#endif

                PromptEntityResult per = ed.GetEntity("\nRaumblock wählen: ");

                //DocumentLock loc = dm.MdiActiveDocument.LockDocument();
                //using (loc)
                //{
                //}

                if (per.Status == PromptStatus.OK)
                {

                    Transaction tr = doc.TransactionManager.StartTransaction();
                    using (tr)
                    {
                        DBObject obj = tr.GetObject(per.ObjectId, OpenMode.ForRead);
                        BlockReference br = obj as BlockReference;
                        if (br == null) return;

                        txtBlockname.Text = Plan2Ext.Globs.GetBlockname(br, tr);
                        tr.Commit();
                    }

                    per = ed.GetNestedEntity("\nFlächen-Attribut wählen: ");
                    if (per.Status == PromptStatus.OK)
                    {

                        tr = doc.TransactionManager.StartTransaction();
                        using (tr)
                        {
                            DBObject obj = tr.GetObject(per.ObjectId, OpenMode.ForRead);
                            AttributeReference ar = obj as AttributeReference;
                            if (ar == null) return;
                            txtAttribute.Text = ar.Tag;
                            tr.Commit();
                        }
                    }

                    txtPeriAtt.Text = "";
                    per = ed.GetNestedEntity("\nUmfang-Attribut wählen: ");
                    if (per.Status == PromptStatus.OK)
                    {
                        tr = doc.TransactionManager.StartTransaction();
                        using (tr)
                        {
                            DBObject obj = tr.GetObject(per.ObjectId, OpenMode.ForRead);
                            AttributeReference ar = obj as AttributeReference;
                            if (ar == null) return;
                            txtPeriAtt.Text = ar.Tag;
                            tr.Commit();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _AcAp.Application.ShowAlertDialog(ex.Message);
            }
        }

        private void btnFG_Click(object sender, EventArgs e)
        {
            try
            {
                _AcAp.DocumentCollection dm = _AcAp.Application.DocumentManager;
                _AcAp.Document doc = dm.MdiActiveDocument;
                Editor ed = doc.Editor;

#if NEWSETFOCUS
                _AcAp.Application.DocumentManager.MdiActiveDocument.Window.Focus();
#else
                Autodesk.AutoCAD.Internal.Utils.SetFocusToDwgView(); // previous 2014 AutoCAD - Versions
#endif

                PromptEntityResult per = ed.GetEntity("\nFlächengrenze wählen: ");

                if (per.Status == PromptStatus.OK)
                {

                    Transaction tr = doc.TransactionManager.StartTransaction();
                    using (tr)
                    {
                        string layer = string.Empty;
                        DBObject obj = tr.GetObject(per.ObjectId, OpenMode.ForRead);
                        layer = GetPolylineLayer(obj);
                        if (string.IsNullOrEmpty(layer)) return;

                        if (string.Compare(txtAG.Text, layer, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            _AcAp.Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Die Layer für Flächengrenze und Abzugsfläche müssen unterschiedlich sein."));
                            return;
                        }

                        txtFG.Text = layer;
                        tr.Commit();
                    }
                }
            }
            catch (Exception ex)
            {
                _AcAp.Application.ShowAlertDialog(ex.Message);
            }
        }

        private void btnAG_Click(object sender, EventArgs e)
        {
            try
            {
                _AcAp.DocumentCollection dm = _AcAp.Application.DocumentManager;
                _AcAp.Document doc = dm.MdiActiveDocument;
                Editor ed = doc.Editor;

#if NEWSETFOCUS
                _AcAp.Application.DocumentManager.MdiActiveDocument.Window.Focus();
#else
                Autodesk.AutoCAD.Internal.Utils.SetFocusToDwgView(); // previous 2014 AutoCAD - Versions
#endif

                PromptEntityResult per = ed.GetEntity("\nAbzugsfläche wählen: ");

                if (per.Status == PromptStatus.OK)
                {

                    Transaction tr = doc.TransactionManager.StartTransaction();
                    using (tr)
                    {
                        string layer = string.Empty;
                        DBObject obj = tr.GetObject(per.ObjectId, OpenMode.ForRead);
                        layer = GetPolylineLayer(obj);
                        if (string.IsNullOrEmpty(layer)) return;

                        if (string.Compare(txtFG.Text, layer, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            _AcAp.Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Die Layer für Flächengrenze und Abzugsfläche müssen unterschiedlich sein."));
                            return;
                        }

                        txtAG.Text = layer;
                        tr.Commit();
                    }

                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void txtBlockname_TextChanged(object sender, EventArgs e)
        {
            HandleControls();
        }

        private void txtAttribute_TextChanged(object sender, EventArgs e)
        {
            HandleControls();
        }

        private void txtFG_TextChanged(object sender, EventArgs e)
        {
            HandleControls();
        }

        private void txtAG_TextChanged(object sender, EventArgs e)
        {
            HandleControls();
        }

        #endregion

        #region Private
        private static string GetPolylineLayer(DBObject obj)
        {
            Polyline pline = obj as Polyline;
            if (pline != null)
            {
                return pline.Layer;
            }
            else
            {
                Polyline2d pl = obj as Polyline2d;
                if (pl != null) return pl.Layer;

            }
            return string.Empty;
        }

        private void HandleControls()
        {
            if (string.IsNullOrEmpty(txtBlockname.Text) || string.IsNullOrEmpty(txtAttribute.Text) || string.IsNullOrEmpty(txtFG.Text) || string.IsNullOrEmpty(txtAG.Text))
                btnCalcArea.Enabled = false;
            else btnCalcArea.Enabled = true;
        }

        #endregion

        #region Internal
        Flaeche.AktFlaecheDelegate _AktFlaecheDelegate;
        internal void SetAktFlaecheDelegate(Flaeche.AktFlaecheDelegate aktFlaecheDelegate)
        {

            _AktFlaecheDelegate = aktFlaecheDelegate;
        }
        #endregion

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {

                Test();
                return;

#if ARX_APP


                ResultBuffer rbInvoke = new ResultBuffer();
                rbInvoke.Add(new TypedValue((int)(_AcBrx.LispDataType.Text), "alx_F:ino_plan2_GetConfiguration"));
                ResultBuffer resInvoke = _AcAp.Application.Invoke(rbInvoke);
                rbInvoke.Dispose();

                object o1 = Globs.GetLispVariable("alx_V:ino_flattrib");
                object o2 = Globs.GetLispVariable("alx_V:ino_flattribdfa");

                object o3 = Globs.LispFindFile("plan2.cfg");
                if (o3 != null)
                {
                    object o4 = Globs.LispTryLoadGlobs(o3.ToString());
                }
#endif

                //rbInvoke = new ResultBuffer();
                //rbInvoke.Add(new TypedValue((int)(Autodesk.AutoCAD.Runtime.LispDataType.Text), "alx_F:ino_EvalLispVariable"));
                //rbInvoke.Add(new TypedValue((int)(Autodesk.AutoCAD.Runtime.LispDataType.Text), "alx_V:ino_flattrib"));
                //resInvoke = _AcAp.Application.Invoke(rbInvoke);
                //rbInvoke.Dispose();

                //rbInvoke = new ResultBuffer();
                //rbInvoke.Add(new TypedValue((int)(Autodesk.AutoCAD.Runtime.LispDataType.Text), "alx_F:ino_EvalLispVariable"));
                //rbInvoke.Add(new TypedValue((int)(Autodesk.AutoCAD.Runtime.LispDataType.Text), "alx_V:ino_flattribxdf"));
                //resInvoke = _AcAp.Application.Invoke(rbInvoke);
                //rbInvoke.Dispose();



            }
            catch (Exception ex)
            {
                string x = ex.Message;
                ;
            }
        }


        private void Test()
        {
            Database db = _AcAp.Application.DocumentManager.MdiActiveDocument.Database;

            try
            {

                // We will write to C:\Temp\Test.dwg. Make sure it exists!

                // Load it into AutoCAD

                //db.ReadDwgFile(@"C:\Temp\Test.dwg",

                //                System.IO.FileShare.ReadWrite, false, null);



                using (Transaction trans =

                                  db.TransactionManager.StartTransaction())
                {

                    // Find the NOD in the database

                    DBDictionary nod = (DBDictionary)trans.GetObject(

                                db.NamedObjectsDictionaryId, OpenMode.ForRead);



                    //// We use Xrecord class to store data in Dictionaries

                    //Xrecord myXrecord = new Xrecord();

                    //myXrecord.Data = new ResultBuffer(

                    //        new TypedValue((int)DxfCode.Int16, 1234),

                    //        new TypedValue((int)DxfCode.Text,

                    //                        "This drawing has been processed"));



                    //// Create the entry in the Named Object Dictionary

                    //nod.SetAt("MyData", myXrecord);

                    //trans.AddNewlyCreatedDBObject(myXrecord, true);



                    // Now let's read the data back and print them out

                    //  to the Visual Studio's Output window

                    ObjectId myDataId = nod.GetAt("AA_PLAN2");
                    DBDictionary  plan2Dict = trans.GetObject(myDataId, OpenMode.ForRead) as DBDictionary ;
                    if (plan2Dict != null)
                    {
                        ObjectId cfId = plan2Dict.GetAt("ConfigFile");
                        object cf = trans.GetObject(cfId, OpenMode.ForRead);


                    }

                    //                              myDataId, OpenMode.ForRead);


                    //Xrecord readBack = (Xrecord)trans.GetObject(

                    //                              myDataId, OpenMode.ForRead);

                    //foreach (TypedValue value in readBack.Data)

                    //    System.Diagnostics.Debug.Print(

                    //              "===== OUR DATA: " + value.TypeCode.ToString()

                    //              + ". " + value.Value.ToString());



                    trans.Commit();



                } // using



                //db.SaveAs(@"C:\Temp\Test.dwg", DwgVersion.Current);



            }

            catch (Exception e)
            {

                System.Diagnostics.Debug.Print(e.ToString());

            }

            finally
            {

                db.Dispose();

            }
        }

        private bool _CalcvolShield = false;
        private void btnCalcVol_Click(object sender, EventArgs e)
        {
            if (_CalcvolShield) return;
            try
            {
                _CalcvolShield = true;
                _AcAp.Application.DocumentManager.MdiActiveDocument.SendStringToExecute("Plan2CalcFlaCalVolumes ", true, false, false);
            }
            catch (Exception ex)
            {
                _AcAp.Application.ShowAlertDialog(ex.Message);
            }
            finally
            {
                _CalcvolShield = false;
            }


        }

        private bool _SelVolAttribsShield = false;
        private void btnSelVolAttribs_Click(object sender, EventArgs e)
        {
            if (_SelVolAttribsShield) return;
            try
            {
                _SelVolAttribsShield = true;
                _AcAp.Application.DocumentManager.MdiActiveDocument.SendStringToExecute("Plan2CalcFlaSelVolAtts ", true, false, false);
            }
            catch (Exception ex)
            {
                _AcAp.Application.ShowAlertDialog(ex.Message);
            }
            finally
            {
                _SelVolAttribsShield = false;
            }


        }
    }
}
