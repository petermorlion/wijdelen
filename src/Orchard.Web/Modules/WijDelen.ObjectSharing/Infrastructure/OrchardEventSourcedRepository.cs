using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters;
using Newtonsoft.Json;
using Orchard.Data;
using WijDelen.ObjectSharing.Domain.EventSourcing;
using WijDelen.ObjectSharing.Models;

namespace WijDelen.ObjectSharing.Infrastructure {
    public class OrchardEventSourcedRepository<T> : IEventSourcedRepository<T> where T : class, IEventSourced {
        private readonly IRepository<EventRecord> _orchardRepository;
        private readonly Func<Guid, IEnumerable<IVersionedEvent>, T> _entityFactory;
        private readonly JsonSerializerSettings _jsonSerializerSettings;

        public OrchardEventSourcedRepository(IRepository<EventRecord> orchardRepository) {
            _jsonSerializerSettings = new JsonSerializerSettings {
                TypeNameHandling = TypeNameHandling.All,
                TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple
            };

            _orchardRepository = orchardRepository;

            var constructor = typeof(T).GetConstructor(new[] { typeof(Guid), typeof(IEnumerable<IVersionedEvent>) });
            if (constructor == null)
            {
                throw new InvalidCastException("Type T must have a constructor with the following signature: .ctor(Guid, IEnumerable<IVersionedEvent>)");
            }

            _entityFactory = (id, events) => (T)constructor.Invoke(new object[] { id, events });
        }

        public T Find(Guid id) {
            var versionedEventRecords = _orchardRepository
                .Fetch(e => e.AggregateType == typeof(T).Name && e.AggregateId == id)
                .OrderBy(e => e.Version)
                .Select(Deserialize)
                .ToList();

            if (versionedEventRecords.Any()) {
                return _entityFactory.Invoke(id, versionedEventRecords);
            }

            return null;
        }

        private IVersionedEvent Deserialize(EventRecord e) {
            var deserializeObject = JsonConvert.DeserializeObject(e.Payload, _jsonSerializerSettings);
            return (IVersionedEvent)deserializeObject;
        }

        public void Save(T eventSourced, string correlationId) {
            var events = eventSourced.Events.Select(x => Serialize(x, correlationId)).ToList();
            events.ForEach(e => {
                _orchardRepository.Update(e);
            });

            // TODO: publish on eventbus
        }

        private EventRecord Serialize(IVersionedEvent e, string correlationId) {
            var versionedEventRecord = new EventRecord {
                AggregateId = e.SourceId,
                AggregateType = typeof(T).Name,
                Version = e.Version,
                Payload = JsonConvert.SerializeObject(e, _jsonSerializerSettings),
                CorrelationId = correlationId,
                Timestamp = DateTime.UtcNow
            };

            return versionedEventRecord;
        }
    }
}