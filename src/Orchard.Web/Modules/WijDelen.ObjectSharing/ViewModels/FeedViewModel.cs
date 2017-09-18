using System.Collections.Generic;
using WijDelen.ObjectSharing.Models;

namespace WijDelen.ObjectSharing.ViewModels {
    public class FeedViewModel {
        public IList<FeedItemRecord> Items { get; set; }
    }
}