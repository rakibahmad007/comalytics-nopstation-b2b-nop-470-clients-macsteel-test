using Nop.Core;

namespace Nop.Plugin.Misc.ErpWebhook.Domain;

public class AllowedWebhookManagerIpAddresses : BaseEntity
{
    public string IpAddress { get; set; }
}
