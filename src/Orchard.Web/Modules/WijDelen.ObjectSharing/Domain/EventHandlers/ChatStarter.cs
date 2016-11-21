using System;
using System.Linq;
using Orchard.Data;
using WijDelen.ObjectSharing.Domain.Entities;
using WijDelen.ObjectSharing.Domain.Events;
using WijDelen.ObjectSharing.Domain.EventSourcing;
using WijDelen.ObjectSharing.Domain.Messaging;
using WijDelen.ObjectSharing.Models;

namespace WijDelen.ObjectSharing.Domain.EventHandlers {
    /// <summary>
    /// Starts a chat when a user has confirmed an object request.
    /// </summary>
    public class ChatStarter : IEventHandler<ObjectRequestConfirmed> {
        private readonly IEventSourcedRepository<Chat> _chatRepository;
        private readonly IRepository<ObjectRequestRecord> _objectRequestRepository;

        public ChatStarter(IEventSourcedRepository<Chat> chatRepository, IRepository<ObjectRequestRecord> objectRequestRepository) {
            _chatRepository = chatRepository;
            _objectRequestRepository = objectRequestRepository;
        }

        public void Handle(ObjectRequestConfirmed e) {
            var objectRequestRecord = _objectRequestRepository.Fetch(x => x.AggregateId == e.SourceId).Single();
            var chat = new Chat(Guid.NewGuid(), e.SourceId, objectRequestRecord.UserId, e.ConfirmingUserId);
            _chatRepository.Save(chat, Guid.NewGuid().ToString());
        }
    }
}