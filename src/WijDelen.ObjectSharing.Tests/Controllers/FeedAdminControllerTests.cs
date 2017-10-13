using System;
using Autofac;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using Orchard;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Notify;
using WijDelen.ObjectSharing.Controllers;
using WijDelen.ObjectSharing.Domain.EventHandlers;
using WijDelen.ObjectSharing.Domain.Events;
using WijDelen.ObjectSharing.Infrastructure;
using WijDelen.ObjectSharing.Models;
using WijDelen.ObjectSharing.Tests.TestInfrastructure.Fakes;

namespace WijDelen.ObjectSharing.Tests.Controllers {
    [TestFixture]
    public class FeedAdminControllerTests {
        private FeedAdminController _controller;
        private ObjectRequested _objectRequested;
        private ChatMessageAdded _chatMessageAdded;
        private ObjectRequestConfirmed _objectRequestConfirmed;
        private Mock<IFeedReadModelGenerator> _feedReadModelGenerator;
        private Mock<IRepository<FeedItemRecord>> _feedItemRecordRepositoryMock;
        private FeedItemRecord _existingFeedItemRecord;
        private ObjectRequestDenied _objectRequestDenied;
        private ObjectRequestDeniedForNow _objectRequestDeniedForNow;
        private ChatStarted _chatStarted;

        [SetUp]
        public void Init() {
            var serializerSettings = new VersionedEventSerializerSettings();

            var builder = new ContainerBuilder();

            _objectRequested = new ObjectRequested { SourceId = Guid.NewGuid() };
            _objectRequestDenied = new ObjectRequestDenied { SourceId = Guid.NewGuid() };
            _objectRequestDeniedForNow = new ObjectRequestDeniedForNow { SourceId = Guid.NewGuid() };
            _objectRequestConfirmed = new ObjectRequestConfirmed { SourceId = Guid.NewGuid() };
            _chatMessageAdded = new ChatMessageAdded { SourceId = Guid.NewGuid() };
            _chatStarted = new ChatStarted { SourceId = Guid.NewGuid() };
            var eventRepositoryMock = new Mock<IRepository<EventRecord>>();
            eventRepositoryMock.SetRecords(new[] {
                new EventRecord {
                    Payload = JsonConvert.SerializeObject(_objectRequested, serializerSettings)
                },
                new EventRecord {
                    Payload = JsonConvert.SerializeObject(_objectRequestConfirmed, serializerSettings)
                },
                new EventRecord {
                    Payload = JsonConvert.SerializeObject(_chatMessageAdded, serializerSettings)
                },
                new EventRecord {
                    Payload = JsonConvert.SerializeObject(_objectRequestDenied, serializerSettings)
                },
                new EventRecord {
                    Payload = JsonConvert.SerializeObject(_objectRequestDeniedForNow, serializerSettings)
                },
                new EventRecord {
                    Payload = JsonConvert.SerializeObject(_chatStarted, serializerSettings)
                }
            });

            _feedReadModelGenerator = new Mock<IFeedReadModelGenerator>();

            var authorizerMock = new Mock<IAuthorizer>();
            var notifierMock = new Mock<INotifier>();
            authorizerMock.Setup(x => x.Authorize(Permissions.ManageFeeds, It.IsAny<LocalizedString>())).Returns(true);
            var fakeOrchardServices = new FakeOrchardServices {Authorizer = authorizerMock.Object, Notifier = notifierMock .Object};

            _feedItemRecordRepositoryMock = new Mock<IRepository<FeedItemRecord>>();
            _existingFeedItemRecord = new FeedItemRecord();
            _feedItemRecordRepositoryMock.SetRecords(new[] { _existingFeedItemRecord });

            builder.RegisterInstance(fakeOrchardServices).As<IOrchardServices>();
            builder.RegisterInstance(_feedItemRecordRepositoryMock.Object).As<IRepository<FeedItemRecord>>();
            builder.RegisterInstance(_feedReadModelGenerator.Object).As<IFeedReadModelGenerator>();
            builder.RegisterInstance(eventRepositoryMock.Object).As<IRepository<EventRecord>>();
            builder.RegisterType<FeedAdminController>();

            var container = builder.Build();

            _controller = container.Resolve<FeedAdminController>();

            _controller.T = NullLocalizer.Instance;
        }

        [Test]
        public void Post_ShouldRebuildFeeds() {
            _controller.IndexPOST();

            _feedReadModelGenerator.Verify(x => x.Handle(It.Is((ObjectRequested e) => e.SourceId == _objectRequested.SourceId)));
            _feedReadModelGenerator.Verify(x => x.Handle(It.Is((ObjectRequestConfirmed e) => e.SourceId == _objectRequestConfirmed.SourceId)));
            _feedReadModelGenerator.Verify(x => x.Handle(It.Is((ObjectRequestDenied e) => e.SourceId == _objectRequestDenied.SourceId)));
            _feedReadModelGenerator.Verify(x => x.Handle(It.Is((ObjectRequestDeniedForNow e) => e.SourceId == _objectRequestDeniedForNow.SourceId)));
            _feedReadModelGenerator.Verify(x => x.Handle(It.Is((ChatMessageAdded e) => e.SourceId == _chatMessageAdded.SourceId)));
            _feedReadModelGenerator.Verify(x => x.Handle(It.Is((ChatStarted e) => e.SourceId == _chatStarted.SourceId)));

            _feedItemRecordRepositoryMock.Verify(x => x.Delete(_existingFeedItemRecord));
        }
    }
}