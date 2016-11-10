using Orchard.Data;
using WijDelen.ObjectSharing.Domain.Events;
using WijDelen.ObjectSharing.Domain.Messaging;
using WijDelen.ObjectSharing.Models;

namespace WijDelen.ObjectSharing.Domain.EventHandlers {
    public class ObjectRequestMailReadModelGenerator : IEventHandler<ObjectRequestMailSent> {
        private readonly IRepository<ObjectRequestMailRecord> _repository;

        public ObjectRequestMailReadModelGenerator(IRepository<ObjectRequestMailRecord> repository) {
            _repository = repository;
        }

        public void Handle(ObjectRequestMailSent objectRequestMailSent) {
            foreach (var recipient in objectRequestMailSent.Recipients) {
                var objectRequestMailRecord = new ObjectRequestMailRecord {
                    AggregateId = objectRequestMailSent.SourceId,
                    EmailAddress = recipient,
                    EmailHtml = objectRequestMailSent.EmailHtml,
                    RequestingUserId = objectRequestMailSent.RequestingUserId
                };

                _repository.Update(objectRequestMailRecord);
            }
        }
    }
}