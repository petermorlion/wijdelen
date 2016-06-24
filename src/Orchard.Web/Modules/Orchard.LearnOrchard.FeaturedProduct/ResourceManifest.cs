using Orchard.UI.Resources;

namespace Orchard.LearnOrchard.FeaturedProduct {
    public class ResourceManifest : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();
            manifest.DefineStyle("FeaturedProduct").SetUrl("FeaturedProduct.css");
        }
    }
}