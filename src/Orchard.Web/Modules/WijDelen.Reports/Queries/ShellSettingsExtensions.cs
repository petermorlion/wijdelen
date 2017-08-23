using System;
using Orchard.Environment.Configuration;

namespace WijDelen.Reports.Queries {
    public static class ShellSettingsExtensions {
        public static string GetFullTableName(this ShellSettings shellSettings, Type type) {
            var tablePrefix = shellSettings.DataTablePrefix;
            var featurePrefix = type.Assembly.GetName().Name.Replace(".", "_");
            if (string.IsNullOrWhiteSpace(tablePrefix)) {
                return $"{featurePrefix}_{type.Name}";
            }

            return $"{tablePrefix}_{featurePrefix}_{type.Name}";
        }
    }
}