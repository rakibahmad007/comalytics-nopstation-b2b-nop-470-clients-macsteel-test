using System.Text;
using Newtonsoft.Json.Linq;
using Nop.Core.Infrastructure;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model.Common;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;
using SAP.Middleware.Connector;

namespace NopStation.Plugin.Misc.B2B.SapIntegration.Services;

public class B2BProductService : IB2BProductService
{
    private readonly INopFileProvider _fileProvider;
    private readonly IErpLogsService _erpLogsService;
    private readonly SapIntegrationSettings _sapIntegrationSettings;

    public B2BProductService(INopFileProvider fileProvider,
        IErpLogsService erpLogsService,
        SapIntegrationSettings sapIntegrationSettings)
    {
        _fileProvider = fileProvider;
        _erpLogsService = erpLogsService;
        _sapIntegrationSettings = sapIntegrationSettings;
    }

    public List<ErpCategoryDataModel> BuildCategoriesForProduct(IRfcStructure product)
    {
        var categories = new List<ErpCategoryDataModel>();

        var filePath = _fileProvider.MapPath("~/Plugins/Misc.B2B.SapIntegration/sapcategories.json");

        var text = _fileProvider.FileExists(filePath) ? _fileProvider.ReadAllText(filePath, Encoding.UTF8) : string.Empty;
        if (string.IsNullOrEmpty(text))
            return new List<ErpCategoryDataModel>();

        var configuration = JArray.Parse(text);

        foreach (JObject config in configuration)
        {
            if (config.GetValue("codes").ToString().Split(',').ToList().Contains(product["EXTWG"].GetString()))
            {
                // Process levels
                foreach (JObject level in config.GetValue("levels"))
                {
                    switch (level.GetValue("type").ToString())
                    {
                        case "FIXED":
                            categories.Add(new ErpCategoryDataModel
                            {
                                CategoryName = level.GetValue("value").ToString(),
                                Description = level.GetValue("value").ToString()
                            });
                            break;
                        case "FIELD":
                            categories.Add(new ErpCategoryDataModel
                            {
                                CategoryName = product[level.GetValue("value").ToString()].GetString(),
                                Description = product[level.GetValue("value").ToString()].GetString(),
                            });
                            break;
                        default:
                            break;
                    }
                }
            }
        }
        return categories;
    }

    public decimal ParseDecimal(string str)
    {
        _ = decimal.TryParse(str, out var dec);
        return dec;
    }

