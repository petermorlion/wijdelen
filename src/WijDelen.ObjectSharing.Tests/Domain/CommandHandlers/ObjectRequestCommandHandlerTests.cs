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
    }
}