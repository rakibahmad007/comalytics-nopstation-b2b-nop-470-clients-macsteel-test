using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Services.Common;
using Nop.Services.Directory;
using Nop.Web.Factories;
using Nop.Web.Framework.Components;
using NopStation.Plugin.B2B.B2BB2CFeatures;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;
using NopStation.Plugin.B2B.ManageB2CandB2BCustomer.Models;

namespace NopStation.Plugin.B2B.ManageB2CandB2BCustomer.Components;

public class B2CShipToAddressSelectorViewComponent(
    IErpShipToAddressService erpShipToAddressService,
    IErpNopUserService erpNopUserService,
    IWorkContext workContext,
    IAddressService addressService,
    IStateProvinceService stateProvinceService,
    ICountryService countryService,
    IErpAccountService erpAccountService,
    ICustomerModelFactory customerModelFactory,
    B2BB2CFeaturesSettings b2BB2CFeaturesSettings) : NopViewComponent
{

    #region Fields

    private readonly IErpShipToAddressService _erpShipToAddressService = erpShipToAddressService;
    private readonly IErpNopUserService _erpNopUserService = erpNopUserService;
    private readonly IWorkContext _workContext = workContext;
    private readonly IAddressService _addressService = addressService;
    private readonly IStateProvinceService _stateProvinceService = stateProvinceService;
    private readonly ICountryService _countryService = countryService;
    private readonly IErpAccountService _erpAccountService = erpAccountService;
    private readonly ICustomerModelFactory _customerModelFactory = customerModelFactory;
    private readonly B2BB2CFeaturesSettings _b2BB2CFeaturesSettings = b2BB2CFeaturesSettings;

    #endregion

    #region Methods

    public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
    {
        var currCustomer = await _workContext.GetCurrentCustomerAsync();
        var erpUser = await _erpNopUserService.GetErpNopUserByCustomerIdAsync(currCustomer.Id, showHidden: false);

        if (erpUser == null || erpUser.ErpUserType != ErpUserType.B2CUser)
            return Content(string.Empty);

        var erpAccount = await _erpAccountService.GetActiveErpAccountByCustomerIdAsync(currCustomer.Id);
        if (erpAccount == null)
            return Content(string.Empty);

        var model = new B2CShipToAddressSelectorModel();
        IList<ErpShipToAddress> erpShipToAddresses;

        if (_b2BB2CFeaturesSettings.UseDefaultAccountForB2CUser)
        {
            erpShipToAddresses = await _erpShipToAddressService.GetErpShipToAddressesByCustomerAddressesAsync(
                                                            customerId: currCustomer.Id,
                                                            erpShipToAddressCreatedByTypeId: (int)ErpShipToAddressCreatedByType.User);
        }
        else
        {
            erpShipToAddresses = await _erpShipToAddressService.GetErpShipToAddressesByErpAccountIdAsync(erpUser.ErpAccountId);
        }

        foreach (var shipto in erpShipToAddresses)
        {
            var nopAddress = await _addressService.GetAddressByIdAsync(shipto.AddressId);
            var stateProvince = await _stateProvinceService.GetStateProvinceByIdAsync(nopAddress?.StateProvinceId ?? 0);
            var country = await _countryService.GetCountryByIdAsync(nopAddress?.CountryId ?? 0);

            model.AvailableShipToAddresses.Add(new B2CShipToAddressModel
            {
                Id = shipto.Id,
                HouseNumber = nopAddress?.Address1 ?? string.Empty,
                Street = nopAddress?.Address2 ?? string.Empty,
                Suburb = shipto.Suburb,
                City = nopAddress?.City ?? string.Empty,
                StateProvince = stateProvince?.Name ?? string.Empty,
                StateProvinceCode = stateProvince?.Abbreviation ?? string.Empty,
                PostalCode = nopAddress?.ZipPostalCode ?? string.Empty,
                Country = country?.Name ?? string.Empty,
                IsSelected = erpUser.ErpShipToAddressId == shipto.Id ? 1 : 0
            });

            if (erpUser.ErpShipToAddressId == shipto.Id && shipto.DeliveryOptionId == (int)DeliveryOption.NoShop)
            {
                model.DefaultAddressChanged = 1;
            }
        }

        model.ErpNopUserId = erpUser.Id;
        model.SelectedErpShipToAddressId = erpUser.ErpShipToAddressId;
        model.AvailableShipToAddresses = model.AvailableShipToAddresses.OrderByDescending(x => x.IsSelected).ThenBy(x => x.CreatedOnUtc).ToList();
        model.NextDefaultErpShipToAddressId = model.AvailableShipToAddresses?.Count >= 2 ? model.AvailableShipToAddresses[1].Id : 0;

        var defaultShipToAddressErpAccountMapping = await _erpShipToAddressService.GetErpShipToAddressErpAccountMapByErpShipToAddressIdAsync(erpUser.ErpShipToAddressId);
        var erpAccountOfDefaultShipToAddress = await _erpAccountService.GetErpAccountByIdAsync(defaultShipToAddressErpAccountMapping?.ErpAccountId ?? 0);

        var nextDefaultShipToAddressErpAccountMapping = await _erpShipToAddressService.GetErpShipToAddressErpAccountMapByErpShipToAddressIdAsync(model.NextDefaultErpShipToAddressId);
        var erpAccountOfNextDefaultShipToAddress = await _erpAccountService.GetErpAccountByIdAsync(nextDefaultShipToAddressErpAccountMapping?.ErpAccountId ?? 0);

        model.IsSalesOrgDifferent = erpAccountOfDefaultShipToAddress?.ErpSalesOrgId != erpAccountOfNextDefaultShipToAddress?.ErpSalesOrgId;

        return View(model);
    }
    #endregion

}
