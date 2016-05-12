using NUnit.Framework;
using UnitTestCS;

namespace Tests
{
    [TestFixture]
    public class UtilitiesTests
    {
        [Test]
        public void NormalizeData_AcceptsGoldenPathData()
        {
            string json = "{\"Name\": \"Maria\", \"PersonalIdentifier\": 2111858}";
            NormalizedData norm = Utilities.NormalizeData(json);
            Assert.AreEqual(norm.Name, "Maria");
            Assert.AreEqual(norm.Id, 2111858);            
        }
    }
}
