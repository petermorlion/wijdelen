using System;
using System.Data;
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
                .Column<string>("Payload", column => column.NotNull())
                .Column<string>("CorrelationId", column => column.NotNull())
            );

            SchemaBuilder.CreateTable(typeof(ObjectRequestRecord).Name, table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<Guid>("AggregateId", column => column.Unique().NotNull())
                    .Column<string>("Description", column => column.NotNull())
                    .Column<string>("ExtraInfo", column => column.NotNull())
                    .Column<int>("Version", column => column.NotNull())
                    .Column<int>("UserId", column => column.NotNull())
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

            return 1;
        }

        public int UpdateFrom1() {
            SchemaBuilder.AlterTable(typeof(EventRecord).Name, table => table
                .AlterColumn("Payload", column => column
                    .WithType(DbType.String)
                    .Unlimited())
            );

            return 2;
        }

        public int UpdateFrom2() {
            SchemaBuilder.CreateTable(typeof(ObjectRequestMailRecord).Name, table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<Guid>("AggregateId", column => column.Unique().NotNull())
                    .Column<string>("EmailAddress", column => column.NotNull().WithLength(255))
                    .Column<string>("EmailHtml", column => column.NotNull().Unlimited())
                    .Column<int>("RequestingUserId", column => column.NotNull())
            );

            return 3;
        }

        public int UpdateFrom3() {
            SchemaBuilder.CreateTable(typeof(MessageRecord).Name, table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<string>("Body", column => column.Unlimited())
                    .Column<string>("CorrelationId", column => column.Nullable())
                    .Column<DateTime>("DeliveryDate", column => column.Nullable())
            );

            return 4;
        }
    }
}