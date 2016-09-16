using System.Linq;
using WijDelen.ObjectSharing.Domain.Commands;
using WijDelen.ObjectSharing.Domain.Entities;
using WijDelen.ObjectSharing.Domain.EventSourcing;
using WijDelen.ObjectSharing.Domain.Messaging;

namespace WijDelen.ObjectSharing.Domain.CommandHandlers {
    public class ArchetypeCommandHandler : ICommandHandler<CreateArchetype>, ICommandHandler<SetSynonymArchetypes> {
        private readonly IEventSourcedRepository<Archetype> _repository;

        public ArchetypeCommandHandler(IEventSourcedRepository<Archetype> repository) {
            _repository = repository;
        }

        public void Handle(CreateArchetype command) {
            var archetype = new Archetype(command.ArchetypeId, command.Name);
            _repository.Save(archetype, command.Id.ToString());
        }

        public void Handle(SetSynonymArchetypes command) {
            foreach (var pair in command.ArchetypeSynonyms) {
                var archetype = _repository.Find(pair.Key);
                if (archetype == null) {
                    continue;
                }

                foreach (var synonym in pair.Value) {
                    if (!archetype.Synonyms.Contains(synonym))
                    {
                        archetype.AddSynonym(synonym);
                    }
                }

                var removedSynonyms = archetype.Synonyms.Except(pair.Value).ToList();
                foreach (var synonym in removedSynonyms) {
                    archetype.RemoveSynonym(synonym);
                }

                _repository.Save(archetype, command.Id.ToString());
            }
        }
    }
}