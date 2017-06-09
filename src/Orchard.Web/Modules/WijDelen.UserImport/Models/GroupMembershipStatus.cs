namespace WijDelen.UserImport.Models {
    /// <summary>
    /// Possible options for the status of a group membership. When a user is created, his/her group membership status
    /// is Pending. Only when he/she clicks on the link in the email will the status change to Approved.
    /// </summary>
    public enum GroupMembershipStatus {
        /// <summary>
        /// The user hasn't yet clicked on the link in the invitation mail.
        /// </summary>
        Pending,

        /// <summary>
        /// The user has approved being a member of the group, by clicking on the link in the invitation mail.
        /// </summary>
        Approved
    }
}