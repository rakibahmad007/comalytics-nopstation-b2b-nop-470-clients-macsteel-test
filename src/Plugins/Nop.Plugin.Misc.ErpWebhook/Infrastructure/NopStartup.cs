using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core;
using Nop.Core.Infrastructure;
using Nop.Plugin.Misc.ErpWebhook.Areas.Admin.Factories;
using Nop.Plugin.Misc.ErpWebhook.Services;
using Nop.Plugin.Misc.ErpWebhook.Services.Interfaces;
using Nop.Services.Configuration;
using Nop.Services.Media;

namespace Nop.Plugin.Misc.ErpWebhook.Infrastructure;

/// <summary>
/// Represents object for the configuring plugin DB context on application startup
/// </summary>
public class NopStartup : INopStartup
{
    /// <summary>
    /// Add and configure any of the middleware
    /// </summary>
    /// <param name="services">Collection of service descriptors</param>
    /// <param name="configuration">Configuration of the application</param>
    /// 
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RazorViewEngineOptions>(options =>
        {
            options.ViewLocationExpanders.Add(new ViewLocationExpander());
        });

        services.AddScoped<ILicenseService, LicenseService>();
        services.AddScoped<IWebhookERPProductService, WebhookERPProductService>();
        services.AddScoped<IWebhookERPAccountService, WebhookERPAccountService>();
        services.AddScoped<IWebhookERPStockDataService, WebhookERPStockDataService>();
        services.AddScoped<IWebhookERPPerAccountPricingService, WebhookERPPerAccountPricingService>();
        services.AddScoped<IWebhookERPPriceGroupPricingService, WebhookERPPriceGroupPricingService>();
        services.AddScoped<IWebhookERPShipToAddressService, WebhookERPShipToAddressService>();
        services.AddScoped<IWebhookErpOrderService, WebhookErpOrderService>();
        services.AddScoped<IErpWebhookService, ErpWebhookService>();
        services.AddScoped<IWebhookDeliveryDatesService, WebhookDeliveryDatesService>();
        services.AddScoped<IWebhookProductsImageService, WebhookProductsImageService>();
        services.AddScoped<IAllowedWebhookManagerIpAddressesService, AllowedWebhookManagerIpAddressesService>();
        services.AddScoped<IWebhookAuthorizationService, WebhookAuthorizationService>();
        services.AddScoped<IAllowedWebhookManagerIpAddressModelFactory, AllowedWebhookManagerIpAddressModelFactory>();
        services.AddScoped<IPictureService, OverridenPictureService>();
    }

    /// <summary>
    /// Configure the using of added middleware
    /// </summary>
    /// <param name="application">Builder for configuring an application's request pipeline</param>
    public void Configure(IApplicationBuilder application)
    {
        #region clear webhook settings
        var settingService = EngineContext.Current.Resolve<ISettingService>();
        var storeContext = EngineContext.Current.Resolve<IStoreContext>();
        var storeScope = storeContext.GetActiveStoreScopeConfigurationAsync();
        var erpWebhookSettings = new ErpWebhookSettings();

        var accountKey = settingService.GetSettingKey(
            erpWebhookSettings,
            x => x.AccounthookAlreadyRunning
        );
        var shipToAddressKey = settingService.GetSettingKey(
            erpWebhookSettings,
            x => x.ShiptoAddresshookAlreadyRunning
        );
        var productKey = settingService.GetSettingKey(
            erpWebhookSettings,
            x => x.ProducthookAlreadyRunning
        );
        var stockKey = settingService.GetSettingKey(
            erpWebhookSettings,
            x => x.StockhookAlreadyRunning
        );
        var accountPricingKey = settingService.GetSettingKey(
            erpWebhookSettings,
            x => x.AccountPricinghookAlreadyRunning
        );
        var orderKey = settingService.GetSettingKey(
            erpWebhookSettings,
            x => x.OrderhookAlreadyRunning
        );
        var creditKey = settingService.GetSettingKey(
            erpWebhookSettings,
            x => x.CredithookAlreadyRunning
        );
        var deliveryDatesKey = settingService.GetSettingKey(
            erpWebhookSettings,
            x => x.DeliveryDateshookAlreadyRunning
        );
        var productsImageKey = settingService.GetSettingKey(
            erpWebhookSettings,
            x => x.ProductsImagehookAlreadyRunning
        );

        settingService.SetSetting(orderKey, false, storeScope.Result, true);
        settingService.SetSetting(stockKey, false, storeScope.Result, true);
        settingService.SetSetting(creditKey, false, storeScope.Result, true);
        settingService.SetSetting(productKey, false, storeScope.Result, true);
        settingService.SetSetting(accountKey, false, storeScope.Result, true);
        settingService.SetSetting(productsImageKey, false, storeScope.Result, true);
        settingService.SetSetting(deliveryDatesKey, false, storeScope.Result, true);
        settingService.SetSetting(shipToAddressKey, false, storeScope.Result, true);
        settingService.SetSetting(accountPricingKey, false, storeScope.Result, true);
        #endregion
    }

    /// <summary>
    /// Gets order of this startup configuration implementation
    /// </summary>
    public int Order => int.MaxValue;
}
