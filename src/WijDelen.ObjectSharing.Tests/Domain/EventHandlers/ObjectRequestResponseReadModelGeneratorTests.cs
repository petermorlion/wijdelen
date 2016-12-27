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
    public class ObjectRequestResponseReadModelGeneratorTests {
        [Test]
        public void WhenObjectRequestConfirmed_CreateObjectRequestResponseRecord() {
            ObjectRequestResponseRecord record = null;

            var repositoryMock = new Mock<IRepository<ObjectRequestResponseRecord>>();
            repositoryMock.SetRecords(new List<ObjectRequestResponseRecord>());
            repositoryMock
                .Setup(x => x.Create(It.IsAny<ObjectRequestResponseRecord>()))
                .Callback((ObjectRequestResponseRecord r) => record = r);

            var handler = new ObjectRequestResponseReadModelGenerator(repositoryMock.Object);
            var objectRequestId = Guid.NewGuid();

            var e = new ObjectRequestConfirmed { ConfirmingUserId = 22, SourceId = objectRequestId };

            handler.Handle(e);

            record.UserId.Should().Be(22);
            record.ObjectRequestId.Should().Be(objectRequestId);
            record.Response.Should().Be("Yes");
        }

        [Test]
        public void WhenObjectRequestDenied_CreateObjectRequestResponseRecord() {
            ObjectRequestResponseRecord record = null;

            var repositoryMock = new Mock<IRepository<ObjectRequestResponseRecord>>();
            repositoryMock.SetRecords(new List<ObjectRequestResponseRecord>());
            repositoryMock
                .Setup(x => x.Create(It.IsAny<ObjectRequestResponseRecord>()))
                .Callback((ObjectRequestResponseRecord r) => record = r);

            var handler = new ObjectRequestResponseReadModelGenerator(repositoryMock.Object);
            var objectRequestId = Guid.NewGuid();

            var e = new ObjectRequestDenied { DenyingUserId = 22, SourceId = objectRequestId };

            handler.Handle(e);

            record.UserId.Should().Be(22);
            record.ObjectRequestId.Should().Be(objectRequestId);
            record.Response.Should().Be("No");
        }

        [Test]
        public void WhenObjectRequestDeniedForNow_CreateObjectRequestResponseRecord() {
            ObjectRequestResponseRecord record = null;

            var repositoryMock = new Mock<IRepository<ObjectRequestResponseRecord>>();
            repositoryMock.SetRecords(new List<ObjectRequestResponseRecord>());
            repositoryMock
                .Setup(x => x.Create(It.IsAny<ObjectRequestResponseRecord>()))
                .Callback((ObjectRequestResponseRecord r) => record = r);

            var handler = new ObjectRequestResponseReadModelGenerator(repositoryMock.Object);
            var objectRequestId = Guid.NewGuid();

            var e = new ObjectRequestDeniedForNow { DenyingUserId = 22, SourceId = objectRequestId };

            handler.Handle(e);

            record.UserId.Should().Be(22);
            record.ObjectRequestId.Should().Be(objectRequestId);
            record.Response.Should().Be("NotNow");
        }
    }
}