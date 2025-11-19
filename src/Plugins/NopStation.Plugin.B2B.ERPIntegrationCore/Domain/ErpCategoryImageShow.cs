using Nop.Core;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

public partial class ErpCategoryImageShow : BaseEntity
{
    public int CategoryId { get; set; }
    public bool ShowImage { get; set; }
}