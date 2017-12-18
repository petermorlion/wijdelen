using System;
using System.Globalization;
using System.IdentityModel.Tokens;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Web.Http;
using Newtonsoft.Json;
using Orchard.ContentManagement;
using Orchard.Security;
using Orchard.Users.Events;
using Orchard.Users.Models;
using WijDelen.MobileAuth.Models;

namespace WijDelen.MobileAuth.Controllers
{
    public class AuthController : ApiController
    {
        private readonly IUserEventHandler _userEventHandler;
        private readonly IMembershipService _membershipService;

        public AuthController(IUserEventHandler userEventHandler, IMembershipService membershipService)
        {
            _userEventHandler = userEventHandler;
            _membershipService = membershipService;
        }

        public HttpResponseMessage Post([FromBody]LoginModel loginModel)
        {
            _userEventHandler.LoggingIn(loginModel.UserNameOrEmail, loginModel.Password);

            var user = ValidateLogOn(loginModel.UserNameOrEmail, loginModel.Password);
            //if (!ModelState.IsValid)
            //{
            //    var shape = _orchardServices.New.LogOn().Title(T("Log On").Text);
            //    return new ShapeResult(this, shape);
            //}

            //_authenticationService.SignIn(user, rememberMe); // not necessary, return JWT
            _userEventHandler.LoggedIn(user);

            var header = new {
                alg = "HS256",
                typ = "JWT"
            };

            var firstPart = Base64UrlEncoder.Encode(JsonConvert.SerializeObject(header));

            var payload = new
            {
                userId = user.Id,
                userEmail = user.Email,
                userName = user.UserName
            };

            var secondPart = Base64UrlEncoder.Encode(JsonConvert.SerializeObject(payload));
            
            var secret = "<?3:c!6/MH.kkW.DVF(RCO#:]/$`^A";

            var hashImplementation = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var hashBytes = hashImplementation.ComputeHash(Encoding.UTF8.GetBytes($"{firstPart}.{secondPart}"));
            var hash = Base64UrlEncoder.Encode(hashBytes);

            return Request.CreateResponse(HttpStatusCode.OK, $"{firstPart}.{secondPart}.{hash}");
        }

        private IUser ValidateLogOn(string userNameOrEmail, string password)
        {
            bool validate = true;

            //if (string.IsNullOrEmpty(userNameOrEmail))
            //{
            //    ModelState.AddModelError("userNameOrEmail", T("You must specify a username or e-mail."));
            //    validate = false;
            //}
            //if (string.IsNullOrEmpty(password))
            //{
            //    ModelState.AddModelError("password", T("You must specify a password."));
            //    validate = false;
            //}

            if (!validate)
                return null;

            var user = _membershipService.ValidateUser(userNameOrEmail, password);
            //if (user == null)
            //{
            //    _userEventHandler.LogInFailed(userNameOrEmail, password);
            //    ModelState.AddModelError("_FORM", T("The username or e-mail or password provided is incorrect."));
            //}

            return user;
        }
    }
}