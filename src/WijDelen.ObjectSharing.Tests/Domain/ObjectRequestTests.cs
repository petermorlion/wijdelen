using FluentAssertions;
using NUnit.Framework;
using System.Linq;
using WijDelen.ObjectSharing.Domain;
using WijDelen.ObjectSharing.Domain.Events;

namespace WijDelen.ObjectSharing.Tests.Domain {
    [TestFixture]
    public class ObjectRequestTests {
        [Test]
        public void UpdateInfo() {
            var objectRequest = new ObjectRequest(1);

            objectRequest.UpdateInfo("Sneakers", "for sneaking");

            objectRequest.Description.Should().Be("Sneakers");
            objectRequest.ExtraInfo.Should().Be("for sneaking");

            objectRequest.Events.Single().ShouldBeEquivalentTo(new ObjectRequestInfoUpdated {
                SourceId = 1,
                Description = "Sneakers",
                ExtraInfo = "for sneaking"
            });

            objectRequest.Version.Should().Be(0);
        }
    }
}