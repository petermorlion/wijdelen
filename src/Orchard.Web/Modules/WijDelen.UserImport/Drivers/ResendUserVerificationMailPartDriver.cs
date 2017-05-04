using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Security;
using WijDelen.UserImport.Models;

namespace WijDelen.UserImport.Drivers {
    public class ResendUserVerificationMailPartDriver : ContentPartDriver<ResendUserVerificationMailPart> {
        private readonly IAuthenticationService _authenticationService;
        private readonly IAuthorizationService _authorizationService;
        private const string TemplateName = "Parts/ResendUserInvitationMail";

        public ResendUserVerificationMailPartDriver(
            IAuthenticationService authenticationService,
            IAuthorizationService authorizationService)
        {

            _authenticationService = authenticationService;
            _authorizationService = authorizationService;
            _authenticationService = authenticationService;
            _authorizationService = authorizationService;
        }

        protected override DriverResult Editor(ResendUserVerificationMailPart resendUserVerificationMailPart, dynamic shapeHelper)
        {
            // don't show editor without correct permission
            if (!_authorizationService.TryCheckAccess(Permissions.SendUserInvitationMails, _authenticationService.GetAuthenticatedUser(), resendUserVerificationMailPart))
                return null;

            return ContentShape("Parts_ResendUserVerificationMail_Edit",
                () => {
                    var model = resendUserVerificationMailPart.As<IUser>().UserName;
                    return shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: model, Prefix: Prefix);
                });
        }
    }
}