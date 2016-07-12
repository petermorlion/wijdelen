using System.Collections.Generic;

namespace WijDelen.UserImport.Models {
    public class UserImportResult {
        private readonly IList<string> _errorMessages;

        public UserImportResult(string userName, string email) {
            UserName = userName;
            Email = email;
            LoginLink = "todo";

            _errorMessages = new List<string>();
        }

        public bool WasImported { get; set; }

        public string UserName { get; private set; }
        public string Email { get; private set; }
        public string LoginLink { get; private set; }

        public IEnumerable<string> ErrorMessages => _errorMessages;
        
        public void AddErrorMessage(string errorMessage) {
            _errorMessages.Add(errorMessage);
        }
    }
}