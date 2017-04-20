using System;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;
using WijDelen.ObjectSharing.Models;

namespace WijDelen.ObjectSharing {
    public class Migrations : DataMigrationImpl {
        public int Create() {
            SchemaBuilder.CreateTable(typeof(EventRecord).Name, table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<DateTime>("Timestamp", column => column.NotNull())
                    .Column<Guid>("AggregateId", column => column.NotNull())
                    .Column<string>("AggregateType", column => column.NotNull())
                    .Column<int>("Version", column => column.NotNull())
                    .Column<string>("Payload", column => column.NotNull().Unlimited())
                    .Column<string>("CorrelationId", column => column.NotNull())
            );

            SchemaBuilder.CreateTable(typeof(ObjectRequestRecord).Name, table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<Guid>("AggregateId", column => column.Unique().NotNull())
                    .Column<string>("Description", column => column.NotNull())
                    .Column<string>("ExtraInfo", column => column.NotNull())
                    .Column<int>("Version", column => column.NotNull())
                    .Column<int>("UserId", column => column.NotNull())
                    .Column<DateTime>("CreatedDateTime")
            );

            ContentDefinitionManager.AlterPartDefinition("Archetype", builder =>
                    builder
                        .Attachable()
                        .WithField("Name", cfg => cfg.OfType("TextField").WithDisplayName("Name"))
            );

            ContentDefinitionManager.AlterTypeDefinition("Archetype", builder =>
                builder
                    .WithPart("CommonPart")
                    .WithPart("TitlePart")
                    .Creatable()
                    .Listable());

            ContentDefinitionManager.AlterPartDefinition("Synonym", builder =>
                    builder
                        .Attachable()
                        .WithField("Archetype", cfg => cfg
                            .OfType("ContentPickerField")
                            .WithDisplayName("Archetype")
                            .WithSetting("ContentPickerFieldSettings.DisplayedContentType", "Archetype")
                            .WithSetting("ContentPickerFieldSettings.Required", "False")
                            .WithSetting("ContentPickerFieldSettings.Multiple", "False")
                            .WithSetting("ContentPickerFieldSettings.ShowContentTab", "True")
                            .WithSetting("ContentPickerFieldSettings.DisplayedContentTypes", "Archetype"))
            );

            ContentDefinitionManager.AlterTypeDefinition("Synonym", builder =>
                builder
                    .WithPart("CommonPart")
                    .WithPart("TitlePart")
                    .WithPart("Synonym")
                    .Creatable()
                    .Listable());

            SchemaBuilder.CreateTable(typeof(ObjectRequestMailRecord).Name, table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<Guid>("AggregateId", column => column.NotNull())
                    .Column<string>("EmailAddress", column => column.NotNull().WithLength(255))
                    .Column<string>("EmailHtml", column => column.NotNull().Unlimited())
                    .Column<int>("RequestingUserId", column => column.NotNull())
                    .Column<int>("ReceivingUserId")
                    .Column<Guid>("ObjectRequestId")
                    .Column<DateTime>("SentDateTime")
            );

            SchemaBuilder.CreateTable(typeof(ChatMessageRecord).Name, table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<Guid>("ChatId", column => column.NotNull())
                    .Column<DateTime>("DateTime", column => column.NotNull())
                    .Column<int>("UserId", column => column.NotNull())
                    .Column<string>("UserName", column => column.NotNull())
                    .Column<string>("Message", column => column.NotNull().Unlimited())
            );

            SchemaBuilder.CreateTable(typeof(ChatRecord).Name, table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<Guid>("ChatId", column => column.NotNull())
                    .Column<Guid>("ObjectRequestId", column => column.NotNull())
                    .Column<int>("RequestingUserId", column => column.NotNull())
                    .Column<int>("ConfirmingUserId", column => column.NotNull())
                    .Column<string>("RequestingUserName", column => column.NotNull())
                    .Column<string>("ConfirmingUserName", column => column.NotNull())
            );

            SchemaBuilder.CreateTable(typeof(ObjectRequestResponseRecord).Name, table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<Guid>("ObjectRequestId", column => column.NotNull())
                    .Column<int>("UserId", column => column.NotNull())
                    .Column<string>("Response", column => column.NotNull().WithLength(50))
            );

            SchemaBuilder.CreateTable(typeof(ReceivedObjectRequestRecord).Name, table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<Guid>("ObjectRequestId", column => column.NotNull())
                    .Column<int>("UserId", column => column.NotNull())
                    .Column<string>("Description", column => column.NotNull())
                    .Column<string>("ExtraInfo", column => column.NotNull())
                    .Column<DateTime>("ReceivedDateTime", column => column.NotNull())
                    .Column<int>("RequestingUserId", column => column.NotNull())
            );

            return 9;
        }

        public int UpdateFrom9() {
            SchemaBuilder.CreateTable(typeof(UserInventoryRecord).Name, table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<int>("UserId", column => column.NotNull())
                    .Column<int>("SynonymId", column => column.NotNull())
                    .Column<string>("Answer", column => column.NotNull())
                    .Column<DateTime>("DateTimeAnswered", column => column.NotNull())
            );

            return 10;
        }

        public int UpdateFrom10() {
            SchemaBuilder.AlterTable(typeof(ObjectRequestResponseRecord).Name, table => table
                .AddColumn<DateTime>("DateTimeResponded"));

            SchemaBuilder.ExecuteSql($"UPDATE {SchemaBuilder.TableDbName(typeof(ObjectRequestResponseRecord).Name)} SET DateTimeResponded = SYSDATETIME()");
            SchemaBuilder.ExecuteSql($"ALTER TABLE {SchemaBuilder.TableDbName(typeof(ObjectRequestResponseRecord).Name)} ALTER COLUMN DateTimeResponded DATETIME NOT NULL");

            return 11;
        }

        public int UpdateFrom11() {
            SchemaBuilder.AlterTable(typeof(ObjectRequestRecord).Name, table => table
                .AddColumn<int>("GroupId"));
            SchemaBuilder.ExecuteSql($"UPDATE {SchemaBuilder.TableDbName(typeof(ObjectRequestRecord).Name)} SET GroupId = 0");
            SchemaBuilder.ExecuteSql($"ALTER TABLE {SchemaBuilder.TableDbName(typeof(ObjectRequestRecord).Name)} ALTER COLUMN GroupId INT NOT NULL");

            SchemaBuilder.AlterTable(typeof(ObjectRequestRecord).Name, table => table
                .AddColumn<string>("GroupName"));
            SchemaBuilder.ExecuteSql($"UPDATE {SchemaBuilder.TableDbName(typeof(ObjectRequestRecord).Name)} SET GroupName = ''");
            SchemaBuilder.ExecuteSql($"ALTER TABLE {SchemaBuilder.TableDbName(typeof(ObjectRequestRecord).Name)} ALTER COLUMN GroupName NVARCHAR(255) NOT NULL");

            return 12;
        }

        public int UpdateFrom12() {
            SchemaBuilder.AlterTable(typeof(ObjectRequestRecord).Name, table => table
                .AddColumn<string>("Status", column => column.WithLength(50)));

            return 13;
        }
    }
}