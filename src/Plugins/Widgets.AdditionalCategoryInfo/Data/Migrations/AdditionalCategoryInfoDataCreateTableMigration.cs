using FluentMigrator;
using Microsoft.AspNetCore.Http.HttpResults;
using Nop.Data.Extensions;
using Nop.Data.Mapping;
using Nop.Data.Migrations;
using Nop.Plugin.Widgets.AdditionalCategoryInfo.Domain;

namespace Nop.Plugin.Widgets.AdditionalCategoryInfo.Data.Migrations
{
    [NopMigration(
        "2025/01/21 17:34:12",
        "Nop.Plugin.Widgets.AdditionalCategoryInfo AdditionalCategoryInfoData CreateTable",
        MigrationProcessType.Update
    )]
    public class AdditionalCategoryInfoDataCreateTableMigration : AutoReversingMigration
    {
        #region Methods

        public override void Up()
        {
            if (
                !Schema
                    .Table(
                        NameCompatibilityManager.GetTableName(typeof(AdditionalCategoryInfoData))
                    )
                    .Exists()
            )
            {
                Create.TableFor<AdditionalCategoryInfoData>();
            }
        }

        #endregion
    }
}
