using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Data.Builders;

public class B2CUserStockRestrictionBuilder : NopEntityBuilder<B2CUserStockRestriction>
{
    #region Methods

    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(B2CUserStockRestriction.ProductId)).AsInt32()
            .WithColumn(nameof(B2CUserStockRestriction.B2CUserId)).AsInt32()
            .WithColumn(nameof(B2CUserStockRestriction.NewPercentageOfAllocatedStock)).AsDecimal()
            .WithColumn(nameof(B2CUserStockRestriction.PercentageOfAllocatedStockResetTimeUtc)).AsDateTime2().Nullable();
    }

    #endregion
}
