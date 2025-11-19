using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Mapping;
using Nop.Data.Migrations;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Data;

[NopMigration("2025/07/21 02:10:00", "NopStation.Plugin.B2B.ERPIntegrationCore ErpUserType Add Column", MigrationProcessType.Update)]
public class ErpUserRegistrationInfoAddColumnErpUserTypeMigration : AutoReversingMigration
{
    public override void Up()
    {
        if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(ErpUserRegistrationInfo)))
           .Column(nameof(ErpUserRegistrationInfo.ErpUserTypeId)).Exists())
        {
            Alter.Table(NameCompatibilityManager.GetTableName(typeof(ErpUserRegistrationInfo)))
                .AddColumn(nameof(ErpUserRegistrationInfo.ErpUserTypeId))
                .AsInt32()
                .Nullable();
        }
    }
}