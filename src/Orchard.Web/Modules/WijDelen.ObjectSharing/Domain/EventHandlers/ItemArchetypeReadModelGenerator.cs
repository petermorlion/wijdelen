using Orchard.Data;
using WijDelen.ObjectSharing.Domain.Events;
using WijDelen.ObjectSharing.Domain.Messaging;
using WijDelen.ObjectSharing.Models;

namespace WijDelen.ObjectSharing.Domain.EventHandlers {
    public class ItemArchetypeReadModelGenerator : IEventHandler<ItemArchetypeCreated> {
        private readonly IRepository<ItemArchetypeRecord> _repository;

        public ItemArchetypeReadModelGenerator(IRepository<ItemArchetypeRecord> repository) {
            _repository = repository;
        }

        public void Handle(ItemArchetypeCreated e) {
            var record = new ItemArchetypeRecord { Name = e.Name };
            _repository.Update(record);
        }
    }
}