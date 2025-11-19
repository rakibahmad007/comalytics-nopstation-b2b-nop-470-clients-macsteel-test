using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models;

public record ErpCategoryImageMappingModel : BaseNopEntityModel
{
    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpCategoryImageShow.Field.IsShowImage")]
    public bool IsShowImage { get; set; }
    public int CategoryId { get; set; }
}