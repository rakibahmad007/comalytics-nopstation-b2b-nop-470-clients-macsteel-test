using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Services.Shipping;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpWorkflowMessage;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpShippingService;

public class ErpShippingService : IErpShippingService
{
    #region Fields

    private readonly IErpAccountService _erpAccountService;
    private readonly IErpNopUserService _nopUserService;
    private readonly IErpShipToAddressService _erpShipToAddressService;
    private readonly IErpSalesOrgService _erpSalesOrgService;
    private readonly B2BB2CFeaturesSettings _b2BB2CFeaturesSettings;
    private readonly IErpIntegrationPluginManager _erpIntegrationPluginManager;
    private readonly IErpLogsService _erpLogsService;
    private readonly IErpWorkflowMessageService _erpWorkflowMessageService;
    private readonly IShippingService _shippingService;

    #endregion Fields

    #region Ctor

    public ErpShippingService(IErpAccountService erpAccountService,
        IErpNopUserService nopUserService,
        IErpShipToAddressService erpShipToAddressService,
        IErpSalesOrgService erpSalesOrgService,
        B2BB2CFeaturesSettings b2BB2CFeaturesSettings,
        IErpIntegrationPluginManager erpIntegrationPluginManager,
        IErpLogsService erpLogsService,
        IErpWorkflowMessageService erpWorkflowMessageService,
        IShippingService shippingService)
    {
        _erpAccountService = erpAccountService;
        _nopUserService = nopUserService;
        _erpShipToAddressService = erpShipToAddressService;
        _erpSalesOrgService = erpSalesOrgService;
        _b2BB2CFeaturesSettings = b2BB2CFeaturesSettings;
        _erpIntegrationPluginManager = erpIntegrationPluginManager;
        _erpLogsService = erpLogsService;
        _erpWorkflowMessageService = erpWorkflowMessageService;
        _shippingService = shippingService;
    }

    #endregion Ctor

    #region Utilities

