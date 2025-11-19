using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Misc.ErpWebhook.Domain.ParallelTables;

namespace Nop.Plugin.Misc.ErpWebhook.Data.Builders
{
    public class ErpB2BAccountBuilder : NopEntityBuilder<Parallel_ErpAccount>
    {
        #region Methods

        /// <summary>
        /// Configures the entity
        /// </summary>
        /// <param name="builder">The builder to be used to configure the entity</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(Parallel_ErpAccount.AccountNumber)).AsString(50)
                .WithColumn(nameof(Parallel_ErpAccount.AccountName)).AsString(100)
                .WithColumn(nameof(Parallel_ErpAccount.B2BSalesOrganisationId)).AsInt32()
                .WithColumn(nameof(Parallel_ErpAccount.BillingAddressId)).AsInt32().Nullable()
                .WithColumn(nameof(Parallel_ErpAccount.BillingSuburb)).AsString(200)
                .WithColumn(nameof(Parallel_ErpAccount.VatNumber)).AsString(50)
                .WithColumn(nameof(Parallel_ErpAccount.CreditLimit)).AsDecimal(18, 4)
                .WithColumn(nameof(Parallel_ErpAccount.CreditLimitAvailable)).AsDecimal(18, 4)
                .WithColumn(nameof(Parallel_ErpAccount.CurrentBalance)).AsDecimal(18, 4)
                .WithColumn(nameof(Parallel_ErpAccount.LastPaymentAmount)).AsDecimal(18, 4).Nullable()
                .WithColumn(nameof(Parallel_ErpAccount.LastPaymentDate)).AsDateTime().Nullable()
                .WithColumn(nameof(Parallel_ErpAccount.AllowOverspend)).AsBoolean()
                .WithColumn(nameof(Parallel_ErpAccount.PreFilterFacets)).AsString(500).Nullable()
                .WithColumn(nameof(Parallel_ErpAccount.PaymentTypeCode)).AsString(10)
                .WithColumn(nameof(Parallel_ErpAccount.AllowSwitchSalesOrg)).AsBoolean()
                .WithColumn(nameof(Parallel_ErpAccount.OverrideBackOrderingConfigSetting)).AsBoolean()
                .WithColumn(nameof(Parallel_ErpAccount.AllowAccountsBackOrdering)).AsBoolean()
                .WithColumn(nameof(Parallel_ErpAccount.OverrideAddressEditOnCheckoutConfigSetting)).AsBoolean()
                .WithColumn(nameof(Parallel_ErpAccount.AllowAccountsAddressEditOnCheckout)).AsBoolean()
                .WithColumn(nameof(Parallel_ErpAccount.OverrideStockDisplayConfig)).AsBoolean()
                .WithColumn(nameof(Parallel_ErpAccount.PercentageOfStockAllowed)).AsDecimal(18, 4)
                .WithColumn(nameof(Parallel_ErpAccount.StockDisplayFormatTypeId)).AsInt32()
                .WithColumn(nameof(Parallel_ErpAccount.B2BAccountStatusTypeId)).AsInt32()
                .WithColumn(nameof(Parallel_ErpAccount.LastAccountRefresh)).AsDateTime().Nullable()
                .WithColumn(nameof(Parallel_ErpAccount.LastPriceRefresh)).AsDateTime().Nullable()
                .WithColumn(nameof(Parallel_ErpAccount.B2BPriceGroupCodeId)).AsInt32().Nullable()
                .WithColumn(nameof(Parallel_ErpAccount.TotalSavingsForthisYear)).AsDecimal(18, 4).Nullable()
                .WithColumn(nameof(Parallel_ErpAccount.TotalSavingsForAllTime)).AsDecimal(18, 4).Nullable()
                .WithColumn(nameof(Parallel_ErpAccount.TotalSavingsForthisYearUpdatedOnUtc)).AsDateTime().Nullable()
                .WithColumn(nameof(Parallel_ErpAccount.TotalSavingsForAllTimeUpdatedOnUtc)).AsDateTime().Nullable()
                .WithColumn(nameof(Parallel_ErpAccount.LastTimeOrderSyncOnUtc)).AsDateTime().Nullable()
                .WithColumn(nameof(Parallel_ErpAccount.PaymentTermsCode)).AsString(10)
                .WithColumn(nameof(Parallel_ErpAccount.PaymentTermsDescription)).AsString(100).Nullable();
        }
        #endregion
    }
}
