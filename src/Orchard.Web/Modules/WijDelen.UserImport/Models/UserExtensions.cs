using Orchard.ContentManagement;
using Orchard.Security;

namespace WijDelen.UserImport.Models {
    public static class UserExtensions {
        /// <summary>
        /// Returns the name of a user to display in the UI of Peergroups. Takes the form of "FirstName LastName", unless
        /// this renders an empty string. In that case, we take the username.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static string GetUserDisplayName(this IUser user) {
            var formattedName = $"{user.As<UserDetailsPart>().FirstName} {user.As<UserDetailsPart>().LastName}";

            return string.IsNullOrWhiteSpace(formattedName) ? user.UserName : formattedName;
        }
    }
}