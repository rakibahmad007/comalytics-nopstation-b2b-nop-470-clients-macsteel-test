using System.Text;
using Newtonsoft.Json;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;
using NUglify.Helpers;
using SAP.Middleware.Connector;

namespace NopStation.Plugin.Misc.B2B.SapIntegration.Services;

public class B2BOrderService : IB2BOrderService
{
    private readonly IErpLogsService _erpLogsService;
    private readonly SapIntegrationSettings _sapIntegrationSettings;

    public B2BOrderService(IErpLogsService erpLogsService, SapIntegrationSettings sapIntegrationSettings)
    {
        _erpLogsService = erpLogsService;
        _sapIntegrationSettings = sapIntegrationSettings;
    }

    #region Utilities

    private string GetOrderType(string orderType)
    {
        var res = "B2BSalesOrder";
        if (orderType == "ZQT1")
            res = "B2BQuote";
        else if (orderType == "ZOR")
            res = "B2BSalesOrder";
        else if (orderType == "ZQCS")
            res = "B2CQuote";
        else if (orderType == "ZCSH")
            res = "B2CSalesOrder";
        return res;
    }

    //test log
    private async Task LogSapBapiMappingsB2B(ErpPlaceOrderDataModel request, IRfcFunction func)
    {
        var mappingLog = new StringBuilder();
        mappingLog.AppendLine($"=== SAP BAPI Field Mappings for B2B Order {request.ErpOrderNumber} ===");

        var header = func.GetStructure("IV_HEADER_DATA");

        // Header mappings logging
        var orderType = MapOrderType(request.OrderType);
        mappingLog.AppendLine($"AUART = MapOrderType(request.OrderType) ({orderType})");
        mappingLog.AppendLine($"ECOM_ID = request.Reference ({request.Reference ?? "null"})");
        mappingLog.AppendLine($"VKORG = request.Location ({request.Location ?? "null"})");
        mappingLog.AppendLine($"VTWEG = hardcoded (10)");

        var dateRequired = request.DateRequired?.ToString("yyyyMMdd");
        mappingLog.AppendLine($"VDATU = request.DateRequired ({dateRequired ?? "null"})");
        mappingLog.AppendLine($"FIRSTNAME = request.CustomerFirstName ({request.CustomerFirstName ?? "null"})");
        mappingLog.AppendLine($"LASTNAME = request.CustomerLastName ({request.CustomerLastName ?? "null"})");
        mappingLog.AppendLine($"BSTKD = request.CustomerReference ({request.CustomerReference ?? "null"})");
        mappingLog.AppendLine($"KUNNR = request.CustomerNumber ({request.CustomerNumber ?? "null"})");
        mappingLog.AppendLine($"KUNAG = request.AccountNumber ({request.AccountNumber ?? "null"})");
        mappingLog.AppendLine($"KUNWE = request.AddressCode ({request.AddressCode ?? "null"})");

        var deliveryMethod = request.DeliveryMethod == "COLLECT" ? "08" : "01";
        mappingLog.AppendLine($"VSBED = request.DeliveryMethod logic ({deliveryMethod})");
        mappingLog.AppendLine($"INSTUCTION = request.DeliveryInstruction ({request.DeliveryInstruction ?? "null"})");

        if (!string.IsNullOrEmpty(request.QuoteNumber))
        {
            mappingLog.AppendLine($"REF_DOC = request.QuoteNumber ({request.QuoteNumber})");
        }

        // Address mappings
        mappingLog.AppendLine($"NAME1 = request.ShippingAddress.Name ({request.ShippingAddress.Name ?? "null"})");
        mappingLog.AppendLine($"STREET = request.ShippingAddress.Address2 ({request.ShippingAddress.Address2 ?? "null"})");
        mappingLog.AppendLine($"CITY1 = request.ShippingAddress.City ({request.ShippingAddress.City ?? "null"})");
        mappingLog.AppendLine($"CITY2 = request.ShippingAddress.Suburb ({request.ShippingAddress.Suburb ?? "null"})");
        mappingLog.AppendLine($"POST_CODE1 = request.ShippingAddress.ZipPostalCode ({request.ShippingAddress.ZipPostalCode ?? "null"})");
        mappingLog.AppendLine($"REGION = request.ShippingAddress.Address3 ({request.ShippingAddress.Address3 ?? "null"})");
        mappingLog.AppendLine($"LAND1 = hardcoded (ZA)");
        mappingLog.AppendLine($"TEL_NUMBER = request.ShippingAddress.PhoneNumber ({request.ShippingAddress.PhoneNumber ?? "null"})");
        mappingLog.AppendLine($"HOUSE_NUM1 = request.ShippingAddress.Address1 ({request.ShippingAddress.Address1 ?? "null"})");

        // Line items mappings
        mappingLog.AppendLine("=== Line Items ===");
        var lineIndex = 0;
        request.ErpPlaceOrderItemDatas.ForEach((line) =>
        {
            mappingLog.AppendLine($"--- Line Item {++lineIndex} ---");
            mappingLog.AppendLine($"MATNR = line.Sku ({line.Sku ?? "null"})");
            mappingLog.AppendLine($"CHARG = line.BatchCode ({line.BatchCode ?? "null"})");
            mappingLog.AppendLine($"KWMENG = line.Quantity ({line.Quantity})");
            mappingLog.AppendLine($"KBETR = line.UnitPriceExclTax ({line.UnitPriceExclTax})");
            mappingLog.AppendLine($"DISCOUNT = line.Discount ({line.Discount})");
            mappingLog.AppendLine($"VRKME = line.UnitOfMeasure ({line.UnitOfMeasure ?? "null"})");
        });

        // Log all mappings
        await _erpLogsService.InsertErpLogAsync(
            ErpLogLevel.Debug,
            ErpSyncLevel.Order,
            $"SAP BAPI Mappings (B2B Order = {request.ErpOrderNumber}) . Click view to see details.",
            $"SAP BAPI Mappings:\n\n{mappingLog}");
    }

