using System;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Orchard.Data;
using WijDelen.ObjectSharing.Domain.EventHandlers;
using WijDelen.ObjectSharing.Domain.Events;
using WijDelen.ObjectSharing.Domain.ValueTypes;
using WijDelen.ObjectSharing.Models;
using WijDelen.ObjectSharing.Tests.TestInfrastructure.Fakes;

namespace WijDelen.ObjectSharing.Tests.Domain.EventHandlers {
    [TestFixture]
    public class ReceivedObjectRequestReadModelGeneratorTests {
        [Test]
        public void WhenObjectRequestConfirmed_DontFailOnRemovingRecord() {
            var repositoryMock = new Mock<IRepository<ReceivedObjectRequestRecord>>();
            var objectRequestId = Guid.NewGuid();

            var e = new ObjectRequestConfirmed {
                SourceId = objectRequestId,
                ConfirmingUserId = 22
            };

            var handler = new ReceivedObjectRequestReadModelGenerator(repositoryMock.Object, null);

            handler.Handle(e);

            repositoryMock.Verify(x => x.Delete(It.IsAny<ReceivedObjectRequestRecord>()), Times.Never);
        }

        [Test]
        public void WhenObjectRequestConfirmed_RemoveRecord() {
            var repositoryMock = new Mock<IRepository<ReceivedObjectRequestRecord>>();
            var objectRequestId = Guid.NewGuid();

            var receivedObjectRequestRecord = new ReceivedObjectRequestRecord {
                UserId = 22,
                ObjectRequestId = objectRequestId
            };
            repositoryMock.SetRecords(new[] {
                receivedObjectRequestRecord
            });

            var e = new ObjectRequestConfirmed {
                SourceId = objectRequestId,
                ConfirmingUserId = 22
            };

            var handler = new ReceivedObjectRequestReadModelGenerator(repositoryMock.Object, null);

            handler.Handle(e);

            repositoryMock.Verify(x => x.Delete(receivedObjectRequestRecord));
        }

        [Test]
        public void WhenObjectRequestDenied_DontFailOnRemovingRecord() {
            var repositoryMock = new Mock<IRepository<ReceivedObjectRequestRecord>>();
            var objectRequestId = Guid.NewGuid();

            var e = new ObjectRequestDenied {
                SourceId = objectRequestId,
                DenyingUserId = 22
            };

            var handler = new ReceivedObjectRequestReadModelGenerator(repositoryMock.Object, null);

            handler.Handle(e);

            repositoryMock.Verify(x => x.Delete(It.IsAny<ReceivedObjectRequestRecord>()), Times.Never);
        }

        [Test]
        public void WhenObjectRequestDenied_RemoveRecord() {
            var repositoryMock = new Mock<IRepository<ReceivedObjectRequestRecord>>();
            var objectRequestId = Guid.NewGuid();

            var receivedObjectRequestRecord = new ReceivedObjectRequestRecord {
                UserId = 22,
                ObjectRequestId = objectRequestId
            };
            repositoryMock.SetRecords(new[] {
                receivedObjectRequestRecord
            });

            var e = new ObjectRequestDenied {
                SourceId = objectRequestId,
                DenyingUserId = 22
            };

            var handler = new ReceivedObjectRequestReadModelGenerator(repositoryMock.Object, null);

            handler.Handle(e);

            repositoryMock.Verify(x => x.Delete(receivedObjectRequestRecord));
        }

        [Test]
        public void WhenObjectRequestDeniedForNow_DontFailOnRemovingRecord() {
            var repositoryMock = new Mock<IRepository<ReceivedObjectRequestRecord>>();
            var objectRequestId = Guid.NewGuid();

            var e = new ObjectRequestDeniedForNow {
                SourceId = objectRequestId,
                DenyingUserId = 22
            };

            var handler = new ReceivedObjectRequestReadModelGenerator(repositoryMock.Object, null);

            handler.Handle(e);

            repositoryMock.Verify(x => x.Delete(It.IsAny<ReceivedObjectRequestRecord>()), Times.Never);
        }

