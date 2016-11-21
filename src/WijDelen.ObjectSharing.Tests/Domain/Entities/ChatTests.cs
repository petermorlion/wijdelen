using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using WijDelen.ObjectSharing.Domain.Entities;
using WijDelen.ObjectSharing.Domain.Events;

namespace WijDelen.ObjectSharing.Tests.Domain.Entities {
    [TestFixture]
    public class ChatTests {
        [Test]
        public void WhenCreatingChat() {
            var id = Guid.NewGuid();
            var objectRequestId = Guid.NewGuid();
            var chat = new Chat(id, objectRequestId, 1, 2);

            chat.RequestingUserId.Should().Be(1);
            chat.ConfirmingUserId.Should().Be(2);
            chat.ObjectRequestId.Should().Be(objectRequestId);
            chat.Id.Should().Be(id);

            chat.Events.Single().ShouldBeEquivalentTo(new ChatStarted {
                SourceId = id,
                ConfirmingUserId = 2,
                RequestingUserId = 1,
                ObjectRequestId = objectRequestId,
                Version = 0
            });

            chat.Version.Should().Be(0);
        }

        [Test]
        public void WhenConstructingFromHistory() {
            var id = Guid.NewGuid();
            var objectRequestId = Guid.NewGuid();
            var previousEvent = new ChatStarted {
                ConfirmingUserId = 2,
                RequestingUserId = 1,
                ObjectRequestId = objectRequestId
            };

            var chat = new Chat(id, new[] {previousEvent});

            chat.Events.Should().BeEmpty();
            chat.Version.Should().Be(0);
            chat.RequestingUserId.Should().Be(1);
            chat.ConfirmingUserId.Should().Be(2);
            chat.ObjectRequestId.Should().Be(objectRequestId);
        }

        [Test]
        public void WhenAddingMessage() {
            var dateTime = new DateTime(2016, 11, 21, 12, 30, 15);
            var chat = new Chat(Guid.NewGuid(), Guid.NewGuid(), 1, 2);

            chat.AddMessage(dateTime, 2, "Hello");

            chat.Events.Last().As<ChatMessageAdded>().DateTime.Should().Be(dateTime);
            chat.Events.Last().As<ChatMessageAdded>().UserId.Should().Be(2);
            chat.Events.Last().As<ChatMessageAdded>().Message.Should().Be("Hello");
            chat.Events.Last().As<ChatMessageAdded>().ChatId.Should().Be(chat.Id);

            chat.Messages.Single().ShouldBeEquivalentTo(new ChatMessage(dateTime, 2, "Hello"));
        }
    }
}