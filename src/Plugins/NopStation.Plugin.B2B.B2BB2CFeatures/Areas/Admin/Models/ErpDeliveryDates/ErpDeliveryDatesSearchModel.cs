using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models.ErpDeliveryDates;
public partial record ErpDeliveryDatesSearchModel : BaseSearchModel
{
    #region Properties

    [NopResourceDisplayName("NopStation.Plugin.B2B.B2BB2CFeatures.ErpDeliveryDates.Field.SalesOrgOrPlant")]
    public string SalesOrgOrPlant { get; set; }
    [NopResourceDisplayName("NopStation.Plugin.B2B.B2BB2CFeatures.ErpDeliveryDates.Field.City")]
    public string City { get; set; }

    #endregion
}
