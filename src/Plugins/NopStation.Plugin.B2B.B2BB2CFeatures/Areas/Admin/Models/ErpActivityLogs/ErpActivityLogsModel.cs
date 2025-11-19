using System;
using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Models;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models.ErpActivityLogs;

/// <summary>
/// Represents an activity log model
/// </summary>
public partial record ErpActivityLogsModel : BaseNopEntityModel
{
    #region Properties
    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogs.Fields.EntityType")]
    public string EntityName { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogs.Fields.EntityDescription")]
    public string EntityDescription { get; set; }
    public int ErpActivityLogTypeId { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogs.Fields.ErpActivityLogsType")]
    public string ErpActivityLogTypeName {  get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogs.Fields.Customer")]
    public int CustomerId { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogs.Fields.CustomerEmail")]
    [DataType(DataType.EmailAddress)]
    public string CustomerEmail { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogs.Fields.Comment")]
    public string Comment { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogs.Fields.CreatedOn")]
    public DateTime CreatedOn { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogs.Fields.IpAddress")]
    public string IpAddress { get; set; }
    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogs.Fields.PropertyName")]
    public string PropertyName { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogs.Fields.OldValue")]
    public string OldValue { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogs.Fields.NewValue")]
    public string NewValue { get; set; }

    #endregion
}