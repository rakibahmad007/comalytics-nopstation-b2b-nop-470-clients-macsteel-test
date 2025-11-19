using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Data.Builders;

public class ErpSalesOrgBuilder : NopEntityBuilder<ErpSalesOrg>
{
    #region Methods

    /// <summary>
    /// Apply entity configuration
    /// </summary>
    /// <param name="table">Create table expression builder</param>
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(ErpSalesOrg.Code)).AsString()
            .WithColumn(nameof(ErpSalesOrg.Name)).AsString()
            .WithColumn(nameof(ErpSalesOrg.Email)).AsString()
            .WithColumn(nameof(ErpSalesOrg.AddressId)).AsInt32()
            .WithColumn(nameof(ErpSalesOrg.IntegrationClientId)).AsString().Nullable()
            .WithColumn(nameof(ErpSalesOrg.AuthenticationKey)).AsString().Nullable()
            .WithColumn(nameof(ErpSalesOrg.ErpAccountIdForB2C)).AsInt32().Nullable()
            .WithColumn(nameof(ErpSalesOrg.Suburb)).AsString(500).Nullable()
            .WithColumn(nameof(ErpSalesOrg.NoItemsMessage)).AsString(500).Nullable()
            .WithColumn(nameof(ErpSalesOrg.ShowWeightOnTheCheckoutScreen)).AsBoolean()
            .WithColumn(nameof(ErpSalesOrg.ServerBaseURL)).AsString(500).Nullable()
            .WithColumn(nameof(ErpSalesOrg.SpecialsCategoryId)).AsInt32().Nullable()
            .WithColumn(nameof(ErpSalesOrg.UserRegistrationEmailAdresses)).AsString(500).Nullable()
            .WithColumn(nameof(ErpSalesOrg.LastTimeSyncOnUtc)).AsDateTime2().Nullable()
            .WithColumn(nameof(ErpSalesOrg.TradingWarehouseId)).AsInt32().Nullable()
            .WithColumn(nameof(ErpSalesOrg.LastErpAccountSyncTimeOnUtc)).AsDateTime2().Nullable()
            .WithColumn(nameof(ErpSalesOrg.LastErpGroupPriceSyncTimeOnUtc)).AsDateTime2().Nullable()
            .WithColumn(nameof(ErpSalesOrg.LastErpShipToAddressSyncTimeOnUtc)).AsDateTime2().Nullable()
            .WithColumn(nameof(ErpSalesOrg.LastErpProductSyncTimeOnUtc)).AsDateTime2().Nullable()
            .WithColumn(nameof(ErpSalesOrg.LastErpStockSyncTimeOnUtc)).AsDateTime2().Nullable();
    }

    #endregion
}
