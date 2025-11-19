using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Services;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models;
using NopStation.Plugin.B2B.B2BB2CFeatures.Helpers;
using NopStation.Plugin.B2B.ERPIntegrationCore;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;
using NopStation.Plugin.Misc.Core.Controllers;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Controllers;

public class B2BB2CFeaturesController : NopStationAdminController
{
    #region Fields

    private readonly IWorkContext _workContext;
    private readonly IStoreContext _storeContext;
    private readonly ICommonHelper _commonHelper;
    private readonly ISettingService _settingService;
    private readonly ICountryService _countryService;
    private readonly IPermissionService _permissionService;
    private readonly IStaticCacheManager _staticCacheManager;
    private readonly ILocalizationService _localizationService;
    private readonly INotificationService _notificationService;
    private readonly IBaseAdminModelFactory _baseAdminModelFactory;
    private readonly ISpecificationAttributeService _specificationAttributeService;
    private readonly IErpLogsService _erpLogsService;
    private readonly IErpSalesOrgService _erpSalesOrgService;
    private readonly ICategoryService _categoryService;

    #endregion

    #region Ctor

    public B2BB2CFeaturesController(
        IWorkContext workContext,
        IStoreContext storeContext,
        ICommonHelper commonHelper,
        ISettingService settingService,
        ICountryService countryService,
        IPermissionService permissionService,
        IStaticCacheManager staticCacheManager,
        ILocalizationService localizationService,
        INotificationService notificationService,
        IBaseAdminModelFactory baseAdminModelFactory,
        ISpecificationAttributeService specificationAttributeService,
        IErpLogsService erpLogsService,
        IErpSalesOrgService erpSalesOrgService,
        ICategoryService categoryService
    )
    {
        _workContext = workContext;
        _storeContext = storeContext;
        _commonHelper = commonHelper;
        _settingService = settingService;
        _countryService = countryService;
        _permissionService = permissionService;
        _staticCacheManager = staticCacheManager;
        _localizationService = localizationService;
        _notificationService = notificationService;
        _baseAdminModelFactory = baseAdminModelFactory;
        _specificationAttributeService = specificationAttributeService;
        _erpLogsService = erpLogsService;
        _erpSalesOrgService = erpSalesOrgService;
        _categoryService = categoryService;
    }

    #endregion

    #region Utilities

    public async Task PrepareAvailableCountriesAsync(ConfigurationModel model)
    {
        //prepare available countries
        var availableCountries = await _countryService.GetAllCountriesAsync(showHidden: true);
        model.DefaultCountryId =
            model.DefaultCountryId > 0 ? model.DefaultCountryId : availableCountries[0]?.Id ?? 0;

        if (!availableCountries.Any())
        {
            model.AvailableCountries.Add(
                new SelectListItem
                {
                    Value = "0",
                    Text = "No Country available",
                    Selected = true,
                }
            );
            return;
        }

        foreach (var country in availableCountries)
        {
            model.AvailableCountries.Add(
                new SelectListItem { Value = $"{country.Id}", Text = country.Name }
            );
        }
    }

    public async Task PrepareAvailableStockDisplayFormatsAsync(ConfigurationModel model)
    {
        //prepare available stock display formats
        var availableStockDisplayFormats =
            await StockDisplayFormat.DoNotShowAnyStockAtAll.ToSelectListAsync(false);
        foreach (var stockDisplayFormat in availableStockDisplayFormats)
        {
            model.AvailableStockDisplayFormats.Add(stockDisplayFormat);
        }

        await _commonHelper.PrepareDefaultItemAsync(model.AvailableStockDisplayFormats, false);
    }

    public async Task PrepareAvailableSalesOrganizationsAsync(ConfigurationModel model)
    {
        var saleOrganizations = await _erpSalesOrgService.GetAllErpSalesOrgAsync(showHidden: false);
        foreach (var salesOrg in saleOrganizations)
        {
            var item = new SelectListItem { Text = salesOrg.Name, Value = salesOrg.Id.ToString() };
            model.AvailableSalesOrganizations.Add(item);
        }

        await _commonHelper.PrepareDefaultItemAsync(model.AvailableSalesOrganizations, true);
    }

