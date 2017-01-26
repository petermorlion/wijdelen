using System;
using System.Linq;
using Orchard.Data;
using WijDelen.ObjectSharing.Domain.Enums;
using WijDelen.ObjectSharing.Domain.Events;
using WijDelen.ObjectSharing.Domain.Messaging;
using WijDelen.ObjectSharing.Infrastructure.Queries;
using WijDelen.ObjectSharing.Models;

namespace WijDelen.ObjectSharing.Domain.EventHandlers {
    public class UserInventoryGenerator :
        IEventHandler<ObjectRequestConfirmed>,
        IEventHandler<ObjectRequestDenied>,
        IEventHandler<ObjectRequestDeniedForNow> {
        private readonly IRepository<ObjectRequestRecord> _objectRequestRepository;
        private readonly IFindSynonymsByExactMatchQuery _synonymQuery;
        private readonly IRepository<UserInventoryRecord> _userInventoryRepository;

        public UserInventoryGenerator(
            IRepository<ObjectRequestRecord> objectRequestRepository,
            IFindSynonymsByExactMatchQuery synonymQuery,
            IRepository<UserInventoryRecord> userInventoryRepository) {
            _objectRequestRepository = objectRequestRepository;
            _synonymQuery = synonymQuery;
            _userInventoryRepository = userInventoryRepository;
        }

        public void Handle(ObjectRequestConfirmed e) {
            Handle(e.ConfirmingUserId, e.SourceId, ObjectRequestAnswer.Yes);
        }

        public void Handle(ObjectRequestDenied e) {
            Handle(e.DenyingUserId, e.SourceId, ObjectRequestAnswer.No);
        }

        public void Handle(ObjectRequestDeniedForNow e) {
            Handle(e.DenyingUserId, e.SourceId, ObjectRequestAnswer.NotNow);
        }

        private void Handle(int userId, Guid objectRequestId, ObjectRequestAnswer answer) {
            var objectRequest = _objectRequestRepository.Get(x => x.AggregateId == objectRequestId);
            if (objectRequest == null)
                return;

            var synonyms = _synonymQuery.GetResults(objectRequest.Description).ToList();
            if (!synonyms.Any())
                return;

            var synonym = synonyms.First();

            var userInventoryItem = _userInventoryRepository.Get(x => x.UserId == userId && x.SynonymId == synonym.Id) ?? new UserInventoryRecord();

            userInventoryItem.UserId = userId;
            userInventoryItem.SynonymId = synonym.Id;
            userInventoryItem.Answer = answer;
            userInventoryItem.DateTimeAnswered = DateTime.UtcNow;

            _userInventoryRepository.Update(userInventoryItem);
        }
    }
}