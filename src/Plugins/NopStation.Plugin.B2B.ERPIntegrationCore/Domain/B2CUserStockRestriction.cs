using System;
using Nop.Core;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

public class B2CUserStockRestriction : BaseEntity
{
    public int ProductId { get; set; }

    public int B2CUserId { get; set; }

    public decimal NewPercentageOfAllocatedStock { get; set; }

    public DateTime? PercentageOfAllocatedStockResetTimeUtc { get; set; }
}
