using Nop.Core.Configuration;

namespace Nop.Plugin.Comalytics.DomainFilter
{
    public class DomainFilterSettings : ISettings
    {
        public bool EnableFilter { get; set; }
        public bool EnableFilter_OverrideForStore { get; set; }
    }
}