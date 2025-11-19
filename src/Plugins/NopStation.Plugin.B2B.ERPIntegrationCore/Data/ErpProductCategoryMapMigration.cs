using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Mapping;
using Nop.Data.Migrations;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Data;

[NopMigration("2024/12/02 03:47:12", "NopStation.Plugin.B2B.ERPIntegrationCore ERPProductCategoryMap adding table", MigrationProcessType.Update)]
public class ErpProductCategoryMapMigration : AutoReversingMigration
{
    #region Methods

    public override void Up()
    {
        if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(ERPProductCategoryMap))).Exists())
        {
            Create.TableFor<ERPProductCategoryMap>();
        }
    }

    #endregion
}