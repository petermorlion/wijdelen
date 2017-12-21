using System.Net;
using System.Net.Http;
using System.Web.Http;
using Orchard.Security;
using Orchard.Users.Events;
using WijDelen.Mobile.Models;
using WijDelen.Mobile.Models.Jwt;
using WijDelen.Mobile.Providers;

namespace WijDelen.Mobile.Controllers {
    public class AuthController : ApiController {
        private readonly IMembershipService _membershipService;
        private readonly IUserEventHandler _userEventHandler;

        public AuthController(IUserEventHandler userEventHandler, IMembershipService membershipService) {
            _userEventHandler = userEventHandler;
            _membershipService = membershipService;
        }

        public HttpResponseMessage Post([FromBody] LoginModel loginModel) {
            _userEventHandler.LoggingIn(loginModel.UserNameOrEmail, loginModel.Password);

            var user = ValidateLogOn(loginModel.UserNameOrEmail, loginModel.Password);

            if (user == null) return Request.CreateResponse(HttpStatusCode.Unauthorized, "The provided username and/or password is incorrect.");

            _userEventHandler.LoggedIn(user);

            var payload = new Payload {
                UserId = user.Id,
                UserEmail = user.Email,
                UserName = user.UserName
            };

            var jwt = JwtEncoder.Encode(payload);

            return Request.CreateResponse(HttpStatusCode.OK, jwt);
        }

        private IUser ValidateLogOn(string userNameOrEmail, string password) {
            if (string.IsNullOrEmpty(userNameOrEmail)) return null;

            if (string.IsNullOrEmpty(password)) return null;

            var user = _membershipService.ValidateUser(userNameOrEmail, password);
            if (user == null) _userEventHandler.LogInFailed(userNameOrEmail, password);

            return user;
        }
    }
}