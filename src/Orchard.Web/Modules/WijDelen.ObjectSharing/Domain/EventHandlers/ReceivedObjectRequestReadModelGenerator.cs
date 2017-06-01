using Orchard.Data;
using WijDelen.ObjectSharing.Domain.Events;
using WijDelen.ObjectSharing.Domain.Messaging;
using WijDelen.ObjectSharing.Models;

namespace WijDelen.ObjectSharing.Domain.EventHandlers {
    public class ReceivedObjectRequestReadModelGenerator :
        IEventHandler<ObjectRequestMailSent>,
        IEventHandler<ObjectRequestConfirmed>,
        IEventHandler<ObjectRequestDenied>,
        IEventHandler<ObjectRequestDeniedForNow> {
        private readonly IRepository<ObjectRequestRecord> _objectRequestRepository;
        private readonly IRepository<ReceivedObjectRequestRecord> _repository;

        public ReceivedObjectRequestReadModelGenerator(IRepository<ReceivedObjectRequestRecord> repository, IRepository<ObjectRequestRecord> objectRequestRepository) {
            _repository = repository;
            _objectRequestRepository = objectRequestRepository;
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

        public void Handle(ObjectRequestMailSent e) {
            var objectRequest = _objectRequestRepository.Get(x => x.AggregateId == e.ObjectRequestId);

            foreach (var recipient in e.Recipients)
                _repository.Create(new ReceivedObjectRequestRecord {
                    UserId = recipient.UserId,
                    ObjectRequestId = e.ObjectRequestId,
                    Description = objectRequest.Description,
                    ExtraInfo = objectRequest.ExtraInfo,
                    ReceivedDateTime = e.SentDateTime,
                    RequestingUserId = e.RequestingUserId
                });
        }
    }
}