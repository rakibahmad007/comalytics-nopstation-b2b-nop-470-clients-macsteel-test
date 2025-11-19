using Nop.Core;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

public partial class ERPSAPErrorMsgTranslation : BaseEntity
{
    public string ErrorMsg { get; set; }
    public string UserTranslation { get; set; }
}
