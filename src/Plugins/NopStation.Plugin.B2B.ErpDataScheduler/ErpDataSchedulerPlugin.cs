using Microsoft.Data.SqlClient;
using Nop.Core;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.ScheduleTasks;
using Nop.Data;
using Nop.Services.Common;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Plugins;
using Nop.Services.ScheduleTasks;
using Nop.Web.Framework.Menu;
using NopStation.Plugin.B2B.ErpDataScheduler.Domain;
using NopStation.Plugin.B2B.ErpDataScheduler.Services.NopStationSyncServices;
using NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices;
using NopStation.Plugin.Misc.Core.Services;

namespace NopStation.Plugin.B2B.ErpDataScheduler;

public class ErpDataSchedulerPlugin : BasePlugin, IAdminMenuPlugin, IMiscPlugin, INopStationPlugin
{
    #region Fields

    private readonly ILocalizationService _localizationService;
    private readonly ISyncTaskService _syncTaskService;
    private readonly IScheduleTaskService _scheduleTaskService;
    private readonly IWebHelper _webHelper;
    private readonly INopStationScheduler _nopStationScheduler;
    private readonly IMessageTemplateService _messageTemplateService;
    private readonly IEmailAccountService _emailAccountService;
    private readonly INopDataProvider _nopDataProvider;
    private const string CONFIG_PAGE_URL_EXTENSION = "Admin/ErpDataScheduler/Configure";
    private const string THIRD_PARTY_PLUGINS = "Third party plugins";
    private const string PLUGIN_SYSTEM_NAME = "NopStation.ErpDataScheduler";
    private const string PLUGIN_TITLE = "Erp Data Scheduler";
    private const string PLUGIN_ICON_CLASS = "nav-icon fas fa-cube";
    private const bool PLUGIN_VISIBLE = true;
    private const string CHILD_NODE_CONFIG_SYSTEM_NAME =
        "NopStation.ErpDataScheduler.Configuration";
    private const string CHILD_NODE_CONFIG_TITLE = "Configuration";
    private const string CHILD_NODE_CONFIG_CONTROLLER_NAME = "ErpDataScheduler";
    private const string CHILD_NODE_CONFIG_ACTION_NAME = "Configure";
    private const string CHILD_NODE_CONFIG_ICON_CLASS = "nav-icon fas fa-cogs";
    private const bool CHILD_NODE_CONFIG_VISIBLE = true;

    private const string CHILD_NODE_PARTIALSYNC_SYSTEM_NAME =
        "NopStation.ErpDataScheduler.PartialSync";
    private const string CHILD_NODE_PARTIALSYNC_TITLE = "Partial Sync";
    private const string CHILD_NODE_PARTIALSYNC_CONTROLLER_NAME = "PartialSync";
    private const string CHILD_NODE_PARTIALSYNC_ACTION_NAME = "Index";
    private const string CHILD_NODE_PARTIALSYNC_ICON_CLASS = "nav-icon fas fa-sync-alt";
    private const bool CHILD_NODE_PARTIALSYNC_VISIBLE = true;

    private const string CHILD_NODE_SYNCTASK_SYSTEM_NAME =
        "NopStation.ErpDataScheduler.SyncTaskList";
    private const string CHILD_NODE_SYNCTASK_TITLE = "Sync Task List";
    private const string CHILD_NODE_SYNCTASK_CONTROLLER_NAME = "SyncTasks";
    private const string CHILD_NODE_SYNCTASK_ACTION_NAME = "List";
    private const string CHILD_NODE_SYNCTASK_ICON_CLASS = "nav-icon fas fa-list";
    private const bool CHILD_NODE_SYNCTASK_VISIBLE = true;

    #endregion

    #region Ctor

    public ErpDataSchedulerPlugin(
        ILocalizationService localizationService,
        ISyncTaskService taskService,
        IWebHelper webHelper,
        IScheduleTaskService scheduleTaskService,
        INopStationScheduler nopStationScheduler,
        IMessageTemplateService messageTemplateService,
        IEmailAccountService emailAccountService,
        INopDataProvider nopDataProvider
    )
    {
        _localizationService = localizationService;
        _webHelper = webHelper;
        _syncTaskService = taskService;
        _scheduleTaskService = scheduleTaskService;
        _nopStationScheduler = nopStationScheduler;
        _messageTemplateService = messageTemplateService;
        _emailAccountService = emailAccountService;
        _nopDataProvider = nopDataProvider;
    }

    #endregion

    #region Methods

    public override string GetConfigurationPageUrl()
    {
        return $"{_webHelper.GetStoreLocation()}{CONFIG_PAGE_URL_EXTENSION}";
    }

    public async Task ManageSiteMapAsync(SiteMapNode rootNode)
    {
        var pluginNode = rootNode.ChildNodes.FirstOrDefault(x =>
            x.SystemName == THIRD_PARTY_PLUGINS
        );

        if (pluginNode is null)
        {
            return;
        }

        pluginNode.ChildNodes.Add(
            new()
            {
                SystemName = PLUGIN_SYSTEM_NAME,
                Title = PLUGIN_TITLE,
                IconClass = PLUGIN_ICON_CLASS,
                Visible = PLUGIN_VISIBLE,
                ChildNodes = new List<SiteMapNode>()
                {
                    new()
                    {
                        SystemName = CHILD_NODE_CONFIG_SYSTEM_NAME,
                        Title = CHILD_NODE_CONFIG_TITLE,
                        ControllerName = CHILD_NODE_CONFIG_CONTROLLER_NAME,
                        ActionName = CHILD_NODE_CONFIG_ACTION_NAME,
                        IconClass = CHILD_NODE_CONFIG_ICON_CLASS,
                        Visible = CHILD_NODE_CONFIG_VISIBLE,
                    },
                    new()
                    {
                        SystemName = CHILD_NODE_PARTIALSYNC_SYSTEM_NAME,
                        Title = CHILD_NODE_PARTIALSYNC_TITLE,
                        ControllerName = CHILD_NODE_PARTIALSYNC_CONTROLLER_NAME,
                        ActionName = CHILD_NODE_PARTIALSYNC_ACTION_NAME,
                        IconClass = CHILD_NODE_PARTIALSYNC_ICON_CLASS,
                        Visible = CHILD_NODE_PARTIALSYNC_VISIBLE,
                    },
                    new()
                    {
                        SystemName = CHILD_NODE_SYNCTASK_SYSTEM_NAME,
                        Title = CHILD_NODE_SYNCTASK_TITLE,
                        ControllerName = CHILD_NODE_SYNCTASK_CONTROLLER_NAME,
                        ActionName = CHILD_NODE_SYNCTASK_ACTION_NAME,
                        IconClass = CHILD_NODE_SYNCTASK_ICON_CLASS,
                        Visible = CHILD_NODE_SYNCTASK_VISIBLE,
                    },
                },
            }
        );
    }

    public override async Task UninstallAsync()
    {
        var erpDataSchedulerTasks = await _syncTaskService.GetAllTasksAsync();

        if (erpDataSchedulerTasks.Count > 0)
        {
            foreach (var erpDataSchedulerTask in erpDataSchedulerTasks)
            {
                await _syncTaskService.DeleteTaskAsync(erpDataSchedulerTask);
            }
        }

        await this.UninstallPluginAsync();

        await base.UninstallAsync();
    }