    //test log
    private async Task LogSapBapiMappingsB2C(ErpPlaceOrderDataModel request, IRfcFunction func)
    {
        var mappingLog = new StringBuilder();
        mappingLog.AppendLine($"=== SAP BAPI Field Mappings for B2C Order {request.ErpOrderNumber} ===\n\n");

        // Location adjustment logic
        var adjustedLocation = request.Location;
        if (!string.IsNullOrWhiteSpace(request.Location) && request.Location.Equals("1032"))
        {
            adjustedLocation = "1030";
            mappingLog.AppendLine($"Location adjusted: {request.Location} -> {adjustedLocation}");
        }

        var header = func.GetStructure("IV_HEADER_DATA");

        // Header mappings logging
        var orderType = MapOrderType(request.OrderType);
        mappingLog.AppendLine($"AUART = MapOrderType(request.OrderType) ({orderType})");
        mappingLog.AppendLine($"VKORG = request.Location (adjusted: {adjustedLocation})");
        mappingLog.AppendLine($"ECOM_ID = request.Reference ({request.Reference ?? "null"})");
        mappingLog.AppendLine($"VTWEG = hardcoded (10)");

        var dateRequired = request.DateRequired?.ToString("yyyyMMdd");
        mappingLog.AppendLine($"VDATU = request.DateRequired ({dateRequired ?? "null"})");
        mappingLog.AppendLine($"BSTKD = request.CustomerReference ({request.CustomerReference ?? "null"})");

        var deliveryMethod = request.DeliveryMethod == "COLLECT" ? "03" : "01"; // Note: B2C uses "03" vs B2B "08"
        mappingLog.AppendLine($"VSBED = request.DeliveryMethod logic ({deliveryMethod})");
        mappingLog.AppendLine($"INSTUCTION = request.DeliveryInstruction ({request.DeliveryInstruction ?? "null"})");

        mappingLog.AppendLine($"KUNWE = request.AccountNumber ({request.AccountNumber ?? "null"})");
        mappingLog.AppendLine($"KUNNR = request.AccountNumber ({request.AccountNumber ?? "null"})");
        mappingLog.AppendLine($"KUNAG = request.AccountNumber ({request.AccountNumber ?? "null"})");

        if (!string.IsNullOrEmpty(request.QuoteNumber))
        {
            mappingLog.AppendLine($"REF_DOC = request.QuoteNumber ({request.QuoteNumber})");
        }

        // B2C specific PAYGATE_ID field
        if (request.OrderType.ToUpper().Equals("B2CORDER") || request.OrderType.ToUpper().Equals("Order from B2C Quote".ToUpper()))
        {
            mappingLog.AppendLine($"PAYGATE_ID = request.Reference ({request.Reference ?? "null"})");
        }

        // Address mappings (B2C specific)
        mappingLog.AppendLine("=== Address Structure ===");
        mappingLog.AppendLine($"NAME1 = request.CustomerName ({request.CustomerName ?? "null"})"); // Different from B2B
        mappingLog.AppendLine($"STREET = request.ShippingAddress.Address2 ({request.ShippingAddress.Address2 ?? "null"})");
        mappingLog.AppendLine($"CITY1 = request.ShippingAddress.City ({request.ShippingAddress.City ?? "null"})");
        mappingLog.AppendLine($"CITY2 = request.ShippingAddress.Suburb ({request.ShippingAddress.Suburb ?? "null"})");
        mappingLog.AppendLine($"POST_CODE1 = request.ShippingAddress.ZipPostalCode ({request.ShippingAddress.ZipPostalCode ?? "null"})");
        mappingLog.AppendLine($"REGION = request.ShippingAddress.Address3 ({request.ShippingAddress.Address3 ?? "null"})");
        mappingLog.AppendLine($"LAND1 = hardcoded (ZA)");
        mappingLog.AppendLine($"HOUSE_NUM1 = request.ShippingAddress.Address1 ({request.ShippingAddress.Address1 ?? "null"})");
        mappingLog.AppendLine($"TEL_NUMBER = request.CustomerPhoneNumber ({request.CustomerPhoneNumber ?? "null"})"); // B2C specific field
        mappingLog.AppendLine($"EMAIL_ID = request.CustomerEmail ({request.CustomerEmail ?? "null"})"); // B2C specific field
        mappingLog.AppendLine($"VAT_REG_NO = request.VatNumber ({request.VatNumber ?? "null"})"); // B2C specific field

        // Line items mappings
        mappingLog.AppendLine("=== Line Items (ET_ITEM_DATA) ===");
        var lineIndex = 0;
        request.ErpPlaceOrderItemDatas.ForEach((line) =>
        {
            var lineNumber = int.TryParse(line.SerialNumber, out var parsedLineNumber) ? parsedLineNumber : 0;
            mappingLog.AppendLine($"--- Line Item {++lineIndex} ---");
            mappingLog.AppendLine($"MATNR = line.Sku ({line.Sku ?? "null"})");
            mappingLog.AppendLine($"CHARG = line.BatchCode ({line.BatchCode ?? "null"})");
            mappingLog.AppendLine($"KWMENG = line.Quantity ({line.Quantity})");
            mappingLog.AppendLine($"KBETR = line.UnitPriceExclTax ({line.UnitPriceExclTax})");
            mappingLog.AppendLine($"DISCOUNT = line.Discount ({line.Discount})");
            mappingLog.AppendLine($"VRKME = line.UnitOfMeasure ({line.UnitOfMeasure ?? "null"})");
            mappingLog.AppendLine($"REQ_DATE = line.DeliveryDate ({line?.DeliveryDate.ToString() ?? "null"})"); // B2C specific
            mappingLog.AppendLine($"WERKS = line.WarehouseCode ({line.WarehouseCode ?? "null"})"); // B2C specific
            mappingLog.AppendLine($"POSNR = parsed from line.SerialNumber ({lineNumber})"); // B2C specific

            // Special instruction handling
            if (!string.IsNullOrEmpty(line.SpecialInstruction))
            {
                mappingLog.AppendLine($"--- Special Instruction for Line {lineIndex} ---");
                mappingLog.AppendLine($"ET_ITEM_TEXT.ID = Z001");
                mappingLog.AppendLine($"ET_ITEM_TEXT.POSNR = line.SerialNumber ({line.SerialNumber ?? "null"})");
                mappingLog.AppendLine($"TEXT.TDLINE = line.SpecialInstruction ({line.SpecialInstruction})");
            }
        });

        // Shipping cost handling (B2C specific)
        if (!string.IsNullOrWhiteSpace(_sapIntegrationSettings.ShippingCostSKU) && request.ShippingAmount > 0)
        {
            mappingLog.AppendLine("=== Shipping Cost Line Item ===");
            mappingLog.AppendLine($"MATNR = _sapIntegrationSettings.ShippingCostSKU ({_sapIntegrationSettings.ShippingCostSKU})");
            mappingLog.AppendLine($"CHARG = (empty)");
            mappingLog.AppendLine($"KWMENG = 1");
            mappingLog.AppendLine($"VRKME = EA");
            mappingLog.AppendLine($"KBETR = request.ShippingAmount ({request.ShippingAmount})");
            mappingLog.AppendLine($"DISCOUNT = 0");
            mappingLog.AppendLine($"POSNR = {request.ErpPlaceOrderItemDatas.Count() + 1}");
            mappingLog.AppendLine($"ET_ITEM_TEXT.ID = Z001");
            mappingLog.AppendLine($"ET_ITEM_TEXT.POSNR = {request.ErpPlaceOrderItemDatas.Count() + 1}");
            mappingLog.AppendLine($"TEXT.TDLINE = (empty)");
        }

        // Log all mappings
        await _erpLogsService.InsertErpLogAsync(
            ErpLogLevel.Debug,
            ErpSyncLevel.Order,
            $"SAP BAPI Mappings (B2C Order = {request.ErpOrderNumber}) . Click view to see details.",
            $"SAP BAPI Mappings:\n\n{mappingLog}");
    }

