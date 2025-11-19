namespace Nop.Plugin.Misc.ErpWebhook;

public class ErpWebhookConfig
{
    public string AccountPrefilterFacets { get; set; }
    public bool? Accounts_Default_AllowOverspend { get; set; }
    public bool? Override_LowStockActivityId { get; set; }
    public int? LowStockActivityId_DefaultValue { get; set; }
    public bool? Override_BackorderModeId { get; set; }
    public int? BackorderModeId_DefaultValue { get; set; }
    public int? DefaultProductsVendorId { get; set; }
    public int? CategoryTemplateId { get; set; }
    public string DefaultCountryThreeLetterIsoCode { get; set; }
    public string ExternalOrderNumberPrefix { get; set; }
    public int? DefaultSalesOrgId { get; set; }
    public int? DefaultCustomerIdToCreateErpOrder { get; set; }
    public string DefaultCurrencyCode { get; set; }
    public int? DefaultCustomerTaxDisplayTypeId { get; set; }
    public int? TrackInventoryMethodId { get; set; }
    public int? ProductAvailabilityRangeId_DefaultValue { get; set; }
    public bool? ClientIsMacsteel { get; set; }
    public bool? AllowAccountsAddressEditOnCheckout { get; set; }
    public bool? OverrideBackOrderingConfigSetting { get; set; }
    public bool? AllowAccountsBackOrdering { get; set; }
    public int? B2BPriceGroupCodeId { get; set; }
    public int? B2BAccountBatchSize { get; set; }
    public int? B2BShipToAddressBatchSize { get; set; }
    public int? B2BProductBatchSize { get; set; }
    public int? B2BStockBatchSize { get; set; }
    public int? B2BAccountPricingBatchSize { get; set; }
    public int? B2BOrderBatchSize { get; set; }
    public int? B2BCreditBatchSize { get; set; }
    public int? DeliveryDatesBatchSize { get; set; }
    public int? ProductsImageBatchSize { get; set; }
}
