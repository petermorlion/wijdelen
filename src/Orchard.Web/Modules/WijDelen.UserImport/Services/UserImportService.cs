using System.Collections.Generic;
using System.Text.RegularExpressions;
using Orchard.Localization;
using Orchard.Security;
using Orchard.Users.Models;
using Orchard.Users.Services;
using WijDelen.UserImport.Models;

namespace WijDelen.UserImport.Services {
    public class UserImportService : IUserImportService {
        private readonly IMembershipService _membershipService;
        private readonly IUserService _userService;

        public UserImportService(IMembershipService membershipService, IUserService userService) {
            _membershipService = membershipService;
            _userService = userService;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public IList<UserImportResult> ImportUsers(IList<User> users) {
            var result = new List<UserImportResult>();

            foreach (var user in users) {
                var userImportResult = new UserImportResult(user.UserName, user.Email);
                var isValid = true;

                if (string.IsNullOrEmpty(user.UserName)) {
                    userImportResult.AddErrorMessage(T("User with email {0} has no username.", user.Email).ToString());
                    isValid = false;
                }

                if (string.IsNullOrEmpty(user.Email))
                {
                    userImportResult.AddErrorMessage(T("User {0} has no email.", user.UserName).ToString());
                    isValid = false;
                }

                if (!string.IsNullOrEmpty(user.Email) && !Regex.IsMatch(user.Email, UserPart.EmailPattern)) {
                    userImportResult.AddErrorMessage(T("User {0} has an invalid email address.", user.UserName).ToString());
                    isValid = false;
                }

                if (!string.IsNullOrEmpty(user.UserName) && ! string.IsNullOrEmpty(user.Email) && !_userService.VerifyUserUnicity(user.UserName, user.Email)) {
                    userImportResult.AddErrorMessage(T("There is already a user with username {0} and/or email {1}.", user.UserName, user.Email).ToString());
                    isValid = false;
                }

                if (isValid) {
                    var newUser = _membershipService.CreateUser(new CreateUserParams(
                        user.UserName,
                        "",
                        user.Email,
                        "",
                        "",
                        true));

                    userImportResult.User = newUser;
                }

                result.Add(userImportResult);
            }

            return result;
        }
    }
}