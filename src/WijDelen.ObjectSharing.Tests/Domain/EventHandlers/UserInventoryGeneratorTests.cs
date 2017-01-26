using System;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Orchard.Data;
using WijDelen.ObjectSharing.Domain.Enums;
using WijDelen.ObjectSharing.Domain.EventHandlers;
using WijDelen.ObjectSharing.Domain.Events;
using WijDelen.ObjectSharing.Infrastructure.Queries;
using WijDelen.ObjectSharing.Models;
using WijDelen.ObjectSharing.Tests.TestInfrastructure.Factories;
using WijDelen.ObjectSharing.Tests.TestInfrastructure.Fakes;

namespace WijDelen.ObjectSharing.Tests.Domain.EventHandlers {
    [TestFixture]
    public class UserInventoryGeneratorTests {
        [Test]
        public void WhenObjectRequestConfirmed() {
            var objectRequestId = Guid.NewGuid();

            var e = new ObjectRequestConfirmed {
                ConfirmingUserId = 22,
                SourceId = objectRequestId
            };

            var objectRequestRepositoryMock = new Mock<IRepository<ObjectRequestRecord>>();
            objectRequestRepositoryMock.SetRecords(new[] {
                new ObjectRequestRecord {
                    AggregateId = objectRequestId,
                    Description = "Sneaky sneakers"
                }
            });

            var synonymFactory = new SynonymFactory();
            var existingSynonym = synonymFactory.Create("Sneaky sneakers");

            var synonymQuery = new Mock<IFindSynonymsByExactMatchQuery>();
            synonymQuery.Setup(x => x.GetResults("Sneaky sneakers")).Returns(new[] {existingSynonym});

            var userInventoryRepositoryMock = new Mock<IRepository<UserInventoryRecord>>();
            UserInventoryRecord record = null;
            userInventoryRepositoryMock
                .Setup(x => x.Update(It.IsAny<UserInventoryRecord>()))
                .Callback((UserInventoryRecord r) => record = r);

            var userInventoryGenerator = new UserInventoryGenerator(objectRequestRepositoryMock.Object, synonymQuery.Object, userInventoryRepositoryMock.Object);

            userInventoryGenerator.Handle(e);

            record.Should().NotBeNull();
            record.UserId.Should().Be(22);
            record.SynonymId.Should().Be(existingSynonym.Id);
            record.Answer.Should().Be(ObjectRequestAnswer.Yes);
            record.DateTimeAnswered.Should().NotBe(default(DateTime));
        }

        [Test]
        public void WhenObjectRequestConfirmedWithExistingAnswer()
        {
            var objectRequestId = Guid.NewGuid();

            var e = new ObjectRequestConfirmed
            {
                ConfirmingUserId = 22,
                SourceId = objectRequestId
            };

            var objectRequestRepositoryMock = new Mock<IRepository<ObjectRequestRecord>>();
            objectRequestRepositoryMock.SetRecords(new[] {
                new ObjectRequestRecord {
                    AggregateId = objectRequestId,
                    Description = "Sneaky sneakers"
                }
            });

            var synonymFactory = new SynonymFactory();
            var existingSynonym = synonymFactory.Create("Sneaky sneakers");

            var synonymQuery = new Mock<IFindSynonymsByExactMatchQuery>();
            synonymQuery.Setup(x => x.GetResults("Sneaky sneakers")).Returns(new[] { existingSynonym });

            var userInventoryRepositoryMock = new Mock<IRepository<UserInventoryRecord>>();
            userInventoryRepositoryMock
                .SetRecords(new[] {
                    new UserInventoryRecord {
                        Id = 666,
                        UserId = 22,
                        SynonymId = existingSynonym.Id,
                        Answer = ObjectRequestAnswer.No,
                        DateTimeAnswered = new DateTime(2015, 1, 1)
                    }
                });
            UserInventoryRecord record = null;
            userInventoryRepositoryMock
                .Setup(x => x.Update(It.IsAny<UserInventoryRecord>()))
                .Callback((UserInventoryRecord r) => record = r);

            var userInventoryGenerator = new UserInventoryGenerator(objectRequestRepositoryMock.Object, synonymQuery.Object, userInventoryRepositoryMock.Object);

            userInventoryGenerator.Handle(e);

            record.Should().NotBeNull();
            record.Id.Should().Be(666);
            record.UserId.Should().Be(22);
            record.SynonymId.Should().Be(existingSynonym.Id);
            record.Answer.Should().Be(ObjectRequestAnswer.Yes);
            record.DateTimeAnswered.Should().NotBe(default(DateTime)).And.NotBe(new DateTime(2015, 1, 1));
        }

