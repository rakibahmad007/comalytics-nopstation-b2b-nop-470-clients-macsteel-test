using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Data.Builders;

public class B2CMacsteelExpressShopBuilder : NopEntityBuilder<B2CMacsteelExpressShop>
{
    #region Methods

    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(B2CMacsteelExpressShop.MacsteelExpressShopName)).AsString()
            .WithColumn(nameof(B2CMacsteelExpressShop.MacsteelExpressShopCode)).AsString()
            .WithColumn(nameof(B2CMacsteelExpressShop.Message)).AsString();
    }

    #endregion
}