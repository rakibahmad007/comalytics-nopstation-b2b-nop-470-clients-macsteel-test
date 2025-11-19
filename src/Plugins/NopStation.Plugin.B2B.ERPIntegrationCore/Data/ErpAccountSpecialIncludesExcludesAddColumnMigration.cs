using FluentMigrator;
using Nop.Data.Mapping;
using Nop.Data.Migrations;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Data;

[NopMigration("2024/12/25 12:07:12", "NopStation.Plugin.B2B.ERPIntegrationCore ErpAccount SpecialIncludesExcludes AddColumn", MigrationProcessType.Update)]
public class ErpAccountSpecialIncludesExcludesAddColumnMigration : AutoReversingMigration
{
    #region Methods

    public override void Up()
    {
        if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(ErpAccount)))
            .Column(nameof(ErpAccount.SpecialIncludes)).Exists())
        {
            Alter.Table(NameCompatibilityManager.GetTableName(typeof(ErpAccount)))
                .AddColumn(nameof(ErpAccount.SpecialIncludes))
                .AsString(int.MaxValue)
                .Nullable();
        }

        if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(ErpAccount)))
        .Column(nameof(ErpAccount.SpecialExcludes)).Exists())
        {
            Alter.Table(NameCompatibilityManager.GetTableName(typeof(ErpAccount)))
                .AddColumn(nameof(ErpAccount.SpecialExcludes))
                .AsString(int.MaxValue)
                .Nullable();
        }
    }

    #endregion
}