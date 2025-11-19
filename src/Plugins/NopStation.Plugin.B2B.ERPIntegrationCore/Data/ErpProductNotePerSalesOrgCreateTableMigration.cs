using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Mapping;
using Nop.Data.Migrations;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Data;

[NopMigration("2024/09/29 10:59:13", "NopStation.Plugin.B2B.ERPIntegrationCore ErpProductNotePerSalesOrg add table", MigrationProcessType.Update)]
public class ErpProductNotePerSalesOrgCreateTableMigration : AutoReversingMigration
{
    #region Methods

    public override void Up()
    {
        if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(ErpProductNotePerSalesOrg))).Exists())
        {
            Create.TableFor<ErpProductNotePerSalesOrg>();
        }
    }

    #endregion
}