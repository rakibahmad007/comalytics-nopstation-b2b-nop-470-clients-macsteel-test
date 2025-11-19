using FluentMigrator;
using Nop.Core;
using Nop.Data.Mapping;
using Nop.Data.Migrations;
using NopStation.Plugin.B2B.ErpDataScheduler.Domain;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Data.Migration;

[NopMigration("2025/01/25 12:46:51:6999999", "ErpDataScheduler synctask alter schema", MigrationProcessType.Update)]
public class SyncTaskTableAlterSchemaMigration : AutoReversingMigration
{
    public static string TableName<T>() where T : BaseEntity
    {
        return NameCompatibilityManager.GetTableName(typeof(T));
    }

    public override void Up()
    {
        if(Schema.Table(TableName<SyncTask>()).Exists())
        {
            var columnExist = Schema.Table(TableName<SyncTask>())
                .Column(nameof(SyncTask.QuartzJobName)).Exists();

            if(!columnExist)
            {
                Create.Column(nameof(SyncTask.QuartzJobName))
                    .OnTable(NameCompatibilityManager.GetTableName(typeof(SyncTask)))
                    .AsString()
                    .Nullable();
            }
        }
    }
}
