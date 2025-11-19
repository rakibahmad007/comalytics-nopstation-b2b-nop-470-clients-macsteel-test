using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Mapping;
using Nop.Data.Migrations;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Data;

[NopMigration("2024/11/12 10:44:16", "NopStation.Plugin.B2B.ERPIntegrationCore ErpSapErrorMsgUpdateMigration  Added table", MigrationProcessType.Update)]
public class ErpSapErrorMsgUpdateMigration : AutoReversingMigration
{
    #region Methods

    public override void Up()
    {

        if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(ERPSAPErrorMsgTranslation)))
            .Column(nameof(ERPSAPErrorMsgTranslation.ErrorMsg)).Exists())
        {
            Alter.Table(NameCompatibilityManager.GetTableName(typeof(ERPSAPErrorMsgTranslation)))
            .AddColumn(nameof(ERPSAPErrorMsgTranslation.ErrorMsg))
            .AsString()
            .Nullable();
        }
        else if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(ERPSAPErrorMsgTranslation))).Exists())
        {
            // Create the table if it doesn't exist
            Create.TableFor<ERPSAPErrorMsgTranslation>();
        }
    }

    #endregion
}