        [Test]
        public void WhenObjectRequestDenied() {
            var objectRequestId = Guid.NewGuid();

            var e = new ObjectRequestDenied {
                DenyingUserId = 22,
                SourceId = objectRequestId
            };

            var objectRequestRepositoryMock = new Mock<IRepository<ObjectRequestRecord>>();
            objectRequestRepositoryMock.SetRecords(new[] {
                new ObjectRequestRecord {
                    AggregateId = objectRequestId,
                    Description = "Sneaky sneakers"
                }
            });

            var synonymFactory = new SynonymFactory();
            var existingSynonym = synonymFactory.Create("Sneaky sneakers");

            var synonymQuery = new Mock<IFindSynonymsByExactMatchQuery>();
            synonymQuery.Setup(x => x.GetResults("Sneaky sneakers")).Returns(new[] {existingSynonym});

            var userInventoryRepositoryMock = new Mock<IRepository<UserInventoryRecord>>();
            UserInventoryRecord record = null;
            userInventoryRepositoryMock
                .Setup(x => x.Update(It.IsAny<UserInventoryRecord>()))
                .Callback((UserInventoryRecord r) => record = r);

            var userInventoryGenerator = new UserInventoryGenerator(objectRequestRepositoryMock.Object, synonymQuery.Object, userInventoryRepositoryMock.Object);

            userInventoryGenerator.Handle(e);

            record.Should().NotBeNull();
            record.UserId.Should().Be(22);
            record.SynonymId.Should().Be(existingSynonym.Id);
            record.Answer.Should().Be(ObjectRequestAnswer.No);
            record.DateTimeAnswered.Should().NotBe(default(DateTime));
        }

        [Test]
        public void WhenObjectRequestDeniedWithExistingAnswer()
        {
            var objectRequestId = Guid.NewGuid();

            var e = new ObjectRequestDenied
            {
                DenyingUserId = 22,
                SourceId = objectRequestId
            };

            var objectRequestRepositoryMock = new Mock<IRepository<ObjectRequestRecord>>();
            objectRequestRepositoryMock.SetRecords(new[] {
                new ObjectRequestRecord {
                    AggregateId = objectRequestId,
                    Description = "Sneaky sneakers"
                }
            });

            var synonymFactory = new SynonymFactory();
            var existingSynonym = synonymFactory.Create("Sneaky sneakers");

            var synonymQuery = new Mock<IFindSynonymsByExactMatchQuery>();
            synonymQuery.Setup(x => x.GetResults("Sneaky sneakers")).Returns(new[] { existingSynonym });

            var userInventoryRepositoryMock = new Mock<IRepository<UserInventoryRecord>>();
            userInventoryRepositoryMock
                .SetRecords(new[] {
                    new UserInventoryRecord {
                        Id = 666,
                        UserId = 22,
                        SynonymId = existingSynonym.Id,
                        Answer = ObjectRequestAnswer.Yes,
                        DateTimeAnswered = new DateTime(2015, 1, 1)
                    }
                });
            UserInventoryRecord record = null;
            userInventoryRepositoryMock
                .Setup(x => x.Update(It.IsAny<UserInventoryRecord>()))
                .Callback((UserInventoryRecord r) => record = r);

            var userInventoryGenerator = new UserInventoryGenerator(objectRequestRepositoryMock.Object, synonymQuery.Object, userInventoryRepositoryMock.Object);

            userInventoryGenerator.Handle(e);

            record.Should().NotBeNull();
            record.Id.Should().Be(666);
            record.UserId.Should().Be(22);
            record.SynonymId.Should().Be(existingSynonym.Id);
            record.Answer.Should().Be(ObjectRequestAnswer.No);
            record.DateTimeAnswered.Should().NotBe(default(DateTime)).And.NotBe(new DateTime(2015, 1, 1));
        }

