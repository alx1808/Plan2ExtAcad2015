using System.Globalization;
using Autodesk.AutoCAD.EditorInput;

// ReSharper disable IdentifierTypo
// ReSharper disable StringLiteralTypo

namespace Plan2Ext.BlockInfo
{
    internal interface IGetsFromUser
    {
        void GetNrOfVerticalBlockElements(Editor ed, ref int nrOfVerticalBlockElements);
        void GetScaleFactor(Editor ed, ref double scaleFactor);
    }

    internal class GetsFromUser : IGetsFromUser
    {
        public void GetNrOfVerticalBlockElements(Editor ed, ref int nrOfVerticalBlockElements)
        {
            var resultInteger = ed.GetInteger(new PromptIntegerOptions(string.Format(CultureInfo.CurrentCulture,
                "\nAnzahl vertikaler Blockelemente in Legende <{0}>:",  nrOfVerticalBlockElements))
            {
                AllowNegative = false,
                AllowArbitraryInput = false,
                AllowZero = false,
                AllowNone = true,
            });
            if (resultInteger.Status == PromptStatus.OK) nrOfVerticalBlockElements = resultInteger.Value;
        }

        public void GetScaleFactor(Editor ed, ref double scaleFactor)
        {
            var resultDouble = ed.GetDouble(new PromptDoubleOptions(string.Format(CultureInfo.CurrentCulture,
                "\nSkalierfaktor <{0}>:", scaleFactor))
            {
                AllowNegative = false,
                AllowZero = false,
                AllowArbitraryInput = false,
                AllowNone = true,
            });
            if (resultDouble.Status == PromptStatus.OK) scaleFactor = resultDouble.Value;
        }

    }
}
