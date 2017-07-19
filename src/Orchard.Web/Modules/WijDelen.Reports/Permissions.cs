using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;

namespace WijDelen.Reports {
    public class Permissions : IPermissionProvider {
        public static readonly Permission ViewReports = new Permission {Description = "View Reports", Name = "ViewReports"};

        public virtual Feature Feature { get; set; }

        public IEnumerable<Permission> GetPermissions() {
            return new[] {
                ViewReports
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] {ViewReports}
                },
                new PermissionStereotype {
                    Name = "PeergroupsAdministrator",
                    Permissions = new[] {ViewReports}
                }
            };
        }
    }
}