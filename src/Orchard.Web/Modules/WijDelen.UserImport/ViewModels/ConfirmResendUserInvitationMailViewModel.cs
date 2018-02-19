namespace WijDelen.UserImport.ViewModels {
    public class ConfirmResendUserInvitationMailViewModel {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string ReturnUrl { get; set; }
        public string Text { get; set; }
    }
}