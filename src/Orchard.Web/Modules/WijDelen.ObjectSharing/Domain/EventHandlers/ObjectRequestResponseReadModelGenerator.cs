using Orchard.Data;
using WijDelen.ObjectSharing.Domain.Enums;
using WijDelen.ObjectSharing.Domain.Events;
using WijDelen.ObjectSharing.Domain.Messaging;
using WijDelen.ObjectSharing.Models;

namespace WijDelen.ObjectSharing.Domain.EventHandlers {
    public class ObjectRequestResponseReadModelGenerator :
        IEventHandler<ObjectRequestConfirmed>,
        IEventHandler<ObjectRequestDenied>,
        IEventHandler<ObjectRequestDeniedForNow> {
        private readonly IRepository<ObjectRequestResponseRecord> _repository;

        public ObjectRequestResponseReadModelGenerator(IRepository<ObjectRequestResponseRecord> repository) {
            _repository = repository;
        }

        public void Handle(ObjectRequestConfirmed e) {
            _repository.Create(new ObjectRequestResponseRecord {
                ObjectRequestId = e.SourceId,
                UserId = e.ConfirmingUserId,
                Response = ObjectRequestAnswer.Yes
            });
        }

        public void Handle(ObjectRequestDenied e) {
            _repository.Create(new ObjectRequestResponseRecord {
                ObjectRequestId = e.SourceId,
                UserId = e.DenyingUserId,
                Response = ObjectRequestAnswer.No
            });
        }

        public void Handle(ObjectRequestDeniedForNow e) {
            _repository.Create(new ObjectRequestResponseRecord {
                ObjectRequestId = e.SourceId,
                UserId = e.DenyingUserId,
                Response = ObjectRequestAnswer.NotNow
            });
        }
    }
}