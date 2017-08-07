using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;
using Orchard.Roles.Services;
using WijDelen.UserImport.Models;

namespace WijDelen.UserImport {
    public class Migrations : DataMigrationImpl {
        private readonly IRoleService _roleService;

        public Migrations(IRoleService roleService) {
            _roleService = roleService;
        }

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

        public int UpdateFrom3() {
            SchemaBuilder.CreateTable("GroupMembershipPartRecord",
                table => table
                    .ContentPartRecord()
                    .Column<int>("Group_Id")
                );

            ContentDefinitionManager.AlterPartDefinition(
                "GroupMembershipPart", builder => builder.Attachable());

            return 4;
        }

        public int UpdateFrom4() {
            ContentDefinitionManager.AlterTypeDefinition("User", cfg => cfg
                .WithPart("GroupMembershipPart"));

            return 5;
        }

        public int UpdateFrom5()
        {
            ContentDefinitionManager.AlterPartDefinition("GroupLogoPart", builder =>
                builder
                    .Attachable()
                    .WithField("Group logo", cfg => cfg
                        .OfType("MediaLibraryPickerField")
                        .WithDisplayName("Group logo")));

            ContentDefinitionManager.AlterTypeDefinition("Group", builder =>
                builder
                    .WithPart("GroupLogoPart"));

            return 6;
        }

        public int UpdateFrom6() {
            SchemaBuilder.CreateTable(typeof(UserDetailsPartRecord).Name, table => table
                .ContentPartRecord()
                .Column<string>("FirstName")
                .Column<string>("LastName")
            );

            ContentDefinitionManager.AlterPartDefinition(
                typeof(UserDetailsPart).Name, cfg => cfg
                    .Attachable());

            ContentDefinitionManager.AlterTypeDefinition("User", cfg => cfg
                .WithPart("UserDetailsPart"));

            return 7;
        }

        public int UpdateFrom7() {
            ContentDefinitionManager.AlterPartDefinition(
                typeof(ResendUserVerificationMailPart).Name, cfg => cfg
                    .Attachable());

            ContentDefinitionManager.AlterTypeDefinition("User", cfg => cfg
                .WithPart("ResendUserVerificationMailPart"));

            return 8;
        }

        public int UpdateFrom8() {
            SchemaBuilder.AlterTable(typeof(UserDetailsPartRecord).Name, table => table
                .AddColumn<string>("Culture")
            );

            return 9;
        }

        public int UpdateFrom9() {
            SchemaBuilder.AlterTable(typeof(UserDetailsPartRecord).Name, table => table
                .AddColumn<bool>("ReceiveMails", column => column.WithDefault(true))
            );

            return 10;
        }

        public int UpdateFrom10() {
            SchemaBuilder.AlterTable(typeof(GroupMembershipPartRecord).Name, table => table
                .AddColumn<string>("GroupMembershipStatus", column => column.WithDefault("Approved").NotNull())
            );

            return 11;
        }

        public int UpdateFrom11() {
            _roleService.CreateRole("PeergroupsAdministrator");
            return 12;
        }

        public int UpdateFrom12() {
            SchemaBuilder.AlterTable(typeof(UserDetailsPartRecord).Name, table => table
                .AddColumn<bool>("IsSubscribedToNewsletter", column => column.WithDefault(false)));
            return 13;
        }
    }
}