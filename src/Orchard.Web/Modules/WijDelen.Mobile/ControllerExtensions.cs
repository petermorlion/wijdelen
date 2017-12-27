using System.Linq;
using System.Text;
using System.Web.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace WijDelen.Mobile {
    public static class ControllerExtensions {
        /// <summary>
        ///     Returns the MVC View or the given model as JSON, depending on the Accept header.
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="model"></param>
        /// <returns>Either a ContentResult with JSON, or a ViewResult (HTML).</returns>
        public static ActionResult ViewOrJson(this Controller controller, object model) {
            if (controller.Request.AcceptTypes.Contains("application/json")) {
                return new ContentResult {
                    ContentType = "application/json",
                    Content = JsonConvert.SerializeObject(model, new JsonSerializerSettings {ContractResolver = new CamelCasePropertyNamesContractResolver()}),
                    ContentEncoding = Encoding.UTF8
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