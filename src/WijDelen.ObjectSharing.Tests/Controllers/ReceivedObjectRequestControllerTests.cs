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
using WijDelen.ObjectSharing.Models;
using WijDelen.ObjectSharing.Tests.TestInfrastructure.Fakes;

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

            var persistentRecords = new[] {
                new ReceivedObjectRequestRecord {
                    UserId = 22,
                    ReceivedDateTime = new DateTime(2016, 11, 27)
                },
                new ReceivedObjectRequestRecord {
                    UserId = 22,
                    ReceivedDateTime = new DateTime(2016, 12, 27)
                },
                new ReceivedObjectRequestRecord {
                    UserId = 23
                }
            };

            var repositoryMock = new Mock<IRepository<ReceivedObjectRequestRecord>>();
            repositoryMock.SetRecords(persistentRecords);

            var controller = new ReceivedObjectRequestController(repositoryMock.Object, services);

            var actionResult = controller.Index();

            var model = ((ViewResult)actionResult).Model as IEnumerable<ReceivedObjectRequestRecord>;
            model.Should().NotBeNull();
            model.Count().Should().Be(2);
            model.ToList()[0].Should().Be(persistentRecords[1]);
            model.ToList()[1].Should().Be(persistentRecords[0]);
        }
    }
}