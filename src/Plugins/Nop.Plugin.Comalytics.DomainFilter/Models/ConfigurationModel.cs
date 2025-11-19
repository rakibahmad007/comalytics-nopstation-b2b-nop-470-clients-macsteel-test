using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Comalytics.DomainFilter.Models
{
    public record ConfigurationModel : BaseNopEntityModel, ISettingsModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Plugins.Comalytics.DomainFilter.Configuration.Fields.EnableFilter")]
        public bool EnableFilter { get; set; }
        public bool EnableFilter_OverrideForStore { get; set; }
    }
}