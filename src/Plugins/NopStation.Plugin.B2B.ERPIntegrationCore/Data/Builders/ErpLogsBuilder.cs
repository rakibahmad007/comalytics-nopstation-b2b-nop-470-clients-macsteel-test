using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Data.Builders;

public class ErpLogsBuilder : NopEntityBuilder<ErpLogs>
{
    #region Methods

    /// <summary>
    /// Apply entity configuration
    /// </summary>
    /// <param name="table">Create table expression builder</param>
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(ErpLogs.ErpLogLevelId)).AsInt32()
            .WithColumn(nameof(ErpLogs.ErpSyncLevelId)).AsInt32()
            .WithColumn(nameof(ErpLogs.ShortMessage)).AsString(int.MaxValue).Nullable()
            .WithColumn(nameof(ErpLogs.FullMessage)).AsString(int.MaxValue).Nullable()
            .WithColumn(nameof(ErpLogs.IpAddress)).AsString().Nullable()
            .WithColumn(nameof(ErpLogs.CustomerId)).AsInt32().Nullable()
            .WithColumn(nameof(ErpLogs.PageUrl)).AsString().Nullable()
            .WithColumn(nameof(ErpLogs.ReferrerUrl)).AsString().Nullable()
            .WithColumn(nameof(ErpLogs.CreatedOnUtc)).AsDateTime2();
    }

    #endregion
}