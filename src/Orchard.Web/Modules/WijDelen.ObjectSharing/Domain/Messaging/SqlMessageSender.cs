using Orchard.Data;
using WijDelen.ObjectSharing.Models;

namespace WijDelen.ObjectSharing.Domain.Messaging {
    /// <summary>
    /// Message sender that stores messages in a SQL database. To be used in conjunction with the SqlMessageReceiver.
    /// </summary>
    public class SqlMessageSender : IMessageSender {
        private readonly IRepository<MessageRecord> _repository;

        public SqlMessageSender(IRepository<MessageRecord> repository) {
            _repository = repository;
        }

        public void Send(Message message) {
            var messageRecord = new MessageRecord {
                Body = message.Body,
                CorrelationId = message.CorrelationId,
                DeliveryDate = message.DeliveryDate
            };

            _repository.Update(messageRecord);
        }
    }
}