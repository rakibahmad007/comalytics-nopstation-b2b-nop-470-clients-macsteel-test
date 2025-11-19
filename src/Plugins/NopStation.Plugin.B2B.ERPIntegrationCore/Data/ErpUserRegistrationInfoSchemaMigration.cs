using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Mapping;
using Nop.Data.Migrations;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Data;

[NopMigration("2024/06/14 02:10:00", "NopStation.Plugin.B2B.ERPIntegrationCore schema migration after data migration", MigrationProcessType.Update)]
public class ErpUserRegistrationInfoSchemaMigration : AutoReversingMigration
{
    public override void Up()
    {
        if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(ErpUserRegistrationInfo))).Exists())
        {
            Create.TableFor<ErpUserRegistrationInfo>();
        }
    }
}