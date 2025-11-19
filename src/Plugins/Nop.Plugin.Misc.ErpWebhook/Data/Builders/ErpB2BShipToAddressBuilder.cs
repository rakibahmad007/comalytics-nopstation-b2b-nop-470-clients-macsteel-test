using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Misc.ErpWebhook.Domain.ParallelTables;

namespace Nop.Plugin.Misc.ErpWebhook.Data.Builders
{
    public class ErpB2BShipToAddressBuilder : NopEntityBuilder<Parallel_ErpShipToAddress>
    {
        #region Methods

        /// <summary>
        /// Configures the entity
        /// </summary>
        /// <param name="builder">The builder to be used to configure the entity</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(Parallel_ErpShipToAddress.ShipToCode)).AsString(50)
                .WithColumn(nameof(Parallel_ErpShipToAddress.ShipToName)).AsString(100)
                .WithColumn(nameof(Parallel_ErpShipToAddress.AddressId)).AsInt32()
                .WithColumn(nameof(Parallel_ErpShipToAddress.Suburb)).AsString(100)
                .WithColumn(nameof(Parallel_ErpShipToAddress.ProvinceCode)).AsString(100)
                .WithColumn(nameof(Parallel_ErpShipToAddress.DeliveryNotes)).AsString(500)
                .WithColumn(nameof(Parallel_ErpShipToAddress.EmailAddresses)).AsString(500)
                .WithColumn(nameof(Parallel_ErpShipToAddress.B2BAccountId)).AsInt32()
                .WithColumn(nameof(Parallel_ErpShipToAddress.B2BSalesOrganisationId)).AsInt32()
                .WithColumn(nameof(Parallel_ErpShipToAddress.RepNumber)).AsString(50)
                .WithColumn(nameof(Parallel_ErpShipToAddress.RepFullName)).AsString(100)
                .WithColumn(nameof(Parallel_ErpShipToAddress.RepPhoneNumber)).AsString(50)
                .WithColumn(nameof(Parallel_ErpShipToAddress.RepEmail)).AsString(100)
                .WithColumn(nameof(Parallel_ErpShipToAddress.ShipToAddressCreatedByTypeId)).AsInt32()
                .WithColumn(nameof(Parallel_ErpShipToAddress.OrderId)).AsInt32();
        }

        #endregion
    }
}
