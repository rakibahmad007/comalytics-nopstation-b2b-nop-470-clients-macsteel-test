using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Mapping;
using Nop.Data.Migrations;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Data;

[NopMigration("2024/05/04 09:33:00", "NopStation.Plugin.B2B.ERPIntegrationCore schema migration for customer configuration", MigrationProcessType.Update)]
public class ErpCustomerConfigurationSchemaMigration : AutoReversingMigration
{
    public override void Up()
    {
        if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(ErpCustomerConfiguration))).Exists())
        {
            Create.TableFor<ErpCustomerConfiguration>();
        }
    }
}