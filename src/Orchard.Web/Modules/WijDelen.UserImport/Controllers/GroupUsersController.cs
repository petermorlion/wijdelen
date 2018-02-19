using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Mvc.Extensions;
using Orchard.Security;
using Orchard.Settings;
using Orchard.UI.Admin;
using Orchard.UI.Navigation;
using Orchard.UI.Notify;
using Orchard.Users.Models;
using WijDelen.UserImport.Models;
using WijDelen.UserImport.Services;
using WijDelen.UserImport.ViewModels;

namespace WijDelen.UserImport.Controllers {
    [Admin]
    public class GroupUsersController : Controller {
        private readonly IGroupService _groupService;
        private readonly IMailService _mailService;
        private readonly IOrchardServices _orchardServices;
        private readonly ISiteService _siteService;

        public GroupUsersController(IOrchardServices orchardServices, ISiteService siteService, IShapeFactory shapeFactory, IGroupService groupService, IMailService mailService) {
            _orchardServices = orchardServices;
            _siteService = siteService;
            _groupService = groupService;
            _mailService = mailService;

            Shape = shapeFactory;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        private dynamic Shape { get; }

        public ActionResult Index(PagerParameters pagerParameters, int selectedGroupId = 0) {
            if (!_orchardServices.Authorizer.Authorize(Permissions.ManageGroups, T("You are not authorized to view this page.")))
                return new HttpUnauthorizedResult();

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            var users = _orchardServices.ContentManager.Query<UserPart, UserPartRecord>().List().Select(x => x.ContentItem).ToList();
            
            if (selectedGroupId > 0) {
                users = users.Where(x => x?.As<GroupMembershipPart>()?.Group?.Id == selectedGroupId).ToList();
            }

            var pagerShape = Shape.Pager(pager).TotalItemCount(users.Count);

            var results = users
                .Skip(pager.GetStartIndex())
                .Take(pager.PageSize)
                .ToList();

            var groups = _groupService.GetGroups().OrderBy(x => x.Name).ToList();
            groups.Insert(0, new GroupViewModel { Id = 0, Name = T("All Groups").ToString() });

            var model = new GroupUsersIndexViewModel {
                Users = results
                    .Select(x => new GroupUserEntry {
                        User = x.As<UserPart>().Record,
                        GroupName = x.As<GroupMembershipPart>()?.Group?.As<NamePart>()?.Name,
                        GroupMembershipStatus = x.As<GroupMembershipPart>()?.GroupMembershipStatus ?? GroupMembershipStatus.Pending
                    })
                    .ToList(),
                Pager = pagerShape,
                Groups = groups
            };

            var routeData = new RouteData();
            routeData.Values.Add("SelectedGroupId", selectedGroupId);

            pagerShape.RouteData(routeData);

            return View(model);
        }

        [HttpPost]
        [Orchard.Mvc.FormValueRequired("submit.Filter")]
        public ActionResult Index(int selectedGroupId = 0) {
            return RedirectToAction("Index", new {selectedGroupId});
        }

        [HttpPost]
        [Orchard.Mvc.FormValueRequired("submit.ResendUserInvitationMails")]
        public ActionResult Index(string returnUrl, int selectedGroupId = 0)
        {
            if (selectedGroupId == 0) {
                _orchardServices.Notifier.Add(NotifyType.Warning, T("You must select a group. No mails were sent."));
                return RedirectToAction("Index", new { selectedGroupId });
            }

            return RedirectToAction("ConfirmResendUserInvitationMails", new { selectedGroupId, returnUrl });
        }

        public ActionResult ConfirmResendUserInvitationMails(string returnUrl, int selectedGroupId) {
            var groupViewModel = _groupService.GetGroups().Single(x => x.Id == selectedGroupId);
            var viewModel = new ConfirmResendUserInvitationMailsViewModel { GroupId = selectedGroupId, GroupName = groupViewModel.Name, ReturnUrl = returnUrl };
            return View(viewModel);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult ConfirmResendUserInvitationMails(ConfirmResendUserInvitationMailsViewModel viewModel) {
            var users = _groupService.GetUsersInGroup(viewModel.GroupId).Where(x => x.As<GroupMembershipPart>().GroupMembershipStatus == GroupMembershipStatus.Pending);
            var groupViewModel = _groupService.GetGroups().Single(x => x.Id == viewModel.GroupId);
            var siteUrl = _orchardServices.WorkContext.CurrentSite.BaseUrl;

            var usersByCulture = users.GroupBy(x => x.As<UserDetailsPart>()?.Culture);

            foreach (var group in usersByCulture) {
                _mailService.SendUserInvitationMails(group.Key, group, nonce => Url.MakeAbsolute(Url.Action("Index", "Register", new { Area = "WijDelen.UserImport", nonce }), siteUrl), groupViewModel.Name, groupViewModel.LogoUrl, viewModel.Text);
            }
            
            _orchardServices.Notifier.Add(NotifyType.Success, T("The invitation mails have been sent."));
            
            return Redirect(viewModel.ReturnUrl);
        }
    }
}