    public async Task PrepareAvailableSpecificationAttributesAsync(ConfigurationModel model)
    {
        model.AvailableSpecificationAttributes = await _staticCacheManager.GetAsync(
            ERPIntegrationCoreDefaults.ErpProductSpecificationAttributeList,
            async () =>
            {
                var specificAttributes =
                    await _specificationAttributeService.GetSpecificationAttributesAsync();
                var selectList = specificAttributes
                    .Select(attribute => new SelectListItem(attribute.Name, $"{attribute.Id}"))
                    .ToList();
                //insert this default item at first
                selectList.Insert(0, new SelectListItem { Text = "Not Selected Yet", Value = "0" });

                return selectList;
            }
        );
    }

    public async Task PrepareAvailableProductAvailabilityRanges_DefaultValue(
        ConfigurationModel model
    )
    {
        //prepare available product availability ranges
        await _baseAdminModelFactory.PrepareProductAvailabilityRangesAsync(
            model.AvailableProductAvailabilityRanges_DefaultValue,
            defaultItemText: await _localizationService.GetResourceAsync(
                "Admin.Catalog.Products.Fields.ProductAvailabilityRange.None"
            )
        );

        await _commonHelper.PrepareDefaultItemAsync(
            model.AvailableProductAvailabilityRanges_DefaultValue,
            true
        );
    }

    public async Task PrapareAvailableCategoryAndStocksAsync(ConfigurationModel model)
    {
        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var settings = await _settingService.LoadSettingAsync<B2BB2CFeaturesSettings>(storeScope);
        model.AvailableCategories = (await _categoryService.GetAllCategoriesAsync())
            .Select(x =>
            {
                return new SelectListItem { Text = x.Name, Value = $"{x.Id}" };
            })
            .ToList();

        model.AvailableCategoriesForStock = model.AvailableCategories;
        if (!string.IsNullOrWhiteSpace(settings.SkipLivePriceCheckCategoryIds))
            model.SkipLivePriceCheckCategoryIds = settings
                .SkipLivePriceCheckCategoryIds.Split(',')
                .Select(int.Parse)
                .ToList();

        model.SkipLivePriceCheckCategoryIds_OverrideForStore =
            await _settingService.SettingExistsAsync(
                settings,
                x => x.SkipLivePriceCheckCategoryIds,
                storeScope
            );

        if (!string.IsNullOrWhiteSpace(settings.SkipLiveStockCheckCategoryIds))
            model.SkipLiveStockCheckCategoryIds = settings
                .SkipLiveStockCheckCategoryIds.Split(',')
                .Select(int.Parse)
                .ToList();

        model.SkipLiveStockCheckCategoryIds_OverrideForStore =
            await _settingService.SettingExistsAsync(
                settings,
                x => x.SkipLiveStockCheckCategoryIds,
                storeScope
            );
    }

    #endregion

    #region Methods

    public async Task<IActionResult> Configure()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
            return AccessDeniedView();

        var model = new ConfigurationModel();

        //load settings for a chosen store scope
        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();

        var settings = await _settingService.LoadSettingAsync<B2BB2CFeaturesSettings>(storeScope);
        if (settings != null)
            model = settings.ToSettingsModel<ConfigurationModel>();

        var erpCoreSettings = await _settingService.LoadSettingAsync<ERPIntegrationCoreSettings>(
            storeScope
        );
        if (erpCoreSettings != null)
            model.ShowDebugLog = erpCoreSettings.ShowDebugLog;

        await PrepareAvailableCountriesAsync(model);
        await PrepareAvailableStockDisplayFormatsAsync(model);
        await PrepareAvailableSalesOrganizationsAsync(model);
        await PrepareAvailableSpecificationAttributesAsync(model);
        await PrepareAvailableProductAvailabilityRanges_DefaultValue(model);
        await PrapareAvailableCategoryAndStocksAsync(model);
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Configure(ConfigurationModel model)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
            return AccessDeniedView();

        var currentCustomer = await _workContext.GetCurrentCustomerAsync();

