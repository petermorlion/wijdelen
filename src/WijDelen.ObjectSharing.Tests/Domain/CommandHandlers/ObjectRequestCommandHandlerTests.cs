using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using WijDelen.ObjectSharing.Domain.CommandHandlers;
using WijDelen.ObjectSharing.Domain.Commands;
using WijDelen.ObjectSharing.Domain.Entities;
using WijDelen.ObjectSharing.Domain.Events;
using WijDelen.ObjectSharing.Domain.EventSourcing;
using WijDelen.ObjectSharing.Domain.ValueTypes;

namespace WijDelen.ObjectSharing.Tests.Domain.CommandHandlers {
    [TestFixture]
    public class ObjectRequestCommandHandlerTests {
        [Test]
        public void WhenHandlingRequestObjectCommand_ShouldCreateNewObjectRequest() {
            ObjectRequest objectRequest = null;
            var command = new RequestObject("description", "extraInfo", 22);
            var repositoryMock = new Mock<IEventSourcedRepository<ObjectRequest>>();
            repositoryMock.Setup(x => x.Save(It.IsAny<ObjectRequest>(), command.Id.ToString())).Callback((ObjectRequest o, string correlationId) => objectRequest = o);
            var commandHandler = new ObjectRequestCommandHandler(repositoryMock.Object);

            commandHandler.Handle(command);

            objectRequest.Id.Should().Be(command.ObjectRequestId);
            objectRequest.UserId.Should().Be(22);
            objectRequest.ExtraInfo.Should().Be("extraInfo");
            objectRequest.Description.Should().Be("description");
            objectRequest.Events.Single().ShouldBeEquivalentTo(new ObjectRequested {
                Description = "description",
                ExtraInfo = "extraInfo"
            }, options => options.Excluding(o => o.SourceId));
        }

        [Test]
        public void WhenConfirmingObjectRequest() {
            var objectRequestId = Guid.NewGuid();
            var objectRequest = new ObjectRequest(objectRequestId, "Sneakers", "For sneaking", 1);
            var command = new ConfirmObjectRequest(22, objectRequestId);

            ObjectRequest persistedObjectRequest = null;
            var repositoryMock = new Mock<IEventSourcedRepository<ObjectRequest>>();
            repositoryMock.Setup(x => x.Find(objectRequestId)).Returns(objectRequest);
            repositoryMock
                .Setup(x => x.Save(It.IsAny<ObjectRequest>(), command.Id.ToString()))
                .Callback((ObjectRequest or, string correlationId) => persistedObjectRequest = or);

            var handler = new ObjectRequestCommandHandler(repositoryMock.Object);

            handler.Handle(command);

            persistedObjectRequest.ConfirmingUserIds.ShouldBeEquivalentTo(new List<int> { 22 });
        }

        [Test]
        public void WhenDenyingObjectRequest() {
            var objectRequestId = Guid.NewGuid();
            var objectRequest = new ObjectRequest(objectRequestId, "Sneakers", "For sneaking", 1);
            var command = new DenyObjectRequest(22, objectRequestId);

            ObjectRequest persistedObjectRequest = null;
            var repositoryMock = new Mock<IEventSourcedRepository<ObjectRequest>>();
            repositoryMock.Setup(x => x.Find(objectRequestId)).Returns(objectRequest);
            repositoryMock
                .Setup(x => x.Save(It.IsAny<ObjectRequest>(), command.Id.ToString()))
                .Callback((ObjectRequest or, string correlationId) => persistedObjectRequest = or);

            var handler = new ObjectRequestCommandHandler(repositoryMock.Object);

            handler.Handle(command);

            persistedObjectRequest.DenyingUserIds.ShouldBeEquivalentTo(new List<int> { 22 });
        }

        [Test]
        public void WhenDenyingObjectRequestForNow() {
            var objectRequestId = Guid.NewGuid();
            var objectRequest = new ObjectRequest(objectRequestId, "Sneakers", "For sneaking", 1);
            var command = new DenyObjectRequestForNow(22, objectRequestId);

            ObjectRequest persistedObjectRequest = null;
            var repositoryMock = new Mock<IEventSourcedRepository<ObjectRequest>>();
            repositoryMock.Setup(x => x.Find(objectRequestId)).Returns(objectRequest);
            repositoryMock
                .Setup(x => x.Save(It.IsAny<ObjectRequest>(), command.Id.ToString()))
                .Callback((ObjectRequest or, string correlationId) => persistedObjectRequest = or);

            var handler = new ObjectRequestCommandHandler(repositoryMock.Object);

            handler.Handle(command);

            persistedObjectRequest.DenyingForNowUserIds.ShouldBeEquivalentTo(new List<int> { 22 });
        }

