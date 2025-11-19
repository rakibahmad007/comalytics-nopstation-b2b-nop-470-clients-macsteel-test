using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using NNopStation.Plugin.B2B.B2BB2CFeatures.Services.SpecialIncludeExcludeService;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Localization;
using Nop.Data;
using Nop.Services;
using Nop.Services.Common;
using Nop.Services.ExportImport.Help;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Common;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpAccountCreditSyncFunctionality;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Factories;

public class ErpAccountModelFactory : IErpAccountModelFactory
{
    #region Fields

    private readonly IDateTimeHelper _dateTimeHelper;
    private readonly IAddressService _addressService;
    private readonly IAddressModelFactory _addressModelFactory;
    private readonly AddressSettings _addressSettings;
    private readonly ILocalizationService _localizationService;
    private readonly IErpAccountService _erpAccountService;
    private readonly IErpSalesOrgService _erpSalesOrgService;
    private readonly IErpSalesOrgModelFactory _erpSalesOrgModelFactory;
    private readonly IErpGroupPriceCodeService _erpGroupPriceCodeService;
    private readonly IErpNopUserModelFactory _erpNopUserModelFactory;
    private readonly IErpShipToAddressModelFactory _erpShipToAddressModelFactory;
    private readonly IErpShipToAddressService _erpShipToAddressService;
    private readonly IErpAccountCreditSyncFunctionality _erpAccountCreditSyncFunctionality; 
    private readonly IErpNopUserService _erpNopUserService;
    private readonly IWorkContext _workContext;
    private readonly IB2BExportImportManager _b2BExportImportManager;
    private readonly CatalogSettings _catalogSettings;

    #endregion

    #region Ctor

    public ErpAccountModelFactory(IDateTimeHelper dateTimeHelper,
        IAddressService addressService,
        IAddressModelFactory addressModelFactory,
        AddressSettings addressSettings,
        ILocalizationService localizationService,
        IErpAccountService erpAccountService,
        IErpSalesOrgService erpSalesOrgService,
        IErpSalesOrgModelFactory erpSalesOrgModelFactory,
        IErpGroupPriceCodeService erpGroupPriceCodeService,
        IErpNopUserModelFactory erpNopUserModelFactory,
        IErpShipToAddressModelFactory erpShipToAddressModelFactory,
        IErpShipToAddressService erpShipToAddressService,
        IErpAccountCreditSyncFunctionality erpAccountCreditSyncFunctionality,
        IErpNopUserService erpNopUserService,
        IWorkContext workContext,
        IB2BExportImportManager b2BExportImportManager,
        CatalogSettings catalogSettings)
    {
        _localizationService = localizationService;
        _dateTimeHelper = dateTimeHelper;
        _addressService = addressService;
        _addressModelFactory = addressModelFactory;
        _addressSettings = addressSettings;
        _erpAccountService = erpAccountService;
        _erpSalesOrgService = erpSalesOrgService;
        _erpSalesOrgModelFactory = erpSalesOrgModelFactory;
        _erpGroupPriceCodeService = erpGroupPriceCodeService;
        _erpNopUserModelFactory = erpNopUserModelFactory;
        _erpShipToAddressModelFactory = erpShipToAddressModelFactory;
        _erpShipToAddressService = erpShipToAddressService;
        _erpAccountCreditSyncFunctionality = erpAccountCreditSyncFunctionality;
        _erpNopUserService = erpNopUserService;
        _workContext = workContext;
        _b2BExportImportManager = b2BExportImportManager;
        _catalogSettings = catalogSettings;
    }

    #endregion

    #region Utilities

    protected virtual void SetAddressFieldsAsRequired(AddressModel model)
    {
        model.FirstNameRequired = true;
        model.EmailRequired = true;
        model.CountryRequired = true;
        model.CityRequired = true;
        model.PhoneRequired = true;
        model.ZipPostalCodeRequired = true;

        model.CompanyRequired = _addressSettings.CompanyRequired;
        model.CountyRequired = _addressSettings.CountyRequired;
        model.StreetAddressRequired = _addressSettings.StreetAddressRequired;
        model.StreetAddress2Required = _addressSettings.StreetAddress2Required;
        model.FaxRequired = _addressSettings.FaxRequired;
    }

    #endregion

    #region Method

    public async Task<ErpAccountSearchModel> PrepareErpAccountSearchModelAsync(ErpAccountSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        var availableErpAccountStatusTypes = await ErpAccountStatusType.Normal.ToSelectListAsync(false);
        foreach (var types in availableErpAccountStatusTypes)
        {
            searchModel.ErpAccountStatusTypes.Add(types);
        }
        searchModel.ErpAccountStatusTypes.Insert(0, new SelectListItem
        {
            Value = "0",
            Text = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpAccount.Select")
        });

        searchModel.AvailableErpSalesOrgs = (await _erpSalesOrgService.GetAllErpSalesOrgAsync(showHidden: false))
            .Select(erpSalesOrg => new SelectListItem
            {
                Value = $"{erpSalesOrg.Id}",
                Text = $"{erpSalesOrg.Name} - ({erpSalesOrg.Code})"
            }).ToList();

        searchModel.AvailableErpSalesOrgs.Insert(0, new SelectListItem
        {
            Value = "0",
            Text = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpAccount.Select")
        });

        //prepare "active" filter (0 - all; 1 - active only; 2 - inactive only)
        searchModel.ShowInActiveOption.Add(new SelectListItem
        {
            Value = "0",
            Text = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpAccountSearchModel.ShowAll"),
        });
        searchModel.ShowInActiveOption.Add(new SelectListItem
        {
            Value = "1",
            Text = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpAccountSearchModel.ShowOnlyActive"),
        });
        searchModel.ShowInActiveOption.Add(new SelectListItem
        {
            Value = "2",
            Text = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpAccountSearchModel.ShowOnlyInactive"),
        });
        searchModel.ShowInActive = 1;

        //prepare grid
        searchModel.SetGridPageSize();

        return searchModel;
    }

