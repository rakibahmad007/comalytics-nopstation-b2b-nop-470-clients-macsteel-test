using FluentMigrator;
using Nop.Data.Mapping;
using Nop.Data.Migrations;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Data;

[NopMigration("2024/12/01 09:00:40", "NopStation.Plugin.B2B.ERPIntegrationCore schema migration after data migration", MigrationProcessType.Update)]
public class ErpOrderAdditionalDataUpdatesAfterDataMigrateSchemaMigration : AutoReversingMigration
{
    public override void Up()
    {
        if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(ErpOrderAdditionalData)))
            .Column(nameof(ErpOrderAdditionalData.ShippingCost)).Exists())
        {
            Alter.Table(NameCompatibilityManager.GetTableName(typeof(ErpOrderAdditionalData)))
                .AddColumn(nameof(ErpOrderAdditionalData.ShippingCost))
                .AsDecimal(18, 4)
                .Nullable();
        }

        if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(ErpOrderAdditionalData)))
            .Column(nameof(ErpOrderAdditionalData.PaygateReferenceNumber)).Exists())
        {
            Alter.Table(NameCompatibilityManager.GetTableName(typeof(ErpOrderAdditionalData)))
                .AddColumn(nameof(ErpOrderAdditionalData.PaygateReferenceNumber))
                .AsString(500)
                .Nullable();
        }

        if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(ErpOrderAdditionalData)))
            .Column(nameof(ErpOrderAdditionalData.CashRounding)).Exists())
        {
            Alter.Table(NameCompatibilityManager.GetTableName(typeof(ErpOrderAdditionalData)))
                .AddColumn(nameof(ErpOrderAdditionalData.CashRounding))
                .AsDecimal(18, 4)
                .Nullable();
        }
    }
}