using FluentMigrator;
using Nop.Data.Mapping;
using Nop.Data.Migrations;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Data;

[NopMigration("2024/10/13 10:32:00", "NopStation.Plugin.B2B.ERPIntegrationCore schema migration after data migration", MigrationProcessType.Update)]
public class ErpSpecialPriceUpdatesAfterDataMigrateSchemaMigration : AutoReversingMigration
{
    public override void Up()
    {
        if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(ErpSpecialPrice)))
            .Column(nameof(ErpSpecialPrice.CustomerUoM)).Exists())
        {
            Alter.Table(NameCompatibilityManager.GetTableName(typeof(ErpSpecialPrice)))
                .AddColumn(nameof(ErpSpecialPrice.CustomerUoM))
                .AsString(500)
                .Nullable();
        }
    }
}