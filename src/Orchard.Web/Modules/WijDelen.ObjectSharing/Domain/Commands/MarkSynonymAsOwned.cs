using System;
using WijDelen.ObjectSharing.Domain.Messaging;

namespace WijDelen.ObjectSharing.Domain.Commands {
    /// <summary>
    /// When a user receives an object request mail, we usually don't know which archetype it is yet. So when the user clicks yes, 
    /// we can only mark the synonym as owned.
    /// </summary>
    public class MarkSynonymAsOwned : ICommand {
        public MarkSynonymAsOwned(int userId, int synonymId, string synonymTitle) {
            Id = Guid.NewGuid();
            UserId = userId;
            SynonymId = synonymId;
            SynonymTitle = synonymTitle;
        }

        public Guid Id { get; }
        public int UserId { get; }
        public int SynonymId { get; set; }
        public string SynonymTitle { get; set; }
    }
}