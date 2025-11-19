using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Web.Areas.Admin.Models.Orders;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models;
using NopStation.Plugin.B2B.B2BB2CFeatures.Helpers;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Factories;

public class ErpOrderModelFactory : IErpOrderModelFactory
{
    #region Fields

    private readonly IDateTimeHelper _dateTimeHelper;
    private readonly ICustomerService _customerService;
    private readonly IOrderService _orderService;
    private readonly ILocalizationService _localizationService;
    private readonly IErpOrderAdditionalDataService _erpOrderAdditionalDataService;
    private readonly IErpOrderItemAdditionalDataService _erpOrderItemAdditionalDataService;
    private readonly IErpAccountService _erpAccountService;
    private readonly IErpSalesOrgService _erpSalesOrgService;
    private readonly ICommonHelper _commonHelper;
    private readonly IErpShipToAddressService _erpShipToAddressService;

    #endregion

    #region Ctor

    public ErpOrderModelFactory(ILocalizationService localizationService,
        IDateTimeHelper dateTimeHelper,
        ICustomerService customerService,
        IOrderService orderService,
        IErpOrderAdditionalDataService erpOrderAdditionalDataService,
        IErpOrderItemAdditionalDataService erpOrderItemAdditionalDataService,
        IErpAccountService erpAccountService,
        IErpSalesOrgService erpSalesOrgService,
        ICommonHelper commonHelper,
        IErpShipToAddressService erpShipToAddressService)
    {
        _localizationService = localizationService;
        _dateTimeHelper = dateTimeHelper;
        _customerService = customerService;
        _orderService = orderService;
        _erpOrderAdditionalDataService = erpOrderAdditionalDataService;
        _erpOrderItemAdditionalDataService = erpOrderItemAdditionalDataService;
        _erpAccountService = erpAccountService;
        _erpSalesOrgService = erpSalesOrgService;
        _commonHelper = commonHelper;
        _erpShipToAddressService = erpShipToAddressService;
    }

    #endregion

    #region Method

    public async Task<ErpOrderAdditionalDataListModel> PrepareErpOrderPerAccountListModel(ErpOrderAdditionalDataSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        var orderPerAccounts = await _erpOrderAdditionalDataService.GetAllErpOrderAdditionalDataAsync(
            nopOrderNumber: searchModel.SearchOrderNumber,
            accountId: searchModel.SearchErpAccountId,
            erpOrderNumber: searchModel.SearchErpOrderNumber,
            erpOrderOriginTypeId: searchModel.SearchErpOrderOriginTypeId,
            erpOrderTypeId: searchModel.SearchErpOrderTypeId,
            integrationStatusTypeId: searchModel.SearchIntegrationStatusTypeId,
            searchOrderDateFrom: searchModel.SearchOrderPlacedOnFrom,
            searchOrderDateTo : searchModel.SearchOrderPlacedOnTo,
            pageIndex: searchModel.Page - 1,
            pageSize: searchModel.PageSize);

        var model = await new ErpOrderAdditionalDataListModel().PrepareToGridAsync(searchModel, orderPerAccounts, () =>
        {
            return orderPerAccounts.SelectAwait(async orderPerAccount =>
            {
                var erpAccount = await _erpAccountService.GetErpAccountByIdAsync(orderPerAccount.ErpAccountId);
                var erpSalesOrg = await _erpSalesOrgService.GetErpSalesOrgByIdAsync(erpAccount.ErpSalesOrgId);
                var erpOrderModel = new ErpOrderAdditionalDataModel
                {
                    Id = orderPerAccount.Id,
                    NopOrderId = orderPerAccount.NopOrderId,
                    ErpOrderNumber = orderPerAccount.ErpOrderNumber,
                    ErpOrderOriginTypeId = orderPerAccount.ErpOrderOriginTypeId,
                    ErpOrderOriginType = await _localizationService.GetLocalizedEnumAsync(orderPerAccount.ErpOrderOriginType),
                    ErpOrderTypeId = orderPerAccount.ErpOrderTypeId,
                    ErpOrderType = await _localizationService.GetLocalizedEnumAsync(orderPerAccount.ErpOrderType),
                    IntegrationStatusTypeId = orderPerAccount.IntegrationStatusTypeId,
                    IntegrationStatusType = await _localizationService.GetLocalizedEnumAsync(orderPerAccount.IntegrationStatusType),
                    ErpAccountId = orderPerAccount.ErpAccountId,
                    ErpAccountName = erpAccount != null ? $"{erpAccount.AccountName} ({erpAccount.AccountNumber})" : "",
                    ErpAccountSalesOrganisationId = erpAccount.ErpSalesOrgId,
                    ErpAccountSalesOrganisationName = $"{erpSalesOrg.Name} - ({erpSalesOrg.Code})"
                };

                erpOrderModel.ErpOrderType = _erpOrderAdditionalDataService.GetErpOrderTypeByOrderTypeEnum(orderPerAccount.ErpOrderType);

                var nopOrder = await _orderService.GetOrderByIdAsync(orderPerAccount.NopOrderId);
                if (nopOrder != null)
                {
                    erpOrderModel.QouteDate = await _dateTimeHelper.ConvertToUserTimeAsync(nopOrder.CreatedOnUtc, DateTimeKind.Utc);
                    erpOrderModel.OrderNumber = string.IsNullOrEmpty(nopOrder.CustomOrderNumber) ? nopOrder.Id.ToString() : nopOrder.CustomOrderNumber;
                }

                return erpOrderModel;
            });
        });
        return model;
    }

