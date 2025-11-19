using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Data.Builders;

public class ErpCustomerConfigurationBuilder : NopEntityBuilder<ErpCustomerConfiguration>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(ErpCustomerConfiguration.NopCustomerId)).AsInt32().NotNullable()
            .WithColumn(nameof(ErpCustomerConfiguration.IsHidePricingNote)).AsBoolean().NotNullable()
            .WithColumn(nameof(ErpCustomerConfiguration.IsHideWeightInfo)).AsBoolean().NotNullable();
    }
}