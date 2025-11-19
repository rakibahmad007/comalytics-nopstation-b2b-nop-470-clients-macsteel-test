using FluentMigrator.Builders.Create.Table;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using System.Data;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Data.Builders;

public class ErpNopUserBuilder : NopEntityBuilder<ErpNopUser>
{
    #region Methods

    /// <summary>
    /// Apply entity configuration
    /// </summary>
    /// <param name="table">Create table expression builder</param>
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(ErpNopUser.NopCustomerId)).AsInt32() 
            .WithColumn(nameof(ErpNopUser.ErpAccountId)).AsInt32().ForeignKey<ErpAccount>(onDelete: Rule.None)
            .WithColumn(nameof(ErpNopUser.ErpShipToAddressId)).AsInt32().ForeignKey<ErpShipToAddress>(onDelete: Rule.None)
            .WithColumn(nameof(ErpNopUser.BillingErpShipToAddressId)).AsInt32()
            .WithColumn(nameof(ErpNopUser.ShippingErpShipToAddressId)).AsInt32()
            .WithColumn(nameof(ErpNopUser.ErpUserTypeId)).AsInt32()
            .WithColumn(nameof(ErpNopUser.SalesOrgAsCustomerRoleId)).AsString(100)
            .WithColumn(nameof(ErpNopUser.RegistrationAuthorisedBy)).AsString(500)
            .WithColumn(nameof(ErpNopUser.LastWarehouseCalculationTimeUtc)).AsDateTime2().Nullable()
            .WithColumn(nameof(ErpNopUser.TotalSavingsForthisYear)).AsDecimal(18, 4).Nullable()
            .WithColumn(nameof(ErpNopUser.TotalSavingsForthisYearUpdatedOnUtc)).AsDateTime2().Nullable()
            .WithColumn(nameof(ErpNopUser.TotalSavingsForAllTime)).AsDecimal(18, 4).Nullable()
            .WithColumn(nameof(ErpNopUser.TotalSavingsForAllTimeUpdatedOnUtc)).AsDateTime2().Nullable();
    }

    #endregion
}
