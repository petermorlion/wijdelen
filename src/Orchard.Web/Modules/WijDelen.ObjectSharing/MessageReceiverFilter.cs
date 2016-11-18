using System.Web.Mvc;
using Orchard.Mvc.Filters;
using WijDelen.ObjectSharing.Domain.Messaging;

namespace WijDelen.ObjectSharing {
    /// <summary>
    /// This filter starts and stops the MessageReciever when an action is performed in the WijDelen.ObjectSharing area.
    /// This ensures that we can work with a transient MessageReceiver and MessageSender instead of a singleton, which
    /// leads to database deadlocks. If our events were handled asynchronously (e.g. via Azure, RabbitMQ,...) we wouldn't
    /// need this setup, but currently our events are handled synchronously.
    /// </summary>
    public class MessageReceiverFilter : FilterProvider, IActionFilter {
        private readonly IMessageReceiver _messageReceiver;

        public MessageReceiverFilter(IMessageReceiver messageReceiver) {
            _messageReceiver = messageReceiver;
        }

        public void OnActionExecuting(ActionExecutingContext filterContext) {
            if (filterContext.RouteData.DataTokens["area"].ToString() != "WijDelen.ObjectSharing") {
                return;
            }

            _messageReceiver.Start();
        }

        public void OnActionExecuted(ActionExecutedContext filterContext) {
            if (filterContext.RouteData.DataTokens["area"].ToString() != "WijDelen.ObjectSharing") {
                return;
            }

            _messageReceiver.Stop();
        }
    }
}