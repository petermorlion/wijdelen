using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using Orchard.Security;
using Orchard.Users.Services;
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

            var service = new UserImportService(memberShipService.Object, Mock.Of<IUserService>());

            var result = service.ImportUsers(users);

            Assert.AreEqual("john.doe", createUserParams.Username);
            Assert.AreEqual("john.doe@example.com", createUserParams.Email);
            Assert.IsTrue(createUserParams.IsApproved);

            Assert.AreEqual(1, result.Count);
            Assert.IsTrue(result[0].WasImported);
            Assert.AreEqual("john.doe", result[0].UserName);
        }

        [Test]
        public void TestWithInvalidEmail()
        {
            var users = new List<User> {
                new User { UserName = "john.doe", Email = "john.doe" }
            };

            var memberShipService = new Mock<IMembershipService>();
            
            var service = new UserImportService(memberShipService.Object, Mock.Of<IUserService>());

            var result = service.ImportUsers(users);

            memberShipService.Verify(x => x.CreateUser(It.IsAny<CreateUserParams>()), Times.Never);

            Assert.AreEqual(1, result.Count);
            Assert.IsTrue(!result[0].WasImported);
            Assert.AreEqual("john.doe", result[0].UserName);
            Assert.AreEqual("User john.doe has an invalid email address.", result[0].ErrorMessages.Single());
        }

        [Test]
        public void TestWithDuplicateUserName()
        {
            var users = new List<User> {
                new User { UserName = "john.doe", Email = "john.doe@example.com" }
            };

            var memberShipService = new Mock<IMembershipService>();

            var userService = new Mock<IUserService>();
            userService.Setup(x => x.VerifyUserUnicity("john.doe", "john.doe@example.com")).Returns(false);
            
            var service = new UserImportService(memberShipService.Object, userService.Object);

            var result = service.ImportUsers(users);

            memberShipService.Verify(x => x.CreateUser(It.IsAny<CreateUserParams>()), Times.Never);

            Assert.AreEqual(1, result.Count);
            Assert.IsTrue(!result[0].WasImported);
            Assert.AreEqual("john.doe", result[0].UserName);
            Assert.AreEqual("There is already a user with username john.doe and/or email john.doe@example.com.", result[0].ErrorMessages.Single());
        }
    }
}