    public async Task<ErpAccountListModel> PrepareErpAccountListModelAsync(ErpAccountSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        var erpAccounts = await _erpAccountService.GetAllErpAccountsAsync(
            pageIndex: searchModel.Page - 1,
            pageSize: searchModel.PageSize,
            showHidden: searchModel.ShowInActive == 0 ? null : (searchModel.ShowInActive == 2),
            erpAccountNo: searchModel.AccountNumber,
            salesOrgId: searchModel.ErpSalesOrgId,
            email: searchModel.Email,
            accountName: searchModel.AccountName,
            erpAccountStatusTypeId: searchModel.ErpAccountStatusTypeId);

        var erpSalesOrgs = await _erpSalesOrgService.GetErpSalesOrgsAsync();

        var model = await new ErpAccountListModel().PrepareToGridAsync(searchModel, erpAccounts, () =>
        {
            return erpAccounts.SelectAwait(async erpAccount =>
            {
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

                    erpAccountModel.ERPNopUserCount = (await _erpNopUserService.GetAllErpNopUsersAsync(accountId: erpAccount.Id,
                        salesOrgId: erpAccountSalesOrgInfo.Id,
                        showHidden: false,
                        getOnlyTotalCount: true)).TotalCount;

                    erpAccountModel.ShipToAddressCount = (await _erpShipToAddressService.GetAllErpShipToAddressesAsync(erpAccountId: erpAccount.Id,
                        salesOrgId: erpAccountSalesOrgInfo.Id,
                        showHidden: false,
                        getOnlyTotalCount: true)).TotalCount;
                }

                return erpAccountModel;
            });
        });

