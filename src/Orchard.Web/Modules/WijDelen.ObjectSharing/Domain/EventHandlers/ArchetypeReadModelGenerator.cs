using Orchard.Data;
using WijDelen.ObjectSharing.Domain.Events;
using WijDelen.ObjectSharing.Domain.Messaging;
using WijDelen.ObjectSharing.Models;

namespace WijDelen.ObjectSharing.Domain.EventHandlers {
    public class ArchetypeReadModelGenerator : IEventHandler<ArchetypeCreated> {
        private readonly IRepository<ArchetypeRecord> _repository;

        public ArchetypeReadModelGenerator(IRepository<ArchetypeRecord> repository) {
            _repository = repository;
        }

        public void Handle(ArchetypeCreated e) {
            var record = new ArchetypeRecord { AggregateId = e.SourceId, Name = e.Name };
            _repository.Update(record);
        }
    }
}