using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models.ERPPriceListDownloadTracks
{
    public record ErpPriceListSearchModel : BaseSearchModel
    {
        public ErpPriceListSearchModel()
        {
            AvailablPriceListTypeOptions = new List<SelectListItem>();
        }

        public int SearchNopCustomerId { get; set; }

        [NopResourceDisplayName("NopStation.Plugin.B2B.B2BB2CFeatures.ErpPriceListSearchModel.Fields.SearchB2BAccount")]
        public int SearchB2BAccountId { get; set; }

        [NopResourceDisplayName("NopStation.Plugin.B2B.B2BB2CFeatures.ErpPriceListSearchModel.Fields.SearchB2BSalesOrganization")]
        public int SearchB2BSalesOrganizationId { get; set; }

        [NopResourceDisplayName("NopStation.Plugin.B2B.B2BB2CFeatures.ErpPriceListSearchModel.Fields.SearchDownloadedFrom")]
        [UIHint("DateNullable")]
        public DateTime? SearchDownloadedFrom { get; set; }

        [NopResourceDisplayName("NopStation.Plugin.B2B.B2BB2CFeatures.ErpPriceListSearchModel.Fields.SearchDownloadedTo")]
        [UIHint("DateNullable")]
        public DateTime? SearchDownloadedTo { get; set; }

        [NopResourceDisplayName("NopStation.Plugin.B2B.B2BB2CFeatures.ErpPriceListSearchModel.Fields.SearchPriceListDownloadType")]
        public int SearchPriceListDownloadTypeId { get; set; }

        public IList<SelectListItem> AvailablPriceListTypeOptions { get; set; }

    }
}
