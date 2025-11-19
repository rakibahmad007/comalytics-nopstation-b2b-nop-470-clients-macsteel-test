using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Misc.ErpWebhook.Domain.ParallelTables;

namespace Nop.Plugin.Misc.ErpWebhook.Data.Builders
{
    public class CustomPictureBinaryForERPBuilder : NopEntityBuilder<Parallel_CustomPictureBinaryForERP>
    {
        #region Methods

        /// <summary>
        /// Configures the entity
        /// </summary>
        /// <param name="builder">The builder to be used to configure the entity</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(Parallel_CustomPictureBinaryForERP.NopPictureId)).AsInt32().PrimaryKey().ForeignKey("Picture", "Id").NotNullable()
                .WithColumn(nameof(Parallel_CustomPictureBinaryForERP.BinaryData)).AsBinary().NotNullable()
                .WithColumn(nameof(Parallel_CustomPictureBinaryForERP.LastUpdatedOn)).AsDateTime().Nullable();
        }

        #endregion
    }
}
