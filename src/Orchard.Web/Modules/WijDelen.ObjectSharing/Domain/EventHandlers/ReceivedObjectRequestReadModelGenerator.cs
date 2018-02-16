using Orchard.Data;
using WijDelen.ObjectSharing.Domain.Events;
using WijDelen.ObjectSharing.Domain.Messaging;
using WijDelen.ObjectSharing.Models;

namespace WijDelen.ObjectSharing.Domain.EventHandlers {
    public class ReceivedObjectRequestReadModelGenerator :
        IEventHandler<ObjectRequestConfirmed>,
        IEventHandler<ObjectRequestDenied>,
        IEventHandler<ObjectRequestDeniedForNow>,
        IEventHandler<ObjectRequestStopped> {
        private readonly IRepository<ReceivedObjectRequestRecord> _repository;

        public ReceivedObjectRequestReadModelGenerator(IRepository<ReceivedObjectRequestRecord> repository) {
            _repository = repository;
        }

        public void Handle(ObjectRequestConfirmed e) {
            var record = _repository.Get(x => x.UserId == e.ConfirmingUserId && x.ObjectRequestId == e.SourceId);
            if (record == null) return;

            _repository.Delete(record);
        }

        public void Handle(ObjectRequestDenied e) {
            var record = _repository.Get(x => x.UserId == e.DenyingUserId && x.ObjectRequestId == e.SourceId);
            if (record == null)
                return;

            _repository.Delete(record);
        }

        public void Handle(ObjectRequestDeniedForNow e) {
            var record = _repository.Get(x => x.UserId == e.DenyingUserId && x.ObjectRequestId == e.SourceId);
            if (record == null)
                return;

            _repository.Delete(record);
        }

        public void Handle(ObjectRequestStopped e) {
            var objectRequests = _repository.Fetch(x => x.ObjectRequestId == e.SourceId);

            foreach (var objectRequestRecord in objectRequests) {
                _repository.Delete(objectRequestRecord);
            }
        }
    }
}