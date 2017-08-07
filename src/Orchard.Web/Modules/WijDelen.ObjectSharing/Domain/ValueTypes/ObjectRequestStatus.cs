namespace WijDelen.ObjectSharing.Domain.ValueTypes {
    public enum ObjectRequestStatus {
        /// <summary>
        /// No specific status. None for lack of better words.
        /// </summary>
        None,

        /// <summary>
        /// Blocked because it contained forbidden words.
        /// </summary>
        BlockedForForbiddenWords,

        /// <summary>
        /// Blocked manually by an administrator
        /// </summary>
        BlockedByAdmin,

        /// <summary>
        /// Stopped by the user.
        /// </summary>
        Stopped
    }
}