namespace WijDelen.ObjectSharing.Domain.ValueTypes {
    /// <summary>
    /// A DTO class that specifies the email for a user id.
    /// </summary>
    public class UserEmail {
        public int UserId { get; set; }
        public string Email { get; set; }

        public override bool Equals(object obj) {
            var other = obj as UserEmail;

            if (other == null) {
                return false;
            }

            return other.UserId == UserId && other.Email == Email;
        }
    }
}