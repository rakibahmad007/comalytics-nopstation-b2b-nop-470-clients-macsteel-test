using Nop.Core.Caching;

namespace NopStation.Plugin.B2B.B2BB2CFeatures;

public static class B2BB2CFeaturesDefaults
{
    public static string XmlResourceStringFilePath => "~/Plugins/NopStation.Plugin.B2B.B2BB2CFeatures/ResourceString/B2BB2CFeatures.Resources.en-us.xml";
    public static string ErpPriceGroupProductPricingInsert => "ErpPriceGroupProductPricingInsert";
    public static string ErpPriceGroupProductPricingUpdate => "ErpPriceGroupProductPricingUpdate";
    public static string ErpPriceGroupProductPricingDelete => "ErpPriceGroupProductPricingDelete";
    public static string ErpDateFormatForDataTable => "DD/MM/YYYY";
    public static string ErpDateFormatForPublicInputField => "dd/mm/yy";
    public static string IsCartActivityOn => "IsCartActivityOn";
    public static string ProductIsOnSpecial => "OnSpecial";
    public static string ProductIsOnSpecialIconPath =>
        "~/Plugins/NopStation.Plugin.B2B.B2BB2CFeatures/Content/img/special_icon.png";

    #region B2B Customer Navigation Tab values

    public static int ErpCustomerNavigationEnum_ErpCompareValue => 1000;
    public static int ErpCustomerNavigationEnum_ErpAccountOrders => 1020;
    public static int ErpCustomerNavigationEnum_ErpAccountQuoteOrders => 1030;
    public static int ErpCustomerNavigationEnum_ErpBillingAddresses => 1040;
    public static int ErpCustomerNavigationEnum_ErpShippingAddresses => 1050;
    public static int ErpCustomerNavigationEnum_ErpCustomerConfiguration => 1060;
    public static int ErpCustomerNavigationEnum_ErpAccountTransactionsInfo => 1010;

    #endregion

    #region View components

    public static string ZoneAfterTirePriceCard => "zone_after_tire_price_card";
    public static string ZoneAfterSpecialPriceCard => "zone_after_special_price_card";
    public static string ErpAdminWidgetZonesOrderDetailsBlock => "admin_erp_order_details_block";
    public static string ProductPriceB2BPricingNote => "productprice_b2b_pricing_note";

    #endregion

    public static string B2BConvertedQuoteB2BOrderId => "B2BConvertedQuoteB2BOrderId";
    public static string B2CConvertedQuoteB2COrderId => "B2CConvertedQuoteB2COrderId";

    public static string B2BOriginalB2BQuoteOrderIdReference =>
        "B2BOriginalB2BQuoteOrderIdReference";
    public static string B2COriginalB2CQuoteOrderIdReference =>
        "B2COriginalB2CQuoteOrderIdReference";

    public static string CartItemsLivePriceSyncProcessing => "CartItemsLivePriceSyncProcessing";
    public static string CustomerLastDateOfDisplayB2BPriceSyncInfo => "LastDateOfDisplayB2BPriceSyncInfo";
    public static string CustomerLastDateOfDisplayB2BPriceGroupPriceSyncInfo => "LastDateOfDisplayB2BPriceGroupPriceSyncInfo";
    public static string B2BCustomerReferenceAsPO => "B2BCustomerReferenceAsPO";
    public static string B2BSpecialInstructions => "B2BSpecialInstructions";
    public static string B2CCustomerReferenceAsPO => "B2CCustomerReferenceAsPO";
    public static string B2CSpecialInstructions => "B2CSpecialInstructions";
    public static string ShippingAddressModifiedIdInCheckoutAttribute =>
        "ShippingAddressModifiedIdInCheckout";
    public static string IsShippingAddressModifiedInCheckoutAttribute =>
        "IsShippingAddressModifiedInCheckout";
    public static string SelectedB2BDeliveryDateAttribute => "SelectedB2BDeliveryDate";
    public static string B2BQouteOrderAttribute => "QouteOrderSelected";
    public static string B2CQouteOrderAttribute => "B2CQouteOrderSelected";
    public static CacheKey ErpProductInfoSpecificationAttributeOptionIdsByNamesCacheKey =>
        new(
            "ErpProductInfoSpecificationAttributeOptionIdsByNamesErpAccountId-{0}-{1}-{2}",
            ErpProductInfoSpecificationAttributeOptionIdsByNamesErpAccountId
        );
    public static string ErpProductInfoSpecificationAttributeOptionIdsByNamesErpAccountId =>
        "ErpProductInfoSpecificationAttributeOptionIdsByNamesErpAccountId.{0}";

