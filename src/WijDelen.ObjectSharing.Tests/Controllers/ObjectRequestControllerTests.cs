using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Security;
using WijDelen.ObjectSharing.Controllers;
using WijDelen.ObjectSharing.Domain.Commands;
using WijDelen.ObjectSharing.Domain.Messaging;
using WijDelen.ObjectSharing.Models;
using WijDelen.ObjectSharing.Tests.TestInfrastructure.Fakes;
using WijDelen.ObjectSharing.ViewModels;

namespace WijDelen.ObjectSharing.Tests.Controllers {
    [TestFixture]
    public class ObjectRequestControllerTests {
        /// <summary>
        /// Verifies that T can be set (not having a setter will not cause a compile-time exception, but it will cause a runtime exception.
        /// </summary>
        [Test]
        public void TestT() {
            var controller = new ObjectRequestController(null, null, null, null);
            var localizer = NullLocalizer.Instance;

            controller.T = localizer;

            Assert.AreEqual(localizer, controller.T);
        }

        [Test]
        public void ShouldValidateNewObjectRequest() {
            var controller = new ObjectRequestController(null, null, null, null);
            var viewModel = new NewObjectRequestViewModel();

            var viewResult = controller.New(viewModel);

            ((ViewResult) viewResult).ViewData.ModelState["Description"].Errors.Single().ErrorMessage.Should().Be("Please provide a description of the item you need.");
            ((ViewResult) viewResult).ViewData.ModelState["ExtraInfo"].Errors.Single().ErrorMessage.Should().Be("Please provide some extra info.");
        }

        [Test]
        public void WhenPosting_ShouldCallCommandHandlerAndRedirect() {
            var commandHandlerMock = new Mock<ICommandHandler<RequestObject>>();
            RequestObject command = null;
            commandHandlerMock.Setup(x => x.Handle(It.IsAny<RequestObject>())).Callback((RequestObject c) => command = c);

            var userMock = new Mock<IUser>();
            userMock.Setup(x => x.Id).Returns(22);
            var services = new FakeOrchardServices();
            services.WorkContext.CurrentUser = userMock.Object;

            var controller = new ObjectRequestController(commandHandlerMock.Object, null, null, services);
            var viewModel = new NewObjectRequestViewModel {
                Description = "Sneakers",
                ExtraInfo = "For sneaking"
            };

            var actionResult = controller.New(viewModel);

            ((RedirectToRouteResult) actionResult).RouteValues["action"].Should().Be("Item");
            ((RedirectToRouteResult) actionResult).RouteValues["id"].Should().Be(command.ObjectRequestId);

            command.Description.Should().Be("Sneakers");
            command.ExtraInfo.Should().Be("For sneaking");
            command.UserId.Should().Be(22);
        }

        [Test]
        public void WhenGettingItemForWrongUser_ShouldReturnUnauthorized() {
            var id = Guid.NewGuid();

            var userMock = new Mock<IUser>();
            userMock.Setup(x => x.Id).Returns(22);
            var services = new FakeOrchardServices();
            services.WorkContext.CurrentUser = userMock.Object;

            var persistentRecords = new[] {
                new ObjectRequestRecord {
                    AggregateId = id,
                    UserId = 10
                }
            };

            var objectRequestRepositoryMock = new Mock<IRepository<ObjectRequestRecord>>();
            objectRequestRepositoryMock.SetRecords(persistentRecords);

            var chatRepositoryMock = new Mock<IRepository<ChatRecord>>();

            var controller = new ObjectRequestController(null, objectRequestRepositoryMock.Object, chatRepositoryMock.Object, services);

            var actionResult = controller.Item(id);

            actionResult.Should().BeOfType<HttpUnauthorizedResult>();
        }

        [Test]
        public void WhenGettingItem_ShouldReturnView() {
            var id = Guid.NewGuid();

            var userMock = new Mock<IUser>();
            userMock.Setup(x => x.Id).Returns(22);
            var services = new FakeOrchardServices();
            services.WorkContext.CurrentUser = userMock.Object;

            var objectRequestRecords = new[] {
                new ObjectRequestRecord {
                    AggregateId = id,
                    UserId = 22
                },
                new ObjectRequestRecord {
                    AggregateId = Guid.NewGuid(),
                    UserId = 22
                }
            };

            var objectRequestRepositoryMock = new Mock<IRepository<ObjectRequestRecord>>();
            objectRequestRepositoryMock.SetRecords(objectRequestRecords);

            var chatRecords = new[] {
                new ChatRecord {
                    ObjectRequestId = id
                },
                new ChatRecord {
                    ObjectRequestId = Guid.NewGuid()
                }
            };

            var chatRepositoryMock = new Mock<IRepository<ChatRecord>>();
            chatRepositoryMock.SetRecords(chatRecords);

            var controller = new ObjectRequestController(null, objectRequestRepositoryMock.Object, chatRepositoryMock.Object, services);

            var actionResult = controller.Item(id);

            ((ViewResult) actionResult).Model.ShouldBeEquivalentTo(new ObjectRequestViewModel {
                ObjectRequestRecord = objectRequestRecords[0],
                ChatRecords = new List<ChatRecord> { chatRecords[0] }
            });
        }

        [Test]
        public void WhenGettingItemForUnknownId_ShouldReturnNotFound() {
            var id = Guid.NewGuid();

            var persistentRecords = new ObjectRequestRecord[0];

            var objectRequestRepositoryMock = new Mock<IRepository<ObjectRequestRecord>>();
            objectRequestRepositoryMock.SetRecords(persistentRecords);

            var chatRepositoryMock = new Mock<IRepository<ChatRecord>>();

            var controller = new ObjectRequestController(null, objectRequestRepositoryMock.Object, chatRepositoryMock.Object, null);

            var actionResult = controller.Item(id);

            actionResult.Should().BeOfType<HttpNotFoundResult>();
        }

        [Test]
        public void WhenGettingIndex_ShouldReturnView() {
            var userMock = new Mock<IUser>();
            userMock.Setup(x => x.Id).Returns(22);
            var services = new FakeOrchardServices();
            services.WorkContext.CurrentUser = userMock.Object;

            var persistentRecords = new[] {
                new ObjectRequestRecord {
                    AggregateId = Guid.NewGuid(),
                    UserId = 22,
                    CreatedDateTime = new DateTime(2016, 11, 27)
                },
                new ObjectRequestRecord {
                    AggregateId = Guid.NewGuid(),
                    UserId = 22,
                    CreatedDateTime = new DateTime(2016, 12, 27)
                },
                new ObjectRequestRecord {
                    AggregateId = Guid.NewGuid(),
                    UserId = 23
                }
            };

            var repositoryMock = new Mock<IRepository<ObjectRequestRecord>>();
            repositoryMock.SetRecords(persistentRecords);

            var controller = new ObjectRequestController(null, repositoryMock.Object, null, services);

            var actionResult = controller.Index();

            var model = ((ViewResult) actionResult).Model as IEnumerable<ObjectRequestRecord>;
            model.Should().NotBeNull();
            model.Count().Should().Be(2);
            model.ToList()[0].Should().Be(persistentRecords[1]);
            model.ToList()[1].Should().Be(persistentRecords[0]);
        }
    }
}