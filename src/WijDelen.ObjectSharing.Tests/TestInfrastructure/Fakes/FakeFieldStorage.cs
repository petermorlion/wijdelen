using System.Collections.Generic;
using Orchard.ContentManagement.FieldStorage;

namespace WijDelen.ObjectSharing.Tests.TestInfrastructure.Fakes {
    public class FakeFieldStorage : IFieldStorage {
        private readonly IDictionary<string, object> _values = new Dictionary<string, object>();

        public T Get<T>(string name) {
            return (T) _values[name ?? "null"];
        }

        public void Set<T>(string name, T value) {
            _values[name ?? "null"] = value;
        }
    }
}