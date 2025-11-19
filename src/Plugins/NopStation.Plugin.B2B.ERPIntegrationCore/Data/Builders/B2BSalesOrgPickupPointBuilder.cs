using System.Data;
using FluentMigrator.Builders.Create.Table;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Data.Builders;

public class B2BSalesOrgPickupPointBuilder : NopEntityBuilder<B2BSalesOrgPickupPoint>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(B2BSalesOrgPickupPoint.Id)).AsInt32().PrimaryKey().Identity()
            .WithColumn(nameof(B2BSalesOrgPickupPoint.B2BSalesOrgId)).AsInt32().NotNullable().ForeignKey<ErpSalesOrg>(onDelete: Rule.None);
    }
}
