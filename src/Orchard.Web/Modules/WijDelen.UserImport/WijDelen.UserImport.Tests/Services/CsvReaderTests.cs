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
            stringBuilder.AppendLine("username;email");
            stringBuilder.AppendLine("john.doe;john.doe@example.com");
            stringBuilder.AppendLine("jane.doe;jane.doe@example.com");
            var stringInMemoryStream = new MemoryStream(Encoding.Default.GetBytes(stringBuilder.ToString()));
            var result = reader.ReadUsers(stringInMemoryStream);

            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("john.doe", result[0].UserName);
            Assert.AreEqual("john.doe@example.com", result[0].Email);
            Assert.AreEqual("jane.doe", result[1].UserName);
            Assert.AreEqual("jane.doe@example.com", result[1].Email);
        }
    }
}