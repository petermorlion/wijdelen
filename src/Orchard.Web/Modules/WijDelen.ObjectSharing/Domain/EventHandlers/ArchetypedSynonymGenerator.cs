using System.Linq;
using Orchard.Data;
using WijDelen.ObjectSharing.Domain.Events;
using WijDelen.ObjectSharing.Domain.Messaging;
using WijDelen.ObjectSharing.Models;

namespace WijDelen.ObjectSharing.Domain.EventHandlers {
    /// <summary>
    /// Creates the read model for synonyms that haven't been assigned to an archetype yet.
    /// </summary>
    public class ArchetypedSynonymGenerator : IEventHandler<ObjectRequested> {
        private readonly IRepository<ArchetypedSynonymRecord> _repository;

        public ArchetypedSynonymGenerator(IRepository<ArchetypedSynonymRecord> repository) {
            _repository = repository;
        }

        public void Handle(ObjectRequested e) {
            if (_repository.Fetch(x => x.Synonym == e.Description).Any()) {
                return;
            }

            var record = new ArchetypedSynonymRecord {Synonym = e.Description};
            _repository.Update(record);
        }
    }
}