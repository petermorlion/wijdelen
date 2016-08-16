using System.Collections.Generic;

namespace WijDelen.UserImport.ViewModels {
    public class EditGroupMembershipViewModel {
        /// <summary>
        /// The Id of the User
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// The Id of the current Group
        /// </summary>
        public int GroupId { get; set; }

        /// <summary>
        /// The available Groups to choose from
        /// </summary>
        public IEnumerable<GroupViewModel> Groups { get; set; }
    }
}