using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;

namespace WijDelen.ObjectSharing {
    public class Permissions : IPermissionProvider {
        public static readonly Permission BlockObjectRequests = new Permission {Description = "Block Object Requests", Name = "BlockObjectRequests"};
        public static readonly Permission ManageArchetypes = new Permission {Description = "Manage Archetypes", Name = "ManageArchetypes"};
        public static readonly Permission ManageObjectRequests = new Permission {Description = "Manage Object Request", Name = "ManageObjectRequests"};

        public virtual Feature Feature { get; set; }

        public IEnumerable<Permission> GetPermissions() {
            return new[] {
                BlockObjectRequests,
                ManageArchetypes,
                ManageObjectRequests
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] {BlockObjectRequests, ManageArchetypes, ManageObjectRequests}
                },
                new PermissionStereotype {
                    Name = "PeergroupsAdministrator",
                    Permissions = new[] {BlockObjectRequests, ManageArchetypes, ManageObjectRequests}
                }
            };
        }
    }
}