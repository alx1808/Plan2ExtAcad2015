// ReSharper disable StringLiteralTypo
// ReSharper disable IdentifierTypo
// ReSharper disable CommentTypo
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Globalization;
using Excel = Microsoft.Office.Interop.Excel;


#if BRX_APP
using _AcAp = Bricscad.ApplicationServices;
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



namespace Plan2Ext.LayTrans
{
	internal class Engine
	{
		#region log4net Initialization
		private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(Convert.ToString((typeof(Engine))));
		#endregion


		private readonly List<string> _header = new List<string>() { "Alter Layer", "Neuer Layer", "Farbe", "Linientyp", "Linienstärke", "Transparenz", "Plot", "Beschreibung" };
		private readonly List<string> _headerWithNrElements = new List<string>() { "Layer", "Anzahl der Elemente", "Farbe", "Linientyp", "Linienstärke", "Transparenz", "Plot", "Beschreibung" };
		private readonly List<string> _errors = new List<string>();

		internal bool LayTrans(string fileName)
		{

			Globs.UnlockAllLayers();

			_errors.Clear();
			var linfos = ExcelImport(fileName);

			foreach (var err in _errors)
			{
				Log.Warn(err);
			}

			var doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
			var db = doc.Database;
			using (_AcDb.Transaction trans = doc.TransactionManager.StartTransaction())
			{
				_AcDb.LayerTable layTb = trans.GetObject(db.LayerTableId, _AcDb.OpenMode.ForWrite) as _AcDb.LayerTable;

				foreach (var linfo in linfos)
				{
					CreateOrModifyLayer(linfo, trans, layTb);
				}

				trans.Commit();
			}

			Dictionary<string, string> substDict = new Dictionary<string, string>();
			foreach (var linfo in linfos)
			{
				substDict[linfo.OldLayer.ToUpperInvariant()] = linfo.NewLayer;
			}

			using (_AcDb.Transaction trans = doc.TransactionManager.StartTransaction())
			{
				ReplaceLayerInEntities(substDict, trans, db);

				trans.Commit();
			}

			Globs.SetLayerCurrent("0");

			List<string> oldLayerNames = linfos.Select(x => x.OldLayer).ToList();
			Globs.PurgeLayer(oldLayerNames);


			return true;
		}

