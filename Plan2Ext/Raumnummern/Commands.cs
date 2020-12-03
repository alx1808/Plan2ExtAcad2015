﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plan2Ext.Raumnummern.ExcelExport;

// ReSharper disable IdentifierTypo

#if BRX_APP
using Bricscad.ApplicationServices;
using Teigha.DatabaseServices;
using Bricscad.EditorInput;
using Bricscad.Runtime;
using Teigha.Runtime;
using Bricscad.Internal;

#elif ARX_APP
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Internal;
#endif

namespace Plan2Ext.Raumnummern
{
	public class Commands
	{
		#region log4net Initialization
		private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Convert.ToString((typeof(Commands))));
		static Commands()
		{
			if (log4net.LogManager.GetRepository(System.Reflection.Assembly.GetExecutingAssembly()).Configured == false)
			{
				log4net.Config.XmlConfigurator.ConfigureAndWatch(new System.IO.FileInfo(System.IO.Path.Combine(new System.IO.FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).DirectoryName, "_log4net.config")));
			}

		}
		#endregion

		internal const string PFEILBLOCKNAME = "AL_TOP";
		internal const string TOPBLOCKNAME = "AL_TOP_NFL";
		internal const string PROPOTYP_DWG_NAME = "PROTO_50.dwg";
		internal const string TOPBLOCK_TOPNR_ATTNAME = "TOP";
		internal const string PFEILBLOCK_TOPNR_ATTNAME = "TOP";
		internal const string TOPBLOCK_M2_ATTNAME = "M2";

		private Dictionary<ObjectId, AreaEngine.FgRbStructure> _fgRbStructs = new Dictionary<ObjectId, AreaEngine.FgRbStructure>();
		private List<ObjectId> _allRaumBlocks = new List<ObjectId>();

		[CommandMethod("Plan2Raumnummern")]
		public void Plan2Raumnummern()
		{
			try
			{
				if (!OpenRnPalette()) return;

				var opts = Globs.TheRnOptions;
				Document doc = Application.DocumentManager.MdiActiveDocument;

				using (DocumentLock m_doclock = doc.LockDocument())
                {
                    AddNumber(opts);
                }
			}
			catch (System.Exception ex)
			{
				Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2Raumnummern aufgetreten! {0}", ex.Message));
			}
		}

        private void AddNumber(RnOptions opts)
        {
            var blockOids = new List<ObjectId>();
            var engine = GetEngine();

            Plan2Ext.Globs.LayerOffRegex(new List<string> {"^X", "^A_BM_", "^A_BE_TXT", "^A_BE_HÖHE$", "^A_BE_HK"});
            Plan2Ext.Globs.LayerOnAndThawRegex(new List<string>
                {"^" + opts.FlaechenGrenzeLayerName + "$", "^" + opts.AbzFlaechenGrenzeLayerName + "$"});

            while (engine.AddNumber(blockOids))
            {
            }

            ;
        }

		[LispFunction("CalcAreaNet")]
		public double CalcAreaNet(ResultBuffer rb)
		{
			double m2 = -1.0;
			try
			{
				Plan2Ext.Kleinbefehle.Layers.Plan2SaveLayerStatus();
				OpenRnPalette();
				var opts = Globs.TheRnOptions;
				//var opts = new RnOptions();

				Document doc = Application.DocumentManager.MdiActiveDocument;

				using (DocumentLock m_doclock = doc.LockDocument())
				{
					// zuerst fläche rechnen
					Plan2Ext.Flaeche.Modify = true;
					Plan2Ext.Flaeche.AktFlaeche(
						Application.DocumentManager.MdiActiveDocument,
						opts.Blockname, opts.FlaechenAttributName, opts.UmfangAttributName, opts.FlaechenGrenzeLayerName, opts.AbzFlaechenGrenzeLayerName, selectAll: true, layerSchalt: true);

					Plan2Ext.Globs.LayerOnAndThawRegex(new List<string>() { "^" + Engine.TOP_LAYER_PREFIX });
					var engine = GetEngine();

					// danach regions etc. bereinig
					Plan2Ext.Flaeche.BereinigRegions(automated: false);

					if (!engine.SumFgs(ref m2))
					{
						m2 = -1.0;
					}
				}
			}
			catch (System.Exception ex)
			{
				Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in CalcAreaNet aufgetreten! {0}", ex.Message));
				return -1.0;
			}

			return m2;
		}


		[CommandMethod("Plan2RaumnummernReinit")]
		public void Plan2RaumnummernReinit()
		{
			try
			{
				_fgRbStructs = new Dictionary<ObjectId, AreaEngine.FgRbStructure>();
				_allRaumBlocks = new List<ObjectId>();
			}
			catch (System.Exception ex)
			{
				Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2RaumnummernReinit aufgetreten! {0}", ex.Message));
			}
		}

		[CommandMethod("Plan2RaumnummernDeleteFehlerlines")]
		public void Plan2RaumnummernDeleteFehlerlines()
		{
			try
			{
				if (!OpenRnPalette()) return;

				var opts = Globs.TheRnOptions;
				Document doc = Application.DocumentManager.MdiActiveDocument;

				using (DocumentLock m_doclock = doc.LockDocument())
				{
					var engine = GetEngine();
					engine.DeleteAllFehlerLines();

				}
			}
			catch (System.Exception ex)
			{
				Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2RaumnummernDeleteFehlerlines aufgetreten! {0}", ex.Message));
			}
		}


		[CommandMethod("Plan2RaumnummernSum")]
		public void Plan2RaumnummernSum()
		{
			try
			{
				UndoHandler.StartUndoMark();
				if (!OpenRnPalette()) return;

				Plan2Ext.Kleinbefehle.Layers.Plan2SaveLayerStatus();

				var opts = Globs.TheRnOptions;
				Document doc = Application.DocumentManager.MdiActiveDocument;

				using (DocumentLock m_doclock = doc.LockDocument())
				{
					Plan2Ext.Globs.LayerOnAndThawRegex(new List<string>
					{
						"^" + opts.FlaechenGrenzeLayerName + "$",
						"^" + opts.AbzFlaechenGrenzeLayerName + "$",
					});

					var engine = GetEngine();

					engine.SumTops();
				}
			}
			catch (System.Exception ex)
			{
				Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture,
					"Fehler in Plan2RaumnummernSum aufgetreten! {0}", ex.Message));
			}
			finally
			{
				UndoHandler.EndUndoMark();
			}
		}


