using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Misc.ErpWebhook.Domain.ParallelTables;

namespace Nop.Plugin.Misc.ErpWebhook.Data.Builders
{
    public class ErpProductBuilder : NopEntityBuilder<Parallel_ErpProduct>
    {
        #region Methods

        /// <summary>
        /// Configures the entity
        /// </summary>
        /// <param name="builder">The builder to be used to configure the entity</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(Parallel_ErpProduct.Sku)).AsString(50)
                .WithColumn(nameof(Parallel_ErpProduct.ManufacturerPartNumber)).AsString(50)
                .WithColumn(nameof(Parallel_ErpProduct.ShortDescription)).AsString(500)
                .WithColumn(nameof(Parallel_ErpProduct.IsSpecial)).AsBoolean()
                .WithColumn(nameof(Parallel_ErpProduct.FullDescription)).AsString(500)
                .WithColumn(nameof(Parallel_ErpProduct.Height)).AsDecimal(18, 4)
                .WithColumn(nameof(Parallel_ErpProduct.Width)).AsDecimal(18, 4)
                .WithColumn(nameof(Parallel_ErpProduct.Length)).AsDecimal(18, 4)
                .WithColumn(nameof(Parallel_ErpProduct.Weight)).AsDecimal(18, 4)
                .WithColumn(nameof(Parallel_ErpProduct.SellingPriceA)).AsDecimal(18, 4)
                .WithColumn(nameof(Parallel_ErpProduct.InStockforLocNo)).AsDecimal(18, 4)
                .WithColumn(nameof(Parallel_ErpProduct.CategoriesJson)).AsString(500)
                .WithColumn(nameof(Parallel_ErpProduct.SpecificationAttributesJson)).AsString(500)
                .WithColumn(nameof(Parallel_ErpProduct.VendorName)).AsString(100)
                .WithColumn(nameof(Parallel_ErpProduct.ManufacturerName)).AsString(100)
                .WithColumn(nameof(Parallel_ErpProduct.ManufacturerDescription)).AsString(500);
        }

        #endregion
    }
}
