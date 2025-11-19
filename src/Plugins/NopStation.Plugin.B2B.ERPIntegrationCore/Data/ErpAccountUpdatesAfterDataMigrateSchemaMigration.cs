using FluentMigrator;
using Nop.Data.Mapping;
using Nop.Data.Migrations;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Data;

[NopMigration("2024/07/17 09:44:00", "NopStation.Plugin.B2B.ERPIntegrationCore schema migration after data migration", MigrationProcessType.Update)]
public class ErpAccountUpdatesAfterDataMigrateSchemaMigration : AutoReversingMigration
{
    public override void Up()
    {
        if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(ErpAccount)))
            .Column(nameof(ErpAccount.PaymentTermsCode)).Exists())
        {
            Alter.Table(NameCompatibilityManager.GetTableName(typeof(ErpAccount)))
                .AddColumn(nameof(ErpAccount.PaymentTermsCode))
                .AsString(500)
                .Nullable();
        }

        if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(ErpAccount)))
            .Column(nameof(ErpAccount.PaymentTermsDescription)).Exists())
        {
            Alter.Table(NameCompatibilityManager.GetTableName(typeof(ErpAccount)))
                .AddColumn(nameof(ErpAccount.PaymentTermsDescription))
                .AsString(500)
                .Nullable();
        }

        if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(ErpAccount)))
            .Column(nameof(ErpAccount.Comment)).Exists())
        {
            Alter.Table(NameCompatibilityManager.GetTableName(typeof(ErpAccount)))
                .AddColumn(nameof(ErpAccount.Comment))
                .AsString(500)
                .Nullable();
        }
    }
}