using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Mapping;
using Nop.Data.Migrations;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Data;

[NopMigration("2025/02/24 20:40:12", "NopStation.Plugin.B2B.ERPIntegrationCore ErpCategoryImageShow CreateTable", MigrationProcessType.Update)]
public class ErpCategoryImageShowCreateTableMigration : AutoReversingMigration
{
    #region Methods

    public override void Up()
    {
        if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(ErpCategoryImageShow))).Exists())
        {
            Create.TableFor<ErpCategoryImageShow>();
        }
    }

    #endregion
}
