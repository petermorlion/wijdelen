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
                new ObjectRequestMailRecord {
                    AggregateId = Guid.NewGuid(),
                    ReceivingUserId = 22,
                    SentDateTime = new DateTime(2016, 11, 27)
                },
                new ObjectRequestMailRecord {
                    AggregateId = Guid.NewGuid(),
                    ReceivingUserId = 22,
                    SentDateTime = new DateTime(2016, 12, 27)
                },
                new ObjectRequestMailRecord {
                    AggregateId = Guid.NewGuid(),
                    ReceivingUserId = 23
                }
            };

            var mailRepositoryMock = new Mock<IRepository<ObjectRequestMailRecord>>();
            mailRepositoryMock.SetRecords(persistentRecords);



            var controller = new ReceivedObjectRequestController(mailRepositoryMock.Object, services);

            var actionResult = controller.Index();

            var model = ((ViewResult)actionResult).Model as IEnumerable<ObjectRequestMailRecord>;
            model.Should().NotBeNull();
            model.Count().Should().Be(2);
            model.ToList()[0].Should().Be(persistentRecords[1]);
            model.ToList()[1].Should().Be(persistentRecords[0]);
        }
    }
}