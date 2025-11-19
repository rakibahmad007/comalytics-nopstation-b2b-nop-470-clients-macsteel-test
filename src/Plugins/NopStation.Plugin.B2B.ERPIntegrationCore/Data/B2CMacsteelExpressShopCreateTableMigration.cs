using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Mapping;
using Nop.Data.Migrations;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Data;

[NopMigration(
    "2025/02/13 15:01:12",
    "NopStation.Plugin.B2B.ERPIntegrationCore B2CMacsteelExpressShop CreateTable",
    MigrationProcessType.Update
)]
public class B2CMacsteelExpressShopCreateTableMigration : AutoReversingMigration
{
    #region Methods

    public override void Up()
    {
        if (
            !Schema
                .Table(NameCompatibilityManager.GetTableName(typeof(B2CMacsteelExpressShop)))
                .Exists()
        )
        {
            Create.TableFor<B2CMacsteelExpressShop>();
        }
    }

    #endregion
}
