using System;
using FluentAssertions;
using NUnit.Framework;
using WijDelen.ObjectSharing.Domain.Messaging;

namespace WijDelen.ObjectSharing.Tests.Domain.Messaging {
    [TestFixture]
    public class InMemoryMessageSenderTests {
        [Test]
        public void ShouldStoreMessageRecord() {
            var message = new Message("Body", new DateTime(2016, 11, 17), "CorrelationId");
            var messageSender = new InMemoryMessageSender();
            SendingMessageEventArgs eventArgs = null;
            messageSender.SendingMessage += (sender, args) => eventArgs = args;

            messageSender.Send(message);

            eventArgs.Message.Body.Should().Be("Body");
            eventArgs.Message.DeliveryDate.Should().Be(new DateTime(2016, 11, 17));
            eventArgs.Message.CorrelationId.Should().Be("CorrelationId");
        }
    }
}