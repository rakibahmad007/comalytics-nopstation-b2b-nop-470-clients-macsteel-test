using System;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Model;

public class ErpStockDataModel
{
    public string WarehouseNameOrCode { get; set; }
    public string Sku { get; set; }
    public string Name { get; set; }
    public string UnitOfMeasure { get; set; }
    public string ManufacturerPartNumber { get; set; }
    public string SalesOrgCode { get; set; }
    public decimal? QuantityOnHand { get; set; }
    public decimal? Weight { get; set; }
    public DateTime? LastChangedDate { get; set; }
}
