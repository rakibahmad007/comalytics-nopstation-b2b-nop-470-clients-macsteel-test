using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Mapping;
using Nop.Data.Migrations;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Data;

[NopMigration("2024/09/20 11:49:12", "NopStation.Plugin.B2B.ERPIntegrationCore ErpUserFavourite", MigrationProcessType.Update)]
public class ErpUserFavouriteCreateTableMigration : AutoReversingMigration
{
    #region Methods

    public override void Up()
    {
        if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(ErpUserFavourite))).Exists())
        {
            Create.TableFor<ErpUserFavourite>();
        }
    }

    #endregion
}