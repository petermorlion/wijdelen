using Orchard.DisplayManagement.Descriptors;

namespace Themes.WijDelen.Groups {
    public class MobileShapeTableProvider : IShapeTableProvider {
        public void Discover(ShapeTableBuilder builder) {
            builder.Describe("Layout")
                .OnDisplaying(displaying => {
                    displaying.ShapeMetadata.Alternates.Add("Layout-mobile");
                });
        }
    }
}