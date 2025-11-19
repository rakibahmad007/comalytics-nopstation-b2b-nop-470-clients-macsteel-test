using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using NopStation.Plugin.B2B.B2BB2CFeatures.Controllers;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;
using NopStation.Plugin.B2B.ManageB2CandB2BCustomer.Controllers;
using NopStation.Plugin.B2B.ManageB2CandB2BCustomer.Factories;

namespace NopStation.Plugin.B2B.ManageB2CandB2BCustomer.Infrastructure;

public class NopStartup : INopStartup
{
    public static string NopStationPluginSystemName => "NopStation.Plugin.B2B.ManageB2CandB2BCustomer";

    /// <summary>
    /// Add and configure any of the middleware
    /// </summary>
    /// <param name="services">Collection of service descriptors</param>
    /// <param name="configuration">Configuration of the application</param>
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {

        services.Configure<RazorViewEngineOptions>(options =>
        {
            options.ViewLocationExpanders.Add(new ViewLocationExpander());
        });

        services.AddScoped<B2BB2CCustomerController, OverriddenB2BB2CCustomerController>();

        services.AddScoped<IB2CRegisterModelFactory, B2CRegisterModelFactory>();
        services.AddScoped<IErpShipToAddressService, ErpShipToAddressService>();
    }

    /// <summary>
    /// Configure the using of added middleware
    /// </summary>
    /// <param name="application">Builder for configuring an application's request pipeline</param>
    public void Configure(IApplicationBuilder application)
    {

    }

    /// <summary>
    /// Gets order of this startup configuration implementation
    /// </summary>
    public int Order => 40000;
}
