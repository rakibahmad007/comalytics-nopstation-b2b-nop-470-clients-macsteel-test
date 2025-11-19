using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Misc.ErpWebhook.Domain.ParallelTables;

namespace Nop.Plugin.Misc.ErpWebhook.Data.Builders
{
    public class ErpAccountPricingBuilder : NopEntityBuilder<Parallel_ErpAccountPricing>
    {
        #region Methods

        /// <summary>
        /// Configures the entity
        /// </summary>
        /// <param name="builder">The builder to be used to configure the entity</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(Parallel_ErpAccountPricing.AccountNumber)).AsString(50)
                .WithColumn(nameof(Parallel_ErpAccountPricing.SalesOrganisationCode)).AsString(100)
                .WithColumn(nameof(Parallel_ErpAccountPricing.Sku)).AsString(50)
                .WithColumn(nameof(Parallel_ErpAccountPricing.Price)).AsDecimal(18, 4)
                .WithColumn(nameof(Parallel_ErpAccountPricing.ListPrice)).AsDecimal(18, 4)
                .WithColumn(nameof(Parallel_ErpAccountPricing.DiscountPerc)).AsDecimal(18, 4)
                .WithColumn(nameof(Parallel_ErpAccountPricing.PricingNotes)).AsString(500);
        }

        #endregion
    }
}
