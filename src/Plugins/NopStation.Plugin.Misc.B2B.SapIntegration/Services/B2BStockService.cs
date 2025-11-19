using NopStation.Plugin.B2B.B2BB2CFeatures;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;
using SAP.Middleware.Connector;

namespace NopStation.Plugin.Misc.B2B.SapIntegration.Services;

public class B2BStockService : IB2BStockService
{
    private readonly IErpLogsService _erpLogService;
    private readonly SapIntegrationSettings _sapIntegrationSettings;
    private readonly B2BB2CFeaturesSettings _b2BB2CFeaturesSettings;

    public B2BStockService(IErpLogsService erpLogService,
        SapIntegrationSettings sapIntegrationSettings,
        B2BB2CFeaturesSettings b2BB2CFeaturesSettings)
    {
        _erpLogService = erpLogService;
        _sapIntegrationSettings = sapIntegrationSettings;
        _b2BB2CFeaturesSettings = b2BB2CFeaturesSettings;
    }

    private dynamic MapStock(IRfcStructure stockItem)
    {
        return new
        {
            MATNR = stockItem["MATNR"].GetString(),
            MATNR_CHARG = stockItem["MATNR"].GetString() + stockItem["CHARG"].GetString(),
            WERKS = stockItem["WERKS"].GetString(),
            QTY = stockItem["QTY"].GetDecimal(),
            QTYPLT = stockItem["QTYPLT"].GetDecimal(),
            CHARG = stockItem["CHARG"].GetString(),
            ARKTX = stockItem["ARKTX"].GetString(),
            UOM = stockItem["UOM"].GetString(),
            MASS = stockItem["MASS"].GetDecimal(),
            UPDATED = stockItem["UPDATED"].GetString()
        };
    }