    public async Task<IList<dynamic>> SAPGetStock(string location, string start, int limit, DateTime? dateFrom = null, IEnumerable<string> itemNos = null)
    {
        if (!RfcDestinationManager.IsDestinationConfigurationRegistered())
            RfcDestinationManager.RegisterDestinationConfiguration(new SapConfig(_sapIntegrationSettings));

        var dest = RfcDestinationManager.GetDestination("HUBCLIENT");
        var repo = dest.Repository;
        var func = repo.CreateFunction("ZEC_STOCK_GETLIST");

        await _erpLogsService.InsertErpLogAsync(
            ErpLogLevel.Debug,
            ErpSyncLevel.Product,
            $"SAPGetStock: Function created (Location = {location}, Start = {start}, Limit = {limit})"
        );

        // We're getting a sales org rather than a warehouse in request.location, so we can't filter by that.
        // So what we do is filter client-wise with a hack specific for Macsteel:
        // the warehouse code should have the same first three characters, out of four
        //if( !String.IsNullOrEmpty(location) )
        //{
        //	IRfcTable ET_VKORG_SELT = func.GetTable("ET_PLANT_SELT");
        //	ET_VKORG_SELT.Append();
        //	ET_VKORG_SELT.SetValue("SIGN", "I");
        //	ET_VKORG_SELT.SetValue("OPTION", "EQ");
        //	ET_VKORG_SELT.SetValue("LOW", location);
        //}

        if (itemNos != null && itemNos.Any())
        {
            var eT_MATNR_SELT = func.GetTable("ET_MATNR_SELT");
            foreach (var itemNo in itemNos)
            {
                eT_MATNR_SELT.Append();
                eT_MATNR_SELT.SetValue("SIGN", "I");
                eT_MATNR_SELT.SetValue("OPTION", "EQ");
                eT_MATNR_SELT.SetValue("LOW", itemNo);
            }
        }

        if (dateFrom != null && dateFrom != DateTime.MinValue)
        {
            func.SetValue("IV_TIME", dateFrom.Value.ToString("yyyyMMddHHmmss"));
        }

        if (!string.IsNullOrEmpty(start))
            func.SetValue("IV_STOCK_START", start);

        if (limit > 0 && string.IsNullOrEmpty(location))
        { 
            // Don't limit if we want to filter by location
            func.SetValue("IV_ROWS", limit);
        }
        else
        {
            func.SetValue("IV_ROWS", 10000);
        }

        await _erpLogsService.InsertErpLogAsync(
            ErpLogLevel.Debug,
            ErpSyncLevel.Product,
            $"SAPGetStock: Before call to BAPI (Location = {location}, Start = {start}, Limit = {limit}), click view to see details.",
            $"SAPGetStock: Before call to BAPI (Location = {location}, Start = {start}, Limit = {limit}): \n\n{func}"
        );

        func.Invoke(dest);

        await _erpLogsService.InsertErpLogAsync(
            ErpLogLevel.Debug,
            ErpSyncLevel.Product,
            $"SAPGetStock: After call to BAPI (Location = {location}, Start = {start}, Limit = {limit}), click view to see details.",
            $"SAPGetStock: After call to BAPI (Location = {location}, Start = {start}, Limit = {limit}): \n\n{func}"
        );

        var stockItems = func.GetTable("ES_STOCKS").AsEnumerable();

        if (!string.IsNullOrEmpty(location))
        {
            stockItems = func.GetTable("ES_STOCKS")
            .Where(item => item["WERKS"].GetString()?[..3] == location[..3]);
        }

        if (!stockItems.Any())
            return new List<dynamic>();

        return stockItems.Select((stockItem) =>
        {
            return new
            {
                MATNR = stockItem["MATNR"].GetString(),
                WERKS = stockItem["WERKS"].GetString(),
                QTY = stockItem["QTY"].GetDecimal(),
                QTYPLT = stockItem["QTYPLT"].GetDecimal(),
                CHARG = stockItem["CHARG"].GetString(),
                ARKTX = stockItem["ARKTX"].GetString(),
                UOM = stockItem["UOM"].GetString(),
                MASS = stockItem["MASS"].GetDecimal(),
                UPDATED = stockItem["UPDATED"].GetString()
            };
        })?.Cast<dynamic>()?.ToList();
    }

