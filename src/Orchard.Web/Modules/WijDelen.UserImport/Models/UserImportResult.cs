namespace WijDelen.UserImport.Models {
    public class UserImportResult {
        public UserImportResult(string userName) {
            UserName = userName;
        }

        public bool IsValid { get; set; }
        public string UserName { get; private set; }
    }
}