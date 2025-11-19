using System;
using Nop.Core;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

public class SpecialIncludesAndExcludes : BaseEntity
{
    public int ErpAccountId { get; set; }

    public DateTime LastUpdate { get; set; }

    public int ProductId { get; set; }

    public int ErpSalesOrgId { get; set; }

    public int SpecialTypeId { get; set; }

    public bool IsActive { get; set; }

    public SpecialType SpecialType
    {
        get => (SpecialType)SpecialTypeId;
        set => SpecialTypeId = (int)value;
    }
}