        return model;
    }

    public async Task<ErpAccountModel> PrepareErpAccountModelAsync(ErpAccountModel model, ErpAccount erpAccount)
    {
        if (erpAccount == null)
        {
            model.AvailableErpSalesOrgs = (await _erpSalesOrgService.GetAllErpSalesOrgAsync(showHidden: false))
              .Select(erpSalesOrg => new SelectListItem
              {
                  Value = $"{erpSalesOrg.Id}",
                  Text = $"{erpSalesOrg.Name} - ({erpSalesOrg.Code})",
              }).ToList();

            model.AvailableB2BPriceGroupCodes = (await _erpGroupPriceCodeService.GetAllErpGroupPriceCodesAsync())
                .Select(erpGroupPrice => new SelectListItem
                {
                    Value = $"{erpGroupPrice.Id}",
                    Text = $"{erpGroupPrice.Code}",
                }).ToList();

            model.AvailableB2BPriceGroupCodes.Insert(0, new SelectListItem
            {
                Value = "0",
                Text = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpAccount.Select")
            });

            // Prepare ErpAccountStatusTypes dropdown options
            var availableErpAccountStatusTypes = await ErpAccountStatusType.Normal.ToSelectListAsync(false);
            foreach (var types in availableErpAccountStatusTypes)
            {
                model.ErpAccountStatusTypes.Add(types);
            }
            model.ErpAccountStatusTypes.Insert(0, new SelectListItem
            {
                Value = "0",
                Text = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpAccount.Select")
            });

            model.IsActive = true;

            await _addressModelFactory.PrepareAddressModelAsync(model.BillingAddress, null);
            return model;
        }
        else
        {
            await _erpAccountCreditSyncFunctionality.LiveErpAccountCreditCheckAsync(erpAccount);

            //fill in model values from the entity
            model ??= new ErpAccountModel();

            model.Id = erpAccount.Id;
            model.AccountNumber = erpAccount.AccountNumber;
            model.AccountName = erpAccount.AccountName;
            model.VatNumber = erpAccount.VatNumber;
            model.CurrentBalance = erpAccount.CurrentBalance;
            model.ErpSalesOrgId = erpAccount.ErpSalesOrgId;
            model.BillingAddressId = erpAccount.BillingAddressId;
            model.BillingSuburb = erpAccount.BillingSuburb;
            model.CreditLimit = erpAccount.CreditLimit;
            model.CreditLimitAvailable = erpAccount.CreditLimitAvailable;
            model.LastPaymentAmount = erpAccount.LastPaymentAmount;
            model.LastPaymentDate = erpAccount.LastPaymentDate;
            model.AllowOverspend = erpAccount.AllowOverspend;
            model.PreFilterFacets = erpAccount.PreFilterFacets;
            model.PaymentTypeCode = erpAccount.PaymentTypeCode;
            model.SpecialIncludes = erpAccount.SpecialIncludes;
            model.SpecialExcludes = erpAccount.SpecialExcludes;
            model.OverrideAddressEditOnCheckoutConfigSetting = erpAccount.OverrideAddressEditOnCheckoutConfigSetting;
            model.OverrideBackOrderingConfigSetting = erpAccount.OverrideBackOrderingConfigSetting;
            model.AllowAccountsAddressEditOnCheckout = erpAccount.AllowAccountsAddressEditOnCheckout;
            model.AllowAccountsBackOrdering = erpAccount.AllowAccountsBackOrdering;
            model.OverrideStockDisplayFormatConfigSetting = erpAccount.OverrideStockDisplayFormatConfigSetting;
            model.ErpAccountStatusTypeId = erpAccount.ErpAccountStatusTypeId;
            model.ErpAccountStatusType = ((ErpAccountStatusType)erpAccount.ErpAccountStatusTypeId).ToString();
            model.LastErpAccountSyncDate = erpAccount.LastErpAccountSyncDate;
            model.B2BPriceGroupCodeId = erpAccount.B2BPriceGroupCodeId;
            model.TotalSavingsForthisYear = erpAccount.TotalSavingsForthisYear ?? 0;
            model.TotalSavingsForAllTime = erpAccount.TotalSavingsForAllTime ?? 0;
            model.TotalSavingsForAllTimeUpdatedOnUtc = erpAccount.TotalSavingsForAllTimeUpdatedOnUtc;
            model.TotalSavingsForthisYearUpdatedOnUtc = erpAccount.TotalSavingsForthisYearUpdatedOnUtc;
            model.LastTimeOrderSyncOnUtc = erpAccount.LastTimeOrderSyncOnUtc;
            model.CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(erpAccount.CreatedOnUtc, DateTimeKind.Utc);
            model.UpdatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(erpAccount.UpdatedOnUtc, DateTimeKind.Utc);
            model.IsActive = erpAccount.IsActive;
            model.StockDisplayFormatTypeId = erpAccount.StockDisplayFormatTypeId;
            model.StockDisplayFormatType = await _localizationService.GetLocalizedEnumAsync(erpAccount.StockDisplayFormatType);
            model.PercentageOfStockAllowed = erpAccount.PercentageOfStockAllowed ?? decimal.Zero;

            //Additional Info
            var erpAccountSalesOrgInfo = await _erpSalesOrgService.GetErpSalesOrgByIdAsync(erpAccount.ErpSalesOrgId);
            if (erpAccountSalesOrgInfo != null)
            {
                var erpAccountSalesOrgInfoModel = new ErpSalesOrgModel();

                model.ErpSalesOrgModel = await _erpSalesOrgModelFactory.PrepareErpSalesOrgModelAsync(erpAccountSalesOrgInfoModel, erpAccountSalesOrgInfo);
                model.ErpSalesOrgName = $"{model.ErpSalesOrgModel.Name} - ({model.ErpSalesOrgModel.Code})";
            }

            // Prepare ErpSalesOrgs dropdown options
            model.AvailableErpSalesOrgs = (await _erpSalesOrgService.GetAllErpSalesOrgAsync(showHidden: false))
            .Select(erpSalesOrg => new SelectListItem
            {
                Value = $"{erpSalesOrg.Id}",
                Text = $"{erpSalesOrg.Name} - ({erpSalesOrg.Code})",
            }).ToList();

            // Prepare B2BPriceGroupCodes dropdown options
            model.AvailableB2BPriceGroupCodes = (await _erpGroupPriceCodeService.GetAllErpGroupPriceCodesAsync())
            .Select(erpGroupPrice => new SelectListItem
            {
                Value = $"{erpGroupPrice.Id}",
                Text = $"{erpGroupPrice.Code}"
            }).ToList();

            model.AvailableB2BPriceGroupCodes.Insert(0, new SelectListItem
            {
                Value = "0",
                Text = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpAccount.Select")
            });

            // Prepare ErpAccountStatusTypes dropdown options
            var availableErpAccountStatusTypes = await ErpAccountStatusType.Normal.ToSelectListAsync(false);
            foreach (var types in availableErpAccountStatusTypes)
            {
                model.ErpAccountStatusTypes.Add(types);
            }
            model.ErpAccountStatusTypes.Insert(0, new SelectListItem
            {
                Value = "0",
                Text = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpAccount.Select")
            });

            //prepare address model
            var address = await _addressService.GetAddressByIdAsync(erpAccount.BillingAddressId ?? 0);
            var addressModel = new AddressModel();
            if (address != null)
                addressModel = address.ToModel(addressModel);

            await _addressModelFactory.PrepareAddressModelAsync(addressModel, address);
            SetAddressFieldsAsRequired(addressModel);
            model.BillingAddress = addressModel;

            //prepare nop user search Model
            model.ErpNopUserSearchModel.AccountId = model.Id;
            model.ErpNopUserSearchModel = await _erpNopUserModelFactory.PrepareErpNopUserSearchModelAsync(searchModel: model.ErpNopUserSearchModel);

            //prepare erp ship to address search model
            model.ErpShipToAddressSearchModel.SearchErpAccountId = model.Id;
            model.ErpShipToAddressSearchModel = await _erpShipToAddressModelFactory.PrepareErpShipToAddressSearchModelAsync(searchModel: model.ErpShipToAddressSearchModel);

            model.PaymentTermsCode = string.IsNullOrEmpty(erpAccount.PaymentTermsCode) ? "" : erpAccount.PaymentTermsCode;
            model.PaymentTermsDescription = string.IsNullOrEmpty(erpAccount.PaymentTermsDescription) ? "" : erpAccount.PaymentTermsDescription;

            return model;
        }
    }

    #endregion

    #region Export/Excel

    public async Task<byte[]> ExportAllErpAccountsToXlsxAsync(ErpAccountSearchModel searchModel)
    {
        var sql = new StringBuilder(@"
            SELECT account.[Id], account.[AccountNumber], account.[AccountName],
                   saleOrg.[Code] AS SalesOrgCode,
                   bAddress.FirstName AS BillingFirstName, bAddress.LastName AS BillingLastName,
                   bAddress.Email AS BillingEmail, bAddress.Company AS BillingCompany,
                   bCountry.[Name] AS BillingCountry, bStateProvince.[Name] AS BillingStateProvince,
                   bAddress.City AS BillingCity, bAddress.Address1 AS BillingAddress1,
                   bAddress.Address2 AS BillingAddress2, account.[BillingSuburb],
                   bAddress.ZipPostalCode AS BillingZipPostalCode, bAddress.PhoneNumber AS BillingPhoneNumber,
                   account.[VatNumber], account.[CreditLimit], account.[CurrentBalance], 
                   account.[CreditLimitAvailable], account.[AllowOverspend],
                   account.[TotalSavingsForthisYear], priceGroup.[Code] AS PriceGroupCode,
                   account.[PreFilterFacets], account.[PaymentTypeCode],
                   account.[OverrideBackOrderingConfigSetting], account.[AllowAccountsBackOrdering],
                   account.[OverrideAddressEditOnCheckoutConfigSetting], account.[AllowAccountsAddressEditOnCheckout],
                   account.[StockDisplayFormatTypeId], account.[ErpAccountStatusTypeId],
                   account.[PercentageOfStockAllowed], account.[LastErpAccountSyncDate],
                   account.[LastPriceRefresh], account.[IsActive], account.[IsDeleted],
                   account.[CreatedOnUtc], account.[CreatedById], account.[UpdatedOnUtc], account.[UpdatedById],
                   account.[IsDefaultPaymentAccount]
            FROM [dbo].[Erp_Account] account
            LEFT JOIN [dbo].[Erp_Sales_Org] saleOrg ON account.ErpSalesOrgId = saleOrg.Id
            LEFT JOIN [dbo].[Erp_Group_Price_Code] priceGroup ON account.B2BPriceGroupCodeId = priceGroup.Id
            LEFT JOIN [dbo].[Address] bAddress ON account.[BillingAddressId] = bAddress.[Id]
            LEFT JOIN [dbo].[Country] bCountry ON bAddress.CountryId = bCountry.[Id]
            LEFT JOIN [dbo].[StateProvince] bStateProvince ON bAddress.StateProvinceId = bStateProvince.[Id]
            WHERE 1 = 1
        ");

        // Append filters
        bool? showHidden = searchModel.ShowInActive == 0 ? null : (searchModel.ShowInActive == 2);
        if (showHidden.HasValue)
            sql.Append(showHidden.Value ? " AND account.IsActive = 0" : " AND account.IsActive = 1");

        if (!string.IsNullOrWhiteSpace(searchModel.AccountNumber))
            sql.Append(" AND account.AccountNumber LIKE '%' + @erpAccountNo + '%'");

        if (searchModel.ErpSalesOrgId > 0)
            sql.Append(" AND account.ErpSalesOrgId = @salesOrgId");

        if (!string.IsNullOrWhiteSpace(searchModel.AccountName))
            sql.Append(" AND account.AccountName LIKE '%' + @accountName + '%'");

        sql.Append(" AND account.IsDeleted = 0");

        if (!string.IsNullOrWhiteSpace(searchModel.Email))
            sql.Append(" AND bAddress.Email LIKE '%' + @email + '%'");

        sql.Append(" ORDER BY account.Id");

        var parameters = new
        {
            erpAccountNo = searchModel.AccountNumber,
            salesOrgId = searchModel.ErpSalesOrgId,
            accountName = searchModel.AccountName,
            email = searchModel.Email
        };

        // Pass parameters to the query
        var dataTable = await _b2BExportImportManager.GetXLWorkbookByQuery(sql.ToString(), parameters);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("ErpAccounts");

        // Load data into the worksheet
        worksheet.Cell(1, 1).InsertTable(dataTable);

        // Return the workbook as a byte array
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public async Task<byte[]> ExportSelectedErpAccountsToXlsxAsync(string ids)
    {
        if (ids == null || ids.Length == 0)
            return null; 

        var sql = @"
            SELECT account.[Id], account.[AccountNumber], account.[AccountName],
                   saleOrg.[Code] AS SalesOrgCode,
                   bAddress.FirstName AS BillingFirstName, bAddress.LastName AS BillingLastName,
                   bAddress.Email AS BillingEmail, bAddress.Company AS BillingCompany,
                   bCountry.[Name] AS BillingCountry, bStateProvince.[Name] AS BillingStateProvince,
                   bAddress.City AS BillingCity, bAddress.Address1 AS BillingAddress1,
                   bAddress.Address2 AS BillingAddress2, account.[BillingSuburb],
                   bAddress.ZipPostalCode AS BillingZipPostalCode, bAddress.PhoneNumber AS BillingPhoneNumber,
                   account.[VatNumber], account.[CreditLimit], account.[CurrentBalance], 
                   account.[CreditLimitAvailable], account.[AllowOverspend],
                   account.[TotalSavingsForthisYear], priceGroup.[Code] AS PriceGroupCode,
                   account.[PreFilterFacets], account.[PaymentTypeCode],
                   account.[OverrideBackOrderingConfigSetting], account.[AllowAccountsBackOrdering],
                   account.[OverrideAddressEditOnCheckoutConfigSetting], account.[AllowAccountsAddressEditOnCheckout],
                   account.[StockDisplayFormatTypeId], account.[ErpAccountStatusTypeId],
                   account.[PercentageOfStockAllowed], account.[LastErpAccountSyncDate],
                   account.[LastPriceRefresh], account.[IsActive], account.[IsDeleted],
                   account.[CreatedOnUtc], account.[CreatedById], account.[UpdatedOnUtc], account.[UpdatedById],
                   account.[IsDefaultPaymentAccount]
            FROM [dbo].[Erp_Account] account
            LEFT JOIN [dbo].[Erp_Sales_Org] saleOrg ON account.ErpSalesOrgId = saleOrg.Id
            LEFT JOIN [dbo].[Erp_Group_Price_Code] priceGroup ON account.B2BPriceGroupCodeId = priceGroup.Id
            LEFT JOIN [dbo].[Address] bAddress ON account.[BillingAddressId] = bAddress.[Id]
            LEFT JOIN [dbo].[Country] bCountry ON bAddress.CountryId = bCountry.[Id]
            LEFT JOIN [dbo].[StateProvince] bStateProvince ON bAddress.StateProvinceId = bStateProvince.[Id]
            WHERE account.Id IN @Ids
            ORDER BY account.Id";

        if (!string.IsNullOrEmpty(ids))
        {
            sql = sql.Replace("@Ids", $"({ids})");
        }
        var dataTable = await _b2BExportImportManager.GetXLWorkbookByQuery(sql, null);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("ErpAccounts_Selected");

        // Load data into the worksheet
        worksheet.Cell(1, 1).InsertTable(dataTable);

        // Return the workbook as a byte array
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    #endregion

    #region Import/Excel

    public static async Task<IList<PropertyByName<T, Language>>> GetPropertiesByExcelCellsAsync<T>(IXLWorksheet workbook)
    {
        var properties = new List<PropertyByName<T, Language>>();
        var poz = 1;
        while (true)
        {
            try
            {
                var x = workbook;
                var y = x.Cell(1, poz).Value;

                if (string.IsNullOrEmpty(y.ToString()))
                    break;

                poz += 1;
                properties.Add(new PropertyByName<T, Language>(y.ToString()));

            }
            catch
            {
                break;
            }
        }

        return properties;
    }

    protected virtual async Task<ErpAccountExportImportModel> GetModelFromXlsxAsync(PropertyManager<ErpAccountExportImportModel, Language> manager, 
        IXLWorksheet worksheet, 
        int iRow)
    {
        manager.ReadDefaultFromXlsx(worksheet, iRow);
        var model = new ErpAccountExportImportModel();

        foreach (var property in manager.GetDefaultProperties)
        {
            switch (property.PropertyName)
            {
                case "AccountNumber":
                    model.AccountNumber = property.StringValue;
                    break;
                case "AccountName":
                    model.AccountName = property.StringValue;
                    break;
                case "SalesOrgCode":
                    model.SalesOrganisationCode = property.StringValue;
                    break;
                case "BillingFirstName":
                    model.BillingFirstName = property.StringValue;
                    break;
                case "BillingLastName":
                    model.BillingLastName = property.StringValue;
                    break;
                case "BillingEmail":
                    model.BillingEmail = property.StringValue;
                    break;
                case "BillingCompany":
                    model.BillingCompany = property.StringValue;
                    break;
                case "BillingCountry":
                    model.BillingCountry = property.StringValue;
                    break;
                case "BillingStateProvince":
                    model.BillingStateProvince = property.StringValue;
                    break;
                case "BillingCity":
                    model.BillingCity = property.StringValue;
                    break;
                case "BillingAddress1":
                    model.BillingAddress1 = property.StringValue;
                    break;
                case "BillingAddress2":
                    model.BillingAddress2 = property.StringValue;
                    break;
                case "BillingSuburb":
                    model.BillingSuburb = property.StringValue;
                    break;
                case "BillingZipPostalCode":
                    model.BillingZipPostalCode = property.StringValue;
                    break;
                case "BillingPhoneNumber":
                    model.BillingPhoneNumber = property.StringValue;
                    break;
                case "VatNumber":
                    model.VatNumber = property.StringValue;
                    break;
                case "CreditLimit":
                    model.CreditLimit = property.StringValue;
                    break;
                case "CurrentBalance":
                    model.CurrentBalance = property.StringValue;
                    break;
                case "AllowOverspend":
                    model.AllowOverspend = property.StringValue;
                    break;
                case "PriceGroupCode":
                    model.PriceGroupCode = property.StringValue;
                    break;
                case "PreFilterFacets":
                    model.PreFilterFacets = property.StringValue;
                    break;
                case "PaymentTypeCode":
                    model.PaymentTypeCode = property.StringValue;
                    break;
                case "OverrideBackOrderingConfigSetting":
                    model.OverrideBackOrderingConfigSetting = property.StringValue;
                    break;
                case "AllowAccountsBackOrdering":
                    model.AllowAccountsBackOrdering = property.StringValue;
                    break;
                case "OverrideAddressEditOnCheckoutConfigSetting":
                    model.OverrideAddressEditOnCheckoutConfigSetting = property.StringValue;
                    break;
                case "AllowAccountsAddressEditOnCheckout":
                    model.AllowAccountsAddressEditOnCheckout = property.StringValue;
                    break;
                case "StockDisplayFormatTypeId":
                    model.StockDisplayFormatTypeId = property.StringValue;
                    break;
                case "ErpAccountStatusTypeId":
                    model.ErpAccountStatusTypeId = property.StringValue;
                    break;
                case "PercentageOfStockAllowed":
                    model.PercentageOfStockAllowed = property.StringValue;
                    break;
                case "LastErpAccountSyncDate":
                    model.LastAccountRefresh = property.StringValue;
                    break;
                case "LastPriceRefresh":
                    model.LastPriceRefresh = property.StringValue;
                    break;
                case "IsActive":
                    model.IsActive = property.StringValue;
                    break;
                case "IsDefaultPaymentAccount":
                    model.IsDefaultPaymentAccount = property.StringValue;
                    break;
            }
        }
        return model;
    }

    public async Task ImportErpAccountsFromXlsxAsync(Stream stream)
    {
        var dataTable = new DataTable();
        using (var workbook = new XLWorkbook(stream))
        {
            var worksheet = workbook.Worksheets.FirstOrDefault() ?? throw new NopException("No workbook found");
            var properties = await GetPropertiesByExcelCellsAsync<ErpAccountExportImportModel>(worksheet);

            // Pass the resolved list to the PropertyManager
            var manager = new PropertyManager<ErpAccountExportImportModel, Language>(properties, _catalogSettings);

            var iRow = 2;

            dataTable.Columns.Add(new DataColumn("AccountNumber", typeof(string)));
            dataTable.Columns.Add(new DataColumn("AccountName", typeof(string)));
            dataTable.Columns.Add(new DataColumn("SalesOrgCode", typeof(string)));
            dataTable.Columns.Add(new DataColumn("BillingFirstName", typeof(string)));
            dataTable.Columns.Add(new DataColumn("BillingLastName", typeof(string)));
            dataTable.Columns.Add(new DataColumn("BillingEmail", typeof(string)));
            dataTable.Columns.Add(new DataColumn("BillingCompany", typeof(string)));
            dataTable.Columns.Add(new DataColumn("BillingCountry", typeof(string)));
            dataTable.Columns.Add(new DataColumn("BillingStateProvince", typeof(string)));
            dataTable.Columns.Add(new DataColumn("BillingCity", typeof(string)));
            dataTable.Columns.Add(new DataColumn("BillingAddress1", typeof(string)));
            dataTable.Columns.Add(new DataColumn("BillingAddress2", typeof(string)));
            dataTable.Columns.Add(new DataColumn("BillingSuburb", typeof(string)));
            dataTable.Columns.Add(new DataColumn("BillingZipPostalCode", typeof(string)));
            dataTable.Columns.Add(new DataColumn("BillingPhoneNumber", typeof(string)));
            dataTable.Columns.Add(new DataColumn("VatNumber", typeof(string)));
            dataTable.Columns.Add(new DataColumn("CreditLimit", typeof(string)));
            dataTable.Columns.Add(new DataColumn("CurrentBalance", typeof(string)));
            dataTable.Columns.Add(new DataColumn("AllowOverspend", typeof(string)));
            dataTable.Columns.Add(new DataColumn("PriceGroupCode", typeof(string)));
            dataTable.Columns.Add(new DataColumn("PreFilterFacets", typeof(string)));
            dataTable.Columns.Add(new DataColumn("PaymentTypeCode", typeof(string)));
            dataTable.Columns.Add(new DataColumn("OverrideBackOrderingConfigSetting", typeof(string)));
            dataTable.Columns.Add(new DataColumn("AllowAccountsBackOrdering", typeof(string)));
            dataTable.Columns.Add(new DataColumn("OverrideAddressEditOnCheckoutConfigSetting", typeof(string)));
            dataTable.Columns.Add(new DataColumn("AllowAccountsAddressEditOnCheckout", typeof(string)));
            dataTable.Columns.Add(new DataColumn("StockDisplayFormatTypeId", typeof(string)));
            dataTable.Columns.Add(new DataColumn("ErpAccountStatusTypeId", typeof(string)));
            dataTable.Columns.Add(new DataColumn("PercentageOfStockAllowed", typeof(string)));
            dataTable.Columns.Add(new DataColumn("LastErpAccountSyncDate", typeof(string)));
            dataTable.Columns.Add(new DataColumn("LastPriceRefresh", typeof(string)));
            dataTable.Columns.Add(new DataColumn("IsActive", typeof(string)));
            dataTable.Columns.Add(new DataColumn("IsDefaultPaymentAccount", typeof(string)));

            while (true)
            {
                var allColumnsAreEmpty = manager.GetDefaultProperties
                .Select(property => worksheet.Cell(iRow, property.PropertyOrderPosition))
                .All(cell => cell == null || string.IsNullOrEmpty(cell.GetValue<string>()));

                if (allColumnsAreEmpty)
                    break;

                var model = await GetModelFromXlsxAsync(manager, worksheet, iRow);
                var row = dataTable.NewRow();
                row[dataTable.Columns.IndexOf("AccountNumber")] = model.AccountNumber;
                row[dataTable.Columns.IndexOf("AccountName")] = model.AccountName;
                row[dataTable.Columns.IndexOf("SalesOrgCode")] = model.SalesOrganisationCode;
                row[dataTable.Columns.IndexOf("BillingFirstName")] = model.BillingFirstName;
                row[dataTable.Columns.IndexOf("BillingLastName")] = model.BillingLastName;
                row[dataTable.Columns.IndexOf("BillingEmail")] = model.BillingEmail;
                row[dataTable.Columns.IndexOf("BillingCompany")] = model.BillingCompany;
                row[dataTable.Columns.IndexOf("BillingCountry")] = model.BillingCountry;
                row[dataTable.Columns.IndexOf("BillingStateProvince")] = model.BillingStateProvince;
                row[dataTable.Columns.IndexOf("BillingCity")] = model.BillingCity;
                row[dataTable.Columns.IndexOf("BillingAddress1")] = model.BillingAddress1;
                row[dataTable.Columns.IndexOf("BillingAddress2")] = model.BillingAddress2;
                row[dataTable.Columns.IndexOf("BillingSuburb")] = model.BillingSuburb;
                row[dataTable.Columns.IndexOf("BillingZipPostalCode")] = model.BillingZipPostalCode;
                row[dataTable.Columns.IndexOf("BillingPhoneNumber")] = model.BillingPhoneNumber;
                row[dataTable.Columns.IndexOf("VatNumber")] = model.VatNumber;
                row[dataTable.Columns.IndexOf("CreditLimit")] = model.CreditLimit;
                row[dataTable.Columns.IndexOf("CurrentBalance")] = model.CurrentBalance;
                row[dataTable.Columns.IndexOf("AllowOverspend")] = model.AllowOverspend;
                row[dataTable.Columns.IndexOf("PriceGroupCode")] = model.PriceGroupCode;
                row[dataTable.Columns.IndexOf("PreFilterFacets")] = model.PreFilterFacets;
                row[dataTable.Columns.IndexOf("PaymentTypeCode")] = model.PaymentTypeCode;
                row[dataTable.Columns.IndexOf("OverrideBackOrderingConfigSetting")] = model.OverrideBackOrderingConfigSetting;
                row[dataTable.Columns.IndexOf("AllowAccountsBackOrdering")] = model.AllowAccountsBackOrdering;
                row[dataTable.Columns.IndexOf("OverrideAddressEditOnCheckoutConfigSetting")] = model.OverrideAddressEditOnCheckoutConfigSetting;
                row[dataTable.Columns.IndexOf("AllowAccountsAddressEditOnCheckout")] = model.AllowAccountsAddressEditOnCheckout;
                row[dataTable.Columns.IndexOf("StockDisplayFormatTypeId")] = model.StockDisplayFormatTypeId;
                row[dataTable.Columns.IndexOf("ErpAccountStatusTypeId")] = model.ErpAccountStatusTypeId;
                row[dataTable.Columns.IndexOf("PercentageOfStockAllowed")] = model.PercentageOfStockAllowed;
                row[dataTable.Columns.IndexOf("LastErpAccountSyncDate")] = model.LastAccountRefresh;
                row[dataTable.Columns.IndexOf("LastPriceRefresh")] = model.LastPriceRefresh;
                row[dataTable.Columns.IndexOf("IsActive")] = model.IsActive;
                row[dataTable.Columns.IndexOf("IsDefaultPaymentAccount")] = model.IsDefaultPaymentAccount;

                dataTable.Rows.Add(row);

                iRow++;
            }
        }

        var connectionString = DataSettingsManager.LoadSettings().ConnectionString;

        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        // Truncate staging table
        using (var truncateCmd = new SqlCommand("TRUNCATE TABLE [dbo].[ErpAccountImport]", connection))
        {
            await truncateCmd.ExecuteNonQueryAsync();
        }

        // Insert each row
        foreach (DataRow row in dataTable.Rows)
        {
            DateTime? lastAccountRefresh = null;
            DateTime tempDate;

            if (DateTime.TryParse(row["LastErpAccountSyncDate"].ToString(), out tempDate))
                lastAccountRefresh = tempDate;

            DateTime? lastPriceRefresh = null;
            if (DateTime.TryParse(row["LastPriceRefresh"].ToString(), out tempDate))
                lastPriceRefresh = tempDate;

            using var insertCmd = new SqlCommand(@"
                    INSERT INTO [dbo].[ErpAccountImport]
                    (AccountNumber,
                    AccountName,
                    SalesOrganisationCode,
                    BillingFirstName,
                    BillingLastName,
                    BillingEmail,
                    BillingCompany,
                    BillingCountry,
                    BillingStateProvince,
                    BillingCity,
                    BillingAddress1,
                    BillingAddress2,
                    BillingSuburb,
                    BillingZipPostalCode,
                    BillingPhoneNumber,
                    VatNumber,
                    CreditLimit,
                    CurrentBalance,
                    AllowOverspend,
                    PriceGroupCode,
                    PreFilterFacets,
                    PaymentTypeCode,
                    OverrideBackOrderingConfigSetting,
                    AllowAccountsBackOrdering,
                    OverrideAddressEditOnCheckoutConfigSetting,
                    AllowAccountsAddressEditOnCheckout,
                    StockDisplayFormatTypeId,
                    ErpAccountStatusTypeId,
                    PercentageOfStockAllowed,
                    LastAccountRefresh,
                    LastPriceRefresh,
                    IsActive,
                    IsDefaultPaymentAccount)
                    VALUES (@AccountNumber,
                    @AccountName,
                    @SalesOrganisationCode,
                    @BillingFirstName,
                    @BillingLastName,
                    @BillingEmail,
                    @BillingCompany,
                    @BillingCountry,
                    @BillingStateProvince,
                    @BillingCity,
                    @BillingAddress1,
                    @BillingAddress2,
                    @BillingSuburb,
                    @BillingZipPostalCode,
                    @BillingPhoneNumber,
                    @VatNumber,
                    @CreditLimit,
                    @CurrentBalance,
                    @AllowOverspend,
                    @PriceGroupCode,
                    @PreFilterFacets,
                    @PaymentTypeCode,
                    @OverrideBackOrderingConfigSetting,
                    @AllowAccountsBackOrdering,
                    @OverrideAddressEditOnCheckoutConfigSetting,
                    @AllowAccountsAddressEditOnCheckout,
                    @StockDisplayFormatTypeId,
                    @ErpAccountStatusTypeId,
                    @PercentageOfStockAllowed,
                    @LastAccountRefresh,
                    @LastPriceRefresh,
                    @IsActive,
                    @IsDefaultPaymentAccount
                    )",
                connection);
            insertCmd.Parameters.AddWithValue("@AccountNumber", row["AccountNumber"]);
            insertCmd.Parameters.AddWithValue("@AccountName", row["AccountName"]);
            insertCmd.Parameters.AddWithValue("@SalesOrganisationCode", row["SalesOrgCode"]);
            insertCmd.Parameters.AddWithValue("@BillingFirstName", row["BillingFirstName"]);
            insertCmd.Parameters.AddWithValue("@BillingLastName", row["BillingLastName"]);
            insertCmd.Parameters.AddWithValue("@BillingEmail", row["BillingEmail"]);
            insertCmd.Parameters.AddWithValue("@BillingCompany", row["BillingCompany"]);
            insertCmd.Parameters.AddWithValue("@BillingCountry", row["BillingCountry"]);
            insertCmd.Parameters.AddWithValue("@BillingStateProvince", row["BillingStateProvince"]);
            insertCmd.Parameters.AddWithValue("@BillingCity", row["BillingCity"]);
            insertCmd.Parameters.AddWithValue("@BillingAddress1", row["BillingAddress1"]);
            insertCmd.Parameters.AddWithValue("@BillingAddress2", row["BillingAddress2"]);
            insertCmd.Parameters.AddWithValue("@BillingSuburb", row["BillingSuburb"]);
            insertCmd.Parameters.AddWithValue("@BillingZipPostalCode", row["BillingZipPostalCode"]);
            insertCmd.Parameters.AddWithValue("@BillingPhoneNumber", row["BillingPhoneNumber"]);
            insertCmd.Parameters.AddWithValue("@VatNumber", row["VatNumber"]);
            insertCmd.Parameters.AddWithValue("@CreditLimit", row["CreditLimit"]);
            insertCmd.Parameters.AddWithValue("@CurrentBalance", row["CurrentBalance"]);
            insertCmd.Parameters.AddWithValue("@AllowOverspend", row["AllowOverspend"]);
            insertCmd.Parameters.AddWithValue("@PriceGroupCode", row["PriceGroupCode"]);
            insertCmd.Parameters.AddWithValue("@PreFilterFacets", row["PreFilterFacets"]);
            insertCmd.Parameters.AddWithValue("@PaymentTypeCode", row["PaymentTypeCode"]);
            insertCmd.Parameters.AddWithValue("@OverrideBackOrderingConfigSetting", row["OverrideBackOrderingConfigSetting"]);
            insertCmd.Parameters.AddWithValue("@AllowAccountsBackOrdering", row["AllowAccountsBackOrdering"]);
            insertCmd.Parameters.AddWithValue("@OverrideAddressEditOnCheckoutConfigSetting", row["OverrideAddressEditOnCheckoutConfigSetting"]);
            insertCmd.Parameters.AddWithValue("@AllowAccountsAddressEditOnCheckout", row["AllowAccountsAddressEditOnCheckout"]);
            insertCmd.Parameters.AddWithValue("@StockDisplayFormatTypeId", row["StockDisplayFormatTypeId"]);
            insertCmd.Parameters.AddWithValue("@ErpAccountStatusTypeId", row["ErpAccountStatusTypeId"]);
            insertCmd.Parameters.AddWithValue("@PercentageOfStockAllowed", row["PercentageOfStockAllowed"]);
            insertCmd.Parameters.AddWithValue("@LastAccountRefresh", (object)lastAccountRefresh ?? DBNull.Value);
            insertCmd.Parameters.AddWithValue("@LastPriceRefresh", (object)lastPriceRefresh ?? DBNull.Value);
            insertCmd.Parameters.AddWithValue("@IsActive", row["IsActive"]);
            insertCmd.Parameters.AddWithValue("@IsDefaultPaymentAccount", row["IsDefaultPaymentAccount"]);

            await insertCmd.ExecuteNonQueryAsync();
        }

        // Call the stored procedure
        using var spCmd = new SqlCommand("[dbo].[ErpAccountImportProcedure]", connection);
        spCmd.CommandType = CommandType.StoredProcedure;
        spCmd.Parameters.AddWithValue("@CurrentUserId", ((await _workContext.GetCurrentCustomerAsync()).Id));

        await spCmd.ExecuteNonQueryAsync();
    }

    #endregion
}
