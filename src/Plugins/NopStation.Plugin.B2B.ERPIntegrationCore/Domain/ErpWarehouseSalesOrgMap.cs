using System;
using Nop.Core;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

public partial class ErpWarehouseSalesOrgMap : BaseEntity
{
    public int NopWarehouseId { get; set; }

    public int ErpSalesOrgId { get; set; }

    public string WarehouseCode { get; set; }

    public DateTime? LastSyncedOnUtc { get; set; }

    public bool IsB2CWarehouse { get; set; }
}
