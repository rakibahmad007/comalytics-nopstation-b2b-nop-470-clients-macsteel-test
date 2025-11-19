using System;
using Nop.Core;

namespace Nop.Plugin.Misc.ErpWebhook.Domain.ParallelTables;

public class BaseParallelEntity : BaseEntity
{
    public bool IsActive { get; set; }
    public DateTime CreatedOnUtc { get; set; }
    public int CreatedById { get; set; }
    public DateTime UpdatedOnUtc { get; set; }
    public int UpdatedById { get; set; }
    public bool IsDeleted { get; set; }
    public bool IsUpdated { get; set; }
}
