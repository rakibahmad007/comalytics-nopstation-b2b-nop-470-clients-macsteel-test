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
    public class ProcessParallelErpProductToB2BProductTask : IScheduleTask
    {
        private readonly IWebhookERPProductService _webhookB2BProductService;
        private readonly IErpWebhookService _erpWebhookService;
        private readonly ILogger _logger;

        public ProcessParallelErpProductToB2BProductTask(IWebhookERPProductService webhookB2BProductService,
            IErpWebhookService erpWebhookService,
            ILogger logger)
        {
            _webhookB2BProductService = webhookB2BProductService;
            _erpWebhookService = erpWebhookService;
            _logger = logger;
        }
        public async Task ExecuteAsync()
        {
            var config = await _erpWebhookService.LoadErpWebhookConfigsFromJsonAsync();
            var batchSize = config.B2BProductBatchSize ?? 30;
            var skipCount = 0;
            while (true)
            {
                try
                {
                    List<Parallel_ErpProduct> erpProducts = await _webhookB2BProductService.GetErpProductsAsync(skipCount, batchSize);
                    skipCount += batchSize;

                    //_webhookB2BProductService.ProcessB2bProducts(erpProducts);

                    //_webhookB2BProductService.Delete(erpProducts);

                    if (erpProducts == null || erpProducts.Count < 1)
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
