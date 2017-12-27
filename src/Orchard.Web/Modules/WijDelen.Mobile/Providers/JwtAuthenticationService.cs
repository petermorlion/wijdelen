using System;
using System.Linq;
using System.Web;
using System.Web.Routing;
using Orchard.Environment.Extensions;
using Orchard.Mvc;
using Orchard.Mvc.Extensions;
using Orchard.Security;
using Orchard.Security.Providers;
using Orchard.Users.Models;

namespace WijDelen.Mobile.Providers {
    /// <summary>
    /// Allows authentication via a JSON Web Token, but falls back to Forms authentication if no header is present. Also disables/enables
    /// anti forgery validation based on the presence of this JSON Web Token. This is necessary because we're using MVC Controllers for
    /// calls from both the web app and the mobile app. This is the only place I could find where I can disable the anti forgery validation
    /// before it is performed in the AntiForgeryAuthorizationFilter (which we cannot replace because the OrchardFilterProvider provides
    /// all registered filters).
    /// </summary>
    public class JwtAuthenticationService : IAuthenticationService {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMembershipService _membershipService;
        private readonly IExtensionManager _extensionManager;
        private readonly FormsAuthenticationService _authenticationService;
        private IUser _signedInUser;

        public JwtAuthenticationService(IHttpContextAccessor httpContextAccessor, IMembershipService membershipService, IExtensionManager extensionManager, FormsAuthenticationService authenticationService) {
            _httpContextAccessor = httpContextAccessor;
            _membershipService = membershipService;
            _extensionManager = extensionManager;
            _authenticationService = authenticationService;
        }

        public void SignIn(IUser user, bool createPersistentCookie) {
            _authenticationService.SignIn(user, createPersistentCookie);
        }

        public void SignOut() {
            _authenticationService.SignOut();
        }

        public void SetAuthenticatedUserForRequest(IUser user) {
            _authenticationService.SetAuthenticatedUserForRequest(user);
        }

        public IUser GetAuthenticatedUser() {
            if (_signedInUser != null)
                return _signedInUser;

            var httpContext = _httpContextAccessor.Current();
            if (httpContext.IsBackgroundContext()) {
                return null;
            }
            
            var jwt = httpContext.Request.Headers["Authorization"];
            if (string.IsNullOrEmpty(jwt) || !jwt.Contains("Bearer ")) {
                EnableAntiForgery(httpContext);
                return _authenticationService.GetAuthenticatedUser();
            }

            jwt = jwt.Replace("Bearer ", "");
            var payload = JwtEncoder.DecodePayload(jwt);
            var userName = payload.UserName;

            _signedInUser = _membershipService.GetUser(userName);
            if (_signedInUser == null || !IsApproved(_signedInUser))
                return null;

            DisableAntiForgery(httpContext);

            return _signedInUser;
        }

        private void EnableAntiForgery(HttpContextBase httpContext) {
            var currentModule = GetArea(httpContext.Request.RequestContext.RouteData);
            if (!string.IsNullOrEmpty(currentModule)) {
                var extension = _extensionManager.AvailableExtensions().First(descriptor => string.Equals(descriptor.Id, currentModule, StringComparison.OrdinalIgnoreCase));
                extension.AntiForgery = "enabled";
            }
        }

        private void DisableAntiForgery(HttpContextBase httpContext) {
            var currentModule = GetArea(httpContext.Request.RequestContext.RouteData);
            if (!string.IsNullOrEmpty(currentModule)) {
                var extension = _extensionManager.AvailableExtensions().First(descriptor => string.Equals(descriptor.Id, currentModule, StringComparison.OrdinalIgnoreCase));
                extension.AntiForgery = "disabled";
            }
        }

        private bool IsApproved(IUser signedInUser) {
            var userPart = signedInUser as UserPart;

            return userPart != null && userPart.RegistrationStatus == UserStatus.Approved;
        }

        private static string GetArea(RouteData routeData) {
            if (routeData.Values.ContainsKey("area"))
                return routeData.Values["area"] as string;

            return routeData.DataTokens["area"] as string ?? "";
        }
    }
}