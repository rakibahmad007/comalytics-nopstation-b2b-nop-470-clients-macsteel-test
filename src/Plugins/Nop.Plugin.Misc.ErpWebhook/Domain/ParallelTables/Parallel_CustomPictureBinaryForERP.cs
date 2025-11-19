using System;
using Nop.Core;

namespace Nop.Plugin.Misc.ErpWebhook.Domain.ParallelTables;

public class Parallel_CustomPictureBinaryForERP : BaseEntity
{
    /// <summary>
    /// Gets or sets the picture identifier
    /// </summary>
    public int NopPictureId { get; set; }

    /// <summary>
    /// Gets or sets the picture binary
    /// </summary>
    public byte[] BinaryData { get; set; }

    public DateTime? LastUpdatedOn { get; set; }
}
