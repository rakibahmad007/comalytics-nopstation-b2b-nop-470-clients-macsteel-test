using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Plugin.Comalytics.DomainFilter.Factories;
using Nop.Plugin.Comalytics.DomainFilter.Services;

namespace Nop.Plugin.Comalytics.DomainFilter.Infrastructure
{
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
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.ViewLocationExpanders.Add(new ViewLocationExpander());
            });

            //register services
            services.AddScoped<IDomainFilterService, DomainFilterService>();
            services.AddScoped<IDomainFilterExportImportService, DomainFilterExportImportService>();

            //register overriden services
            services.AddScoped<Nop.Services.Customers.ICustomerRegistrationService, OverridenCustomerRegistrationService>();

            // register factories
            services.AddScoped<IDomainModelFactory, DomainModelFactory>();
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
        public int Order => int.MaxValue;
    }
}