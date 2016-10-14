using System.Linq;
using Orchard.Data;
using WijDelen.ObjectSharing.Domain.Events;
using WijDelen.ObjectSharing.Domain.Messaging;
using WijDelen.ObjectSharing.Models;

namespace WijDelen.ObjectSharing.Domain.EventHandlers {
    /// <summary>
    /// Generates the read model of an object request.
    /// </summary>
    public class ObjectRequestReadModelGenerator :
        IEventHandler<ObjectRequested> {
        private readonly IRepository<ObjectRequestRecord> _repository;

        public ObjectRequestReadModelGenerator(IRepository<ObjectRequestRecord> repository) {
            _repository = repository;
        }

        public void Handle(ObjectRequested e) {
            var existingRecord = _repository.Fetch(x => x.AggregateId == e.SourceId).SingleOrDefault();
            if (existingRecord == null) {
                var newRecord = new ObjectRequestRecord {
                    AggregateId = e.SourceId,
                    Description = e.Description,
                    ExtraInfo = e.ExtraInfo,
                    Version = e.Version,
                    UserId = e.UserId
                };

                _repository.Update(newRecord);
            }
        }
    }
}