using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models;
public record B2CSalesOrgWarehouseModel : BaseNopEntityModel
{
    public B2CSalesOrgWarehouseModel()
    {
        AvailableWarehouses = new List<SelectListItem>();
    }

    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpSalesOrgWarehouse.Fields.Warehouse")]
    public int WarehouseId { get; set; }

    [NopResourceDisplayName("Admin.Configuration.Shipping.Warehouses.Fields.Name")]
    public string WarehouseName { get; set; }

    public int ErpSalesOrgId { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.Fields.SalesOrgCode")]
    public string SalesOrgCode { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.Fields.SalesOrgName")]
    public string SalesOrgName { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.B2BSalesOrgWarehouse.Fields.B2CWarehouseCode")]
    public string B2CWarehouseCode { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.B2BSalesOrgWarehouse.Fields.LastUpdateTime")]
    public string LastUpdateTime { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.B2BSalesOrgWarehouse.Fields.IsB2CWarehouse")]
    public bool IsB2CWarehouse { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.B2BSalesOrgWarehouse.Fields.IsTradingWarehouse")]
    public bool IsTradingWarehouse { get; set; }

    public IList<SelectListItem> AvailableWarehouses { get; set; }
}
