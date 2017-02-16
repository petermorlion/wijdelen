using System.Collections.Generic;
using WijDelen.ObjectSharing.Models;

namespace WijDelen.ObjectSharing.ViewModels {
    public class ObjectRequestViewModel {
        public ObjectRequestRecord ObjectRequestRecord { get; set; }
        public IList<ChatRecord> ChatRecords { get; set; }

        /// <summary>
        /// The message to show to the user.
        /// </summary>
        public string Message { get; set; }
    }
}