    public override async Task InstallAsync()
    {
        await this.InstallPluginAsync();

        await InsertSyncTasksAsync();

        await InsertSyncFailedNotificationMessageTemplate();

        await RegisterScheduleJobsAsync();

        await base.InstallAsync();
    }

    public override async Task UpdateAsync(string currentVersion, string targetVersion)
    {
        if (targetVersion == currentVersion)
            return;

        #region Remove Seconds and StopOnError columns from Erp_Data_Sync_Task table

        var connectionString = new SqlConnectionStringBuilder(
            DataSettingsManager.LoadSettings().ConnectionString
        );
        var databaseName = connectionString.InitialCatalog;

        var syncTaskSecondsColumnCheckCommand =
            "SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS "
            + "WHERE TABLE_SCHEMA = 'dbo' AND "
            + "TABLE_NAME = 'Erp_Data_Sync_Task' AND "
            + "COLUMN_NAME = 'Seconds';";

        var secondsColumnExists = await _nopDataProvider.QueryAsync<int>(
            syncTaskSecondsColumnCheckCommand
        );
        if (secondsColumnExists.Count > 0 && secondsColumnExists.FirstOrDefault() > 0)
        {
            await _nopDataProvider.ExecuteNonQueryAsync(
                $"ALTER TABLE [{databaseName}].[dbo].[Erp_Data_Sync_Task] DROP COLUMN Seconds;"
            );
        }

        var syncTaskStopOnErrorColumnCheckCommand =
            "SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS "
            + "WHERE TABLE_SCHEMA = 'dbo' AND "
            + "TABLE_NAME = 'Erp_Data_Sync_Task' AND "
            + "COLUMN_NAME = 'StopOnError';";

        var stopOnErrorColumnExists = await _nopDataProvider.QueryAsync<int>(
            syncTaskStopOnErrorColumnCheckCommand
        );
        if (stopOnErrorColumnExists.Count > 0 && stopOnErrorColumnExists.FirstOrDefault() > 0)
        {
            await _nopDataProvider.ExecuteNonQueryAsync(
                $"ALTER TABLE [{databaseName}].[dbo].[Erp_Data_Sync_Task] DROP COLUMN StopOnError;"
            );
        }

        #endregion

        await InsertSyncTasksAsync();

        await RegisterScheduleJobsAsync();

        await InsertSyncFailedNotificationMessageTemplate();

        var keyValuePairs = PluginResouces().ToDictionary(kv => kv.Key, kv => kv.Value);
        foreach (var keyValuePair in keyValuePairs)
        {
            await _localizationService.AddOrUpdateLocaleResourceAsync(
                keyValuePair.Key,
                keyValuePair.Value
            );
        }

        await base.UpdateAsync(currentVersion, targetVersion);
    }

