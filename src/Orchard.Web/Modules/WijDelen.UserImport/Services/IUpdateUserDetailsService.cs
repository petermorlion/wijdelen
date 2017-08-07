using Orchard;
using Orchard.Security;

namespace WijDelen.UserImport.Services {
    public interface IUpdateUserDetailsService : IDependency {
        void UpdateUserDetails(IUser user, string firstName, string lastName, string culture, bool receiveMails, bool isSubscribedToNewsletter);
    }
}