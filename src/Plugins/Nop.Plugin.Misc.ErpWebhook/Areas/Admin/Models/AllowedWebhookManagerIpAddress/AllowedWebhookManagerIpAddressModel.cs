using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.ErpWebhook.Areas.Admin.Models.AllowedWebhookManagerIpAddress;

public record AllowedWebhookManagerIpAddressModel : BaseNopEntityModel
{
    [NopResourceDisplayName("Plugins.Misc.ErpWebhook.Fields.IpAddress")]
    [Required]
    public string IpAddress { get; set; }
}
