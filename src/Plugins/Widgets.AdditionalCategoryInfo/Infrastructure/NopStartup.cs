using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Plugin.Widgets.AdditionalCategoryInfo.Services;

namespace Nop.Plugin.Widgets.AdditionalCategoryInfo.Infrastructure
{
    public class NopStartup : INopStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.ViewLocationExpanders.Add(new ViewLocationExpander());
            });

            services.AddScoped<
                IAdditionalCategoryInfoDataService,
                AdditionalCategoryInfoDataService
            >();
        }

        public void Configure(IApplicationBuilder application) { }

        public int Order => 100;
    }
}
