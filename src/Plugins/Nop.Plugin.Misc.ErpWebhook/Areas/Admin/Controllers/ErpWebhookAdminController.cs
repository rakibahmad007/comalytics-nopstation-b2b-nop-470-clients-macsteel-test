using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Misc.ErpWebhook.Areas.Admin.Models.Configuration;
using Nop.Plugin.Misc.ErpWebhook.Services.Interfaces;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Misc.ErpWebhook.Areas.Admin.Controllers;

[AutoValidateAntiforgeryToken]
[AuthorizeAdmin]
[Area(AreaNames.ADMIN)]
public class ErpWebhookAdminController : BaseAdminController
{
    #region Fields

    private readonly INotificationService _notificationService;
    private readonly ILocalizationService _localizationService;
    private readonly IStoreContext _storeContext;
    private readonly ISettingService _settingService;
    private readonly IPermissionService _permissionService;
    private readonly IWebhookAuthorizationService _webhookAuthorizationService;
    private readonly ICountryService _countryService;

    #endregion

    #region Ctor

    public ErpWebhookAdminController(
        INotificationService notificationService,
        ILocalizationService localizationService,
        IStoreContext storeContext,
        ISettingService settingService,
        IPermissionService permissionService,
        IWebhookAuthorizationService webhookAuthorizationService,
        ICountryService countryService
    )
    {
        _notificationService = notificationService;
        _localizationService = localizationService;
        _storeContext = storeContext;
        _settingService = settingService;
        _permissionService = permissionService;
        _webhookAuthorizationService = webhookAuthorizationService;
        _countryService = countryService;
    }

    #endregion

    #region Methods

    public async Task<IActionResult> Configure()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
            return AccessDeniedView();

        //load settings for a chosen store scope
        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var erpWebhookSettings = await _settingService.LoadSettingAsync<ErpWebhookSettings>(
            storeScope
        );
        var model = erpWebhookSettings.ToSettingsModel<ConfigurationModel>();

        model.ActiveStoreScopeConfiguration = storeScope;

        //prepare available countries
        await PrepareAvailableCountriesAsync(model);

        if (storeScope > 0)
        {
            model.WebhookSecretKey_OverrideForStore = await _settingService.SettingExistsAsync(
                erpWebhookSettings,
                x => x.WebhookSecretKey,
                storeScope
            );
            model.DefaultCountryThreeLetterIsoCode_OverrideForStore = await _settingService.SettingExistsAsync(
                erpWebhookSettings,
                x => x.DefaultCountryThreeLetterIsoCode,
                storeScope
            );
        }

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Configure(ConfigurationModel model)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageWidgets))
            return AccessDeniedView();

        //load settings for a chosen store scope
        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var erpWebhookSettings = await _settingService.LoadSettingAsync<ErpWebhookSettings>(
            storeScope
        );
        var baseUri = $"{Request.Scheme}://{Request.Host}";

        erpWebhookSettings = model.ToSettings(erpWebhookSettings);

        if (!string.IsNullOrWhiteSpace(erpWebhookSettings.WebhookSecretKey))
        {
            var alreadyValidToken = _webhookAuthorizationService.ValidateBearerToken(
                erpWebhookSettings.WebhookSecretKey
            );
            if (!alreadyValidToken)
                erpWebhookSettings.WebhookSecretKey =
                    _webhookAuthorizationService.GenereateWebhookBearerToken();
        }
        else
        {
            erpWebhookSettings.WebhookSecretKey =
                _webhookAuthorizationService.GenereateWebhookBearerToken();
        }

        await _settingService.SaveSettingOverridablePerStoreAsync(
            erpWebhookSettings,
            x => x.WebhookSecretKey,
            model.WebhookSecretKey_OverrideForStore,
            storeScope,
            false
        );

        await _settingService.SaveSettingOverridablePerStoreAsync(
            erpWebhookSettings,
            x => x.DefaultCountryThreeLetterIsoCode,
            model.DefaultCountryThreeLetterIsoCode_OverrideForStore,
            storeScope,
            false
        );

        //now clear settings cache
        await _settingService.ClearCacheAsync();

        _notificationService.SuccessNotification(
            await _localizationService.GetResourceAsync("Admin.Plugins.Saved")
        );

        return RedirectToAction(nameof(Configure));
    }

    #region Utilities

    private async Task PrepareAvailableCountriesAsync(ConfigurationModel model)
    {
        var countries = await _countryService.GetAllCountriesAsync(showHidden: true);
        foreach (var country in countries)
        {
            model.AvailableCountries.Add(new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Text = country.Name,
                Value = country.ThreeLetterIsoCode,
                Selected = country.ThreeLetterIsoCode == model.DefaultCountryThreeLetterIsoCode
            });
        }
    }

    #endregion

    #endregion
}