    public static CacheKey ErpSpecificationAttributeOptionIdsForSpecialExcludeOptionNamesCacheKey =>
        new(
            "ErpSpecificationAttributeOptionIdsBySpecialExcludeOptionNames-{0}-{1}",
            ErpSpecificationAttributeOptionIdsBySpecialExcludeOptionNames
        );
    public static string ErpSpecificationAttributeOptionIdsBySpecialExcludeOptionNames =>
        "ErpSpecificationAttributeOptionIdsBySpecialExcludeOptionNames.{0}";

    public static string LanguageIdAttribute => "LanguageId";

    public static CacheKey ErpProductInfoUOMCacheKey =>
        new("ErpProductInfoUOM-{0}-{1}", ErpProductInfoUOM);
    public static string ErpProductInfoUOM => "ErpProductInfoUOM.{0}";

    public static CacheKey ErpProductIdsBySpecialExcludeOptionNamesCacheKey =>
        new(
            "ErpProductIdsBySpecialExcludeOptionNames-{0}-{1}",
            ErpProductIdsBySpecialExcludeOptionNames
        );
    public static string ErpProductIdsBySpecialExcludeOptionNames =>
        "ErpProductIdsBySpecialExcludeOptionNames.{0}";

    public static CacheKey ProductsByIdCacheKey => new("NopProductId-{0}", NopProductId);
    public static string NopProductId => "NopProductId.{0}";

    public static CacheKey ErpCustomerAccountErpLogsSyncLabelSelectList =>
        new("B2BB2CFeatures.ErpLogs.SyncLabelSelectList");
    public static CacheKey ErpProductModelProductPriceCacheKey =>
        new("Erp.ProductPricing.ProductModel.ProductPrice-{0}", ProductPrice);
    public static string ProductPrice => "ProductPrice.{0}";

    #region Erp Order Status

    public static string ErpOrderStatusApproved => "Approved";
    public static string ErpOrderStatusPendingApproval => "Pending Approval";
    public static string ErpOrderStatusProcessing => "Processing";

    #endregion

    public static string B2COrderOnlineSavingsAdjustmentValue => "B2COrderOnlineSavingsAdjustmentValue";
    public static string CustomerIdForCurrentYearSavings => "CurrentYearSavings-{0}";
    public static string CustomerIdForAllTimeSavings => "AllTimeSavings-{0}";

    public static string B2BUserCurrentYearSavingsByCustomerCacheKey => "B2B.UserInformation.CurrentYearSavings-{0}";
    public static string B2BUserAllTimeSavingsByCustomerCacheKey => "B2B.UserInformation.AllTimeSavings-{0}";

    public static string B2CUserCurrentYearSavingsByCustomerCacheKey => "B2C.UserInformation.currentyearsavings-{0}";
    public static string B2CUserAllTimeSavingsByCustomerCacheKey => "B2C.UserInformation.alltimesavings-{0}";

    public static string B2BCustomerConfigurationByIdCacheKey => "B2B.CustomerConfiguration.id-{0}";
    public static string B2BCustomerConfigurationPrefixCacheKey => "B2B.CustomerConfiguration.";
    public static string B2CShippingOptionSystemName => "Shipping.B2CShipping";
    public static string B2CShippingOptionName => "Plugins.Shipping.B2CShipping.Options.OptionName";
    public static string B2CShippingOptionDescription =>
        "Plugins.Shipping.B2CShipping.Options.Description";

    #region Sales org warehouse

    public static string B2BSalesOrgWarehouseCreate => "B2BB2CFeatures.B2BSalesOrgWarehouse.Create";
    public static string B2BSalesOrgWarehouseUpdate => "B2BB2CFeatures.B2BSalesOrgWarehouse.Update";
    public static string B2BSalesOrgWarehouseDelete => "B2BB2CFeatures.B2BSalesOrgWarehouse.Delete";

    public static string B2CSalesOrgWarehouseCreate => "B2BB2CFeatures.B2CSalesOrgWarehouse.Create";
    public static string B2CSalesOrgWarehouseUpdate => "B2BB2CFeatures.B2CSalesOrgWarehouse.Update";
    public static string B2CSalesOrgWarehouseDelete => "B2BB2CFeatures.B2CSalesOrgWarehouse.Delete";

    #endregion

    #region Work Flow Message Template