        [Test]
        public void WhenObjectRequestDeniedForNow() {
            var objectRequestId = Guid.NewGuid();

            var e = new ObjectRequestDeniedForNow {
                DenyingUserId = 22,
                SourceId = objectRequestId
            };

            var objectRequestRepositoryMock = new Mock<IRepository<ObjectRequestRecord>>();
            objectRequestRepositoryMock.SetRecords(new[] {
                new ObjectRequestRecord {
                    AggregateId = objectRequestId,
                    Description = "Sneaky sneakers"
                }
            });

            var synonymFactory = new SynonymFactory();
            var existingSynonym = synonymFactory.Create("Sneaky sneakers");

            var synonymQuery = new Mock<IFindSynonymsByExactMatchQuery>();
            synonymQuery.Setup(x => x.GetResults("Sneaky sneakers")).Returns(new[] {existingSynonym});

            var userInventoryRepositoryMock = new Mock<IRepository<UserInventoryRecord>>();
            UserInventoryRecord record = null;
            userInventoryRepositoryMock
                .Setup(x => x.Update(It.IsAny<UserInventoryRecord>()))
                .Callback((UserInventoryRecord r) => record = r);

            var userInventoryGenerator = new UserInventoryGenerator(objectRequestRepositoryMock.Object, synonymQuery.Object, userInventoryRepositoryMock.Object);

            userInventoryGenerator.Handle(e);

            record.Should().NotBeNull();
            record.UserId.Should().Be(22);
            record.SynonymId.Should().Be(existingSynonym.Id);
            record.Answer.Should().Be(ObjectRequestAnswer.NotNow);
            record.DateTimeAnswered.Should().NotBe(default(DateTime));
        }

        [Test]
        public void WhenObjectRequestDeniedForNowWithExistingAnswer()
        {
            var objectRequestId = Guid.NewGuid();

            var e = new ObjectRequestDeniedForNow
            {
                DenyingUserId = 22,
                SourceId = objectRequestId
            };

            var objectRequestRepositoryMock = new Mock<IRepository<ObjectRequestRecord>>();
            objectRequestRepositoryMock.SetRecords(new[] {
                new ObjectRequestRecord {
                    AggregateId = objectRequestId,
                    Description = "Sneaky sneakers"
                }
            });

            var synonymFactory = new SynonymFactory();
            var existingSynonym = synonymFactory.Create("Sneaky sneakers");

            var synonymQuery = new Mock<IFindSynonymsByExactMatchQuery>();
            synonymQuery.Setup(x => x.GetResults("Sneaky sneakers")).Returns(new[] { existingSynonym });

            var userInventoryRepositoryMock = new Mock<IRepository<UserInventoryRecord>>();
            userInventoryRepositoryMock
                .SetRecords(new[] {
                    new UserInventoryRecord {
                        Id = 666,
                        UserId = 22,
                        SynonymId = existingSynonym.Id,
                        Answer = ObjectRequestAnswer.Yes,
                        DateTimeAnswered = new DateTime(2015, 1, 1)
                    }
                });
            UserInventoryRecord record = null;
            userInventoryRepositoryMock
                .Setup(x => x.Update(It.IsAny<UserInventoryRecord>()))
                .Callback((UserInventoryRecord r) => record = r);

            var userInventoryGenerator = new UserInventoryGenerator(objectRequestRepositoryMock.Object, synonymQuery.Object, userInventoryRepositoryMock.Object);

            userInventoryGenerator.Handle(e);

            record.Should().NotBeNull();
            record.Id.Should().Be(666);
            record.UserId.Should().Be(22);
            record.SynonymId.Should().Be(existingSynonym.Id);
            record.Answer.Should().Be(ObjectRequestAnswer.NotNow);
            record.DateTimeAnswered.Should().NotBe(default(DateTime)).And.NotBe(new DateTime(2015, 1, 1));
        }
    }
}