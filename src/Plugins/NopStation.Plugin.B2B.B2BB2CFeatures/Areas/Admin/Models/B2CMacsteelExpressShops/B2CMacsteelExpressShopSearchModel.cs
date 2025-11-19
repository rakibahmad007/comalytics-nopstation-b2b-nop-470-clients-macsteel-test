using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models.B2CMacsteelExpressShops;
public record B2CMacsteelExpressShopSearchModel : ErpBaseSearchModel
{
    [NopResourceDisplayName("Plugin.Misc.NopStation.Plugin.B2B.B2BB2CFeatures.B2CMacsteelExpressShop.Fields.SearchMacsteelExpressShopName")]
    public string SearchMacsteelExpressShopName { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.Plugin.B2B.B2BB2CFeatures.B2CMacsteelExpressShop.Fields.SearchMacsteelExpressShopCode")]
    public string SearchMacsteelExpressShopCode { get; set; }
}