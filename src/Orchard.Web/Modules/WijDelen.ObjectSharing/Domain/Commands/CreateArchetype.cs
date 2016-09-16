using System;
using WijDelen.ObjectSharing.Domain.Messaging;

namespace WijDelen.ObjectSharing.Domain.Commands {
    public class CreateArchetype : ICommand {
        public string Name { get; set; }

        public Guid Id { get; }

        public Guid ArchetypeId { get; }

        public CreateArchetype(string name) {
            Id = Guid.NewGuid();
            ArchetypeId = Guid.NewGuid();
            Name = name;
        }
    }
}