    public List<KeyValuePair<string, string>> PluginResouces()
    {
        return new List<KeyValuePair<string, string>>
        {
            new(
                "Plugin.Misc.NopStation.ErpDataScheduler.Admin.Configure",
                "Erp Data Scheduler Configuration"
            ),
            new("Plugin.Misc.NopStation.ErpDataScheduler.Admin.Configure.Block.General", "General"),
            new(
                "Plugin.Misc.NopStation.ErpDataScheduler.Admin.Configure.Fields.NeedQuoteOrderCall",
                "Need Quote Order Call"
            ),
            new(
                "Plugin.Misc.NopStation.ErpDataScheduler.Admin.SyncTaskList",
                "Erp Data Scheduler Sync Tasks"
            ),
            new(
                "Plugin.Misc.NopStation.ErpDataScheduler.Admin.Configure.Fields.EnalbeSendingEmailNotificationToStoreOwnerOnSyncError",
                "Enable sending email notification to store owner on sync error"
            ),
            new(
                "Plugin.Misc.NopStation.ErpDataScheduler.Admin.Configure.Fields.EnalbeSendingEmailNotificationToStoreOwnerOnSyncError.Hint",
                "Enable sending email notification to store owner on sync error"
            ),
            new(
                "Plugin.Misc.NopStation.ErpDataScheduler.Admin.Configure.Fields.AdditionalEmailAddresses",
                "Additional Email Addresses"
            ),
            new(
                "Plugin.Misc.NopStation.ErpDataScheduler.Admin.Configure.Fields.AdditionalEmailAddresses.Hint",
                "Configure the email addresses (semi-colon separated) to which the sync related notification will be sent"
            ),
            new(
                "Plugin.Misc.NopStation.ErpDataScheduler.Admin.Configure.Fields.SpecSheetLocation",
                "SpecSheet Location"
            ),
            new(
                "Plugin.Misc.NopStation.ErpDataScheduler.Admin.Configure.Fields.SpecSheetLocation.Hint",
                "SpecSheet Download Location"
            ),
            new("Plugin.Misc.NopStation.ErpDataScheduler.Tasks.Name", "Name"),
            new("Plugin.Misc.NopStation.ErpDataScheduler.Tasks.Enabled", "Enabled"),
            new(
                "Plugin.Misc.NopStation.ErpDataScheduler.Tasks.IsRunning",
                "The schedule task is running"
            ),
            new("Plugin.Misc.NopStation.ErpDataScheduler.Tasks.LastStart", "Last Start"),
            new("Plugin.Misc.NopStation.ErpDataScheduler.Tasks.LastEnd", "Last End"),
            new("Plugin.Misc.NopStation.ErpDataScheduler.Tasks.LastSuccess", "Last Success"),
            new("Plugin.Misc.NopStation.ErpDataScheduler.Tasks.SlotsAndLogs", "Slots and Logs"),
            new("Plugin.Misc.NopStation.ErpDataScheduler.Tasks.RunNow", "Run Now"),
            new(
                "Plugin.Misc.NopStation.ErpDataScheduler.Tasks.RunNow.Progress",
                "Running the schedule task"
            ),
            new(
                "Plugin.Misc.NopStation.ErpDataScheduler.Tasks.RunNow.Done",
                "Schedule task was run"
            ),
            new(
                "Plugin.Misc.NopStation.ErpDataScheduler.Tasks.Edit.Details",
                "Edit the Scheduler Settings"
            ),
            new("Plugin.Misc.NopStation.ErpDataScheduler.Tasks.BackToList", "Back to list"),
            new(
                "Plugin.Misc.NopStation.ErpDataScheduler.Tasks.Scheduler.Settings",
                "Scheduler Settings"
            ),
            new(
                "Plugin.Misc.NopStation.ErpDataScheduler.Tasks.Error",
                "The \"{0}\" scheduled task failed with the \"{1}\" error. Task type: \"{2}\". Store name: \"{3}\". Task run address: \"{4}\"."
            ),
            new(
                "Plugin.Misc.NopStation.ErpDataScheduler.Tasks.TimeoutError",
                "A scheduled task canceled. Timeout expired."
            ),
            new(
                "Plugin.Misc.NopStation.ErpDataScheduler.Tasks.Execute.Succeded",
                "\"{0}\" has started. Sync is running now"
            ),
            new(
                "Plugin.Misc.NopStation.ErpDataScheduler.Tasks.Terminate",
                "The scheduled task cancellation is in progress, this might take few time."
            ),
            new(
                "Plugin.Misc.NopStation.ErpDataScheduler.ErpActivityLogs.EditConfigurations",
                "Edit Erp Data Scheduler plugin configurations"
            ),
            new(
                "Plugin.Misc.NopStation.ErpDataScheduler.ErpActivityLogs.EditSyncTask",
                "Edited a Sync task. (ID = \"{0}\", Name = \"{1}\")"
            ),
            new(
                "Plugin.Misc.NopStation.ErpDataScheduler.Configuration.Updated",
                "The settings have been updated successfully."
            ),
            new("Plugin.Misc.NopStation.ErpDataScheduler.SyncLogs.FileName", "File Name"),
            new("Plugin.Misc.NopStation.ErpDataScheduler.SyncLogs.FileSize", "File Size"),
            new("Plugin.Misc.NopStation.ErpDataScheduler.SyncLogs.FileDownloadLink", "Download"),
            new("Plugin.Misc.NopStation.ErpDataScheduler.SyncLogs.FileDelete", "Delete"),
            new("Plugin.Misc.NopStation.ErpDataScheduler.SyncLogs.SyncTaskLogs", "Sync Task Logs"),
            new("Plugin.Misc.NopStation.ErpDataScheduler.SyncLogs.FileNotFound", "File not found"),
            new(
                "Plugin.Misc.NopStation.ErpDataScheduler.SyncLogs.FileCouldNotBeDeleted",
                "File could not be deleted"
            ),
            new(
                "Plugin.Misc.NopStation.ErpDataScheduler.PartialSync.ErpAccountNumber",
                "Erp Account Number"
            ),
            new(
                "Plugin.Misc.NopStation.ErpDataScheduler.PartialSync.ErpAccountNumber.Hint",
                "Erp Account Number"
            ),
            new("Plugin.Misc.NopStation.ErpDataScheduler.PartialSync.RouteCode", "Route Code"),
            new("Plugin.Misc.NopStation.ErpDataScheduler.PartialSync.RouteCode.Hint", "Route Code"),
            new("Plugin.Misc.NopStation.ErpDataScheduler.PartialSync.PriceCode", "Price Code"),
            new("Plugin.Misc.NopStation.ErpDataScheduler.PartialSync.PriceCode.Hint", "Price Code"),
            new("Plugin.Misc.NopStation.ErpDataScheduler.PartialSync.StockCode", "Stock Code"),
            new("Plugin.Misc.NopStation.ErpDataScheduler.PartialSync.StockCode.Hint", "Stock Code"),
            new(
                "NopStation.Plugin.B2B.ErpDataScheduler.PartialSync.SalesOrgCode",
                "Sales Org Code"
            ),
            new(
                "NopStation.Plugin.B2B.ErpDataScheduler.PartialSync.SalesOrgCode.Hint",
                "Sales Org Code"
            ),
            new("Plugin.Misc.NopStation.ErpDataScheduler.PartialSync.OrderNumber", "Order Number"),
            new(
                "Plugin.Misc.NopStation.ErpDataScheduler.PartialSync.OrderNumber.Hint",
                "Order Number"
            ),
            new("Plugin.Misc.NopStation.ErpDataScheduler.PartialSync.SyncNow", "Sync Now"),
            new(
                "Plugin.Misc.NopStation.ErpDataScheduler.PartialSync.Error.StockCodeEmptyInputError",
                "Stock Code cannot be empty"
            ),
            new(
                "Plugin.Misc.NopStation.ErpDataScheduler.PartialSync.Error.RouteCodeEmptyInputError",
                "Route Code cannot be empty"
            ),
            new(
                "Plugin.Misc.NopStation.ErpDataScheduler.PartialSync.Error.ErpAccountNumberEmptyInputError",
                "Erp Account Number cannot be empty"
            ),
            new(
                "Plugin.Misc.NopStation.ErpDataScheduler.PartialSync.Error.PriceCodeAndStockCodeEmptyInputError",
                "Both Price Code and Stock Code cannot be empty"
            ),
            new(
                "Plugin.Misc.NopStation.ErpDataScheduler.PartialSync.Error.ErpAccountNumberAndStockCodeEmptyInputError",
                "Both Erp Account Number and Stock Code cannot be empty"
            ),
            new(
                "Plugin.Misc.NopStation.ErpDataScheduler.PartialSync.Error.ErpAccountNumberAndOrderNumberEmptyInputError",
                "Both Erp Account Number and Order Number cannot be empty"
            ),
            new(
                "NopStation.Plugin.B2B.ErpDataScheduler.PartialSync.Error.SalesOrgCodeEmptyInputError",
                "Sales Org Code cannot be empty"
            ),
            new("Plugin.Misc.NopStation.ErpDataScheduler.Admin.PartialSync", "Partial Sync"),
            new(
                "Plugin.Misc.NopStation.ErpDataScheduler.Admin.PartialSync.ErpAccount",
                "Sync Erp Account"
            ),
            new(
                "Plugin.Misc.NopStation.ErpDataScheduler.Admin.PartialSync.ErpAccountCredit",
                "Sync Erp Account Credit"
            ),
            new(
                "Plugin.Misc.NopStation.ErpDataScheduler.Admin.PartialSync.ErpGroupPrice",
                "Sync Erp Group Price"
            ),
            new(
                "Plugin.Misc.NopStation.ErpDataScheduler.Admin.PartialSync.ErpInvoice",
                "Sync Erp Invoice"
            ),
            new(
                "Plugin.Misc.NopStation.ErpDataScheduler.Admin.PartialSync.ErpProduct",
                "Sync Erp Product"
            ),
            new(
                "Plugin.Misc.NopStation.ErpDataScheduler.Admin.PartialSync.ErpShipToAddress",
                "Sync Erp Ship To Address"
            ),
            new(
                "Plugin.Misc.NopStation.ErpDataScheduler.Admin.PartialSync.ErpSpecialPrice",
                "Sync Erp Special Price"
            ),
            new(
                "Plugin.Misc.NopStation.ErpDataScheduler.Admin.PartialSync.ErpStock",
                "Sync Erp Stock"
            ),
            new(
                "Plugin.Misc.NopStation.ErpDataScheduler.Admin.PartialSync.DeliveryRoute",
                "Sync Erp Delivery Route"
            ),
            new(
                "Plugin.Misc.NopStation.ErpDataScheduler.Admin.PartialSync.ErpOrder",
                "Sync Erp Order"
            ),
            new(
                "Plugin.Misc.NopStation.ErpDataScheduler.Admin.PartialSync.ErpSpecSheet",
                "Sync Erp SpecSheet"
            ),
            new(
                "Plugin.Misc.NopStation.ErpDataScheduler.Admin.PartialSync.TaskScheduledToExecute",
                "The task has been scheduled to execute. This might take a few minutes."
            ),
            new(
                "Plugin.Misc.NopStation.ErpDataScheduler.Admin.PartialSync.RequestedTaskNotFound",
                "The requested task was not found"
            ),
            new(
                "Plugin.Misc.NopStation.ErpDataScheduler.Admin.PartialSync.InternalServerError",
                "Internal server error"
            ),
            new("NopStation.Plugin.B2B.ErpDataScheduler.PartialSync.ShowLogs", "Show Logs"),
        };
    }

