using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;

namespace WijDelen.ObjectSharing {
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission BlockObjectRequests = new Permission { Description = "Block Object Requests", Name = "BlockObjectRequests" };

        public virtual Feature Feature { get; set; }

        public IEnumerable<Permission> GetPermissions()
        {
            return new[] {
                BlockObjectRequests,
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] { BlockObjectRequests }
                },
                new PermissionStereotype {
                    Name = "PeergroupsAdministrator",
                    Permissions = new[] { BlockObjectRequests }
                },
            };
        }

    }
}