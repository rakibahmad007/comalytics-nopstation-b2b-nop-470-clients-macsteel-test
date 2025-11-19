using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Logging;
using Nop.Plugin.Misc.ErpWebhook.Domain.ParallelTables;
using Nop.Plugin.Misc.ErpWebhook.Services.Interfaces;
using Nop.Services.Logging;
using Nop.Services.ScheduleTasks;

namespace Nop.Plugin.Misc.ErpWebhook.Services.ScheduleTasks
{
    public class ProcessParallelErpOrderToOrderTask : IScheduleTask
    {
        private readonly IWebhookErpOrderService _webhookB2BOrderService;
        private readonly IErpWebhookService _erpWebhookService;
        private readonly ILogger _logger;

        public ProcessParallelErpOrderToOrderTask(IWebhookErpOrderService webhookB2BOrderService,
            IErpWebhookService erpWebhookService,
            ILogger logger)
        {
            _webhookB2BOrderService = webhookB2BOrderService;
            _erpWebhookService = erpWebhookService;
            _logger = logger;
        }
        public async Task ExecuteAsync()
        {
            var config = await _erpWebhookService.LoadErpWebhookConfigsFromJsonAsync();
            var batchSize = config.B2BOrderBatchSize ?? 200;
            var skipCount = 0;
            while (true)
            {
                try
                {
                    List<Parallel_ErpOrder> erpOrders = await _webhookB2BOrderService.GetErpOrdersAsync(skipCount, batchSize);
                    skipCount += batchSize;

                    //_webhookB2BOrderService.ProcessB2bOrders(erpOrders);

                    await _webhookB2BOrderService.UpdateErpOrdersAsync(erpOrders);

                    if (erpOrders == null || erpOrders.Count < 1)
                        break;
                }
                catch (Exception ex)
                {
                    _logger.InsertLog(LogLevel.Error, "Error occured on process parallel erp product to B2BProduct task", fullMessage: $"{ex}");
                }
            }
        }
    }
}
