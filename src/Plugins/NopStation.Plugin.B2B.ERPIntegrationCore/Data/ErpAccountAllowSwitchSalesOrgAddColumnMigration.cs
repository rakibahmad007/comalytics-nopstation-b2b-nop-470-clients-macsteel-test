using FluentMigrator;
using Nop.Data.Mapping;
using Nop.Data.Migrations;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Data;

[NopMigration("2025/01/14 12:12:12", "NopStation.Plugin.B2B.ERPIntegrationCore ErpAccount AllowSwitchSalesOrg AddColumn", MigrationProcessType.Update)]
public class ErpAccountAllowSwitchSalesOrgAddColumnMigration : AutoReversingMigration
{
    #region Methods

    public override void Up()
    {
        if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(ErpAccount)))
            .Column(nameof(ErpAccount.AllowSwitchSalesOrg)).Exists())
        {
            Alter.Table(NameCompatibilityManager.GetTableName(typeof(ErpAccount)))
                .AddColumn(nameof(ErpAccount.AllowSwitchSalesOrg))
                .AsInt32()
                .Nullable();
        }
    }

    #endregion
}