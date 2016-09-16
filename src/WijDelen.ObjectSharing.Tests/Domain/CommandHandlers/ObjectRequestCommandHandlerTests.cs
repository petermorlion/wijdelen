using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using WijDelen.ObjectSharing.Domain;
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
    }
}