    public async Task<ErpOrderAdditionalDataSearchModel> PrepareErpOrderPerAccountSearchModel(ErpOrderAdditionalDataSearchModel orderSearchModel)
    {
        ArgumentNullException.ThrowIfNull(orderSearchModel);

        // to prepare enum dropdown pass ant of the enum value.
        await _commonHelper.PrepareDropdownFromEnumAsync(
            orderSearchModel.AvailableErpOrderTypeOptions,
            ErpOrderType.B2BSalesOrder
        );
        await _commonHelper.PrepareDropdownFromEnumAsync(
            orderSearchModel.AvailableIntegrationStatusTypeOptions,
            IntegrationStatusType.Confirmed
        );
        await _commonHelper.PrepareDropdownFromEnumAsync(
            orderSearchModel.AvailableCustomerTypes,
            ErpUserType.B2BUser
        );
        await _commonHelper.PrepareDropdownFromEnumAsync(
            orderSearchModel.AvailableErpOrderOriginTypeOptions,
            ErpOrderOriginType.OnlineOrder
        );

        orderSearchModel.SetGridPageSize();
        return orderSearchModel;
    }

    public async Task<ErpOrderAdditionalDataModel> PrepareErpOrderPerAccountModel(ErpOrderAdditionalDataModel model, ErpOrderAdditionalData erpOrderAdditionalData)
    {
        if (erpOrderAdditionalData != null)
        {
            model = model ?? new ErpOrderAdditionalDataModel();
            model.Id = erpOrderAdditionalData.Id;
            model.NopOrderId = erpOrderAdditionalData.NopOrderId;
            model.ErpOrderOriginTypeId = erpOrderAdditionalData.ErpOrderOriginTypeId;
            model.ErpOrderOriginType = await _localizationService.GetLocalizedEnumAsync(erpOrderAdditionalData.ErpOrderOriginType);
            model.ErpOrderTypeId = erpOrderAdditionalData.ErpOrderTypeId;
            model.ErpOrderType = await _localizationService.GetLocalizedEnumAsync(erpOrderAdditionalData.ErpOrderType);
            model.OrderPlacedByNopCustomerId = erpOrderAdditionalData.OrderPlacedByNopCustomerId;
            model.OrderPlacedByNopCustomerEmail = erpOrderAdditionalData.OrderPlacedByNopCustomerId > 0 ? 
                (await _customerService.GetCustomerByIdAsync(erpOrderAdditionalData.OrderPlacedByNopCustomerId))?.Email ?? string.Empty : string.Empty;

            var erpAccount = await _erpAccountService.GetErpAccountByIdAsync(erpOrderAdditionalData.ErpAccountId);

            if (erpAccount == null)
                return model;

            model.ErpAccountId = erpOrderAdditionalData.ErpAccountId;
            model.ErpAccountName = $"{erpAccount.AccountName} ({erpAccount.AccountNumber})";

            model.ErpAccountSalesOrganisationId = erpAccount.ErpSalesOrgId;
            var salesOrg = await _erpSalesOrgService.GetErpSalesOrgByIdAsync(erpAccount.ErpSalesOrgId);
            if (salesOrg != null)
            {
                model.ErpAccountSalesOrganisationName = $"{salesOrg.Name} ({salesOrg.Code})";
            }

            model.ErpShipToAddressId = erpOrderAdditionalData.ErpShipToAddressId;
            var erpShipAddress = await _erpShipToAddressService.GetErpShipToAddressByIdAsync(erpOrderAdditionalData.ErpShipToAddressId ?? 0);
            if (erpShipAddress != null)
            {
                model.ErpShipToName = !string.IsNullOrWhiteSpace(erpShipAddress.ShipToCode) ? 
                    $"{model.ErpShipToName} ({erpShipAddress.ShipToCode})" : 
                    $"{erpShipAddress.ShipToName}";
            }            
            
            var nopOrder = await _orderService.GetOrderByIdAsync(model.NopOrderId);
            model.OrderNumber = nopOrder?.CustomOrderNumber ?? erpOrderAdditionalData.ErpOrderNumber;

            model.ErpOrderNumber = erpOrderAdditionalData.ErpOrderNumber;
            model.SpecialInstructions = erpOrderAdditionalData.SpecialInstructions;
            model.CustomerReference = erpOrderAdditionalData.CustomerReference;
            model.ERPOrderStatus = erpOrderAdditionalData.ERPOrderStatus;
            model.ExpectedDeliveryDate = erpOrderAdditionalData.DeliveryDate;
            model.QuoteExpiryDate = erpOrderAdditionalData.QuoteExpiryDate;
            model.IntegrationStatusTypeId = erpOrderAdditionalData.IntegrationStatusTypeId;
            model.IntegrationStatusType = await _localizationService.GetLocalizedEnumAsync(erpOrderAdditionalData.IntegrationStatusType);
            model.IntegrationError = erpOrderAdditionalData.IntegrationError;
            model.IntegrationRetries = erpOrderAdditionalData.IntegrationRetries ?? 0;
            model.IntegrationErrorDateTime = !erpOrderAdditionalData.IntegrationErrorDateTimeUtc.HasValue ? 
                model.IntegrationErrorDateTime : await _dateTimeHelper.ConvertToUserTimeAsync(erpOrderAdditionalData.IntegrationErrorDateTimeUtc.Value, DateTimeKind.Utc);
            model.LastERPUpdate = !erpOrderAdditionalData.LastERPUpdateUtc.HasValue ? 
                model.LastERPUpdate : await _dateTimeHelper.ConvertToUserTimeAsync(erpOrderAdditionalData.LastERPUpdateUtc.Value, DateTimeKind.Utc);

            if (erpOrderAdditionalData.ChangedOnUtc.HasValue)
                model.ChangedOn = await _dateTimeHelper.ConvertToUserTimeAsync(erpOrderAdditionalData.ChangedOnUtc.Value, DateTimeKind.Utc);

            model.ChangedById = erpOrderAdditionalData.ChangedById;

            var customer = await _customerService.GetCustomerByIdAsync(erpOrderAdditionalData.ChangedById);
            model.ChangedByCustomerEmail = customer?.Email;

            if (erpOrderAdditionalData.QuoteSalesOrderId.HasValue && erpOrderAdditionalData.QuoteSalesOrderId.Value > 0)
            {
                model.QuoteSalesOrderId = erpOrderAdditionalData.QuoteSalesOrderId.Value;

                var originalQuoteOrder = await _erpOrderAdditionalDataService.GetErpOrderAdditionalDataByIdAsync(erpOrderAdditionalData.QuoteSalesOrderId.Value);
                if (originalQuoteOrder != null)
                {
                    model.QuoteSalesOrderNumber = !string.IsNullOrWhiteSpace(originalQuoteOrder.ErpOrderNumber) ? 
                        originalQuoteOrder.ErpOrderNumber : nopOrder?.CustomOrderNumber;
                    model.QuoteSalesOrderNopOrderId = originalQuoteOrder.NopOrderId;
                }
            }
        }

        return model;
    }

