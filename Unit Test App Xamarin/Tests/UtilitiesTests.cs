using NUnit.Framework;
using uTestAppX;

namespace Tests
{
    [TestFixture]
    public class UtilitiesTests
    {
        // 1. Golden path
        [Test]
        public void NormalizeData_AcceptsGoldenPathData()
        {
            string json = "{\"Name\": \"Maria\", \"PersonalIdentifier\": 2111858}";
            NormalizedData norm = Utilities.NormalizeData(json);
            Assert.AreEqual("Maria", norm.Name);
            Assert.AreEqual(2111858, norm.Id);
        }

        // 2. Challenge JSON.parse to fail with invalid JSON or other common attack vectors
        [Test]
        public void NormalizeData_RejectsNull()
        {
            string json = null;
            NormalizedData norm = Utilities.NormalizeData(json);
            Assert.AreEqual(null, norm);
        }

        [Test]
        public void NormalizeData_RejectsEmptyString()
        {
            string json = "";
            NormalizedData norm = Utilities.NormalizeData(json);
            Assert.AreEqual(null, norm);
        }

        [Test]
        public void NormalizeData_RejectsNonJsonString()
        {
            string json = "blahblahblah";
            NormalizedData norm = Utilities.NormalizeData(json);
            Assert.AreEqual(null, norm);
        }

        [Test]
        public void NormalizeData_RejectsBadJson1()
        {
            string json = "{{}";
            NormalizedData norm = Utilities.NormalizeData(json);
            Assert.AreEqual(null, norm);
        }

        [Test]
        public void NormalizeData_RejectsBadJson2()
        {
            string json = "{{[]}}}";
            NormalizedData norm = Utilities.NormalizeData(json);
            Assert.AreEqual(null, norm);
        }

        [Test]
        public void NormalizeData_RejectsInjectedHTMLJavaScript()
        {
            string json = "document.location=\"malware.site.com\"";
            NormalizedData norm = Utilities.NormalizeData(json);
            Assert.AreEqual(null, norm);
        }

        [Test]
        public void NormalizeData_RejectsInjectedSQL()
        {
            string json = "drop database users";
            NormalizedData norm = Utilities.NormalizeData(json);
            Assert.AreEqual(null, norm);
        }

        // 3. Challenge assumptions about the JSON structure
        [Test]
        public void NormalizeData_AcceptsNameOnlyIdDefaults()
        {
            string json = "{\"Name\": \"Maria\"}";
            NormalizedData norm = Utilities.NormalizeData(json);
            Assert.AreEqual("Maria", norm.Name);
            Assert.AreEqual(0, norm.Id);  //Default
        }

        [Test]
        public void NormalizeData_AcceptsPersonalIdentifierOnlyNameDefaults()
        {
            string json = "{\"PersonalIdentifier\": 2111858}";
            NormalizedData norm = Utilities.NormalizeData(json);
            Assert.AreEqual("default", norm.Name); //Default
            Assert.AreEqual(2111858, norm.Id);  
        }

        [Test]
        public void NormalizeData_DefaultsWithEmptyJson()
        {
            string json = "{}";
            NormalizedData norm = Utilities.NormalizeData(json);
            Assert.AreEqual("default", norm.Name); //Default
            Assert.AreEqual(0, norm.Id);           //Default
        }

        [Test]
        public void NormalizeData_DefaultsWithUnknownFieldsDifferingByCase()
        {
            string json = "{\"name\": \"Maria\", \"personalIdentifier\": 2111858}";
            NormalizedData norm = Utilities.NormalizeData(json);
            Assert.AreEqual("default", norm.Name); //Default
            Assert.AreEqual(0, norm.Id);           //Default
        }

        [Test]
        public void NormalizeData_DefaultWithUnknownFieldsSimilarProperties()
        {
            string json = "{\"nm\": \"Maria\", \"pid\": 2111858}";
            NormalizedData norm = Utilities.NormalizeData(json);
            Assert.AreEqual("default", norm.Name); //Default
            Assert.AreEqual(0, norm.Id);           //Default
        }

        [Test]
        public void NormalizeData_IgnoresExtraFields()
        {
            string json = "{\"Name\": \"Maria\", \"PersonalIdentifier\": 2111858, \"Other1\": 123, \"Other2\": \"foobar\"}";
            NormalizedData norm = Utilities.NormalizeData(json);
            Assert.AreEqual("Maria", norm.Name);
            Assert.AreEqual(2111858, norm.Id);
        }

        // 4. Variations on data types

        [Test]
        public void NormalizeData_AcceptsStringPersonalIdentifier()
        {
            string json = "{\"Name\": \"Maria\", \"PersonalIdentifier\": \"2111858\"}";
            NormalizedData norm = Utilities.NormalizeData(json);
            Assert.AreEqual("Maria", norm.Name);
            Assert.AreEqual(2111858, norm.Id);
        }

