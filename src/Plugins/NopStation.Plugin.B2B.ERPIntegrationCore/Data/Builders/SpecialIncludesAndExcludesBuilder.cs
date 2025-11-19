using System.Data;
using FluentMigrator.Builders.Create.Table;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Data.Builders;

public class SpecialIncludesAndExcludesBuilder : NopEntityBuilder<SpecialIncludesAndExcludes>
{
    #region Methods

    /// <summary>
    /// Apply entity configuration
    /// </summary>
    /// <param name="table">Create table expression builder</param>
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(SpecialIncludesAndExcludes.ErpSalesOrgId)).AsInt32().ForeignKey<ErpSalesOrg>(onDelete: Rule.None)
            .WithColumn(nameof(SpecialIncludesAndExcludes.ErpAccountId)).AsInt32().Nullable().ForeignKey<ErpAccount>(onDelete: Rule.None)
            .WithColumn(nameof(SpecialIncludesAndExcludes.ProductId)).AsInt32().Nullable().ForeignKey<ErpAccount>(onDelete: Rule.None)
            .WithColumn(nameof(SpecialIncludesAndExcludes.LastUpdate)).AsDateTime2().Nullable()
            .WithColumn(nameof(SpecialIncludesAndExcludes.SpecialTypeId)).AsInt32()
            .WithColumn(nameof(SpecialIncludesAndExcludes.IsActive)).AsBoolean();
    }

    #endregion
}
