using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Data.Builders;

public class ERPSAPErrorMsgTranslationMapBuilder : NopEntityBuilder<ERPSAPErrorMsgTranslation>
{
    #region Methods

    /// <summary>
    /// Configures the entity
    /// </summary>
    /// <param name="builder">The builder to be used to configure the entity</param>
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(ERPSAPErrorMsgTranslation.ErrorMsg)).AsString().NotNullable()
            .WithColumn(nameof(ERPSAPErrorMsgTranslation.UserTranslation)).AsString().NotNullable();
    }

    #endregion
}
