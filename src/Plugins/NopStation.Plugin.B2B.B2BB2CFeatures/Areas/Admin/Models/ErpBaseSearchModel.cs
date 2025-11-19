using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models;

public record ErpBaseSearchModel : BaseSearchModel
{
    public ErpBaseSearchModel()
    {
        AvailableActiveOptions = new List<SelectListItem>();
    }

    [NopResourceDisplayName("NopStation.Plugin.B2B.B2BB2CFeatures.SearchActive")]
    public int SearchActiveId { get; set; }
    public IList<SelectListItem> AvailableActiveOptions { get; set; }
}