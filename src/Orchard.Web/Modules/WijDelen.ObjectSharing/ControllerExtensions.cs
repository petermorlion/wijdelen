using System.Linq;
using System.Web.Mvc;

namespace WijDelen.ObjectSharing {
    public static class ControllerExtensions {
        /// <summary>
        /// Returns the MVC View or the given model as JSON, depending on the Accept header.
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public static ActionResult ViewOrJson(this Controller controller, object model) {
            if (controller.Request.AcceptTypes.Contains("application/json")) {
                return new JsonResult {
                    Data = model,
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }

            if (model != null)
                controller.ViewData.Model = model;

            return new ViewResult {
                ViewData = controller.ViewData,
                TempData = controller.TempData,
                ViewEngineCollection = controller.ViewEngineCollection
            };
        }
    }
}