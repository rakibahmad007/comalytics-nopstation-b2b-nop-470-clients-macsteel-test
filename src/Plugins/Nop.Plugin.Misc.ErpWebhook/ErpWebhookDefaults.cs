namespace Nop.Plugin.Misc.ErpWebhook;

public static class ErpWebhookDefaults
{
    public static string SystemName => "Misc.ErpWebhook";
    public static int TrailPeriodsInDays => 0;
    public static string WebhookRoute => "PepsicoWebhookRoute";
    public static string ProductWebhookRoute => "ProductWebhook";
    public static string OrderWebhookRoute => "OrderWebhook";
    public static string PriceWebhookRoute => "PriceWebhook";
    public static string StockWebhookRoute => "StockWebhook";
    public static string AccountWebhookRoute => "AccountWebhook";
    public static string ShiptoAddressWebhookRoute => "ShiptoAddressWebhook";
    public static string CreditWebhookRoute => "CreditWebhook";
    public static string DeliveryDates => "DeliveryDatesModel";
    public static string ProductsImage => "ProductsImage";

    public static string PluginsInfoFilePath => "~/Plugins/Misc.ErpWebhook/erpWebhookConfig.json";
    public static string ErpWebhookManagerRoleSystemName => "ErpWebhookManager";
    public static string ErpWebhookManagerRoleName => "Erp Webhook Manager";

    public static string ParallelErpAccountToB2BAccountTaskName => "Process Parallel WebhookErpAccountModel To B2BAccount Task";
    public static string ParallelErpAccountToB2BAccountTaskType => "Nop.Plugin.Misc.ErpWebhook.Services.ScheduleTasks.ProcessParallelErpAccountToB2BAccountTask";
    public static string ParallelErpShipToAddressToShipToAddressTaskName => "Process Parallel Erp ship to address To B2B Ship to address Task";
    public static string ParallelErpShipToAddressToShipToAddressTaskType => "Nop.Plugin.Misc.ErpWebhook.Services.ScheduleTasks.ProcessParallelErpShipToAddressToShipToAddressTask";
    public static string ParallelErpProductToB2BProductTaskName => "Process Parallel Erp Product To B2B Product Task";
    public static string ParallelErpProductToB2BProductTaskType => "Nop.Plugin.Misc.ErpWebhook.Services.ScheduleTasks.ProcessParallelErpProductToB2BProductTask";

    public static string ParallelErpOrderToOrderTaskName => "Process Parallel Erp Order To Order Task";
    public static string ParallelErpOrderToOrderTaskType => "Nop.Plugin.Misc.ErpWebhook.Services.ScheduleTasks.ProcessParallelErpOrderToOrderTask";

    public static string ParallelErpAccountPricingToB2BPerAccountPricingTaskName => "Process Parallel Parallel_ErpAccountPricing To B2BPerAccountPricing Task";
    public static string ParallelErpAccountPricingToB2BPerAccountPricingTaskType => "Nop.Plugin.Misc.ErpWebhook.Services.ScheduleTasks.ProcessParallelErpAccountPricingToB2BPerAccountPricingTask";

    public static string ParallelErpStockToB2BStockTaskName => "Process Parallel Parallel_ErpStock To B2BStock Task";
    public static string ParallelErpStockToB2BStockTaskType => "Nop.Plugin.Misc.ErpWebhook.Services.ScheduleTasks.ProcessParallelErpStockToB2BStockTask";

    #region JWT

    public static readonly string Token = "Authorization";
    public static readonly string SecretKey = "SecretKey";
    public static readonly string CustomerId = "CustomerId";

    #endregion
}
