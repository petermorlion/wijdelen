using System;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using WijDelen.ObjectSharing.Domain.Messaging;
using WijDelen.ObjectSharing.Tests.TestInfrastructure.Fakes;

namespace WijDelen.ObjectSharing.Tests.Domain.Messaging {
    [TestFixture]
    public class InMemoryMessageReceiverTests {
        [Test]
        public void ShouldReceiveMessagesAndSendToEventHandlers() {
            var messageBody = @"{""$type"":""WijDelen.ObjectSharing.Tests.TestInfrastructure.Fakes.FakeEvent, WijDelen.ObjectSharing.Tests"",""SourceId"":""00a69bc3-ce8c-48ee-8f29-c7e67a31e31a""}";
            
            FakeEvent handledEvent = null;
            var relevantEventHandlerMock = new Mock<IEventHandler<FakeEvent>>();
            relevantEventHandlerMock.Setup(x => x.Handle(It.IsAny<FakeEvent>())).Callback((FakeEvent e) => handledEvent = e);
            var irrelevantEventHandlerMock = new Mock<IEventHandler<IrrelevantEvent>>();

            var messageSender = new InMemoryMessageSender();

            var receiver = new InMemoryMessageReceiver(messageSender, new IEventHandler[] {relevantEventHandlerMock.Object, irrelevantEventHandlerMock.Object});
            receiver.Start();
            
            messageSender.Send(new Message(messageBody, new DateTime(2016, 11, 18), "CorrelationId"));

            handledEvent.SourceId.Should().Be(Guid.Parse("00a69bc3-ce8c-48ee-8f29-c7e67a31e31a"));
            irrelevantEventHandlerMock.Verify(x => x.Handle(It.IsAny<IrrelevantEvent>()), Times.Never);

            receiver.Dispose();
        }

        public class IrrelevantEvent : IEvent {
            public Guid SourceId { get; }
        }
    }
}