using System;
using Orchard.Alias;
using Orchard.ContentManagement;

namespace Orchard.LearnOrchard.FeaturedProduct.Services {
    public class FeaturedProductService : IFeaturedProductService {
        private readonly IContentManager _contentManager;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly IAliasService _aliasService;
        private IContent _currentContent = null;

        private IContent CurrentContent {
            get {
                if (_currentContent == null) {
                    var itemRoute = _aliasService.Get(_workContextAccessor.GetContext()
                        .HttpContext.Request.AppRelativeCurrentExecutionFilePath
                        .Substring(1).Trim('/'));
                    _currentContent = _contentManager
                        .Get(Convert.ToInt32(itemRoute["Id"]));
                }

                return _currentContent;
            }
        }

        public FeaturedProductService(IContentManager contentManager, IWorkContextAccessor workContextAccessor, IAliasService aliasService) {
            _contentManager = contentManager;
            _workContextAccessor = workContextAccessor;
            _aliasService = aliasService;
        }

        public bool IsOnFeaturedProductPage() {
            var isOnFeaturedProductPage = false;
            var itemTypeName = CurrentContent.ContentItem.TypeDefinition.Name;
            if (itemTypeName.Equals("Product",
                StringComparison.InvariantCultureIgnoreCase)) {
                var dynamicContentItem = (dynamic) CurrentContent.ContentItem;
                var itemProductId = dynamicContentItem.Product.ProductId.Value;
                if (itemProductId.Equals("SPROCKET9000",
                    StringComparison.InvariantCulture)) {
                    isOnFeaturedProductPage = true;
                }
            }

            return isOnFeaturedProductPage;
        }
    }
}