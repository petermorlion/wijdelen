using System.Web.Mvc;
using Moq;
using NUnit.Framework;
using WijDelen.ObjectSharing.Domain.Messaging;

namespace WijDelen.ObjectSharing.Tests {
    [TestFixture]
    public class MessageReceiverFilterTests {
        [Test]
        public void ShouldStartMessageReceiverOnExectingActionInObjectSharingArea() {
            var messageReceiverMock = new Mock<IMessageReceiver>();
            var messageRecieverFilter = new MessageReceiverFilter(messageReceiverMock.Object);
            var filterContext = new ActionExecutingContext();
            filterContext.RouteData.DataTokens["area"] = "WijDelen.ObjectSharing";

            messageRecieverFilter.OnActionExecuting(filterContext);

            messageReceiverMock.Verify(x => x.Start());
        }

        [Test]
        public void ShouldNotStartMessageReceiverOnExectingActionOutsideObjectSharingArea() {
            var messageReceiverMock = new Mock<IMessageReceiver>();
            var messageRecieverFilter = new MessageReceiverFilter(messageReceiverMock.Object);
            var filterContext = new ActionExecutingContext();
            filterContext.RouteData.DataTokens["area"] = "WijDelen.UserImport";

            messageRecieverFilter.OnActionExecuting(filterContext);

            messageReceiverMock.Verify(x => x.Start(), Times.Never);
        }

        [Test]
        public void ShouldStopMessageReceiverOnActionExecutedInObjectSharingArea() {
            var messageReceiverMock = new Mock<IMessageReceiver>();
            var messageRecieverFilter = new MessageReceiverFilter(messageReceiverMock.Object);
            var filterContext = new ActionExecutedContext();
            filterContext.RouteData.DataTokens["area"] = "WijDelen.ObjectSharing";

            messageRecieverFilter.OnActionExecuted(filterContext);

            messageReceiverMock.Verify(x => x.Stop());
        }

        [Test]
        public void ShouldNotStopMessageReceiverOnActionExecutedOutsideObjectSharingArea() {
            var messageReceiverMock = new Mock<IMessageReceiver>();
            var messageRecieverFilter = new MessageReceiverFilter(messageReceiverMock.Object);
            var filterContext = new ActionExecutedContext();
            filterContext.RouteData.DataTokens["area"] = "WijDelen.UserImport";

            messageRecieverFilter.OnActionExecuted(filterContext);

            messageReceiverMock.Verify(x => x.Stop(), Times.Never);
        }
    }
}