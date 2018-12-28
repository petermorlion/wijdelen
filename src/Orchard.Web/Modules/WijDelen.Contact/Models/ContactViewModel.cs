namespace WijDelen.Contact.Models {
    public class ContactViewModel {
        public string Email { get; set; }
        public string Subject { get; set; }
        public string Name { get; set; }
        public string Text { get; set; }

        /// <summary>
        /// Unused property, only so we can add a validation message in case the recaptcha wasn't valid.
        /// </summary>
        public string Recaptcha { get; set; }
    }
}