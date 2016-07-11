using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Webstation.Module.UserImport.ViewModels;
using Orchard.Security;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Roles.Services;
using Orchard.Localization;
using Orchard.Roles.ViewModels;
using System.IO;
using System.Text.RegularExpressions;
using Webstation.Module.UserImport.Extensions;
using Webstation.Module.UserImport.Models;
using Orchard.Users.Models;
using Orchard.Data;
using Orchard.Roles.Models;
using Orchard.Users.Services;
using System.Text;
using System.Xml;
using Orchard.ContentManagement.FieldStorage.InfosetStorage;

namespace Webstation.Module.UserImport.Controllers
{
    public class AdminController : Controller
    {
        private IOrchardServices _services;
        private IMembershipService _membershipService;
        private IUserService _userService;
        private IRoleService _roleService;
        private IRepository<UserRolesPartRecord> _userRolesRepository;

        public AdminController(IOrchardServices services, IMembershipService memberService, IUserService userService,
            IRoleService roleService, IRepository<UserRolesPartRecord> userRolesRepository)
        {
            _services = services;
            _membershipService = memberService;
            _userService = userService;
            _roleService = roleService;
            _userRolesRepository = userRolesRepository;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public ActionResult Index()
        {
            if (!_services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to manage users")))
                return new HttpUnauthorizedResult();

            return View(new AdminIndexViewModel
            {
                Roles = _roleService.GetRoles()
                    .Where(role =>
                        !string.Equals(role.Name, "Authenticated", StringComparison.OrdinalIgnoreCase) &&
                        !string.Equals(role.Name, "Anonymous", StringComparison.OrdinalIgnoreCase))
                    .Select(role =>
                    new UserRoleEntry
                    {
                        RoleId = role.Id,
                        Name = role.Name,
                        Granted = false
                    }).ToList()
            });
        }

        [HttpPost, ActionName("Index")]
        public ActionResult IndexPOST(HttpPostedFileBase usersFile, AdminIndexViewModel model)
        {
            if (!_services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to manage users")))
                return new HttpUnauthorizedResult();

            if (usersFile == null)
            {
                _services.Notifier.Add(Orchard.UI.Notify.NotifyType.Error, T("Please select a user file"));
                return View(model);
            }

            string[] lines;

            using (StreamReader reader = new StreamReader(usersFile.InputStream))
                lines = Regex.Split(reader.ReadToEnd(), "\\r?\\n");

            if (!lines.Any())
            {
                _services.Notifier.Add(Orchard.UI.Notify.NotifyType.Error, T("User file contains no users"));
                return View(model);
            }

            string[] columnNames = lines.First().CsvSplit();

            //TODO finish this validation of input file
            //if (columnNames[0] != "Username")
            //{
            //    _services.Notifier.Add(Orchard.UI.Notify.NotifyType.Error, T("User file is not in correct format"));
            //    return View(model);
            //}

            List<UserCreationResult> allResults = new List<UserCreationResult>();

            lines
                .Skip(1) //Skip the header
                .ToList()
                .ForEach(l =>
                {
                    if (string.IsNullOrEmpty(l))
                        return;

                    var s = l.CsvSplit();
                    var input = new UserCreationInput
                    {
                        UserName = s[0],
                        Password = s[1],
                        Email = s[2],
                        PasswordQuestion = s[3],
                        PasswordAnswer = s[4]
                    };

                    bool flag;
                    input.Approved = bool.TryParse(s[5], out flag) ? flag : false;

                    var result = new UserCreationResult(input);

                    if (!string.IsNullOrEmpty(input.UserName))
                        if (!_userService.VerifyUserUnicity(input.UserName, input.Email))
                            result.AddError(T("User with that username and/or email already exists. {0} / {1}", input.UserName, input.Email));

                    if (!Regex.IsMatch(input.Email ?? "", UserPart.EmailPattern, RegexOptions.IgnoreCase))
                        result.AddError(T("You must specify a valid email address."));

                    IUser user = _services.ContentManager.New<IUser>("User");
                    if (result.Valid)
                    {
                        user = _membershipService.CreateUser(new CreateUserParams(
                                                            input.UserName,
                                                            input.Password,
                                                            input.Email,
                                                            input.PasswordQuestion == "" ? null : input.PasswordQuestion,
                                                            input.PasswordAnswer == "" ? null : input.PasswordAnswer,
                                                            model.Approve.HasValue ? model.Approve.Value : input.Approved));

                        //TODO this uses the Contrib.Profile module to add extra data to users
                        //using XMLWriter stuff here to avoid unnecessary whitepace XElement.ToString() caused
                        //var sbXml = new StringBuilder();
                        //using (var xwXml = XmlWriter.Create(sbXml, new XmlWriterSettings { OmitXmlDeclaration = true, Indent = false }))
                        //    new XElement("Data",
                        //        new XElement("ProfilePart",
                        //            //TODO Change to linq statement that adds all extra data fields in extra columns and appropriate data
                        //            new XElement(columnNames[6], s[6])
                        //        )
                        //    ).WriteTo(xwXml);

                        //var contentItem = user.Get<ContentItem>();
                        //contentItem.Record.Data = sbXml.ToString();

                        result.AddMessage(T("User {0} created", input.UserName));
                    }
                    else
                        user = model.UpdateExisting == UpdateExistingTypes.AddRoles
                            ? _membershipService.GetUser(input.UserName)
                            : null;

                    if (user != null)
                    {
                        var currentRoleRecords = _userRolesRepository
                            .Fetch(x => x.UserId == user.Id)
                            .Select(x => x.Role);
                        var targetRoleRecords = model.Roles.Where(x => x.Granted).Select(x => _roleService.GetRole(x.RoleId));
                        foreach (var addingRole in targetRoleRecords.Where(x => !currentRoleRecords.Contains(x)))
                        {
                            result.AddMessage(T("Adding role {0} to user {1}", addingRole.Name, input.UserName));
                            _userRolesRepository.Create(new UserRolesPartRecord { UserId = user.Id, Role = addingRole });
                        }
                    }

                    allResults.Add(result);
                });

            return View("ImportComplete", allResults);
        }
    }
}