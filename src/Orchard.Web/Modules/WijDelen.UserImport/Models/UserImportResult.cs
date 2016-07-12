using System.Collections.Generic;
using Orchard.Security;

namespace WijDelen.UserImport.Models {
    public class UserImportResult {
        private readonly IList<string> _errorMessages;

        public UserImportResult(string userName, string email) {
            UserName = userName;
            Email = email;

            _errorMessages = new List<string>();
        }

        public bool WasImported => User != null;

        public string UserName { get; private set; }
        public string Email { get; private set; }
        public IUser User { get; set; }

        public IEnumerable<string> ErrorMessages => _errorMessages;
        

        public void AddErrorMessage(string errorMessage) {
            _errorMessages.Add(errorMessage);
        }
    }
}