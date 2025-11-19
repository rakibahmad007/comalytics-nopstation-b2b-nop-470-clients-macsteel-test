using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Data.Builders;

public class ErpCategoryImageShowBuilder : NopEntityBuilder<ErpCategoryImageShow>
{
    #region Methods

    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(ErpCategoryImageShow.CategoryId)).AsInt32()
            .WithColumn(nameof(ErpCategoryImageShow.ShowImage)).AsBoolean();
    }

    #endregion
}