        if (ModelState.IsValid)
        {
            model.UpdatedById = currentCustomer.Id;
            model.UpdatedOnUtc = DateTime.UtcNow;
            model.PreFilterFacetSpecificationAttributeId = !model.UsePrefilterFacet
                ? 0
                : model.PreFilterFacetSpecificationAttributeId;

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();

            var erpCoreSettings =
                await _settingService.LoadSettingAsync<ERPIntegrationCoreSettings>(storeScope);
            if (erpCoreSettings != null)
                erpCoreSettings.ShowDebugLog = model.ShowDebugLog;

            var b2BB2CFeaturesSettings =
                await _settingService.LoadSettingAsync<B2BB2CFeaturesSettings>(storeScope);
            var settings = model.ToSettings(b2BB2CFeaturesSettings);

            if (!settings.UseDefaultAccountForB2CUser)
            {
                settings.DefaultB2CErpAccountId = 0;
            }

            if (
                !settings.IsCustomerReferenceRequiredDuringPayment
                || !settings.PreventSpecialCharactersInCustomerReference
            )
            {
                if (!settings.IsCustomerReferenceRequiredDuringPayment)
                {
                    settings.MaintainUniqueCustomerReference = false;
                    settings.PreventSpecialCharactersInCustomerReference = false;
                }
                settings.SpecialCharactersToPreventInCustomerReference = string.Empty;
            }
            if (
                model.SkipLivePriceCheckCategoryIds != null
                && model.SkipLivePriceCheckCategoryIds.Any()
            )
                settings.SkipLivePriceCheckCategoryIds = string.Join(
                    ",",
                    model.SkipLivePriceCheckCategoryIds
                );
            else
                settings.SkipLivePriceCheckCategoryIds = string.Empty;

            if (
                model.SkipLiveStockCheckCategoryIds != null
                && model.SkipLiveStockCheckCategoryIds.Any()
            )
                settings.SkipLiveStockCheckCategoryIds = string.Join(
                    ",",
                    model.SkipLiveStockCheckCategoryIds
                );
            else
                settings.SkipLiveStockCheckCategoryIds = string.Empty;

            #region Save Settings

            await _settingService.SaveSettingOverridablePerStoreAsync(
                settings,
                x => x.IsActive,
                model.IsActive_OverrideForStore,
                storeScope,
                false
            );
            await _settingService.SaveSettingOverridablePerStoreAsync(
                settings,
                x => x.EnableWarehouse,
                model.EnableWarehouse_OverrideForStore,
                storeScope,
                false
            );
            await _settingService.SaveSettingOverridablePerStoreAsync(
                settings,
                x => x.PlaceB2BOrder,
                model.PlaceB2BOrder_OverrideForStore,
                storeScope,
                false
            );
            await _settingService.SaveSettingOverridablePerStoreAsync(
                settings,
                x => x.PlaceB2COrder,
                model.PlaceB2COrder_OverrideForStore,
                storeScope,
                false
            );
            await _settingService.SaveSettingOverridablePerStoreAsync(
                settings,
                x => x.UseNopProductPrice,
                model.UseNopProductPrice_OverrideForStore,
                storeScope,
                false
            );
            await _settingService.SaveSettingOverridablePerStoreAsync(
                settings,
                x => x.UseProductGroupPrice,
                model.UseProductGroupPrice_OverrideForStore,
                storeScope,
                false
            );
            await _settingService.SaveSettingOverridablePerStoreAsync(
                settings,
                x => x.UseProductSpecialPrice,
                model.UseProductSpecialPrice_OverrideForStore,
                storeScope,
                false
            );
            await _settingService.SaveSettingOverridablePerStoreAsync(
                settings,
                x => x.UseProductCombinedPrice,
                model.UseProductCombinedPrice_OverrideForStore,
                storeScope,
                false
            );
            await _settingService.SaveSettingOverridablePerStoreAsync(
                settings,
                x => x.AllowBackOrderingForAll,
                model.AllowBackOrderingForAll_OverrideForStore,
                storeScope,
                false
            );
            await _settingService.SaveSettingOverridablePerStoreAsync(
                settings,
                x => x.IsB2BUserRegisterAllowed,
                model.IsB2BUserRegisterAllowed_OverrideForStore,
                storeScope,
                false
            );
            await _settingService.SaveSettingOverridablePerStoreAsync(
                settings,
                x => x.IsShowLoginForPrice,
                model.IsShowLoginForPrice_OverrideForStore,
                storeScope,
                false
            );
            await _settingService.SaveSettingOverridablePerStoreAsync(
                settings,
                x => x.IsShowYearlySavings,
                model.IsShowYearlySavings_OverrideForStore,
                storeScope,
                false
            );
            await _settingService.SaveSettingOverridablePerStoreAsync(
                settings,
                x => x.IsShowAllTimeSavings,
                model.IsShowAllTimeSavings_OverrideForStore,
                storeScope,
                false
            );
            await _settingService.SaveSettingOverridablePerStoreAsync(
                settings,
                x => x.HideInStockFilterWhenCustomerIsLoggedOut,
                model.HideInStockFilterWhenCustomerIsLoggedOut_OverrideForStore,
                storeScope,
                false
            );
            await _settingService.SaveSettingOverridablePerStoreAsync(
                settings,
                x => x.WarehouseCalculationTimeout,
                model.WarehouseCalculationTimeout_OverrideForStore,
                storeScope,
                false
            );
            await _settingService.SaveSettingOverridablePerStoreAsync(
                settings,
                x => x.LastDateTimeOfTCUpdate,
                model.LastDateTimeOfTCUpdate_OverrideForStore,
                storeScope,
                false
            );
            await _settingService.SaveSettingOverridablePerStoreAsync(
                settings,
                x => x.SetNopOrderNumberAsCustomerReferenceForB2COrder,
                model.SetNopOrderNumberAsCustomerReferenceForB2COrder_OverrideForStore,
                storeScope,
                false
            );
            await _settingService.SaveSettingOverridablePerStoreAsync(
                settings,
                x => x.UpdatedOnUtc,
                model.UpdatedOnUtc_OverrideForStore,
                storeScope,
                false
            );
            await _settingService.SaveSettingOverridablePerStoreAsync(
                settings,
                x => x.UpdatedById,
                model.UpdatedById_OverrideForStore,
                storeScope,
                false
            );
            await _settingService.SaveSettingOverridablePerStoreAsync(
                settings,
                x => x.MaxErpIntegrationOrderPlaceRetries,
                model.MaxERPIntegrationOrderPlaceReties_OverrideForStore,
                storeScope,
                false
            );
            await _settingService.SaveSettingOverridablePerStoreAsync(
                settings,
                x => x.EnableLogOnErpCall,
                model.EnableLogOnErpCall_OverrideForStore,
                storeScope,
                false
            );
            await _settingService.SaveSettingOverridablePerStoreAsync(
                settings,
                x => x.DisableLiveStockCheckProductGreaterThanAmount,
                model.DisableLiveStockCheckProductGreaterThanAmount_OverrideForStore,
                storeScope,
                false
            );
            await _settingService.SaveSettingOverridablePerStoreAsync(
                settings,
                x => x.EnableLiveStockChecks,
                model.EnableLiveStockChecks_OverrideForStore,
                storeScope,
                false
            );
            await _settingService.SaveSettingOverridablePerStoreAsync(
                settings,
                x => x.EnableLivePriceChecks,
                model.EnableLivePriceChecks_OverrideForStore,
                storeScope,
                false
            );
            await _settingService.SaveSettingOverridablePerStoreAsync(
                settings,
                x => x.StockDisplayFormatId,
                model.StockDisplayFormat_OverrideForStore,
                storeScope,
                false
            );
            await _settingService.SaveSettingOverridablePerStoreAsync(
                settings,
                x => x.EnableQuoteFunctionality,
                model.EnableQuoteFunctionality_OverrideForStore,
                storeScope,
                false
            );
            await _settingService.SaveSettingOverridablePerStoreAsync(
                settings,
                x => x.DisplayAddToQuickListFavouriteButton,
                model.DisplayAddToQuickListFavouriteButton_OverrideForStore,
                storeScope,
                false
            );
            await _settingService.SaveSettingOverridablePerStoreAsync(
                settings,
                x => x.AllowAddressEditOnCheckoutForAll,
                model.AllowAddressEditOnCheckoutForAll_OverrideForStore,
                storeScope,
                false
            );
            await _settingService.SaveSettingOverridablePerStoreAsync(
                settings,
                x => x.DeliveryDays,
                model.DeliveryDays_OverrideForStore,
                storeScope,
                false
            );
            await _settingService.SaveSettingOverridablePerStoreAsync(
                settings,
                x => x.CutoffTime,
                model.CutoffTime_OverrideForStore,
                storeScope,
                false
            );
            await _settingService.SaveSettingOverridablePerStoreAsync(
                settings,
                x => x.ERPToDetermineDate,
                model.ERPToDetermineDate_OverrideForStore,
                storeScope,
                false
            );
            await _settingService.SaveSettingOverridablePerStoreAsync(
                settings,
                x => x.DisplayErpRepDetailsToCustomer,
                model.DisplayErpRepDetailsToCustomer_OverrideForStore,
                storeScope,
                false
            );
            await _settingService.SaveSettingOverridablePerStoreAsync(
                settings,
                x => x.OverSpendWarningText,
                model.OverSpendWarningText_OverrideForStore,
                storeScope,
                false
            );
            await _settingService.SaveSettingOverridablePerStoreAsync(
                settings,
                x => x.IsShowOverSpendWarningText,
                model.IsShowOverSpendWarningText_OverrideForStore,
                storeScope,
                false
            );
            await _settingService.SaveSettingOverridablePerStoreAsync(
                settings,
                x => x.DefaultB2COrganizationId,
                model.DefaultB2COrganizationId_OverrideForStore,
                storeScope,
                false
            );
            await _settingService.SaveSettingOverridablePerStoreAsync(
                settings,
                x => x.PreFilterFacetSpecificationAttributeId,
                model.PreFilterFacetSpecificationAttributeId_OverrideForStore,
                storeScope,
                false
            );
            await _settingService.SaveSettingOverridablePerStoreAsync(
                settings,
                x => x.UnitOfMeasureSpecificationAttributeId,
                model.UnitOfMeasureSpecificationAttributeId_OverrideForStore,
                storeScope,
                false
            );
            await _settingService.SaveSettingOverridablePerStoreAsync(
                settings,
                x => x.TrackInventoryMethodId,
                model.Override_TrackInventoryMethodId,
                storeScope,
                false
            );
            await _settingService.SaveSettingOverridablePerStoreAsync(
                settings,
                x => x.LowStockActivityId_DefaultValue,
                model.Override_LowStockActivityId,
                storeScope,
                false
            );
            await _settingService.SaveSettingOverridablePerStoreAsync(
                settings,
                x => x.BackorderModeId_DefaultValue,
                model.Override_BackorderModeId,
                storeScope,
                false
            );
            await _settingService.SaveSettingOverridablePerStoreAsync(
                settings,
                x => x.AllowBackInStockSubscriptions_DefaultValue,
                model.Override_AllowBackInStockSubscriptions,
                storeScope,
                false
            );
            await _settingService.SaveSettingOverridablePerStoreAsync(
                settings,
                x => x.ProductAvailabilityRangeId_DefaultValue,
                model.Override_ProductAvailabilityRangeId,
                storeScope,
                false
            );
            await _settingService.SaveSettingOverridablePerStoreAsync(
                settings,
                x => x.AvailableForPreOrder_DefaultValue,
                model.Override_AvailableForPreOrder,
                storeScope,
                false
            );
            await _settingService.SaveSettingOverridablePerStoreAsync(
                settings,
                x => x.DisplayStockAvailability_DefaultValue,
                model.Override_DisplayStockAvailability,
                storeScope,
                false
            );
            await _settingService.SaveSettingOverridablePerStoreAsync(
                settings,
                x => x.DisplayStockQuantity_DefaultValue,
                model.Override_DisplayStockQuantity,
                storeScope,
                false
            );
            await _settingService.SaveSettingOverridablePerStoreAsync(
                settings,
                x => x.EnableLiveCreditChecks,
                model.EnableLiveCreditChecks_OverrideForStore,
                storeScope,
                false
            );

            await _settingService.SaveSettingOverridablePerStoreAsync(
                settings,
                x => x.OrderMaximumQuantity,
                model.OrderMaximumQuantity_OverrideForStore,
                storeScope,
                false
            );
            await _settingService.SaveSettingOverridablePerStoreAsync(
                settings,
                x => x.DefaultCountryId,
                model.DefaultCountryId_OverrideForStore,
                storeScope,
                false
            );
            await _settingService.SaveSettingOverridablePerStoreAsync(
                settings,
                x => x.IsB2CUserRegisterAllowed,
                model.IsB2CUserRegisterAllowed_OverrideForStore,
                storeScope,
                false
            );
            await _settingService.SaveSettingOverridablePerStoreAsync(
                settings,
                x => x.EnableAccountPayment,
                model.EnableAccountPayment_OverrideForStore,
                storeScope,
                false
            );
            await _settingService.SaveSettingOverridablePerStoreAsync(
                settings,
                x => x.UsePrefilterFacet,
                model.UsePrefilterFacet_OverrideForStore,
                storeScope,
                false
            );
            await _settingService.SaveSettingOverridablePerStoreAsync(
                settings,
                x => x.UseDefaultAccountForB2CUser,
                model.UseDefaultAccountForB2CUser_OverrideForStore,
                storeScope,
                false
            );
            await _settingService.SaveSettingOverridablePerStoreAsync(
                settings,
                x => x.DefaultB2CErpAccountId,
                model.DefaultB2CErpAccountId_OverrideForStore,
                storeScope,
                false
            );
            await _settingService.SaveSettingOverridablePerStoreAsync(
                settings,
                x => x.UseERPIntegration,
                model.UseERPIntegration_OverrideForStore,
                storeScope,
                false
            );
            await _settingService.SaveSettingOverridablePerStoreAsync(
                settings,
                x => x.UseMultiSalesOrg,
                model.UseMultiSalesOrg_OverrideForStore,
                storeScope,
                false
            );
            await _settingService.SaveSettingOverridablePerStoreAsync(
                settings,
                x => x.UsePercentageOfAllocatedStock,
                model.UsePercentageOfAllocatedStock_OverrideForStore,
                storeScope,
                false
            );
            await _settingService.SaveSettingOverridablePerStoreAsync(
                settings,
                x => x.PercentageOfStockAllowed,
                model.PercentageOfStockAllowed_OverrideForStore,
                storeScope,
                false
            );
            await _settingService.SaveSettingOverridablePerStoreAsync(
                settings,
                x => x.IsErpAccountCustomerRegisterAllowed,
                model.IsErpAccountCustomerRegisterAllowed_OverrideForStore,
                storeScope,
                false
            );
            await _settingService.SaveSettingOverridablePerStoreAsync(
                settings,
                x => x.DisplayPricingNoteColumnInExcelAndPdf,
                model.DisplayPricingNoteColumnInExcelAndPdf_OverrideForStore,
                storeScope,
                false
            );
            await _settingService.SaveSettingOverridablePerStoreAsync(
                settings,
                x => x.DisplayWeightColumnInExcelAndPdf,
                model.DisplayWeightColumnInExcelAndPdf_OverrideForStore,
                storeScope,
                false
            );
            await _settingService.SaveSettingOverridablePerStoreAsync(
                settings,
                x => x.IsCategoryPriceListAppliedToSubCategories,
                model.IsCategoryPriceListAppliedToSubCategories_OverrideForStore,
                storeScope,
                false
            );
            await _settingService.SaveSettingOverridablePerStoreAsync(
                settings,
                x => x.LastNOrdersPerAccount,
                model.LastNOrdersPerAccount_OverrideForStore,
                storeScope,
                false
            );
            await _settingService.SaveSettingOverridablePerStoreAsync(
                settings,
                x => x.DisplayWeightInformation,
                model.DisplayWeightInformation_OverrideForStore,
                storeScope,
                false
            );
            //Ftp settings
            await _settingService.SaveSettingOverridablePerStoreAsync(
                settings,
                x => x.DownloadInvoicesPath,
                model.DownloadInvoicesPath_OverrideForStore,
                storeScope,
                false
            );
            await _settingService.SaveSettingOverridablePerStoreAsync(
                settings,
                x => x.FtpUserName,
                model.FtpUserName_OverrideForStore,
                storeScope,
                false
            );
            await _settingService.SaveSettingOverridablePerStoreAsync(
                settings,
                x => x.FtpPassword,
                model.FtpPassword_OverrideForStore,
                storeScope,
                false
            );

            //Google Maps Setting
            await _settingService.SaveSettingOverridablePerStoreAsync(
                settings,
                x => x.GoogleMapsApiKey,
                model.GoogleMapsApiKey_OverrideForStore,
                storeScope,
                false
            );
            await _settingService.SaveSettingOverridablePerStoreAsync(
                settings,
                x => x.Latitude,
                model.Latitude_OverrideForStore,
                storeScope,
                false
            );
            await _settingService.SaveSettingOverridablePerStoreAsync(
                settings,
                x => x.Longitude,
                model.Longitude_OverrideForStore,
                storeScope,
                false
            );
            await _settingService.SaveSettingOverridablePerStoreAsync(
                settings,
                x => x.EnableAccountStatementDownload,
                model.EnableAccountStatementDownload_OverrideForStore,
                storeScope,
                false
            );

            await _settingService.SaveSettingOverridablePerStoreAsync(
                settings,
                x => x.IsCustomerReferenceRequiredDuringPayment,
                model.IsCustomerReferenceRequiredDuringPayment_OverrideForStore,
                storeScope,
                false
            );
            await _settingService.SaveSettingOverridablePerStoreAsync(
                settings,
                x => x.MaintainUniqueCustomerReference,
                model.MaintainUniqueCustomerReference_OverrideForStore,
                storeScope,
                false
            );
            await _settingService.SaveSettingOverridablePerStoreAsync(
                settings,
                x => x.PreventSpecialCharactersInCustomerReference,
                model.PreventSpecialCharactersInCustomerReference_OverrideForStore,
                storeScope,
                false
            );
            await _settingService.SaveSettingOverridablePerStoreAsync(
                settings,
                x => x.SpecialCharactersToPreventInCustomerReference,
                model.SpecialCharactersToPreventInCustomerReference_OverrideForStore,
                storeScope,
                false
            );
            await _settingService.SaveSettingOverridablePerStoreAsync(
                settings,
                x => x.PaymentPopupMessageDelayTimeInSec,
                model.PaymentPopupMessageDelayTimeInSec_OverrideForStore,
                storeScope,
                false
            );
            await _settingService.SaveSettingOverridablePerStoreAsync(
                settings,
                x => x.PercentageOfAllocatedStockResetTimeUtc,
                model.PercentageOfAllocatedStockResetTimeUtc_OverrideForStore,
                storeScope,
                false
            );
            await _settingService.SaveSettingOverridablePerStoreAsync(
                settings,
                x => x.SkipLivePriceCheckCategoryIds,
                model.SkipLivePriceCheckCategoryIds_OverrideForStore,
                storeScope,
                false
            );
            await _settingService.SaveSettingOverridablePerStoreAsync(
                settings,
                x => x.SkipLiveStockCheckCategoryIds,
                model.SkipLiveStockCheckCategoryIds_OverrideForStore,
                storeScope,
                false
            );

            await _settingService.SaveSettingOverridablePerStoreAsync(
                settings,
                x => x.DisplayAllCategoriesPriceListInCategoryPage,
                model.DisplayAllCategoriesPriceListInCategoryPage_OverrideForStore,
                storeScope,
                false
            );
            await _settingService.SaveSettingOverridablePerStoreAsync(
                settings,
                x => x.DisplayCategoryPriceListInCategoryPage,
                model.DisplayCategoryPriceListInCategoryPage_OverrideForStore,
                storeScope,
                false
            );
            await _settingService.SaveSettingOverridablePerStoreAsync(
                settings,
                x => x.DisplayQuantityColumnInExcel,
                model.DisplayQuantityColumnInExcel_OverrideForStore,
                storeScope,
                false
            );
            await _settingService.SaveSettingOverridablePerStoreAsync(
                settings,
                x => x.DisplayUOMColumnInExcelAndPdf,
                model.DisplayUOMColumnInExcelAndPdf_OverrideForStore,
                storeScope,
                false
            );

            await _settingService.SaveSettingOverridablePerStoreAsync(
                settings,
                x => x.SoltrackBaseUrl,
                model.SoltrackBaseUrl_OverrideForStore,
                storeScope,
                false
            );

            await _settingService.SaveSettingOverridablePerStoreAsync(
                settings,
                x => x.SoltrackUserName,
                model.SoltrackUserName_OverrideForStore,
                storeScope,
                false
            );

            await _settingService.SaveSettingOverridablePerStoreAsync(
                settings,
                x => x.SoltrackPassword,
                model.SoltrackPassword_OverrideForStore,
                storeScope,
                false
            );

            await _settingService.SaveSettingOverridablePerStoreAsync(
                settings,
                x => x.DistributionAreaType,
                model.DistributionAreaType_OverrideForStore,
                storeScope,
                false
            );

            await _settingService.SaveSettingOverridablePerStoreAsync(
                settings,
                x => x.NoGoAreaType,
                model.NoGoAreaType_OverrideForStore,
                storeScope,
                false
            );

            await _settingService.SaveSettingOverridablePerStoreAsync(
                settings,
                x => x.GoRoutsAreaType,
                model.GoRoutsAreaType_OverrideForStore,
                storeScope,
                false
            );

            await _settingService.SaveSettingOverridablePerStoreAsync(
                settings,
                x => x.BranchAreaType,
                model.BranchAreaType_OverrideForStore,
                storeScope,
                false
            );

            await _settingService.SaveSettingOverridablePerStoreAsync(
                settings,
                x => x.MainHubAreaType,
                model.MainHubAreaType_OverrideForStore,
                storeScope,
                false
            );

            await _settingService.SaveSettingOverridablePerStoreAsync(
                settings,
                x => x.ExpressStoreAreaType,
                model.ExpressStoreAreaType_OverrideForStore,
                storeScope,
                false
            );

            await _settingService.SaveSettingOverridablePerStoreAsync(
                settings,
                x => x.SoltrackTimeOutMilliseconds,
                model.SoltrackTimeOutMilliseconds_OverrideForStore,
                storeScope,
                false
            );

            await _settingService.SaveSettingOverridablePerStoreAsync(
                settings,
                x => x.DeliveryDateCacheTime,
                model.DeliveryDateCacheTime_OverrideForStore,
                storeScope,
                false
            );

            await _settingService.SaveSettingOverridablePerStoreAsync(
                settings,
                x => x.EnableOnlineSavings,
                model.EnableOnlineSavings_OverrideForStore,
                storeScope,
                false
            );

            await _settingService.SaveSettingOverridablePerStoreAsync(
                settings,
                x => x.OnlineSavingsCacheTime,
                model.OnlineSavingsCacheTime_OverrideForStore,
                storeScope,
                false
            );

            await _settingService.SaveSettingOverridablePerStoreAsync(
                settings,
                x => x.EnableUpdatingSkippedProductsPriceStockDuringProductSync,
                model.EnableUpdatingSkippedProductsPriceStockDuringProductSync_OverrideForStore,
                storeScope,
                false
            );

            await _settingService.SaveSettingOverridablePerStoreAsync(
                settings,
                x => x.UseManufacturerPartNumberAsItemNo,
                model.UseManufacturerPartNumberAsItemNo_OverrideForStore,
                storeScope,
                false
            );
            await _settingService.SaveSettingOverridablePerStoreAsync(
                settings,
                x => x.ProductQuotePrice,
                model.ProductQuotePrice_OverrideForStore,
                storeScope,
                false
            );
            await _settingService.SaveSettingOverridablePerStoreAsync(
                erpCoreSettings,
                x => x.ShowDebugLog,
                model.ShowDebugLog_OverrideForStore,
                storeScope,
                false
            );

            #endregion
        }

        //now clear settings cache
        await _settingService.ClearCacheAsync();

        // clear price cache
        await _staticCacheManager.RemoveByPrefixAsync(
            ERPIntegrationCoreDefaults.ErpProductPricingPrefix
        );

        var successMsg = await _localizationService.GetResourceAsync(
            "B2BB2CFeatures.Configuration.Updated"
        );
        _notificationService.SuccessNotification(successMsg);

        await _erpLogsService.InformationAsync(
            successMsg,
            ErpSyncLevel.Account,
            customer: currentCustomer
        );

        await PrepareAvailableCountriesAsync(model);
        await PrepareAvailableStockDisplayFormatsAsync(model);
        await PrepareAvailableSalesOrganizationsAsync(model);
        await PrepareAvailableSpecificationAttributesAsync(model);
        await PrepareAvailableProductAvailabilityRanges_DefaultValue(model);
        await PrapareAvailableCategoryAndStocksAsync(model);

        return View(model);
    }

    #endregion
}
