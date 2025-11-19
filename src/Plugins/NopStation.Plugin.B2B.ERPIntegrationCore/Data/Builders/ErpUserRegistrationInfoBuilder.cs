using System.Data;
using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Data.Builders;

public class ErpUserRegistrationInfoBuilder : NopEntityBuilder<ErpUserRegistrationInfo>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(ErpUserRegistrationInfo.NopCustomerId)).AsInt32().ForeignKey<Customer>(onDelete: Rule.None)
            .WithColumn(nameof(ErpUserRegistrationInfo.ErpSalesOrgId)).AsInt32().ForeignKey<ErpSalesOrg>(onDelete: Rule.None)
            .WithColumn(nameof(ErpUserRegistrationInfo.ErpUserId)).AsInt32().ForeignKey<ErpNopUser>(onDelete: Rule.None)
            .WithColumn(nameof(ErpUserRegistrationInfo.NearestWareHouseId)).AsInt32()
            .WithColumn(nameof(ErpUserRegistrationInfo.AddressId)).AsInt32().ForeignKey<Address>(onDelete: Rule.None)
            .WithColumn(nameof(ErpUserRegistrationInfo.DeliveryOptionId)).AsInt32()
            .WithColumn(nameof(ErpUserRegistrationInfo.DistanceToNearestWarehouse)).AsDecimal(18, 6).Nullable()
            .WithColumn(nameof(ErpUserRegistrationInfo.Longitude)).AsString(200).Nullable()
            .WithColumn(nameof(ErpUserRegistrationInfo.Latitude)).AsString(200).Nullable()
            .WithColumn(nameof(ErpUserRegistrationInfo.HouseNumber)).AsString(50).Nullable()
            .WithColumn(nameof(ErpUserRegistrationInfo.Street)).AsString(200).Nullable()
            .WithColumn(nameof(ErpUserRegistrationInfo.Suburb)).AsString(200).Nullable()
            .WithColumn(nameof(ErpUserRegistrationInfo.City)).AsString(200).Nullable()
            .WithColumn(nameof(ErpUserRegistrationInfo.PostalCode)).AsString(50).Nullable()
            .WithColumn(nameof(ErpUserRegistrationInfo.Country)).AsString(200).Nullable()
            .WithColumn(nameof(ErpUserRegistrationInfo.ErrorMessage)).AsString(500).Nullable()
            .WithColumn(nameof(ErpUserRegistrationInfo.SpecialInstructions)).AsString(500).Nullable()
            .WithColumn(nameof(ErpUserRegistrationInfo.QueuedEmailInfo)).AsString(500).Nullable()
            .WithColumn(nameof(ErpUserRegistrationInfo.AuthorisationFullName)).AsString(200).Nullable()
            .WithColumn(nameof(ErpUserRegistrationInfo.AuthorisationContactNumber)).AsString(50).Nullable()
            .WithColumn(nameof(ErpUserRegistrationInfo.AuthorisationAlternateContactNumber)).AsString(50).Nullable()
            .WithColumn(nameof(ErpUserRegistrationInfo.PersonalAlternateContactNumber)).AsString(50).Nullable()
            .WithColumn(nameof(ErpUserRegistrationInfo.AuthorisationJobTitle)).AsString(200).Nullable()
            .WithColumn(nameof(ErpUserRegistrationInfo.AuthorisationAdditionalComment)).AsString(500).Nullable()
            .WithColumn(nameof(ErpUserRegistrationInfo.ErpAccountIdForB2C)).AsInt32().Nullable()
            .WithColumn(nameof(ErpUserRegistrationInfo.ErpSalesOrganisationIds)).AsString(200).Nullable()
            .WithColumn(nameof(ErpUserRegistrationInfo.ErpAccountNumber)).AsString(200).Nullable();
    }
}
