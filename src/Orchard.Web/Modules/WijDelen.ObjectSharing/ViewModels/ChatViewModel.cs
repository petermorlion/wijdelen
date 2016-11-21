using System;
using System.Collections.Generic;

namespace WijDelen.ObjectSharing.ViewModels {
    public class ChatViewModel {
        public IList<ChatMessageViewModel> Messages { get; set; }
    }

    public class ChatMessageViewModel {
        public DateTime DateTime { get; set; }
        public string Message { get; set; }
        public string UserName { get; set; }
    }
}