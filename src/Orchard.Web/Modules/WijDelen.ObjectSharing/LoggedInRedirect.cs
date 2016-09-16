using System.Web.Mvc;
using Orchard.Mvc;
using Orchard.Security;
using Orchard.Users.Events;

namespace WijDelen.ObjectSharing {
    public class LoggedInRedirect : IUserEventHandler {
        private readonly IHttpContextAccessor _httpContext;
        public LoggedInRedirect(IHttpContextAccessor httpContext)
        {
            _httpContext = httpContext;
        }
        public void Creating(UserContext context) {}

        public void Created(UserContext context) {}

        public void LoggingIn(string userNameOrEmail, string password) {}

        public void LoggedIn(IUser user) {
            var urlHelper = new UrlHelper(_httpContext.Current().Request.RequestContext);
            _httpContext.Current().Response.Redirect(urlHelper.Action("New", "ObjectRequest", new {area = "WijDelen.ObjectSharing" }));
        }

        public void LogInFailed(string userNameOrEmail, string password) {}

        public void LoggedOut(IUser user) {}

        public void AccessDenied(IUser user) {}

        public void ChangedPassword(IUser user) {}

        public void SentChallengeEmail(IUser user) {}

        public void ConfirmedEmail(IUser user) {}

        public void Approved(IUser user) {}
    }
}