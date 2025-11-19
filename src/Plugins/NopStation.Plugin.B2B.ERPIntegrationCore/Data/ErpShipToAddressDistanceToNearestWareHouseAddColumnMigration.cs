using FluentMigrator;
using Nop.Data.Mapping;
using Nop.Data.Migrations;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Data;

[NopMigration("2024/12/30 10:24:12", "NopStation.Plugin.B2B.ERPIntegrationCore.Data ErpShipToAddress DistanceToNearestWareHouse AddColumn", MigrationProcessType.Update)]
public class ErpShipToAddressDistanceToNearestWareHouseAddColumnMigration : AutoReversingMigration
{
    #region Methods

    public override void Up()
    {
        if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(ErpShipToAddress)))
            .Column(nameof(ErpShipToAddress.DistanceToNearestWareHouse)).Exists())
        {
            Alter.Table(NameCompatibilityManager.GetTableName(typeof(ErpShipToAddress)))
                .AddColumn(nameof(ErpShipToAddress.DistanceToNearestWareHouse))
                .AsDouble()
                .Nullable();
        }
    }

    #endregion
}