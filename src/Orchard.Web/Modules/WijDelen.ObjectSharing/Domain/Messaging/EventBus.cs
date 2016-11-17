using Newtonsoft.Json;

namespace WijDelen.ObjectSharing.Domain.Messaging {
    public class EventBus : IEventBus {
        private readonly IMessageSender _messageSender;

        public EventBus(IMessageSender messageSender) {
            _messageSender = messageSender;
        }

        public void Publish(IEvent e, string correlationId) {
            var messageBody = JsonConvert.SerializeObject(e, new JsonSerializerSettings {TypeNameHandling = TypeNameHandling.All});
            var message = new Message(messageBody, null, correlationId);
            _messageSender.Send(message);
        }
    }
}