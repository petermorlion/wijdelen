using System;
using Orchard.Data.Migration;
using WijDelen.ObjectSharing.Models;

namespace WijDelen.ObjectSharing {
    public class Migrations : DataMigrationImpl {

        public int Create() {
            SchemaBuilder.CreateTable(typeof(VersionedEventRecord).Name, table => table
                .Column<DateTime>("Timestamp", column => column.NotNull())
                .Column<int>("AggregateId", column => column.NotNull())
                .Column<string>("AggregateType", column => column.NotNull())
                .Column<int>("Version", column => column.NotNull())
                .Column<string>("Payload", column => column.NotNull())
                .Column<string>("CorrelationId", column => column.NotNull())
            );

            return 1;
        }
    }
}