        [Test]
        public void WhenHandlingUnblockObjectRequestsCommand_ShouldUnblockObjectRequests() {
            var objectRequestId1 = Guid.NewGuid();
            var objectRequest1 = new ObjectRequest(objectRequestId1, "Sneakers", "For sneaking", 1);
            var objectRequestId2 = Guid.NewGuid();
            var objectRequest2 = new ObjectRequest(objectRequestId2, "Flaming Moe", "For drinking", 1);
            var command = new UnblockObjectRequests(new [] { objectRequestId1, objectRequestId2 });
            var repositoryMock = new Mock<IEventSourcedRepository<ObjectRequest>>();
            repositoryMock.Setup(x => x.Find(objectRequestId1)).Returns(objectRequest1);
            repositoryMock.Setup(x => x.Find(objectRequestId2)).Returns(objectRequest2);

            ObjectRequest persistedObjectRequest1 = null;
            ObjectRequest persistedObjectRequest2 = null;
            repositoryMock
                .Setup(x => x.Save(It.IsAny<ObjectRequest>(), command.Id.ToString()))
                .Callback((ObjectRequest or, string correlationId) => {
                    if (or.Id == objectRequestId1) {
                        persistedObjectRequest1 = or;
                    } else if (or.Id == objectRequestId2) {
                        persistedObjectRequest2 = objectRequest2;
                    } else {
                        Assert.Fail("Wrong ObjectRequest persisted.");
                    }
                });

            var commandHandler = new ObjectRequestCommandHandler(repositoryMock.Object);

            commandHandler.Handle(command);

            persistedObjectRequest1.Events.Last().ShouldBeEquivalentTo(new ObjectRequestUnblocked {
                SourceId = objectRequestId1,
                Description = "Sneakers",
                ExtraInfo = "extraInfo",
                UserId = 1,
                Version = 1
            });

            persistedObjectRequest2.Events.Last().ShouldBeEquivalentTo(new ObjectRequestUnblocked
            {
                SourceId = objectRequestId2,
                Description = "Sneakers",
                ExtraInfo = "extraInfo",
                UserId = 1,
                Version = 1
            });
        }

        [Test]
        public void WhenHandlingBlockObjectRequestByAdminCommand_ShouldBlockObjectRequest()
        {
            var objectRequestId = Guid.NewGuid();
            var objectRequest = new ObjectRequest(objectRequestId, "Sneakers", "For sneaking", 1);
            var command = new BlockObjectRequestByAdmin(objectRequestId, "Just because");
            var repositoryMock = new Mock<IEventSourcedRepository<ObjectRequest>>();
            repositoryMock.Setup(x => x.Find(objectRequestId)).Returns(objectRequest);

            ObjectRequest persistedObjectRequest = null;
            repositoryMock
                .Setup(x => x.Save(It.IsAny<ObjectRequest>(), command.Id.ToString()))
                .Callback((ObjectRequest or, string correlationId) => {
                    if (or.Id == objectRequestId)
                    {
                        persistedObjectRequest = or;
                    }
                    else
                    {
                        Assert.Fail("Wrong ObjectRequest persisted.");
                    }
                });

            var commandHandler = new ObjectRequestCommandHandler(repositoryMock.Object);

            commandHandler.Handle(command);

            objectRequest.Events.Last().ShouldBeEquivalentTo(new ObjectRequestBlockedByAdmin
            {
                SourceId = objectRequestId,
                Version = 1,
                Reason = "Just because"
            });
        }

        [Test]
        public void WhenStoppingObjectRequest() {
            var objectRequestId = Guid.NewGuid();
            var objectRequest = new ObjectRequest(objectRequestId, "Sneakers", "For sneaking", 1);
            var command = new StopObjectRequest(objectRequestId);
            var repositoryMock = new Mock<IEventSourcedRepository<ObjectRequest>>();
            repositoryMock.Setup(x => x.Find(objectRequestId)).Returns(objectRequest);

            var commandHandler = new ObjectRequestCommandHandler(repositoryMock.Object);

            commandHandler.Handle(command);

            objectRequest.Events.Last().ShouldBeEquivalentTo(new ObjectRequestStopped {
                SourceId = objectRequestId,
                Version = 1
            });

            objectRequest.Status.Should().Be(ObjectRequestStatus.Stopped);

            repositoryMock.Verify(x => x.Save(objectRequest, command.Id.ToString()));
        }
    }
}