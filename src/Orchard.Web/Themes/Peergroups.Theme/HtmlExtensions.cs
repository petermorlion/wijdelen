using System;
using System.Web.Mvc;

namespace Themes.WijDelen.Groups {
    public static class HtmlExtensions {
        public static bool IsCurrent(this HtmlHelper html, string actionName, string controllerName) {
            var contextAction = (string)html.ViewContext.RouteData.Values["action"];
            var contextController = (string)html.ViewContext.RouteData.Values["controller"];

            var isCurrent =
                string.Equals(contextAction, actionName, StringComparison.CurrentCultureIgnoreCase) &&
                string.Equals(contextController, controllerName, StringComparison.CurrentCultureIgnoreCase);

            return isCurrent;
        }
    }
}