    // Detailed request payload logger for Get Orders BAPI
    private async Task LogGetOrdersBapiRequestAsync(ErpGetRequestModel request, IRfcFunction func)
    {
        var mappingLog = new StringBuilder();
        mappingLog.AppendLine("=== SAP BAPI Get Orders Request (ZEC_ORDER_GETLIST_DETAIL) ===");

        // Basic request model values
        mappingLog.AppendLine($"AccountNumber = request.AccountNumber ({request?.AccountNumber ?? "null"})");
        mappingLog.AppendLine($"Location = request.Location ({request?.Location ?? "null"})");
        mappingLog.AppendLine($"Start (OrderNo) = request.Start ({request?.Start ?? "null"})");
        mappingLog.AppendLine($"DateFrom = request.DateFrom ({request?.DateFrom?.ToString("yyyy-MM-dd HH:mm:ss") ?? "null"})");
        mappingLog.AppendLine($"Limit = request.Limit ({request?.Limit})");

        // Selection tables populated on the function
        try
        {
            var kunnrSelt = func.GetTable("ET_KUNNR_SELT");
            mappingLog.AppendLine("--- ET_KUNNR_SELT (Customer Selection) ---");
            if (kunnrSelt != null && kunnrSelt.RowCount > 0)
            {
                var rowIdx = 0;
                foreach (var row in kunnrSelt)
                {
                    mappingLog.AppendLine($"Row {++rowIdx}: " +
                        $"SIGN={row.GetString("SIGN")}, " +
                        $"OPTION={row.GetString("OPTION")}, " +
                        $"LOW={row.GetString("LOW")}, " +
                        $"HIGH={row.GetString("HIGH")}");
                }
            }
            else
            {
                mappingLog.AppendLine("(empty)");
            }
        }
        catch { mappingLog.AppendLine("--- ET_KUNNR_SELT: (unavailable) ---"); }

        try
        {
            var vkorgSelt = func.GetTable("ET_VKORG_SELT");
            mappingLog.AppendLine("--- ET_VKORG_SELT (Sales Org Selection) ---");
            if (vkorgSelt != null && vkorgSelt.RowCount > 0)
            {
                var rowIdx = 0;
                foreach (var row in vkorgSelt)
                {
                    mappingLog.AppendLine($"Row {++rowIdx}: " +
                        $"SIGN={row.GetString("SIGN")}, " +
                        $"OPTION={row.GetString("OPTION")}, " +
                        $"LOW={row.GetString("LOW")}, " +
                        $"HIGH={row.GetString("HIGH")}");
                }
            }
            else
            {
                mappingLog.AppendLine("(empty)");
            }
        }
        catch { mappingLog.AppendLine("--- ET_VKORG_SELT: (unavailable) ---"); }

        // IV_ORD_START structure
        try
        {
            var ordStart = func.GetStructure("IV_ORD_START");
            mappingLog.AppendLine("--- IV_ORD_START ---");
            mappingLog.AppendLine($"KUNNR = {ordStart?.GetString("KUNNR") ?? "null"}");
            mappingLog.AppendLine($"VBELN = {ordStart?.GetString("VBELN") ?? "null"}");
            mappingLog.AppendLine($"VKORG = {ordStart?.GetString("VKORG") ?? "null"}");
        }
        catch { mappingLog.AppendLine("--- IV_ORD_START: (unavailable) ---"); }

        // Scalar parameters
        try
        {
            mappingLog.AppendLine("--- Scalars ---");
            mappingLog.AppendLine($"IV_CHANGED = {func.GetString("IV_CHANGED")}");
            mappingLog.AppendLine($"IV_ROWS = {func.GetInt("IV_ROWS")}");
        }
        catch { /* ignore */ }

        await _erpLogsService.InsertErpLogAsync(
            ErpLogLevel.Debug,
            ErpSyncLevel.Order,
            $"Before Get-Orders call to BAPI (Account = {request?.AccountNumber}, Location = {request?.Location}, Start = {request?.Start}, Limit = {request?.Limit}, DateFrom = {request?.DateFrom?.ToString("yyyy-MM-dd HH:mm:ss")}). Click view to see details.",
            $"Data used to create request: {JsonConvert.SerializeObject(request)}\n\n" +
            $"{func}\n\n" + 
            $"Mappings:\n\n{mappingLog}"
        );
    }

