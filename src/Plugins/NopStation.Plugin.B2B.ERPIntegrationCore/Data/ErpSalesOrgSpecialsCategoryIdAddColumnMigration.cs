using FluentMigrator;
using Nop.Data.Mapping;
using Nop.Data.Migrations;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Data;

[NopMigration("2024/09/12 12:44:12", "NopStation.Plugin.B2B.ERPIntegrationCore ErpSalesOrg SpecialsCategoryId AddColumn", MigrationProcessType.Update)]
public class ErpSalesOrgSpecialsCategoryIdAddColumnMigration : AutoReversingMigration
{
    #region Methods

    public override void Up()
    {
        if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(ErpSalesOrg)))
            .Column(nameof(ErpSalesOrg.SpecialsCategoryId)).Exists())
        {
            Alter.Table(NameCompatibilityManager.GetTableName(typeof(ErpSalesOrg)))
                .AddColumn(nameof(ErpSalesOrg.SpecialsCategoryId))
                .AsInt32()
                .Nullable();
        }
    }

    #endregion
}