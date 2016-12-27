using System.Web.Mvc;
using Orchard.Mvc;
using Orchard.Security;
using Orchard.Users.Events;

namespace WijDelen.ObjectSharing {
    /// <summary>
    /// Custom UserEventHandler for WijDelen Groups that redirects a user to the homepage after logging out, and to the new object request page after logging in.
    /// </summary>
    public class RedirectingUserEventHandler : IUserEventHandler {
        private readonly IHttpContextAccessor _httpContext;

        public RedirectingUserEventHandler(IHttpContextAccessor httpContext) {
            _httpContext = httpContext;
        }

        public void Creating(UserContext context) {}

        public void Created(UserContext context) {}

        public void LoggingIn(string userNameOrEmail, string password) {}

        public void LoggedIn(IUser user) {
            var urlHelper = new UrlHelper(_httpContext.Current().Request.RequestContext);
            _httpContext.Current().Response.Redirect(urlHelper.Action("New", "ObjectRequest", new { area = "WijDelen.ObjectSharing" }));
        }

        public void LogInFailed(string userNameOrEmail, string password) {}

        public void LoggedOut(IUser user) {
            _httpContext.Current().Response.Redirect("~");
        }

        public void AccessDenied(IUser user) {}

        public void ChangedPassword(IUser user) {}

        public void SentChallengeEmail(IUser user) {}

        public void ConfirmedEmail(IUser user) {}

        public void Approved(IUser user) {}
    }
}