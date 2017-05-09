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

        public IList<UserImportResult> ImportUsers(IList<string> emails) {
            var result = new List<UserImportResult>();

            foreach (var email in emails) {
                var userImportResult = new UserImportResult(email);
                var isValid = true;

                if (!string.IsNullOrEmpty(email) && !Regex.IsMatch(email, UserPart.EmailPattern)) {
                    userImportResult.AddErrorMessage(T("{0} is an invalid email address.", email).ToString());
                    isValid = false;
                }

                if (!string.IsNullOrEmpty(email) && !_userService.VerifyUserUnicity(email, email)) {
                    userImportResult.AddErrorMessage(T("User {0} already exists.", email).ToString());
                    isValid = false;
                }

                if (isValid) {
                    var newUser = _membershipService.CreateUser(new CreateUserParams(
                        email,
                        "",
                        email,
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