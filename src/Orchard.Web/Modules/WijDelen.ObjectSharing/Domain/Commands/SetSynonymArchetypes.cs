using System;
using System.Collections.Generic;
using WijDelen.ObjectSharing.Domain.Messaging;

namespace WijDelen.ObjectSharing.Domain.Commands {
    public class SetSynonymArchetypes : ICommand {
        public IDictionary<Guid, IList<string>> ArchetypeSynonyms { get; }

        public Guid Id { get; }
        
        public SetSynonymArchetypes(IDictionary<Guid, IList<string>> archetypeSynonyms) {
            ArchetypeSynonyms = archetypeSynonyms;
            Id = Guid.NewGuid();
        }
    }
}