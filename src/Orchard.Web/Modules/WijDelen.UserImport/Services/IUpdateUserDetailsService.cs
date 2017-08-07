using Orchard;
using Orchard.Security;

namespace WijDelen.UserImport.Services {
    public interface IUpdateUserDetailsService : IDependency {
        /// <summary>
        /// Updates the details of a user.
        /// </summary>
        /// <param name="user">The user to update the details of</param>
        /// <param name="firstName">The first name of the user</param>
        /// <param name="lastName">The last name of the user</param>
        /// <param name="culture">The culture of the user</param>
        /// <param name="receiveMails">Whether or not to receive emails about object requests and chats</param>
        /// <param name="isSubscribedToNewsletter">Whether or not to subscribe to the newsletter. Omit this parameter to avoid any changes</param>
        void UpdateUserDetails(IUser user, string firstName, string lastName, string culture, bool receiveMails, bool? isSubscribedToNewsletter = null);
    }
}