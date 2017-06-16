using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;

namespace WijDelen.UserImport {
    public class Permissions : IPermissionProvider {
        public static readonly Permission SendUserInvitationMails = new Permission {Description = "Send user invitation mails", Name = "SendUserInvitationMails"};

        public virtual Feature Feature { get; set; }

        public IEnumerable<Permission> GetPermissions() {
            return new[] {
                SendUserInvitationMails
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] {SendUserInvitationMails}
                },
                new PermissionStereotype {
                    Name = "PeergroupsAdministrator",
                    Permissions = new[] {SendUserInvitationMails}
                }
            };
        }
    }
}