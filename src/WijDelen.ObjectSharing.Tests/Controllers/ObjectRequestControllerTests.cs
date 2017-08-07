using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Autofac;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Orchard;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Notify;
using WijDelen.ObjectSharing.Controllers;
using WijDelen.ObjectSharing.Domain.Commands;
using WijDelen.ObjectSharing.Domain.Messaging;
using WijDelen.ObjectSharing.Domain.ValueTypes;
using WijDelen.ObjectSharing.Models;
using WijDelen.ObjectSharing.Tests.TestInfrastructure.Fakes;
using WijDelen.ObjectSharing.ViewModels;

namespace WijDelen.ObjectSharing.Tests.Controllers {
    [TestFixture]
    public class ObjectRequestControllerTests {
        [SetUp]
        public void Init() {
            var containerBuilder = new ContainerBuilder();

            _requestObjectCommandHandlerMock = new Mock<ICommandHandler<RequestObject>>();
            _stopObjectRequestCommandHandlerMock = new Mock<ICommandHandler<StopObjectRequest>>();

            var userMock = new Mock<IUser>();
            userMock.Setup(x => x.Id).Returns(22);
            var services = new FakeOrchardServices();
            services.WorkContext.CurrentUser = userMock.Object;
            var notifierMock = new Mock<INotifier>();
            services.Notifier = notifierMock.Object;

            _objectRequestIdForOtherUser = Guid.NewGuid();
            _objectRequestId = Guid.NewGuid();
            _blockedObjectRequestId = Guid.NewGuid();
            _persistentRecords = new[] {
                new ObjectRequestRecord {
                    AggregateId = _objectRequestIdForOtherUser,
                    UserId = 10
                },
                new ObjectRequestRecord {
                    AggregateId = _objectRequestId,
                    UserId = 22,
                    CreatedDateTime = new DateTime(2016, 1, 1),
                    Description = "Sneakers"
                },
                new ObjectRequestRecord {
                    AggregateId = Guid.NewGuid(),
                    UserId = 22,
                    CreatedDateTime = new DateTime(2017, 1, 1)
                },
                new ObjectRequestRecord {
                    AggregateId = _blockedObjectRequestId,
                    UserId = 22,
                    Status = ObjectRequestStatus.BlockedForForbiddenWords.ToString()
                },
                new ObjectRequestRecord {
                    AggregateId = Guid.NewGuid(),
                    UserId = 22,
                    Status = ObjectRequestStatus.Stopped.ToString()
                }
            };

            var objectRequestRepositoryMock = new Mock<IRepository<ObjectRequestRecord>>();
            objectRequestRepositoryMock.SetRecords(_persistentRecords);

            var chatRepositoryMock = new Mock<IRepository<ChatRecord>>();
            _chatRecords = new[] {
                new ChatRecord {
                    ObjectRequestId = _objectRequestId
                },
                new ChatRecord {
                    ObjectRequestId = Guid.NewGuid()
                }
            };
            chatRepositoryMock.SetRecords(_chatRecords);

            containerBuilder.RegisterInstance(_requestObjectCommandHandlerMock.Object).As<ICommandHandler<RequestObject>>();
            containerBuilder.RegisterInstance(_stopObjectRequestCommandHandlerMock.Object).As<ICommandHandler<StopObjectRequest>>();
            containerBuilder.RegisterInstance(services).As<IOrchardServices>();
            containerBuilder.RegisterInstance(objectRequestRepositoryMock.Object).As<IRepository<ObjectRequestRecord>>();
            containerBuilder.RegisterInstance(chatRepositoryMock.Object).As<IRepository<ChatRecord>>();
            containerBuilder.RegisterType<ObjectRequestController>();
            var container = containerBuilder.Build();

            _controller = container.Resolve<ObjectRequestController>();
            _controller.T = NullLocalizer.Instance;
        }

        private ObjectRequestController _controller;
        private Mock<ICommandHandler<RequestObject>> _requestObjectCommandHandlerMock;
        private Guid _objectRequestIdForOtherUser;
        private Guid _objectRequestId;
        private ObjectRequestRecord[] _persistentRecords;
        private ChatRecord[] _chatRecords;
        private Guid _blockedObjectRequestId;
        private Mock<ICommandHandler<StopObjectRequest>> _stopObjectRequestCommandHandlerMock;

        [Test]
        public void ShouldNotAllowEmptyValuesForNewObjectRequest() {
            var viewModel = new NewObjectRequestViewModel();

            var viewResult = _controller.New(viewModel);

            ((ViewResult) viewResult).ViewData.ModelState["Description"].Errors.Single().ErrorMessage.Should().Be("Please provide a description of the item you need.");
            ((ViewResult) viewResult).ViewData.ModelState["ExtraInfo"].Errors.Single().ErrorMessage.Should().Be("Please provide some extra info.");
        }

