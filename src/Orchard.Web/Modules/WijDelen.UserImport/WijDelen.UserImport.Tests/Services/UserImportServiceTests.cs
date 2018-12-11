using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Orchard.ContentManagement;
using Orchard.Security;
using Orchard.Users.Services;
using WijDelen.UserImport.Models;
using WijDelen.UserImport.Services;
using WijDelen.UserImport.Tests.Mocks;

namespace WijDelen.UserImport.Tests.Services {
    [TestFixture]
    public class UserImportServiceTests {
        [Test]
        public void TestWithValidUser() {
            var users = new List<string> {
                "john.doe@example.com"
            };

            CreateUserParams createUserParams = null;
            var memberShipService = new Mock<IMembershipService>();
            memberShipService
                .Setup(x => x.CreateUser(It.IsAny<CreateUserParams>()))
                .Callback((CreateUserParams x) => createUserParams = x)
                .Returns(new UserMockFactory().Create("", "", "", "", "", GroupMembershipStatus.Pending));

            var userService = new Mock<IUserService>();
            userService.Setup(x => x.VerifyUserUnicity("john.doe@example.com", "john.doe@example.com")).Returns(true);

            var service = new UserImportService(memberShipService.Object, userService.Object);

            var result = service.ImportUsers("fr", users);

            createUserParams.Username.Should().Be("john.doe@example.com");
            createUserParams.Email.Should().Be("john.doe@example.com");
            createUserParams.IsApproved.Should().BeTrue();

            result.Count.Should().Be(1);
            result[0].WasImported.Should().BeTrue();
            result[0].Email.Should().Be("john.doe@example.com");
            result[0].User.As<UserDetailsPart>().Culture.Should().Be("fr");
        }

        [Test]
        public void TestWithCapitalizedEmail()
        {
            var users = new List<string> {
                "John.Doe@Example.Com"
            };

            CreateUserParams createUserParams = null;
            var memberShipService = new Mock<IMembershipService>();
            memberShipService
                .Setup(x => x.CreateUser(It.IsAny<CreateUserParams>()))
                .Callback((CreateUserParams x) => createUserParams = x)
                .Returns(new UserMockFactory().Create("", "", "", "", "", GroupMembershipStatus.Pending));

            var userService = new Mock<IUserService>();
            userService.Setup(x => x.VerifyUserUnicity("John.Doe@Example.Com", "John.Doe@Example.Com")).Returns(true);

            var service = new UserImportService(memberShipService.Object, userService.Object);

            var result = service.ImportUsers("fr", users);

            createUserParams.Username.Should().Be("John.Doe@Example.Com");
            createUserParams.Email.Should().Be("John.Doe@Example.Com");
            createUserParams.IsApproved.Should().BeTrue();

            result.Count.Should().Be(1);
            result[0].WasImported.Should().BeTrue();
            result[0].Email.Should().Be("John.Doe@Example.Com");
            result[0].User.As<UserDetailsPart>().Culture.Should().Be("fr");
        }

        [Test]
        public void TestWithInvalidEmail()
        {
            var users = new List<string> {
                "john.doe"
            };

            var memberShipService = new Mock<IMembershipService>();

            var userService = new Mock<IUserService>();
            userService.Setup(x => x.VerifyUserUnicity("john.doe", "john.doe")).Returns(true);

            var service = new UserImportService(memberShipService.Object, userService.Object);

            var result = service.ImportUsers("fr", users);

            memberShipService.Verify(x => x.CreateUser(It.IsAny<CreateUserParams>()), Times.Never);

            Assert.AreEqual(1, result.Count);
            Assert.IsTrue(!result[0].WasImported);
            Assert.AreEqual("john.doe", result[0].Email);
            Assert.AreEqual("john.doe is an invalid email address.", result[0].ErrorMessages.Single());
        }

        [Test]
        public void TestWithDuplicateUserName()
        {
            var users = new List<string> {
                "john.doe@example.com"
            };

            var memberShipService = new Mock<IMembershipService>();

            var userService = new Mock<IUserService>();
            userService.Setup(x => x.VerifyUserUnicity("john.doe@example.com", "john.doe@example.com")).Returns(false);
            
            var service = new UserImportService(memberShipService.Object, userService.Object);

            var result = service.ImportUsers("fr", users);

            memberShipService.Verify(x => x.CreateUser(It.IsAny<CreateUserParams>()), Times.Never);

            Assert.AreEqual(1, result.Count);
            Assert.IsTrue(!result[0].WasImported);
            Assert.AreEqual("john.doe@example.com", result[0].Email);
            Assert.AreEqual("User john.doe@example.com already exists.", result[0].ErrorMessages.Single());
        }
    }
}