    private async Task<decimal?> GetShippingCostFromERPAsync(decimal totalWeightInKgs, Customer customer, ErpShipToAddress erpShipToAddress = null, Address nopAddress = null)
    {
        if (customer == null || (erpShipToAddress == null && nopAddress == null))
            return null;

        var erpNopUser = await _nopUserService.GetErpNopUserByCustomerIdAsync(customer.Id, showHidden: false);

        if (erpNopUser == null || erpNopUser.ErpUserType == ErpUserType.B2BUser)
            return null;

        var erpAccount = await _erpAccountService.GetErpAccountByIdAsync(erpNopUser.ErpAccountId);

        if (erpAccount == null)
            return null;

        var shipToAddress = erpShipToAddress ?? await _erpShipToAddressService.GetErpShipToAddressByNopAddressIdAsync(nopAddress.Id);
        if (shipToAddress == null)
        {
            await _erpLogsService.InsertErpLogAsync(
                ErpLogLevel.Error,
                ErpSyncLevel.Order,
                $"GetShippingCostFromERP: Ship To Address not found for Nop Address Id: {nopAddress.Id}, Customer: {customer.Email} (Id: {customer.Id}), Account Number: {erpAccount.AccountNumber} (Id: {erpAccount.Id})"
                );
            return null;
        }

        var accountSalesOrg = await _erpSalesOrgService.GetErpSalesOrgByIdAsync(erpAccount.ErpSalesOrgId);
        if (accountSalesOrg == null)
        {
            await _erpLogsService.InsertErpLogAsync(
                ErpLogLevel.Error,
                ErpSyncLevel.Order,
                $"GetShippingCostFromERP: Sales Org not found for Sales Org Id: {erpAccount.ErpSalesOrgId}, Customer: {customer.Email} (Id: {customer.Id}), Account Number: {erpAccount.AccountNumber} (Id: {erpAccount.Id})"
                );
            await _erpLogsService.ErrorAsync($"GetShippingCostFromERP: Sales Org not found.", ErpSyncLevel.Order);
            return null;
        }

        var erpIntegrationPlugin = await _erpIntegrationPluginManager.LoadActiveERPIntegrationPlugin();
        if (erpIntegrationPlugin is null)
        {
            await _erpLogsService.InsertErpLogAsync(
                ErpLogLevel.Error,
                ErpSyncLevel.ShipToAddress,
                $"GetShippingCostFromERP: No integration plugin found to get Shipping Rate for Customer: {customer.Email} (Id: {customer.Id}), Account Number: {erpAccount.AccountNumber} (Id: {erpAccount.Id})"
                );
            return null;
        }

        try
        {
            var shippingRateModel = await erpIntegrationPlugin.GetShippingRateFromERPAsync(new ErpGetRequestModel
            {
                Distance = erpShipToAddress.DistanceToNearestWareHouse.ToString(),
                Location = accountSalesOrg.Code,
                Weight = $"{totalWeightInKgs}"
            });

            if (shippingRateModel == null)
            {
                if (_b2BB2CFeaturesSettings.EnableLogOnErpCall)
                {
                    await _erpLogsService.ErrorAsync($"SAP ERP Integration: Error occured in GetShippingCostFromERP. " +
                        $"Response data not found for Customer: {customer.Email}," +
                        $"Account Number: {erpAccount.AccountNumber}, Sales Org {accountSalesOrg.Code}," +
                        $"Distance: {erpShipToAddress.DistanceToNearestWareHouse}, Weight: {totalWeightInKgs}", ErpSyncLevel.Order);
                }

                await _erpWorkflowMessageService.SendOrderOrDeliveryDatesOrShippingCostBAPIFailedMessageAsync(customer, (int)ERPFailedTypes.ShippingCostFails, 0);
                return null;
            }

            if (_b2BB2CFeaturesSettings.EnableLogOnErpCall)
            {
                await _erpLogsService.InformationAsync($"SAP ERP Integration: " +
                    $"GetShippingCostFromERP for customer: {customer.Email}, Account Number: {erpAccount.AccountNumber}" +
                    $", Sales Org {accountSalesOrg.Code}", ErpSyncLevel.Order);
            }

            if (decimal.TryParse(shippingRateModel.ShippingRate, out var shippingRate))
            {
                return shippingRate;
            }
            return null;
        }
        catch (Exception ex)
        {
            await _erpWorkflowMessageService.SendOrderOrDeliveryDatesOrShippingCostBAPIFailedMessageAsync(customer, (int)ERPFailedTypes.ShippingCostFails, 0);
            if (_b2BB2CFeaturesSettings.EnableLogOnErpCall)
            {
                await _erpLogsService.ErrorAsync($"SAP ERP Integration: Error occured in GetShippingCostFromERP. " +
                    $"Response data not found for Customer: {customer.Email}," +
                    $"Account Number: {erpAccount.AccountNumber}, Sales Org {accountSalesOrg.Code}," +
                    $"Distance: {erpShipToAddress.DistanceToNearestWareHouse}, Weight: {totalWeightInKgs}", ErpSyncLevel.Order);
            }
        }

        return null;
    }

    #endregion Utils

    #region Methods

    public async Task<decimal?> GetB2CShippingCostAsync(IList<ShoppingCartItem> cart, Customer customer, ErpShipToAddress erpShipToAddress)
    {
        if (cart == null || customer == null || erpShipToAddress == null)
            return null;

        // calculate total weight of items in the cart
        var cartItemsWeightInKG = (
            await Task.WhenAll(cart.Select(async item =>
            {
                return (await _shippingService.GetShoppingCartItemWeightAsync(item)) * item.Quantity;
            }))
        ).Sum();

        var totalWeightInKgs = cartItemsWeightInKG != 0 ? cartItemsWeightInKG : 0.00M;

        return await GetShippingCostFromERPAsync(
            totalWeightInKgs,
            customer,
            erpShipToAddress
        );
    }

    #endregion Methods
}