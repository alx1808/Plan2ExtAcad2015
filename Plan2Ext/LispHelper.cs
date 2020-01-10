#if BRX_APP
using _AcAp = Bricscad.ApplicationServices;
using _AcCm = Teigha.Colors;
using Teigha.DatabaseServices;
using Teigha.Runtime;
using Bricscad.ApplicationServices;
#elif ARX_APP
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;


namespace Plan2Ext
{
    public static class LispHelper
    {
#if BRX_APP
		internal static IEnumerable<TypedValue> GetLispDict(string dictionaryName, string entryName)
		{
			return new TypedValue[0];
		}
#elif ARX_APP
        [DllImport("accore.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr acdbEntGet(AdsName objName);
        [DllImport("accore.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr acdbEntGetX(AdsName objName, IntPtr app);

        [DllImport("acdb20.dll",
            CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "?acdbGetAdsName@@YA?AW4ErrorStatus@Acad@@AEAY01_JVAcDbObjectId@@@Z")]
        static extern ErrorStatus acdbGetAdsName64_2015(out AdsName objName, ObjectId id);
        [DllImport("acdb21.dll",
            CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "?acdbGetAdsName@@YA?AW4ErrorStatus@Acad@@AEAY01_JVAcDbObjectId@@@Z")]
        static extern ErrorStatus acdbGetAdsName64_2017(out AdsName objName, ObjectId id);
        [DllImport("acdb22.dll",
            CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "?acdbGetAdsName@@YA?AW4ErrorStatus@Acad@@AEAY01_JVAcDbObjectId@@@Z")]
        static extern ErrorStatus acdbGetAdsName64_2018(out AdsName objName, ObjectId id);
        [DllImport("acdb23.dll",
            CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "?acdbGetAdsName@@YA?AW4ErrorStatus@Acad@@AEAY01_JVAcDbObjectId@@@Z")]
        static extern ErrorStatus acdbGetAdsName64_2019(out AdsName objName, ObjectId id);


        private static void GetAdsNAme(out AdsName objName, ObjectId id)
        {
            var acadver = Application.GetSystemVariable("ACADVER").ToString();
            if (acadver.StartsWith("20."))
            {
                acdbGetAdsName64_2015(out objName, id);
                return;
            }
            if (acadver.StartsWith("21."))
            {
                acdbGetAdsName64_2017(out objName, id);
                return;
            }
            if (acadver.StartsWith("22."))
            {
                acdbGetAdsName64_2018(out objName, id);
                return;
            }
            if (acadver.StartsWith("23."))
            {
                acdbGetAdsName64_2019(out objName, id);
                return;
            }

            objName = default(AdsName);
        }

        internal static IEnumerable<TypedValue> GetLispDict(string dictionaryName, string entryName)
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            var arr = new TypedValue[0];
            if (doc == null) return arr;
            using (var trans = doc.TransactionManager.StartTransaction())
            {
                var dbDictionary = (DBDictionary)trans.GetObject(doc.Database.NamedObjectsDictionaryId, OpenMode.ForRead);
                if (dbDictionary != null)
                {
                    if (dbDictionary.Contains(dictionaryName))
                    {
                        var dictObjectId = dbDictionary.GetAt(dictionaryName);
                        var dict = (DBDictionary)trans.GetObject(dictObjectId, OpenMode.ForRead);
                        if (dict.Contains(entryName))
                        {
                            var hObjectId = dict.GetAt(entryName);
                            var impDbObjectAsDbObject = (DBObject)trans.GetObject(
                                hObjectId,
                                OpenMode.ForRead);
                            var resultBuffer = new ResultBuffer();
                            var eName = new AdsName();
                            GetAdsNAme(out eName, impDbObjectAsDbObject.ObjectId);
                            if (!eName.Equals(default(AdsName)))
                            {
                                var result = acdbEntGetX(eName, resultBuffer.UnmanagedObject);
                                if (result != IntPtr.Zero)
                                {
                                    var rb = ResultBuffer.Create(result, true);
                                    arr = rb.AsArray();
                                }
                            }
                        }
                    }
                }

                trans.Commit();
            }

            return arr;
        }

#endif

        internal static string GetConfigFileName()
        {
            var arr = GetLispDict("AA_PLAN2", "ConfigFile").ToArray();
            if (arr.Length >= 12)
            {
                var configFileName = arr[11].Value.ToString();
                configFileName = configFileName.Replace("\"", "");
                return configFileName;
            }
            return null;
        }

        [CommandMethod("Plan2GetConfigFileName")]
        public static void Plan2GetConfigFileName()
        {
            var c = GetConfigFileName();
            if (!string.IsNullOrEmpty(c))
            {
                Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\n" + c);
            }
        }
    }
}
