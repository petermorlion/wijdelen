using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using Orchard.Security;
using WijDelen.UserImport.Models;
using WijDelen.UserImport.Services;

namespace WijDelen.UserImport.Tests.Services {
    [TestFixture]
    public class UserImportServiceTests {
        [Test]
        public void TestWithValidUser() {
            var users = new List<User> {
                new User { UserName = "john.doe", Email = "john.doe@example.com" }
            };

            CreateUserParams createUserParams = null;
            var memberShipService = new Mock<IMembershipService>();
            memberShipService
                .Setup(x => x.CreateUser(It.IsAny<CreateUserParams>()))
                .Callback((CreateUserParams x) => createUserParams = x);

            var service = new UserImportService(memberShipService.Object);

            var result = service.ImportUsers(users);

            Assert.AreEqual("john.doe", createUserParams.Username);
            Assert.AreEqual("john.doe@example.com", createUserParams.Email);
            Assert.IsTrue(createUserParams.IsApproved);

            Assert.AreEqual(1, result.Count);
            Assert.IsTrue(result[0].IsValid);
            Assert.AreEqual("john.doe", result[0].UserName);
        }
    }
}