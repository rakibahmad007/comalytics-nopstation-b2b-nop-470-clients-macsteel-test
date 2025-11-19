using System;
using Nop.Core.Configuration;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;

namespace NopStation.Plugin.B2B.B2BB2CFeatures;

public partial class B2BB2CFeaturesSettings : ISettings
{
    public int OrderMaximumQuantity { get; set; }

    public int DefaultCountryId { get; set; }

    public bool EnableWarehouse { get; set; }

    public int TrackInventoryMethodId { get; set; }

    public int LowStockActivityId_DefaultValue { get; set; }

    public int BackorderModeId_DefaultValue { get; set; }

    public bool AllowBackInStockSubscriptions_DefaultValue { get; set; }

    public int ProductAvailabilityRangeId_DefaultValue { get; set; }

    public bool AvailableForPreOrder_DefaultValue { get; set; }

    public bool DisplayStockAvailability_DefaultValue { get; set; }

    public bool DisplayStockQuantity_DefaultValue { get; set; }

    public bool IsActive { get; set; }

    public bool PlaceB2BOrder { get; set; }

    public bool PlaceB2COrder { get; set; }

    public bool UseNopProductPrice { get; set; }

    public bool UseProductGroupPrice { get; set; }

    public bool UseProductSpecialPrice { get; set; }

    public bool UseProductCombinedPrice { get; set; }

    public bool AllowBackOrderingForAll { get; set; }

    public bool IsB2BUserRegisterAllowed { get; set; }

    public bool IsB2CUserRegisterAllowed { get; set; }

    public bool IsShowLoginForPrice { get; set; }

    public bool IsShowYearlySavings { get; set; }

    public bool IsShowAllTimeSavings { get; set; }

    public DateTime LastDateTimeOfTCUpdate { get; set; }

    public DateTime UpdatedOnUtc { get; set; }

    public int UpdatedById { get; set; }

    public string DooFinderHashId { get; set; }

    public string DooFinderTopBarLogo { get; set; }

    #region Checkout

    public bool IsCustomerReferenceRequiredDuringPayment { get; set; }

    public bool MaintainUniqueCustomerReference { get; set; }

    public bool PreventSpecialCharactersInCustomerReference { get; set; }

    public string SpecialCharactersToPreventInCustomerReference { get; set; }

    public int PaymentPopupMessageDelayTimeInSec { get; set; }

    #endregion

    public int MaxErpIntegrationOrderPlaceRetries { get; set; }

    public bool EnableLogOnErpCall { get; set; }

    public bool EnableLiveStockChecks { get; set; }

    public bool EnableLivePriceChecks { get; set; }

    public bool EnableLiveCreditChecks { get; set; }

    public int DisableLiveStockCheckProductGreaterThanAmount { get; set; }

    public StockDisplayFormat StockDisplayFormat
    {
        get => (StockDisplayFormat)StockDisplayFormatId;
        set => StockDisplayFormatId = (int)value;
    }

    public int StockDisplayFormatId { get; set; }

    public bool EnableQuoteFunctionality { get; set; }

    public int PreFilterFacetSpecificationAttributeId { get; set; }

    public int UnitOfMeasureSpecificationAttributeId { get; set; }

    public bool DisplayAddToQuickListFavouriteButton { get; set; }

    public bool AllowAddressEditOnCheckoutForAll { get; set; }

    public int DeliveryDays { get; set; }

    public int CutoffTime { get; set; }

    public bool ERPToDetermineDate { get; set; }

    public bool IsShowOverSpendWarningText { get; set; }

    public string OverSpendWarningText { get; set; }

    public int DefaultB2COrganizationId { get; set; }

    public bool DisplayWeightInformation { get; set; }

    public string SkipLivePriceCheckCategoryIds { get; set; }

    public string SkipLiveStockCheckCategoryIds { get; set; }

    public bool EnableAccountPayment { get; set; }

    public bool UsePrefilterFacet { get; set; }

    public bool UseDefaultAccountForB2CUser { get; set; }

    public int DefaultB2CErpAccountId { get; set; }

    public bool DisplayErpRepDetailsToCustomer { get; set; }

    public bool UseERPIntegration { get; set; }

    public string DownloadInvoicesPath { get; set; }

    public string FtpUserName { get; set; }

    public string FtpPassword { get; set; }

    public bool UseMultiSalesOrg { get; set; }

    public bool UsePercentageOfAllocatedStock { get; set; }

    public int PercentageOfStockAllowed { get; set; }

    public bool IsErpAccountCustomerRegisterAllowed { get; set; }

    public string GoogleMapsApiKey { get; set; }

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public bool EnableAccountStatementDownload { get; internal set; }

    public bool EnableOnlineSavings { get; set; }

    public int OnlineSavingsCacheTime { get; set; }

    public bool HideInStockFilterWhenCustomerIsLoggedOut { get; set; }

    public int WarehouseCalculationTimeout { get; set; }

    public int PercentageOfAllocatedStockResetTimeUtc { get; set; }

    #region excel and pdf

    public bool DisplayQuantityColumnInExcel { get; set; }

    public bool DisplayUOMColumnInExcelAndPdf { get; set; }

    public bool DisplayPricingNoteColumnInExcelAndPdf { get; set; }

    public bool DisplayWeightColumnInExcelAndPdf { get; set; }

    public bool DisplayAllCategoriesPriceListInCategoryPage { get; set; }

    public bool DisplayCategoryPriceListInCategoryPage { get; set; }

    public bool IsCategoryPriceListAppliedToSubCategories { get; set; }

    public int LastNOrdersPerAccount { get; set; }

    #endregion

    public bool EnableStockRunFullLogInfo { get; set; }
    public bool SetNopOrderNumberAsCustomerReferenceForB2COrder { get; set; }

    #region Soltrack Fields

    public string SoltrackBaseUrl { get; set; }
    public string SoltrackUserName { get; set; }
    public string SoltrackPassword { get; set; }
    public string DistributionAreaType { get; set; }
    public string NoGoAreaType { get; set; }
    public string GoRoutsAreaType { get; set; }
    public string BranchAreaType { get; set; }
    public string MainHubAreaType { get; set; }
    public string ExpressStoreAreaType { get; set; }
    public int SoltrackTimeOutMilliseconds { get; set; }

    #endregion

    public int DeliveryDateCacheTime { get; set; }

    public bool EnableUpdatingSkippedProductsPriceStockDuringProductSync { get; set; } = true;

    public int SyncInvoicesForLastXMonths { get; set; } = 3;
    public bool UseManufacturerPartNumberAsItemNo { get; set; }

    public decimal ProductQuotePrice { get; set; }
}