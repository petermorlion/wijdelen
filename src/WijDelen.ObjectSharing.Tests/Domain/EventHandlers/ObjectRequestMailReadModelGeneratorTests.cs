using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Orchard.Data;
using WijDelen.ObjectSharing.Domain.EventHandlers;
using WijDelen.ObjectSharing.Domain.Events;
using WijDelen.ObjectSharing.Models;
using WijDelen.ObjectSharing.Tests.TestInfrastructure.Fakes;

namespace WijDelen.ObjectSharing.Tests.Domain.EventHandlers {
    [TestFixture]
    public class ObjectRequestMailReadModelGeneratorTests {
        [Test]
        public void WhenHandlingObjectRequestMailSent_ShouldSaveNewObjectRequestMailRecord() {
            var persistentRecords = new List<ObjectRequestMailRecord>();

            var repositoryMock = new Mock<IRepository<ObjectRequestMailRecord>>();
            repositoryMock.SetRecords(persistentRecords);

            repositoryMock
                .Setup(x => x.Update(It.IsAny<ObjectRequestMailRecord>()))
                .Callback((ObjectRequestMailRecord r) => persistentRecords.Add(r));

            var aggregateId = Guid.NewGuid();
            var handler = new ObjectRequestMailReadModelGenerator(repositoryMock.Object);
            var e = new ObjectRequestMailSent {
                SourceId = aggregateId,
                Version = 0,
                Recipients = new[] { "peter.morlion@gmail.com", "peter.morlion@telenet.be" },
                EmailHtml = "<html></html>",
                RequestingUserId = 22
            };

            handler.Handle(e);

            persistentRecords.ShouldBeEquivalentTo(new List<ObjectRequestMailRecord> {
                new ObjectRequestMailRecord {
                    AggregateId = aggregateId,
                    EmailAddress = "peter.morlion@gmail.com",
                    EmailHtml = "<html></html>",
                    RequestingUserId = 22   
                },
                new ObjectRequestMailRecord {
                    AggregateId = aggregateId,
                    EmailAddress = "peter.morlion@telenet.be",
                    EmailHtml = "<html></html>",
                    RequestingUserId = 22
                }
            });
        }
    }
}