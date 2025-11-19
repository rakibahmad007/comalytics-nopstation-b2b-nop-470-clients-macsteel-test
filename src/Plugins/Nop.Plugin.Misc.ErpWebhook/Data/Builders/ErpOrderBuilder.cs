using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Misc.ErpWebhook.Domain.ParallelTables;

namespace Nop.Plugin.Misc.ErpWebhook.Data.Builders
{
    public class ErpOrderBuilder : NopEntityBuilder<Parallel_ErpOrder>//erporderadditional like
    {
        #region Methods

        /// <summary>
        /// Configures the entity
        /// </summary>
        /// <param name="builder">The builder to be used to configure the entity</param>

        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(Parallel_ErpOrder.AccountNumber)).AsString(50)
                .WithColumn(nameof(Parallel_ErpOrder.SalesOrganisationCode)).AsString(100)
                .WithColumn(nameof(Parallel_ErpOrder.OrderType)).AsString(50)
                .WithColumn(nameof(Parallel_ErpOrder.OrderDate)).AsDateTime().Nullable()
                .WithColumn(nameof(Parallel_ErpOrder.QuoteExpiryDate)).AsDateTime().Nullable()
                .WithColumn(nameof(Parallel_ErpOrder.TotalExcl)).AsDecimal(18, 4)
                .WithColumn(nameof(Parallel_ErpOrder.TotalIncl)).AsDecimal(18, 4)
                .WithColumn(nameof(Parallel_ErpOrder.CustomerReference)).AsString(100)
                .WithColumn(nameof(Parallel_ErpOrder.CustomNopOrderNumber)).AsString(100)
                .WithColumn(nameof(Parallel_ErpOrder.OrderNumber)).AsString(100)
                .WithColumn(nameof(Parallel_ErpOrder.ERPOrderStatus)).AsString(100)
                .WithColumn(nameof(Parallel_ErpOrder.VAT)).AsDecimal(18, 4)
                .WithColumn(nameof(Parallel_ErpOrder.ShippingFees)).AsDecimal(18, 4)
                .WithColumn(nameof(Parallel_ErpOrder.DeliveryDate)).AsDateTime().Nullable()
                .WithColumn(nameof(Parallel_ErpOrder.SpecialInstructions)).AsString(500).Nullable()
                .WithColumn(nameof(Parallel_ErpOrder.ShippingName)).AsString(100)
                .WithColumn(nameof(Parallel_ErpOrder.ShippingPhone)).AsString(100)
                .WithColumn(nameof(Parallel_ErpOrder.ShippingEmail)).AsString(100)
                .WithColumn(nameof(Parallel_ErpOrder.ShippingAddress1)).AsString(100)
                .WithColumn(nameof(Parallel_ErpOrder.ShippingAddress2)).AsString(100)
                .WithColumn(nameof(Parallel_ErpOrder.ShippingCity)).AsString(100)
                .WithColumn(nameof(Parallel_ErpOrder.ShippingPostalCode)).AsString(100)
                .WithColumn(nameof(Parallel_ErpOrder.ShippingCountryCode)).AsString(100)
                .WithColumn(nameof(Parallel_ErpOrder.ShippingProvince)).AsString(100)
                .WithColumn(nameof(Parallel_ErpOrder.BillingName)).AsString(100)
                .WithColumn(nameof(Parallel_ErpOrder.BillingPhone)).AsString(100)
                .WithColumn(nameof(Parallel_ErpOrder.BillingEmail)).AsString(100)
                .WithColumn(nameof(Parallel_ErpOrder.BillingAddress1)).AsString(100)
                .WithColumn(nameof(Parallel_ErpOrder.BillingAddress2)).AsString(100)
                .WithColumn(nameof(Parallel_ErpOrder.BillingCity)).AsString(100)
                .WithColumn(nameof(Parallel_ErpOrder.BillingPostalCode)).AsString(100)
                .WithColumn(nameof(Parallel_ErpOrder.BillingCountryCode)).AsString(100)
                .WithColumn(nameof(Parallel_ErpOrder.BillingProvince)).AsString(100)
                .WithColumn(nameof(Parallel_ErpOrder.DetailLinesJson)).AsString(500).Nullable();
        }

        #endregion
    }
}
