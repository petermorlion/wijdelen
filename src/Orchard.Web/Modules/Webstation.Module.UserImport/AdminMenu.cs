using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.UI.Navigation;
using Orchard.Localization;
using Orchard.Security;

namespace Webstation.Module.UserImport
{
    public class AdminMenu : INavigationProvider
    {
        #region INavigationProvider Members

        public Localizer T { get; set; }
        public void GetNavigation(NavigationBuilder builder)
        {
            builder.Add(T("User Import"), "12",
                menu => menu.Add(T("Import Users"), item => item
                    .Action("Index", "Admin", new { area = "Webstation.Module.UserImport" })
                    .LocalNav().Permission(StandardPermissions.SiteOwner)));
        }

        public string MenuName
        {
            get { return "admin"; }
        }

        #endregion
    }
}