        [Test]
        public void WhenObjectRequestDeniedForNow_RemoveRecord() {
            var repositoryMock = new Mock<IRepository<ReceivedObjectRequestRecord>>();
            var objectRequestId = Guid.NewGuid();

            var receivedObjectRequestRecord = new ReceivedObjectRequestRecord {
                UserId = 22,
                ObjectRequestId = objectRequestId
            };
            repositoryMock.SetRecords(new[] {
                receivedObjectRequestRecord
            });

            var e = new ObjectRequestDeniedForNow {
                SourceId = objectRequestId,
                DenyingUserId = 22
            };

            var handler = new ReceivedObjectRequestReadModelGenerator(repositoryMock.Object, null);

            handler.Handle(e);

            repositoryMock.Verify(x => x.Delete(receivedObjectRequestRecord));
        }

        [Test]
        public void WhenObjectRequestMailSent_CreateRecord() {
            ReceivedObjectRequestRecord record = null;
            var repositoryMock = new Mock<IRepository<ReceivedObjectRequestRecord>>();
            repositoryMock
                .Setup(x => x.Create(It.IsAny<ReceivedObjectRequestRecord>()))
                .Callback((ReceivedObjectRequestRecord rec) => record = rec);

            var e = new ObjectRequestMailSent {
                ObjectRequestId = Guid.NewGuid(),
                SentDateTime = DateTime.UtcNow,
                Recipients = new[] {new UserEmail {Email = "peter.morlion@gmail.com", UserId = 22}},
                RequestingUserId = 666
            };

            var objectRequestRepositoryMock = new Mock<IRepository<ObjectRequestRecord>>();
            objectRequestRepositoryMock.SetRecords(new[] {
                new ObjectRequestRecord {
                    AggregateId = e.ObjectRequestId,
                    Description = "Sneakers",
                    ExtraInfo = "For sneaking"
                }
            });

            var handler = new ReceivedObjectRequestReadModelGenerator(repositoryMock.Object, objectRequestRepositoryMock.Object);

            handler.Handle(e);

            record.Should().NotBeNull();
            record.UserId.Should().Be(22);
            record.Description.Should().Be("Sneakers");
            record.ExtraInfo.Should().Be("For sneaking");
            record.ReceivedDateTime.Should().Be(e.SentDateTime);
            record.RequestingUserId.Should().Be(666);
        }

        [Test]
        public void WhenObjectRequestStopped_RemoveRecords() {
            var repositoryMock = new Mock<IRepository<ReceivedObjectRequestRecord>>();
            var objectRequestId = Guid.NewGuid();

            var receivedObjectRequestRecord1 = new ReceivedObjectRequestRecord {
                UserId = 22,
                ObjectRequestId = objectRequestId
            };

            var receivedObjectRequestRecord2 = new ReceivedObjectRequestRecord
            {
                UserId = 23,
                ObjectRequestId = objectRequestId
            };

            var otherObjectRequestRecord = new ReceivedObjectRequestRecord
            {
                UserId = 23,
                ObjectRequestId = Guid.NewGuid()
            };

            repositoryMock.SetRecords(new[] {
                receivedObjectRequestRecord1,
                receivedObjectRequestRecord2,
                otherObjectRequestRecord
            });

            var e = new ObjectRequestStopped {
                SourceId = objectRequestId
            };

            var handler = new ReceivedObjectRequestReadModelGenerator(repositoryMock.Object, null);

            handler.Handle(e);

            repositoryMock.Verify(x => x.Delete(receivedObjectRequestRecord1));
            repositoryMock.Verify(x => x.Delete(receivedObjectRequestRecord2));
            repositoryMock.Verify(x => x.Delete(otherObjectRequestRecord), Times.Never);
        }
    }
}