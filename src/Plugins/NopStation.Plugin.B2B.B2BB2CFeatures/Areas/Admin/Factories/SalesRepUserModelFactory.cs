using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Common;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models.SalesRepUser;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Factories;

public class SalesRepUserModelFactory : ISalesRepUserModelFactory
{
    #region Fields

    private readonly ICustomerService _customerService;
    private readonly IErpAccountService _erpAccountService;
    private readonly IDateTimeHelper _dateTimeHelper;
    private readonly IErpSalesRepService _erpSalesRepService;
    private readonly IErpSalesOrgService _erpSalesOrgService;
    private readonly IAddressService _addressService;
    private readonly IAddressModelFactory _addressModelFactory;
    private readonly IErpSalesOrgModelFactory _erpSalesOrgModelFactory;
    private readonly IErpUserFavouriteService _erpUserFavouriteService;
    private readonly ILocalizationService _localizationService;

    #endregion

    #region Ctor

    public SalesRepUserModelFactory(ICustomerService customerService,
        IErpAccountService erpAccountService,
        IDateTimeHelper dateTimeHelper,
        IErpSalesRepService erpSalesRepService,
        IErpSalesOrgService erpSalesOrgService,
        IAddressService addressService,
        IAddressModelFactory addressModelFactory,
        IErpSalesOrgModelFactory erpSalesOrgModelFactory,
        IErpUserFavouriteService erpUserFavouriteService,
        ILocalizationService localizationService)
    {
        _customerService = customerService;
        _erpAccountService = erpAccountService;
        _dateTimeHelper = dateTimeHelper;
        _erpSalesRepService = erpSalesRepService;
        _erpSalesOrgService = erpSalesOrgService;
        _addressService = addressService;
        _addressModelFactory = addressModelFactory;
        _erpSalesOrgModelFactory = erpSalesOrgModelFactory;
        _erpUserFavouriteService = erpUserFavouriteService;
        _localizationService = localizationService;
    }

    #endregion

    #region Methods

    public async Task<SalesRepUserSearchModel> PrepareSalesRepUserSearchModelAsync(SalesRepUserSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        // Prepare available sales organizations
        var salesOrgs = await _erpSalesOrgService.GetAllErpSalesOrgAsync();
        searchModel.AvailableSalesOrgs = salesOrgs.Select(x => new SelectListItem
        {
            Text = $"{x.Name} ({x.Code})",
            Value = $"{x.Id}"
        }).ToList();
        searchModel.AvailableSalesOrgs.Insert(0, new SelectListItem 
        { 
            Text = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpNopUserSearchModel.ShowAll"),
            Value = "0" 
        });

        // Prepare active status options
        searchModel.AvailableActiveOptions = new List<SelectListItem>
        {
            new () { 
                Text = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpNopUserSearchModel.ShowAll"), 
                Value = "0" 
            },
            new () {
                Text = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpNopUserSearchModel.ShowOnlyActive"), 
                Value = "1"
            },
            new () {
                Text = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpNopUserSearchModel.ShowOnlyInactive"), 
                Value = "2"
            },
        };

        searchModel.SetGridPageSize();

        return searchModel;
    }

    public async Task<SalesRepUserListModel> PrepareSalesRepUserListModel(SalesRepUserSearchModel searchModel, ErpSalesRep erpSalesRep)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        var erpNopUsers = await _erpSalesRepService.GetAllSalesRepUsersAsync(
            salesRepId: erpSalesRep.Id,
            salesRepCustomerId: erpSalesRep.NopCustomerId,
            salesRepTypeId: erpSalesRep.SalesRepTypeId,
            erpAccontNo: searchModel.SearchERPAccountNumber,
            accountName: searchModel.SearchERPAccountName,
            email: searchModel.SearchCustomerEmail,
            salesOrgId: searchModel.SearchSalesOrgId,
            isActive: searchModel.SearchActiveId == 0 ? null : searchModel.SearchActiveId == 1,
            pageIndex: searchModel.Page - 1,
            pageSize: searchModel.PageSize);

        var favouriteNopUsers = await _erpUserFavouriteService.GetErpUserFavouriteIdsByErpSalesRepCustomerIdAsync(erpSalesRep.NopCustomerId);

        var model = await new SalesRepUserListModel().PrepareToGridAsync(searchModel, erpNopUsers, () =>
        {
            return erpNopUsers.SelectAwait(async user =>
            {
                var customer = await _customerService.GetCustomerByIdAsync(user.NopCustomerId);
                var erpAccount = await _erpAccountService.GetErpAccountByIdAsync(user.ErpAccountId);

                var userModel = new SalesRepUserModel
                {
                    Id = user.Id,
                    NopCustomerId = user.NopCustomerId,
                    CustomerFullName = await _customerService.GetCustomerFullNameAsync(customer),
                    CustomerEmail = customer.Email,
                    ErpShipToAddressId = user.ErpShipToAddressId,
                    CreatedOnUtc = user.CreatedOnUtc,
                    IsActive = user.IsActive,
                    ErpUserType = $"{(ErpUserType)user.ErpUserTypeId}",
                    IsFavourite = favouriteNopUsers.Contains(user.Id)
                };

                if (erpAccount != null)
                {
                    userModel.ErpAccountId = user.ErpAccountId;
                    userModel.ErpAccountNumber = erpAccount.AccountNumber;
                    userModel.ErpAccountName = erpAccount.AccountName;

                    var salesOrg = await _erpSalesOrgService.GetErpSalesOrgByIdAsync(erpAccount.ErpSalesOrgId);
                    if (salesOrg != null)
                    {
                        userModel.ErpSalesOrgName = $"{salesOrg.Name} ({salesOrg.Code})";
                    }
                }

                return userModel;
            }).OrderByDescending(x => x.IsFavourite);
        });

