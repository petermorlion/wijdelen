using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Orchard.Data;
using WijDelen.ObjectSharing.Domain.Enums;
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

            var e = new ObjectRequestConfirmed { ConfirmingUserId = 22, SourceId = objectRequestId, DateTimeConfirmed = new DateTime(2017, 1, 27) };

            handler.Handle(e);

            record.UserId.Should().Be(22);
            record.ObjectRequestId.Should().Be(objectRequestId);
            record.Response.Should().Be(ObjectRequestAnswer.Yes);
            record.DateTimeResponded.Should().Be(new DateTime(2017, 1, 27));
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

            var e = new ObjectRequestDenied { DenyingUserId = 22, SourceId = objectRequestId, DateTimeDenied = new DateTime(2017, 1, 27) };

            handler.Handle(e);

            record.UserId.Should().Be(22);
            record.ObjectRequestId.Should().Be(objectRequestId);
            record.Response.Should().Be(ObjectRequestAnswer.No);
            record.DateTimeResponded.Should().Be(new DateTime(2017, 1, 27));
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

            var e = new ObjectRequestDeniedForNow { DenyingUserId = 22, SourceId = objectRequestId, DateTimeDenied = new DateTime(2017, 1, 27) };

            handler.Handle(e);

            record.UserId.Should().Be(22);
            record.ObjectRequestId.Should().Be(objectRequestId);
            record.Response.Should().Be(ObjectRequestAnswer.NotNow);
            record.DateTimeResponded.Should().Be(new DateTime(2017, 1, 27));
        }
    }
}