#if !OLDER_THAN_2015
		[CommandMethod("Plan2RaumnummernInsertTop")]
#if BRX_APP
		public void Plan2RaumnummernInsertTop()

#else
        async public void Plan2RaumnummernInsertTop()

#endif
		{
			var oldAttReq = Application.GetSystemVariable("ATTREQ");
			var curLayer = Application.GetSystemVariable("CLAYER").ToString();
			try
			{
				if (!OpenRnPalette()) return;
				var opts = Globs.TheRnOptions;

				var doc = Application.DocumentManager.MdiActiveDocument;
				var ed = doc.Editor;

				var blockLayer = "A_RA_NUMMER";

				LayerManager.VerifyLayerExists(blockLayer, null);
				Plan2Ext.Globs.SetLayerCurrent(blockLayer);

				if (!BlockManager.BlockExists(PFEILBLOCKNAME))
				{
					if (!BlockManager.InsertFromPrototype(PFEILBLOCKNAME, PROPOTYP_DWG_NAME))
					{
						ed.WriteMessage(string.Format(CultureInfo.CurrentCulture, "\nBlock '{0}' existiert nicht!", PFEILBLOCKNAME));
						return;
					}
				}
				if (!BlockManager.BlockExists(TOPBLOCKNAME))
				{
					if (!BlockManager.InsertFromPrototype(TOPBLOCKNAME, PROPOTYP_DWG_NAME))
					{
						ed.WriteMessage(string.Format(CultureInfo.CurrentCulture, "\nBlock '{0}' existiert nicht!", TOPBLOCKNAME));
						return;
					}
				}

				//var oidL = Utils.EntLast();
				Application.SetSystemVariable("ATTREQ", 0);
#if BRX_APP
				bool userBreak = Plan2Ext.Globs.CallCommand("_.INSERT", PFEILBLOCKNAME, "\\", 1, 1, "\\");
#else
                bool userBreak = await Plan2Ext.Globs.CallCommandAsync("_.INSERT", PFEILBLOCKNAME, Editor.PauseToken, 1, 1, Editor.PauseToken);
#endif
				if (userBreak) return;
				//var oid = Utils.EntLast();
				//var topNr = opts.TopNr;
				//SetTopNr(doc.Database, oid, topNr, "TOP");

				var vctrU = Plan2Ext.Globs.GetViewCtrW();
#if BRX_APP
				Plan2Ext.Globs.CallCommand("_.INSERT", TOPBLOCKNAME, vctrU, 1, 1, 0.0);
#else
                ed.Command("_.INSERT", TOPBLOCKNAME, vctrU, 1, 1, 0.0);
#endif
				if (userBreak) return;
				//oid = Utils.EntLast();
				//SetTopBlockNr(doc.Database, oid, topNr, TOPBLOCK_TOPNR_ATTNAME);
#if BRX_APP
				userBreak = Plan2Ext.Globs.CallCommand("_.MOVE", "_L", "", vctrU, "\\");
#else
                userBreak = await Plan2Ext.Globs.CallCommandAsync("_.MOVE", "_L", "", vctrU, Editor.PauseToken);
#endif

				// update in properties for add room
				//opts.SetTop(topNr);

				//IncrementTopNr();

			}
			catch (System.Exception ex)
			{
				Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2RaumnummernInsertTop aufgetreten! {0}", ex.Message));
			}
			finally
			{
				Application.SetSystemVariable("ATTREQ", oldAttReq);
				Plan2Ext.Globs.SetLayerCurrent(curLayer);
			}
		}

		private static bool SetTopBlockNr(Database db, ObjectId blockOid, string topNr, string attName)
		{
			bool ok = true;
			try
			{
				using (Transaction tr = db.TransactionManager.StartTransaction())
				{
					var blockRef = (BlockReference)tr.GetObject(blockOid, OpenMode.ForRead);

					var atts = GetAttributEntities(blockRef, tr);
					foreach (var att in atts)
					{
						if (att.Tag == attName)
						{
							att.UpgradeOpen();
							att.TextString = "TOP " + topNr;
						}
					}
					tr.Commit();
				}
			}
			catch (System.Exception ex)
			{
				Log.Error(string.Format(CultureInfo.CurrentCulture, "Fehler beim Ändern der Attribute: {0}", ex.Message), ex);
				ok = false;

			}
			return ok;
		}
		private static bool SetTopNr(Database db, ObjectId blockOid, string topNr, string attName)
		{
			bool ok = true;
			try
			{
				using (Transaction tr = db.TransactionManager.StartTransaction())
				{
					var blockRef = (BlockReference)tr.GetObject(blockOid, OpenMode.ForRead);

					var atts = GetAttributEntities(blockRef, tr);
					foreach (var att in atts)
					{
						if (att.Tag == attName)
						{
							att.UpgradeOpen();
							att.TextString = topNr;

							var tok = Plan2Ext.Globs.IsTextAngleOk(att.Rotation, Plan2Ext.Globs.GetRadRotationTolerance());
							if (!tok)
							{
								att.Rotation += Math.PI;
							}
						}
					}
					tr.Commit();
				}
			}
			catch (System.Exception ex)
			{
				Log.Error(string.Format(CultureInfo.CurrentCulture, "Fehler beim Ändern der Attribute: {0}", ex.Message), ex);
				ok = false;

			}
			return ok;
		}

		public static List<AttributeReference> GetAttributEntities(BlockReference blockRef, Transaction tr)
		{
			var atts = new List<AttributeReference>();
			foreach (ObjectId attId in blockRef.AttributeCollection)
			{
				if (attId.IsErased) continue;
				var anyAttRef = tr.GetObject(attId, OpenMode.ForRead) as AttributeReference;
				if (anyAttRef != null)
				{
					atts.Add(anyAttRef);
				}
			}
			return atts;
		}

