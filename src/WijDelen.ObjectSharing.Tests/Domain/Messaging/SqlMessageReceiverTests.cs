using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Orchard.Data;
using WijDelen.ObjectSharing.Domain.Messaging;
using WijDelen.ObjectSharing.Models;
using WijDelen.ObjectSharing.Tests.TestInfrastructure.Fakes;

namespace WijDelen.ObjectSharing.Tests.Domain.Messaging {
    [TestFixture]
    public class SqlMessageReceiverTests {
        [Test]
        public void ShouldReceiveMessagesAndSendToEventHandlersAndDeleteRecord() {
            var messageRecord = new MessageRecord {
                Id = 2,
                Body = @"{""$type"":""WijDelen.ObjectSharing.Tests.TestInfrastructure.Fakes.FakeEvent, WijDelen.ObjectSharing.Tests"",""SourceId"":""00a69bc3-ce8c-48ee-8f29-c7e67a31e31a""}"
            };

            var messages = new[] {
                new MessageRecord { Id = 1, DeliveryDate = DateTime.UtcNow.AddDays(1) },
                messageRecord,
                new MessageRecord { Id = 3 },
            };

            var repositoryMock = new Mock<IRepository<MessageRecord>>();
            repositoryMock
                .Setup(x => x.Fetch(It.IsAny<Expression<Func<MessageRecord, bool>>>(), It.IsAny<Action<Orderable<MessageRecord>>>(), 0, 1))
                .Returns((Expression<Func<MessageRecord, bool>> predicate, Action<Orderable<MessageRecord>> order, int skip, int take) => {
                    var orderable = new Orderable<MessageRecord>(messages.Where(predicate.Compile()).AsQueryable());
                    order(orderable);
                    return orderable.Queryable.Skip(skip).Take(take).ToList();
                });

            FakeEvent handledEvent = null;
            var relevantEventHandlerMock = new Mock<IEventHandler<FakeEvent>>();
            relevantEventHandlerMock.Setup(x => x.Handle(It.IsAny<FakeEvent>())).Callback((FakeEvent e) => handledEvent = e);
            var irrelevantEventHandlerMock = new Mock<IEventHandler<IrrelevantEvent>>();

            var receiver = new SqlMessageReceiver(repositoryMock.Object, new IEventHandler[] { relevantEventHandlerMock.Object, irrelevantEventHandlerMock.Object });
            var receiveMethod = typeof(SqlMessageReceiver).GetMethod("ReceiveMessage", BindingFlags.Instance | BindingFlags.NonPublic);

            var result = (bool)receiveMethod.Invoke(receiver, new object[0]);

            result.Should().BeTrue();
            handledEvent.SourceId.Should().Be(Guid.Parse("00a69bc3-ce8c-48ee-8f29-c7e67a31e31a"));
            irrelevantEventHandlerMock.Verify(x => x.Handle(It.IsAny<IrrelevantEvent>()), Times.Never);
            repositoryMock.Verify(x => x.Delete(messageRecord));
        }

        [Test]
        public void ShouldReturnFalseIfNoMessagesReceived()
        {
            var repositoryMock = new Mock<IRepository<MessageRecord>>();
            repositoryMock
                .Setup(x => x.Fetch(It.IsAny<Expression<Func<MessageRecord, bool>>>(), It.IsAny<Action<Orderable<MessageRecord>>>(), 0, 1))
                .Returns((Expression<Func<MessageRecord, bool>> predicate, Action<Orderable<MessageRecord>> order, int skip, int take) => null);

            var eventHandlerMock1 = new Mock<IEventHandler<FakeEvent>>();
            var eventHandlerMock2 = new Mock<IEventHandler<IrrelevantEvent>>();

            var receiver = new SqlMessageReceiver(repositoryMock.Object, new IEventHandler[] { eventHandlerMock1.Object, eventHandlerMock2.Object });
            var receiveMethod = typeof(SqlMessageReceiver).GetMethod("ReceiveMessage", BindingFlags.Instance | BindingFlags.NonPublic);

            var result = (bool)receiveMethod.Invoke(receiver, new object[0]);

            result.Should().BeFalse();
            eventHandlerMock1.Verify(x => x.Handle(It.IsAny<FakeEvent>()), Times.Never);
            eventHandlerMock2.Verify(x => x.Handle(It.IsAny<IrrelevantEvent>()), Times.Never);
            repositoryMock.Verify(x => x.Delete(It.IsAny<MessageRecord>()), Times.Never);
        }

        public class IrrelevantEvent : IEvent {
            public Guid SourceId { get; }
        }
    }
}