    private async Task<IRfcFunction> MapB2BOrderAndQuoteData(ErpPlaceOrderDataModel request, IRfcFunction func)
    {
        //2330
        if (!string.IsNullOrWhiteSpace(request.Location) && request.Location.Equals("1032"))
            request.Location = "1030";

        var header = func.GetStructure("IV_HEADER_DATA");
        //header.SetValue("VBLEN1", request.Reference);
        header.SetValue("AUART", MapOrderType(request.OrderType));
        header.SetValue("ECOM_ID", request.Reference);
        header.SetValue("VKORG", request.Location);
        header.SetValue("VTWEG", "10");
        header.SetValue("VDATU", request.DateRequired?.ToString("yyyyMMdd"));
        header.SetValue("FIRSTNAME", request.CustomerFirstName);
        header.SetValue("LASTNAME", request.CustomerLastName);
        header.SetValue("BSTKD", request.CustomerReference);
        header.SetValue("KUNNR", request.CustomerNumber);
        header.SetValue("KUNAG", request.AccountNumber);
        header.SetValue("KUNWE", request.AddressCode);
        header.SetValue("VSBED", request.DeliveryMethod == "COLLECT" ? "08" : "01");
        header.SetValue("INSTUCTION", request.DeliveryInstruction);

        if (!string.IsNullOrEmpty(request.QuoteNumber))
        {
            header.SetValue("REF_DOC", request.QuoteNumber);
        }

        var address = header.GetStructure("ADDRESS");
        address.SetValue("NAME1", request.ShippingAddress.Name);
        address.SetValue("STREET", request.ShippingAddress.Address2);
        address.SetValue("CITY1", request.ShippingAddress.City);
        address.SetValue("CITY2", request.ShippingAddress.Suburb);
        address.SetValue("POST_CODE1", request.ShippingAddress.ZipPostalCode);
        address.SetValue("REGION", request.ShippingAddress.Address3);
        //address.SetValue("LAND1", request.ShippingAddress.Country);
        address.SetValue("LAND1", "ZA");
        address.SetValue("TEL_NUMBER", request.ShippingAddress.PhoneNumber);
        address.SetValue("HOUSE_NUM1", request.ShippingAddress.Address1);

        request.ErpPlaceOrderItemDatas.ForEach((line) =>
        {
            var lines = func.GetTable("ET_ITEM_DATA");
            lines.Append();
            lines.SetValue("MATNR", line.Sku);
            lines.SetValue("CHARG", line.BatchCode);
            lines.SetValue("KWMENG", line.Quantity);
            lines.SetValue("KBETR", line.UnitPriceExclTax);
            lines.SetValue("DISCOUNT", line.Discount);
            lines.SetValue("VRKME", line.UnitOfMeasure);
        });

        //log mapping for the req
        await LogSapBapiMappingsB2B(request, func);

        return func;
    }

    private async Task<IRfcFunction> MapB2COrderAndQuoteData(ErpPlaceOrderDataModel request, IRfcFunction func)
    {
        //2330
        if (!string.IsNullOrWhiteSpace(request.Location) && request.Location.Equals("1032"))
            request.Location = "1030";

        var header = func.GetStructure("IV_HEADER_DATA");
        //header.SetValue("VBLEN1", request.Reference);
        header.SetValue("AUART", MapOrderType(request.OrderType));
        header.SetValue("VKORG", request.Location);
        header.SetValue("ECOM_ID", request.Reference);
        header.SetValue("VTWEG", "10");
        header.SetValue("VDATU", request.DateRequired?.ToString("yyyyMMdd"));
        header.SetValue("BSTKD", request.CustomerReference);
        header.SetValue("VSBED", request.DeliveryMethod == "COLLECT" ? "03" : "01");
        header.SetValue("INSTUCTION", request.DeliveryInstruction);
        header.SetValue("KUNWE", request.AccountNumber);
        header.SetValue("KUNNR", request.AccountNumber);
        header.SetValue("KUNAG", request.AccountNumber);
        if (!string.IsNullOrEmpty(request.QuoteNumber))
        {
            header.SetValue("REF_DOC", request.QuoteNumber);
        }

        if (request.OrderType.ToUpper().Equals("B2CORDER") || request.OrderType.ToUpper().Equals("Order from B2C Quote".ToUpper()))
        {
            header.SetValue("PAYGATE_ID", request.Reference);
        }

        var address = header.GetStructure("ADDRESS");
        address.SetValue("NAME1", request.CustomerName);
        address.SetValue("STREET", request.ShippingAddress.Address2);
        address.SetValue("CITY1", request.ShippingAddress.City);
        address.SetValue("CITY2", request.ShippingAddress.Suburb);
        address.SetValue("POST_CODE1", request.ShippingAddress.ZipPostalCode);
        address.SetValue("REGION", request.ShippingAddress.Address3);
        //address.SetValue("LAND1", request.ShippingAddress.Country);
        address.SetValue("LAND1", "ZA");
        address.SetValue("HOUSE_NUM1", request.ShippingAddress.Address1);

        address.SetValue("TEL_NUMBER", request.CustomerPhoneNumber);
        address.SetValue("EMAIL_ID", request.CustomerEmail);
        address.SetValue("VAT_REG_NO", request.VatNumber);
        //var vat = header.GetStructure("KNA1");
        //vat.SetValue("STECG", request.CustomerName);

        request.ErpPlaceOrderItemDatas.ForEach((line) =>
        {
            _ = int.TryParse(line.SerialNumber, out var lineNumber);

            var lines = func.GetTable("ET_ITEM_DATA");
            lines.Append();
            lines.SetValue("MATNR", line.Sku);
            lines.SetValue("CHARG", line.BatchCode);
            lines.SetValue("KWMENG", line.Quantity);
            lines.SetValue("KBETR", line.UnitPriceExclTax);
            lines.SetValue("DISCOUNT", line.Discount);
            lines.SetValue("VRKME", line.UnitOfMeasure);
            lines.SetValue("REQ_DATE", line.DeliveryDate);
            lines.SetValue("WERKS", line.WarehouseCode);
            lines.SetValue("POSNR", lineNumber);
            if (!string.IsNullOrEmpty(line.SpecialInstruction))
            {
                var text = func.GetTable("ET_ITEM_TEXT");
                text.Append();
                text.SetValue("ID", "Z001");
                text.SetValue("POSNR", line.SerialNumber);

                var tabText = text.GetTable("TEXT");
                tabText.Append();
                tabText.SetValue("TDLINE", line.SpecialInstruction);
            }
        });

        if (!string.IsNullOrWhiteSpace(_sapIntegrationSettings.ShippingCostSKU) && request.ShippingAmount > 0)
        {
            var lines = func.GetTable("ET_ITEM_DATA");
            lines.Append();
            lines.SetValue("MATNR", _sapIntegrationSettings.ShippingCostSKU);
            lines.SetValue("CHARG", "");
            lines.SetValue("KWMENG", 1);
            lines.SetValue("VRKME", "EA");
            lines.SetValue("KBETR", request.ShippingAmount);
            lines.SetValue("DISCOUNT", 0);
            lines.SetValue("POSNR", request.ErpPlaceOrderItemDatas.Count + 1);
            var text = func.GetTable("ET_ITEM_TEXT");
            text.Append();
            text.SetValue("ID", "Z001");
            text.SetValue("POSNR", request.ErpPlaceOrderItemDatas.Count + 1);
            var tabText = text.GetTable("TEXT");
            tabText.Append();
            tabText.SetValue("TDLINE", "");
        }

        await LogSapBapiMappingsB2C(request, func);
        
        return func;
    }

