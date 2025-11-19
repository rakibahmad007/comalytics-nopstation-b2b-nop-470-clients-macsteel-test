using FluentMigrator;
using Nop.Data.Mapping;
using Nop.Data.Migrations;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Data;

[NopMigration("2024/05/17 09:22:00", "NopStation.Plugin.B2B.ERPIntegrationCore schema migration after data migration", MigrationProcessType.Update)]
public class ErpNopUserUpdatesAfterDataMigrateSchemaMigration : AutoReversingMigration
{
    public override void Up()
    {
        var tableName = NameCompatibilityManager.GetTableName(typeof(ErpNopUser));

        if (!Schema.Table(tableName).Column(nameof(ErpNopUser.SalesOrgAsCustomerRoleId)).Exists())
        {
            Alter.Table(tableName)
                .AddColumn(nameof(ErpNopUser.SalesOrgAsCustomerRoleId))
                .AsString(100)
                .Nullable();
        }

        if (!Schema.Table(tableName).Column(nameof(ErpNopUser.RegistrationAuthorisedBy)).Exists())
        {
            Alter.Table(tableName)
                .AddColumn(nameof(ErpNopUser.RegistrationAuthorisedBy))
                .AsString(500)
                .Nullable();
        }

        if (!Schema.Table(tableName).Column(nameof(ErpNopUser.LastWarehouseCalculationTimeUtc)).Exists())
        {
            Alter.Table(tableName)
                .AddColumn(nameof(ErpNopUser.LastWarehouseCalculationTimeUtc))
                .AsDateTime2()
                .Nullable();
        }

        if (!Schema.Table(tableName).Column(nameof(ErpNopUser.TotalSavingsForthisYear)).Exists())
        {
            Alter.Table(tableName)
                .AddColumn(nameof(ErpNopUser.TotalSavingsForthisYear))
                .AsDecimal(18, 4)
                .Nullable();
        }

        if (!Schema.Table(tableName).Column(nameof(ErpNopUser.TotalSavingsForthisYearUpdatedOnUtc)).Exists())
        {
            Alter.Table(tableName)
                .AddColumn(nameof(ErpNopUser.TotalSavingsForthisYearUpdatedOnUtc))
                .AsDateTime2()
                .Nullable();
        }

        if (!Schema.Table(tableName).Column(nameof(ErpNopUser.TotalSavingsForAllTime)).Exists())
        {
            Alter.Table(tableName)
                .AddColumn(nameof(ErpNopUser.TotalSavingsForAllTime))
                .AsDecimal(18, 4)
                .Nullable();
        }

        if (!Schema.Table(tableName).Column(nameof(ErpNopUser.TotalSavingsForAllTimeUpdatedOnUtc)).Exists())
        {
            Alter.Table(tableName)
                .AddColumn(nameof(ErpNopUser.TotalSavingsForAllTimeUpdatedOnUtc))
                .AsDateTime2()
                .Nullable();
        }
    }
}