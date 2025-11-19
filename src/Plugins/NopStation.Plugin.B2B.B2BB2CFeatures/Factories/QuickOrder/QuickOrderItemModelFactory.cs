using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using ClosedXML.Excel;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.ExportImport;
using Nop.Services.ExportImport.Help;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.B2B.B2BB2CFeatures.Model.QuickOrderModels.QuickOrderItems;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpCustomerFunctionality;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpShoppingCartItemService;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpSpecificationAttributeService;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services.QuickOrderServices;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Factories.QuickOrder;

public class QuickOrderItemModelFactory : IQuickOrderItemModelFactory
{
    #region Fields

    private readonly IShoppingCartService _shoppingCartService;
    private readonly IProductService _productService;
    private readonly IQuickOrderItemService _quickOrderItemService;
    private readonly IOrderService _orderService;
    private readonly IWorkContext _workContext;
    private readonly IStoreContext _storeContext;
    private readonly ILocalizationService _localizationService;
    private readonly IPriceFormatter _priceFormatter;
    private readonly IUrlRecordService _urlRecordService;
    private readonly ICurrencyService _currencyService;
    private readonly IGenericAttributeService _genericAttributeService;
    private readonly IPermissionService _permissionService;
    private readonly ShoppingCartSettings _shoppingCartSettings;
    private readonly IErpSpecificationAttributeService _erpSpecificationAttributeService;
    private readonly ILanguageService _languageService;
    private readonly CatalogSettings _catelogSettings;
    private readonly IProductAttributeFormatter _productAttributeFormatter;
    private readonly IQuickOrderTemplateService _quickOrderTemplateService;
    private readonly ICustomerService _customerService;
    private readonly IProductAttributeService _productAttributeService;
    private readonly B2BB2CFeaturesSettings _b2BB2CFeaturesSettings;
    private readonly IErpCustomerFunctionalityService _erpCustomerFunctionalityService;
    private readonly IErpShoppingCartItemService _erpShoppingCartItemService;
    private readonly IErpLogsService _erpLogsService;
    private readonly IErpSpecialPriceService _erpSpecialPriceService;

    #endregion Fields

    #region Ctor

    public QuickOrderItemModelFactory(IShoppingCartService shoppingCartService,
        IProductService productService,
        IQuickOrderItemService quickOrderItemService,
        IOrderService orderService,
        IWorkContext workContext,
        IStoreContext storeContext,
        ISettingService settingService,
        ILocalizationService localizationService,
        IPriceFormatter priceFormatter,
        IUrlRecordService urlRecordService,
        ICurrencyService currencyService,
        IGenericAttributeService genericAttributeService,
        IQuickOrderTemplateService quickOrderTemplateService,
        ICustomerService customerService,
        IPermissionService permissionService,
        ShoppingCartSettings shoppingCartSettings,
        IErpAccountService erpAccountService,
        IErpSpecialPriceService erpSpecialPriceService,
        IErpSpecificationAttributeService erpSpecificationAttributeService,
        IProductAttributeService productAttributeService,
        ILanguageService languageService,
        CatalogSettings catelogSettings,
        IProductAttributeFormatter productAttributeFormatter,
        B2BB2CFeaturesSettings b2BB2CFeaturesSettings,
        IErpCustomerFunctionalityService erpCustomerFunctionalityService,
        IErpShoppingCartItemService erpShoppingCartItemService,
        IErpLogsService erpLogsService)
    {
        _shoppingCartService = shoppingCartService;
        _productService = productService;
        _quickOrderItemService = quickOrderItemService;
        _orderService = orderService;
        _workContext = workContext;
        _storeContext = storeContext;
        _localizationService = localizationService;
        _priceFormatter = priceFormatter;
        _urlRecordService = urlRecordService;
        _currencyService = currencyService;
        _genericAttributeService = genericAttributeService;
        _permissionService = permissionService;
        _shoppingCartSettings = shoppingCartSettings;
        _erpSpecificationAttributeService = erpSpecificationAttributeService;
        _productAttributeService = productAttributeService;
        _quickOrderTemplateService = quickOrderTemplateService;
        _customerService = customerService;
        _languageService = languageService;
        _catelogSettings = catelogSettings;
        _productAttributeFormatter = productAttributeFormatter;
        _b2BB2CFeaturesSettings = b2BB2CFeaturesSettings;
        _erpCustomerFunctionalityService = erpCustomerFunctionalityService;
        _erpShoppingCartItemService = erpShoppingCartItemService;
        _erpLogsService = erpLogsService;
        _erpSpecialPriceService = erpSpecialPriceService;
    }

    #endregion

    #region Utilities

