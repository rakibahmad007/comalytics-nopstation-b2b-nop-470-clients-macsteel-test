using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using Nop.Plugin.Widgets.AdditionalCategoryInfo.Domain;

namespace Nop.Plugin.Widgets.AdditionalCategoryInfo.Data.Migrations
{
    [NopMigration(
        "2025/01/21 17:30:17:6455422",
        "Widgets.AdditionalCategoryInfoData base schema",
        MigrationProcessType.Installation
    )]
    public class SchemaMigration : AutoReversingMigration
    {
        public override void Up()
        {
            Create.TableFor<AdditionalCategoryInfoData>();
        }
    };
}
