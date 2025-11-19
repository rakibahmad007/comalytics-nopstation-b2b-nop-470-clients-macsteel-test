using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models.B2CMacsteelExpressShops;
public record B2CMacsteelExpressShopModel : BaseNopEntityModel
{
    [NopResourceDisplayName("Plugin.Misc.NopStation.Plugin.B2B.B2BB2CFeatures.B2CMacsteelExpressShop.Fields.MacsteelExpressShopName")]
    public string MacsteelExpressShopName { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.Plugin.B2B.B2BB2CFeatures.B2CMacsteelExpressShop.Fields.MacsteelExpressShopCode")]
    public string MacsteelExpressShopCode { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.Plugin.B2B.B2BB2CFeatures.B2CMacsteelExpressShop.Fields.Message")]
    public string Message { get; set; }
}