using System;
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

            return 1;
        }

        public int UpdateFrom2() {
            SchemaBuilder.CreateTable(typeof(UnarchetypedSynonymRecord).Name, table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<string>("Synonym", column => column.Unique().NotNull())
            );

            SchemaBuilder.CreateTable(typeof(ItemArchetypeRecord).Name, table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<string>("Name", column => column.Unique().NotNull())
            );

            return 2;
        }
    }
}