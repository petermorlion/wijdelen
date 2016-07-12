using System.Web.Mvc;

namespace WijDelen.UserImport.Controllers {
    public class OneTimeLoginController : Controller {
        public ActionResult Index(string loginToken) {
            // check loginToken
            // check if already logged in
            // log in user
            // redirect to change password page
            return new RedirectResult(Url.Action("ChangePassword", "Account", new {area = "Orchard.Users"}));
        }
    }
}