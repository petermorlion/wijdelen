using System;

namespace WijDelen.ObjectSharing.ViewModels.Feed {
    public class ChatMessageViewModel : IFeedItemViewModel {
        public Guid ChatId { get; set; }
        public string Description { get; set; }
        public string UserName { get; set; }
        public DateTime DateTime { get; set; }
    }
}