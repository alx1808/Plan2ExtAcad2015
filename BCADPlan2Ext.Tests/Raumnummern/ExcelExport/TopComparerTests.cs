
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using Plan2Ext.Raumnummern.ExcelExport;

namespace Plan2Ext.Tests.Raumnummern.ExcelExport
{
    [TestFixture]
    public class TopComparerTests
    {
        [TestCase("","",0)]
        [TestCase("Top 123", "Top 123", 0)]
        [TestCase("Top 123", "Top 124", -1)]
        [TestCase("Top 123+124+125", "Top 123", 1)]
        [TestCase("Top 123", "Top 123+124+125",  -1)]
        [TestCase("Top 123", "123", 1)]
        [TestCase("Top 123", "124", -1)]
        public void Compare(string a, string b, int expected)
        {
            var comparer = new TopComparer();
            
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

