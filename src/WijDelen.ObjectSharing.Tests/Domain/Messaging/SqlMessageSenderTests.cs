using System;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Orchard.Data;
using WijDelen.ObjectSharing.Domain.Messaging;
using WijDelen.ObjectSharing.Models;

namespace WijDelen.ObjectSharing.Tests.Domain.Messaging {
    [TestFixture]
    public class SqlMessageSenderTests {
        [Test]
        public void ShouldStoreMessageRecord() {
            MessageRecord storedMessageRecord = null;

            var repositoryMock = new Mock<IRepository<MessageRecord>>();
            repositoryMock.Setup(x => x.Update(It.IsAny<MessageRecord>())).Callback((MessageRecord mr) => storedMessageRecord = mr);

            var message = new Message("Body", new DateTime(2016, 11, 17), "CorrelationId");
            var messageSender = new SqlMessageSender(repositoryMock.Object);

            messageSender.Send(message);

            storedMessageRecord.Body.Should().Be("Body");
            storedMessageRecord.DeliveryDate.Should().Be(new DateTime(2016, 11, 17));
            storedMessageRecord.CorrelationId.Should().Be("CorrelationId");
        }
    }
}