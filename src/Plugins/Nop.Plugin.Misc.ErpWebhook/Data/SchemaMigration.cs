using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using Nop.Plugin.Misc.ErpWebhook.Domain;
using Nop.Plugin.Misc.ErpWebhook.Domain.ParallelTables;

namespace Nop.Plugin.Misc.ErpWebhook.Data;
[NopMigration("2024/10/08 12:10:00", "Nop.Plugin.Misc.ErpWebhook base schema", MigrationProcessType.Installation)]
public class SchemaMigration : AutoReversingMigration
{
    #region Methods

    /// <summary>
    /// Collect the UP migration expressions
    /// </summary>
    public override void Up()
    {
        Create.TableFor<Parallel_ErpStock>();
        Create.TableFor<Parallel_ErpProduct>();
        Create.TableFor<Parallel_ErpOrder>();
        Create.TableFor<Parallel_ErpShipToAddress>();
        Create.TableFor<Parallel_ErpAccount>();
        Create.TableFor<Parallel_ErpAccountPricing>();
        Create.TableFor<Parallel_CustomPictureBinaryForERP>();
        Create.TableFor<AllowedWebhookManagerIpAddresses>();
    }

    #endregion
}