    public async Task<ErpResponseData<IList<ErpProductDataModel>>> GetProductsFromErpAsync(ErpGetRequestModel erpRequest)
    {
        var erpResponseData = new ErpResponseData<IList<ErpProductDataModel>>();
        var responseContent = string.Empty;

        try
        {
            if (erpRequest == null)
            {
                erpResponseData.ErpResponseModel.IsError = false;
                erpResponseData.ErpResponseModel.ErrorShortMessage = "Request body contains no data";
                return erpResponseData;
            }

            #region SAP

            var locationChanged = false;
            if (!string.IsNullOrWhiteSpace(erpRequest.Location) && erpRequest.Location.Equals("1032"))
            {
                erpRequest.Location = "1030";
                locationChanged = true;
            }

            if (erpRequest.Limit == 0)
                erpRequest.Limit = 1000;

            // Get Products
            IEnumerable<ErpProductDataModel> products = null;
            var eV_MAT_LAST = "";

            try
            {
                if (!RfcDestinationManager.IsDestinationConfigurationRegistered())
                    RfcDestinationManager.RegisterDestinationConfiguration(new SapConfig(_sapIntegrationSettings));
                var dest = RfcDestinationManager.GetDestination("HUBCLIENT");
                var repo = dest.Repository;
                var func = repo.CreateFunction("ZEC_MATERIAL_GETLIST");

                if (!string.IsNullOrEmpty(erpRequest.Location))
                {
                    var eT_VKORG_SELT = func.GetTable("ET_VKORG_SELT");
                    eT_VKORG_SELT.Append();
                    eT_VKORG_SELT.SetValue("SIGN", "I");
                    eT_VKORG_SELT.SetValue("OPTION", "EQ");
                    eT_VKORG_SELT.SetValue("LOW", erpRequest.Location); // Location == sales org (unlike in GetStock)               
                }
                if (erpRequest.DateFrom != null && erpRequest.DateFrom != DateTime.MinValue)
                {
                    func.SetValue("IV_CHANGED", erpRequest.DateFrom.Value);
                }
                func.SetValue("IV_MAT_START", erpRequest.Start);
                func.SetValue("IV_ROWS", erpRequest.Limit);

                await _erpLogsService.InsertErpLogAsync(
                    ErpLogLevel.Debug,
                    ErpSyncLevel.Product,
                    $"GetProducts before BAPI call (Location = {erpRequest.Location}, Start = {erpRequest.Start}, Limit = {erpRequest.Limit}, DateFrom = {erpRequest.DateFrom}). Click view to see details.",
                    $"GetProducts before BAPI call: {func}"
                );

                func.Invoke(dest);

                await _erpLogsService.InsertErpLogAsync(
                    ErpLogLevel.Debug,
                    ErpSyncLevel.Product,
                    $"GetProducts after BAPI call (Location = {erpRequest.Location}, Start = {erpRequest.Start}, Limit = {erpRequest.Limit}, DateFrom = {erpRequest.DateFrom}). Click view to see details.",
                    $"GetProducts after BAPI call: {func}"
                );

                var sapProducts = func.GetTable("ES_MATERIALS");
                eV_MAT_LAST = func.GetString("EV_MAT_LAST");
                products = sapProducts.Select((product) =>
                {
                    var categories = BuildCategoriesForProduct(product);
                    if (categories.Count >= 3 && "FLANGES".Equals(categories[0].CategoryName, StringComparison.OrdinalIgnoreCase))
                    {
                        if ((product["EXTWG2"].GetString()?.Length ?? 0) >= 4)
                        {
                            categories[2] = new ErpCategoryDataModel 
                            { 
                                CategoryName = product["EWBEZ2"].GetString(), 
                                Description = product["EWBEZ2"].GetString() 
                            };
                        }
                        else
                        {
                            categories[2] = new ErpCategoryDataModel 
                            { 
                                CategoryName = product["MAT_PH_CLASS_T"].GetString(), 
                                Description = product["MAT_PH_CLASS_T"].GetString() 
                            };
                        }
                    }

                    return new ErpProductDataModel
                    {
                        Sku = product["MATNR"].GetString(),
                        Name = product["MAT_LONG_DESC"].GetString(),
                        ManufacturerPartNumber = product["MATNR"].GetString(),
                        Location = locationChanged ? "1032" : product["VKORG"].GetString(),
                        FullDescription = product["MAT_LONG_DESC"].GetString(),
                        Size = product["MAT_SIZE"].GetString(),
                        Color = product["MAT_COLOR"].GetString(),
                        Height = ParseDecimal(product["MAT_HEIGHT"].GetString()),
                        Width = ParseDecimal(product["MAT_WIDTH"].GetString()),
                        Length = ParseDecimal(product["MAT_LENGTH"].GetString()),
                        Weight = 0,
                        ShortDescription = product["MAT_SHORT_DESC"].GetString(),
                        IsSpecial = product["ZESPEC"].GetString() == "X",
                        ProductCategories = categories,
                        ProductAttributes = new List<KeyValuePair<string, string>>
                        {
                            new ("Size", product["MAT_SIZE"].GetString()),
                            new ("Color", product["MAT_COLOR"].GetString()),
                            new ("Grade", product["MAT_PH_GRADE_T"].GetString()),
                            new ("Thickness", product["MAT_HEIGHT"].GetString()),
                            new ("UOM", product["VRKME"].GetString()),
                            new ("Length", product["MAT_LENGTH"].GetString()),
                            new ("Face Finish", product["MS_FACE_FIN"].GetString()),
                            new ("Other", product["MS_DESCRIP"].GetString()),
                            new ("Nominal bore", product["MS_NOMBORE"].GetString()),
                            new ("Flute NF/BF", product["MS_FLUTE"].GetString()),
                            new ("Schedule/Thickness", product["MS_SHEDTHK"].GetString()),
                        },
                        VendorName = _sapIntegrationSettings.VendorName ?? "",
                        Weblist = true,
                        Active = true,
                        Published = true,
                        XCHPF = product["XCHPF"].GetString(),
                        //unused prop
                        //Thickness = product["MAT_HEIGHT"].GetString(),
                        //Length = product["MAT_LENGTH"].GetString(),
                        //MAT_PH_CAT = product["MAT_PH_CAT"].GetString(),
                        //MAT_PH_SUB_CAT = product["MAT_PH_SUB_CAT"].GetString(),
                        //MAT_PH_MAJOR = product["MAT_PH_MAJOR"].GetString(),
                        //MAT_PH_MAJOR_T = product["MAT_PH_MAJOR_T"].GetString(),
                        //MAT_PH_MINOR = product["MAT_PH_MINOR"].GetString(),
                        //MAT_PH_MINOR_T = product["MAT_PH_MINOR_T"].GetString(),
                        //MAT_PH_CLASS = product["MAT_PH_CLASS"].GetString(),
                        //MAT_PH_CLASS_T = product["MAT_PH_CLASS_T"].GetString(),
                        //MAT_PH_GRADE = product["MAT_PH_GRADE"].GetString(),
                        //MAT_PH_GRADE_T = product["MAT_PH_GRADE_T"].GetString(),
                        //EWBEZ = product["EWBEZ"].GetString(),
                        //ZESPEC = product["ZESPEC"].GetString(),
                        //EXTWG = product["EXTWG"].GetString(),
                        //EXTWG2 = product["EXTWG2"].GetString(),
                        //MAT_PH_CAT_T = product["MAT_PH_CAT_T"].GetString(),
                        //MAT_PH_SUB_CAT_T = product["MAT_PH_SUB_CAT_T"].GetString(),
                        //VRKME = product["VRKME"].GetString(),
                        //VTWEG = product["VTWEG"].GetString(),
                    };
                });
            }
            catch (Exception ex)
            {
                await _erpLogsService.InsertErpLogAsync(
                    ErpLogLevel.Error,
                    ErpSyncLevel.Product,
                    $"Call GetProducts error (Location = {erpRequest.Location}, Start = {erpRequest.Start}, Limit = {erpRequest.Limit}). Click view to see details.",
                    $"Call GetProducts error: \nException message: {ex.Message}\nStack trace: {ex.StackTrace}"
                );
                erpResponseData.ErpResponseModel.IsError = true;
                erpResponseData.ErpResponseModel.ErrorShortMessage = ex.Message;
                return erpResponseData;
            }

            try
            {
                var batchItemsToAdd = new Dictionary<string, ErpProductDataModel>();

                // select itemNos for batch items
                var itemNos = products.Where((prod) => prod.XCHPF == "X").Select((prod) => prod.Sku);
                if (itemNos.Any())
                {
                    var batchItems = await SAPGetStock(erpRequest.Location, "", 0, erpRequest.DateFrom, itemNos);
                    await _erpLogsService.InsertErpLogAsync(
                        ErpLogLevel.Debug,
                        ErpSyncLevel.Product,
                        $"SAPGetStock downloaded total {batchItems.Count} batch items. Click view to see details.", 
                        $"SAPGetStock downloaded the following {batchItems.Count} batch items: \n\n" + 
                        $"{string.Join("; ", batchItems.Select(p => $"{{ MATNR={p.MATNR}, CHARG={p.CHARG}, WERKS={p.WERKS} }}\n"))}"
                    );

                    var notFoundProducts = new List<string>();

                    foreach (var batchItem in batchItems)
                    {
                        var mainProduct = products.FirstOrDefault(prod => prod.Sku == batchItem.MATNR);
                        if (mainProduct == null)
                        {
                            if (!notFoundProducts.Contains(batchItem.MATNR))
                            {
                                notFoundProducts.Add(batchItem.MATNR);
                            }
                        }
                        var matnr = batchItem.MATNR + batchItem.CHARG;
                        if (batchItemsToAdd.ContainsKey(matnr))
                        {
                            continue; // Don't add duplicates
                        }

                        var batchProduct = new ErpProductDataModel
                        {
                            Sku = matnr,
                            ManufacturerPartNumber = batchItem.MATNR,
                            FullDescription = batchItem.ARKTX,
                            UnitOfMeasure = batchItem.UOM,
                            Weight = batchItem.MASS,
                            Location = mainProduct.Location,
                            Size = mainProduct.Size,
                            Color = mainProduct.Color,
                            Height = mainProduct.Height,
                            Width = mainProduct.Width,
                            Length = mainProduct.Length,
                            ShortDescription = mainProduct.ShortDescription,
                            IsSpecial = mainProduct.IsSpecial,
                            ProductCategories = mainProduct.ProductCategories,
                            ProductAttributes = mainProduct.ProductAttributes,
                            VendorName = mainProduct.VendorName,
                            Active = mainProduct.Active,
                            Weblist = mainProduct.Weblist,
                            Published = mainProduct.Weblist && mainProduct.Active,
                            //VARIANT_CODE = batchItem.CHARG,
                            //mainProduct.VTWEG,
                            //mainProduct.MAT_PH_CAT_T,
                            //mainProduct.MAT_PH_SUB_CAT_T,
                            //mainProduct.VRKME,
                            //mainProduct.MAT_PH_CAT,
                            //mainProduct.MAT_PH_SUB_CAT,
                            //mainProduct.MAT_PH_MAJOR,
                            //mainProduct.MAT_PH_MAJOR_T,
                            //mainProduct.MAT_PH_MINOR,
                            //mainProduct.MAT_PH_MINOR_T,
                            //mainProduct.MAT_PH_CLASS,
                            //mainProduct.MAT_PH_CLASS_T,
                            //mainProduct.MAT_PH_GRADE,
                            //mainProduct.MAT_PH_GRADE_T,
                            //mainProduct.EWBEZ,
                            //mainProduct.ZESPEC,
                            //mainProduct.EXTWG2,
                            XCHPF = mainProduct.XCHPF,
                        };
                        batchItemsToAdd.Add(matnr, batchProduct);
                    }

                    if (notFoundProducts.Count != 0)
                    {
                        await _erpLogsService.InsertErpLogAsync(
                            ErpLogLevel.Debug,
                            ErpSyncLevel.Product,
                            $"SAPGetStock: {notFoundProducts.Count} products with batch items were not found in product list. Click view to see details.",
                            $"SAPGetStock: The following {notFoundProducts.Count} products with batch items were not found in product list: " +
                            string.Join(", ", notFoundProducts)
                        );
                    }
                }
                var finalProducts = batchItemsToAdd.Values.ToList();
                // We only add the products after the batchitems to make sure paging works!
                finalProducts.AddRange(products.Where((prd) => prd.XCHPF == ""));

                // Unnecessary for the import process, but helps keep sanity when debugging
                finalProducts.Sort((p1, p2) => StringComparer.OrdinalIgnoreCase.Compare(p1.Sku, p2.Sku));

                if (finalProducts != null && finalProducts.Count > 0)
                {
                    erpResponseData.Data = finalProducts;
                    erpResponseData.ErpResponseModel = new ErpResponseModel
                    {
                        Next = eV_MAT_LAST == "" ? null : eV_MAT_LAST
                    };
                }
                else
                {
                    erpResponseData.Data = null;
                    erpResponseData.ErpResponseModel = new ErpResponseModel
                    {
                        Next = null,
                    };
                }
            }
            catch (Exception ex)
            {
                await _erpLogsService.InsertErpLogAsync(
                    ErpLogLevel.Error,
                    ErpSyncLevel.Product,
                    $"Call GetProducts error (Location = {erpRequest.Location}, Start = {erpRequest.Start}, Limit = {erpRequest.Limit}). Click view to see details.",
                    $"Call GetProducts error: \nException message: {ex.Message}\nStack trace: {ex.StackTrace}"
                );
                erpResponseData.ErpResponseModel.IsError = true;
                erpResponseData.ErpResponseModel.ErrorShortMessage = ex.Message;
            }

            return erpResponseData;

            #endregion

        }
        catch (Exception ex)
        {
            erpResponseData.ErpResponseModel.IsError = true;
            erpResponseData.ErpResponseModel.ErrorShortMessage = ex.Message;
            erpResponseData.ErpResponseModel.ErrorFullMessage = !string.IsNullOrEmpty(responseContent) ? responseContent : ex.StackTrace;
        }

        return erpResponseData;
    }