    private static ExportedAttributeType GetTypeOfExportedAttribute(IXLWorksheet defaultWorksheet, List<IXLWorksheet> localizedWorksheets, PropertyManager<ImportProductMetadata, Language> productAttributeManager, int iRow)
    {
        productAttributeManager.ReadDefaultFromXlsx(defaultWorksheet, iRow, ExportProductAttribute.ProductAttributeCellOffset);

        foreach (var worksheet in localizedWorksheets)
            productAttributeManager.ReadLocalizedFromXlsx(worksheet, iRow, ExportProductAttribute.ProductAttributeCellOffset);

        return ExportedAttributeType.ProductAttribute;
    }

    protected virtual async Task SetOutLineForProductAttributeRowAsync(object cellValue, IXLWorksheet worksheet, int endRow)
    {
        try
        {
            var aid = Convert.ToInt32(cellValue ?? -1);

            var productAttribute = await _productAttributeService.GetProductAttributeByIdAsync(aid);

            if (productAttribute != null)
                worksheet.Row(endRow).OutlineLevel = 1;
        }
        catch
        {
            if ((cellValue ?? string.Empty).ToString() == "AttributeId")
                worksheet.Row(endRow).OutlineLevel = 1;
        }
    }

    private async Task<(PropertyManager<Product, Language> Manager, IList<PropertyByName<Product, Language>> Properties, PropertyManager<ImportProductMetadata, Language> ProductAttributeManager)> PrepareImportDataAsync(IXLWorkbook workbook, IXLWorksheet worksheet, IList<Language> languages)
    {
        //get metadata
        var metadata = ImportManager.GetWorkbookMetadata<Product>(workbook, languages);
        var defaultWorksheet = metadata.DefaultWorksheet;
        //get properties
        var properties = metadata.DefaultProperties;

        var manager = new PropertyManager<Product, Language>(properties, _catelogSettings);

        var productAttributeProperties = new[]
        {
            new PropertyByName<ImportProductMetadata, Language>("AttributeId"),
            new PropertyByName<ImportProductMetadata, Language>("ProductAttributeName"),
            new PropertyByName<ImportProductMetadata, Language>("ProductAttributeValueName"),
            new PropertyByName<ImportProductMetadata, Language>("ProductAttributeValueId")
        };

        var productAttributeLocalizedProperties = new[]
        {
            new PropertyByName<ImportProductMetadata, Language>("DefaultValue"),
            new PropertyByName<ImportProductMetadata, Language>("AttributeTextPrompt"),
            new PropertyByName<ImportProductMetadata, Language>("ValueName")
        };

        var productAttributeManager = new PropertyManager<ImportProductMetadata, Language>(productAttributeProperties, _catelogSettings, productAttributeLocalizedProperties, languages);

        var endRow = 2;

        var allSkuCells = new List<string>();

        var tempProperty = manager.GetDefaultProperty("SKU");
        var skuCellNum = tempProperty?.PropertyOrderPosition ?? -1;

        var allQunatities = new List<string>();
        tempProperty = manager.GetDefaultProperty("Quantity");
        var quantityCellNum = tempProperty?.PropertyOrderPosition ?? -1;

        var allAttributeIds = new List<string>();
        var allAttributeNames = new List<string>();
        var allAttributeValueIds = new List<string>();
        var allAttributeValueNames = new List<string>();

        var attributeIdCellNum = 1 + ExportProductAttribute.ProductAttributeCellOffset;
        var productsInFile = new List<int>();
        var typeOfExportedAttribute = ExportedAttributeType.NotSpecified;

        while (true)
        {
            var allColumnsAreEmpty = metadata.DefaultProperties.Select(property => worksheet.Row(endRow).Cell(property.PropertyOrderPosition))
                .All(cell => string.IsNullOrWhiteSpace($"{cell?.Value}"));

            if (allColumnsAreEmpty)
                break;

            if (new[] { 1, 2 }
                .Select(cellNum => defaultWorksheet.Row(endRow).Cell(cellNum))
                .All(cell => string.IsNullOrWhiteSpace($"{cell?.Value}")) && 
                defaultWorksheet.Row(endRow).OutlineLevel == 0)
            {
                var cellValue = defaultWorksheet.Row(endRow).Cell(attributeIdCellNum).Value;
                await SetOutLineForProductAttributeRowAsync(cellValue, defaultWorksheet, endRow);
            }

            if (defaultWorksheet.Row(endRow).OutlineLevel != 0)
            {
                var newTypeOfExportedAttribute = GetTypeOfExportedAttribute(defaultWorksheet, metadata.LocalizedWorksheets, productAttributeManager, endRow);

                //skip caption row
                if (newTypeOfExportedAttribute != ExportedAttributeType.NotSpecified && newTypeOfExportedAttribute != typeOfExportedAttribute)
                {
                    typeOfExportedAttribute = newTypeOfExportedAttribute;
                    endRow++;
                    continue;
                }

                switch (typeOfExportedAttribute)
                {
                    case ExportedAttributeType.ProductAttribute:
                        productAttributeManager.ReadDefaultFromXlsx(defaultWorksheet, endRow, ExportProductAttribute.ProductAttributeCellOffset);

                        if (int.TryParse((defaultWorksheet.Row(endRow).Cell(attributeIdCellNum).Value).ToString(), out var aid))
                        {
                            allAttributeIds.Add(aid.ToString());
                            var attributeName = (defaultWorksheet.Row(endRow).Cell(attributeIdCellNum + 1).Value).ToString();
                            allAttributeNames.Add(attributeName.ToString());
                            var attributeValueName = (defaultWorksheet.Row(endRow).Cell(attributeIdCellNum + 2).Value).ToString();
                            allAttributeValueNames.Add(attributeValueName.ToString());
                            var attributeValueNameId = (defaultWorksheet.Row(endRow).Cell(attributeIdCellNum + 3).Value).ToString();
                            allAttributeValueIds.Add(attributeValueNameId.ToString());
                        }
                        break;
                }

                endRow++;
                continue;
            }

            if (skuCellNum > 0)
            {
                var skuCellName = worksheet.Row(endRow).Cell(skuCellNum).Value.ToString() ?? string.Empty;

                if (!string.IsNullOrEmpty(skuCellName))
                    allSkuCells.Add(skuCellName);
            }

            if (quantityCellNum > 0)
            {
                var quantityCellName = worksheet.Row(endRow).Cell(quantityCellNum).Value.ToString() ??
                                      string.Empty;
                if (!string.IsNullOrEmpty(quantityCellName))
                    allQunatities.AddRange(quantityCellName
                        .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()));
            }

            productsInFile.Add(endRow);

            endRow++;
        }

