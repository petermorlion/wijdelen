using System.Runtime.Serialization.Formatters;
using Newtonsoft.Json;

namespace WijDelen.ObjectSharing.Infrastructure {
    public class VersionedEventSerializerSettings : JsonSerializerSettings {
        public VersionedEventSerializerSettings() {
            TypeNameHandling = TypeNameHandling.All;
            TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple;
        }
    }
}