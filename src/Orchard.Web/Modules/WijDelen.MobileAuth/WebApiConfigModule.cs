using System.Linq;
using System.Net.Http.Formatting;
using System.Web.Http;
using Autofac;
using Newtonsoft.Json.Serialization;

namespace WijDelen.MobileAuth {
    /// <summary>
    ///     This Autofac module ensures all WebApi results are returned using camelcasing.
    /// </summary>
    public class WebApiConfigModule : Module {
        protected override void Load(ContainerBuilder builder) {
            var jsonFormatter = GlobalConfiguration.Configuration.Formatters.OfType<JsonMediaTypeFormatter>().FirstOrDefault();

            if (jsonFormatter != null) jsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        }
    }
}