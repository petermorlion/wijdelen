using Orchard.ContentManagement.Records;

namespace Orchard.LearnOrchard.FeaturedProduct.Models {
    public class FeaturedProductPartRecord : ContentPartRecord {
        public virtual bool IsOnSale { get; set; }
    }
}