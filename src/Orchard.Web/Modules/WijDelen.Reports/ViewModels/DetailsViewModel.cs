using System;
using System.Collections.Generic;

namespace WijDelen.Reports.ViewModels {
    public class DetailsViewModel {
        public DateTime StartDate { get; set; }
        public DateTime StopDate { get; set; }
        public IEnumerable<GroupDetailsViewModel> GroupDetails { get; set; }
    }
}