    public static string MessageTemplateSystemNames_ERPOrderPlaceFailedSalesRepNotification =>
        "ERPOrderPlaceFailed.SalesRepNotification";

    public static string MessageTemplateSystemNames_ERPAccountCustomerRegistrationCreatedNotificationToCustomer =>
        "ERPAccountCustomerRegistration.CreatedNotificationToCustomer";

    public static string MessageTemplateSystemNames_ERPAccountCustomerRegistrationCreatedNotificationToAdmin =>
        "ERPAccountCustomerRegistration.CreatedNotificationToAdmin";

    public static string MessageTemplateSystemNames_ERPAccountCustomerRegistrationApprovedNotification =>
        "ERPAccountCustomerRegistration.ApprovedNotification";

    #endregion

    public static string ProcessFailedErpOrdersTask =>
        "NopStation.Plugin.B2B.B2BB2CFeatures.Services.ProcessFailedErpOrdersTask";
    public static string ProcessFailedErpOrdersTaskName => "B2B Process Failed ERP Orders";
    public static string LastStockSyncDateTime => "LastStockSyncDateTime";
    public static int DefaultTaskTimeOutPeriod => 360;

    public static string ProcessBackInStockSubscriptionsTask =>
        "NopStation.Plugin.B2B.B2BB2CFeatures.Services.ProcessBackInStockSubscriptionsTask";
    public static string ProcessBackInStockSubscriptionsTaskName =>
        "ERP Process BackInStockSubscription";
    public static int DefaultSubscriptionsTaskTimeOutPeriod => 300;

    public static string B2BB2CFeatureErpAccountUpdate =>
        "NopStation.Plugin.B2B.B2BB2CFeatures.ErpAccount.Update";

    /// <summary>
    /// Gets a key for caching
    /// </summary>
    /// <remarks>
    /// {0} : erp account id
    /// {1} : Use Price Group Pricing
    /// {1} : Price Group Code Id
    /// {1} : product id
    /// </remarks>
    public static CacheKey ErpProductPricingCommonCacheKey =>
        new("ERP.ProductPricing.Common-{0}-{1}-{2}-{3}");
    public static string MessageTemplateSystemNames_OrderOrDeliveryDatesOrShippingCostBAPIFailedMessage =>
        "ERPFailed.StoreOwnerNotification";

    public static string MessageTemplateSystemNames_B2CCustomerWelcomeMessage => "B2C.Customer.WelcomeMessage";

    public static string MessageTemplateSystemNames_B2CCustomerEmailVerificationMessage => "B2C.Customer.EmailVerificationMessage";

    public static string MessageTemplateSystemNames_B2CCustomerRegisteredNotification => "B2C.NewCustomer.Notification";

    public static string MessageTemplateSystemNames_B2BCustomerRegisteredNotification => "B2B.NewCustomer.Notification";


    public static string MessageTemplateSystemNames_SendCombinedLineItemWarningMessage =>
        "Warning.LineItemsCombined";
    public static string B2BPriceListExcelPricingNoteColumnResourceString =>
        "B2B.PriceList.Excel.PricingNote";
    public static string B2BPriceListExcelWeightColumnResourceString =>
        "B2B.PriceList.Excel.Weight";
    public static string B2CCashRounding => "B2CCashRounding";

    public static string CategoryViewModeAttribute => "categoryviewmode";

    public static string B2CVatNumberAttribute => "B2CVatNumber";

    #region B2CMacsteelExpressShop
    public static string B2CMacsteelExpressShopInsert => "B2BCustomerAccount.B2CMacsteelExpressShop.Insert";
    public static string B2CMacsteelExpressShopUpdate => "B2BCustomerAccount.B2CMacsteelExpressShop.Update";
    public static string B2CMacsteelExpressShopDelete => "B2BCustomerAccount.B2CMacsteelExpressShop.Delete";

    #endregion

    public static string ProductOverviewModelStockAvailabilityKey => "StockAvailability";

    public static string CategoryVisibilityBlock => "admin_category_image_visibility_setup_block";
    public static string ERPDeliveryDatesforPlantByFiltersCacheKey => "B2C.User.Customer.Suburb.City.Plant-{0}-{1}-{2}-{3}-{4}";
    public static string CustomZoneForB2CShipToAddressSelector => "CustomZoneForB2CShipToAddressSelector";

    #region Site Map Node System Names

    // Root menu
    public static string B2BB2C_Features_Root_SiteMapNode_SystemName => "NopStation.Plugin.B2B.B2BB2CFeatures.SiteMapNode.SystemName";

