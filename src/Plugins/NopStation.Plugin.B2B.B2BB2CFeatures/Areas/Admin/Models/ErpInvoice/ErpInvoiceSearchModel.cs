using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models.ErpInvoice;

public record ErpInvoiceSearchModel: BaseSearchModel
{
    public ErpInvoiceSearchModel()
    {
        AvailableDocumentTypes = new List<SelectListItem>();
        AvailableErpAccounts = new List<SelectListItem>();
    }

    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpInvoice.Field.ErpAccountId")]
    public int ErpAccountId { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpInvoiceModel.Field.PostingStartDate")]
    [UIHint("DateNullable")]
    public DateTime? PostingStartDate { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpInvoiceModel.Field.PostingEndDate")]
    [UIHint("DateNullable")]
    public DateTime? PostingEndDate { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpInvoiceModel.Field.ErpDocumentNumber")]
    public string ErpDocumentNumber { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpInvoiceModel.Field.ErpOrderNumber")]
    public string ErpOrderNumber { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpInvoice.Field.DocumentTypeId")]
    public IList<int> DocumentTypeIds { get; set; }
    public IList<SelectListItem> AvailableDocumentTypes { get; set; }
    public IList<SelectListItem> AvailableErpAccounts { get; set; }
}
