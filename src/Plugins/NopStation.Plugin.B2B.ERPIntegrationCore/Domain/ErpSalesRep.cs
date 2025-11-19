using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

public partial class ErpSalesRep : ErpBaseEntity
{
    public int NopCustomerId { get; set; }

    public int SalesRepTypeId { get; set; }

    public SalesRepType SalesRepType
    {
        get => (SalesRepType)SalesRepTypeId;
        set => SalesRepTypeId = (int)value;
    }
}
