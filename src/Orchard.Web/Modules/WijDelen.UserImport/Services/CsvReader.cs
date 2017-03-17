using System.Collections.Generic;
using System.IO;
using WijDelen.UserImport.Models;

namespace WijDelen.UserImport.Services {
    public class CsvReader : ICsvReader {
        public IList<User> ReadUsers(Stream stream) {
            var result = new List<User>();
            using (var reader = new StreamReader(stream)) {
                var line = reader.ReadLine();
                while (line != null) {
                    
                    if (line == "") {
                        line = reader.ReadLine();
                        continue;
                    }

                    var user = new User
                    {
                        UserName = line,
                        Email = line
                    };

                    result.Add(user);

                    line = reader.ReadLine();
                }
            }

            return result;
        }
    }
}