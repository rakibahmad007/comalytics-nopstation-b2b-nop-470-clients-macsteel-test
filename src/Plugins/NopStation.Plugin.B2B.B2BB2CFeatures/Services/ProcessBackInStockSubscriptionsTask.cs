using System;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Nop.Core;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.ScheduleTasks;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Services;

public class ProcessBackInStockSubscriptionsTask : IScheduleTask
{
    #region Fields

    private readonly IBackInStockSubscriptionService _backInStockSubscriptionService;
    private readonly IGenericAttributeService _genericAttributeService;
    private readonly IWorkflowMessageService _workflowMessageService;
    private readonly ICustomerService _customerService;
    private readonly IErpLogsService _erpLogsService;
    private readonly ILocalizationService _localizationService;

    #endregion

    #region Ctor

    public ProcessBackInStockSubscriptionsTask(IBackInStockSubscriptionService backInStockSubscriptionService,
        IGenericAttributeService genericAttributeService,
        IWorkflowMessageService workflowMessageService,
        ICustomerService customerService,
        IErpLogsService erpLogsService,
        ILocalizationService localizationService)
    {
        _backInStockSubscriptionService = backInStockSubscriptionService;
        _genericAttributeService = genericAttributeService;
        _workflowMessageService = workflowMessageService;
        _customerService = customerService;
        _erpLogsService = erpLogsService;
        _localizationService = localizationService;
    }

    #endregion

    #region Methods

    public async Task ExecuteAsync()
    {
        var connectionString = DataSettingsManager.LoadSettings()?.ConnectionString;

        if (string.IsNullOrEmpty(connectionString))
            throw new InvalidOperationException("Connection string is not defined.");

        var subscriptionListQuery = @"SELECT bis.Id,
                bis.CustomerId
                FROM [BackInStockSubscription] bis
                LEFT JOIN [Erp_Nop_User] bu ON bis.[CustomerId] = bu.[NopCustomerId]
                LEFT JOIN [Erp_Account] ba ON bu.[ErpAccountId] = ba.[Id]
                LEFT JOIN [Erp_Sales_Org] bs ON ba.[ErpSalesOrgId] = bs.[Id]
                LEFT JOIN [Erp_Warehouse_Sales_Org_Map] bsw ON bsw.[ErpSalesOrgId] = bs.[Id]
                LEFT JOIN [ProductWarehouseInventory] pw ON pw.[WarehouseId] = bsw.[NopWarehouseId]
                WHERE bis.[ProductId] = pw.[ProductId]
                AND pw.[StockQuantity] > 0";

        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        using var cmd = new SqlCommand(subscriptionListQuery, connection);
        using var dr = await cmd.ExecuteReaderAsync();
        if (dr.HasRows)
        {
            while (await dr.ReadAsync())
            {
                var subscriptionId = dr.GetInt32(0);
                var customerId = dr.GetInt32(1);

                var subscription = await _backInStockSubscriptionService.GetSubscriptionByIdAsync(subscriptionId);

                if (subscription == null)
                {
                    await _erpLogsService.ErrorAsync(
                        string.Format(await _localizationService.GetResourceAsync("NopStation.Plugin.B2B.B2BB2CFeatures.BackInStockSubscriptionPerSalesOrg.Error.BackInStockSubscriptionNotFound"),
                        subscriptionId, customerId),
                        ErpSyncLevel.Stock);
                    continue;
                }

                var customer = await _customerService.GetCustomerByIdAsync(subscription.CustomerId);

                if (customer == null || !CommonHelper.IsValidEmail(customer.Email))
                {
                    await _erpLogsService.ErrorAsync(
                        string.Format(await _localizationService.GetResourceAsync("NopStation.Plugin.B2B.B2BB2CFeatures.BackInStockSubscriptionPerSalesOrg.Error.CustomerNotFoundOrInvalidCustomer"),
                        customerId, subscriptionId),
                        ErpSyncLevel.Stock);
                    continue;
                }

                var customerLanguageId = await _genericAttributeService.GetAttributeAsync<int>(
                    customer,
                    B2BB2CFeaturesDefaults.LanguageIdAttribute,
                    subscription.StoreId
                );

                await _workflowMessageService.SendBackInStockNotificationAsync(subscription, customerLanguageId);
                await _backInStockSubscriptionService.DeleteSubscriptionAsync(subscription);
            }
        }
    }


    #endregion
}
