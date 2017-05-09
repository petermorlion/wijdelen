using Moq;
using NUnit.Framework;
using Orchard.Security;
using WijDelen.UserImport.Models;

namespace WijDelen.UserImport.Tests.Models {
    [TestFixture]
    public class UserImportResultTests {
        [Test]
        public void TestWasImportedWithUser() {
            var userImportResult = new UserImportResult("email") { User = Mock.Of<IUser>()};
            Assert.IsTrue(userImportResult.WasImported);
        }

        [Test]
        public void TestWasImportedWithoutUser() {
            var userImportResult = new UserImportResult("email");
            Assert.IsFalse(userImportResult.WasImported);
        }
    }
}