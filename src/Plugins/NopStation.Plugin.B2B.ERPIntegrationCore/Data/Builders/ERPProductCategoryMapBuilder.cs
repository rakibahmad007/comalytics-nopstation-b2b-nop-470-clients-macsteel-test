using System.Data;
using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Catalog;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Data.Builders;

public class ERPProductCategoryMapBuilder : NopEntityBuilder<ERPProductCategoryMap>
{
    #region Methods

    /// <summary>
    /// Apply entity configuration
    /// </summary>
    /// <param name="table">Create table expression builder</param>
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(ERPProductCategoryMap.ProductId)).AsInt32().ForeignKey<Product>(onDelete: Rule.None)
            .WithColumn(nameof(ERPProductCategoryMap.CategoryId)).AsInt32().ForeignKey<Category>(onDelete: Rule.None);
    }

    #endregion
}
