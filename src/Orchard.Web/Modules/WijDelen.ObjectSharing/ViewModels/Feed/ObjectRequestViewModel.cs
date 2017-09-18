using System;
using WijDelen.ObjectSharing.Domain.Enums;

namespace WijDelen.ObjectSharing.ViewModels.Feed {
    public class ObjectRequestViewModel : IFeedItemViewModel {
        public string UserName { get; set; }
        public string Description { get; set; }
        public DateTime DateTime { get; set; }
        public int ChatCount { get; set; }
        public string ExtraInfo { get; set; }
        public Guid ObjectRequestId { get; set; }

        /// <summary>
        /// Gets or sets what the current user replied to this object request, if any.
        /// </summary>
        public ObjectRequestAnswer? CurrentUsersResponse { get; set; }

        /// <summary>
        /// Gets or sets the id of the corresponding chat, if any.
        /// </summary>
        public Guid? ChatId { get; set; }
    }
}