        [Test]
        public void NormalizeData_RejectsNonNumericalStringPersonalIdentifier()
        {
            string json = "{\"Name\": \"Maria\", \"PersonalIdentifier\": \"AA2111858\"}";
            NormalizedData norm = Utilities.NormalizeData(json);
            Assert.AreEqual(null, norm);            
        }

        [Test]
        public void NormalizeData_RejectsNegativePersonalIdentifier()
        {
            string json = "{\"Name\": \"Maria\", \"PersonalIdentifier\": -1}";
            NormalizedData norm = Utilities.NormalizeData(json);
            Assert.AreEqual(null, norm);
        }

        [Test]
        public void NormalizeData_RejectsExcessivelyLargeIntegerPersonalIdentifier()
        {
            string json = "{\"Name\": \"Maria\", \"PersonalIdentifier\": 123456789123456789123456789123456789}";
            NormalizedData norm = Utilities.NormalizeData(json);
            Assert.AreEqual(null, norm);
        }

        [Test]
        public void NormalizeData_AcceptsMaxPersonalIdentifier()
        {
            string json = "{\"Name\": \"Maria\", \"PersonalIdentifier\": 9999999999}";
            NormalizedData norm = Utilities.NormalizeData(json);
            Assert.AreEqual("Maria", norm.Name);
            Assert.AreEqual(9999999999, norm.Id);
        }

        [Test]
        public void NormalizeData_RejectsMaxPlusOnePersonalIdentifier()
        {
            string json = "{\"Name\": \"Maria\", \"PersonalIdentifier\": 10000000000}";
            NormalizedData norm = Utilities.NormalizeData(json);
            Assert.AreEqual(null, norm);
        }

        [Test]
        public void NormalizeData_TruncatesExcessivelyLongName()
        {
            //Create a string longer than 255 characters            
            string name = "";
            for (int i = 0; i < 30; i++)
            {
                name += "aaaaaaaaaa" + i;
            }

            string json = "{\"Name\": \"" + name + "\", \"PersonalIdentifier\": 2111858}";
            NormalizedData norm = Utilities.NormalizeData(json);

            int truncateLength = 255;
            Assert.AreEqual(name.Substring(0, truncateLength), norm.Name);
            Assert.AreEqual(truncateLength, norm.Name.Length);
            Assert.AreEqual(norm.Id, 2111858);
        }

        [Test]
        public void NormalizeData_RejectsIntegerName()
        {
            string json = "{\"Name\": 12345, \"PersonalIdentifier\": 2111858}";
            NormalizedData norm = Utilities.NormalizeData(json);
            Assert.AreEqual(null, norm);
        }

        [Test]
        public void NormalizeData_RejectsObjectName()
        {
            string json = "{\"Name\": {\"First\": \"Maria\"}, \"PersonalIdentifier\": 2111858}";
            NormalizedData norm = Utilities.NormalizeData(json);
            Assert.AreEqual(null, norm);
        }

        [Test] public void NormalizeData_RejectsObjectPersonalIdentifier()
        {
            string json = "{\"Name\": \"Maria\", \"PersonalIdentifier\": {\"id\": 2111858}}";
            NormalizedData norm = Utilities.NormalizeData(json);
        }

        [Test]
        public void NormalizeData_RejectsObjectNameAndPersonalIdentifier()
        {
            string json = "{\"Name\": {\"First\": \"Maria\"}, \"PersonalIdentifier\": {\"id\": 2111858}}";
            NormalizedData norm = Utilities.NormalizeData(json);
        }

        [Test]
        public void NormalizeData_RejectsArrayName()
        {
            string json = "{\"Name\": [\"Maria\"], \"PersonalIdentifier\": 2111858}";
            NormalizedData norm = Utilities.NormalizeData(json);
            Assert.AreEqual(null, norm);
        }

        [Test]
        public void NormalizeData_RejectsArrayPersonalIdentifier()
        {
            string json = "{\"Name\": \"Maria\", \"PersonalIdentifier\": [2111858]}";
            NormalizedData norm = Utilities.NormalizeData(json);
            Assert.AreEqual(null, norm);
        }

        [Test]
        public void NormalizeData_RejectsArrayNameAndPersonalIdentifier()
        {
            string json = "{\"Name\": [\"Maria\"], \"PersonalIdentifier\": [2111858]}";
            NormalizedData norm = Utilities.NormalizeData(json);
            Assert.AreEqual(null, norm);
        }

        [Test]
        public void NormalizeData_AcceptsLeadingZerosInStringPersonalIdentifier()
        {
            string json = "{\"Name\": \"Maria\", \"PersonalIdentifier\": \"002111858\"}";
            NormalizedData norm = Utilities.NormalizeData(json);
            Assert.AreEqual("Maria", norm.Name);
            Assert.AreEqual(2111858, norm.Id);
        }

        [Test]
        public void NormalizeData_RejectsLeadingZerosInIntegerPersonalIdentifier()
        {
            string json = "{\"Name\": \"Maria\", \"PersonalIdentifier\": 002111858}";
            NormalizedData norm = Utilities.NormalizeData(json);
            Assert.AreEqual(null, norm);
        }
    }
}