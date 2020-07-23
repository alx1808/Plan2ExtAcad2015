
using NUnit.Framework;
using Plan2Ext.Raumnummern.ExcelExport;

namespace Plan2Ext.Tests.Raumnummern.ExcelExport
{
    [TestFixture]
    public class TopNrComparerTests
    {
        [TestCase("", "", 0)]
        [TestCase("Top1/1", "Top1/2", -1)]
        [TestCase("Top1/2", "Top1/1", 1)]
        [TestCase("Abc/1", "Top1/1", -1)]
        public void Compare(string a, string b, int expected)
        {
            var comparer = new TopNrComparer();

            var result = comparer.Compare(a, b);

            switch (expected)
            {
                case 0:
                    Assert.That(result, Is.EqualTo(0));
                    break;
                case -1:
                    Assert.That(result, Is.LessThan(0));
                    break;
                default:
                    Assert.That(result, Is.GreaterThan(0));
                    break;
            }
        }

        
    }
}

