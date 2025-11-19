using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Data.Builders;

public class ErpShipToCodeChangeBuilder : NopEntityBuilder<ErpShipToCodeChange>
{
    #region Methods

    /// <summary>
    /// Apply entity configuration
    /// </summary>
    /// <param name="table">Create table expression builder</param>
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(ErpShipToCodeChange.MatchField)).AsString().Nullable()
            .WithColumn(nameof(ErpShipToCodeChange.CustomerAccountNumber)).AsString().Nullable()
            .WithColumn(nameof(ErpShipToCodeChange.SalesOrg)).AsString().Nullable()
            .WithColumn(nameof(ErpShipToCodeChange.AsIsCustomerShiptoParty)).AsString().Nullable()
            .WithColumn(nameof(ErpShipToCodeChange.ToBeCustomerShiptoParty)).AsString().Nullable();
    }

    #endregion
}
