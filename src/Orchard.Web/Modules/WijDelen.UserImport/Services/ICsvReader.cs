using System.Collections.Generic;
using System.IO;
using Orchard;
using WijDelen.UserImport.Models;

namespace WijDelen.UserImport.Services {
    public interface ICsvReader : IDependency {
        IEnumerable<User> ReadUsers(Stream stream);
    }
}