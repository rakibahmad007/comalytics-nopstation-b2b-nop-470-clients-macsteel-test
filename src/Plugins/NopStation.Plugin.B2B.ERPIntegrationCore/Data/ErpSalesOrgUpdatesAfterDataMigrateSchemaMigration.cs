using FluentMigrator;
using Nop.Data.Mapping;
using Nop.Data.Migrations;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Data;

[NopMigration("2024/12/26 10:31:42", "NopStation.Plugin.B2B.ERPIntegrationCore schema migration after data migration", MigrationProcessType.Update)]
public class ErpSalesOrgUpdatesAfterDataMigrateSchemaMigration : AutoReversingMigration
{
    public override void Up()
    {
        if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(ErpSalesOrg)))
            .Column(nameof(ErpSalesOrg.Suburb)).Exists())
        {
            Alter.Table(NameCompatibilityManager.GetTableName(typeof(ErpSalesOrg)))
                .AddColumn(nameof(ErpSalesOrg.Suburb))
                .AsString(500)
                .Nullable();
        }

        if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(ErpSalesOrg)))
            .Column(nameof(ErpSalesOrg.NoItemsMessage)).Exists())
        {
            Alter.Table(NameCompatibilityManager.GetTableName(typeof(ErpSalesOrg)))
                .AddColumn(nameof(ErpSalesOrg.NoItemsMessage))
                .AsString(500)
                .Nullable();
        }

        if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(ErpSalesOrg)))
            .Column(nameof(ErpSalesOrg.ShowWeightOnTheCheckoutScreen)).Exists())
        {
            Alter.Table(NameCompatibilityManager.GetTableName(typeof(ErpSalesOrg)))
                .AddColumn(nameof(ErpSalesOrg.ShowWeightOnTheCheckoutScreen))
                .AsBoolean()
                .NotNullable()
                .WithDefaultValue(false);
        }

        if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(ErpSalesOrg)))
            .Column(nameof(ErpSalesOrg.ServerBaseURL)).Exists())
        {
            Alter.Table(NameCompatibilityManager.GetTableName(typeof(ErpSalesOrg)))
                .AddColumn(nameof(ErpSalesOrg.ServerBaseURL))
                .AsString(500)
                .Nullable();
        }

        if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(ErpSalesOrg)))
            .Column(nameof(ErpSalesOrg.UserRegistrationEmailAdresses)).Exists())
        {
            Alter.Table(NameCompatibilityManager.GetTableName(typeof(ErpSalesOrg)))
                .AddColumn(nameof(ErpSalesOrg.UserRegistrationEmailAdresses))
                .AsString(500)
                .Nullable();
        }

        if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(ErpSalesOrg)))
            .Column(nameof(ErpSalesOrg.LastTimeSyncOnUtc)).Exists())
        {
            Alter.Table(NameCompatibilityManager.GetTableName(typeof(ErpSalesOrg)))
                .AddColumn(nameof(ErpSalesOrg.LastTimeSyncOnUtc))
                .AsDateTime2()
                .Nullable();
        }

        if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(ErpSalesOrg)))
            .Column(nameof(ErpSalesOrg.TradingWarehouseId)).Exists())
        {
            Alter.Table(NameCompatibilityManager.GetTableName(typeof(ErpSalesOrg)))
                .AddColumn(nameof(ErpSalesOrg.TradingWarehouseId))
                .AsInt32()
                .Nullable();
        }
    }
}