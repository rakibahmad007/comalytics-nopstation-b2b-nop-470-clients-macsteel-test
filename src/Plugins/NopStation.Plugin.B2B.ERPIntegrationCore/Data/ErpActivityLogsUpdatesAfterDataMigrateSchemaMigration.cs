using FluentMigrator;
using Nop.Data.Mapping;
using Nop.Data.Migrations;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Data;

[NopMigration("2024/04/13 09:33:00", "NopStation.Plugin.B2B.ERPIntegrationCore schema migration after data migration", MigrationProcessType.Update)]
public class ErpActivityLogsUpdatesAfterDataMigrateSchemaMigration : AutoReversingMigration
{
    public override void Up()
    {
        if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(ErpActivityLogs)))
            .Column(nameof(ErpActivityLogs.EntityDescription)).Exists())
        {
            Alter.Table(NameCompatibilityManager.GetTableName(typeof(ErpActivityLogs)))
                .AddColumn(nameof(ErpActivityLogs.EntityDescription))
                .AsString(500)
                .Nullable();
        }

        if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(ErpActivityLogs)))
            .Column(nameof(ErpActivityLogs.PropertyName)).Exists())
        {
            Alter.Table(NameCompatibilityManager.GetTableName(typeof(ErpActivityLogs)))
                .AddColumn(nameof(ErpActivityLogs.PropertyName))
                .AsString(200)
                .Nullable();
        }

        if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(ErpActivityLogs)))
            .Column(nameof(ErpActivityLogs.OldValue)).Exists())
        {
            Alter.Table(NameCompatibilityManager.GetTableName(typeof(ErpActivityLogs)))
                .AddColumn(nameof(ErpActivityLogs.OldValue))
                .AsString(int.MaxValue)
                .Nullable();
        }

        if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(ErpActivityLogs)))
            .Column(nameof(ErpActivityLogs.NewValue)).Exists())
        {
            Alter.Table(NameCompatibilityManager.GetTableName(typeof(ErpActivityLogs)))
                .AddColumn(nameof(ErpActivityLogs.NewValue))
                .AsString(int.MaxValue)
                .Nullable();
        }
    }
}