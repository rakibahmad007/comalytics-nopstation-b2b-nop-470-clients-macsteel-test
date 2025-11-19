using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Mapping;
using Nop.Data.Migrations;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Data;

[NopMigration(
    "2025/01/07 19:01:12",
    "NopStation.Plugin.B2B.ERPIntegrationCore B2CShoppingCartItem CreateTable",
    MigrationProcessType.Update
)]
public class B2CShoppingCartItemCreateTableMigration : AutoReversingMigration
{
    #region Methods

    public override void Up()
    {
        if (
            !Schema
                .Table(NameCompatibilityManager.GetTableName(typeof(B2CShoppingCartItem)))
                .Exists()
        )
        {
            Create.TableFor<B2CShoppingCartItem>();
        }
    }

    #endregion
}
