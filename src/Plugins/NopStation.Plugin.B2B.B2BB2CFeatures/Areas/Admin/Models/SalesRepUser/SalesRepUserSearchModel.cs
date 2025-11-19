using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models.SalesRepUser;

public record SalesRepUserSearchModel: BaseSearchModel
{
    public SalesRepUserSearchModel()
    {
        AvailableSalesOrgs = new List<SelectListItem>();
        AvailableActiveOptions = new List<SelectListItem>();
    }

    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeaturesErpNopUser.Fields.SearchERPAccountNumber")]
    public string SearchERPAccountNumber { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeaturesErpNopUser.Fields.SearchERPAccountName")]
    public string SearchERPAccountName { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeaturesErpNopUser.Fields.SearchCustomerEmail")]
    public string SearchCustomerEmail { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeaturesErpNopUser.Fields.SearchSalesOrgId")]
    public int SearchSalesOrgId { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeaturesErpNopUser.Fields.SearchActiveId")]
    public int SearchActiveId { get; set; }

    public IList<SelectListItem> AvailableSalesOrgs { get; set; }

    public IList<SelectListItem> AvailableActiveOptions { get; set; }
}
