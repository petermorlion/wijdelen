using System.Collections.Generic;

namespace WijDelen.Mobile.Models {
    public class ErrorModel {
        public string Field { get; set; }
        public IList<string> Messages { get; set; }
    }
}