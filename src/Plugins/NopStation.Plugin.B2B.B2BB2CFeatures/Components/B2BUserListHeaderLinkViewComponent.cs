using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Customers;
using Nop.Core.Infrastructure;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Web.Framework.Components;
using NopStation.Plugin.B2B.B2BB2CFeatures.Factories;
using NopStation.Plugin.B2B.B2BB2CFeatures.Model;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpCustomerFunctionality;
using NopStation.Plugin.B2B.ERPIntegrationCore;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Components;

public class B2BUserListHeaderLinkViewComponent : NopViewComponent
{
    #region Fields

    private readonly ILocalizationService _localizationService;
    private readonly IErpAccountPublicModelFactory _erpAccountPublicModelFactory;
    private readonly IStoreContext _storeContext;
    private readonly IErpCustomerFunctionalityService _erpCustomerFunctionalityService;
    private readonly ISettingService _settingService;
    private readonly B2BB2CFeaturesSettings _b2BB2CFeaturesSettings;
    private readonly ICurrencyService _currencyService;
    private readonly IGenericAttributeService _genericAttributeService;
    private readonly IPriceFormatter _priceFormatter;
    private readonly IWorkContext _workContext;
    private readonly IErpShipToAddressService _erpShipToAddressService;
    private readonly IStaticCacheManager _staticCacheManager;
    private readonly ICustomerService _customerService;

    #endregion Fields



    #region Ctor

    public B2BUserListHeaderLinkViewComponent(ILocalizationService localizationService, IErpAccountPublicModelFactory erpAccountPublicModelFactory, IStoreContext storeContext, IErpCustomerFunctionalityService erpCustomerFunctionalityService, ISettingService settingService, B2BB2CFeaturesSettings b2BB2CFeaturesSettings, ICurrencyService currencyService, IGenericAttributeService genericAttributeService, IPriceFormatter priceFormatter, IWorkContext workContext, IErpShipToAddressService erpShipToAddressService, IStaticCacheManager staticCacheManager, ICustomerService customerService)
    {
        _localizationService = localizationService;
        _erpAccountPublicModelFactory = erpAccountPublicModelFactory;
        _storeContext = storeContext;
        _erpCustomerFunctionalityService = erpCustomerFunctionalityService;
        _settingService = settingService;
        _b2BB2CFeaturesSettings = b2BB2CFeaturesSettings;
        _currencyService = currencyService;
        _genericAttributeService = genericAttributeService;
        _priceFormatter = priceFormatter;
        _workContext = workContext;
        _erpShipToAddressService = erpShipToAddressService;
        _staticCacheManager = staticCacheManager;
        _customerService = customerService;
    }

    #endregion Ctor

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var currCustomer = await _workContext.GetCurrentCustomerAsync();

        var model = new B2BHeaderLinkModel();
        model.HasB2BCustomerAccountManagerRole = await _customerService.IsInCustomerRoleAsync(currCustomer, ERPIntegrationCoreDefaults.B2BCustomerAccountManagerRoleSystemName);
        if (_workContext.OriginalCustomerIfImpersonated != null)
        {
            model.IsB2BSalesRepImpersonating = await _customerService.IsInCustomerRoleAsync(_workContext.OriginalCustomerIfImpersonated, ERPIntegrationCoreDefaults.B2BSalesRepRoleSystemName);
        }

        // for administrator we won't show exclusive sales rep functionality
        if (_workContext.OriginalCustomerIfImpersonated != null &&
            await _customerService.IsInCustomerRoleAsync(_workContext.OriginalCustomerIfImpersonated, NopCustomerDefaults.AdministratorsRoleName))
        {
            model.IsB2BSalesRepImpersonating = false;
        }

        if (!model.IsB2BSalesRepImpersonating && !model.HasB2BCustomerAccountManagerRole)
        {
            model.IsB2BSalesRepImpersonating = false;
            model.HasB2BCustomerAccountManagerRole = false;
        }

        var erpAccount = await _erpCustomerFunctionalityService.GetActiveErpAccountByCustomerAsync(currCustomer);
        model.IsSalesRep = await _erpCustomerFunctionalityService.IsCurrentCustomerInErpSalesRepRoleAsync();

        if (EngineContext.Current.Resolve<IHttpContextAccessor>().HttpContext.User.Identity.IsAuthenticated && erpAccount != null)
        {
            //customer currency
            var currencyTmp = await _currencyService.GetCurrencyByIdAsync(currCustomer.CurrencyId ?? 0);
            var customerCurrency = currencyTmp != null && currencyTmp.Published ? currencyTmp : await _workContext.GetWorkingCurrencyAsync();
            var customerCurrencyCode = customerCurrency.CurrencyCode;
            var currLanguageId = (await _workContext.GetWorkingLanguageAsync())?.Id ?? 0;

            model.IsShowYearlySavings = _b2BB2CFeaturesSettings.IsShowYearlySavings;

            if (model.IsShowYearlySavings)
            {
                if (erpAccount.TotalSavingsForthisYearUpdatedOnUtc.HasValue &&
                    erpAccount.TotalSavingsForthisYearUpdatedOnUtc.Value.AddMinutes(_b2BB2CFeaturesSettings.OnlineSavingsCacheTime) > DateTime.UtcNow &&
                    erpAccount.TotalSavingsForthisYear.HasValue)
                {
                    var currentYearOnlineSavings = erpAccount.TotalSavingsForthisYear.Value;
                    model.CurrentYearOnlineSavings = await _priceFormatter.FormatPriceAsync(currentYearOnlineSavings, true, customerCurrencyCode, currLanguageId, true);
                }
            }

            model.IsShowAllTimeSavings = _b2BB2CFeaturesSettings.IsShowAllTimeSavings;

            if (model.IsShowAllTimeSavings)
            {
                if (erpAccount.TotalSavingsForAllTimeUpdatedOnUtc.HasValue &&
                    erpAccount.TotalSavingsForAllTimeUpdatedOnUtc.Value.AddMinutes(_b2BB2CFeaturesSettings.OnlineSavingsCacheTime) > DateTime.UtcNow &&
                    erpAccount.TotalSavingsForthisYear.HasValue)
                {
                    var allTimeOnlineSavings = erpAccount.TotalSavingsForAllTime.Value;
                    model.AllTimeOnlineSavings = await _priceFormatter.FormatPriceAsync(allTimeOnlineSavings, true, customerCurrencyCode, currLanguageId, true);
                }
            }
        }
        return View(model);
    }
}