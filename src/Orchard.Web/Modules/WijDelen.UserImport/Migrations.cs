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
                typeof(NamePartRecord).Name, cfg => cfg
                    .Attachable());

            return 1;
        }

        public int UpdateFrom1() {
            ContentDefinitionManager.AlterTypeDefinition("Group", builder =>
                builder
                    .WithPart("CommonPart")
                    .WithPart("NamePart")
                    .Creatable()
                    .Draftable());

            return 2;
        }

        public int UpdateFrom2() {
            ContentDefinitionManager.AlterPartDefinition(
                typeof(NamePartRecord).Name, cfg => cfg
                    .Attachable()
                    .WithDescription("Provides a name."));

            return 3;
        }
    }
}