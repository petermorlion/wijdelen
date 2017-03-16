using System.Web.Mvc;
using Orchard.Localization.Providers;
using Orchard.Mvc.Extensions;

namespace WijDelen.Localization.Controllers {
    public class CultureController : Controller {
        private readonly ICultureStorageProvider _cultureStorageProvider;

        public CultureController(ICultureStorageProvider cultureStorageProvider) {
            _cultureStorageProvider = cultureStorageProvider;
        }

        public ActionResult ChangeCulture(string culture, string returnUrl) {
            _cultureStorageProvider.SetCulture(culture);

            return this.RedirectLocal(returnUrl);
        }
    }
}