    private string MapOrderType(string inputType)
    {
        var typeMap = B2bOrderTypeMap;
        if (inputType != null && inputType.Trim().ToLowerInvariant().Contains("b2c"))
        {
            typeMap = B2cOrderTypeMap;
        }
        if (inputType != null && typeMap.Value.TryGetValue(inputType, out var result))
        {
            return result;
        }
        else if (typeMap.Value.TryGetValue(string.Empty, out result))
        {
            return result;
        }
        return string.Empty;
    }

    private Lazy<IDictionary<string, string>> B2bOrderTypeMap => new Lazy<IDictionary<string, string>>(() =>
    {
        var configValue = _sapIntegrationSettings.B2BOrderTypeMappings ?? "";
        if (string.IsNullOrWhiteSpace(configValue))
        {
            configValue =
                @"{""QUOTE"": ""ZQT1"", ""OrderFromQuote"": ""ZOR"", ""Order From Quote"": ""ZOR"", """": ""ZOR""}";
        }
        var mappings = JsonConvert.DeserializeObject<IDictionary<string, string>>(
            configValue
        );
        return new Dictionary<string, string>(mappings, StringComparer.InvariantCultureIgnoreCase);
    });

    private Lazy<IDictionary<string, string>> B2cOrderTypeMap => new Lazy<IDictionary<string, string>>(() =>
    {
        var configValue = _sapIntegrationSettings.B2COrderTypeMappings ?? "";
        if (string.IsNullOrWhiteSpace(configValue))
        {
            configValue =
                @"{""B2CQUOTE"": ""ZQCS"", ""B2C QUOTE"": ""ZQCS"", ""OrderFromB2CQuote"": ""ZCSH"", ""Order From B2C Quote"": ""ZCSH"", """": ""ZCSH""}";
        }
        var mappings = JsonConvert.DeserializeObject<IDictionary<string, string>>(
            configValue
        );
        return new Dictionary<string, string>(mappings, StringComparer.InvariantCultureIgnoreCase);
    });

    #endregion

    #region Methods

    public async Task<ErpResponseModel> CreateOrderOnErpAsync(ErpPlaceOrderDataModel erpPlaceOrderDataModel)
    {
        var erpResponseData = new ErpResponseModel();
        var responseContent = string.Empty;
        try
        {
            if (erpPlaceOrderDataModel == null)
            {
                erpResponseData.IsError = false;
                erpResponseData.ErrorShortMessage = "Request body content no data";
                return erpResponseData;
            }

            try
            {
                if (!RfcDestinationManager.IsDestinationConfigurationRegistered())
                    RfcDestinationManager.RegisterDestinationConfiguration(new SapConfig(_sapIntegrationSettings));

                var dest = RfcDestinationManager.GetDestination("HUBCLIENT");
                var repo = dest.Repository;
                var func = repo.CreateFunction("ZEC_ORDERS_CREATE");
                if (erpPlaceOrderDataModel.OrderType.Trim().Contains("b2c", StringComparison.InvariantCultureIgnoreCase))
                {
                    func = await MapB2COrderAndQuoteData(erpPlaceOrderDataModel, func);
                }
                else
                {
                    func = await MapB2BOrderAndQuoteData(erpPlaceOrderDataModel, func);
                }

                await _erpLogsService.InsertErpLogAsync(
                    ErpLogLevel.Information,
                    ErpSyncLevel.Order,
                    $"Before Place-Order call to BAPI (OrderType = {erpPlaceOrderDataModel.OrderType}, Account = {erpPlaceOrderDataModel.AccountNumber}). Click view to see details.",
                    $"Before Place-Order call to BAPI: {func}\n\n" +
                    $"Data used to create request: {JsonConvert.SerializeObject(erpPlaceOrderDataModel)}"
                );

                func.Invoke(dest);

                await _erpLogsService.InsertErpLogAsync(
                    ErpLogLevel.Debug, 
                    ErpSyncLevel.Order,
                    $"After Place-Order call to BAPI (OrderType = {erpPlaceOrderDataModel.OrderType}, Account = {erpPlaceOrderDataModel.AccountNumber}). Click view to see details.",
                    $"After Place-Order call to BAPI: {func}"
                );

                var order = func.GetStructure("EV_SALES_ORDER");
                var orderNo = order.GetString("VBELN");
                if (erpPlaceOrderDataModel.OrderType.Trim().Contains("quote", StringComparison.InvariantCultureIgnoreCase))
                {
                    var expiryDateString = order.GetString("VALID_TO");
                    if (string.IsNullOrEmpty(expiryDateString))
                    {
                        await _erpLogsService.InsertErpLogAsync(
                            ErpLogLevel.Debug,
                            ErpSyncLevel.Order,
                            $"QuoteExpiryDate: expiry date of '{orderNo}' is null or empty."
                        );
                    }
                    else
                    {
                        var convertedToDateTime = DateTime.TryParse(expiryDateString, out DateTime quoteExpiryDate);
                        if (convertedToDateTime)
                        {
                            erpResponseData.QuoteExpiryDate = quoteExpiryDate;
                            await _erpLogsService.InsertErpLogAsync(
                                ErpLogLevel.Debug,
                                ErpSyncLevel.Order,
                                $"QuoteExpiryDate: expiry date of '{orderNo}' is {quoteExpiryDate}."
                            );
                        }
                    }
                }

                var errorMessage = order.GetString("MESSAGE");
                var isError = errorMessage != "";

                /*
                 * VBELN now carries the order number when duplcate order message is sent from sap
                 * which means this order was placed successfully
                 * we will send this order number to nop
                */
                if (
                    !string.IsNullOrEmpty(orderNo)
                    && !string.IsNullOrEmpty(errorMessage)
                    && errorMessage.Contains("Duplicate")
                )
                {
                    isError = false;
                    errorMessage = string.Empty;
                }

                erpResponseData.IsTemporary = false;
                erpResponseData.ErrorShortMessage = errorMessage;
                erpResponseData.OrderNumber = orderNo;
                erpResponseData.IsError = isError;

                if (erpPlaceOrderDataModel.OrderType.Trim().Contains("b2c", StringComparison.InvariantCultureIgnoreCase))
                {
                    await _erpLogsService.InformationAsync(
                        $"Placed B2C Order/Quote Number = '{orderNo}', " +
                        $"Location = '{erpPlaceOrderDataModel.Location}', " +
                        $"Customer number = '{erpPlaceOrderDataModel.CustomerNumber}'",
                        ErpSyncLevel.Order
                    );
                }
                else
                {
                    await _erpLogsService.InformationAsync(
                        $"Placed Order/Quote Number = '{orderNo}', " +
                        $"Customer email = '{erpPlaceOrderDataModel.CustomerEmail}'",
                        ErpSyncLevel.Order
                    );
                }
            }
            catch (Exception ex)
            {
                await _erpLogsService.InsertErpLogAsync(
                    ErpLogLevel.Error,
                    ErpSyncLevel.Order,
                    $"Error placing order to SAP (OrderType = {erpPlaceOrderDataModel.OrderType}, Account = {erpPlaceOrderDataModel.AccountNumber}). Click view to see details.",
                    $"Error placing order to SAP:\nMessage: {ex.Message}\nStackTrace: {ex.StackTrace}"
                );
                erpResponseData.IsError = true;
                erpResponseData.ErrorShortMessage = ex.Message;
            }

            return erpResponseData;
        }
        catch (Exception ex)
        {
            erpResponseData.IsError = true;
            erpResponseData.ErrorShortMessage = ex.Message;
            erpResponseData.ErrorFullMessage = !string.IsNullOrEmpty(responseContent)
                ? responseContent
                : ex.StackTrace;
        }

        return erpResponseData;
    }

