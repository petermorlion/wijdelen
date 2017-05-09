using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Mvc;
using Orchard.Security;
using Orchard.Settings;
using Orchard.UI.Admin;
using Orchard.UI.Navigation;
using Orchard.Users.Models;
using Orchard.Users.ViewModels;
using WijDelen.UserImport.Models;
using WijDelen.UserImport.Services;
using WijDelen.UserImport.ViewModels;

namespace WijDelen.UserImport.Controllers {
    [Admin]
    public class GroupUsersController : Controller {
        private readonly IGroupService _groupService;
        private readonly IOrchardServices _orchardServices;
        private readonly ISiteService _siteService;

        public GroupUsersController(IOrchardServices orchardServices, ISiteService siteService, IShapeFactory shapeFactory, IGroupService groupService) {
            _orchardServices = orchardServices;
            _siteService = siteService;
            _groupService = groupService;

            Shape = shapeFactory;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        private dynamic Shape { get; }

        public ActionResult Index(PagerParameters pagerParameters, int selectedGroupId = 0) {
            if (!_orchardServices.Authorizer.Authorize(StandardPermissions.SiteOwner, T("You are not authorized to view this page.")))
                return new HttpUnauthorizedResult();

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            var users = _orchardServices.ContentManager.Query<UserPart, UserPartRecord>().List().Select(x => x.ContentItem).ToList();
            
            if (selectedGroupId > 0) {
                users = users.Where(x => x.As<GroupMembershipPart>().Group.Id == selectedGroupId).ToList();
            }

            var pagerShape = Shape.Pager(pager).TotalItemCount(users.Count);

            var results = users
                .Skip(pager.GetStartIndex())
                .Take(pager.PageSize)
                .ToList();

            var groups = _groupService.GetGroups().OrderBy(x => x.Name).ToList();
            groups.Insert(0, new GroupViewModel());

            var model = new GroupUsersIndexViewModel {
                Users = results
                    .Select(x => new GroupUserEntry {User = x.As<UserPart>().Record})
                    .ToList(),
                Pager = pagerShape,
                Groups = groups
            };

            var routeData = new RouteData();
            routeData.Values.Add("SelectedGroupId", selectedGroupId);

            pagerShape.RouteData(routeData);

            return View(model);
        }
    }
}