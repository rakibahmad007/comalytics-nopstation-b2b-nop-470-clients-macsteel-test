using Nop.Core;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

public class B2BSalesOrgPickupPoint : BaseEntity
{
    public int NopPickupPointId { get; set; }

    public int B2BSalesOrgId { get; set; }
}
