using System;
using System.Collections.Generic;

namespace WijDelen.Reports.ViewModels {
    public class RequestsViewModel {
        public IEnumerable<GroupViewModel> Groups { get; set; }
        public int SelectedGroupId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime StopDate { get; set; }
    }
}