    private async Task<ErpResponseData<IList<ErpStockDataModel>>> GetStockWithItemNos(ErpGetRequestModel erpRequest)
    {
        var erpResponseData = new ErpResponseData<IList<ErpStockDataModel>>();
        if (erpRequest.Limit == 0)
            erpRequest.Limit = 1000;
        try
        {
            if (!RfcDestinationManager.IsDestinationConfigurationRegistered())
                RfcDestinationManager.RegisterDestinationConfiguration(new SapConfig(_sapIntegrationSettings));

            var dest = RfcDestinationManager.GetDestination("HUBCLIENT");
            var repo = dest.Repository;

            var itemNos = erpRequest.ProductSku.Split(',');
            var stockItems = new List<ErpStockDataModel>();

            foreach (var itemNo in itemNos)
            {
                var func = repo.CreateFunction("ZEC_STOCK_GETLIST");

                if (!string.IsNullOrEmpty(erpRequest.WarehouseCode))  // location == warehouse (unlike in GetProducts)
                {
                    var eT_VKORG_SELT = func.GetTable("ET_PLANT_SELT");
                    eT_VKORG_SELT.Append();
                    eT_VKORG_SELT.SetValue("SIGN", "I");
                    eT_VKORG_SELT.SetValue("OPTION", "EQ");
                    eT_VKORG_SELT.SetValue("LOW", erpRequest.WarehouseCode);
                }

                var parts = itemNo.Split('|');
                var matnr = string.Empty;
                var charg = string.Empty;
                if (parts.Length > 0)
                {
                    matnr = parts[0];
                }
                else
                {
                    continue;
                }

                var eT_MATNR_SELT = func.GetTable("ET_MATNR_SELT");
                eT_MATNR_SELT.Append();
                eT_MATNR_SELT.SetValue("SIGN", "I");
                eT_MATNR_SELT.SetValue("OPTION", "EQ");
                eT_MATNR_SELT.SetValue("LOW", matnr);

                // We get "Element ET_CHARG_SELT of container metadata ZEC_STOCK_GETLIST unknown"
                // if we try to use this filter
                //
                // Until this is fixed on their side, the best we can do filter ourselves below:
                // ?.FirstOrDefault(si => charg.Equals(si.CHARG));
                //
                // Note that this in particular includes the case where charg == "".
                if (parts.Length > 1)
                {
                    charg = parts[1];
                    if (charg.StartsWith(parts[0]))
                        charg = charg.Substring(parts[0].Length);
                }

                if (erpRequest.DateFrom != null && erpRequest.DateFrom != DateTime.MinValue)
                {
                    func.SetValue("IV_TIME", erpRequest.DateFrom.Value.ToString("yyyyMMddHHmmss"));
                }

                await _erpLogService.InsertErpLogAsync(
                    ErpLogLevel.Debug,
                    ErpSyncLevel.Stock,
                    $"GetStock before BAPI call (Warehouse = {erpRequest.WarehouseCode}, Start = {erpRequest.Start}). Click view to see details.",
                    $"GetStock before BAPI call: {func}"
                );                

                func.Invoke(dest);

                await _erpLogService.InsertErpLogAsync(
                    ErpLogLevel.Debug,
                    ErpSyncLevel.Stock,
                    $"GetStock after BAPI call (Warehouse = {erpRequest.WarehouseCode}, Start = {erpRequest.Start}). Click view to see details.",
                    $"GetStock after BAPI call: {func}"
                );                

                var stockItem = func.GetTable("ES_STOCKS")?.Select(MapStock)
                    ?.FirstOrDefault(si => charg.Equals(si.CHARG));

                if (stockItem != null)
                {
                    stockItems.Add(new ErpStockDataModel
                    {
                        ManufacturerPartNumber = stockItem.MATNR ?? string.Empty,
                        Sku = $"{stockItem.MATNR ?? ""}{stockItem.CHARG ?? ""}",
                        //QTYPLT = stockItem.QTYPLT ?? 0,
                        Name = stockItem.ARKTX ?? string.Empty,
                        UnitOfMeasure = stockItem.UOM ?? string.Empty,
                        Weight = stockItem.MASS,
                        //UPDATED = stockItem.UPDATED ?? string.Empty,
                        WarehouseNameOrCode = stockItem.WERKS ?? string.Empty,
                        QuantityOnHand = Math.Floor(stockItem.QTY),
                    });
                }
            }

            erpResponseData.Data = stockItems;
            erpResponseData.ErpResponseModel.Next = null;
        }
        catch (Exception ex)
        {
            erpResponseData.ErpResponseModel.IsError = true;
            erpResponseData.ErpResponseModel.ErrorShortMessage = ex.Message;
            await _erpLogService.InsertErpLogAsync(
                ErpLogLevel.Error,
                ErpSyncLevel.Stock,
                $"GetStock error (Warehouse = {erpRequest.WarehouseCode}, Start = {erpRequest.Start}). Click view to see details.",
                $"Call GetProducts error: \nException message: {ex.Message}\nStack trace: {ex.StackTrace}"
            );
        }

        return erpResponseData;
    }

