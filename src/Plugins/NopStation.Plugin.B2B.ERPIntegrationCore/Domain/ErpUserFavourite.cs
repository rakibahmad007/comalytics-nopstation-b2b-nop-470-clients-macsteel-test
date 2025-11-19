using Nop.Core;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

public partial class ErpUserFavourite : BaseEntity
{
    public int NopCustomerId { get; set; }
    public int ErpNopUserId { get; set; }
}
