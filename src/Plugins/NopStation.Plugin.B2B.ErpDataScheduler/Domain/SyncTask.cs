using Nop.Core;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Domain;

public partial class SyncTask : BaseEntity
{
    public string Name { get; set; }
    public string Type { get; set; }
    public DateTime? LastEnabledUtc { get; set; }
    public bool Enabled { get; set; }
    public DateTime? LastStartUtc { get; set; }
    public DateTime? LastEndUtc { get; set; }
    public DateTime? LastSuccessUtc { get; set; }
    public string DayTimeSlots { get; set; }
    public string QuartzJobName { get; set; }
    public bool IsRunning { get; set; }
    public bool IsIncremental { get; set; }
}