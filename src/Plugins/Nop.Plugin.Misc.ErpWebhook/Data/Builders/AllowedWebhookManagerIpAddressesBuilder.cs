using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Misc.ErpWebhook.Domain;

namespace Nop.Plugin.Misc.ErpWebhook.Data.Builders
{
    public class AllowedWebhookManagerIpAddressesBuilder : NopEntityBuilder<AllowedWebhookManagerIpAddresses>
    {
        #region Methods

        /// <summary>
        /// Configures the entity
        /// </summary>
        /// <param name="builder">The builder to be used to configure the entity</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(AllowedWebhookManagerIpAddresses.Id)).AsInt32().PrimaryKey().Identity();
        }
        #endregion
    }
}