#endif

		private static string IncrementNumberInString(string s)
		{
			string prefix, suffix;
			int? i = Plan2Ext.Globs.GetFirstIntInString(s, out prefix, out suffix);
			if (i.HasValue)
			{
				int origIntLen = s.Length - (prefix.Length + suffix.Length);
				var incI = i.Value + 1;
				var iString = incI.ToString().PadLeft(origIntLen, '0');
				return prefix + iString + suffix;
			}
			else
			{
				return prefix + suffix;
			}
		}

		[CommandMethod("Plan2RaumnummerSelTop")]
		public void Plan2RaumnummerSelTop()
		{
			try
			{
				if (!OpenRnPalette()) return;

				var opts = Globs.TheRnOptions;
				Document doc = Application.DocumentManager.MdiActiveDocument;

				using (DocumentLock m_doclock = doc.LockDocument())
				{
					if (doc == null) return;
					Editor ed = doc.Editor;
#if NEWSETFOCUS
					doc.Window.Focus();
#else
                    Autodesk.AutoCAD.Internal.Utils.SetFocusToDwgView(); // previous 2014 AutoCAD - Versions
#endif

					PromptEntityResult per = ed.GetEntity("\nTopblock oder Pfeilblock wählen: ");
					if (per.Status == PromptStatus.OK)
                    {
                        var topWasSet = false;
						using (var tr = doc.TransactionManager.StartTransaction())
						{
							DBObject obj = tr.GetObject(per.ObjectId, OpenMode.ForRead);
							var bref = obj as BlockReference;
							if (bref != null)
							{
								var attribs = Plan2Ext.Globs.GetAttributes(bref);
								if (bref.Name == PFEILBLOCKNAME)
								{
									string tops;
									if (attribs.TryGetValue(PFEILBLOCK_TOPNR_ATTNAME, out tops))
									{
										opts.SetTop(tops);
                                        topWasSet = true;
                                    }
								}
								else if (bref.Name == TOPBLOCKNAME)
								{
									string tops;
									if (attribs.TryGetValue(TOPBLOCK_TOPNR_ATTNAME, out tops))
									{
										opts.SetTop(tops);
                                        topWasSet = true;
                                    }
								}
							}
							tr.Commit();
						}
                        if (topWasSet) AddNumber(opts);
					}
				}
			}
			catch (System.Exception ex)
			{
				Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2MoveFbhWithNumber aufgetreten! {0}", ex.Message));
			}
		}

		[CommandMethod("Plan2RaumnummerSelHBlock")]
		public void Plan2RaumnummerSelHBlock()
		{
			try
			{
				if (!OpenRnPalette()) return;

				var opts = Globs.TheRnOptions;
				Document doc = Application.DocumentManager.MdiActiveDocument;

				using (DocumentLock m_doclock = doc.LockDocument())
				{
					DocumentCollection dm = Application.DocumentManager;
					if (doc == null) return;
					Editor ed = doc.Editor;
#if NEWSETFOCUS
					doc.Window.Focus();
#else
                Autodesk.AutoCAD.Internal.Utils.SetFocusToDwgView(); // previous 2014 AutoCAD - Versions
#endif

					PromptEntityResult per = ed.GetEntity("\nHöhenblock wählen: ");
					if (per.Status == PromptStatus.OK)
					{
						Transaction tr = doc.TransactionManager.StartTransaction();
						using (tr)
						{
							DBObject obj = tr.GetObject(per.ObjectId, OpenMode.ForRead);
							BlockReference br = obj as BlockReference;
							if (br == null) return;

							opts.SetHBlockname(br.Name);

							tr.Commit();
						}
					}
				}

			}
			catch (System.Exception ex)
			{
				Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2MoveFbhWithNumber aufgetreten! {0}", ex.Message));
			}
		}

		[CommandMethod("Plan2RaumnummerSelBlockAndAtt")]
		public void Plan2RaumnummerSelBlockAndAtt()
		{
			try
			{
				if (!OpenRnPalette()) return;

				var opts = Globs.TheRnOptions;
				Document doc = Application.DocumentManager.MdiActiveDocument;

				using (DocumentLock m_doclock = doc.LockDocument())
				{
					DocumentCollection dm = Application.DocumentManager;
					if (doc == null) return;
					Editor ed = doc.Editor;
#if NEWSETFOCUS
					doc.Window.Focus();
#else
                Autodesk.AutoCAD.Internal.Utils.SetFocusToDwgView(); // previous 2014 AutoCAD - Versions
#endif

					PromptEntityResult per = ed.GetEntity("\nRaumblock wählen: ");

					if (per.Status == PromptStatus.OK)
					{

						Transaction tr = doc.TransactionManager.StartTransaction();
						using (tr)
						{
							DBObject obj = tr.GetObject(per.ObjectId, OpenMode.ForRead);
							BlockReference br = obj as BlockReference;
							if (br == null) return;

							opts.SetBlockname(br.Name);

							tr.Commit();
						}

						per = ed.GetNestedEntityEx("\nNummer-Attribut wählen: ");
						if (per.Status == PromptStatus.OK)
						{
							tr = doc.TransactionManager.StartTransaction();
							using (tr)
							{
								DBObject obj = tr.GetObject(per.ObjectId, OpenMode.ForRead);
								AttributeReference ar = obj as AttributeReference;
								if (ar == null) return;

								opts.SetAttribname(ar.Tag);

								tr.Commit();
							}
						}
						per = ed.GetNestedEntityEx("\nFlächen-Attribut wählen: ");
						if (per.Status == PromptStatus.OK)
						{
							tr = doc.TransactionManager.StartTransaction();
							using (tr)
							{
								DBObject obj = tr.GetObject(per.ObjectId, OpenMode.ForRead);
								AttributeReference ar = obj as AttributeReference;
								if (ar == null) return;

								opts.SetFlaechenAttributName(ar.Tag);

								tr.Commit();
							}
						}
					}
				}

			}
			catch (System.Exception ex)
			{
				Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2MoveFbhWithNumber aufgetreten! {0}", ex.Message));
			}
		}

		[CommandMethod("Plan2RaumnummerSelFgLayer")]
		public void Plan2RaumnummerSelFgLayer()
		{
			try
			{
				if (!OpenRnPalette()) return;

				var opts = Globs.TheRnOptions;
				Document doc = Application.DocumentManager.MdiActiveDocument;

				using (DocumentLock m_doclock = doc.LockDocument())
				{
					DocumentCollection dm = Application.DocumentManager;
					if (doc == null) return;
					Editor ed = doc.Editor;
#if NEWSETFOCUS
					doc.Window.Focus();
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

							if (string.Compare(opts.AbzFlaechenGrenzeLayerName, layer, StringComparison.OrdinalIgnoreCase) == 0)
							{
								Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Die Layer für Flächengrenze und Abzugsfläche müssen unterschiedlich sein."));
								return;
							}

							opts.SetFlaechenGrenzeLayerName(layer);
							tr.Commit();
						}
					}
				}

			}
			catch (System.Exception ex)
			{
				Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2RaumnummerSelFgLayer aufgetreten! {0}", ex.Message));
			}
		}

		[CommandMethod("Plan2RaumnummerSelAbzFgLayer")]
		public void Plan2RaumnummerSelAbzFgLayer()
		{
			try
			{
				if (!OpenRnPalette()) return;

				var opts = Globs.TheRnOptions;
				Document doc = Application.DocumentManager.MdiActiveDocument;

				using (DocumentLock m_doclock = doc.LockDocument())
				{
					DocumentCollection dm = Application.DocumentManager;
					if (doc == null) return;
					Editor ed = doc.Editor;
#if NEWSETFOCUS
					doc.Window.Focus();
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

							if (string.Compare(opts.FlaechenGrenzeLayerName, layer, StringComparison.OrdinalIgnoreCase) == 0)
							{
								Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Die Layer für Flächengrenze und Abzugsfläche müssen unterschiedlich sein."));
								return;
							}

							opts.SetAbzFlaechenGrenzeLayerName(layer);
							tr.Commit();
						}
					}
				}
			}
			catch (System.Exception ex)
			{
				Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2RaumnummerSelAabzFgLayer aufgetreten! {0}", ex.Message));
			}
		}

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

		[CommandMethod("Plan2MoveFbhWithNumber")]
		public void Plan2MoveFbhWithNumber()
		{
			string distVar = "alx_V:ino_rb_fbhYDistWithNr";

			Plan2MoveFb(distVar);
		}

		[CommandMethod("Plan2MoveFbhWithOutNumber")]
		public void Plan2MoveFbhWithOutNumber()
		{
			string distVar = "alx_V:ino_rb_fbhYDistNoNr";

			Plan2MoveFb(distVar, true);
		}

		private void Plan2MoveFb(string distVar, bool ignoreIfNrExists = false)
		{
			try
			{
				if (!OpenRnPalette()) return;

				var opts = Globs.TheRnOptions;
				Document doc = Application.DocumentManager.MdiActiveDocument;

				using (DocumentLock m_doclock = doc.LockDocument())
				{

					var engine = GetEngine();

					string sDist = TheConfiguration.GetValueString(distVar);
					double dist = double.Parse(sDist, CultureInfo.InvariantCulture);

					engine.MoveFbh(0.0, dist, ignoreIfNrExists);
				}

			}
			catch (System.Exception ex)
			{
				Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2MoveFbhWithNumber aufgetreten! {0}", ex.Message));
			}
		}

		[CommandMethod("Plan2RaumnummernRemoveAllInfos")]
		public void Plan2RaumnummernRemoveAllInfos()
		{
			try
			{
				if (!OpenRnPalette()) return;

				var opts = Globs.TheRnOptions;
				Document doc = Application.DocumentManager.MdiActiveDocument;

				using (DocumentLock m_doclock = doc.LockDocument())
				{

					var engine = GetEngine();

					engine.RemoveAllInfos();
				}

			}
			catch (System.Exception ex)
			{
				Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2RaumnummernRemoveAllInfos aufgetreten! {0}", ex.Message));
			}
		}

		[CommandMethod("Plan2RaumnummernRenameTop")]
		public void Plan2RaumnummernRenameTop()
		{
			try
			{
				if (!OpenRnPalette()) return;

				var opts = Globs.TheRnOptions;
				Document doc = Application.DocumentManager.MdiActiveDocument;

				using (DocumentLock m_doclock = doc.LockDocument())
				{

					var engine = GetEngine();
					engine.RenameTop(opts.Top);
				}

			}
			catch (System.Exception ex)
			{
				Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2RaumnummernRenameTop aufgetreten! {0}", ex.Message));
			}
		}

		[CommandMethod("Plan2RaumnummernRemoveRaum")]
		public void Plan2RaumnummernRemoveRaum()
		{
			try
			{
				if (!OpenRnPalette()) return;

				var opts = Globs.TheRnOptions;
				Document doc = Application.DocumentManager.MdiActiveDocument;

				using (DocumentLock m_doclock = doc.LockDocument())
				{

					var engine = GetEngine();

					while (engine.RemoveRaum()) { };
				}

			}
			catch (System.Exception ex)
			{
				Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2RaumnummernRemoveRaum aufgetreten! {0}", ex.Message));
			}
		}

		[CommandMethod("Plan2RaumnummernCalcArea")]
		public void Plan2RaumnummernCalcArea()
		{
			try
			{
				if (!OpenRnPalette()) return;

				Plan2Ext.Kleinbefehle.Layers.Plan2SaveLayerStatus();

				var opts = Globs.TheRnOptions;
				Document doc = Application.DocumentManager.MdiActiveDocument;

				using (DocumentLock m_doclock = doc.LockDocument())
				{
					Plan2Ext.Flaeche.Modify = true;
					Plan2Ext.Flaeche.AktFlaeche(
						Application.DocumentManager.MdiActiveDocument,
						opts.Blockname, opts.FlaechenAttributName, opts.UmfangAttributName, opts.FlaechenGrenzeLayerName, opts.AbzFlaechenGrenzeLayerName
						);
				}

			}
			catch (System.Exception ex)
			{
				Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2RaumnummernCalcArea aufgetreten! {0}", ex.Message));
			}
		}

		static RnPalette _RnPalette;

		[CommandMethod("Plan2RaumnummernBereinig")]
		public void Plan2RaumnummernBereinig()
		{
			try
			{

				if (!OpenRnPalette()) return;

				var opts = Globs.TheRnOptions;
				Document doc = Application.DocumentManager.MdiActiveDocument;

				using (DocumentLock m_doclock = doc.LockDocument())
				{
					Plan2Ext.Flaeche.BereinigFehlerlinienAndRegions(automated: false);
					var engine = GetEngine();
					engine.BereinigFehlerlinien();
				}
			}
			catch (System.Exception ex)
			{
				Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2RaumnummernBereinig aufgetreten! {0}", ex.Message));
			}
		}


		[LispFunction("Plan2Raumnummern")]
		public static object Plan2Raumnummern(ResultBuffer rb)
		{
			Log.Debug("--------------------------------------------------------------------------------");
			Log.Debug("Plan2Raumnummern");
			try
			{
				Document doc = Application.DocumentManager.MdiActiveDocument;
				Log.DebugFormat("Dokumentname: {0}.", doc.Name);

				if (_RnPalette == null)
				{
					_RnPalette = new RnPalette();
				}

				bool wasOpen = _RnPalette.Show();

				if (wasOpen)
				{
					return true;
				}
				else
					return false; // returns nil
			}
			catch (System.Exception ex)
			{
				Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2Raumnummern aufgetreten! {0}", ex.Message));
			}
			return false;
		}

		private static bool OpenRnPalette()
		{
			if (_RnPalette == null)
			{
				_RnPalette = new RnPalette();
			}

			bool wasOpen = _RnPalette.Show();
			if (!wasOpen) return false;

			Document doc = Application.DocumentManager.MdiActiveDocument;
			if (doc == null) return false;
			Log.DebugFormat("Dokumentname: {0}.", doc.Name);

			if (Globs.TheRnOptions == null) return false;
			else return true;
		}

		private Engine GetEngine()
		{
			var doc = Application.DocumentManager.MdiActiveDocument;
			InitFgRbStructs(doc);
			var fgRbsPerTopNr = new FgRbsPerTopNr(_fgRbStructs.Values);

			var engineParameter = new EngineParameter()
			{
				Options = Globs.TheRnOptions,
				FgRbs = _fgRbStructs,
				AllRaumBlocks = _allRaumBlocks,
				FgRbPerTopNr = fgRbsPerTopNr
			};

			return new Engine(engineParameter);
		}

		private void InitFgRbStructs(Document doc)
		{
			var opts = Globs.TheRnOptions;
			if (_fgRbStructs.Count == 0)
			{
				_fgRbStructs = AreaEngine.GetFgRbStructs(opts.Blockname, opts.FlaechenGrenzeLayerName,
					opts.AbzFlaechenGrenzeLayerName, doc.Database);
				_allRaumBlocks = Engine.SelectAllRaumblocks(opts.Blockname);
			}
		}

        private static string ProjektName = "";

        [CommandMethod("Plan2RaumnummernExcelExport")]
        public void Plan2RaumnummernExcelExport()
        {
            try
            {
                var opts = Globs.TheRnOptions ?? new RnOptions();
                var doc = Application.DocumentManager.MdiActiveDocument;

                using (doc.LockDocument())
                {
                    var geschossNameHelper = (IGeschossnameHelper)new GeschossnameHelper();

					Plan2Ext.Raumnummern.ExcelExport.BlockInfo.DeleteFehlerlines();

                    if (GetProjectName(doc)) return;

                    var readyKey = "Fertig";
                    var geschossKeywordsList = geschossNameHelper.GetKeywords().ToList();
					geschossKeywordsList.Add(readyKey);
                    var geschossKeywords = geschossKeywordsList.ToArray();

					var model = new ExcelExportModel(ProjektName);

                    string geschossKurz;
                    do
                    {
                        geschossKurz =
                            Plan2Ext.Globs.AskKeywordFromUser("", geschossKeywords, geschossKeywords.Length - 1);
                        if (geschossKurz == null) return;
                        if (geschossKurz.Equals(readyKey)) continue;
                        var rbOids = Engine.SelectRaumblocks(opts, doc);
                        model.Add(geschossKurz, rbOids, opts, doc);

                    } while (!geschossKurz.Equals(readyKey));

					if (model.BlockInfos.Count <= 0) return;

					var exporter = new ExcelExporter();
					exporter.Export(model, geschossNameHelper, doc);
                }
            }
            catch (System.Exception ex)
            {
                Application.ShowAlertDialog(string.Format(CultureInfo.CurrentCulture, "Fehler in Plan2Raumnummern aufgetreten! {0}", ex.Message));
            }
        }

        private static bool GetProjectName(Document doc)
        {
            var pso = new PromptStringOptions("Projektname: ")
            {
                AllowSpaces = true, DefaultValue = ProjektName, UseDefaultValue = true
            };
            var res = doc.Editor.GetString(pso);
            if (res.Status != PromptStatus.OK) return true;
            ProjektName = res.StringResult;
            return false;
        }
    }
}