    #region Message Templates

    private async Task InsertSyncFailedNotificationMessageTemplate()
    {
        var messageTemplate = await _messageTemplateService.GetMessageTemplatesByNameAsync(
            ErpDataSchedulerDefaults.SyncFailedNotificationMessageTemplate
        );

        if (messageTemplate.Count == 0)
        {
            var emailAccount = await _emailAccountService.GetAllEmailAccountsAsync();

            var notificationMessageTemplate = new MessageTemplate
            {
                Name = ErpDataSchedulerDefaults.SyncFailedNotificationMessageTemplate,
                Subject =
                    "Critical Alert - The Scheduler Task %SyncNotification.SyncTaskName%, has stopped running due to an error",
                Body =
                    $"<p>{Environment.NewLine}"
                    + $"Dear Concern,{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}"
                    + $"This is an automated notification to alert you that the scheduler task <strong>%SyncNotification.SyncTaskName%</strong> has unexpectedly stopped running "
                    + $"on <strong>%SyncNotification.Datetime% UTC time</strong> due to the following error:{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}"
                    + $"%SyncNotification.Message%{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}"
                    + $"If you need assistance, please contact the IT support team.{Environment.NewLine}</p>{Environment.NewLine}",
                IsActive = true,
                EmailAccountId =
                    emailAccount.Count > 0 ? emailAccount.FirstOrDefault()?.Id ?? 0 : 0,
            };

            await _messageTemplateService.InsertMessageTemplateAsync(notificationMessageTemplate);
        }
    }

    #endregion

    #region Insert Sync Tasks

