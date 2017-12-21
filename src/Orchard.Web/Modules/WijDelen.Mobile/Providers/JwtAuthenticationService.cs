using Orchard.Mvc;
using Orchard.Mvc.Extensions;
using Orchard.Security;
using Orchard.Security.Providers;
using Orchard.Users.Models;

namespace WijDelen.Mobile.Providers {
    public class JwtAuthenticationService : IAuthenticationService {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMembershipService _membershipService;
        private readonly FormsAuthenticationService _authenticationService;
        private IUser _signedInUser;

        public JwtAuthenticationService(IHttpContextAccessor httpContextAccessor, IMembershipService membershipService, FormsAuthenticationService authenticationService) {
            _httpContextAccessor = httpContextAccessor;
            _membershipService = membershipService;
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
                return _authenticationService.GetAuthenticatedUser();
            }

            jwt = jwt.Replace("Bearer ", "");
            var payload = JwtEncoder.DecodePayload(jwt);
            var userName = payload.UserName;

            _signedInUser = _membershipService.GetUser(userName);
            if (_signedInUser == null || !IsApproved(_signedInUser))
                return null;

            return _signedInUser;
        }

        private bool IsApproved(IUser signedInUser) {
            var userPart = signedInUser as UserPart;

            return userPart != null && userPart.RegistrationStatus == UserStatus.Approved;
        }
    }
}