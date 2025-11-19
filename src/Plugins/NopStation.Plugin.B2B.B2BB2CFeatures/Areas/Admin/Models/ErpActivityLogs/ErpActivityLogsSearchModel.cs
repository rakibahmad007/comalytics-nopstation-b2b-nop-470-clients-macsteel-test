using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Models;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models.ErpActivityLogs;

/// <summary>
/// Represents an activity log search model
/// </summary>
public partial record ErpActivityLogsSearchModel : BaseSearchModel
{
    #region Ctor

    public ErpActivityLogsSearchModel()
    {
        ErpActivityLogsType = new List<SelectListItem>();
        AvailableEntityType = new List<SelectListItem>();
    }

    #endregion

    #region Properties
    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogs.Fields.ErpActivityLogsType")]
    public int ErpActivityLogsTypeId { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogs.Fields.CreatedOnFrom")]
    [UIHint("DateNullable")]
    public DateTime? CreatedOnFrom { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogs.Fields.CreatedOnTo")]
    [UIHint("DateNullable")]
    public DateTime? CreatedOnTo { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogs.Fields.EntityDescription")]
    public string EntityDescription { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogs.Fields.EntityType")]
    public string EntityType { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogs.Fields.IpAddress")]
    public string IpAddress { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogs.Fields.OldValue")]
    public string OldValue { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogs.Fields.NewValue")]
    public string NewValue { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogs.Fields.PropertyName")]
    public string PropertyName { get; set; }
    public IList<SelectListItem> ErpActivityLogsType { get; set; }
    public IList<SelectListItem> AvailableEntityType { get; set; }



    #endregion
}