    // Sub menu items
    public static string B2BB2C_Features_Configuration_SiteMapNode_SystemName => "NopStation.Plugin.B2B.B2BB2CFeatures.SiteMapNode.SystemName.Configuration";
    public static string ErpAccounts_SiteMapNode_SystemName => "NopStation.Plugin.B2B.B2BB2CFeatures.SiteMapNode.SystemName.ErpAccounts";
    public static string ErpRegistrationApplication_SiteMapNode_SystemName => "NopStation.Plugin.B2B.B2BB2CFeatures.SiteMapNode.SystemName.ErpRegistrationApplication";
    public static string ErpSalesOrgs_SiteMapNode_SystemName => "NopStation.Plugin.B2B.B2BB2CFeatures.SiteMapNode.SystemName.ErpSalesOrgs";
    public static string ErpShipToAddress_SiteMapNode_SystemName => "NopStation.Plugin.B2B.B2BB2CFeatures.SiteMapNode.SystemName.ErpShipToAddress";
    public static string ErpNopUsers_SiteMapNode_SystemName => "NopStation.Plugin.B2B.B2BB2CFeatures.SiteMapNode.SystemName.ErpNopUsers";
    public static string ErpGroupPriceCode_SiteMapNode_SystemName => "NopStation.Plugin.B2B.B2BB2CFeatures.SiteMapNode.SystemName.ErpGroupPriceCode";
    public static string ErpInvoices_SiteMapNode_SystemName => "NopStation.Plugin.B2B.B2BB2CFeatures.SiteMapNode.SystemName.ErpInvoices";
    public static string ErpDeliveryDates_SiteMapNode_SystemName => "NopStation.Plugin.B2B.B2BB2CFeatures.SiteMapNode.SystemName.ErpDeliveryDates";
    public static string SalesRepresentatives_SiteMapNode_SystemName => "NopStation.Plugin.B2B.B2BB2CFeatures.SiteMapNode.SystemName.SalesRepresentatives";
    public static string B2CMacsteelExpressShop_SiteMapNode_SystemName => "NopStation.Plugin.B2B.B2BB2CFeatures.SiteMapNode.SystemName.B2CMacsteelExpressShop";
    public static string ErpAllProducts_SiteMapNode_SystemName => "NopStation.Plugin.B2B.B2BB2CFeatures.SiteMapNode.SystemName.ErpAllProducts";
    public static string ErpOrder_SiteMapNode_SystemName => "NopStation.Plugin.B2B.B2BB2CFeatures.SiteMapNode.SystemName.ErpOrder";
    public static string CustomerRestrictionShown_SiteMapNode_SystemName => "NopStation.Plugin.B2B.B2BB2CFeatures.SiteMapNode.SystemName.CustomerRestrictionShown";
    public static string CustomerRestrictionHide_SiteMapNode_SystemName => "NopStation.Plugin.B2B.B2BB2CFeatures.SiteMapNode.SystemName.CustomerRestrictionHide";
    public static string ERPPriceListDownloadTrack_SiteMapNode_SystemName => "NopStation.Plugin.B2B.B2BB2CFeatures.SiteMapNode.SystemName.ERPPriceListDownloadTrack";
    public static string ERPSAPErrorMsgTranslations_SiteMapNode_SystemName => "NopStation.Plugin.B2B.B2BB2CFeatures.SiteMapNode.SystemName.ERPSAPErrorMsgTranslations";
    // Separate menu items
    public static string ErpLogs_SiteMapNode_SystemName => "NopStation.Plugin.B2B.B2BB2CFeatures.SiteMapNode.SystemName.ErpLogs";
    public static string ErpActivityLogs_SiteMapNode_SystemName => "NopStation.Plugin.B2B.B2BB2CFeatures.SiteMapNode.SystemName.ErpActivityLogs";
    public static string ErpActivityLogsList_SiteMapNode_SystemName => "NopStation.Plugin.B2B.B2BB2CFeatures.SiteMapNode.SystemName.ErpActivityLogsList";
    public static string ErpActivityLogsTypes_SiteMapNode_SystemName => "NopStation.Plugin.B2B.B2BB2CFeatures.SiteMapNode.SystemName.ErpActivityLogsTypes";

    #endregion

    public static string B2BCustomerAccountPluginSystemName => "NopStation.Plugin.Payments.B2BAccount";
    public static string CompanyAttribute => "Company";
    public static string PhoneAttribute => "Phone";
    public static string ZipPostalCodeAttribute => "ZipPostalCode";
}
