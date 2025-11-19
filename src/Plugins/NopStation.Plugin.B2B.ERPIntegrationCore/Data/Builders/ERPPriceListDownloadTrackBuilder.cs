using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Customers;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using System.Data;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Data.Builders;

public class ERPPriceListDownloadTrackBuilder : NopEntityBuilder<ERPPriceListDownloadTrack>
{
    #region Methods

    /// <summary>
    /// Apply entity configuration
    /// </summary>
    /// <param name="table">Create table expression builder</param> 
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(ERPPriceListDownloadTrack.NopCustomerId)).AsInt32().ForeignKey<Customer>(onDelete: Rule.None)
            .WithColumn(nameof(ERPPriceListDownloadTrack.B2BAccountId)).AsInt32().ForeignKey<ErpAccount>(onDelete: Rule.None)
            .WithColumn(nameof(ERPPriceListDownloadTrack.B2BSalesOrganisationId)).AsInt32().ForeignKey<ErpSalesOrg>(onDelete: Rule.None)
            .WithColumn(nameof(ERPPriceListDownloadTrack.DownloadedOnUtc)).AsDateTime2()
            .WithColumn(nameof(ERPPriceListDownloadTrack.PriceListDownloadTypeId)).AsInt32();
    }

    #endregion
}
