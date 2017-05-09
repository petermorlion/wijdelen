using Orchard.ContentManagement;
using Orchard.Security;
using WijDelen.UserImport.Models;

namespace WijDelen.UserImport.Services {
    public class UpdateUserDetailsService : IUpdateUserDetailsService {
        public void UpdateUserDetails(IUser user, string firstName, string lastName, string culture) {
            user.As<UserDetailsPart>().FirstName = firstName;
            user.As<UserDetailsPart>().LastName = lastName;
            user.As<UserDetailsPart>().Culture = culture;
        }
    }
}