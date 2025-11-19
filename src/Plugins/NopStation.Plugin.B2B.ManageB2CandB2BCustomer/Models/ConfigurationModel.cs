using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.B2B.ManageB2CandB2BCustomer.Models;
public class ConfigurationModel
{
    [NopResourceDisplayName("NopStation.Plugin.B2B.ManageB2CandB2BCustomer.Fields.ServiceUrl")]
    public string ServiceUrl { get; set; }
    [NopResourceDisplayName("NopStation.Plugin.B2B.ManageB2CandB2BCustomer.Fields.AuthTocken")]
    public string AuthToken { get; set; }
    public bool HideGeneralBlock { get; set; }
}
