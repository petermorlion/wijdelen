using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;

namespace WijDelen.UserImport {
    public class Permissions : IPermissionProvider {
        public static readonly Permission SendUserVerificationMails = new Permission {Description = "Send user verifiation mails", Name = "SendUserVerificationMails"};

        public virtual Feature Feature { get; set; }

        public IEnumerable<Permission> GetPermissions() {
            return new[] {
                SendUserVerificationMails
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] {SendUserVerificationMails}
                }
            };
        }
    }
}