using Nop.Core;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

public class ErpProductNotePerSalesOrg : BaseEntity
{
    public int ProductId { get; set; }

    public int SalesOrgId { get; set; }

    public string ProductNotes { get; set; }
}
