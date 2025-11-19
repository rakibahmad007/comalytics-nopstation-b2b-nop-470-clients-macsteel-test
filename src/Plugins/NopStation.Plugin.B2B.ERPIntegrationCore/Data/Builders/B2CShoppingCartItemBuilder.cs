using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Data.Builders;

public class B2CShoppingCartItemBuilder : NopEntityBuilder<B2CShoppingCartItem>
{
    #region Methods

    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(B2CShoppingCartItem.ShoppingCartItemId)).AsInt32()
            .WithColumn(nameof(B2CShoppingCartItem.NopWarehouseId)).AsInt32()
            .WithColumn(nameof(B2CShoppingCartItem.WarehouseCode)).AsString()
            .WithColumn(nameof(B2CShoppingCartItem.DeliveryDate)).AsDateTime2().Nullable()
            .WithColumn(nameof(B2CShoppingCartItem.SpecialInstructions)).AsString();
    }

    #endregion
}
