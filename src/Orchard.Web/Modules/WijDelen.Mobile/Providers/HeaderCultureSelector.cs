using System.Web;
using Orchard.Localization.Services;
using Orchard.Mvc;

namespace WijDelen.Mobile.Providers {
    public class HeaderCultureSelector : ICultureSelector {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HeaderCultureSelector(IHttpContextAccessor httpContextAccessor) {
            _httpContextAccessor = httpContextAccessor;
        }

        public CultureSelectorResult GetCulture(HttpContextBase context) {
            if (!IsAppRequest(context))
                return null;

            var httpContext = _httpContextAccessor.Current();

            var languageHeader = httpContext?.Request.Headers["Accept-Language"];

            if (languageHeader == null) return null;

            var culture = languageHeader;
            if (languageHeader.Contains(",")) culture = languageHeader.Substring(0, languageHeader.IndexOf(","));

            return new CultureSelectorResult {Priority = 1, CultureName = culture};
        }

        /// <summary>
        ///     Determines whether or not the current request comes from the mobile app.
        /// </summary>
        private bool IsAppRequest(HttpContextBase context) {
            return context.Request.RequestContext.HttpContext.Request.Headers["Authorization"] != null;
        }
    }
}