using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Model.ErpAccountPublic;
public class ErpCustomerConfigurationModel
{

    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpCustomerConfiguration.Fields.IsHidePricingnote")]
    public bool IsHidePricingNote { get; set; }


    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpCustomerConfiguration.Fields.IsHideWeightinfo")]
    public bool IsHideWeightInfo { get; set; }

}
