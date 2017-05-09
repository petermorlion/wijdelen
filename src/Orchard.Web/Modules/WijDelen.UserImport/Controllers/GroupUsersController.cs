using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Security;
using Orchard.Users.Models;
using Orchard;
using Orchard.Settings;
using Orchard.UI.Admin;
using Orchard.UI.Navigation;
using WijDelen.UserImport.Services;
using WijDelen.UserImport.ViewModels;

namespace WijDelen.UserImport.Controllers {
    [Admin]
    public class GroupUsersController : Controller {
        private readonly IOrchardServices _orchardServices;
        private readonly ISiteService _siteService;
        private readonly IGroupService _groupService;

        public Localizer T { get; set; }
        dynamic Shape { get; set; }

        public GroupUsersController(IOrchardServices orchardServices, ISiteService siteService, IShapeFactory shapeFactory, IGroupService groupService) {
            _orchardServices = orchardServices;
            _siteService = siteService;
            _groupService = groupService;

            Shape = shapeFactory;
            T = NullLocalizer.Instance;
        }

        public ActionResult Index(PagerParameters pagerParameters) {
            if (!_orchardServices.Authorizer.Authorize(StandardPermissions.SiteOwner, T("You are not authorized to view this page.")))
                return new HttpUnauthorizedResult();

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            var users = _orchardServices.ContentManager.Query<UserPart, UserPartRecord>();

            var pagerShape = Shape.Pager(pager).TotalItemCount(users.Count());

            var results = users
                .Slice(pager.GetStartIndex(), pager.PageSize)
                .ToList();

            var groups = _groupService.GetGroups().OrderBy(x => x.Name).ToList();
            groups.Insert(0, new GroupViewModel());

            var model = new GroupUsersIndexViewModel {
                Users = results
                    .Select(x => new GroupUserEntry { User = x.Record })
                    .ToList(),
                Pager = pagerShape,
                Groups = groups
            };

            return View(model);
        }
    }
}