#if BRX_APP
using Bricscad.ApplicationServices;
using Bricscad.Runtime;
using Teigha.DatabaseServices;
using Teigha.Runtime;
#elif ARX_APP
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.ApplicationServices;
#endif
using System;

namespace Plan2Ext
{
	public class DocumentData
	{
		[LispFunction("Plan2_SaveValueInDocument")]
		// ReSharper disable once UnusedMember.Global
		public bool Plan2_SaveValueInDocument(ResultBuffer rb)
		{
			if (rb == null) return false;

			var arr = rb.AsArray();
			if (arr.Length < 2) return false;

			var name = arr[0].Value.ToString();
			var val = arr[1].Value;

			if (val is string)
			{
				var newrb = new ResultBuffer() {new TypedValue((int) DxfCode.Text, val.ToString())};
				var a = newrb.AsArray();

				Save(name, newrb,
					// ReSharper disable once AccessToStaticMemberViaDerivedType
					Application.DocumentManager.MdiActiveDocument.Database);
				return true;
			}

			if (val is double)
			{
				Save(name, new ResultBuffer() { new TypedValue((int)DxfCode.Real, val) },
					// ReSharper disable once AccessToStaticMemberViaDerivedType
					Application.DocumentManager.MdiActiveDocument.Database);
				return true;
			}

			if (val is Int32)
			{
				Save(name, new ResultBuffer() { new TypedValue((int)DxfCode.Int32, (Int32)val) },
					// ReSharper disable once AccessToStaticMemberViaDerivedType
					Application.DocumentManager.MdiActiveDocument.Database);
				return true;
			}


			return false;
		}

		[LispFunction("Plan2_GetValueInDocument")]
		// ReSharper disable once UnusedMember.Global
		public object Plan2_GetValueInDocument(ResultBuffer rb)
		{
			if (rb == null) return null;
			var arr = rb.AsArray();
			if (arr.Length < 1) return null;
			var name = arr[0].Value.ToString();
			// ReSharper disable once AccessToStaticMemberViaDerivedType
			var result = Load(name, Application.DocumentManager.MdiActiveDocument.Database);
			if (result == null) return null;
			arr = result.AsArray();
			if (arr.Length == 0) return null;
			return arr[0].Value;
		}

		internal static ResultBuffer Load(string name, Database db)
		{
			ResultBuffer rb = null;
			using (var trans = db.TransactionManager.StartTransaction())
			{
				var nod = (DBDictionary)trans.GetObject(db.NamedObjectsDictionaryId, OpenMode.ForRead);
				if (nod.Contains(name))
				{
					var xrecord = (Xrecord)trans.GetObject(nod.GetAt(name), OpenMode.ForWrite);
					rb = xrecord.Data;
				}
				trans.Commit();
			}

			return rb;
		}


		internal static void Save(string name, ResultBuffer rb, Database db)
		{
			using (var trans = db.TransactionManager.StartTransaction())
			{
				var nod = (DBDictionary)trans.GetObject(db.NamedObjectsDictionaryId, OpenMode.ForWrite);
				Xrecord xrecord = null;
				if (nod.Contains(name))
				{
					xrecord = (Xrecord)trans.GetObject(nod.GetAt(name), OpenMode.ForWrite);
				}

				if (xrecord == null)
				{
					var a = rb.AsArray();

					xrecord = new Xrecord();
					// dxfcode not lispcode
					xrecord.Data = rb;

					nod.SetAt(name, xrecord);
					trans.AddNewlyCreatedDBObject(xrecord, true);
				}
				else
				{
					xrecord.Data = rb;
				}
				trans.Commit();
			}
		}
	}
}