    public async Task<ErpResponseData<IList<ErpPlaceOrderDataModel>>> GetOrderByAccountFromErpAsync(
        ErpGetRequestModel erpRequest
    )
    {
        var erpResponseData = new ErpResponseData<IList<ErpPlaceOrderDataModel>>()
        {
            Data = new List<ErpPlaceOrderDataModel>(),
            ErpResponseModel = new ErpResponseModel(),
        };
        var responseContent = string.Empty;

        try
        {
            if (erpRequest == null)
            {
                erpResponseData.ErpResponseModel.IsError = false;
                erpResponseData.ErpResponseModel.ErrorShortMessage = "Request body content no data";
                return erpResponseData;
            }

            var locationChanged = false;
            if (
                !string.IsNullOrWhiteSpace(erpRequest.Location)
                && erpRequest.Location.Equals("1032")
            )
            {
                erpRequest.Location = "1030";
                locationChanged = true;
            }

            if (erpRequest.Start == "0")
                erpRequest.Start = null;

            if (erpRequest.Limit == 0)
                erpRequest.Limit = 1000;
            try
            {
                if (!RfcDestinationManager.IsDestinationConfigurationRegistered())
                    RfcDestinationManager.RegisterDestinationConfiguration(new SapConfig(_sapIntegrationSettings));

                var dest = RfcDestinationManager.GetDestination("HUBCLIENT");
                var repo = dest.Repository;
                var func = repo.CreateFunction("ZEC_ORDER_GETLIST_DETAIL");
                if (!string.IsNullOrEmpty(erpRequest.AccountNumber))
                {
                    var eT_KUNNR_SELT = func.GetTable("ET_KUNNR_SELT");
                    eT_KUNNR_SELT.Append();
                    eT_KUNNR_SELT.SetValue("SIGN", "I");
                    eT_KUNNR_SELT.SetValue("OPTION", "EQ");
                    eT_KUNNR_SELT.SetValue("LOW", erpRequest.AccountNumber);
                    eT_KUNNR_SELT.SetValue("HIGH", erpRequest.AccountNumber);
                }
                if (!string.IsNullOrEmpty(erpRequest.Location))
                {
                    var eT_KUNNR_SELT = func.GetTable("ET_VKORG_SELT");
                    eT_KUNNR_SELT.Append();
                    eT_KUNNR_SELT.SetValue("SIGN", "I");
                    eT_KUNNR_SELT.SetValue("OPTION", "EQ");
                    eT_KUNNR_SELT.SetValue("LOW", erpRequest.Location);
                    eT_KUNNR_SELT.SetValue("HIGH", erpRequest.Location);
                }
                if (!string.IsNullOrEmpty(erpRequest.Start))
                {
                    var iV_ORD_START = func.GetStructure("IV_ORD_START");
                    iV_ORD_START.SetValue("KUNNR", erpRequest.AccountNumber);
                    iV_ORD_START.SetValue("VBELN", erpRequest.Start);
                    iV_ORD_START.SetValue("VKORG", erpRequest.Location);
                }
                if (erpRequest.DateFrom.HasValue)
                {
                    func.SetValue("IV_CHANGED", erpRequest.DateFrom);
                }

                func.SetValue("IV_ROWS", erpRequest.Limit);

                await LogGetOrdersBapiRequestAsync(erpRequest, func);

                func.Invoke(dest);

                var shippingCost = decimal.Zero;
                var orders = func.GetTable("ES_ORDERS");

                var allBapiFields = orders != null ? orders.ToString() : "Empty result";

                await _erpLogsService.InsertErpLogAsync(
                    ErpLogLevel.Debug,
                    ErpSyncLevel.Order,
                    $"After GetOrders call to BAPI (Account = {erpRequest.AccountNumber}, " +
                    $"Location = {erpRequest.Location}, " +
                    $"Start = {erpRequest.Start}, " +
                    $"Limit = {erpRequest.Limit}, " +
                    $"DateFrom = {erpRequest.DateFrom?.ToString("yyyy-MM-dd HH:mm:ss")}" +
                    (orders != null && orders.Any() ? 
                        $", Next = {orders.Last()["VBELN"].GetString()}, Total orders received: {orders.Count}" : "") + 
                    $"). Click view to see details.",
                    $"After GetOrders call to BAPI: {func}\n\n" +
                    $"Order fields: \n\n{allBapiFields}."
                );

                if (orders != null && orders.Any())
                {
                    foreach (var order in orders)
                    {
                        try
                        {
                            var orderType = order["AUART"].GetString();
                            decimal? cashRounding = null;
                            try
                            {
                                cashRounding = order["ROUNDING_OFF_H"].GetDecimal();
                            }
                            catch
                            {
                                cashRounding = null;
                            }

                            _ = DateTime.TryParse(order["VDATU"].GetString(), out var delDate);
                            _ = DateTime.TryParse(order["ERDAT"].GetString(), out var orderDate);
                            _ = DateTime.TryParse(order["BNDDT"].GetString(), out var quoteExpiryDate);

                            var placeOrderDataModel = new ErpPlaceOrderDataModel
                            {
                                OrderType = GetOrderType(orderType),
                                Location = locationChanged ? "1032" : order["VKORG"].GetString(),
                                ErpOrderNumber = order["VBELN"].GetString(),
                                AccountNumber = order["KUNNR"].GetString(),
                                DeliveryInstruction = order["SPCL_INS"].GetString(),
                                CustomerReference = order["BSTKD"].GetString(),
                                CustomerEmail = order["SMTP_ADDR"].GetString(),

                                CustomOrderNumber = order["ECOM_ID"].GetString(),
                                OrderSubtotalExclTax = order["NETWR"].GetDecimal(),
                                OrderSubtotalInclTax = order["NETWR1"].GetDecimal(),
                                OrderTax = order["KWERT"].GetDecimal(),

                                DeliveryDate = delDate == DateTime.MinValue ? null : delDate,
                                OrderDate = orderDate == DateTime.MinValue ? null : orderDate,
                                QuoteExpiryDate = quoteExpiryDate == DateTime.MinValue ? null : orderDate,
                                CashRounding = cashRounding,

                                Address1 = order.GetStructure("CUST_ADDR")["HOUSE_NUM1"].GetString(),
                                Address2 = order.GetStructure("CUST_ADDR")["STREET"].GetString(),
                                Address3 = order.GetStructure("ORD_ADDR")["CITY2"].GetString(),
                                AddressCountryCode = order.GetStructure("ORD_ADDR")["LAND1"].GetString(),  // SAP does not have Land code on Billing Address
                                AddressProvince = order.GetStructure("CUST_ADDR")["REGION"].GetString(),
                                PostalCode = order.GetStructure("CUST_ADDR")["POST_CODE1"].GetString(),
                                DelName = order["NAME1"].GetString(),
                                DelPhone = order["TELF1"].GetString(),
                                DelEmail = order["SMTP_ADDR"].GetString(),
                                DelAddress1 = order.GetStructure("ORD_ADDR")["HOUSE_NUM1"].GetString(),
                                DelAddress2 = order.GetStructure("ORD_ADDR")["STREET"].GetString(),
                                DelAddress3 = order.GetStructure("ORD_ADDR")["CITY2"].GetString(),
                                DelPostalCode = order.GetStructure("ORD_ADDR")["POST_CODE1"].GetString(),
                                DelCountryCode = order.GetStructure("ORD_ADDR")["LAND1"].GetString(),
                                DelProvince = order.GetStructure("ORD_ADDR")["REGION"].GetString(),
                                Status = order["GBSTK"].GetString(),

                                ShippingAddress = new ErpAddressModel
                                {
                                    Name = order["NAME1"].GetString(),
                                    Email = order["SMTP_ADDR"].GetString(),
                                    PhoneNumber = order["TELF1"].GetString(),
                                    Address1 = order.GetStructure("ORD_ADDR")["HOUSE_NUM1"].GetString(),
                                    Address2 = order.GetStructure("ORD_ADDR")["STREET"].GetString(),
                                    Address3 = order.GetStructure("ORD_ADDR")["CITY2"].GetString(),
                                    City = order.GetStructure("ORD_ADDR")["CITY2"].GetString(),
                                    StateProvince = order.GetStructure("ORD_ADDR")["REGION"].GetString(),
                                    Region = order.GetStructure("ORD_ADDR")["REGION"].GetString(),
                                    ZipPostalCode = order.GetStructure("ORD_ADDR")["POST_CODE1"].GetString(),
                                    Country = order.GetStructure("ORD_ADDR")["LAND1"].GetString()
                                },

                                BillingAddress = new ErpAddressModel
                                {
                                    Name = order.GetStructure("CUST_ADDR")["NAME1"].GetString(),
                                    Email = order.GetStructure("CUST_ADDR")["SMTP_ADDR2"].GetString(),
                                    PhoneNumber = order.GetStructure("ORD_ADDR")["TEL_NUMBER"].GetString(),
                                    Address1 = order.GetStructure("CUST_ADDR")["HOUSE_NUM1"].GetString(),
                                    Address2 = order.GetStructure("CUST_ADDR")["STREET"].GetString(),
                                    ZipPostalCode = order.GetStructure("CUST_ADDR")["POST_CODE1"].GetString(),
                                    StateProvince = order.GetStructure("CUST_ADDR")["REGION"].GetString(),
                                    Country = order.GetStructure("ORD_ADDR")["LAND1"].GetString(),  // SAP does not have Land code on Billing Address
                                },

                                ErpPlaceOrderItemDatas = order
                                    .GetTable("ORD_ITEM")
                                    .Select(line =>
                                    {
                                        //return new
                                        //{
                                        //    ERPLineNumber = line["POSNR"].GetString(), // erp line number
                                        //    MATNR = line["MATNR"].GetString(), // sku or material number
                                        //    MATNR_CHARG = line["MATNR"].GetString() + line["CHARG"].GetString(),
                                        //    CHARG = line["CHARG"].GetString(),
                                        //    ARKTX = line["ARKTX"].GetString(), // item name
                                        //    KWMENG = line["KWMENG"].GetDecimal(), // Quantity
                                        //    GrossPrice = line["GRSPR"].GetDecimal(), // unit price without discount
                                        //    GrossPrice_INCVAT = line["GRSPR"].GetDecimal() * 1.15m, // unit price without discount including vat
                                        //    NETPR = line["NETPR"].GetDecimal(), // unit price after discount
                                        //    NETPR_INCVAT = line["NETPR"].GetDecimal() * 1.15m, // net price including vat
                                        //    KMEIN = line["KMEIN"].GetString(), // uom
                                        //    LINE_TOTAL = line["NETPR"].GetDecimal() * line["KWMENG"].GetDecimal(), // line total = unit price * quantity
                                        //    LINE_TOTAL_INCVAT = (line["NETPR"].GetDecimal() * line["KWMENG"].GetDecimal() * 1.15m) - cashRoundingValueOfLine, // line total including vat= unit price * quantity
                                        //    EDATU = edatu, //
                                        //    NTGEW1 = line["NTGEW1"].GetDecimal(), // item weight
                                        //    ABSTA = line["ABSTA"].GetString(), // status
                                        //    OrderStatus = line["ABSTA"].GetString() == "R" ? "Rejected" : "Ok", // formated status
                                        //    ITM_MSG = line["ITM_MSG"].GetString(),
                                        //    DISCOUNT = totalQuantityDiscount,
                                        //    DISCOUNT_INCVAT = totalQuantityDiscount_IncVat,
                                        //    UnitQuantityDiscount = unitDiscount,
                                        //    SpecInstruct = SpecInstruct,
                                        //    DeliveryDate = line["EDATU"].GetString(),
                                        //    WarehouseCode = line["WERKS"].GetString(),
                                        //};
                                        var cashRoundingValueOfLine = line["ROUNDING_OFF_I"]?.GetDecimal() ?? decimal.Zero;
                                        var unitDiscount = Math.Round(line["ECOM_DISC"].GetDecimal(), 2);
                                        var itemQuantity = line["KWMENG"].GetDecimal();
                                        var totalQuantityDiscount = unitDiscount * itemQuantity;
                                        var totalQuantityDiscount_IncVat = totalQuantityDiscount * (decimal)1.15;
                                        var specInstruct = "";

                                        if (orderType == "ZQCS" || orderType == "ZCSH")
                                        {
                                            var itemText = line.GetTable("ITEM_TEXT");
                                            if (itemText != null)
                                            {
                                                var text = itemText.GetTable("TEXT").Select((a) =>
                                                specInstruct = a["TDLINE"].GetString());
                                            }
                                        }

                                        if (line["MATNR"].GetString() == _sapIntegrationSettings.ShippingCostSKU)
                                        {
                                            shippingCost = line["NETPR"].GetDecimal();
                                        }

                                        return new ErpPlaceOrderItemDataModel
                                        {
                                            ERPLineNumber = line["POSNR"].GetString(), // erp line number
                                            Weight = line["NTGEW1"].GetDecimal(), // item weight
                                            ErpOrderLineNumber = line["POSNR"].GetString(), // erp line number
                                            ErpSalesUoM = line["KMEIN"].GetString(), // uom
                                            Sku = line["MATNR"].GetString(), // sku or material number
                                            Quantity = line["KWMENG"].GetDecimal(), // Quantity
                                            Description = line["ARKTX"].GetString(), // item name
                                            PriceExclTax = line["NETPR"].GetDecimal() * line["KWMENG"].GetDecimal(), // line total = unit price * quantity
                                            PriceInclTax = (line["NETPR"].GetDecimal() * line["KWMENG"].GetDecimal() * 1.15m) - cashRoundingValueOfLine, // line total including vat = unit price * quantity 
                                            SpecialInstruction = specInstruct,
                                            WarehouseCode = line["WERKS"].GetString(),
                                            DiscountAmountExclTax = totalQuantityDiscount,
                                            DiscountAmountInclTax = totalQuantityDiscount_IncVat,
                                            UnitPriceExclTax = line["NETPR"].GetDecimal(), // unit price after discount
                                            UnitPriceInclTax = line["NETPR"].GetDecimal() * 1.15m, // net price including vat
                                            UnitQuantityDiscount = unitDiscount,
                                            ErpOrderLineStatus = line["ABSTA"].GetString() == "R" ? "Rejected" : "Ok", // formated status
                                            GrossPrice = line["GRSPR"].GetDecimal(), // unit price without discount
                                            GrossPrice_INCVAT = line["GRSPR"].GetDecimal() * 1.15m, // unit price without discount including vat
                                        };
                                    })
                                    .ToList(),
                            };

                            erpResponseData.Data.Add(placeOrderDataModel);
                        }
                        catch (Exception ex)
                        {
                            var vbeln = "Unknown order number";
                            try
                            {
                                vbeln = order["VBELN"].GetString();
                            }
                            catch 
                            { 

                            }

                            var kunnr = "Unknown account number";

                            try
                            {
                                kunnr = order["KUNNR"].GetString();
                            }
                            catch 
                            { 

                            }

                            var errorMessage = $"Problem importing Order number '{vbeln}', Account '{kunnr}': {ex.Message}";
                            var detailedError = $"Message: {ex.Message}\nStackTrace: {ex.StackTrace}";

                            await _erpLogsService.InsertErpLogAsync(
                                ErpLogLevel.Error,
                                ErpSyncLevel.Order,
                                errorMessage,
                                detailedError
                            );
                            return null;
                        }
                    }

                    if (!erpResponseData.Data.Any())
                        erpResponseData.Data = null;

                    erpResponseData.ErpResponseModel.Next =
                        erpResponseData.Data.Count <= 0 ? 
                        null :
                        erpResponseData.Data.Last().ErpOrderNumber;
                }
                else
                {
                    erpResponseData.Data = null;
                    erpResponseData.ErpResponseModel = new ErpResponseModel { Next = null };
                }
            }
            catch (Exception ex)
            {
                await _erpLogsService.InsertErpLogAsync(
                    ErpLogLevel.Error,
                    ErpSyncLevel.Order,
                    $"GetOrders error (Account = {erpRequest.AccountNumber}, Location = {erpRequest.Location}, Start = {erpRequest.Start}, Limit = {erpRequest.Limit}, DateFrom ({erpRequest.DateFrom?.ToString("yyyy-MM-dd HH:mm:ss")}). Click view to see details.",
                    $"GetOrders error:\nMessage: {ex.Message}\nStackTrace: {ex.StackTrace}"
                );
                erpResponseData.ErpResponseModel.IsError = true;
                erpResponseData.ErpResponseModel.ErrorShortMessage = ex.Message;
            }

            return erpResponseData;
        }
        catch (Exception ex)
        {
            erpResponseData.ErpResponseModel.IsError = true;
            erpResponseData.ErpResponseModel.ErrorShortMessage = ex.Message;
            erpResponseData.ErpResponseModel.ErrorFullMessage = !string.IsNullOrEmpty(
                responseContent
            )
                ? responseContent
                : ex.StackTrace;
        }

        return erpResponseData;
    }

    #endregion
}
