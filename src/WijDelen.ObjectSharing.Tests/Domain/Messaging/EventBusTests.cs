using System;
using FluentAssertions;
using NUnit.Framework;
using WijDelen.ObjectSharing.Domain.Messaging;
using WijDelen.ObjectSharing.Tests.TestInfrastructure.Fakes;

namespace WijDelen.ObjectSharing.Tests.Domain.Messaging {
    [TestFixture]
    public class EventBusTests {
        [Test]
        public void ShouldPublishToCorrectHandlers() {
            var eventHandler = new FakeEventHandler();
            var eventBus = new EventBus(new [] { eventHandler });
            var e = new FakeEvent();

            eventBus.Publish(e);

            eventHandler.WasCalled.Should().BeTrue();
        }

        [Test]
        public void ShouldNotFailIfNoHandlers()
        {
            var eventBus = new EventBus(new IEventHandler[] {});
            var e = new FakeEvent();

            eventBus.Publish(e);
        }
    }
}