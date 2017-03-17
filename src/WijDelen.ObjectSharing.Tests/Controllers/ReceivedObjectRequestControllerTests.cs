using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Orchard.Data;
using Orchard.Security;
using WijDelen.ObjectSharing.Controllers;
using WijDelen.ObjectSharing.Infrastructure.Queries;
using WijDelen.ObjectSharing.Models;
using WijDelen.ObjectSharing.Tests.TestInfrastructure.Factories;
using WijDelen.ObjectSharing.Tests.TestInfrastructure.Fakes;
using WijDelen.ObjectSharing.ViewModels;

namespace WijDelen.ObjectSharing.Tests.Controllers {
    [TestFixture]
    public class ReceivedObjectRequestControllerTests {
        [Test]
        public void WhenGettingReceived_ShouldReturnView()
        {
            var userMock = new Mock<IUser>();
            userMock.Setup(x => x.Id).Returns(22);
            var services = new FakeOrchardServices();
            services.WorkContext.CurrentUser = userMock.Object;

            var fakeUserFactory = new UserFactory();
            var user1 = fakeUserFactory.Create("jane.doe", "jane.doe@example.com", "Jane", "Doe");
            var user2 = fakeUserFactory.Create("john.doe", "john.doe@example.com", "John", "Doe");

            var persistentRecords = new[] {
                new ReceivedObjectRequestRecord {
                    ObjectRequestId = Guid.NewGuid(),
                    Description = "Flaming Moe",
                    ExtraInfo = "For drinking",
                    UserId = 22,
                    ReceivedDateTime = new DateTime(2016, 11, 27),
                    RequestingUserId = user1.Id
                },
                new ReceivedObjectRequestRecord {
                    ObjectRequestId = Guid.NewGuid(),
                    Description = "Sneakers",
                    ExtraInfo = "For sneaking",
                    UserId = 22,
                    ReceivedDateTime = new DateTime(2016, 12, 27),
                    RequestingUserId = user2.Id
                },
                new ReceivedObjectRequestRecord {
                    UserId = 23
                }
            };

            var queryMock = new Mock<IFindUsersByIdsQuery>();
            queryMock.Setup(x => x.GetResult(user2.Id, user1.Id)).Returns(new List<IUser> {user1, user2});

            var repositoryMock = new Mock<IRepository<ReceivedObjectRequestRecord>>();
            repositoryMock.SetRecords(persistentRecords);

            var controller = new ReceivedObjectRequestController(repositoryMock.Object, queryMock.Object, services);

            var actionResult = controller.Index();

            var model = ((ViewResult)actionResult).Model as IEnumerable<ReceivedObjectRequestViewModel>;
            model.Should().NotBeNull();
            model.Count().Should().Be(2);

            model.ToList()[0].ShouldBeEquivalentTo(new ReceivedObjectRequestViewModel {
                ObjectRequestId = persistentRecords[1].ObjectRequestId,
                Description = "Sneakers",
                ExtraInfo = "For sneaking",
                ReceivedDateTime = new DateTime(2016, 12, 27),
                UserName = "John Doe"
            });

            model.ToList()[1].ShouldBeEquivalentTo(new ReceivedObjectRequestViewModel
            {
                ObjectRequestId = persistentRecords[0].ObjectRequestId,
                Description = "Flaming Moe",
                ExtraInfo = "For drinking",
                ReceivedDateTime = new DateTime(2016, 11, 27),
                UserName = "Jane Doe"
            });
        }
    }
}