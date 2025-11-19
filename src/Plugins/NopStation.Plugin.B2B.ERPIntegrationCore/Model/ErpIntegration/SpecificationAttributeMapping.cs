using Nop.Core.Domain.Catalog;
namespace NopStation.Plugin.B2B.ERPIntegrationCore.Model.ErpIntegration;

public class SpecificationAttributeMapping
{
    public string from { get => ErpAttribute; set { ErpAttribute = value; } }
    public string to { get => NopAttribute; set { NopAttribute = value; } }
    public string ErpAttribute { get; set; }
    public string NopAttribute { get; set; }
    public bool AllowFiltering { get; set; } = false;
    public bool ShowOnProductPage { get; set; } = false;

    public int? AttributeTypeId { get; set; } = null;
    public SpecificationAttributeType? AttributeType
    {
        get => (SpecificationAttributeType?)AttributeTypeId;
        set { AttributeTypeId = (int?)value; }
    }

    public int? SpecAttributeId { get; set; } = null;
}