    public async Task<ErpResponseData<IList<ErpStockDataModel>>> GetStockFromErpAsync(ErpGetRequestModel erpRequest)
    {
        var erpResponseData = new ErpResponseData<IList<ErpStockDataModel>>();
        var responseContent = string.Empty;

        try
        {
            if (erpRequest == null)
            {
                erpResponseData.ErpResponseModel.IsError = false;
                erpResponseData.ErpResponseModel.ErrorShortMessage = "Request body content no data";
                return erpResponseData;
            }

            #region SAP

            if (!string.IsNullOrEmpty(erpRequest.ProductSku))
            {
                return await GetStockWithItemNos(erpRequest);
            }

            if (erpRequest.Limit == 0)
                erpRequest.Limit = 1000;
            try
            {
                if (!RfcDestinationManager.IsDestinationConfigurationRegistered())
                    RfcDestinationManager.RegisterDestinationConfiguration(new SapConfig(_sapIntegrationSettings));

                var dest = RfcDestinationManager.GetDestination("HUBCLIENT");
                var repo = dest.Repository;
                var func = repo.CreateFunction("ZEC_STOCK_GETLIST");

                if (!string.IsNullOrEmpty(erpRequest.WarehouseCode))
                {
                    var eT_VKORG_SELT = func.GetTable("ET_PLANT_SELT");
                    eT_VKORG_SELT.Append();
                    eT_VKORG_SELT.SetValue("SIGN", "I");
                    eT_VKORG_SELT.SetValue("OPTION", "EQ");
                    eT_VKORG_SELT.SetValue("LOW", erpRequest.WarehouseCode);
                }
                if (!string.IsNullOrEmpty(erpRequest.Start))
                {
                    func.SetValue("IV_STOCK_START", erpRequest.Start);
                }
                if (!string.IsNullOrEmpty(erpRequest.ProductSku))
                {
                    var eT_MATNR_SELT = func.GetTable("ET_MATNR_SELT");
                    var productSkus = erpRequest.ProductSku.Split(',');
                    foreach (var itemNo in productSkus)
                    {
                        eT_MATNR_SELT.Append();
                        eT_MATNR_SELT.SetValue("SIGN", "I");
                        eT_MATNR_SELT.SetValue("OPTION", "EQ");
                        eT_MATNR_SELT.SetValue("LOW", itemNo);
                    }
                }
                if (erpRequest.DateFrom != null && erpRequest.DateFrom != DateTime.MinValue)
                {
                    func.SetValue("IV_TIME", erpRequest.DateFrom.Value.ToString("yyyyMMddHHmmss"));
                }
                func.SetValue("IV_ROWS", erpRequest.Limit);

                await _erpLogService.InsertErpLogAsync(
                    ErpLogLevel.Debug,
                    ErpSyncLevel.Stock,
                    $"GetStock before BAPI call (Warehouse = {erpRequest.WarehouseCode}, Start = {erpRequest.Start}, Product = {erpRequest.ProductSku}). Click view to see details.",
                    $"GetStock before BAPI call : {func}"
                );                

                func.Invoke(dest);

                await _erpLogService.InsertErpLogAsync(
                    ErpLogLevel.Debug,
                    ErpSyncLevel.Stock,
                    $"GetStock after BAPI call (Warehouse = {erpRequest.WarehouseCode}, Start = {erpRequest.Start}, Product = {erpRequest.ProductSku}). Click view to see details.",
                    $"GetStock after BAPI call : {func}"
                );

                var stockItems = func.GetTable("ES_STOCKS");
                var stockDataList = stockItems
                    .Select(stockItem => new ErpStockDataModel
                    {
                        ManufacturerPartNumber = stockItem["MATNR"].GetString(),
                        Sku = stockItem["MATNR"].GetString() + stockItem["CHARG"].GetString(),
                        //QTYPLT = stockItem["QTYPLT"].GetDecimal(),
                        Name = stockItem["ARKTX"].GetString(),
                        UnitOfMeasure = stockItem["UOM"].GetString(),
                        Weight = stockItem["MASS"].GetDecimal(),
                        //UPDATED = stockItem["UPDATED"].GetString()
                        WarehouseNameOrCode = stockItem["WERKS"].GetString(),
                        QuantityOnHand = Math.Floor(stockItem["QTY"].GetDecimal()),
                    })
                    .ToList();

                erpResponseData.Data = stockDataList;

                erpResponseData.ErpResponseModel.Next = stockDataList.Count < erpRequest.Limit ? 
                    null : 
                    func.GetString("EV_STOCK_LAST");
            }
            catch (Exception ex)
            {
                await _erpLogService.InsertErpLogAsync(
                    ErpLogLevel.Error,
                    ErpSyncLevel.Stock, 
                    $"GetStock error (Warehouse = {erpRequest.WarehouseCode}, Start = {erpRequest.Start}, Product = {erpRequest.ProductSku}). Click view to see details.",
                    $"GetStock error: \nException message: {ex.Message}\nStack trace: {ex.StackTrace}"
                );
                erpResponseData.ErpResponseModel.IsError = true;
                erpResponseData.ErpResponseModel.ErrorShortMessage = ex.Message;
                return erpResponseData;
            }

            return erpResponseData;

            #endregion
        }
        catch (Exception ex)
        {
            erpResponseData.ErpResponseModel.IsError = true;
            erpResponseData.ErpResponseModel.ErrorShortMessage = ex.Message;
            erpResponseData.ErpResponseModel.ErrorFullMessage = !string.IsNullOrEmpty(responseContent)
                ? responseContent
                : ex.StackTrace;
        }

        return erpResponseData;
    }
}
