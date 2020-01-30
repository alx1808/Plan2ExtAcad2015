#if BRX_APP
using Application = Bricscad.ApplicationServices.Application;
using Teigha.DatabaseServices;
#elif ARX_APP
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Autodesk.AutoCAD.DatabaseServices;
#endif
using System;
// ReSharper disable IdentifierTypo

namespace Plan2Ext.Raumnummern
{
    internal class HatchColorServer
    {
        private const string EntryName = "RaumnummernHatchColorIndex";
		private readonly int[] _hatchColors = {
			71, 11, 92, 140, 50, 62, 221, 171, 73, 141, 70, 31, 123, 53, 133, 26, 41, 101, 72, 61, 40, 113, 221, 21, 111
		};

		private int GetCurrentHatchColorIndex()
        {
            try
            {
                var db = Application.DocumentManager.MdiActiveDocument.Database;
                var rb = DocumentData.Load(EntryName, db);
                if (rb == null)
                {
                    return 0;
                }
                else
                {
                    var index = Convert.ToInt32(rb.AsArray()[0].Value);
                    if (!IsValid(index)) index = 0;
                    return index;
                }
            }
            catch (Exception)
            {
                return 0;
            }
        }

        private void SetCurrentHatchColorIndex(int index)
        {
            try
            {
                var db = Application.DocumentManager.MdiActiveDocument.Database;
                var newrb = new ResultBuffer() { new TypedValue((int)DxfCode.Int32, index) };
                DocumentData.Save(EntryName, newrb,db);
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private bool IsValid(int index)
        {
            return index < _hatchColors.Length && index >= 0;
        }

        public int CurrentHatchColor
        {
            get
            {
                return _hatchColors[GetCurrentHatchColorIndex()];
            }
        }

        public void IncrementHatchColor()
        {
            var index = GetCurrentHatchColorIndex();
            index++;
            if (!IsValid(index))
            {
                index = 0;
            }
            SetCurrentHatchColorIndex(index);
        }
    }
}
