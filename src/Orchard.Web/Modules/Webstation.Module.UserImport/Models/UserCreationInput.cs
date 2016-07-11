using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Webstation.Module.UserImport.Models
{
    public class UserCreationInput
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string PasswordQuestion { get; set; }
        public string PasswordAnswer { get; set; }
        public bool Approved { get; set; }
        public bool UpdateExisting { get; set; }
    }
}