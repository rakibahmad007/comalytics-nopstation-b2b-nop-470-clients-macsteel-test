using Nop.Core;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

public partial class ERPProductCategoryMap : BaseEntity
{
    public int ProductId { get; set; }

    public int CategoryId { get; set; }
}