        return model;
    }

    public async Task<ErpAccountListModel> PrepareSalesRepErpUserListModelForSalesRep(ErpAccountSearchModel searchModel, ErpSalesRep erpSalesRep)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        var erpAccountIdMaps = 
            (await _erpAccountService.GetAllErpAccountsBySalesRepIdAsync(erpSalesRepId: searchModel.ErpAccountId))
            .ToPagedList(searchModel);

        var erpAccounts = await _erpAccountService.GetErpAccountListAsync(showHidden: false);
        var erpSalesOrgs = await _erpSalesOrgService.GetErpSalesOrgsAsync();

        var model = await new ErpAccountListModel().PrepareToGridAsync(searchModel, erpAccountIdMaps, () =>
        {
            return erpAccountIdMaps.SelectAwait(async erpIdMap =>
            {
                var erpAccount = erpAccounts.FirstOrDefault(x => x.Id == erpIdMap.ErpAccountId);

                if (erpAccount == null)
                    return null;

                var address = await _addressService.GetAddressByIdAsync(erpAccount.BillingAddressId ?? 0);
                var addressModel = new AddressModel();
                if (address != null)
                    addressModel = address.ToModel(addressModel);
                await _addressModelFactory.PrepareAddressModelAsync(addressModel, address);

                var erpAccountModel = new ErpAccountModel
                {
                    Id = erpAccount.Id,
                    AccountNumber = erpAccount.AccountNumber,
                    AccountName = erpAccount.AccountName,
                    VatNumber = erpAccount.VatNumber,
                    CurrentBalance = erpAccount.CurrentBalance,
                    ErpSalesOrgId = erpAccount.ErpSalesOrgId,
                    BillingAddressId = erpAccount.BillingAddressId,
                    BillingAddress = addressModel,
                    BillingSuburb = erpAccount.BillingSuburb,
                    CreditLimit = erpAccount.CreditLimit,
                    CreditLimitAvailable = erpAccount.CreditLimitAvailable,
                    LastPaymentAmount = erpAccount.LastPaymentAmount,
                    LastPaymentDate = erpAccount.LastPaymentDate,
                    AllowOverspend = erpAccount.AllowOverspend,
                    PreFilterFacets = erpAccount.PreFilterFacets,
                    PaymentTypeCode = erpAccount.PaymentTypeCode,
                    OverrideAddressEditOnCheckoutConfigSetting = erpAccount.OverrideAddressEditOnCheckoutConfigSetting,
                    OverrideBackOrderingConfigSetting = erpAccount.OverrideBackOrderingConfigSetting,
                    AllowAccountsAddressEditOnCheckout = erpAccount.AllowAccountsAddressEditOnCheckout,
                    AllowAccountsBackOrdering = erpAccount.AllowAccountsBackOrdering,
                    OverrideStockDisplayFormatConfigSetting = erpAccount.OverrideStockDisplayFormatConfigSetting,
                    ErpAccountStatusTypeId = erpAccount.ErpAccountStatusTypeId,
                    ErpAccountStatusType = $"{(ErpAccountStatusType)erpAccount.ErpAccountStatusTypeId}",
                    LastErpAccountSyncDate = erpAccount.LastErpAccountSyncDate,
                    B2BPriceGroupCodeId = erpAccount.B2BPriceGroupCodeId,
                    TotalSavingsForthisYear = erpAccount.TotalSavingsForthisYear ?? 0,
                    TotalSavingsForAllTime = erpAccount.TotalSavingsForAllTime ?? 0,
                    TotalSavingsForAllTimeUpdatedOnUtc = erpAccount.TotalSavingsForAllTimeUpdatedOnUtc,
                    TotalSavingsForthisYearUpdatedOnUtc = erpAccount.TotalSavingsForthisYearUpdatedOnUtc,
                    LastTimeOrderSyncOnUtc = erpAccount.LastTimeOrderSyncOnUtc,
                    CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(erpAccount.CreatedOnUtc, DateTimeKind.Utc),
                    UpdatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(erpAccount.UpdatedOnUtc, DateTimeKind.Utc),
                    IsActive = erpAccount.IsActive,
                    StockDisplayFormatType = await _localizationService.GetLocalizedEnumAsync(erpAccount.StockDisplayFormatType),
                    StockDisplayFormatTypeId = erpAccount.StockDisplayFormatTypeId
                };

                var erpAccountSalesOrgInfo = erpSalesOrgs.FirstOrDefault(x => x.Id == erpAccount.ErpSalesOrgId);
                if (erpAccountSalesOrgInfo != null)
                {
                    erpAccountModel.ErpSalesOrgName = $"{erpAccountSalesOrgInfo.Name} - ({erpAccountSalesOrgInfo.Code})";
                }

                return erpAccountModel;
            }).Where(x => x != null);
        });

        return model;
    }

    #endregion
}