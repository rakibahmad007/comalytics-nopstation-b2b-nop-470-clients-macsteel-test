using System;
using Nop.Core;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

public partial class ErpDeliveryDates : BaseEntity
{
    public string SalesOrgOrPlant { get; set; }
    public string CutOffTime { get; set; }
    public string City { get; set; }
    public bool? AllWeekIndicator { get; set; }
    public bool? Monday { get; set; }
    public bool? Tuesday { get; set; }
    public bool? Wednesday { get; set; }
    public bool? Thursday { get; set; }
    public bool? Friday { get; set; }
    public DateTime? DelDate1 { get; set; }
    public DateTime? DelDate2 { get; set; }
    public DateTime? DelDate3 { get; set; }
    public DateTime? DelDate4 { get; set; }
    public DateTime? DelDate5 { get; set; }
    public DateTime? DelDate6 { get; set; }
    public DateTime? DelDate7 { get; set; }
    public DateTime? DelDate8 { get; set; }
    public DateTime? DelDate9 { get; set; }
    public DateTime? DelDate10 { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime UpdatedOn { get; set; }
    public bool Deleted { get; set; }
    public bool? IsFullLoadRequired { get; set; }
}
