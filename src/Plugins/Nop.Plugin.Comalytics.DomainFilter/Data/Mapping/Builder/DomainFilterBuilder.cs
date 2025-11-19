using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Comalytics.DomainFilter.Domains;

namespace Nop.Plugin.Comalytics.DomainFilter.Data.Mapping.Builder;
public class DomainFilterBuilder : NopEntityBuilder<Domain>
{
    #region Methods

    /// <summary>
    /// Apply entity configuration
    /// </summary>
    /// <param name="table">The builder to be used to configure the entity</param>
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(NameCompatibilityManager.GetColumnName(typeof(Domain), nameof(Domain.Id))).AsInt32().PrimaryKey().Identity()
            .WithColumn(NameCompatibilityManager.GetColumnName(typeof(Domain), nameof(Domain.DomainOrEmailName))).AsString()
            .WithColumn(NameCompatibilityManager.GetColumnName(typeof(Domain), nameof(Domain.IsActive))).AsBoolean()
            .WithColumn(NameCompatibilityManager.GetColumnName(typeof(Domain), nameof(Domain.TypeId))).AsInt32();
    }

    #endregion
}
