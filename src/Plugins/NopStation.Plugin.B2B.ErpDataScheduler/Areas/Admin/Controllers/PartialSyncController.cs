using Microsoft.AspNetCore.Mvc;
using Nop.Services.Localization;
using NopStation.Plugin.B2B.ErpDataScheduler.Areas.Admin.Models.PartialSyncModels;
using NopStation.Plugin.B2B.ErpDataScheduler.Services.NopStationSyncServices;
using NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncLogServices;
using NopStation.Plugin.Misc.Core.Controllers;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Areas.Admin.Controllers;

public class PartialSyncController : NopStationAdminController
{
    #region Fields

    private readonly ISyncTaskService _syncTaskService;
    private readonly ILocalizationService _localizationService;
    private readonly INopStationScheduler _nopStationScheduler;
    private readonly ISyncLogService _syncLogService;

    #endregion

    #region Ctor

    public PartialSyncController(INopStationScheduler nopStationScheduler,
        ISyncLogService syncLogService,
        ISyncTaskService syncTaskService,
        ILocalizationService localizationService)
    {
        _nopStationScheduler = nopStationScheduler;
        _syncLogService = syncLogService;
        _syncTaskService = syncTaskService;
        _localizationService = localizationService;
    }

    #endregion

    #region Methods

    public async Task<IActionResult> Index()
    {
        var model = new PartialSyncModel();
        model.GroupPricePartialSyncModel.SyncTaskId = 
            (await _syncTaskService.GetTaskByQuartzJobNameAsync(ErpDataSchedulerDefaults.ErpGroupPriceSyncTaskIdentity))?.Id ?? 0;
        model.InvoicePartialSyncModel.SyncTaskId = 
            (await _syncTaskService.GetTaskByQuartzJobNameAsync(ErpDataSchedulerDefaults.ErpInvoiceSyncTaskIdentity))?.Id ?? 0;
        model.ProductPartialSyncModel.SyncTaskId = 
            (await _syncTaskService.GetTaskByQuartzJobNameAsync(ErpDataSchedulerDefaults.ErpProductSyncTaskIdentity))?.Id ?? 0;
        model.SpecialPricePartialSyncModel.SyncTaskId = 
            (await _syncTaskService.GetTaskByQuartzJobNameAsync(ErpDataSchedulerDefaults.ErpSpecialPriceSyncTaskIdentity))?.Id ?? 0;
        model.OrderPartialSyncModel.SyncTaskId = 
            (await _syncTaskService.GetTaskByQuartzJobNameAsync(ErpDataSchedulerDefaults.ErpOrderSyncTaskIdentity))?.Id ?? 0;
        model.StockPartialSyncModel.SyncTaskId = 
            (await _syncTaskService.GetTaskByQuartzJobNameAsync(ErpDataSchedulerDefaults.ErpStockSyncTaskIdentity))?.Id ?? 0;
        model.SpecSheetPartialSyncModel.SyncTaskId = 
            (await _syncTaskService.GetTaskByQuartzJobNameAsync(ErpDataSchedulerDefaults.ErpSpecSheetSyncTaskIdentity))?.Id ?? 0;

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> SyncErpAccount(ErpAccountPartialSyncModel model)
    {
        var task = await _syncTaskService.GetTaskByQuartzJobNameAsync(ErpDataSchedulerDefaults.ErpAccountSyncTaskIdentity);

        if (task is null)
        {
            return Json(new
            {
                Success = false,
                Message = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ErpDataScheduler.Admin.PartialSync.RequestedTaskNotFound")
            });
        }

        try
        {
            var dataMapList = new List<KeyValuePair<string, object>>
            {
                new(nameof(ErpAccountPartialSyncModel.ErpAccountNumber), model.ErpAccountNumber)
            };

            var jobDataMap = _nopStationScheduler.PrepareJobDataMap(dataMapList);

            await _nopStationScheduler.ExecuteSchedulerAsync(task.QuartzJobName, jobDataMap);

            return Json(new
            {
                Success = true,
                Message = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ErpDataScheduler.Admin.PartialSync.TaskScheduledToExecute")
            });
        }
        catch (Exception ex)
        {
            await _syncLogService.SyncLogSaveOnFileAsync(task.Name, 0, ex.Message, ex.StackTrace ?? string.Empty);

            return Json(new
            {
                Success = false,
                Message = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ErpDataScheduler.Admin.PartialSync.InternalServerError")
            });
        }
    }

    [HttpPost]
    public async Task<IActionResult> SyncErpGroupPrice(ErpGroupPricePartialSyncModel model)
    {
        var task = await _syncTaskService.GetTaskByQuartzJobNameAsync(ErpDataSchedulerDefaults.ErpGroupPriceSyncTaskIdentity);

        if (task is null)
        {
            return Json(new
            {
                Success = false,
                Message = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ErpDataScheduler.Admin.PartialSync.RequestedTaskNotFound")
            });
        }

        try
        {
            var dataMapList = new List<KeyValuePair<string, object>>
            {
                new(nameof(ErpGroupPricePartialSyncModel.SalesOrgCode), model.SalesOrgCode),
                new(nameof(ErpGroupPricePartialSyncModel.PriceCode), model.PriceCode),
                new(nameof(ErpGroupPricePartialSyncModel.StockCode), model.StockCode),
            };

            var jobDataMap = _nopStationScheduler.PrepareJobDataMap(dataMapList);

            await _nopStationScheduler.ExecuteSchedulerAsync(task.QuartzJobName, jobDataMap);

            return Json(new
            {
                Success = true,
                Message = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ErpDataScheduler.Admin.PartialSync.TaskScheduledToExecute")
            });
        }
        catch (Exception ex)
        {
            await _syncLogService.SyncLogSaveOnFileAsync(task.Name, 0, ex.Message, ex.StackTrace ?? string.Empty);

            return Json(new
            {
                Success = false,
                Message = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ErpDataScheduler.Admin.PartialSync.InternalServerError")
            });
        }
    }

    [HttpPost]
    public async Task<IActionResult> SyncErpInvoice(ErpInvoicePartialSyncModel model)
    {
        var task = await _syncTaskService.GetTaskByQuartzJobNameAsync(ErpDataSchedulerDefaults.ErpInvoiceSyncTaskIdentity);

        if (task is null)
        {
            return Json(new
            {
                Success = false,
                Message = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ErpDataScheduler.Admin.PartialSync.RequestedTaskNotFound")
            });
        }

        try
        {
            var dataMapList = new List<KeyValuePair<string, object>>
            {
                new(nameof(ErpInvoicePartialSyncModel.ErpAccountNumber), model.ErpAccountNumber),
                new(nameof(ErpInvoicePartialSyncModel.SalesOrgCode), model.SalesOrgCode)
            };

            var jobDataMap = _nopStationScheduler.PrepareJobDataMap(dataMapList);

            await _nopStationScheduler.ExecuteSchedulerAsync(task.QuartzJobName, jobDataMap);

            return Json(new
            {
                Success = true,
                Message = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ErpDataScheduler.Admin.PartialSync.TaskScheduledToExecute")
            });
        }
        catch (Exception ex)
        {
            await _syncLogService.SyncLogSaveOnFileAsync(task.Name, 0, ex.Message, ex.StackTrace ?? string.Empty);

            return Json(new
            {
                Success = false,
                Message = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ErpDataScheduler.Admin.PartialSync.InternalServerError")
            });
        }
    }

    [HttpPost]
    public async Task<IActionResult> SyncErpProduct(ErpProductPartialSyncModel model)
    {
        var task = await _syncTaskService.GetTaskByQuartzJobNameAsync(ErpDataSchedulerDefaults.ErpProductSyncTaskIdentity);

        if (task is null)
        {
            return Json(new
            {
                Success = false,
                Message = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ErpDataScheduler.Admin.PartialSync.RequestedTaskNotFound")
            });
        }

        try
        {
            var dataMapList = new List<KeyValuePair<string, object>>
            {
                new(nameof(ErpProductPartialSyncModel.SalesOrgCode), model.SalesOrgCode),
                new(nameof(ErpProductPartialSyncModel.StockCode), model.StockCode),
            };

            var jobDataMap = _nopStationScheduler.PrepareJobDataMap(dataMapList);

            await _nopStationScheduler.ExecuteSchedulerAsync(task.QuartzJobName, jobDataMap);

            return Json(new
            {
                Success = true,
                Message = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ErpDataScheduler.Admin.PartialSync.TaskScheduledToExecute")
            });
        }
        catch (Exception ex)
        {
            await _syncLogService.SyncLogSaveOnFileAsync(task.Name, 0, ex.Message, ex.StackTrace ?? string.Empty);

            return Json(new
            {
                Success = false,
                Message = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ErpDataScheduler.Admin.PartialSync.InternalServerError")
            });
        }
    }

    [HttpPost]
    public async Task<IActionResult> SyncErpShipToAddress(ErpShipToAddressPartialSyncModel model)
    {
        var task = await _syncTaskService.GetTaskByQuartzJobNameAsync(ErpDataSchedulerDefaults.ErpShipToAddressSyncTaskIdentity);

        if (task is null)
        {
            return Json(new
            {
                Success = false,
                Message = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ErpDataScheduler.Admin.PartialSync.RequestedTaskNotFound")
            });
        }

        try
        {
            var dataMapList = new List<KeyValuePair<string, object>>
            {
                new(nameof(ErpShipToAddressPartialSyncModel.ErpAccountNumber), model.ErpAccountNumber),
            };

            var jobDataMap = _nopStationScheduler.PrepareJobDataMap(dataMapList);

            await _nopStationScheduler.ExecuteSchedulerAsync(task.QuartzJobName, jobDataMap);

            return Json(new
            {
                Success = true,
                Message = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ErpDataScheduler.Admin.PartialSync.TaskScheduledToExecute")
            });
        }
        catch (Exception ex)
        {
            await _syncLogService.SyncLogSaveOnFileAsync(task.Name, 0, ex.Message, ex.StackTrace ?? string.Empty);

            return Json(new
            {
                Success = false,
                Message = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ErpDataScheduler.Admin.PartialSync.InternalServerError")
            });
        }
    }

    [HttpPost]
    public async Task<IActionResult> SyncErpSpecialPrice(ErpSpecialPricePartialSyncModel model)
    {
        var task = await _syncTaskService.GetTaskByQuartzJobNameAsync(ErpDataSchedulerDefaults.ErpSpecialPriceSyncTaskIdentity);

        if (task is null)
        {
            return Json(new
            {
                Success = false,
                Message = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ErpDataScheduler.Admin.PartialSync.RequestedTaskNotFound")
            });
        }

        try
        {
            var dataMapList = new List<KeyValuePair<string, object>>
            {
                new(nameof(ErpSpecialPricePartialSyncModel.SalesOrgCode), model.SalesOrgCode),
                new(nameof(ErpSpecialPricePartialSyncModel.ErpAccountNumber), model.ErpAccountNumber),
                new(nameof(ErpSpecialPricePartialSyncModel.StockCode), model.StockCode),
            };

            var jobDataMap = _nopStationScheduler.PrepareJobDataMap(dataMapList);

            await _nopStationScheduler.ExecuteSchedulerAsync(task.QuartzJobName, jobDataMap);

            return Json(new
            {
                Success = true,
                Message = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ErpDataScheduler.Admin.PartialSync.TaskScheduledToExecute")
            });
        }
        catch (Exception ex)
        {
            await _syncLogService.SyncLogSaveOnFileAsync(task.Name, 0, ex.Message, ex.StackTrace ?? string.Empty);

            return Json(new
            {
                Success = false,
                Message = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ErpDataScheduler.Admin.PartialSync.InternalServerError")
            });
        }
    }

    [HttpPost]
    public async Task<IActionResult> SyncErpOrder(ErpOrderPartialSyncModel model)
    {
        var task = await _syncTaskService.GetTaskByQuartzJobNameAsync(ErpDataSchedulerDefaults.ErpOrderSyncTaskIdentity);

        if (task is null)
        {
            return Json(new
            {
                Success = false,
                Message = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ErpDataScheduler.Admin.PartialSync.RequestedTaskNotFound")
            });
        }

        try
        {
            var dataMapList = new List<KeyValuePair<string, object>>
            {
                new(nameof(ErpOrderPartialSyncModel.ErpAccountNumber), model.ErpAccountNumber),
                new(nameof(ErpOrderPartialSyncModel.OrderNumber), model.OrderNumber),
                new(nameof(ErpOrderPartialSyncModel.SalesOrgCode), model.SalesOrgCode)
            };

            var jobDataMap = _nopStationScheduler.PrepareJobDataMap(dataMapList);

            await _nopStationScheduler.ExecuteSchedulerAsync(task.QuartzJobName, jobDataMap);

            return Json(new
            {
                Success = true,
                Message = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ErpDataScheduler.Admin.PartialSync.TaskScheduledToExecute")
            });
        }
        catch (Exception ex)
        {
            await _syncLogService.SyncLogSaveOnFileAsync(task.Name, 0, ex.Message, ex.StackTrace ?? string.Empty);

            return Json(new
            {
                Success = false,
                Message = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ErpDataScheduler.Admin.PartialSync.InternalServerError")
            });
        }
    }

    [HttpPost]
    public async Task<IActionResult> SyncErpStock(ErpStockPartialSyncModel model)
    {
        var task = await _syncTaskService.GetTaskByQuartzJobNameAsync(ErpDataSchedulerDefaults.ErpStockSyncTaskIdentity);

        if (task is null)
        {
            return Json(new
            {
                Success = false,
                Message = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ErpDataScheduler.Admin.PartialSync.RequestedTaskNotFound")
            });
        }

        try
        {
            var dataMapList = new List<KeyValuePair<string, object>>
            {
                new(nameof(ErpStockPartialSyncModel.SalesOrgCode), model.SalesOrgCode),
                new(nameof(ErpStockPartialSyncModel.StockCode), model.StockCode),
            };

            var jobDataMap = _nopStationScheduler.PrepareJobDataMap(dataMapList);

            await _nopStationScheduler.ExecuteSchedulerAsync(task.QuartzJobName, jobDataMap);

            return Json(new
            {
                Success = true,
                Message = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ErpDataScheduler.Admin.PartialSync.TaskScheduledToExecute")
            });
        }
        catch (Exception ex)
        {
            await _syncLogService.SyncLogSaveOnFileAsync(task.Name, 0, ex.Message, ex.StackTrace ?? string.Empty);

            return Json(new
            {
                Success = false,
                Message = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ErpDataScheduler.Admin.PartialSync.InternalServerError")
            });
        }
    }

    [HttpPost]
    public async Task<IActionResult> SyncErpSpecSheet(ErpSpecSheetPartialSyncModel model)
    {
        var task = await _syncTaskService.GetTaskByQuartzJobNameAsync(ErpDataSchedulerDefaults.ErpSpecSheetSyncTaskIdentity);

        if (task is null)
        {
            return Json(new
            {
                Success = false,
                Message = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ErpDataScheduler.Admin.PartialSync.RequestedTaskNotFound")
            });
        }

        try
        {
            var dataMapList = new List<KeyValuePair<string, object>>
            {
                new(nameof(ErpSpecSheetPartialSyncModel.StockCode), model.StockCode),
                new(nameof(ErpSpecSheetPartialSyncModel.SalesOrgCode), model.SalesOrgCode)
            };

            var jobDataMap = _nopStationScheduler.PrepareJobDataMap(dataMapList);

            await _nopStationScheduler.ExecuteSchedulerAsync(task.QuartzJobName, jobDataMap);

            return Json(new
            {
                Success = true,
                Message = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ErpDataScheduler.Admin.PartialSync.TaskScheduledToExecute")
            });
        }
        catch (Exception ex)
        {
            await _syncLogService.SyncLogSaveOnFileAsync(task.Name, 0, ex.Message, ex.StackTrace ?? string.Empty);

            return Json(new
            {
                Success = false,
                Message = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ErpDataScheduler.Admin.PartialSync.InternalServerError")
            });
        }
    }

    #endregion
}
