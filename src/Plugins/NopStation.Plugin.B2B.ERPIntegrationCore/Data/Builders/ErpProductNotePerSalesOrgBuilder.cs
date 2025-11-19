using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Data.Builders;

public class ErpProductNotePerSalesOrgBuilder : NopEntityBuilder<ErpProductNotePerSalesOrg>
{
    #region Methods

    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(ErpProductNotePerSalesOrg.ProductId)).AsInt32()
            .WithColumn(nameof(ErpProductNotePerSalesOrg.SalesOrgId)).AsInt32()
            .WithColumn(nameof(ErpProductNotePerSalesOrg.ProductNotes)).AsString(int.MaxValue); 
    }

    #endregion
}
