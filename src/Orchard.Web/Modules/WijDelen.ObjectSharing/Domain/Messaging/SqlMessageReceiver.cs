using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Orchard.Data;
using WijDelen.ObjectSharing.Models;

namespace WijDelen.ObjectSharing.Domain.Messaging {
    /// <summary>
    /// Message receiver that receives (i.e. reads) messages from the Orchard SQL database. Goes hand in hand with the SqlMessageSender.
    /// </summary>
    public class SqlMessageReceiver : IMessageReceiver, IDisposable {
        private readonly IRepository<MessageRecord> _repository;
        private readonly object _lockObject = new object();
        private CancellationTokenSource _cancellationSource;
        private readonly TimeSpan _pollDelay;
        private readonly IDictionary<Type, IList<Action<IEvent>>> _eventHandlerActions = new Dictionary<Type, IList<Action<IEvent>>>();

        public SqlMessageReceiver(IRepository<MessageRecord> repository, IEnumerable<IEventHandler> eventHandlers) {
            _repository = repository;
            _pollDelay = TimeSpan.FromMilliseconds(100);

            foreach (var eventHandler in eventHandlers)
            {
                RegisterEventHandler(eventHandler);
            }
        }

        public void Start() {
            lock (_lockObject)
            {
                if (_cancellationSource != null) return;

                _cancellationSource = new CancellationTokenSource();
                Task.Factory.StartNew(
                    () => ReceiveMessages(_cancellationSource.Token),
                    _cancellationSource.Token,
                    TaskCreationOptions.LongRunning,
                    TaskScheduler.Current);
            }
        }

        public void Stop() {
            lock (_lockObject)
            {
                using (_cancellationSource)
                {
                    if (_cancellationSource == null) return;

                    _cancellationSource.Cancel();
                    _cancellationSource = null;
                }
            }
        }

        private void RegisterEventHandler(IEventHandler eventHandler)
        {
            var eventHandlerType = eventHandler.GetType();
            if (typeof(IEventHandler<>).IsAssignableFrom(eventHandlerType))
            {
                return;
            }

            var handleMethods = eventHandlerType
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => m.Name == "Handle" && m.GetParameters().Length == 1);

            foreach (var handleMethod in handleMethods)
            {
                var eventType = handleMethod.GetParameters().Single().ParameterType;
                if (!_eventHandlerActions.ContainsKey(eventType))
                {
                    _eventHandlerActions[eventType] = new List<Action<IEvent>>();
                }

                _eventHandlerActions[eventType].Add(e => { handleMethod.Invoke(eventHandler, new[] { e }); });
            }
        }

        /// <summary>
        /// Receives the messages in an endless loop.
        /// </summary>
        private void ReceiveMessages(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (!ReceiveMessage())
                {
                    Thread.Sleep(_pollDelay);
                }
            }
        }

        /// <summary>
        /// Takes one message from the database and sends it to the appropriate event handlers.
        /// </summary>
        /// <returns>True if a message was handled, so the next message can be received. False if nothing was received, so we can wait for the duration of the pollDelay.</returns>
        private bool ReceiveMessage() {
            try {
                var messageRecord = _repository.Fetch(x => !x.DeliveryDate.HasValue || x.DeliveryDate < DateTime.UtcNow, orderable => orderable.Asc(record => record.Id), 0, 1)?.SingleOrDefault();
                if (messageRecord == null)
                {
                    return false;
                }

                var e = JsonConvert.DeserializeObject(messageRecord.Body, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });

                IList<Action<IEvent>> actions;
                _eventHandlerActions.TryGetValue(e.GetType(), out actions);
                if (actions == null || !actions.Any())
                {
                    return false;
                }

                foreach (var action in actions)
                {
                    action((IEvent)e);
                }

                _repository.Delete(messageRecord);

                return true;
            }
            catch (Exception) {
                return false;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            Stop();
        }

        ~SqlMessageReceiver()
        {
            Dispose(false);
        }
    }
}