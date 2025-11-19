using System;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using Nop.Web.Areas.Admin.Models.Common;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models;

public record ErpSalesOrgModel : BaseNopEntityModel
{
    #region Ctor

    public ErpSalesOrgModel()
    {
        AvailableSalesOrgs = new List<SelectListItem>();
        ErpSalesOrgWarehouseSearchModel = new ErpSalesOrgWarehouseSearchModel();
        AddErpSalesOrgWarehouseModel = new ErpSalesOrgWarehouseModel();
        AddB2CSalesOrgWarehouseModel = new B2CSalesOrgWarehouseModel();
        AvailableCategories = new List<SelectListItem>();
    }

    #endregion

    #region Properties

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpSalesOrg.Field.Name")]
    public string Name { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpSalesOrg.Field.Code")]
    public string Code { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpSalesOrg.Field.Email")]
    public string Email { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpSalesOrg.Field.AddressId")]
    public int AddressId { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpSalesOrg.Field.Address")]
    public AddressModel Address { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpSalesOrg.Field.IntegrationClientId")]
    public string IntegrationClientId { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpSalesOrg.Field.AuthenticationKey")]
    public string AuthenticationKey { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpSalesOrg.Field.CreatedOnUtc")]
    public DateTime CreatedOn { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpSalesOrg.Field.UpdatedOnUtc")]
    public DateTime UpdatedOn { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpSalesOrg.Field.CreatedBy")]
    public string CreatedBy { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpSalesOrg.Field.UpdatedBy")]
    public string UpdatedBy { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpSalesOrg.Field.IsActive")]
    public bool IsActive { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpSalesOrg.Field.LastErpAccountSyncTimeOnUtc")]
    public DateTime? LastErpAccountSyncTimeOnUtc { get; set; }
    
    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpSalesOrg.Field.LastErpGroupPriceSyncTimeOnUtc")]
    public DateTime? LastErpGroupPriceSyncTimeOnUtc { get; set; }
    
    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpSalesOrg.Field.LastErpShipToAddressSyncTimeOnUtc")]
    public DateTime? LastErpShipToAddressSyncTimeOnUtc { get; set; }
    
    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpSalesOrg.Field.LastErpProductSyncTimeOnUtc")]
    public DateTime? LastErpProductSyncTimeOnUtc { get; set; }
    
    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpSalesOrg.Field.LastErpStockSyncTimeOnUtc")]
    public DateTime? LastErpStockSyncTimeOnUtc { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpSalesOrg.Field.ErpAccountIdForB2C")]
    public int ErpAccountIdForB2C { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpSalesOrg.Field.ErpAccountNameForB2C")]
    public string ErpAccountNameForB2C { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpSalesOrg.Field.NoItemsMessage")]    
    public string NoItemsMessage { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpSalesOrg.Field.SpecialsCategory")]
    public int? SpecialsCategoryId { get; set; }
    public IList<SelectListItem> AvailableSalesOrgs { get; set; }
    public IList<SelectListItem> AvailableCategories { get; set; }
    public ErpSalesOrgWarehouseSearchModel ErpSalesOrgWarehouseSearchModel { get; set; }
    public ErpSalesOrgWarehouseModel AddErpSalesOrgWarehouseModel { get; set; }
    public B2CSalesOrgWarehouseModel AddB2CSalesOrgWarehouseModel { get; set; }

    #endregion
}
