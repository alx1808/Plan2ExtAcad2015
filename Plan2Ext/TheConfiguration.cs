// ReSharper disable CommentTypo
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
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
//using _AcIntCom = BricscadDb;
//using _AcInt = BricscadApp;
// ReSharper disable StringLiteralTypo
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

namespace Plan2Ext
{

	internal static class TheConfiguration
	{
		private static string _currentConfig = string.Empty;
		private const int RTNORM = 5100;
		private static readonly Dictionary<string, ConfigVar> ConfigDict = new Dictionary<string, ConfigVar>();
		private static bool _loaded;
		private static Encoding _encoding = Encoding.Default;

		public static event EventHandler ConfigurationChanged;
		static TheConfiguration()
		{
			_AcAp.Application.DocumentManager.DocumentActivated += dc_DocumentActivated;
		}

		static void dc_DocumentActivated(object sender, _AcAp.DocumentCollectionEventArgs e)
		{

			_loaded = false;
			// holen der konfigdatei funkt noch nicht in bricscad ohne lisp
			OnConfigurationChanged();
		}

		public static bool Loaded
		{
			get { return _loaded; }
			set { _loaded = value; }
		}

		public static string GetValueString(string name)
		{
			Type type;
			object value;
			GetValue(name, out type, out value);
			return value.ToString();
		}

		public static bool GetValueString(string varName, out string val)
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

		public static object GetValue(string name)
		{
			if (!_loaded)
			{
				LoadConfig();
			}

			ConfigVar cv;
			if (ConfigDict.TryGetValue(name, out cv))
			{
				object value;
				Type type;
				cv.GetValue(out type, out value);
				return value;
			}
			throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Konfigurationseintrag für Configvar {0} nicht gefunden!", name));
		}

		public static object GetValue(string name, out Type type)
		{
			if (!_loaded)
			{
				LoadConfig();
			}

			ConfigVar cv;
			if (ConfigDict.TryGetValue(name, out cv))
			{
				object value;
				cv.GetValue(out type, out value);
				return value;
			}
			throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Konfigurationseintrag für Configvar {0} nicht gefunden!", name));
		}

		public static bool GetValue(string name, out Type type, out object value)
		{
			if (!_loaded)
			{
				LoadConfig();
			}

			ConfigVar cv;
			if (ConfigDict.TryGetValue(name, out cv))
			{
				cv.GetValue(out type, out value);
				return true;
			}
			throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Konfigurationseintrag für Configvar {0} nicht gefunden!", name));
		}
		private static void LoadConfig()
		{
			GetEncoding();
			GetConfiguration();
			ReadVals();
			_loaded = true;

		}
		private static void ReadVals()
		{
			string[] Lines = File.ReadAllLines(_currentConfig, _encoding);
			for (int i = 0; i < Lines.Length; i++)
			{
				string Line = Lines[i];
				ConfigVar oConfigVar = null;
				try
				{
					oConfigVar = new ConfigVar(Line);

					ConfigDict[oConfigVar.VarName] = oConfigVar;
				}
				catch (InvalidOperationException ex)
				{
					throw new InvalidOperationException(string.Format("Fehler in Konfiguration '{0}', Zeile {1};\n{2}", _currentConfig, i + 1, ex.Message));
				}
			}

		}

		private static void GetEncoding()
		{

			foreach (EncodingInfo ei in Encoding.GetEncodings())
			{
				Encoding e = ei.GetEncoding();
				if (ei.CodePage == 1252) { _encoding = e; return; }
			}
			throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Konnte Encoding für Ansi nicht finden!"));
		}


		private static void GetConfiguration()
		{
			_currentConfig = string.Empty;
			GetConfiguration_NewMechanism();
			if (!string.IsNullOrEmpty(_currentConfig)) return;
#if BRX_APP
			throw new InvalidOperationException("Konnte Konfigurationsdateiname nicht ermitteln!");
#else

			using (var rb = new _AcDb.ResultBuffer(new _AcDb.TypedValue((int)_AcBrx.LispDataType.Text, "c:Plan2CurrentConfig")))
			{
				int stat = 0;
				_AcDb.ResultBuffer res = CADDZone.AutoCAD.Samples.AcedInvokeSample.InvokeLisp(rb, ref stat);
				if (stat == RTNORM && res != null)
				{
					_currentConfig = res.AsArray()[0].Value.ToString();
					res.Dispose();
				}
				else
				{
					// on loading drawing lispcall doesnt work, get lisp dict automatically as last resort
					var c = LispHelper.GetConfigFileName();
					if (string.IsNullOrEmpty(c))
						throw new InvalidOperationException("Konnte Konfigurationsdateiname nicht ermitteln!");
					string configFile;
					if (!Globs.FindFile(c, _AcAp.Application.DocumentManager.MdiActiveDocument.Database, out configFile))
						throw new InvalidOperationException("Konnte Konfigurationsdateiname nicht ermitteln!");
					_currentConfig = configFile;
				}
			}
#endif
		}

		private static void GetConfiguration_NewMechanism()
		{
			try
			{
				var doc = _AcAp.Application.DocumentManager.MdiActiveDocument;
				using (_AcAp.DocumentLock m_doclock = doc.LockDocument())
				{
					var rb = DocumentData.Load("ALX_F_PLAN2_CURRENTCONFIG",doc.Database);
					if (rb != null)
					{
						var arr = rb.AsArray();
						if (arr.Length > 0)
						{
							var c = arr[0].Value.ToString();
							string configFile;
							if (Globs.FindFile(c, doc.Database,
								out configFile))
							{
								_currentConfig = configFile;
							}
						}
					}
				}
			}
			catch (Exception e)
			{
				;
			}

		}

		private static void OnConfigurationChanged()
		{
			var handler = ConfigurationChanged;
			if (handler != null) handler(null, EventArgs.Empty);
		}
	}

	internal class Category
	{
		public string Name { get; set; }
		public List<ConfigVar> ConfigVars = new List<ConfigVar>();
		public Category(string name)
		{
			Name = name;
		}
		public override string ToString()
		{
			return Name;
		}
		public IEnumerable<string> AsLines()
		{
			return ConfigVars.Select(x => x.ToLine());
		}
	}

	internal class ConfigVar
	{
		#region Constants
		public const string TRENN = "|";
		#endregion

		public string Category { get; set; }
		public string Description { get; set; }
		public string VarName { get; set; }
		public string Value { get; set; }
		public string VarType { get; set; }

		public ConfigVar(string line)
		{
			string[] array = line.Split(new string[] { TRENN }, StringSplitOptions.None);
			if (array.Length != 5) throw new InvalidOperationException(string.Format("Ungültige Zeile in Konfiguration: '{0}'!", line));

			Category = array[0];
			Description = array[1];
			VarName = array[2];
			Value = array[3];
			VarType = array[4];
		}


		internal string ToLine()
		{
			return Category + TRENN + Description + TRENN + VarName + TRENN + Value + TRENN + VarType;
		}

		internal void GetValue(out Type type, out object value)
		{
			switch (VarType)
			{
				case "String":
					type = typeof(string);
					value = Value;
					break;
				case "Real":
					type = typeof(double);
					value = double.Parse(Value, CultureInfo.InvariantCulture);
					break;
				case "Int":
					type = typeof(int);
					value = int.Parse(Value, CultureInfo.InvariantCulture);
					break;
				default:
					throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Ungültiger Typ in Configvar {0}\\{1}: '{2}'!", Category, Description, VarType));
			}
		}
	}
}
