using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;
using WijDelen.UserImport.Models;

namespace WijDelen.UserImport {
    public class Migrations : DataMigrationImpl {

        public int Create() {
            SchemaBuilder.CreateTable(typeof(NamePartRecord).Name, table => table
                .ContentPartRecord()
                .Column<string>("Name")
            );

            ContentDefinitionManager.AlterPartDefinition(
                typeof(NamePartRecord).Name, cfg => cfg.Attachable());

            return 1;
        }
    }
}