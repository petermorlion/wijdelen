using Orchard.ContentManagement;
using Orchard.Security;
using WijDelen.MailChimp;
using WijDelen.UserImport.Models;

namespace WijDelen.UserImport.Services {
    public class UpdateUserDetailsService : IUpdateUserDetailsService {
        private readonly IMailChimpClient _mailChimpClient;

        public UpdateUserDetailsService(IMailChimpClient mailChimpClient) {
            _mailChimpClient = mailChimpClient;
        }

        public void UpdateUserDetails(IUser user, string firstName, string lastName, string culture, bool receiveMails, bool? isSubscribedToNewsletter = null) {
            user.As<UserDetailsPart>().FirstName = firstName;
            user.As<UserDetailsPart>().LastName = lastName;
            user.As<UserDetailsPart>().Culture = culture;
            user.As<UserDetailsPart>().ReceiveMails = receiveMails;

            if (!isSubscribedToNewsletter.HasValue) {
                return;
            }

            var groupName = user.As<GroupMembershipPart>()?.Group?.As<NamePart>()?.Name;

            if (isSubscribedToNewsletter.Value) {
                _mailChimpClient.Subscribe(user.Email, firstName, lastName, groupName);
            }
            else {
                _mailChimpClient.Unsubscribe(user.Email, firstName, lastName, groupName);
            }
        }
    }
}