using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Mapping;
using Nop.Data.Migrations;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Data;

[NopMigration(
    "2025/01/23 09:40:12",
    "NopStation.Plugin.B2B.ERPIntegrationCore B2CUserStockRestriction CreateTable",
    MigrationProcessType.Update
)]
public class B2CUserStockRestrictionCreateTableMigration : AutoReversingMigration
{
    #region Methods

    public override void Up()
    {
        if (
            !Schema
                .Table(NameCompatibilityManager.GetTableName(typeof(B2CUserStockRestriction)))
                .Exists()
        )
        {
            Create.TableFor<B2CUserStockRestriction>();
        }
    }

    #endregion
}
