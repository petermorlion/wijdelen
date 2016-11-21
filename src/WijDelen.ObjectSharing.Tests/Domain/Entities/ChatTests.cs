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
        public void WhenCreatingChat()
        {
            var id = Guid.NewGuid();
            var objectRequestId = Guid.NewGuid();
            var chat = new Chat(id, objectRequestId, 1, 2);

            chat.RequestingUserId.Should().Be(1);
            chat.ConfirmingUserId.Should().Be(2);
            chat.ObjectRequestId.Should().Be(objectRequestId);
            chat.Id.Should().Be(id);

            chat.Events.Single().ShouldBeEquivalentTo(new ChatStarted
            {
                SourceId = id,
                ConfirmingUserId = 2,
                RequestingUserId = 1,
                ObjectRequestId = objectRequestId,
                Version = 0
            });

            chat.Version.Should().Be(0);
        }

        [Test]
        public void WhenConstructingFromHistory()
        {
            var id = Guid.NewGuid();
            var objectRequestId = Guid.NewGuid();
            var previousEvent = new ChatStarted()
            {
                ConfirmingUserId = 2,
                RequestingUserId = 1,
                ObjectRequestId = objectRequestId
            };

            var chat = new Chat(id, new[] { previousEvent });

            chat.Events.Should().BeEmpty();
            chat.Version.Should().Be(0);
            chat.RequestingUserId.Should().Be(1);
            chat.ConfirmingUserId.Should().Be(2);
            chat.ObjectRequestId.Should().Be(objectRequestId);
        }
    }
}