    public async Task<ErpResponseData<ErpProductImageDataModel>> GetProductImageFromErpAsync(ErpGetRequestModel erpRequest)
    {
        var erpResponseData = new ErpResponseData<ErpProductImageDataModel>
        {
            Data = new ErpProductImageDataModel()
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

            #region SAP

            try
            {
                if (!RfcDestinationManager.IsDestinationConfigurationRegistered())
                    RfcDestinationManager.RegisterDestinationConfiguration(new SapConfig(_sapIntegrationSettings));

                var dest = RfcDestinationManager.GetDestination("HUBCLIENT");
                var repo = dest.Repository;
                var func = repo.CreateFunction("ZEC_MAT_IMAGE_GETLIST");

                if (!string.IsNullOrEmpty(erpRequest.ProductSku))
                {
                    var eT_MATNR_SELT = func.GetTable("ET_MATNR_SELT");
                    eT_MATNR_SELT.Append();
                    eT_MATNR_SELT.SetValue("SIGN", "I");
                    eT_MATNR_SELT.SetValue("OPTION", "EQ");
                    eT_MATNR_SELT.SetValue("LOW", erpRequest.ProductSku);
                }
                if (erpRequest.DateFrom != null && erpRequest.DateFrom != DateTime.MinValue)
                {
                    func.SetValue("IV_CHANGED", erpRequest.DateFrom.Value);
                }
                func.SetValue("IV_MAT_START", erpRequest.ProductSku);
                func.SetValue("IV_ROWS", erpRequest.Limit);

                await _erpLogsService.InsertErpLogAsync(
                    ErpLogLevel.Debug,
                    ErpSyncLevel.Product,
                    $"GetProductImage before call to BAPI (Product = {erpRequest.ProductSku}, Limit = {erpRequest.Limit}). Click view to see details.",
                    $"GetProductImage before call to BAPI: {func}"
                );

                func.Invoke(dest);

                await _erpLogsService.InsertErpLogAsync(
                    ErpLogLevel.Debug,
                    ErpSyncLevel.Product,
                    $"GetProductImage after call to BAPI (Product = {erpRequest.ProductSku}, Limit = {erpRequest.Limit}). Click view to see details.",
                    $"GetProductImage after call to BAPI: {func}"
                );

                var product = func.GetTable("ES_MATERIALS")?.FirstOrDefault();
                byte[] imageData = null;
                if (product != null)
                {
                    var imageTab = product["MAT_IMAGE_TAB"]?.GetTable();

                    if (imageTab != null)
                    {
                        imageData = imageTab.SelectMany(img => img["LINE"]?.GetByteArray())?.ToArray();
                    }
                    erpResponseData.Data.Sku = product["MATNR"]?.GetString();
                    erpResponseData.Data.ImageData = imageData;

                    return erpResponseData;

                }
                else
                {
                    erpResponseData.ErpResponseModel.IsError = true;
                    erpResponseData.ErpResponseModel.ErrorShortMessage = "No image data";
                }
            }
            catch (Exception ex)
            {
                await _erpLogsService.InsertErpLogAsync(
                    ErpLogLevel.Error,
                    ErpSyncLevel.Product,
                    $"GetProductImage error for (Product = {erpRequest.ProductSku}, Limit = {erpRequest.Limit}). Click view to see details.",
                    $"GetProductImage error: \nException message: {ex.Message}\nStack trace: {ex.StackTrace}"
                );
                erpResponseData.ErpResponseModel.IsError = true;
                erpResponseData.ErpResponseModel.ErrorShortMessage = ex.Message;
            }
            return erpResponseData;

            #endregion

        }
        catch (Exception ex)
        {
            erpResponseData.ErpResponseModel.IsError = true;
            erpResponseData.ErpResponseModel.ErrorShortMessage = ex.Message;
            erpResponseData.ErpResponseModel.ErrorFullMessage = !string.IsNullOrEmpty(responseContent) ? responseContent : ex.StackTrace;
        }

        return erpResponseData;

    }

