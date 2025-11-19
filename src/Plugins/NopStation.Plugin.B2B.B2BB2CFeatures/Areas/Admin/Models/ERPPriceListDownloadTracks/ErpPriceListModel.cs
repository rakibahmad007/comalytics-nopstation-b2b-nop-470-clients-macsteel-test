using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models.ERPPriceListDownloadTracks;

public record ErpPriceListModel : BaseNopEntityModel
{
    public ErpPriceListModel()
    {
        AvailablPriceListTypeOptions = new List<SelectListItem>();
    }

    [NopResourceDisplayName("NopStation.Plugin.B2B.B2BB2CFeatures.ErpPriceListModel.Fields.CustomerName")]
    public string CustomerName { get; set; }

    [NopResourceDisplayName("NopStation.Plugin.B2B.B2BB2CFeatures.ErpPriceListModel.Fields.CustomerEmail")]
    public string CustomerEmail { get; set; }

    [NopResourceDisplayName("NopStation.Plugin.B2B.B2BB2CFeatures.ErpPriceListModel.Fields.B2BAccountName")]
    public string B2BAccountName { get; set; }

    [NopResourceDisplayName("NopStation.Plugin.B2B.B2BB2CFeatures.ErpPriceListModel.Fields.B2BAccountNumber")]
    public string B2BAccountNumber { get; set; }

    [NopResourceDisplayName("NopStation.Plugin.B2B.B2BB2CFeatures.ErpPriceListModel.Fields.B2BSalesOrganisationName")]
    public string B2BSalesOrganisationName { get; set; }

    [NopResourceDisplayName("NopStation.Plugin.B2B.B2BB2CFeatures.ErpPriceListModel.Fields.DownloadedOn")]
    public DateTime DownloadedOn { get; set; }

    [NopResourceDisplayName("NopStation.Plugin.B2B.B2BB2CFeatures.ErpPriceListModel.Fields.PriceListDownloadType")]
    public string PriceListDownloadType { get; set; }

    public IList<SelectListItem> AvailablPriceListTypeOptions { get; set; }
}
