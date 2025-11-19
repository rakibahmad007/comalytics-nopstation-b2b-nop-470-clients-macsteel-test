using FluentMigrator;
using Nop.Data.Mapping;
using Nop.Data.Migrations;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Data;

[NopMigration("2024/08/11 05:37:00", "NopStation.Plugin.B2B.ERPIntegrationCore schema migration after data migration", MigrationProcessType.Update)]
public class ErpShipToAddressUpdatesAfterDataMigrateSchemaMigration : AutoReversingMigration
{
    public override void Up()
    {
        if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(ErpShipToAddress)))
            .Column(nameof(ErpShipToAddress.Comment)).Exists())
        {
            Alter.Table(NameCompatibilityManager.GetTableName(typeof(ErpShipToAddress)))
                .AddColumn(nameof(ErpShipToAddress.Comment))
                .AsString(500)
                .Nullable();
        }

        if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(ErpShipToAddress)))
            .Column(nameof(ErpShipToAddress.Latitude)).Exists())
        {
            Alter.Table(NameCompatibilityManager.GetTableName(typeof(ErpShipToAddress)))
                .AddColumn(nameof(ErpShipToAddress.Latitude))
                .AsString(200)
                .Nullable();
        }

        if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(ErpShipToAddress)))
            .Column(nameof(ErpShipToAddress.Longitude)).Exists())
        {
            Alter.Table(NameCompatibilityManager.GetTableName(typeof(ErpShipToAddress)))
                .AddColumn(nameof(ErpShipToAddress.Longitude))
                .AsString(200)
                .Nullable();
        }

        if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(ErpShipToAddress)))
            .Column(nameof(ErpShipToAddress.NearestWareHouseId)).Exists())
        {
            Alter.Table(NameCompatibilityManager.GetTableName(typeof(ErpShipToAddress)))
                .AddColumn(nameof(ErpShipToAddress.NearestWareHouseId))
                .AsInt32()
                .Nullable();
        }

        if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(ErpShipToAddress)))
            .Column(nameof(ErpShipToAddress.DeliveryOptionId)).Exists())
        {
            Alter.Table(NameCompatibilityManager.GetTableName(typeof(ErpShipToAddress)))
                .AddColumn(nameof(ErpShipToAddress.DeliveryOptionId))
                .AsInt32()
                .Nullable();
        }

        if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(ErpShipToAddress)))
            .Column(nameof(ErpShipToAddress.OrderId)).Exists())
        {
            Alter.Table(NameCompatibilityManager.GetTableName(typeof(ErpShipToAddress)))
                .AddColumn(nameof(ErpShipToAddress.OrderId))
                .AsInt32();
        }
    }
}