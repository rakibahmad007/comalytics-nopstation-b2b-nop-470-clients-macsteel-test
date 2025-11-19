using Nop.Core;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Domain;

public class CommonSalesOrgWarehouseDto
{
    public int B2BSalesOrgWarehouseId { get; set; }
    public int B2CSalesOrgWarehouseId { get; set; } 
    public int ErpSalesOrgId { get; set; }
    public int NopWarehouseId { get; set; }
    public string WarehouseCode { get; set; }
    public DateTime? LastSyncedOnUtc { get; set; }
}