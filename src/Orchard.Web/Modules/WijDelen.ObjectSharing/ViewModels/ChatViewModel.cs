using System;
using System.Collections.Generic;

namespace WijDelen.ObjectSharing.ViewModels {
    public class ChatViewModel {
        public IList<ChatMessageViewModel> Messages { get; set; }

        public string NewMessage { get; set; }

        public Guid ChatId { get; set; }
    }
}