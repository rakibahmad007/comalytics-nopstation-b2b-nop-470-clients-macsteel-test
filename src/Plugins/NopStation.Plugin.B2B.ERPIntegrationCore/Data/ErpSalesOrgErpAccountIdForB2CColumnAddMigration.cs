using FluentMigrator;
using Nop.Data.Mapping;
using Nop.Data.Migrations;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Data;

[NopMigration("2024/12/31 12:56:12", "NopStation.Plugin.B2B.ErpSalesOrg ErpAccountIdForB2C Column adding", MigrationProcessType.Update)]
public class ErpSalesOrgErpAccountIdForB2CColumnAddMigration : AutoReversingMigration
{
    #region Methods

    public override void Up()
    {
        if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(ErpSalesOrg)))
            .Column(nameof(ErpSalesOrg.ErpAccountIdForB2C)).Exists())
        {
            Alter.Table(NameCompatibilityManager.GetTableName(typeof(ErpSalesOrg)))
                .AddColumn(nameof(ErpSalesOrg.ErpAccountIdForB2C))
                .AsInt32()
                .Nullable()
                .WithDefaultValue(0);
        }
    }

    #endregion
}
