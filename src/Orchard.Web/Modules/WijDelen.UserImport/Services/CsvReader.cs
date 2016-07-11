using System.Collections.Generic;
using System.IO;
using WijDelen.UserImport.Models;

namespace WijDelen.UserImport.Services {
    public class CsvReader : ICsvReader {
        public IEnumerable<User> ReadUsers(Stream stream) {
            var result = new List<User>();
            using (var reader = new StreamReader(stream)) {
                var line = reader.ReadLine();
                while (line != null) {
                    var parts = line.Split(';');

                    if (parts[0] == "username") {
                        line = reader.ReadLine();
                        continue;
                    }

                    var user = new User
                    {
                        UserName = parts[0],
                        Email = parts[1]
                    };

                    result.Add(user);

                    line = reader.ReadLine();
                }
            }

            return result;
        }
    }
}