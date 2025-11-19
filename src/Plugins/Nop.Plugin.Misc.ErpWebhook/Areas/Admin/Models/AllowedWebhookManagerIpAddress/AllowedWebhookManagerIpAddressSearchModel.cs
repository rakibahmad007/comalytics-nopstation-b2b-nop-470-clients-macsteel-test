using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.ErpWebhook.Areas.Admin.Models.AllowedWebhookManagerIpAddress;

public record AllowedWebhookManagerIpAddressSearchModel : BaseSearchModel
{
    public string IpAddress { get; set; }
}
