using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using NopStation.Plugin.Misc.B2B.SapIntegration.Services;
using NopStation.Plugin.Misc.B2B.SapIntegration.Services.Auth;

namespace NopStation.Plugin.Misc.B2B.SapIntegration.Infrastructure;

/// <summary>
/// Represents object for the configuring services on application startup
/// </summary>
public class NopStartup : INopStartup
{
    /// <summary>
    /// Add and configure any of the middleware
    /// </summary>
    /// <param name="services">Collection of service descriptors</param>
    /// <param name="configuration">Configuration of the application</param>
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IB2BAccountService, B2BAccountService>();
        services.AddScoped<IB2BProductService, B2BProductService>();
        services.AddScoped<IB2BPricingService, B2BPricingService>();
        services.AddScoped<IB2BInvoiceService, B2BInvoiceService>();
        services.AddScoped<IB2BStockService, B2BStockService>();
        services.AddScoped<IB2BShipToAddressService, B2BShipToAddressService>();
        services.AddScoped<IB2BOrderService, B2BOrderService>();
        services.AddScoped<IB2BShippingService, B2BShippingService>();
        services.AddScoped<IB2BDocumentService, B2BDocumentService>();
        services.AddScoped<IB2BSalesOrgService, B2BSalesOrgService>();
        services.AddScoped<IIntegrationAuthService, IntegrationAuthService>();
    }

    /// <summary>
    /// Configure the using of added middleware
    /// </summary>
    /// <param name="application">Builder for configuring an application's request pipeline</param>
    public void Configure(IApplicationBuilder application) { }

    /// <summary>
    /// Gets order of this startup configuration implementation
    /// </summary>
    public int Order => 3000;
}