		private void ReplaceLayerInEntities(Dictionary<string, string> substDict, _AcDb.Transaction trans, _AcDb.Database db)
		{
			_AcDb.BlockTable blkTable = (_AcDb.BlockTable)trans.GetObject(db.BlockTableId, _AcDb.OpenMode.ForRead);
			foreach (var id in blkTable)
			{
				_AcDb.BlockTableRecord btRecord = (_AcDb.BlockTableRecord)trans.GetObject(id, _AcDb.OpenMode.ForRead);
				//System.Diagnostics.Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Block: {0}", btRecord.Name));

				//btRecord.UpgradeOpen();

				foreach (var entId in btRecord)
				{
					_AcDb.Entity entity = (_AcDb.Entity)trans.GetObject(entId, _AcDb.OpenMode.ForWrite);

					CheckSetLayer(substDict, entity);
					//if (string.Compare(entity.Layer, oldLayer, StringComparison.OrdinalIgnoreCase) == 0)
					//{
					//    entity.Layer = newLayer;
					//}

					_AcDb.BlockReference block = entity as _AcDb.BlockReference;
					if (block != null)
					{
						// sequend correction
						string saveLay = block.Layer;
						block.Layer = "0";
						block.Layer = saveLay;

						foreach (var att in block.AttributeCollection)
						{
							_AcDb.ObjectId attOid = (_AcDb.ObjectId)att;
                            if (attOid.IsErased) continue;
							_AcDb.AttributeReference attrib = trans.GetObject(attOid, _AcDb.OpenMode.ForWrite) as _AcDb.AttributeReference;
							if (attrib != null)
							{
								CheckSetLayer(substDict, attrib);
								//if (string.Compare(attrib.Layer, oldLayer, StringComparison.OrdinalIgnoreCase) == 0)
								//{
								//    attrib.Layer = newLayer;
								//}
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
				//System.Diagnostics.Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Layout: {0}", lo.LayoutName));

				_AcDb.BlockTableRecord btRecord = (_AcDb.BlockTableRecord)trans.GetObject(lo.BlockTableRecordId, _AcDb.OpenMode.ForRead);
				foreach (var entId in btRecord)
				{
					_AcDb.Entity entity = (_AcDb.Entity)trans.GetObject(entId, _AcDb.OpenMode.ForWrite);

					CheckSetLayer(substDict, entity);

					//if (string.Compare(entity.Layer, oldLayer, StringComparison.OrdinalIgnoreCase) == 0)
					//{
					//    entity.Layer = newLayer;
					//}

					_AcDb.BlockReference block = entity as _AcDb.BlockReference;
					if (block != null)
					{

						string saveLay = block.Layer;
						block.Layer = "0";
						block.Layer = saveLay;

						foreach (var att in block.AttributeCollection)
						{
							_AcDb.ObjectId attOid = (_AcDb.ObjectId)att;
                            if (attOid.IsErased) continue;
							_AcDb.AttributeReference attrib = trans.GetObject(attOid, _AcDb.OpenMode.ForWrite) as _AcDb.AttributeReference;
							if (attrib != null)
							{
								CheckSetLayer(substDict, attrib);
								//if (string.Compare(attrib.Layer, oldLayer, StringComparison.OrdinalIgnoreCase) == 0)
								//{

								//    //attrib.UpgradeOpen();
								//    attrib.Layer = newLayer;
								//}
							}
						}

					}

				}

			}



		}

		private static void CheckSetLayer(Dictionary<string, string> substDict, _AcDb.Entity entity)
		{
			string newLayer;
			if (substDict.TryGetValue(entity.Layer.ToUpperInvariant(), out newLayer))
			{
				entity.Layer = newLayer;
			}
		}

		private void CreateOrModifyLayer(LayerInfo layerInfo, _AcDb.Transaction trans, _AcDb.LayerTable layTb)
		{

			if (layTb.Has(layerInfo.NewLayer))
			{
				var oid = layTb[layerInfo.NewLayer];
				_AcDb.LayerTableRecord ltr = (_AcDb.LayerTableRecord)trans.GetObject(oid, _AcDb.OpenMode.ForWrite);
				layerInfo.ModifyLayer(ltr);
			}
			else
			{
				using (_AcDb.LayerTableRecord ltr = new _AcDb.LayerTableRecord())
				{
					// Assign the layer a name
					ltr.Name = layerInfo.NewLayer;

					// Upgrade the Layer table for write
					//layTb.UpgradeOpen();


					// Append the new layer to the Layer table and the transaction
					layTb.Add(ltr);
					trans.AddNewlyCreatedDBObject(ltr, true);

					layerInfo.ModifyLayer(ltr);

				}
			}


		}

		private List<LayerInfo> ExcelImport(string fileName)
		{
			Excel.Application myApp = new Excel.Application();
			Excel.Workbook workBook = null;
			Excel.Worksheet sheet = null;
			try
			{
				workBook = myApp.Workbooks.Open(fileName, Missing.Value, true); //, 5, "", "", true, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);
																				// ReSharper disable once UseIndexedProperty
				sheet = workBook.Worksheets.get_Item(1);

				var biis = GetLayerInfos(sheet);

				workBook.Close(false, Missing.Value, Missing.Value);
				myApp.Quit();

				return biis;

			}
			finally
			{
				releaseObject(sheet);
				releaseObject(workBook);
				releaseObject(myApp);
			}
		}

		private List<LayerInfo> GetLayerInfos(Excel.Worksheet sheet)
		{
			// test import
			int nrRows;
			var nrCols = _header.Count;
			GetNrRows(sheet, out nrRows);
			var b1 = GetCellBez(0, 0);
			var b2 = GetCellBez(nrRows, nrCols);
			var range = sheet.Range[b1, b2];
			// ReSharper disable once UseIndexedProperty
			object[,] impMatrix = range.get_Value(Excel.XlRangeValueDataType.xlRangeValueDefault);

			var doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
			var db = doc.Database;

			List<LayerInfo> biis = new List<LayerInfo>();
			using (_AcDb.Transaction trans = doc.TransactionManager.StartTransaction())
			{
				try
				{
					for (int r = 2; r <= nrRows; r++)
					{
						for (int c = 1; c <= nrCols; c++)
						{
							if (impMatrix[r, c] == null) impMatrix[r, c] = "";
						}
					}


					for (int r = 2; r <= nrRows; r++)
					{

						//List<string> HEADER = new List<string>() { "Alter Layer", "Neuer Layer", "Farbe", "Linientyp", "Linienstärke", "Transparenz", "Beschreibung" };

						LayerInfo layerInfo = new LayerInfo
						{
							OldLayer = impMatrix[r, 1].ToString(),
							NewLayer = impMatrix[r, 2].ToString(),
							Color = impMatrix[r, 3].ToString()
						};
						layerInfo.SetLineType(impMatrix[r, 4].ToString(), trans, db);
						layerInfo.LineWeight = impMatrix[r, 5].ToString();
						layerInfo.Transparency = impMatrix[r, 6].ToString();
						layerInfo.Plot = impMatrix[r, 7].ToString();
						layerInfo.Description = impMatrix[r, 8].ToString();

						if (layerInfo.Ok)
						{
							biis.Add(layerInfo);
						}
						if (!string.IsNullOrEmpty(layerInfo.Errors))
						{
							_errors.Add(layerInfo.Errors);
						}

					}

				}
				finally
				{

					trans.Commit();
				}
			}

			return biis;
		}

		private const int Maxrows = 3000;
		private void GetNrRows(Excel.Worksheet sheet, out int nrRows)
		{
			nrRows = 0;

			var b1 = GetCellBez(0, 0);
			var b2 = GetCellBez(Maxrows, 0);
			Log.DebugFormat("B1 '{0}' und B2 '{1}", b1, b2);
			var range = sheet.Range[b1, b2];
			Log.DebugFormat("Nach getrange!");

			// ReSharper disable once UseIndexedProperty
			object[,] indexMatrix = range.get_Value(Excel.XlRangeValueDataType.xlRangeValueDefault);

			for (int i = 1; i <= Maxrows; i++)
			{
				var v1 = indexMatrix[i, 1];
				if (v1 == null) break;
				nrRows++;
			}

		}
		private void releaseObject(object obj)
		{
			try
			{
				if (obj != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(obj);
			}
			catch (Exception)
			{
				// ignored
			}
			finally
			{
				GC.Collect();
			}
		}


		internal bool ExcelExport(string[] dwgFiles = null)
		{
			Excel.Application myApp = null;
			Excel.Workbook workBook = null;
			Excel.Worksheet sheet = null;

			try
			{
				myApp = new Excel.Application();

				var layerInfos = dwgFiles == null ? GetLayerInfos() : GetLayerInfos(dwgFiles.ToList());

				workBook = myApp.Workbooks.Add(Missing.Value);
				sheet = workBook.ActiveSheet;

				// Pull in all the cells of the worksheet
				Excel.Range cells = sheet.Cells;
				// set each cell's format to Text
				cells.NumberFormat = "@";
				//cells.HorizontalAlignment = XlHAlign.xlHAlignRight;

				//var colLists = valuesPerColumn.Values.ToList();


				var b1 = GetCellBez(0, 0);
				var b2 = GetCellBez(0, _header.Count);
				var range = sheet.Range[b1, b2];
				range.Font.Bold = true;
				range.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;


				int rowCount = 1 + layerInfos.Count; // colLists[0].Count;
				int colCount = _header.Count; // colLists.Count;
				b2 = GetCellBez(rowCount - 1, colCount - 1);
				range = sheet.Range[b1, b2];
				//range.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;

				string[,] indexMatrix = new string[rowCount, colCount];
				for (int i = 0; i < _header.Count; i++)
				{
					indexMatrix[0, i] = _header[i];
				}
				for (int r = 1; r <= layerInfos.Count; r++)
				{
					var linfo = layerInfos[r - 1];
					List<string> vals = linfo.RowAsList();
					for (int i = 0; i < vals.Count; i++)
					{
						indexMatrix[r, i] = vals[i];
					}
				}

				// ReSharper disable once UseIndexedProperty
				range.set_Value(Excel.XlRangeValueDataType.xlRangeValueDefault, indexMatrix);

				range.Font.Name = "Arial";
				range.Columns.AutoFit();

			}
			finally
			{
				if (myApp != null)
				{
					myApp.Visible = true;
					myApp.ScreenUpdating = true;
				}

				releaseObject(sheet);
				releaseObject(workBook);
				releaseObject(myApp);

			}

			return true;
		}

		internal bool ExcelExportWithNrElements(string[] dwgFiles = null)
		{
			if (dwgFiles != null) return false; // not supported yet

			Excel.Application myApp = null;
			Excel.Workbook workBook = null;
			Excel.Worksheet sheet = null;

			try
			{
				var elementsPerLayer = GetNrElementsPerLayer();
				myApp = new Excel.Application();

				var layerInfos = GetLayerInfosWithNrElements(elementsPerLayer);

				workBook = myApp.Workbooks.Add(Missing.Value);
				sheet = workBook.ActiveSheet;

				// Pull in all the cells of the worksheet
				Excel.Range cells = sheet.Cells;
				// set each cell's format to Text
				cells.NumberFormat = "@";

				var b1 = GetCellBez(0, 0);
				var b2 = GetCellBez(0, _headerWithNrElements.Count);
				var range = sheet.Range[b1, b2];
				range.Font.Bold = true;
				range.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;


				int rowCount = 1 + layerInfos.Count; // colLists[0].Count;
				int colCount = _headerWithNrElements.Count; // colLists.Count;
				b2 = GetCellBez(rowCount - 1, colCount - 1);
				range = sheet.Range[b1, b2];
				//range.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;

				string[,] indexMatrix = new string[rowCount, colCount];
				for (int i = 0; i < _headerWithNrElements.Count; i++)
				{
					indexMatrix[0, i] = _headerWithNrElements[i];
				}
				for (int r = 1; r <= layerInfos.Count; r++)
				{
					var linfo = layerInfos[r - 1];
					List<string> vals = linfo.RowAsList();
					for (int i = 0; i < vals.Count; i++)
					{
						indexMatrix[r, i] = vals[i];
					}
				}

				// ReSharper disable once UseIndexedProperty
				range.set_Value(Excel.XlRangeValueDataType.xlRangeValueDefault, indexMatrix);
				range.Font.Name = "Arial";
				range.Columns.AutoFit();
			}
			finally
			{
				if (myApp != null)
				{
					myApp.Visible = true;
					myApp.ScreenUpdating = true;
				}

				releaseObject(sheet);
				releaseObject(workBook);
				releaseObject(myApp);
			}

			return true;
		}

		/// <summary>
		/// Bulk-Befehl
		/// </summary>
		/// <param name="dwgFiles"></param>
		/// <returns></returns>
		private List<LayerInfo> GetLayerInfos(List<string> dwgFiles)
		{
			var layDict = new Dictionary<string, LayerInfo>();
			foreach (var fileName in dwgFiles)
			{
				int layerCount = 0;
				int layerAddedCount = 0;
				var db = new _AcDb.Database(false, true);
				using (db)
				{
					try
					{
						db.ReadDwgFile(fileName, System.IO.FileShare.Read, allowCPConversion: false, password: "");
					}
					catch (Exception ex)
					{
						Log.WarnFormat("Fehler beim Öffnen der Zeichnung '{0}'!{1}", fileName, ex.Message);
						continue;
					}
					using (_AcDb.Transaction trans = db.TransactionManager.StartTransaction())
					{
						try
						{
							_AcDb.LayerTable layTb = (_AcDb.LayerTable)trans.GetObject(db.LayerTableId, _AcDb.OpenMode.ForRead);
							foreach (var ltrOid in layTb)
							{
								_AcDb.LayerTableRecord ltr = (_AcDb.LayerTableRecord)trans.GetObject(ltrOid, _AcDb.OpenMode.ForRead);
								string layNameUc = ltr.Name.ToUpperInvariant();
								layerCount++;
								if (!layDict.ContainsKey(layNameUc))
								{
									layDict.Add(layNameUc, new LayerInfo(ltr, trans));
									layerAddedCount++;
								}
							}
						}
						finally
						{
							trans.Commit();
						}
					}
				}
				Log.InfoFormat("{0}: {1} von {2} Layer hinzugefügt.", fileName, layerAddedCount, layerCount);
			}

			var linfos = layDict.Values.ToList();
			return linfos;
		}

		private List<LayerInfo> GetLayerInfos()
		{
			List<LayerInfo> linfos = new List<LayerInfo>();
			var doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
			var db = doc.Database;

			using (_AcDb.Transaction trans = doc.TransactionManager.StartTransaction())
			{
				try
				{
					_AcDb.LayerTable layTb = (_AcDb.LayerTable)trans.GetObject(db.LayerTableId, _AcDb.OpenMode.ForRead);
					foreach (var ltrOid in layTb)
					{
						_AcDb.LayerTableRecord ltr = (_AcDb.LayerTableRecord)trans.GetObject(ltrOid, _AcDb.OpenMode.ForRead);
						linfos.Add(new LayerInfo(ltr, trans));
					}
				}
				finally
				{
					trans.Commit();
				}
			}

			return linfos;
		}

		private List<LayerInfoWithNrElements> GetLayerInfosWithNrElements(Dictionary<string, int> elementsPerLayer)
		{
			List<LayerInfoWithNrElements> linfos = new List<LayerInfoWithNrElements>();
			var doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
			var db = doc.Database;

			using (_AcDb.Transaction trans = doc.TransactionManager.StartTransaction())
			{
				try
				{
					_AcDb.LayerTable layTb = (_AcDb.LayerTable)trans.GetObject(db.LayerTableId, _AcDb.OpenMode.ForRead);
					foreach (var ltrOid in layTb)
					{
						_AcDb.LayerTableRecord ltr = (_AcDb.LayerTableRecord)trans.GetObject(ltrOid, _AcDb.OpenMode.ForRead);
						int nrElements;
						if (!elementsPerLayer.TryGetValue(ltr.Name, out nrElements))
						{
							nrElements = 0;
						}
						linfos.Add(new LayerInfoWithNrElements(ltr, trans, nrElements));
					}
				}
				finally
				{
					trans.Commit();
				}
			}

			return linfos;
		}

		private Dictionary<string, int> GetNrElementsPerLayer()
		{
			var dict = new Dictionary<string, int>();
			var doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
			var db = doc.Database;

			using (_AcDb.Transaction trans = doc.TransactionManager.StartTransaction())
			{
				try
				{
					_AcDb.BlockTable blockTb = (_AcDb.BlockTable)trans.GetObject(db.BlockTableId, _AcDb.OpenMode.ForRead);
					foreach (var btrOid in blockTb)
					{
						var btr = (_AcDb.BlockTableRecord)trans.GetObject(btrOid, _AcDb.OpenMode.ForRead);
						foreach (_AcDb.ObjectId objId in btr)
						{
							_AcDb.Entity ent = trans.GetObject(objId, _AcDb.OpenMode.ForRead) as _AcDb.Entity;
							CountEntity(dict, ent);

							var bref = ent as _AcDb.BlockReference;
							if (bref != null)
							{
								foreach (_AcDb.ObjectId attId in bref.AttributeCollection)
								{
                                    if (attId.IsErased) continue;
									ent = trans.GetObject(attId, _AcDb.OpenMode.ForRead) as _AcDb.Entity;
									CountEntity(dict, ent);
								}
							}
						}
					}
				}
				finally
				{
					trans.Commit();
				}
			}

			return dict;
		}

		private static void CountEntity(Dictionary<string, int> dict, _AcDb.Entity ent)
		{
			if (ent == null) return;
			string layer = ent.Layer;
			int nr;
			if (dict.TryGetValue(layer, out nr))
			{
				dict[layer] = ++nr;
			}
			else
			{
				dict[layer] = 1;
			}
		}

		private class LayerInfo
		{
			//List<string> HEADER = new List<string>() { "Alter Layer", "Neuer Layer", "Farbe", "Linientyp", "Linienstärke", "Transparenz", "Beschreibung" };

			#region Lifecycle
			public LayerInfo()
			{
			}
			public LayerInfo(_AcDb.LayerTableRecord ltr, _AcDb.Transaction trans)
			{
				OldLayer = ltr.Name;
				NewLayer = "";
				ColorO = ltr.Color;
				Color = ColorToString();
				LineTypeO = ltr.LinetypeObjectId;
				_LineType = GetNameFromLinetypeOid(ltr.LinetypeObjectId, trans);
				LineWeightO = ltr.LineWeight;
				LineWeight = LineWeightToString();
				TransparencyO = ltr.Transparency;
				if (TransparencyO != default(_AcCm.Transparency))
				{
					Transparency = AlphaToTransparenz(TransparencyO.Alpha).ToString(CultureInfo.InvariantCulture);
				}
				else
				{
					Transparency = string.Empty;
				}
				Plot = ltr.IsPlottable ? "Ja" : "Nein";

				Description = ltr.Description;
			}
			#endregion

			#region Internal
			internal virtual List<string> RowAsList()
			{
				return new List<string>() { OldLayer, NewLayer, Color, LineType, LineWeight, Transparency, Plot, Description };
			}

			internal void ModifyLayer(_AcDb.LayerTableRecord ltr)
			{

				if (ColorO != null)
				{
					ltr.Color = ColorO;
				}

				if (!LineTypeO.IsNull)
				{
					ltr.LinetypeObjectId = LineTypeO;
				}

				ltr.LineWeight = LineWeightO;

				if (TransparencyO != default(_AcCm.Transparency))
				{
					ltr.Transparency = TransparencyO;
				}
				else
				{
					ltr.Transparency = default(_AcCm.Transparency);
				}

				if (!string.IsNullOrEmpty(Description)) ltr.Description = Description;

				ltr.IsPlottable = _isPlottable;

			}
			#endregion

			#region Properties
			private string _errors = string.Empty;
			public string Errors { get { return _errors; } }

			public string OldLayer { get; set; }
			private string _newLayer = string.Empty;
			public string NewLayer
			{
				get { return _newLayer; }
				set
				{
					_newLayer = value;
					if (!string.IsNullOrEmpty(OldLayer) && !String.IsNullOrEmpty(_newLayer)) _ok = true;
					else
					{
						_errors = _errors + string.Format(CultureInfo.CurrentCulture, "\nKein neuer Layer für Layer '{0}'", OldLayer);
					}
				}
			}
			protected _AcCm.Color ColorO;
			private string _color = string.Empty;
			public string Color
			{
				protected get { return _color; }
				set
				{
					_color = value;
					StringToColor();
				}
			}

			protected _AcDb.ObjectId LineTypeO;
			// ReSharper disable once InconsistentNaming
			protected string _LineType = string.Empty;
			public void SetLineType(string lt, _AcDb.Transaction trans, _AcDb.Database db)
			{
				_LineType = lt;
				var lto = GetLinetypeFromName(_LineType, trans, db);
				if (lto == default(_AcDb.ObjectId))
				{
					_errors = _errors + string.Format(CultureInfo.CurrentCulture, "\nUngültiger Linientyp '{0}' für Layer '{1}'", _LineType, OldLayer);
					return;
				}

				LineTypeO = lto;
			}

			protected string LineType { get { return _LineType; } }
			protected _AcDb.LineWeight LineWeightO;
			private string _lineWeight = string.Empty;
			public string LineWeight
			{
				protected get { return _lineWeight; }
				set
				{
					_lineWeight = value;
					SetLineWeight();


				}
			}

			protected _AcCm.Transparency TransparencyO;
			private string _transparency = string.Empty;
			public string Transparency
			{
				protected get { return _transparency; }
				set
				{
					_transparency = value;
					SetTransparency();
				}
			}

			private bool _isPlottable;
			private string _plot = string.Empty;
			public string Plot
			{
				protected get { return _plot; }
				set
				{
					_plot = value;
					SetPlot();
				}
			}

			public string Description { protected get; set; }

			private bool _ok;
			public bool Ok { get { return _ok; } }

			#endregion

			#region Protected

			private void SetLineWeight()
			{
				LineWeightO = _AcDb.LineWeight.ByLineWeightDefault;
				if (string.IsNullOrEmpty(_lineWeight))
				{
					return;
				}
				double d;
				if (!double.TryParse(_lineWeight, NumberStyles.Any, CultureInfo.InvariantCulture, out d))
				{
					_errors = _errors + string.Format(CultureInfo.CurrentCulture, "\nUngültiger Eintrag für Linienstärke für Layer '{0}': {1}", OldLayer, _lineWeight);
					return;
				}

				d = d * 100.0;
				int val = (int)Math.Floor(d);
				string cmpVal = "LineWeight" + val.ToString().PadLeft(3, '0');

				foreach (var e in Enum.GetValues(typeof(_AcDb.LineWeight)))
				{
					if (cmpVal == e.ToString())
					{
						LineWeightO = (_AcDb.LineWeight)e;
						return;
					}
				}

				_errors = _errors + string.Format(CultureInfo.CurrentCulture, "\nUngültiger Eintrag für Linienstärke für Layer '{0}': {1}", OldLayer, _lineWeight);
			}

			private void SetTransparency()
			{
				if (string.IsNullOrEmpty(_transparency)) return;
				int t;
				if (!int.TryParse(_transparency, out t))
				{
					_errors = _errors + string.Format(CultureInfo.CurrentCulture, "\nUngültiger Eintrag für Transparenz für Layer '{0}': {1}", OldLayer, _transparency);
					return;
				}
				if (t < 0 || t > 90)
				{
					_errors = _errors + string.Format(CultureInfo.CurrentCulture, "\nUngültiger Eintrag für Transparenz für Layer '{0}': {1}", OldLayer, _transparency);
					return;
				}
				Byte alpha = TransparenzToAlpha(t);
				TransparencyO = new _AcCm.Transparency(alpha);

			}

			private void SetPlot()
			{
				_isPlottable = false;
				if (string.IsNullOrEmpty(_plot)) return;

				if (string.Compare(_plot.Trim(), "Ja", StringComparison.OrdinalIgnoreCase) == 0)
				{
					_isPlottable = true;
				}
			}

			protected string LineWeightToString()
			{
				int lw = (int)LineWeightO;
				if (lw < 0) return "";

				double d = lw / 100.0;
				return d.ToString("F", CultureInfo.InvariantCulture);
			}

			private void StringToColor()
			{
				if (string.IsNullOrEmpty(_color))
				{
					_errors = _errors + string.Format(CultureInfo.CurrentCulture, "\nFehler in Eintrag für Layer '{0}': Es ist keine Farbe festgelegt!", OldLayer);
					return;
				}

				var vals = _color.Split(new[] { '/' });
				if (vals.Length != 1 && vals.Length != 3)
				{
					_errors = _errors + string.Format(CultureInfo.CurrentCulture, "\nUngültiger Eintrag für Farbe für Layer '{0}': {1}", OldLayer, _color);
					return;
				}

				if (vals.Length == 1)
				{
					byte index;
					if (!GetColorInt(vals[0], out index)) return;

					ColorO = _AcCm.Color.FromColorIndex(_AcCm.ColorMethod.ByColor, index);
				}
				else
				{
					// rgb
					byte rIndex, gIndex, bIndex;
					if (!GetColorInt(vals[0], out rIndex)) return;
					if (!GetColorInt(vals[1], out gIndex)) return;
					if (!GetColorInt(vals[2], out bIndex)) return;

					ColorO = _AcCm.Color.FromRgb(rIndex, gIndex, bIndex);
				}
			}

			private bool GetColorInt(string val, out byte index)
			{
				if (!byte.TryParse(val, out index))
				{
					_errors = _errors + string.Format(CultureInfo.CurrentCulture, "\nUngültiger Wert für Farbe für Layer '{0}': {1}", OldLayer, _color);
					return false;
				}
				if (index > 256)
				{
					_errors = _errors + string.Format(CultureInfo.CurrentCulture, "\nUngültiger Wert für Farbe für Layer '{0}': {1}", OldLayer, _color);
					return false;
				}
				return true;
			}

			protected string ColorToString()
			{
				if (ColorO.IsByAci)
				{
					return ColorO.ColorIndex.ToString(CultureInfo.InvariantCulture);
				}
				else
				{
					return string.Format(CultureInfo.InvariantCulture, "{0}/{1}/{2}",
						ColorO.Red.ToString(CultureInfo.InvariantCulture),
						ColorO.Green.ToString(CultureInfo.InvariantCulture),
						ColorO.Blue.ToString(CultureInfo.InvariantCulture));
				}
			}
			#endregion

		}

		private class LayerInfoWithNrElements : LayerInfo
		{
			#region Lifecycle

			public LayerInfoWithNrElements(_AcDb.LayerTableRecord ltr, _AcDb.Transaction trans, int nrElements)
			{
				OldLayer = ltr.Name;
				NewLayer = "";
				ColorO = ltr.Color;
				Color = ColorToString();
				LineTypeO = ltr.LinetypeObjectId;
				_LineType = GetNameFromLinetypeOid(ltr.LinetypeObjectId, trans);
				LineWeightO = ltr.LineWeight;
				LineWeight = LineWeightToString();
				TransparencyO = ltr.Transparency;
				if (TransparencyO != default(_AcCm.Transparency))
				{
					Transparency = AlphaToTransparenz(TransparencyO.Alpha).ToString(CultureInfo.InvariantCulture);
				}
				else
				{
					Transparency = string.Empty;
				}
				Plot = ltr.IsPlottable ? "Ja" : "Nein";

				Description = ltr.Description;

				NrElements = nrElements;
			}
			#endregion

			#region Properties

			private int NrElements { get; set; }

			#endregion

			#region Internal
			internal override List<string> RowAsList()
			{
				return new List<string>() { OldLayer, NrElements.ToString(CultureInfo.InvariantCulture), Color, LineType, LineWeight, Transparency, Plot, Description };
			}

			#endregion
		}

		private static string GetCellBez(int rowIndex, int colIndex)
		{
			return TranslateColumnIndexToName(colIndex) + (rowIndex + 1).ToString(CultureInfo.InvariantCulture);
		}

		private static String TranslateColumnIndexToName(int index)
		{
			//assert (index >= 0);

			int quotient = (index) / 26;

			if (quotient > 0)
			{
				return TranslateColumnIndexToName(quotient - 1) + (char)((index % 26) + 65);
			}
			else
			{
				return "" + (char)((index % 26) + 65);
			}


		}

		private static _AcDb.ObjectId GetLinetypeFromName(string name, _AcDb.Transaction trans, _AcDb.Database db)
		{
			var acLinTbl = (_AcDb.LinetypeTable)trans.GetObject(db.LinetypeTableId,
				_AcDb.OpenMode.ForRead);

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


	}
}