        [Test]
        public void ShouldNotAllowTooLongValuesForNewObjectRequest() {
            var viewModel = new NewObjectRequestViewModel {
                Description = new string('*', 51),
                ExtraInfo = new string('*', 1001)
            };

            var viewResult = _controller.New(viewModel);

            ((ViewResult) viewResult).ViewData.ModelState["Description"].Errors.Single().ErrorMessage.Should().Be("Please limit your description to 50 characters.");
            ((ViewResult) viewResult).ViewData.ModelState["ExtraInfo"].Errors.Single().ErrorMessage.Should().Be("Please limit the extra info to 1000 characters.");
        }

        [Test]
        public void WhenGettingIndex_ShouldReturnView() {
            var actionResult = _controller.Index();

            var model = ((ViewResult) actionResult).Model as IEnumerable<ObjectRequestRecord>;
            model.Should().NotBeNull();
            model.Count().Should().Be(3);
            model.ToList()[0].Should().Be(_persistentRecords[2]);
            model.ToList()[1].Should().Be(_persistentRecords[1]);
            model.ToList()[2].Should().Be(_persistentRecords[3]);
        }

        [Test]
        public void WhenGettingItem_ShouldReturnView() {
            var actionResult = _controller.Item(_objectRequestId);

            ((ViewResult) actionResult).Model.ShouldBeEquivalentTo(new ObjectRequestViewModel {
                ObjectRequestRecord = _persistentRecords[1],
                ChatRecords = new List<ChatRecord> {_chatRecords[0]}
            });
        }

        [Test]
        public void WhenGettingItemForUnknownId_ShouldReturnNotFound() {
            var id = Guid.NewGuid();

            var actionResult = _controller.Item(id);

            actionResult.Should().BeOfType<HttpNotFoundResult>();
        }

        [Test]
        public void WhenGettingItemForWrongUser_ShouldReturnUnauthorized() {
            var actionResult = _controller.Item(_objectRequestIdForOtherUser);

            actionResult.Should().BeOfType<HttpUnauthorizedResult>();
        }

        [Test]
        public void WhenPosting_ShouldCallCommandHandlerAndRedirect() {
            RequestObject command = null;
            _requestObjectCommandHandlerMock.Setup(x => x.Handle(It.IsAny<RequestObject>())).Callback((RequestObject c) => command = c);

            var viewModel = new NewObjectRequestViewModel {
                Description = "Sneakers",
                ExtraInfo = "For sneaking.........................."
            };

            var actionResult = _controller.New(viewModel);

            ((RedirectToRouteResult) actionResult).RouteValues["action"].Should().Be("Item");
            ((RedirectToRouteResult) actionResult).RouteValues["id"].Should().Be(command.ObjectRequestId);

            command.Description.Should().Be("Sneakers");
            command.ExtraInfo.Should().Be("For sneaking..........................");
            command.UserId.Should().Be(22);
        }

        [Test]
        public void WhenGettingStop() {
            var actionResult = _controller.Stop(_objectRequestId);

            actionResult.Should().BeOfType<ViewResult>();
            ((ViewResult) actionResult).Model.ShouldBeEquivalentTo(new ConfirmStopObjectRequestViewModel {
                Id = _objectRequestId,
                Description = "Sneakers"
            });
        }

        [Test]
        public void WhenGettingStopForObjectRequestOfOtherUser() {
            var actionResult = _controller.Stop(_objectRequestIdForOtherUser);

            actionResult.Should().BeOfType<HttpUnauthorizedResult>();
        }

        [Test]
        public void WhenGettingStopForUnknownId_ShouldReturnNotFound() {
            var id = Guid.NewGuid();

            var actionResult = _controller.Stop(id);

            actionResult.Should().BeOfType<HttpNotFoundResult>();
        }

        [Test]
        public void WhenConfirmingStop_ShouldCallCommandHandlerAndRedirect() {
            var id = Guid.NewGuid();
            StopObjectRequest command = null;
            _stopObjectRequestCommandHandlerMock.Setup(x => x.Handle(It.IsAny<StopObjectRequest>())).Callback((StopObjectRequest c) => command = c);

            var viewModel = new ConfirmStopObjectRequestViewModel
            {
                Id = id,
                Description = "Sneakers"
            };

            var actionResult = _controller.Stop(viewModel);

            ((RedirectToRouteResult)actionResult).RouteValues["action"].Should().Be("Index");

            command.ObjectRequestId.Should().Be(id);
        }

        /// <summary>
        /// These properties should also remain public, as they are used in the cshtml file.
        /// </summary>
        [Test]
        public void TestMaximumLengths() {
            ObjectRequestController.MaximumDescriptionLength.Should().Be(50);
            ObjectRequestController.MaximumExtraInfoLength.Should().Be(1000);
        }
    }
}