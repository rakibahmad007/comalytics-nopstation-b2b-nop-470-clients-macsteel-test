using FluentMigrator;
using Nop.Data.Mapping;
using Nop.Data.Migrations;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Data;

[NopMigration("2024/12/15 17:25:00", "NopStation.Plugin.B2B.ERPIntegrationCore ErpOrderItemAdditionalData Adding columns", MigrationProcessType.Update)]
public class ErpOrderItemAdditionalDataMigrateSchemaMigration : AutoReversingMigration
{
    public override void Up()
    {
        if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(ErpAccount)))
            .Column(nameof(ErpOrderItemAdditionalData.DeliveryDate)).Exists())
        {
            Alter.Table(NameCompatibilityManager.GetTableName(typeof(ErpAccount)))
                .AddColumn(nameof(ErpOrderItemAdditionalData.DeliveryDate))
                .AsDateTime2()
                .Nullable();
        }

        if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(ErpAccount)))
            .Column(nameof(ErpOrderItemAdditionalData.NopWarehouseId)).Exists())
        {
            Alter.Table(NameCompatibilityManager.GetTableName(typeof(ErpAccount)))
                .AddColumn(nameof(ErpOrderItemAdditionalData.NopWarehouseId))
                .AsInt32()
                .Nullable();
        }

        if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(ErpAccount)))
            .Column(nameof(ErpOrderItemAdditionalData.SpecialInstruction)).Exists())
        {
            Alter.Table(NameCompatibilityManager.GetTableName(typeof(ErpAccount)))
                .AddColumn(nameof(ErpOrderItemAdditionalData.SpecialInstruction))
                .AsString(500)
                .Nullable();
        }
    }
}