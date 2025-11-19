using System;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models.ErpDeliveryDates;
public partial record ErpDeliveryDatesModel : BaseNopEntityModel
{
    [NopResourceDisplayName("NopStation.Plugin.B2B.B2BB2CFeatures.ErpDeliveryDates.Field.SalesOrgOrPlant")]
    public string SalesOrgOrPlant { get; set; }
    [NopResourceDisplayName("NopStation.Plugin.B2B.B2BB2CFeatures.ErpDeliveryDates.Field.CutOffTime")]
    public string CutOffTime { get; set; }
    [NopResourceDisplayName("NopStation.Plugin.B2B.B2BB2CFeatures.ErpDeliveryDates.Field.City")]
    public string City { get; set; }
    [NopResourceDisplayName("NopStation.Plugin.B2B.B2BB2CFeatures.ErpDeliveryDates.Field.AllWeekIndicator")]
    public bool AllWeekIndicator { get; set; }
    [NopResourceDisplayName("NopStation.Plugin.B2B.B2BB2CFeatures.ErpDeliveryDates.Field.Monday")]
    public bool Monday { get; set; }
    [NopResourceDisplayName("NopStation.Plugin.B2B.B2BB2CFeatures.ErpDeliveryDates.Field.Tuesday")]
    public bool Tuesday { get; set; }
    [NopResourceDisplayName("NopStation.Plugin.B2B.B2BB2CFeatures.ErpDeliveryDates.Field.Wednesday")]
    public bool Wednesday { get; set; }
    [NopResourceDisplayName("NopStation.Plugin.B2B.B2BB2CFeatures.ErpDeliveryDates.Field.Thursday")]
    public bool Thursday { get; set; }
    [NopResourceDisplayName("NopStation.Plugin.B2B.B2BB2CFeatures.ErpDeliveryDates.Field.Friday")]
    public bool Friday { get; set; }
    [NopResourceDisplayName("NopStation.Plugin.B2B.B2BB2CFeatures.ErpDeliveryDates.Field.DelDate1")]
    public string? DelDate1 { get; set; }
    [NopResourceDisplayName("NopStation.Plugin.B2B.B2BB2CFeatures.ErpDeliveryDates.Field.DelDate2")]
    public string? DelDate2 { get; set; }
    [NopResourceDisplayName("NopStation.Plugin.B2B.B2BB2CFeatures.ErpDeliveryDates.Field.DelDate3")]
    public string? DelDate3 { get; set; }
    [NopResourceDisplayName("NopStation.Plugin.B2B.B2BB2CFeatures.ErpDeliveryDates.Field.DelDate4")]
    public string? DelDate4 { get; set; }
    [NopResourceDisplayName("NopStation.Plugin.B2B.B2BB2CFeatures.ErpDeliveryDates.Field.DelDate5")]
    public string? DelDate5 { get; set; }
    [NopResourceDisplayName("NopStation.Plugin.B2B.B2BB2CFeatures.ErpDeliveryDates.Field.DelDate6")]
    public string? DelDate6 { get; set; }
    [NopResourceDisplayName("NopStation.Plugin.B2B.B2BB2CFeatures.ErpDeliveryDates.Field.DelDate7")]
    public string? DelDate7 { get; set; }
    [NopResourceDisplayName("NopStation.Plugin.B2B.B2BB2CFeatures.ErpDeliveryDates.Field.DelDate8")]
    public string? DelDate8 { get; set; }
    [NopResourceDisplayName("NopStation.Plugin.B2B.B2BB2CFeatures.ErpDeliveryDates.Field.DelDate9")]
    public string? DelDate9 { get; set; }
    [NopResourceDisplayName("NopStation.Plugin.B2B.B2BB2CFeatures.ErpDeliveryDates.Field.DelDate10")]
    public string? DelDate10 { get; set; }
    [NopResourceDisplayName("NopStation.Plugin.B2B.B2BB2CFeatures.ErpDeliveryDates.Field.CreatedOn")]
    public DateTime CreatedOn { get; set; }
    [NopResourceDisplayName("NopStation.Plugin.B2B.B2BB2CFeatures.ErpDeliveryDates.Field.UpdatedOn")]
    public DateTime UpdatedOn { get; set; }
    [NopResourceDisplayName("NopStation.Plugin.B2B.B2BB2CFeatures.ErpDeliveryDates.Field.Deleted")]
    public string Deleted { get; set; }
    [NopResourceDisplayName("NopStation.Plugin.B2B.B2BB2CFeatures.ErpDeliveryDates.Field.IsFullLoadRequired")]
    public bool IsFullLoadRequired { get; set; }
}
