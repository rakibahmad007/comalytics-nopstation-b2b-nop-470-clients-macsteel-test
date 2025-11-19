using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Misc.ErpWebhook.Domain.ParallelTables;

namespace Nop.Plugin.Misc.ErpWebhook.Data.Builders
{
    public class ErpStockBuilder : NopEntityBuilder<Parallel_ErpStock>
    {
        #region Methods

        /// <summary>
        /// Configures the entity
        /// </summary>
        /// <param name="builder">The builder to be used to configure the entity</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(Parallel_ErpStock.Sku)).AsString(50)
                .WithColumn(nameof(Parallel_ErpStock.SalesOrganisationCode)).AsString(100)
                .WithColumn(nameof(Parallel_ErpStock.WarehouseCode)).AsString(100)
                .WithColumn(nameof(Parallel_ErpStock.TotalOnHand)).AsInt32()
                .WithColumn(nameof(Parallel_ErpStock.UOM)).AsString(100)
                .WithColumn(nameof(Parallel_ErpStock.Weight)).AsDecimal(18, 4);
        }

        #endregion
    }
}