    private async Task InsertSyncTasksAsync()
    {
        #region Sync tasks installation

        if (
            (await _syncTaskService.GetTaskByTypeAsync(ErpDataSchedulerDefaults.ErpAccountSyncTask))
            is null
        )
        {
            await _syncTaskService.InsertTaskAsync(
                new SyncTask
                {
                    Enabled = false,
                    LastEnabledUtc = DateTime.UtcNow,
                    Name = ErpDataSchedulerDefaults.ErpAccountSyncTaskName,
                    Type = ErpDataSchedulerDefaults.ErpAccountSyncTask,
                    IsIncremental = false,
                }
            );
        }

        if (
            (await _syncTaskService.GetTaskByTypeAsync(ErpDataSchedulerDefaults.ErpInvoiceSyncTask))
            is null
        )
        {
            await _syncTaskService.InsertTaskAsync(
                new SyncTask
                {
                    Enabled = false,
                    LastEnabledUtc = DateTime.UtcNow,
                    Name = ErpDataSchedulerDefaults.ErpInvoiceSyncTaskName,
                    Type = ErpDataSchedulerDefaults.ErpInvoiceSyncTask,
                    IsIncremental = false,
                }
            );
        }

        if (
            (await _syncTaskService.GetTaskByTypeAsync(ErpDataSchedulerDefaults.ErpProductSyncTask))
            is null
        )
        {
            await _syncTaskService.InsertTaskAsync(
                new SyncTask
                {
                    Enabled = false,
                    LastEnabledUtc = DateTime.UtcNow,
                    Name = ErpDataSchedulerDefaults.ErpProductSyncTaskName,
                    Type = ErpDataSchedulerDefaults.ErpProductSyncTask,
                    IsIncremental = false,
                }
            );
        }

        if (
            (await _syncTaskService.GetTaskByTypeAsync(ErpDataSchedulerDefaults.ErpStockSyncTask))
            is null
        )
        {
            await _syncTaskService.InsertTaskAsync(
                new SyncTask
                {
                    Enabled = false,
                    LastEnabledUtc = DateTime.UtcNow,
                    Name = ErpDataSchedulerDefaults.ErpStockSyncTaskName,
                    Type = ErpDataSchedulerDefaults.ErpStockSyncTask,
                    IsIncremental = false,
                }
            );
        }

        if (
            await _syncTaskService.GetTaskByTypeAsync(
                ErpDataSchedulerDefaults.ErpSpecialPriceSyncTask
            )
            is null
        )
        {
            await _syncTaskService.InsertTaskAsync(
                new SyncTask
                {
                    Enabled = false,
                    LastEnabledUtc = DateTime.UtcNow,
                    Name = ErpDataSchedulerDefaults.ErpSpecialPriceSyncTaskName,
                    Type = ErpDataSchedulerDefaults.ErpSpecialPriceSyncTask,
                    IsIncremental = false,
                }
            );
        }

        if (
            await _syncTaskService.GetTaskByTypeAsync(
                ErpDataSchedulerDefaults.ErpGroupPriceSyncTask
            )
            is null
        )
        {
            await _syncTaskService.InsertTaskAsync(
                new SyncTask
                {
                    Enabled = false,
                    LastEnabledUtc = DateTime.UtcNow,
                    Name = ErpDataSchedulerDefaults.ErpGroupPriceSyncTaskName,
                    Type = ErpDataSchedulerDefaults.ErpGroupPriceSyncTask,
                    IsIncremental = false,
                }
            );
        }

        if (
            await _syncTaskService.GetTaskByTypeAsync(
                ErpDataSchedulerDefaults.ErpShipToAddressSyncTask
            )
            is null
        )
        {
            await _syncTaskService.InsertTaskAsync(
                new SyncTask
                {
                    Enabled = false,
                    LastEnabledUtc = DateTime.UtcNow,
                    Name = ErpDataSchedulerDefaults.ErpShipToAddressSyncTaskName,
                    Type = ErpDataSchedulerDefaults.ErpShipToAddressSyncTask,
                    IsIncremental = false,
                }
            );
        }

        if (
            (await _syncTaskService.GetTaskByTypeAsync(ErpDataSchedulerDefaults.ErpOrderSyncTask))
            is null
        )
        {
            await _syncTaskService.InsertTaskAsync(
                new SyncTask
                {
                    Enabled = false,
                    LastEnabledUtc = DateTime.UtcNow,
                    Name = ErpDataSchedulerDefaults.ErpOrderSyncTaskName,
                    Type = ErpDataSchedulerDefaults.ErpOrderSyncTask,
                    IsIncremental = false,
                }
            );
        }

        if (
            await _syncTaskService.GetTaskByTypeAsync(ErpDataSchedulerDefaults.ErpSpecSheetSyncTask)
            is null
        )
        {
            await _syncTaskService.InsertTaskAsync(
                new SyncTask
                {
                    Enabled = false,
                    LastEnabledUtc = DateTime.UtcNow,
                    Name = ErpDataSchedulerDefaults.ErpSpecSheetSyncTaskName,
                    Type = ErpDataSchedulerDefaults.ErpSpecSheetSyncTask,
                    IsIncremental = false,
                }
            );
        }

        if (
            await _syncTaskService.GetTaskByTypeAsync(
                ErpDataSchedulerDefaults.ErpAccountIncrementalSyncTask
            )
            is null
        )
        {
            await _syncTaskService.InsertTaskAsync(
                new SyncTask
                {
                    Enabled = false,
                    LastEnabledUtc = DateTime.UtcNow,
                    Name = ErpDataSchedulerDefaults.ErpAccountIncrementalSyncTaskName,
                    Type = ErpDataSchedulerDefaults.ErpAccountIncrementalSyncTask,
                    IsIncremental = true,
                }
            );
        }

        if (
            await _syncTaskService.GetTaskByTypeAsync(
                ErpDataSchedulerDefaults.ErpInvoiceIncrementalSyncTask
            )
            is null
        )
        {
            await _syncTaskService.InsertTaskAsync(
                new SyncTask
                {
                    Enabled = false,
                    LastEnabledUtc = DateTime.UtcNow,
                    Name = ErpDataSchedulerDefaults.ErpInvoiceIncrementalSyncTaskName,
                    Type = ErpDataSchedulerDefaults.ErpInvoiceIncrementalSyncTask,
                    IsIncremental = true,
                }
            );
        }

        if (
            await _syncTaskService.GetTaskByTypeAsync(
                ErpDataSchedulerDefaults.ErpProductIncrementalSyncTask
            )
            is null
        )
        {
            await _syncTaskService.InsertTaskAsync(
                new SyncTask
                {
                    Enabled = false,
                    LastEnabledUtc = DateTime.UtcNow,
                    Name = ErpDataSchedulerDefaults.ErpProductIncrementalSyncTaskName,
                    Type = ErpDataSchedulerDefaults.ErpProductIncrementalSyncTask,
                    IsIncremental = true,
                }
            );
        }

        if (
            await _syncTaskService.GetTaskByTypeAsync(
                ErpDataSchedulerDefaults.ErpStockIncrementalSyncTask
            )
            is null
        )
        {
            await _syncTaskService.InsertTaskAsync(
                new SyncTask
                {
                    Enabled = false,
                    LastEnabledUtc = DateTime.UtcNow,
                    Name = ErpDataSchedulerDefaults.ErpStockIncrementalSyncTaskName,
                    Type = ErpDataSchedulerDefaults.ErpStockIncrementalSyncTask,
                    IsIncremental = true,
                }
            );
        }

        if (
            await _syncTaskService.GetTaskByTypeAsync(
                ErpDataSchedulerDefaults.ErpSpecialPriceIncrementalSyncTask
            )
            is null
        )
        {
            await _syncTaskService.InsertTaskAsync(
                new SyncTask
                {
                    Enabled = false,
                    LastEnabledUtc = DateTime.UtcNow,
                    Name = ErpDataSchedulerDefaults.ErpSpecialPriceIncrementalSyncTaskName,
                    Type = ErpDataSchedulerDefaults.ErpSpecialPriceIncrementalSyncTask,
                    IsIncremental = true,
                }
            );
        }

        if (
            await _syncTaskService.GetTaskByTypeAsync(
                ErpDataSchedulerDefaults.ErpGroupPriceIncrementalSyncTask
            )
            is null
        )
        {
            await _syncTaskService.InsertTaskAsync(
                new SyncTask
                {
                    Enabled = false,
                    LastEnabledUtc = DateTime.UtcNow,
                    Name = ErpDataSchedulerDefaults.ErpGroupPriceIncrementalSyncTaskName,
                    Type = ErpDataSchedulerDefaults.ErpGroupPriceIncrementalSyncTask,
                    IsIncremental = true,
                }
            );
        }

        if (
            await _syncTaskService.GetTaskByTypeAsync(
                ErpDataSchedulerDefaults.ErpShipToAddressIncrementalSyncTask
            )
            is null
        )
        {
            await _syncTaskService.InsertTaskAsync(
                new SyncTask
                {
                    Enabled = false,
                    LastEnabledUtc = DateTime.UtcNow,
                    Name = ErpDataSchedulerDefaults.ErpShipToAddressIncrementalSyncTaskName,
                    Type = ErpDataSchedulerDefaults.ErpShipToAddressIncrementalSyncTask,
                    IsIncremental = true,
                }
            );
        }

        if (
            await _syncTaskService.GetTaskByTypeAsync(
                ErpDataSchedulerDefaults.ErpOrderIncrementalSyncTask
            )
            is null
        )
        {
            await _syncTaskService.InsertTaskAsync(
                new SyncTask
                {
                    Enabled = false,
                    LastEnabledUtc = DateTime.UtcNow,
                    Name = ErpDataSchedulerDefaults.ErpOrderIncrementalSyncTaskName,
                    Type = ErpDataSchedulerDefaults.ErpOrderIncrementalSyncTask,
                    IsIncremental = true,
                }
            );
        }

        if (
            await _syncTaskService.GetTaskByTypeAsync(
                ErpDataSchedulerDefaults.ErpSpecSheetIncrementalSyncTask
            )
            is null
        )
        {
            await _syncTaskService.InsertTaskAsync(
                new SyncTask
                {
                    Enabled = false,
                    LastEnabledUtc = DateTime.UtcNow,
                    Name = ErpDataSchedulerDefaults.ErpSpecSheetIncrementalSyncTaskName,
                    Type = ErpDataSchedulerDefaults.ErpSpecSheetIncrementalSyncTask,
                    IsIncremental = true,
                }
            );
        }

        if (
            await _syncTaskService.GetTaskByTypeAsync(
                ErpDataSchedulerDefaults.ErpAccountIncrementalSyncTask
            )
            is null
        )
        {
            await _syncTaskService.InsertTaskAsync(
                new SyncTask
                {
                    Enabled = false,
                    LastEnabledUtc = DateTime.UtcNow,
                    Name = ErpDataSchedulerDefaults.ErpAccountIncrementalSyncTaskName,
                    Type = ErpDataSchedulerDefaults.ErpAccountIncrementalSyncTask,
                    IsIncremental = true,
                }
            );
        }

        if (
            await _syncTaskService.GetTaskByTypeAsync(
                ErpDataSchedulerDefaults.ErpInvoiceIncrementalSyncTask
            )
            is null
        )
        {
            await _syncTaskService.InsertTaskAsync(
                new SyncTask
                {
                    Enabled = false,
                    LastEnabledUtc = DateTime.UtcNow,
                    Name = ErpDataSchedulerDefaults.ErpInvoiceIncrementalSyncTaskName,
                    Type = ErpDataSchedulerDefaults.ErpInvoiceIncrementalSyncTask,
                    IsIncremental = true,
                }
            );
        }

        if (
            await _syncTaskService.GetTaskByTypeAsync(
                ErpDataSchedulerDefaults.ErpProductIncrementalSyncTask
            )
            is null
        )
        {
            await _syncTaskService.InsertTaskAsync(
                new SyncTask
                {
                    Enabled = false,
                    LastEnabledUtc = DateTime.UtcNow,
                    Name = ErpDataSchedulerDefaults.ErpProductIncrementalSyncTaskName,
                    Type = ErpDataSchedulerDefaults.ErpProductIncrementalSyncTask,
                    IsIncremental = true,
                }
            );
        }

        if (
            await _syncTaskService.GetTaskByTypeAsync(
                ErpDataSchedulerDefaults.ErpStockIncrementalSyncTask
            )
            is null
        )
        {
            await _syncTaskService.InsertTaskAsync(
                new SyncTask
                {
                    Enabled = false,
                    LastEnabledUtc = DateTime.UtcNow,
                    Name = ErpDataSchedulerDefaults.ErpStockIncrementalSyncTaskName,
                    Type = ErpDataSchedulerDefaults.ErpStockIncrementalSyncTask,
                    IsIncremental = true,
                }
            );
        }

        if (
            await _syncTaskService.GetTaskByTypeAsync(
                ErpDataSchedulerDefaults.ErpSpecialPriceIncrementalSyncTask
            )
            is null
        )
        {
            await _syncTaskService.InsertTaskAsync(
                new SyncTask
                {
                    Enabled = false,
                    LastEnabledUtc = DateTime.UtcNow,
                    Name = ErpDataSchedulerDefaults.ErpSpecialPriceIncrementalSyncTaskName,
                    Type = ErpDataSchedulerDefaults.ErpSpecialPriceIncrementalSyncTask,
                    IsIncremental = true,
                }
            );
        }

        if (
            await _syncTaskService.GetTaskByTypeAsync(
                ErpDataSchedulerDefaults.ErpGroupPriceIncrementalSyncTask
            )
            is null
        )
        {
            await _syncTaskService.InsertTaskAsync(
                new SyncTask
                {
                    Enabled = false,
                    LastEnabledUtc = DateTime.UtcNow,
                    Name = ErpDataSchedulerDefaults.ErpGroupPriceIncrementalSyncTaskName,
                    Type = ErpDataSchedulerDefaults.ErpGroupPriceIncrementalSyncTask,
                    IsIncremental = true,
                }
            );
        }

        if (
            await _syncTaskService.GetTaskByTypeAsync(
                ErpDataSchedulerDefaults.ErpShipToAddressIncrementalSyncTask
            )
            is null
        )
        {
            await _syncTaskService.InsertTaskAsync(
                new SyncTask
                {
                    Enabled = false,
                    LastEnabledUtc = DateTime.UtcNow,
                    Name = ErpDataSchedulerDefaults.ErpShipToAddressIncrementalSyncTaskName,
                    Type = ErpDataSchedulerDefaults.ErpShipToAddressIncrementalSyncTask,
                    IsIncremental = true,
                }
            );
        }

        if (
            await _syncTaskService.GetTaskByTypeAsync(
                ErpDataSchedulerDefaults.ErpOrderIncrementalSyncTask
            )
            is null
        )
        {
            await _syncTaskService.InsertTaskAsync(
                new SyncTask
                {
                    Enabled = false,
                    LastEnabledUtc = DateTime.UtcNow,
                    Name = ErpDataSchedulerDefaults.ErpOrderIncrementalSyncTaskName,
                    Type = ErpDataSchedulerDefaults.ErpOrderIncrementalSyncTask,
                    IsIncremental = true,
                }
            );
        }

        if (
            await _syncTaskService.GetTaskByTypeAsync(
                ErpDataSchedulerDefaults.ErpSpecSheetIncrementalSyncTask
            )
            is null
        )
        {
            await _syncTaskService.InsertTaskAsync(
                new SyncTask
                {
                    Enabled = false,
                    LastEnabledUtc = DateTime.UtcNow,
                    Name = ErpDataSchedulerDefaults.ErpSpecSheetIncrementalSyncTaskName,
                    Type = ErpDataSchedulerDefaults.ErpSpecSheetIncrementalSyncTask,
                    IsIncremental = true,
                }
            );
        }

        if (
            await _scheduleTaskService.GetTaskByTypeAsync(
                ErpDataSchedulerDefaults.SyncLogFileDeleteTask
            )
            is null
        )
        {
            await _scheduleTaskService.InsertTaskAsync(
                new ScheduleTask
                {
                    Enabled = true,
                    LastEnabledUtc = DateTime.UtcNow,
                    Seconds = ErpDataSchedulerDefaults.DefaultSyncLogFileDeleteTaskInverval,
                    Name = ErpDataSchedulerDefaults.SyncLogFileDeleteTaskName,
                    Type = ErpDataSchedulerDefaults.SyncLogFileDeleteTask,
                }
            );
        }

        #endregion
    }

