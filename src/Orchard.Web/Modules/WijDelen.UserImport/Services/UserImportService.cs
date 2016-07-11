using System.Collections.Generic;
using Orchard.Security;
using Orchard.Users.Services;
using WijDelen.UserImport.Models;

namespace WijDelen.UserImport.Services {
    public class UserImportService : IUserImportService {
        private readonly IMembershipService _membershipService;

        public UserImportService(IMembershipService membershipService) {
            _membershipService = membershipService;
        }

        public IList<UserImportResult> ImportUsers(IList<User> users) {
            var result = new List<UserImportResult>();

            foreach (var user in users) {
                _membershipService.CreateUser(new CreateUserParams(
                    user.UserName,
                    "",
                    user.Email,
                    "",
                    "",
                    true));

                result.Add(new UserImportResult(user.UserName) { IsValid = true });
            }

            return result;
        }
    }
}