using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Data.Mapping;
using Nop.Plugin.Widgets.AdditionalCategoryInfo.Domain;

namespace Nop.Plugin.Widgets.AdditionalCategoryInfo.Data.Builders
{
    public class AdditionalCategoryInfoDataBuilder : NopEntityBuilder<AdditionalCategoryInfoData>
    {
        #region Methods
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(
                    NameCompatibilityManager.GetColumnName(
                        typeof(AdditionalCategoryInfoData),
                        nameof(AdditionalCategoryInfoData.CategoryId)
                    )
                )
                .AsInt32()
                .WithColumn(
                    NameCompatibilityManager.GetColumnName(
                        typeof(AdditionalCategoryInfoData),
                        nameof(AdditionalCategoryInfoData.AdditionalInfoField)
                    )
                )
                .AsString()
                .WithColumn(
                    NameCompatibilityManager.GetColumnName(
                        typeof(AdditionalCategoryInfoData),
                        nameof(AdditionalCategoryInfoData.Active)
                    )
                )
                .AsBoolean();
        }
        #endregion
    }
}

