using Orchard.ContentManagement;
using Orchard.Localization.Providers;
using Orchard.Security;
using Orchard.Users.Events;
using WijDelen.UserImport.Models;

namespace WijDelen.Localization {
    public class CultureSelectorUserEventHandler : IUserEventHandler {
        private readonly ICultureStorageProvider _cultureStorageProvider;

        public CultureSelectorUserEventHandler(ICultureStorageProvider cultureStorageProvider) {
            _cultureStorageProvider = cultureStorageProvider;
        }

        public void Creating(UserContext context) {
            
        }

        public void Created(UserContext context) {
            
        }

        public void LoggingIn(string userNameOrEmail, string password) {
            
        }

        public void LoggedIn(IUser user) {
            var userDetailsPart = user.As<UserDetailsPart>();
            if (userDetailsPart != null && !string.IsNullOrEmpty(userDetailsPart.Culture)) {
                _cultureStorageProvider.SetCulture(userDetailsPart.Culture);
            }
        }

        public void LogInFailed(string userNameOrEmail, string password) {
            
        }

        public void LoggedOut(IUser user) {
            
        }

        public void AccessDenied(IUser user) {
            
        }

        public void ChangedPassword(IUser user) {
            
        }

        public void SentChallengeEmail(IUser user) {
            
        }

        public void ConfirmedEmail(IUser user) {
            
        }

        public void Approved(IUser user) {
            
        }
    }
}