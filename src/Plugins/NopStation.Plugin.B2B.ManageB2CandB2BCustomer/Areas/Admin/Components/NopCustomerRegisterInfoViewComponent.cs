using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Shipping;
using Nop.Web.Areas.Admin.Models.Customers;
using Nop.Web.Framework.Components;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpWorkflowMessage;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services.ErpUserRegistrationInfoService;
using NopStation.Plugin.B2B.ManageB2CandB2BCustomer.Areas.Admin.Models;

namespace NopStation.Plugin.B2B.ManageB2CandB2BCustomer.Areas.Admin.Components;
public class NopCustomerRegisterInfoViewComponent : NopViewComponent
{
    private readonly IErpUserRegistrationInfoService _erpUserRegistrationInfoService;
    private readonly IErpSalesOrgService _erpSalesOrgService;
    private readonly IErpAccountService _erpAccountService;
    private readonly IShippingService _shippingService;
    private readonly IGenericAttributeService _genericAttributeService;
    private readonly ICustomerService _customerService;

    public NopCustomerRegisterInfoViewComponent(
        IErpUserRegistrationInfoService erpUserRegistrationInfoService,
        IErpSalesOrgService erpSalesOrgService,
        IErpAccountService erpAccountService,
        IShippingService shippingService,
        IGenericAttributeService genericAttributeService,
        ICustomerService customerService)
    {
        _erpUserRegistrationInfoService = erpUserRegistrationInfoService;
        _erpSalesOrgService = erpSalesOrgService;
        _erpAccountService = erpAccountService;
        _shippingService = shippingService;
        _genericAttributeService = genericAttributeService;
        _customerService = customerService;
    }
    public async Task<List<ErpSalesOrg>> GetSalesOrganisationsBySplitIdAsync(string erpSalesOrganisationIds)
    {
        if (string.IsNullOrEmpty(erpSalesOrganisationIds))
        {
            return null;
        }

        var salesOrganizationIds = erpSalesOrganisationIds.Split('\u002C').Select(Int32.Parse).ToArray();

        if (salesOrganizationIds.Length == 0)
            return null;

        var salesOrganization = await _erpSalesOrgService.GetErpSalesOrganisationsByIdsAsync(salesOrganizationIds);

        if (salesOrganization == null || salesOrganization.Count == 0)
            return null;

        return salesOrganization.ToList();
    }

