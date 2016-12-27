using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;

namespace WijDelen.ObjectSharing.Domain.Messaging {
    /// <summary>
    /// Message receiver that receives messages. Goes hand in hand with the InMemoryMessageSender. Uses .NET events.
    /// </summary>
    public class InMemoryMessageReceiver : IMessageReceiver, IDisposable {
        private readonly InMemoryMessageSender _messageSender;
        private readonly IDictionary<Type, IList<Action<IEvent>>> _eventHandlerActions = new Dictionary<Type, IList<Action<IEvent>>>();

        public InMemoryMessageReceiver(InMemoryMessageSender messageSender, IEnumerable<IEventHandler> eventHandlers) {
            _messageSender = messageSender;

            foreach (var eventHandler in eventHandlers) {
                RegisterEventHandler(eventHandler);
            }
        }

        public void Start() {
            _messageSender.SendingMessage += OnSendingMessage;
        }

        public void Stop() {
            _messageSender.SendingMessage -= OnSendingMessage;
        }

        private void RegisterEventHandler(IEventHandler eventHandler) {
            var eventHandlerType = eventHandler.GetType();
            if (typeof(IEventHandler<>).IsAssignableFrom(eventHandlerType)) {
                return;
            }

            var handleMethods = eventHandlerType
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => m.Name == "Handle" && m.GetParameters().Length == 1);

            foreach (var handleMethod in handleMethods) {
                var eventType = handleMethod.GetParameters().Single().ParameterType;
                if (!_eventHandlerActions.ContainsKey(eventType)) {
                    _eventHandlerActions[eventType] = new List<Action<IEvent>>();
                }

                _eventHandlerActions[eventType].Add(e => { handleMethod.Invoke(eventHandler, new[] {e}); });
            }
        }

        private void OnSendingMessage(object sender, SendingMessageEventArgs sendingMessageEventArgs) {
            var message = sendingMessageEventArgs.Message;

            var e = JsonConvert.DeserializeObject(message.Body, new JsonSerializerSettings {TypeNameHandling = TypeNameHandling.All});

            IList<Action<IEvent>> actions;
            _eventHandlerActions.TryGetValue(e.GetType(), out actions);
            if (actions == null || !actions.Any()) {
                return;
            }

            foreach (var action in actions) {
                action((IEvent) e);
            }
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            Stop();
        }

        ~InMemoryMessageReceiver() {
            Dispose(false);
        }
    }
}