    #endregion

    #region Register Jobs

    private async Task RegisterScheduleJobsAsync()
    {
        #region Erp Account Sync Task

        var erpAccountSyncTask = await _syncTaskService.GetTaskByTypeAsync(
            ErpDataSchedulerDefaults.ErpAccountSyncTask
        );

        if (erpAccountSyncTask is not null)
        {
            await _nopStationScheduler.CreateScheduleJobAsync<ErpAccountSyncTask>(
                ErpDataSchedulerDefaults.ErpAccountSyncTaskIdentity,
                true
            );

            if (
                erpAccountSyncTask.QuartzJobName
                != ErpDataSchedulerDefaults.ErpAccountSyncTaskIdentity
            )
            {
                erpAccountSyncTask.QuartzJobName =
                    ErpDataSchedulerDefaults.ErpAccountSyncTaskIdentity;
                await _syncTaskService.UpdateTaskAsync(erpAccountSyncTask);
            }
        }

        #endregion

        #region Erp Group Price Sync Task

        var erpGroupPriceSyncTask = await _syncTaskService.GetTaskByTypeAsync(
            ErpDataSchedulerDefaults.ErpGroupPriceSyncTask
        );

        if (erpGroupPriceSyncTask is not null)
        {
            await _nopStationScheduler.CreateScheduleJobAsync<ErpGroupPriceSyncTask>(
                ErpDataSchedulerDefaults.ErpGroupPriceSyncTaskIdentity,
                true
            );

            if (
                erpGroupPriceSyncTask.QuartzJobName
                != ErpDataSchedulerDefaults.ErpGroupPriceSyncTaskIdentity
            )
            {
                erpGroupPriceSyncTask.QuartzJobName =
                    ErpDataSchedulerDefaults.ErpGroupPriceSyncTaskIdentity;
                await _syncTaskService.UpdateTaskAsync(erpGroupPriceSyncTask);
            }
        }

        #endregion

        #region Erp Invoice Sync Task

        var erpInvoiceSyncTask = await _syncTaskService.GetTaskByTypeAsync(
            ErpDataSchedulerDefaults.ErpInvoiceSyncTask
        );

        if (erpInvoiceSyncTask is not null)
        {
            await _nopStationScheduler.CreateScheduleJobAsync<ErpInvoiceSyncTask>(
                ErpDataSchedulerDefaults.ErpInvoiceSyncTaskIdentity,
                true
            );

            if (
                erpInvoiceSyncTask.QuartzJobName
                != ErpDataSchedulerDefaults.ErpInvoiceSyncTaskIdentity
            )
            {
                erpInvoiceSyncTask.QuartzJobName =
                    ErpDataSchedulerDefaults.ErpInvoiceSyncTaskIdentity;
                await _syncTaskService.UpdateTaskAsync(erpInvoiceSyncTask);
            }
        }

        #endregion

        #region Erp Order Sync Task

        var erpOrderSyncTask = await _syncTaskService.GetTaskByTypeAsync(
            ErpDataSchedulerDefaults.ErpOrderSyncTask
        );

        if (erpOrderSyncTask is not null)
        {
            await _nopStationScheduler.CreateScheduleJobAsync<ErpOrderSyncTask>(
                ErpDataSchedulerDefaults.ErpOrderSyncTaskIdentity,
                true
            );

            if (erpOrderSyncTask.QuartzJobName != ErpDataSchedulerDefaults.ErpOrderSyncTaskIdentity)
            {
                erpOrderSyncTask.QuartzJobName = ErpDataSchedulerDefaults.ErpOrderSyncTaskIdentity;
                await _syncTaskService.UpdateTaskAsync(erpOrderSyncTask);
            }
        }

        #endregion

        #region Erp Product Sync Task

        var erpProductSyncTask = await _syncTaskService.GetTaskByTypeAsync(
            ErpDataSchedulerDefaults.ErpProductSyncTask
        );

        if (erpProductSyncTask is not null)
        {
            await _nopStationScheduler.CreateScheduleJobAsync<ErpProductSyncTask>(
                ErpDataSchedulerDefaults.ErpProductSyncTaskIdentity,
                true
            );

            if (
                erpProductSyncTask.QuartzJobName
                != ErpDataSchedulerDefaults.ErpProductSyncTaskIdentity
            )
            {
                erpProductSyncTask.QuartzJobName =
                    ErpDataSchedulerDefaults.ErpProductSyncTaskIdentity;
                await _syncTaskService.UpdateTaskAsync(erpProductSyncTask);
            }
        }

        #endregion

        #region Erp Ship To Address Sync Task

        var erpShipToAddressSyncTask = await _syncTaskService.GetTaskByTypeAsync(
            ErpDataSchedulerDefaults.ErpShipToAddressSyncTask
        );

        if (erpShipToAddressSyncTask is not null)
        {
            await _nopStationScheduler.CreateScheduleJobAsync<ErpShipToAddressSyncTask>(
                ErpDataSchedulerDefaults.ErpShipToAddressSyncTaskIdentity,
                true
            );

            if (
                erpShipToAddressSyncTask.QuartzJobName
                != ErpDataSchedulerDefaults.ErpShipToAddressSyncTaskIdentity
            )
            {
                erpShipToAddressSyncTask.QuartzJobName =
                    ErpDataSchedulerDefaults.ErpShipToAddressSyncTaskIdentity;

                await _syncTaskService.UpdateTaskAsync(erpShipToAddressSyncTask);
            }
        }

        #endregion

        #region Erp Special Price Sync Task

        var erpSpecialPriceSyncTask = await _syncTaskService.GetTaskByTypeAsync(
            ErpDataSchedulerDefaults.ErpSpecialPriceSyncTask
        );

        if (erpSpecialPriceSyncTask is not null)
        {
            await _nopStationScheduler.CreateScheduleJobAsync<ErpSpecialPriceSyncTask>(
                ErpDataSchedulerDefaults.ErpSpecialPriceSyncTaskIdentity,
                true
            );

            if (
                erpSpecialPriceSyncTask.QuartzJobName
                != ErpDataSchedulerDefaults.ErpSpecialPriceSyncTaskIdentity
            )
            {
                erpSpecialPriceSyncTask.QuartzJobName =
                    ErpDataSchedulerDefaults.ErpSpecialPriceSyncTaskIdentity;

                await _syncTaskService.UpdateTaskAsync(erpSpecialPriceSyncTask);
            }
        }

        #endregion

        #region Erp Stock Sync Task

        var erpStockSyncTask = await _syncTaskService.GetTaskByTypeAsync(
            ErpDataSchedulerDefaults.ErpStockSyncTask
        );

        if (erpStockSyncTask is not null)
        {
            await _nopStationScheduler.CreateScheduleJobAsync<ErpStockSyncTask>(
                ErpDataSchedulerDefaults.ErpStockSyncTaskIdentity,
                true
            );

            if (erpStockSyncTask.QuartzJobName != ErpDataSchedulerDefaults.ErpStockSyncTaskIdentity)
            {
                erpStockSyncTask.QuartzJobName = ErpDataSchedulerDefaults.ErpStockSyncTaskIdentity;
                await _syncTaskService.UpdateTaskAsync(erpStockSyncTask);
            }
        }

        #endregion

        #region Erp SpecSheet Sync Task

        var erpSpecSheetSyncTask = await _syncTaskService.GetTaskByTypeAsync(
            ErpDataSchedulerDefaults.ErpSpecSheetSyncTask
        );

        if (erpSpecSheetSyncTask is not null)
        {
            await _nopStationScheduler.CreateScheduleJobAsync<ErpSpecSheetSyncTask>(
                ErpDataSchedulerDefaults.ErpSpecSheetSyncTaskIdentity,
                true
            );

            if (
                erpSpecSheetSyncTask.QuartzJobName
                != ErpDataSchedulerDefaults.ErpSpecSheetSyncTaskIdentity
            )
            {
                erpSpecSheetSyncTask.QuartzJobName =
                    ErpDataSchedulerDefaults.ErpSpecSheetSyncTaskIdentity;

                await _syncTaskService.UpdateTaskAsync(erpSpecSheetSyncTask);
            }
        }

        #endregion

        #region Erp Account Incremental Sync Task

        var erpAccountIncrementalSyncTask = await _syncTaskService.GetTaskByTypeAsync(
            ErpDataSchedulerDefaults.ErpAccountIncrementalSyncTask
        );

        if (erpAccountIncrementalSyncTask is not null)
        {
            await _nopStationScheduler.CreateScheduleJobAsync<ErpAccountIncrementalSyncTask>(
                ErpDataSchedulerDefaults.ErpAccountIncrementalSyncTaskIdentity,
                true
            );

            if (
                erpAccountIncrementalSyncTask.QuartzJobName
                != ErpDataSchedulerDefaults.ErpAccountIncrementalSyncTaskIdentity
            )
            {
                erpAccountIncrementalSyncTask.QuartzJobName =
                    ErpDataSchedulerDefaults.ErpAccountIncrementalSyncTaskIdentity;

                await _syncTaskService.UpdateTaskAsync(erpAccountIncrementalSyncTask);
            }
        }

        #endregion

        #region Erp Group Price Incremental Sync Task

        var erpGroupPriceIncrementalSyncTask = await _syncTaskService.GetTaskByTypeAsync(
            ErpDataSchedulerDefaults.ErpGroupPriceIncrementalSyncTask
        );

        if (erpGroupPriceIncrementalSyncTask is not null)
        {
            await _nopStationScheduler.CreateScheduleJobAsync<ErpGroupPriceIncrementalSyncTask>(
                ErpDataSchedulerDefaults.ErpGroupPriceIncrementalSyncTaskIdentity,
                true
            );

            if (
                erpGroupPriceIncrementalSyncTask.QuartzJobName
                != ErpDataSchedulerDefaults.ErpGroupPriceIncrementalSyncTaskIdentity
            )
            {
                erpGroupPriceIncrementalSyncTask.QuartzJobName =
                    ErpDataSchedulerDefaults.ErpGroupPriceIncrementalSyncTaskIdentity;

                await _syncTaskService.UpdateTaskAsync(erpGroupPriceIncrementalSyncTask);
            }
        }

        #endregion

        #region Erp Invoice Incremental Sync Task

        var erpInvoiceIncrementalSyncTask = await _syncTaskService.GetTaskByTypeAsync(
            ErpDataSchedulerDefaults.ErpInvoiceIncrementalSyncTask
        );

        if (erpInvoiceIncrementalSyncTask is not null)
        {
            await _nopStationScheduler.CreateScheduleJobAsync<ErpInvoiceIncrementalSyncTask>(
                ErpDataSchedulerDefaults.ErpInvoiceIncrementalSyncTaskIdentity,
                true
            );

            if (
                erpInvoiceIncrementalSyncTask.QuartzJobName
                != ErpDataSchedulerDefaults.ErpInvoiceIncrementalSyncTaskIdentity
            )
            {
                erpInvoiceIncrementalSyncTask.QuartzJobName =
                    ErpDataSchedulerDefaults.ErpInvoiceIncrementalSyncTaskIdentity;

                await _syncTaskService.UpdateTaskAsync(erpInvoiceIncrementalSyncTask);
            }
        }

        #endregion

        #region Erp Order Incremental Sync Task

        var erpOrderIncrementalSyncTask = await _syncTaskService.GetTaskByTypeAsync(
            ErpDataSchedulerDefaults.ErpOrderIncrementalSyncTask
        );

        if (erpOrderIncrementalSyncTask is not null)
        {
            await _nopStationScheduler.CreateScheduleJobAsync<ErpOrderIncrementalSyncTask>(
                ErpDataSchedulerDefaults.ErpOrderIncrementalSyncTaskIdentity,
                true
            );

            if (
                erpOrderIncrementalSyncTask.QuartzJobName
                != ErpDataSchedulerDefaults.ErpOrderIncrementalSyncTaskIdentity
            )
            {
                erpOrderIncrementalSyncTask.QuartzJobName =
                    ErpDataSchedulerDefaults.ErpOrderIncrementalSyncTaskIdentity;

                await _syncTaskService.UpdateTaskAsync(erpOrderIncrementalSyncTask);
            }
        }

        #endregion

        #region Erp Product Incremental Sync Task

        var erpProductIncrementalSyncTask = await _syncTaskService.GetTaskByTypeAsync(
            ErpDataSchedulerDefaults.ErpProductIncrementalSyncTask
        );

        if (erpProductIncrementalSyncTask is not null)
        {
            await _nopStationScheduler.CreateScheduleJobAsync<ErpProductIncrementalSyncTask>(
                ErpDataSchedulerDefaults.ErpProductIncrementalSyncTaskIdentity,
                true
            );

            if (
                erpProductIncrementalSyncTask.QuartzJobName
                != ErpDataSchedulerDefaults.ErpProductIncrementalSyncTaskIdentity
            )
            {
                erpProductIncrementalSyncTask.QuartzJobName =
                    ErpDataSchedulerDefaults.ErpProductIncrementalSyncTaskIdentity;

                await _syncTaskService.UpdateTaskAsync(erpProductIncrementalSyncTask);
            }
        }

        #endregion

        #region Erp Ship To Address Incremental Sync Task

        var erpShipToAddressIncrementalSyncTask = await _syncTaskService.GetTaskByTypeAsync(
            ErpDataSchedulerDefaults.ErpShipToAddressIncrementalSyncTask
        );

        if (erpShipToAddressIncrementalSyncTask is not null)
        {
            await _nopStationScheduler.CreateScheduleJobAsync<ErpShipToAddressIncrementalSyncTask>(
                ErpDataSchedulerDefaults.ErpShipToAddressIncrementalSyncTaskIdentity,
                true
            );

            if (
                erpShipToAddressIncrementalSyncTask.QuartzJobName
                != ErpDataSchedulerDefaults.ErpShipToAddressIncrementalSyncTaskIdentity
            )
            {
                erpShipToAddressIncrementalSyncTask.QuartzJobName =
                    ErpDataSchedulerDefaults.ErpShipToAddressIncrementalSyncTaskIdentity;

                await _syncTaskService.UpdateTaskAsync(erpShipToAddressIncrementalSyncTask);
            }
        }

        #endregion

        #region Erp Special Price Incremental Sync Task

        var erpSpecialPriceIncrementalSyncTask = await _syncTaskService.GetTaskByTypeAsync(
            ErpDataSchedulerDefaults.ErpSpecialPriceIncrementalSyncTask
        );

        if (erpSpecialPriceIncrementalSyncTask is not null)
        {
            await _nopStationScheduler.CreateScheduleJobAsync<ErpSpecialPriceIncrementalSyncTask>(
                ErpDataSchedulerDefaults.ErpSpecialPriceIncrementalSyncTaskIdentity,
                true
            );

            if (
                erpSpecialPriceIncrementalSyncTask.QuartzJobName
                != ErpDataSchedulerDefaults.ErpSpecialPriceIncrementalSyncTaskIdentity
            )
            {
                erpSpecialPriceIncrementalSyncTask.QuartzJobName =
                    ErpDataSchedulerDefaults.ErpSpecialPriceIncrementalSyncTaskIdentity;

                await _syncTaskService.UpdateTaskAsync(erpSpecialPriceIncrementalSyncTask);
            }
        }

        #endregion

        #region Erp Stock Incremental Sync Task

        var erpStockIncrementalSyncTask = await _syncTaskService.GetTaskByTypeAsync(
            ErpDataSchedulerDefaults.ErpStockIncrementalSyncTask
        );

        if (erpStockIncrementalSyncTask is not null)
        {
            await _nopStationScheduler.CreateScheduleJobAsync<ErpStockIncrementalSyncTask>(
                ErpDataSchedulerDefaults.ErpStockIncrementalSyncTaskIdentity,
                true
            );

            if (
                erpStockIncrementalSyncTask.QuartzJobName
                != ErpDataSchedulerDefaults.ErpStockIncrementalSyncTaskIdentity
            )
            {
                erpStockIncrementalSyncTask.QuartzJobName =
                    ErpDataSchedulerDefaults.ErpStockIncrementalSyncTaskIdentity;

                await _syncTaskService.UpdateTaskAsync(erpStockIncrementalSyncTask);
            }
        }

        #endregion

        #region Erp SpecSheet Incremental Sync Task

        var erpSpecSheetIncrementalSyncTask = await _syncTaskService.GetTaskByTypeAsync(
            ErpDataSchedulerDefaults.ErpSpecSheetIncrementalSyncTask
        );

        if (erpSpecSheetIncrementalSyncTask is not null)
        {
            await _nopStationScheduler.CreateScheduleJobAsync<ErpSpecSheetIncrementalSyncTask>(
                ErpDataSchedulerDefaults.ErpSpecSheetIncrementalSyncTaskIdentity,
                true
            );

            if (
                erpSpecSheetIncrementalSyncTask.QuartzJobName
                != ErpDataSchedulerDefaults.ErpSpecSheetIncrementalSyncTaskIdentity
            )
            {
                erpSpecSheetIncrementalSyncTask.QuartzJobName =
                    ErpDataSchedulerDefaults.ErpSpecSheetIncrementalSyncTaskIdentity;

                await _syncTaskService.UpdateTaskAsync(erpSpecSheetIncrementalSyncTask);
            }
        }

        #endregion
    }

    #endregion

    #endregion
}
