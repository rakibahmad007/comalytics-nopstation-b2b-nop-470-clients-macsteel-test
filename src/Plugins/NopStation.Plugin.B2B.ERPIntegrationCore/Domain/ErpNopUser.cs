using System;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

public partial class ErpNopUser : ErpBaseEntity
{
    public int NopCustomerId { get; set; }

    public int ErpAccountId { get; set; }

    public int ErpShipToAddressId { get; set; }

    public int BillingErpShipToAddressId { get; set; }

    public int ShippingErpShipToAddressId { get; set; }

    public int ErpUserTypeId { get; set; }

    public ErpUserType ErpUserType
    {
        get => (ErpUserType)ErpUserTypeId;
        set => ErpUserTypeId = (int)value;
    }

    public string SalesOrgAsCustomerRoleId { get; set; }

    public string RegistrationAuthorisedBy { get; set; }

    public DateTime? LastWarehouseCalculationTimeUtc { get; set; }

    public decimal? TotalSavingsForthisYear { get; set; }

    public DateTime? TotalSavingsForthisYearUpdatedOnUtc { get; set; }

    public decimal? TotalSavingsForAllTime { get; set; }

    public DateTime? TotalSavingsForAllTimeUpdatedOnUtc { get; set; }
}
