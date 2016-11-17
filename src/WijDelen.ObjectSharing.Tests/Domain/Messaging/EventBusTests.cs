using System;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using WijDelen.ObjectSharing.Domain.Messaging;
using WijDelen.ObjectSharing.Tests.TestInfrastructure.Fakes;

namespace WijDelen.ObjectSharing.Tests.Domain.Messaging {
    [TestFixture]
    public class EventBusTests {
        [Test]
        public void ShouldPublishToMessageSender() {
            Message sentMessage = null;

            var messageSenderMock = new Mock<IMessageSender>();
            messageSenderMock.Setup(x => x.Send(It.IsAny<Message>())).Callback((Message m) => sentMessage = m);

            var eventBus = new EventBus(messageSenderMock.Object);
            var e = new FakeEvent {
                SourceId = Guid.Parse("00a69bc3-ce8c-48ee-8f29-c7e67a31e31a")
            };

            eventBus.Publish(e, "correlationId");

            sentMessage.DeliveryDate.Should().NotHaveValue("because currently we don't schedule messages for the future.");
            sentMessage.CorrelationId.Should().Be("correlationId");
            sentMessage.Body.Should().Be(@"{""$type"":""WijDelen.ObjectSharing.Tests.TestInfrastructure.Fakes.FakeEvent, WijDelen.ObjectSharing.Tests"",""SourceId"":""00a69bc3-ce8c-48ee-8f29-c7e67a31e31a""}");
        }
    }
}