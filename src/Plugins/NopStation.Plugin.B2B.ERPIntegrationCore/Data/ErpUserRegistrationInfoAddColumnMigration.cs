using FluentMigrator;
using Nop.Data.Mapping;
using Nop.Data.Migrations;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Data;

[NopMigration("2024/10/30 11:24:11", "NopStation.Plugin.B2B.ERPIntegrationCore.Data ErpUserRegistrationInfo, ErpSalesOrganisationId, ErpAccountIdForB2C and ErpAccountNumber AddColumn", MigrationProcessType.Update)]
public class ErpUserRegistrationInfoSchemaMigrationAddColumnMigration : AutoReversingMigration
{
    #region Methods

    public override void Up()
    {
        if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(ErpUserRegistrationInfo)))
            .Column(nameof(ErpUserRegistrationInfo.ErpSalesOrganisationIds)).Exists())
        {
            Alter.Table(NameCompatibilityManager.GetTableName(typeof(ErpUserRegistrationInfo)))
                .AddColumn(nameof(ErpUserRegistrationInfo.ErpSalesOrganisationIds))
                .AsString(200)
                .Nullable();
        }

        if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(ErpUserRegistrationInfo)))
             .Column(nameof(ErpUserRegistrationInfo.ErpAccountNumber)).Exists())
        {
            Alter.Table(NameCompatibilityManager.GetTableName(typeof(ErpUserRegistrationInfo)))
                .AddColumn(nameof(ErpUserRegistrationInfo.ErpAccountNumber))
                .AsString(200)
                .Nullable();
        }

        if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(ErpUserRegistrationInfo)))
             .Column(nameof(ErpUserRegistrationInfo.ErpAccountIdForB2C)).Exists())
        {
            Alter.Table(NameCompatibilityManager.GetTableName(typeof(ErpUserRegistrationInfo)))
                .AddColumn(nameof(ErpUserRegistrationInfo.ErpAccountIdForB2C))
                .AsInt32()
                .Nullable();
        }
    }

    #endregion
}