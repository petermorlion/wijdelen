using System.Net;
using System.Web;
using System.Web.Mvc;

namespace WijDelen.Mobile.Providers {
    /// <summary>
    /// Combines the traditional AuthorizeAttribute with custom logic to allow users to authorize
    /// with a JSON Web Token.
    /// </summary>
    public class HybridAuthorizeAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            var authorizationHeader = httpContext.Request.Headers["Authorization"];
            if (authorizationHeader != null && authorizationHeader.Contains("Bearer ")) {
                var jwt = authorizationHeader.Replace("Bearer ", "");
                return JwtEncoder.IsValid(jwt);
            }

            return base.AuthorizeCore(httpContext);
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext) {
            var authorizationHeader = filterContext.RequestContext.HttpContext.Request.Headers["Authorization"];
            if (authorizationHeader != null && authorizationHeader.Contains("Bearer ")) {
                filterContext.Result = new JsonResult { Data = "Invalid JSON Web Token.", JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                filterContext.RequestContext.HttpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                filterContext.RequestContext.HttpContext.Response.SuppressFormsAuthenticationRedirect = true;
            }
            else {
                base.HandleUnauthorizedRequest(filterContext);
            }
        }
    }
}