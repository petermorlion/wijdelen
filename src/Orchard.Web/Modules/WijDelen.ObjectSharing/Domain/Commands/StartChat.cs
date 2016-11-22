using System;
using WijDelen.ObjectSharing.Domain.Messaging;

namespace WijDelen.ObjectSharing.Domain.Commands {
    public class StartChat : ICommand {
        public Guid Id { get; }
        public Guid ObjectRequestId { get; set; }
        public int RequestingUserId { get; set; }
        public int ConfirmingUserId { get; set; }
    }
}