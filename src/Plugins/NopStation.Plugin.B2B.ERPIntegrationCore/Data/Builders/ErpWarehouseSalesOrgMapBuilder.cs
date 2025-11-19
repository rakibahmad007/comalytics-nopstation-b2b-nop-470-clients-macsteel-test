using FluentMigrator.Builders.Create.Table;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using System.Data;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Data.Builders;

public class ErpWarehouseSalesOrgMapBuilder : NopEntityBuilder<ErpWarehouseSalesOrgMap>
{
    #region Methods

    /// <summary>
    /// Apply entity configuration
    /// </summary>
    /// <param name="table">Create table expression builder</param> 
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(ErpWarehouseSalesOrgMap.NopWarehouseId)).AsInt32()
            .WithColumn(nameof(ErpWarehouseSalesOrgMap.ErpSalesOrgId)).AsInt32().ForeignKey<ErpSalesOrg>(onDelete: Rule.None)
            .WithColumn(nameof(ErpWarehouseSalesOrgMap.WarehouseCode)).AsString().Nullable()
            .WithColumn(nameof(ErpWarehouseSalesOrgMap.LastSyncedOnUtc)).AsDateTime2().Nullable()
            .WithColumn(nameof(ErpWarehouseSalesOrgMap.IsB2CWarehouse)).AsBoolean().WithDefaultValue(false);
    }

    #endregion
}
