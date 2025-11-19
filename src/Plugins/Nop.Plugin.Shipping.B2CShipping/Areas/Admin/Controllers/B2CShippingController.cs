using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Directory;
using Nop.Plugin.Shipping.B2CShipping.Areas.Admin.Models;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Services.Shipping;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Controllers;

namespace Nop.Plugin.Shipping.B2CShipping.Controllers
{
    public class B2CShippingController : BaseAdminController
    {
        #region Fields

        private readonly CurrencySettings _currencySettings;
        private readonly ICountryService _countryService;
        private readonly ICurrencyService _currencyService;
        private readonly ILocalizationService _localizationService;
        private readonly IMeasureService _measureService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IShippingService _shippingService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IStoreService _storeService;
        private readonly MeasureSettings _measureSettings;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly ICustomerService _customerService;
        private readonly INotificationService _notificationService;

        #endregion

        #region Ctor

        public B2CShippingController(CurrencySettings currencySettings,
            ICountryService countryService,
            ICurrencyService currencyService,
            ILocalizationService localizationService,
            IMeasureService measureService,
            IPermissionService permissionService,
            ISettingService settingService,
            IShippingService shippingService,
            IStateProvinceService stateProvinceService,
            IStoreService storeService,
            MeasureSettings measureSettings,
            IWorkContext workContext,
            IStoreContext storeContext,
            ICustomerService customerService,
            INotificationService notificationService)
        {
            _currencySettings = currencySettings;
            _countryService = countryService;
            _currencyService = currencyService;
            _localizationService = localizationService;
            _measureService = measureService;
            _permissionService = permissionService;
            _settingService = settingService;
            _stateProvinceService = stateProvinceService;
            _shippingService = shippingService;
            _storeService = storeService;
            _measureSettings = measureSettings;
            _workContext = workContext;
            _storeContext = storeContext;
            _customerService = customerService;
            _notificationService = notificationService;
        }

        #endregion

        #region Methods

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageShippingSettings))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var b2CShippingSettings = await _settingService.LoadSettingAsync<B2CShippingSettings>(storeScope);

            var customerRoles = await _customerService.GetAllCustomerRolesAsync();
            var model = new ConfigurationModel
            {
                AvailableCustomerRoles = customerRoles.Select(x =>
                {
                    return new SelectListItem
                    {
                        Text = x.Name,
                        Value = x.Id.ToString()
                    };
                }).ToList()
            };

            if (!string.IsNullOrWhiteSpace(b2CShippingSettings.AllowedRoleIds))
            {
                model.AllowedRoleIds = b2CShippingSettings.AllowedRoleIds.Contains(",")
                    ? b2CShippingSettings.AllowedRoleIds.Split(',').Select(int.Parse).ToList()
                    : new List<int> { int.Parse(b2CShippingSettings.AllowedRoleIds) };
            }

            if (storeScope > 0)
            {
                model.AllowedRoleIds_OverrideForStore = await _settingService.SettingExistsAsync(b2CShippingSettings, x => x.AllowedRoleIds, storeScope);
            }

            return View("~/Plugins/Shipping.B2CShipping/Areas/Admin/Views/Configure.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageShippingSettings))
                return Content("Access denied");

            if (!ModelState.IsValid)
                return await Configure();

            // Load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var b2CShippingSettings = await _settingService.LoadSettingAsync<B2CShippingSettings>(storeScope);

            // Save settings
            b2CShippingSettings.AllowedRoleIds = string.Join(",", model.AllowedRoleIds);

            await _settingService.ClearCacheAsync();

            await _settingService.SaveSettingOverridablePerStoreAsync(b2CShippingSettings, x => x.AllowedRoleIds, model.AllowedRoleIds_OverrideForStore, storeScope, false);

            // Now clear settings cache
            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

            return await Configure();
        }
        #endregion
    }
}