using System.Globalization;
using System.Text;
using Newtonsoft.Json;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;
using SAP.Middleware.Connector;

namespace NopStation.Plugin.Misc.B2B.SapIntegration.Services;

public class B2BPricingService : IB2BPricingService
{
    private readonly IErpLogsService _erpLogsService;
    private readonly SapIntegrationSettings _sapIntegrationSettings;

    public B2BPricingService(IErpLogsService erpLogsService,
        SapIntegrationSettings sapIntegrationSettings)
    {
        _erpLogsService = erpLogsService;
        _sapIntegrationSettings = sapIntegrationSettings;
    }

    private async Task<ErpResponseData<IList<ErpPriceSpecialPricingDataModel>>> GetPricingWithItemNosAsync(ErpGetRequestModel erpRequest)
    {
        var erpResponseData = new ErpResponseData<IList<ErpPriceSpecialPricingDataModel>>()
        {
            Data = new List<ErpPriceSpecialPricingDataModel>()
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
            //2330
            if (!string.IsNullOrWhiteSpace(erpRequest.Location) && erpRequest.Location.Equals("1032"))
            {
                erpRequest.Location = "1030";
            }

            if (erpRequest.Limit == 0)
                erpRequest.Limit = 1000;
            try
            {
                if (!RfcDestinationManager.IsDestinationConfigurationRegistered())
                    RfcDestinationManager.RegisterDestinationConfiguration(new SapConfig(_sapIntegrationSettings));

                var dest = RfcDestinationManager.GetDestination("HUBCLIENT");
                var repo = dest.Repository;

                var productSkus = erpRequest.ProductSku.Split(',');
                var message = new StringBuilder();

                if (!string.IsNullOrEmpty(erpRequest.AccountNumber))
                {
                    message.AppendLine($"GetPricingWithItemNos Call: " +
                        $"Setting Acc No.: {erpRequest.AccountNumber}, " +
                        $"Location: {erpRequest.Location}" +
                        erpRequest.DateFrom != null && erpRequest.DateFrom != DateTime.MinValue ? 
                        $", DateFrom: {erpRequest.DateFrom.Value}" : "");
                }

                foreach (var itemNo in productSkus)
                {
                    var func = repo.CreateFunction("ZEC_PRICING_GETLIST");

                    if (!string.IsNullOrEmpty(erpRequest.AccountNumber))
                    {
                        var eT_KUNNR_SELT = func.GetTable("ET_KUNNR_SELT");
                        eT_KUNNR_SELT.Append();
                        eT_KUNNR_SELT.SetValue("SIGN", "I");
                        eT_KUNNR_SELT.SetValue("OPTION", "EQ");
                        eT_KUNNR_SELT.SetValue("LOW", erpRequest.AccountNumber);
                        message.AppendLine($"[DEBUG] ET_KUNNR_SELT: {eT_KUNNR_SELT}; Count: {eT_KUNNR_SELT.Count}");
                    }
                    if (!string.IsNullOrEmpty(erpRequest.Location))
                    {
                        var eT_VKORG_SELT = func.GetTable("ET_VKORG_SELT");
                        eT_VKORG_SELT.Append();
                        eT_VKORG_SELT.SetValue("SIGN", "I".ToString());
                        eT_VKORG_SELT.SetValue("OPTION", "EQ".ToString());
                        eT_VKORG_SELT.SetValue("LOW", erpRequest.Location.ToString());
                        message.AppendLine($"ET_VKORG_SELT: {eT_VKORG_SELT}; Count: {eT_VKORG_SELT.Count}");
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
                    eT_MATNR_SELT.SetValue("LOW", parts[0]);

                    if (parts.Length > 1)
                    {
                        charg = parts[1];
                        if (charg.StartsWith(parts[0]))
                            charg = charg.Substring(parts[0].Length);
                    }

                    var eT_CHARG_SELT = func.GetTable("ET_CHARG_SELT");
                    eT_CHARG_SELT.Append();
                    eT_CHARG_SELT.SetValue("SIGN", "I");
                    eT_CHARG_SELT.SetValue("OPTION", "EQ");
                    eT_CHARG_SELT.SetValue("LOW", charg);

                    if (erpRequest.DateFrom != null && erpRequest.DateFrom != DateTime.MinValue)
                    {
                        func.SetValue("IV_CHANGED", erpRequest.DateFrom.Value);
                    }

                    func.SetValue("IV_ROWS", 1);

                    await _erpLogsService.InsertErpLogAsync(
                        ErpLogLevel.Debug,
                        ErpSyncLevel.SpecialPrice,
                        $"GetPricingWithItemNos before BAPI call (Account: {erpRequest.AccountNumber}). Click view to see details.",
                        $"{message}\n\n Before GetPricingWithItemNos call to BAPI: {func}"
                    );
                    message.Clear();

                    func.Invoke(dest);

                    await _erpLogsService.InsertErpLogAsync(
                        ErpLogLevel.Debug,
                        ErpSyncLevel.SpecialPrice,
                        $"GetPricingWithItemNos after BAPI call (Account: {erpRequest.AccountNumber})",
                        $"After GetPricingWithItemNos call to BAPI: {func}"
                    );

                    var price = func.GetTable("ES_PRICING")
                        ?.Select(product => new ErpPriceSpecialPricingDataModel
                        {
                            AccountNumber = product["KUNNR"]?.GetString() ?? string.Empty,
                            Sku = product["MATNR"]?.GetString() ?? string.Empty,
                            SpecialPrice = decimal.TryParse(
                                product["ZEC_KBETR"]?.GetString()?.Replace(',', '.'),
                                NumberStyles.Any,
                                CultureInfo.InvariantCulture,
                                out var specialPrice)
                                ? specialPrice
                                : 0, // Default value if conversion fails
                            Branch = product["VKORG"].GetString(),
                            DiscountPercentage = product["ZEC_ZTDE_PERC"]?.GetDecimal() ?? 0,
                            PricingNotes = $"R {product["ZEC_KBETR_OR"]?.GetDecimal() ?? 0} / " +
                                $"{product["ZEC_KMEIN_OR"]?.GetString() ?? string.Empty}",
                        })
                        .FirstOrDefault();

                    if (price != null)
                    {
                        erpResponseData.Data.Add(price);
                    }
                }
                erpResponseData.ErpResponseModel.Next = null;

                await _erpLogsService.InsertErpLogAsync(
                    ErpLogLevel.Debug,
                    ErpSyncLevel.SpecialPrice,
                    $"GetPricingWithItemNos BAPI call (Account: {erpRequest.AccountNumber}) result. Click view to see details.",
                    $"GetPricingWithItemNos result: {JsonConvert.SerializeObject(erpResponseData.Data)}"
                );
            }
            catch (Exception ex)
            {
                await _erpLogsService.InsertErpLogAsync(
                    ErpLogLevel.Error,
                    ErpSyncLevel.SpecialPrice,
                    $"GetPricingWithItemNos error (Account: {erpRequest.AccountNumber}). Click view to see details.",
                    $"GetPricingWithItemNos error: \nException message: {ex.Message}\nStack trace: {ex.StackTrace}"
                );
                erpResponseData.ErpResponseModel.IsError = true;
                erpResponseData.ErpResponseModel.ErrorShortMessage = ex.Message;
                return erpResponseData;
            }

            return erpResponseData;
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

    public async Task<ErpResponseData<IList<ErpPriceSpecialPricingDataModel>>> GetPerAccountProductPricingFromErpAsync(ErpGetRequestModel erpRequest)
    {
        var erpResponseData = new ErpResponseData<IList<ErpPriceSpecialPricingDataModel>>();
        var responseContent = string.Empty;

        try
        {
            if (erpRequest == null)
            {
                erpResponseData.ErpResponseModel.IsError = false;
                erpResponseData.ErpResponseModel.ErrorShortMessage = "Request body content no data";
                return erpResponseData;
            }

            if (!string.IsNullOrEmpty(erpRequest.ProductSku))
            {
                return await GetPricingWithItemNosAsync(erpRequest);
            }

            //2330
            if (
                !string.IsNullOrWhiteSpace(erpRequest.Location)
                && erpRequest.Location.Equals("1032")
            )
                erpRequest.Location = "1030";

            if (erpRequest.Limit == 0)
                erpRequest.Limit = 1000;

            if (erpRequest.Start == "0")
                erpRequest.Start = string.Empty;

            try
            {
                if (!RfcDestinationManager.IsDestinationConfigurationRegistered())
                    RfcDestinationManager.RegisterDestinationConfiguration(new SapConfig(_sapIntegrationSettings));

                var dest = RfcDestinationManager.GetDestination("HUBCLIENT");
                var repo = dest.Repository;
                var func = repo.CreateFunction("ZEC_PRICING_GETLIST");

                var message = new StringBuilder();

                if (!string.IsNullOrEmpty(erpRequest.AccountNumber))
                {
                    var eT_KUNNR_SELT = func.GetTable("ET_KUNNR_SELT");
                    eT_KUNNR_SELT.Append();
                    eT_KUNNR_SELT.SetValue("SIGN", "I");
                    eT_KUNNR_SELT.SetValue("OPTION", "EQ");
                    eT_KUNNR_SELT.SetValue("LOW", erpRequest.AccountNumber);

                    message.AppendLine($"ET_KUNNR_SELT: {eT_KUNNR_SELT}; Count: {eT_KUNNR_SELT.Count}");
                }
                if (!string.IsNullOrEmpty(erpRequest.Location))
                {
                    var eT_VKORG_SELT = func.GetTable("ET_VKORG_SELT");
                    eT_VKORG_SELT.Append();
                    eT_VKORG_SELT.SetValue("SIGN", "I");
                    eT_VKORG_SELT.SetValue("OPTION", "EQ");
                    eT_VKORG_SELT.SetValue("LOW", erpRequest.Location);

                    message.AppendLine($"Setting location: {erpRequest.Location}");
                    message.AppendLine($"ET_VKORG_SELT: {eT_VKORG_SELT}; Count: {eT_VKORG_SELT.Count}");
                }

                if (erpRequest.DateFrom != null && erpRequest.DateFrom != DateTime.MinValue)
                {
                    func.SetValue("IV_CHANGED", erpRequest.DateFrom.Value);
                    message.AppendLine($"DateFrom: {erpRequest.DateFrom.Value}");
                }
                if (!string.IsNullOrEmpty(erpRequest.Start))
                {
                    var startParts = erpRequest.Start.Split(',');
                    var iV_PRIC_STRT = func.GetStructure("IV_PRIC_STRT");
                    iV_PRIC_STRT.SetValue(
                        "MATNR",
                        (startParts.Length > 0) ? startParts[0] : string.Empty
                    );
                    iV_PRIC_STRT.SetValue(
                        "CHARG",
                        (startParts.Length > 1) ? startParts[1] : string.Empty
                    );
                    iV_PRIC_STRT.SetValue("KUNNR", erpRequest.AccountNumber);
                    iV_PRIC_STRT.SetValue("VKORG", erpRequest.Location);
                    message.AppendLine($"Setting IV_PRIC_STRT: {iV_PRIC_STRT}");
                }
                func.SetValue("IV_ROWS", erpRequest.Limit);
                message.AppendLine($"Setting IV_ROWS: {erpRequest.Limit}");

                await _erpLogsService.InsertErpLogAsync(
                    ErpLogLevel.Debug,
                    ErpSyncLevel.SpecialPrice,
                    $"GetPricing before BAPI call (Account: {erpRequest.AccountNumber}). Click view to see details.",
                    $"GetPricing before BAPI call: \n{message}\n\nRequest: {JsonConvert.SerializeObject(erpRequest)}\n\nFunc: {func}"
                );

                func.Invoke(dest);

                var prices = func.GetTable("ES_PRICING");

                erpResponseData.Data = prices
                    .Select(product => new ErpPriceSpecialPricingDataModel
                    {
                        AccountNumber = product["KUNNR"]?.GetString() ?? string.Empty,
                        Sku = product["MATNR"]?.GetString() ?? string.Empty,
                        SpecialPrice = decimal.TryParse(
                               product["ZEC_KBETR"]?.GetString()?.Replace(',', '.'),
                               NumberStyles.Any,
                               CultureInfo.InvariantCulture,
                               out var specialPrice)
                               ? specialPrice
                               : 0, // Default value if conversion fails
                        Branch = product["VKORG"]?.GetString() ?? string.Empty,
                        DiscountPercentage = product["ZEC_ZTDE_PERC"]?.GetDecimal() ?? 0,
                        PricingNotes =
                            $"R {product["ZEC_KBETR_OR"]?.GetDecimal() ?? 0} / {product["ZEC_KMEIN_OR"]?.GetString() ?? string.Empty}",
                        //MATNR_CHARG = $"{product["MATNR"]?.GetString() ?? string.Empty}{product["CHARG"]?.GetString() ?? string.Empty}",
                        //ZEC_KBETR = product["ZEC_KBETR"]?.GetString() ?? string.Empty,
                        //CHARG = product["CHARG"]?.GetString() ?? string.Empty,
                        //ZEC_KBETR_OR = product["ZEC_KBETR_OR"]?.GetString() ?? string.Empty,
                        //ZEC_KMEIN_OR = product["ZEC_KMEIN_OR"]?.GetString() ?? string.Empty,
                        //KDMAT = product["KDMAT"]?.GetString() ?? string.Empty,
                    })
                    .ToList();

                var eV_PRIC_LAST = func.GetStructure("EV_PRIC_LAST");

                if (!erpResponseData.Data.Any())
                {
                    erpResponseData.ErpResponseModel.Next = null;
                    erpResponseData.Data = null;
                }
                else
                {
                    var last = prices.Last();
                    var matnr = last["MATNR"].GetString();
                    var charg = last["CHARG"].GetString();
                    erpResponseData.ErpResponseModel.Next = matnr;
                    if (!string.IsNullOrEmpty(charg))
                    {
                        erpResponseData.ErpResponseModel.Next += "," + charg;
                    }
                }

                await _erpLogsService.InsertErpLogAsync(
                    ErpLogLevel.Debug,
                    ErpSyncLevel.SpecialPrice,
                    $"GetPricing after BAPI call (Account: {erpRequest.AccountNumber}). Click view to see details.",
                    $"GetPricing after BAPI call: " +
                    $"\n\nRequest: {message} " +
                    $"\n\nResponse: {JsonConvert.SerializeObject(erpResponseData.Data)} " +
                    $"\n\nNext:{erpResponseData.ErpResponseModel.Next} " +
                    $"\n\nFunc: {func}"
                );
            }
            catch (Exception ex)
            {
                await _erpLogsService.InsertErpLogAsync(
                    ErpLogLevel.Error,
                    ErpSyncLevel.SpecialPrice,
                    $"GetPricing error (Account: {erpRequest.AccountNumber}). Click view to see details.",
                    $"GetPricing error: \nException message: {ex.Message}\nStack trace: {ex.StackTrace}"
                );
                erpResponseData.ErpResponseModel.IsError = true;
                erpResponseData.ErpResponseModel.ErrorShortMessage = ex.Message;
                return erpResponseData;
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

    public async Task<ErpResponseData<IList<ErpPriceGroupPricingDataModel>>> GetProductGroupPricingFromErpAsync(ErpGetRequestModel erpRequest)
    {
        throw new NotImplementedException();
    }
}