    public async Task<IViewComponentResult> InvokeAsync(RouteValueDictionary data)
    {
        data.TryGetValue("additionalData", out var customerModelData);

        if (!(customerModelData is CustomerModel customerModel))
        {
            return Content(string.Empty);
        }

        var registerInfoModel = new RegistrationInfoModel();
        if (customerModel.Id > 0)
        {
            var erpUserRegistrationInfo = await _erpUserRegistrationInfoService.GetErpUserRegistrationInfoByCustomerIdAsync(customerModel.Id);
            if (erpUserRegistrationInfo!=null)
            {
                if(erpUserRegistrationInfo.ErpUserType == ErpUserType.B2BUser)
                {
                    registerInfoModel.NopCustomerId = customerModel.Id;
                    registerInfoModel.AccountNumber = erpUserRegistrationInfo.ErpAccountNumber;
                    registerInfoModel.B2BSalesOrganisationIds = erpUserRegistrationInfo.ErpSalesOrganisationIds;

                    var salsesOrganizations = await GetSalesOrganisationsBySplitIdAsync(erpUserRegistrationInfo.ErpSalesOrganisationIds);
                    registerInfoModel.SalesOrganisationCode = salsesOrganizations == null ? string.Empty : string.Join(',', salsesOrganizations.Select(x => x.Code));
                    registerInfoModel.SalesOrganisationName = salsesOrganizations == null ? string.Empty : string.Join(", ", salsesOrganizations.Select(x => x.Name));

                    var customer = await _customerService.GetCustomerByIdAsync(customerModel.Id);
                    var customerJobTitle = customer != null ? await _genericAttributeService.GetAttributeAsync<string>(customer, ErpWorkflowMessageService.JobTitleAttribute) : "";
                    registerInfoModel.JobTitle = customerJobTitle;

                    registerInfoModel.SpecialInstructions = erpUserRegistrationInfo.SpecialInstructions ?? string.Empty;
                    registerInfoModel.QueuedEmailInfo = erpUserRegistrationInfo.QueuedEmailInfo ?? string.Empty;
                    registerInfoModel.PersonalAlternateContactNumber = erpUserRegistrationInfo.PersonalAlternateContactNumber ?? string.Empty;
                    registerInfoModel.AuthorisationFullName = erpUserRegistrationInfo.AuthorisationFullName ?? string.Empty;
                    registerInfoModel.AuthorisationContactNumber = erpUserRegistrationInfo.AuthorisationContactNumber ?? string.Empty;
                    registerInfoModel.AuthorisationAlternateContactNumber = erpUserRegistrationInfo.AuthorisationAlternateContactNumber ?? string.Empty;
                    registerInfoModel.AuthorisationJobTitle = erpUserRegistrationInfo.AuthorisationJobTitle ?? string.Empty;
                    registerInfoModel.AuthorisationAdditionalComment = erpUserRegistrationInfo.AuthorisationAdditionalComment ?? string.Empty;
                    registerInfoModel.IsB2BRegistrationInfo = true;
                }
                if (erpUserRegistrationInfo.ErpUserType == ErpUserType.B2CUser)
                {
                    registerInfoModel = new RegistrationInfoModel()
                    {
                        NopCustomerId = erpUserRegistrationInfo.Id,
                        NearestWarehouseId = erpUserRegistrationInfo.NearestWareHouseId,
                        DistanceToNearestWarehouse = (double)(erpUserRegistrationInfo.DistanceToNearestWarehouse ?? 0m),
                        DeliveryOption = ((DeliveryOption)erpUserRegistrationInfo.DeliveryOptionId).ToString(),
                        Latitude = erpUserRegistrationInfo.Latitude,
                        Longitude = erpUserRegistrationInfo.Longitude,
                        AddressId = erpUserRegistrationInfo.AddressId,
                        HouseNumber = erpUserRegistrationInfo.HouseNumber,
                        City = erpUserRegistrationInfo.City,
                        Suburb = erpUserRegistrationInfo.Suburb,
                        Street = erpUserRegistrationInfo.Street,
                        PostalCode = erpUserRegistrationInfo.PostalCode,
                        Country = erpUserRegistrationInfo.Country,
                        SalesOrganisationId = erpUserRegistrationInfo.ErpSalesOrgId,
                        B2BAccountIdForB2C = erpUserRegistrationInfo.ErpAccountIdForB2C,
                        B2CUserId = erpUserRegistrationInfo.ErpUserId,
                        ErrorMessage = erpUserRegistrationInfo.ErrorMessage
                    };

                    var warehouse = await _shippingService.GetWarehouseByIdAsync(erpUserRegistrationInfo.NearestWareHouseId);
                    var salesOrg = await _erpSalesOrgService.GetErpSalesOrgByIdAsync(erpUserRegistrationInfo.ErpSalesOrgId);
                    registerInfoModel.WarehouseName = warehouse?.Name;
                    var account = await _erpAccountService.GetErpAccountByIdAsync(erpUserRegistrationInfo.ErpAccountIdForB2C);
                    registerInfoModel.B2BAccountName = account?.AccountName;
                    registerInfoModel.SalesOrganisationName = salesOrg?.Name;
                    registerInfoModel.SalesOrganisationCode = salesOrg?.Code;
                    registerInfoModel.CustomerEmail = customerModel.Email;
                }
            }
            
        }

        return View(registerInfoModel);
    }
}