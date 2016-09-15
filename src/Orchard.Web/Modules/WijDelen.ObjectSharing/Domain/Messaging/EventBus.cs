using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace WijDelen.ObjectSharing.Domain.Messaging {
    /// <summary>
    /// A crude implementation of IEventBus. In the future, it would be wise to move to a real event bus infrastructure, like that of Azure.
    /// Also because that would allow us to work asynchrounously (eventual consistency).
    /// </summary>
    public class EventBus : IEventBus {
        private readonly IDictionary<Type, IList<Action<IEvent>>> _eventHandlerActions = new Dictionary<Type, IList<Action<IEvent>>>();

        public EventBus(IEnumerable<IEventHandler> eventHandlers) {
            foreach (var eventHandler in eventHandlers) {
                Register(eventHandler);
            }
        }

        private void Register(IEventHandler eventHandler) {
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

                _eventHandlerActions[eventType].Add(e => { handleMethod.Invoke(eventHandler, new [] {e}); });
            }
        }

        public void Publish(IEvent e) {
            IList<Action<IEvent>> actions;
            _eventHandlerActions.TryGetValue(e.GetType(), out actions);
            if (actions == null || !actions.Any()) {
                return;
            }

            foreach (var action in actions) {
                action(e);
            }
        }
    }
}