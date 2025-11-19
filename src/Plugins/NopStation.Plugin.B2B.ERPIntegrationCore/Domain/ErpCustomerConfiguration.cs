using Nop.Core;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

public partial class ErpCustomerConfiguration : BaseEntity
{
    public int NopCustomerId { get; set; }
    public bool IsHidePricingNote { get; set; }
    public bool IsHideWeightInfo { get; set; }
}
