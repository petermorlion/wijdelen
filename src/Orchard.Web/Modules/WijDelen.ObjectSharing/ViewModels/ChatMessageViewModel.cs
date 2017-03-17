using System;

namespace WijDelen.ObjectSharing.ViewModels {
    public class ChatMessageViewModel {
        public DateTime DateTime { get; set; }
        public string Message { get; set; }
        public int UserId { get; set; }
    }
}