        return (manager, properties, productAttributeManager);
    }

    #endregion

    #region Methods

    public async Task<string> GetValidationResultAsync(Product product, int quantity, string attribute)
    {
        if (product == null)
        {
            //no product found
            return "No product found with the specified SKU";
        }

        var warnings = await _shoppingCartService.GetShoppingCartItemWarningsAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, product,
         (await _storeContext.GetCurrentStoreAsync()).Id, attribute, decimal.Zero, quantity: quantity, addRequiredProducts: false);

        if (warnings.Any())
            return warnings.FirstOrDefault();

        return "OK";
    }

    public async Task<QuickOrderItemListModel> PrepareQuickOrderItemListModelAsync(QuickOrderItemSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        // get QuickOrderItems
        var quickOrderItems = await _quickOrderItemService.GetAllQuickOrderItemsPagedAsync(productSku: searchModel.ProductSku, quickOrderTemplateId: searchModel.QuickOrderTemplateId,
            pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

        var store = await _storeContext.GetCurrentStoreAsync();
        var customer = await _workContext.GetCurrentCustomerAsync();
        var erpAccount = await _erpCustomerFunctionalityService.GetActiveErpAccountByCustomerAsync(customer);

        //customer currency
        var currencyTmp = await _currencyService.GetCurrencyByIdAsync(
          await _genericAttributeService.GetAttributeAsync<int>(customer, customer.CustomCustomerAttributesXML, store.Id));
        var customerCurrency = currencyTmp != null && currencyTmp.Published ? currencyTmp : await _workContext.GetWorkingCurrencyAsync();
        var customerCurrencyCode = customerCurrency.CurrencyCode;

        //prepare list model
        var model = await new QuickOrderItemListModel().PrepareToGridAsync(searchModel, quickOrderItems, () =>
        {
            return quickOrderItems.SelectAwait(async quickOrder =>
            {
                var product = await _productService.GetProductBySkuAsync(quickOrder.ProductSku);

                //fill in model values from the entity
                var quickOrderItem = new QuickOrderItemModel
                {
                    Id = quickOrder.Id,
                    ProductSku = quickOrder.ProductSku,
                    Quantity = quickOrder.Quantity,
                    QuickOrderTemplateId = quickOrder.QuickOrderTemplateId
                };

                if (product != null)
                {
                    quickOrderItem.AttributesInfo = await _productAttributeFormatter.FormatAttributesAsync(product, quickOrder.AttributesXml);

                    var (finalPrice, _, _) = await _shoppingCartService.GetUnitPriceAsync(product,
                        customer,
                        store,
                        ShoppingCartType.ShoppingCart, 1, quickOrder.AttributesXml, 0, null, null, true);

                    var isProductOnSpecial = await _erpCustomerFunctionalityService.IsTheProductFromSpecialCategoryAsync(product);

                    if (isProductOnSpecial)
                        quickOrderItem.ProductIsOnSpecialIconUrl = B2BB2CFeaturesDefaults.ProductIsOnSpecialIconPath;
                    else
                        quickOrderItem.ProductIsOnSpecialIconUrl = string.Empty;
                    // set values
                    quickOrderItem.ProductId = product.Id;
                    quickOrderItem.Name = await _localizationService.GetLocalizedAsync(product, x => x.Name);
                    quickOrderItem.SeName = await _urlRecordService.GetSeNameAsync(product);
                    quickOrderItem.StockAvailability = await _productService.FormatStockMessageAsync(product, string.Empty);

                    ErpSpecialPrice erpSpecialPrice = null;
                    if (!_b2BB2CFeaturesSettings.UseProductGroupPrice)
                    {
                        erpSpecialPrice = await _erpSpecialPriceService.GetErpSpecialPricesByErpAccountIdAndNopProductIdAsync(erpAccount?.Id ?? 0, product.Id);
                    }

                    // Alternate Method Could not find , Taking empty string -1188
                    quickOrderItem.PricingNotes = await _erpSpecialPriceService.GetProductPricingNoteByErpSpecialPriceAsync(erpSpecialPrice);

                    // Change UnitOfMeasureSpecificationAttributeId to PreFilterFacetSpecificationAttributeId -- need to check the relativity
                    quickOrderItem.UOM = await _erpSpecificationAttributeService.GetProductUOMByProductIdAndSpecificationAttributeId(product.Id,
                        _b2BB2CFeaturesSettings.UnitOfMeasureSpecificationAttributeId) ?? string.Empty;

                    var language = await _workContext.GetWorkingLanguageAsync();

                    quickOrderItem.Price = await _priceFormatter.FormatPriceAsync(finalPrice, true, customerCurrencyCode, language.Id, true);
                    quickOrderItem.PriceValue = finalPrice;

                    if (finalPrice == _b2BB2CFeaturesSettings.ProductQuotePrice)
                    {
                        quickOrderItem.Price = await _localizationService.GetResourceAsync("Products.ProductForQuote");
                        quickOrderItem.PricingNotes = await _localizationService.GetResourceAsync("Products.ProductForQuote");
                    }
                }

                if (searchModel.Validate)
                    quickOrderItem.ValidationResult = await GetValidationResultAsync(product, quickOrder.Quantity, quickOrder.AttributesXml);

                return quickOrderItem;
            });
        });

        return model;
    }

    public async Task<QuickOrderItemModel> PrepareQuickOrderItemModelAsync(QuickOrderItemModel model, QuickOrderItem quickOrderItem)
    {
        if (quickOrderItem != null)
        {
            model = model ?? new QuickOrderItemModel();
            model.Id = quickOrderItem.Id;
            model.ProductSku = quickOrderItem.ProductSku;
            model.Quantity = quickOrderItem.Quantity;
            model.QuickOrderTemplateId = quickOrderItem.QuickOrderTemplateId;
        }

        return model;
    }

    public async Task<QuickOrderItemSearchModel> PrepareQuickOrderItemSearchModelAsync(QuickOrderItemSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        // set always validate
        searchModel.Validate = true;

        //prepare page parameters
        searchModel.SetGridPageSize();

        return searchModel;
    }

    public async Task<(IList<string> warnings, int totalProducts, int added, int failed)> ImportQuickOrderItemsFromXlsxAsync(int templateId, Stream stream)
    {
        var warnings = new List<string>();
        var total = 0;
        var failed = 0;
        var customer = await _workContext.GetCurrentCustomerAsync();
        var store = await _storeContext.GetCurrentStoreAsync();
        var languages = await _languageService.GetAllLanguagesAsync(showHidden: true);
        var productsFailedToImport = new List<string>();
        var quickOrderItemsInsertList = new List<QuickOrderItem>();
        var quickOrderItemsUpdateList = new List<QuickOrderItem>();

        try
        {
            var quickOrderTemplate = await _quickOrderTemplateService.GetQuickOrderTemplateByIdAsync(templateId);
            if (quickOrderTemplate == null)
            {
                warnings.Add(await _localizationService.GetResourceAsync("NopStation.B2BB2CFeatures.QuickOrderTemplate.TemplateNotFound"));
                return (warnings, total, quickOrderItemsInsertList.Count + quickOrderItemsUpdateList.Count, failed);
            }

            var quickOrderItems = await _quickOrderItemService.GetAllQuickOrderItemsAsync(quickOrderTemplateId: templateId);

            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheets.FirstOrDefault() ?? throw new NopException("No worksheet found");
            var metadata = await PrepareImportDataAsync(workbook, worksheet, languages);

            var iRow = 2;
            var metadatas = ImportManager.GetWorkbookMetadata<Product>(workbook, languages);
            var defaultWorksheets = metadatas.DefaultWorksheet;
            var attributeIdCellNum = 1 + ExportProductAttribute.ProductAttributeCellOffset;
            var itemWithQuantityGreaterThenZero = 0;

            while (true)
            {
                var allColumnsAreEmpty = metadata.Manager.GetDefaultProperties
                    .Select(property => worksheet.Row(iRow).Cell(property.PropertyOrderPosition))
                    .All(cell => cell?.Value == null || string.IsNullOrEmpty(cell.Value.ToString()));

                if (allColumnsAreEmpty)
                    break;

                total++;

                if (new[] { 1, 2 }.Select(cellNum => defaultWorksheets.Row(iRow).Cell(cellNum))
                    .All(cell => string.IsNullOrEmpty(cell?.Value.ToString())) &&
                     defaultWorksheets.Row(iRow).OutlineLevel == 0)
                {
                    var cellValue = defaultWorksheets.Row(iRow).Cell(attributeIdCellNum).Value;
                    await SetOutLineForProductAttributeRowAsync(cellValue, defaultWorksheets, iRow);
                }

                metadata.Manager.ReadDefaultFromXlsx(worksheet, iRow);

                var quickOrderItem = new QuickOrderItem();

                var skuPropNameInExcel = await _localizationService.GetResourceAsync("NopStation.Plugin.B2B.B2BB2CFeatures.Services.ExportManager.Excel.Sku");
                var qtyPropNameInExcel = await _localizationService.GetResourceAsync("NopStation.Plugin.B2B.B2BB2CFeatures.Services.ExportManager.Excel.Quantity");

                if (!metadata.Manager.GetDefaultProperties.Any(p => p.PropertyName == skuPropNameInExcel))
                {
                    warnings.Add(string.Format(
                        await _localizationService.GetResourceAsync("NopStation.B2BB2CFeatures.QuickOrderTemplate.ExcelImport.ColumnMissing"),
                        skuPropNameInExcel));
                    failed++;
                    return (warnings, total, quickOrderItemsInsertList.Count + quickOrderItemsUpdateList.Count, failed);
                }

                if (!metadata.Manager.GetDefaultProperties.Any(p => p.PropertyName == qtyPropNameInExcel))
                {
                    warnings.Add(string.Format(
                        await _localizationService.GetResourceAsync("NopStation.B2BB2CFeatures.QuickOrderTemplate.ExcelImport.ColumnMissing"),
                        qtyPropNameInExcel));
                    failed++;
                    return (warnings, total, quickOrderItemsInsertList.Count + quickOrderItemsUpdateList.Count, failed);
                }

                foreach (var property in metadata.Manager.GetDefaultProperties)
                {
                    switch (property.PropertyName)
                    {
                        case var sku when sku == skuPropNameInExcel:
                            quickOrderItem.ProductSku = property.StringValue;
                            break;

                        case var qty when qty == qtyPropNameInExcel:
                            quickOrderItem.Quantity = property.IntValue;
                            break;
                    }
                }
                var attrbutesIds = new List<int>();
                var attrbuteValuesIds = new List<int>();

                var checkIfAttributeRowExists = metadata.ProductAttributeManager.GetDefaultProperties
                    .Where(property => property.PropertyValue != null)
                    .Any();
                if (checkIfAttributeRowExists)
                {
                    var innerRow = iRow + 2;
                    while (true)
                    {
                        checkIfAttributeRowExists = metadata.ProductAttributeManager.GetDefaultProperties
                            .Where(property => property.PropertyValue != null)
                            .Any();

                        if (!checkIfAttributeRowExists)
                            break;

                        var allAttrbuteColumnsAreEmpty = metadata.ProductAttributeManager.GetDefaultProperties
                           .Select(property => worksheet.Row(innerRow).Cell(property.PropertyOrderPosition))
                           .All(cell => cell?.Value == null || string.IsNullOrEmpty(cell.Value.ToString()));

                        if (allColumnsAreEmpty)
                            break;

                        if (allAttrbuteColumnsAreEmpty)
                            break;

                        if (new[] { 1, 2 }.Select(cellNum => defaultWorksheets.Row(innerRow).Cell(cellNum))
                            .All(cell => string.IsNullOrEmpty(cell?.Value.ToString())) &&
                       defaultWorksheets.Row(innerRow).OutlineLevel == 0)
                        {
                            break;
                        }

                        metadata.ProductAttributeManager.ReadDefaultFromXlsx(worksheet, innerRow, 2);

                        foreach (var property in metadata.ProductAttributeManager.GetDefaultProperties)
                        {
                            if (property.PropertyName.Equals("AttributeId"))
                            {
                                var attributeId = 0;
                                if (!string.IsNullOrEmpty(property.PropertyValue.ToString()))
                                    attributeId = int.Parse(property.PropertyValue.ToString());
                                var attribute = await _productAttributeService.GetProductAttributeByIdAsync(attributeId);
                                if (attribute != null)
                                {
                                    var productAttributeMapping = await _quickOrderItemService.GetProductAttributeMapping(attribute.Id);
                                    if (productAttributeMapping != null)
                                    {
                                        attrbutesIds.Add(productAttributeMapping.Id);
                                    }
                                }
                            }

                            if (property.PropertyName.Equals("ProductAttributeName"))
                            {
                                var attributeName = property.PropertyValue.ToString();
                                var attributeByName = await _quickOrderItemService.GetProductAttributeByName(attributeName);
                                if (attributeByName != null)
                                {
                                    var productAttributeMapping = await _quickOrderItemService.GetProductAttributeMapping(attributeByName.Id);
                                    if (productAttributeMapping != null)
                                    {
                                        attrbutesIds.Add(productAttributeMapping.Id);
                                    }
                                }
                            }

                            if (property.PropertyName.Equals("ProductAttributeValueId"))
                            {
                                var attributeValueId = 0;
                                if (!string.IsNullOrEmpty(property.PropertyValue.ToString()))
                                    attributeValueId = int.Parse(property.PropertyValue.ToString());
                                var attributeValue = await _productAttributeService.GetProductAttributeValueByIdAsync(attributeValueId);
                                if (attributeValue != null)
                                {
                                    attrbuteValuesIds.Add(attributeValueId);
                                }
                            }

                            if (property.PropertyName.Equals("ProductAttributeValueName"))
                            {
                                var attributeValueName = property.PropertyValue.ToString();
                                var attributeValueByName = await _quickOrderItemService.GetAttributeValueByNameAsync(attributeValueName);
                                if (attributeValueByName != null)
                                {
                                    attrbuteValuesIds.Add(attributeValueByName.Id);
                                }
                            }
                        }

                        innerRow++;
                    }

                    if (iRow == innerRow)
                        iRow++;
                    else
                        iRow = innerRow;
                }
                else
                {
                    iRow++;
                }

                var xmlDocumentToString = string.Empty;

                try
                {
                    if (attrbutesIds.Count > 0 && attrbuteValuesIds.Count > 0)
                    {
                        var xmlDocument = new XDocument(
                             new XElement("Attributes",
                                 from index in Enumerable.Range(0, attrbutesIds.Count)
                                 select new XElement("ProductAttribute",
                                     new XAttribute("ID", attrbutesIds[index]),
                                     new XElement("ProductAttributeValue",
                                         new XElement("Value", attrbuteValuesIds[index])
                                        )
                                    )
                                )
                            );

                        xmlDocumentToString = xmlDocument.ToString();
                    }

                    var product = await _productService.GetProductBySkuAsync(quickOrderItem.ProductSku);
                    var attributesXml = xmlDocumentToString;

                    if (product != null)
                    {
                        if (quickOrderItem.Quantity > 0)
                        {
                            itemWithQuantityGreaterThenZero++;

                            var alreadyExistsQuickOrderItem = quickOrderItems
                                .FirstOrDefault(x => x.ProductSku == quickOrderItem.ProductSku && x.AttributesXml == attributesXml);

                            if (alreadyExistsQuickOrderItem == null)
                            {
                                alreadyExistsQuickOrderItem = new QuickOrderItem();
                                alreadyExistsQuickOrderItem.Quantity = quickOrderItem.Quantity;
                                alreadyExistsQuickOrderItem.ProductSku = quickOrderItem.ProductSku;
                                alreadyExistsQuickOrderItem.AttributesXml = attributesXml;
                                alreadyExistsQuickOrderItem.QuickOrderTemplateId = templateId;
                                quickOrderItems.Add(alreadyExistsQuickOrderItem);
                                quickOrderItemsInsertList.Add(alreadyExistsQuickOrderItem);
                            }
                            else
                            {
                                alreadyExistsQuickOrderItem.Quantity += quickOrderItem.Quantity;
                                quickOrderItemsUpdateList.Add(alreadyExistsQuickOrderItem);
                            }
                        }
                        else
                        {
                            productsFailedToImport.Add(quickOrderItem.ProductSku);
                            failed++;
                        }
                    }
                    else
                    {
                        productsFailedToImport.Add(quickOrderItem.ProductSku);
                        failed++;
                    }
                }
                catch (Exception e)
                {
                    warnings.Add(e.Message);
                    productsFailedToImport.Add(quickOrderItem.ProductSku);
                    failed++;
                }
            }
            if (itemWithQuantityGreaterThenZero < 1)
            {
                warnings.Add(await _localizationService.GetResourceAsync("NopStation.B2BB2CFeatures.QuickOrderTemplate.ExclImport.NoItemWithQuantity"));
            }
            if (quickOrderItemsInsertList.Count > 0)
            {
                await _quickOrderItemService.InsertQuickOrderItemsAsync(quickOrderItemsInsertList);
            }
            if (quickOrderItemsUpdateList.Count > 0)
            {
                await _quickOrderItemService.UpdateQuickOrderItemsAsync(quickOrderItemsUpdateList);
            }
        }
        catch (Exception ex)
        {
            warnings.Add(ex.Message);
            failed++;
        }

        if (productsFailedToImport.Count > 0)
        {
            await _erpLogsService.InsertErpLogAsync(
                ErpLogLevel.Information,
                ErpSyncLevel.Product, 
                $"Some of the products failed to import to favorites for the customer: {customer.Email} (Id: {customer.Id})", 
                $"The products which failed to import for Customer (Id: {customer.Id}): {string.Join(", ", productsFailedToImport)}");
        }

        return (warnings, total, quickOrderItemsInsertList.Count + quickOrderItemsUpdateList.Count, failed);
    }

    public async Task<bool> CreateQuickOrderItemsFromShoppingCartAsync(int templateId)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();

        if (customer != null && !customer.HasShoppingCartItems)
        {
            return false;
        }

        var cartItems = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);

        if (cartItems.Count == 0)
        {
            return false;
        }

        foreach (var cartItem in cartItems)
        {
            var product = await _productService.GetProductByIdAsync(cartItem.ProductId);
            var sku = product?.Sku;
            if (string.IsNullOrEmpty(sku))
                continue;

            var quickOrderItem = new QuickOrderItem
            {
                QuickOrderTemplateId = templateId,
                ProductSku = product.Sku,
                Quantity = cartItem.Quantity,
                AttributesXml = cartItem.AttributesXml ?? string.Empty,
            };

            await _quickOrderItemService.InsertQuickOrderItemAsync(quickOrderItem);
        }

        return true;
    }

    public async Task<bool> CreateQuickOrderItemsFromOrderAsync(int templateId, int orderId)
    {
        var orderItems = await _orderService.GetOrderItemsAsync(orderId);

        if (orderItems.Count == 0)
        {
            return false;
        }

        foreach (var orderItem in orderItems)
        {
            var product = await _productService.GetProductByIdAsync(orderItem.ProductId);
            var sku = product?.Sku;
            if (string.IsNullOrEmpty(sku))
                continue;

            var quickOrderItem = new QuickOrderItem
            {
                QuickOrderTemplateId = templateId,
                ProductSku = product.Sku,
                Quantity = orderItem.Quantity,
                AttributesXml = orderItem.AttributesXml ?? string.Empty,
            };

            await _quickOrderItemService.InsertQuickOrderItemAsync(quickOrderItem);
        }

        return true;
    }

    public async Task<string> AddToCartAllItemByTemplateAsync(QuickOrderTemplate quickOrderTemplate)
    {
        int added = 0, failed = 0;
        var customer = await _workContext.GetCurrentCustomerAsync();
        var storeId = (await _storeContext.GetCurrentStoreAsync()).Id;
        await _customerService.ResetCheckoutDataAsync(customer, storeId);

        var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, storeId);
        var quickOrderItems = await _quickOrderItemService.GetAllQuickOrderItemsAsync(quickOrderTemplateId: quickOrderTemplate.Id);

        var products = await _productService.GetProductsBySkuAsync(quickOrderItems.Select(x => x.ProductSku).ToArray());

        var cartItemsToInsert = new List<ShoppingCartItem>();
        var cartItemsToUpdate = new List<ShoppingCartItem>();

        foreach (var item in quickOrderItems)
        {
            var product = products.FirstOrDefault(p => p.Sku == item.ProductSku);
            if (product != null && item.Quantity > 0)
            {
                var addToCart = await AddToCartQuickOrderTempleteAsync(customer, 
                    product, 
                    ShoppingCartType.ShoppingCart,
                    cart, 
                    storeId, 
                    attributesXml: item.AttributesXml,
                    quantity: item.Quantity, 
                    addRequiredProducts: false);

                if (addToCart.warnings.Any())
                {
                    failed++;
                }
                else if (addToCart.shoppingCartItem != null)
                {
                    if (addToCart.shoppingCartItem.Id > 0)
                    {
                        cartItemsToUpdate.Add(addToCart.shoppingCartItem);
                    }
                    else
                    {
                        cartItemsToInsert.Add(addToCart.shoppingCartItem);
                    }
                    added++;
                }
            }
            else
            {
                failed++;
            }
        }

        var nopUser = await _erpCustomerFunctionalityService.GetActiveErpNopUserByCustomerAsync(customer);

        if (cartItemsToInsert.Count > 0)
        {
            await _erpShoppingCartItemService.InsertBulkShoppingCartItemsAsync
                (cartItemsToInsert, cartActivityFromB2CUser: nopUser.ErpUserType == ErpUserType.B2CUser);            
        }
        if (cartItemsToUpdate.Count > 0)
        {
            await _erpShoppingCartItemService.UpdateBulkShoppingCartItemsAsync
                (cartItemsToUpdate, cartActivityFromB2CUser: nopUser.ErpUserType == ErpUserType.B2CUser);
        }

        customer.HasShoppingCartItems = cart.Any() || added > 0;
        await _customerService.UpdateCustomerAsync(customer);

        return string.Format(await _localizationService.GetResourceAsync("NopStation.Plugin.B2B.B2BB2CFeatures.QuickOrderTemplate.AddToCartResult"), quickOrderItems.Count, added, failed);
    }

    public virtual async Task<(ShoppingCartItem shoppingCartItem, IList<string> warnings)> AddToCartQuickOrderTempleteAsync(Customer customer, 
        Product product,
        ShoppingCartType shoppingCartType, 
        IList<ShoppingCartItem> cart, 
        int storeId, 
        string attributesXml = null,
        decimal customerEnteredPrice = decimal.Zero,
        DateTime? rentalStartDate = null, 
        DateTime? rentalEndDate = null,
        int quantity = 1, 
        bool addRequiredProducts = true)
    {
        ArgumentNullException.ThrowIfNull(customer);
        ArgumentNullException.ThrowIfNull(product);

        var warnings = new List<string>();
        if (shoppingCartType == ShoppingCartType.ShoppingCart && 
            !await _permissionService.AuthorizeAsync(StandardPermissionProvider.EnableShoppingCart, customer))
        {
            warnings.Add(await _localizationService.GetResourceAsync("NopStation.Plugin.B2B.B2BB2CFeatures.QuickOrderTemplate.Warning.ShoppingCartIsDisabled"));
            return (null, warnings);
        }

        if (shoppingCartType == ShoppingCartType.Wishlist && 
            !await _permissionService.AuthorizeAsync(StandardPermissionProvider.EnableWishlist, customer))
        {
            warnings.Add(await _localizationService.GetResourceAsync("NopStation.Plugin.B2B.B2BB2CFeatures.QuickOrderTemplate.Warning.WishlistIsDisabled"));
            return (null, warnings);
        }

        if (customer.IsSearchEngineAccount())
        {
            warnings.Add(await _localizationService.GetResourceAsync("NopStation.Plugin.B2B.B2BB2CFeatures.QuickOrderTemplate.Warning.SearchEngineCannotAddToCart"));
            return (null, warnings);
        }

        if (quantity <= 0)
        {
            warnings.Add(await _localizationService.GetResourceAsync("ShoppingCart.QuantityShouldPositive"));
            return (null, warnings);
        }

        var shoppingCartItem = await _shoppingCartService.FindShoppingCartItemInTheCartAsync(cart,
            shoppingCartType, product, attributesXml, customerEnteredPrice,
            rentalStartDate, rentalEndDate);

        if (shoppingCartItem != null)
        {
            //update existing shopping cart item
            var newQuantity = shoppingCartItem.Quantity + quantity;
            warnings.AddRange(await _shoppingCartService.GetShoppingCartItemWarningsAsync(customer, 
                shoppingCartType, 
                product,
                storeId, 
                attributesXml,
                customerEnteredPrice, 
                rentalStartDate, 
                rentalEndDate,
                newQuantity, 
                addRequiredProducts, 
                shoppingCartItem.Id));

            if (warnings.Count != 0)
                return (null, warnings);

            shoppingCartItem.AttributesXml = attributesXml;
            shoppingCartItem.Quantity = newQuantity;
            shoppingCartItem.UpdatedOnUtc = DateTime.UtcNow;
        }
        else
        {
            //new shopping cart item
            warnings.AddRange(await _shoppingCartService.GetShoppingCartItemWarningsAsync(customer, shoppingCartType, product,
                storeId, attributesXml, customerEnteredPrice,
                rentalStartDate, rentalEndDate,
                quantity, addRequiredProducts));

            if (warnings.Count != 0)
                return (null, warnings);

            //maximum items validation
            switch (shoppingCartType)
            {
                case ShoppingCartType.ShoppingCart:
                    if (cart.Count >= _shoppingCartSettings.MaximumShoppingCartItems)
                    {
                        warnings.Add(string.Format(await _localizationService.GetResourceAsync("ShoppingCart.MaximumShoppingCartItems"), _shoppingCartSettings.MaximumShoppingCartItems));
                        return (null, warnings);
                    }
                    break;

                case ShoppingCartType.Wishlist:
                    if (cart.Count >= _shoppingCartSettings.MaximumWishlistItems)
                    {
                        warnings.Add(string.Format(await _localizationService.GetResourceAsync("ShoppingCart.MaximumWishlistItems"), _shoppingCartSettings.MaximumWishlistItems));
                        return (null, warnings);
                    }
                    break;

                default:
                    break;
            }

            var now = DateTime.UtcNow;
            shoppingCartItem = new ShoppingCartItem
            {
                ShoppingCartType = shoppingCartType,
                StoreId = storeId,
                ProductId = product.Id,
                AttributesXml = attributesXml,
                CustomerEnteredPrice = customerEnteredPrice,
                Quantity = quantity,
                RentalStartDateUtc = rentalStartDate,
                RentalEndDateUtc = rentalEndDate,
                CreatedOnUtc = now,
                UpdatedOnUtc = now,
                CustomerId = customer.Id
            };
        }

        return (shoppingCartItem, warnings);
    }

    #endregion
}