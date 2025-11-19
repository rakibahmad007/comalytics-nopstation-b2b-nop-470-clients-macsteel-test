using Nop.Data.Mapping;
using NopStation.Plugin.B2B.ErpDataScheduler.Domain;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Data;

public class BaseNameCompatibility : INameCompatibility
{
    public Dictionary<Type, string> TableNames => new()
    {
        { typeof(SyncTask), "Erp_Data_Sync_Task" },
    };

    public Dictionary<(Type, string), string> ColumnName => new()
    {

    };
}