    public async Task<ErpResponseData<IList<ErpProductImageDataModel>>> GetProductImagesFromErpAsync(ErpGetRequestModel erpRequest)
    {
        var erpResponseData = new ErpResponseData<IList<ErpProductImageDataModel>>
        {
            Data = new List<ErpProductImageDataModel>()
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

            #region SAP

            if (erpRequest.Limit == 0)
                erpRequest.Limit = 10;
            try
            {
                if (!RfcDestinationManager.IsDestinationConfigurationRegistered())
                    RfcDestinationManager.RegisterDestinationConfiguration(new SapConfig(_sapIntegrationSettings));

                var dest = RfcDestinationManager.GetDestination("HUBCLIENT");
                var repo = dest.Repository;
                var func = repo.CreateFunction("ZEC_MAT_IMAGE_GETLIST");

                if (!string.IsNullOrEmpty(erpRequest.Start))
                {
                    var eT_VKORG_SELT = func.GetTable("ET_MATNR_SELT");
                    eT_VKORG_SELT.Append();
                    eT_VKORG_SELT.SetValue("SIGN", "I");
                    eT_VKORG_SELT.SetValue("OPTION", "EQ");
                    eT_VKORG_SELT.SetValue("LOW", erpRequest.Start);
                }
                if (erpRequest.DateFrom != null && erpRequest.DateFrom != DateTime.MinValue)
                {
                    func.SetValue("IV_CHANGED", erpRequest.DateFrom.Value);
                }
                func.SetValue("IV_MAT_START", erpRequest.Start);
                func.SetValue("IV_ROWS", erpRequest.Limit);

                await _erpLogsService.InsertErpLogAsync(
                    ErpLogLevel.Debug,
                    ErpSyncLevel.Product,
                    $"GetProductImages before call to BAPI details (Start = {erpRequest.Start}, Limit = {erpRequest.Limit}). Click view to see details.",
                    $"GetProductImages before call to BAPI: {func}"
                );

                func.Invoke(dest);

                await _erpLogsService.InsertErpLogAsync(
                    ErpLogLevel.Debug,
                    ErpSyncLevel.Product,
                    $"GetProductImages after call to BAPI details (Start = {erpRequest.Start}, Limit = {erpRequest.Limit}). Click view to see details.",
                    $"GetProductImages after call to BAPI: {func}"
                );

                var products = func.GetTable("ES_MATERIALS");
                //resp.data = JArray.FromObject(products.Select((product) => new
                //{
                //    MATNR = product["MATNR"].GetString(),
                //    MAT_IMAGE_TAB = product["MAT_IMAGE_TAB"]?.GetTable()?.SelectMany((img) => img["LINE"]?.GetByteArray())?.ToArray(),
                //    MAT_SPEC_TAB = product["MAT_SPEC_TAB"]?.GetTable()?.SelectMany((img) => img["LINE"]?.GetByteArray())?.ToArray()
                //}));

                if (products != null && products.Count > 0)
                {
                    foreach (var product in products)
                    {
                        var imageTab = product["MAT_IMAGE_TAB"]?.GetTable();
                        var specTab = product["MAT_SPEC_TAB"]?.GetTable();

                        var imageData = imageTab?.SelectMany(img => img["LINE"]?.GetByteArray()).ToArray();

                        var specData = specTab?.SelectMany(spec => spec["LINE"]?.GetByteArray()).ToArray();

                        var productData = new ErpProductImageDataModel
                        {
                            Sku = product["MATNR"]?.GetString(),
                            ImageData = imageData,
                            SpecData = specData
                        };

                        erpResponseData.Data.Add(productData);
                    }
                }
            }
            catch (Exception ex)
            {
                await _erpLogsService.InsertErpLogAsync(
                    ErpLogLevel.Error,
                    ErpSyncLevel.Product,
                    $"GetProductImages error (Start = {erpRequest.Start}, Limit = {erpRequest.Limit}). Click view to see details.",
                    $"GetProductImages error: \nException message: {ex.Message}\nStack trace: {ex.StackTrace}"
                );
                erpResponseData.ErpResponseModel.IsError = true;
                erpResponseData.ErpResponseModel.ErrorShortMessage = ex.Message;
            }

            return erpResponseData;

            #endregion

        }
        catch (Exception ex)
        {
            erpResponseData.ErpResponseModel.IsError = true;
            erpResponseData.ErpResponseModel.ErrorShortMessage = ex.Message;
            erpResponseData.ErpResponseModel.ErrorFullMessage = !string.IsNullOrEmpty(responseContent) ? 
                responseContent : ex.StackTrace;
        }

        return erpResponseData;
    }
}
