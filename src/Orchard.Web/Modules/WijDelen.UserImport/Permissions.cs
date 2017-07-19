using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;

namespace WijDelen.UserImport {
    public class Permissions : IPermissionProvider {
        public static readonly Permission SendUserInvitationMails = new Permission {Description = "Send user invitation mails", Name = "SendUserInvitationMails"};
        public static readonly Permission ImportUsers = new Permission {Description = "Import users", Name = "ImportUsers" };
        public static readonly Permission ManageGroups = new Permission {Description = "Manage groups", Name = "ManageGroups" };

        public virtual Feature Feature { get; set; }

        public IEnumerable<Permission> GetPermissions() {
            return new[] {
                SendUserInvitationMails,
                ImportUsers,
                ManageGroups
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] {SendUserInvitationMails, ImportUsers, ManageGroups}
                },
                new PermissionStereotype {
                    Name = "PeergroupsAdministrator",
                    Permissions = new[] {SendUserInvitationMails, ImportUsers, ManageGroups}
                }
            };
        }
    }
}