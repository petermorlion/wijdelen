using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Roles.ViewModels;

namespace Webstation.Module.UserImport.ViewModels
{
    public static class UpdateExistingTypes
    {
        public static string Ignore = "Ignore";
        public static string AddRoles = "AddRoles";
    }

    public class AdminIndexViewModel
    {

        public AdminIndexViewModel()
        {
            Roles = new List<UserRoleEntry>();
        }

        public bool? Approve { get; set; }
        public string UpdateExisting { get; set; }
        public List<UserRoleEntry> Roles { get; set; }
    }
}