    public async Task<ErpOrderModel> PrepareErpOrderModel(ErpOrderModel erpOrderModel, OrderModel orderModel)
    {
        erpOrderModel.Id = orderModel.Id;
        erpOrderModel.HasDownloadableProducts = orderModel.HasDownloadableProducts;
        erpOrderModel.IsLoggedInAsVendor = orderModel.IsLoggedInAsVendor;
        erpOrderModel.AllowCustomersToSelectTaxDisplayType = orderModel.AllowCustomersToSelectTaxDisplayType;
        erpOrderModel.TaxDisplayType = orderModel.TaxDisplayType;
        erpOrderModel.CheckoutAttributeInfo = orderModel.CheckoutAttributeInfo;
        erpOrderModel.CustomOrderNumber = orderModel.CustomOrderNumber;

        foreach (var item in orderModel.Items)
        {
            var erpOrderLineItem = await _erpOrderItemAdditionalDataService.GetErpOrderItemAdditionalDataByNopOrderItemIdAsync(item.Id);
            if (erpOrderLineItem == null)
                continue;

            var erpOrderItemModel = new ErpOrderItemAdditionalDataModel
            {
                Id = item.Id,
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                PictureThumbnailUrl = item.PictureThumbnailUrl,
                AttributeInfo = item.AttributeInfo,
                RecurringInfo = item.RecurringInfo,
                RentalInfo = item.RentalInfo,
                Sku = item.Sku,
                VendorName = item.VendorName,
                ReturnRequests = item.ReturnRequests,
                PurchasedGiftCardIds = item.PurchasedGiftCardIds,
                IsDownload = item.IsDownload,
                DownloadCount = item.DownloadCount,
                DownloadActivationType = item.DownloadActivationType,
                IsDownloadActivated = item.IsDownloadActivated,
                LicenseDownloadGuid = item.LicenseDownloadGuid,
                UnitPriceInclTax = item.UnitPriceInclTax,
                UnitPriceInclTaxValue = item.UnitPriceInclTaxValue,
                UnitPriceExclTax = item.UnitPriceExclTax,
                UnitPriceExclTaxValue = item.UnitPriceExclTaxValue,
                Quantity = item.Quantity,
                DiscountInclTax = item.DiscountInclTax,
                DiscountInclTaxValue = item.DiscountInclTaxValue,
                DiscountExclTax = item.DiscountExclTax,
                DiscountExclTaxValue = item.DiscountExclTaxValue,
                SubTotalInclTax = item.SubTotalInclTax,
                SubTotalInclTaxValue = item.SubTotalInclTaxValue,
                SubTotalExclTax = item.SubTotalExclTax,
                SubTotalExclTaxValue = item.SubTotalExclTaxValue,
                NopOrderItemId = item.Id,
                ERPOrderLineNumber = erpOrderLineItem.ErpOrderLineNumber,
                ERPSalesUoM = erpOrderLineItem.ErpSalesUoM,
                ERPOrderLineStatus = erpOrderLineItem.ErpOrderLineStatus,
                ERPDateRequired = erpOrderLineItem.ErpDateRequired,
                ERPDateExpected = erpOrderLineItem.ErpDateExpected,
                ERPDeliveryMethod = erpOrderLineItem.ErpDeliveryMethod,
                ERPInvoiceNumber = erpOrderLineItem.ErpInvoiceNumber,
                ERPOrderLineNotes = erpOrderLineItem.ErpOrderLineNotes,
                LastERPUpdateUtc = erpOrderLineItem.LastErpUpdateUtc,
            };

            erpOrderModel.ErpOrderItems.Add(erpOrderItemModel);
        }
        return erpOrderModel;
    }

    #endregion
}