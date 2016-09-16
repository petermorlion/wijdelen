using WijDelen.ObjectSharing.Domain.Commands;
using WijDelen.ObjectSharing.Domain.Entities;
using WijDelen.ObjectSharing.Domain.EventSourcing;
using WijDelen.ObjectSharing.Domain.Messaging;

namespace WijDelen.ObjectSharing.Domain.CommandHandlers {
    public class ArchetypeCommandHandler : ICommandHandler<CreateArchetype> {
        private readonly IEventSourcedRepository<Archetype> _repository;

        public ArchetypeCommandHandler(IEventSourcedRepository<Archetype> repository) {
            _repository = repository;
        }

        public void Handle(CreateArchetype command) {
            var archetype = new Archetype(command.ArchetypeId, command.Name);
            _repository.Save(archetype, command.Id.ToString());
        }
    }
}