using System.IO;
using System.Text;
using NUnit.Framework;
using WijDelen.UserImport.Services;

namespace WijDelen.UserImport.Tests.Services {
    [TestFixture]
    public class CsvReaderTests {
        [Test]
        public void TestValidStream() {
            var reader = new CsvReader();

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("john.doe@example.com");
            stringBuilder.AppendLine("jane.doe@example.com");
            var stringInMemoryStream = new MemoryStream(Encoding.Default.GetBytes(stringBuilder.ToString()));
            var result = reader.ReadUsers(stringInMemoryStream);

            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("john.doe@example.com", result[0].UserName);
            Assert.AreEqual("john.doe@example.com", result[0].Email);
            Assert.AreEqual("jane.doe@example.com", result[1].UserName);
            Assert.AreEqual("jane.doe@example.com", result[1].Email);
        }

        [Test]
        public void TestWithEmptyLine()
        {
            var reader = new CsvReader();

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("");
            stringBuilder.AppendLine("jane.doe@example.com");
            var stringInMemoryStream = new MemoryStream(Encoding.Default.GetBytes(stringBuilder.ToString()));
            var result = reader.ReadUsers(stringInMemoryStream);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("jane.doe@example.com", result[0].UserName);
            Assert.AreEqual("jane.doe@example.com", result[0].Email);
        }
    }
}