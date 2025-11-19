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
using Nop.Services.Common;
using Nop.Services.ExportImport.Help;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Common;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models.ErpShipToAddress;
using NopStation.Plugin.B2B.B2BB2CFeatures.Helpers;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Factories;

public class ErpShipToAddressModelFactory : IErpShipToAddressModelFactory
{
    #region Fields

    private readonly IErpShipToAddressService _erpShipToAddressService;
    private readonly ICommonHelper _commonHelper;
    private readonly IAddressService _addressService;
    private readonly IErpAccountService _erpAccountService;
    private readonly IAddressModelFactory _addressModelFactory;
    private readonly IDateTimeHelper _dateTimeHelper;
    private readonly AddressSettings _addressSettings;
    private readonly IErpSalesOrgService _erpSalesOrgService;
    private readonly ILocalizationService _localizationService;
    private readonly IB2BExportImportManager _b2BExportImportManager;
    private readonly CatalogSettings _catalogSettings;
    private readonly IWorkContext _workContext;

    #endregion

    #region Ctor

    public ErpShipToAddressModelFactory(IErpShipToAddressService erpShipToAddressService,
        ICommonHelper commonHelper,
        IAddressService addressService,
        IErpAccountService erpAccountService,
        IAddressModelFactory addressModelFactory,
        IDateTimeHelper dateTimeHelper,
        AddressSettings addressSettings,
        IErpSalesOrgService erpSalesOrgService,
        ILocalizationService localizationService,
        IB2BExportImportManager b2BExportImportManager,
        CatalogSettings catalogSettings,
        IWorkContext workContext)
    {
        _erpShipToAddressService = erpShipToAddressService;
        _commonHelper = commonHelper;
        _addressService = addressService;
        _erpAccountService = erpAccountService;
        _addressModelFactory = addressModelFactory;
        _dateTimeHelper = dateTimeHelper;
        _addressSettings = addressSettings;
        _erpSalesOrgService = erpSalesOrgService;
        _localizationService = localizationService;
        _b2BExportImportManager = b2BExportImportManager;
        _catalogSettings = catalogSettings;
        _workContext = workContext;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Prepare erpShipToAddress search model
    /// </summary>
    /// <param name="searchModel">ErpShipToAddress search model</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the erpShipToAddress search model
    /// </returns>
    public virtual async Task<ErpShipToAddressSearchModel> PrepareErpShipToAddressSearchModelAsync(ErpShipToAddressSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

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

        //prepare page parameters
        searchModel.SetGridPageSize();

        return searchModel;
    }

    /// <summary>
    /// Prepare paged erpShipToAddress list model
    /// </summary>
    /// <param name="searchModel">ErpShipToAddress search model</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the erpShipToAddress list model
    /// </returns>
    public virtual async Task<ErpShipToAddressListModel> PrepareErpShipToAddressListModelAsync(ErpShipToAddressSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        //get erpShipToAddresses
        var erpShipToAddresses = await _erpShipToAddressService.GetAllErpShipToAddressesAsync(shipToCode: searchModel.SearchShipToCode,
            shipToName: searchModel.SearchShipToName,
            erpAccountId: searchModel.SearchErpAccountId,
            repNum: searchModel.SearchRepNumber,
            repFullName: searchModel.SearchRepFullName,
            repEmail: searchModel.SearchRepEmail,
            salesOrgId:  searchModel.ErpSalesOrganisationId,
            pageIndex: searchModel.Page - 1,
            pageSize: searchModel.PageSize,
            showHidden: searchModel.ShowInActive == 0 ? null : (searchModel.ShowInActive == 2),
            emailAddresses: searchModel.SearchEmailAddresses);

        //prepare list model
        var model = await new ErpShipToAddressListModel().PrepareToGridAsync(searchModel, erpShipToAddresses, () =>
        {
            //fill in model values from the entity
            return erpShipToAddresses.SelectAwait(async erpShipToAddress =>
            {

                var data = erpShipToAddress.ToModel<ErpShipToAddressModel>();
                var erpAccount = await _erpAccountService.GetErpAccountByErpShipToAddressAsync(erpShipToAddress);

                if (erpAccount != null)
                {
                    data.ErpAccount = string.Concat(erpAccount.AccountName, "(", erpAccount.AccountNumber, ")");
                    var erpAccountSalesOrg = await _erpSalesOrgService.GetErpSalesOrgByIdAsync(erpAccount.ErpSalesOrgId);
                    if (erpAccountSalesOrg != null)
                    {
                        data.ErpAccountSalesOrgId = erpAccountSalesOrg.Id;
                        if (!string.IsNullOrEmpty(erpAccountSalesOrg.Code))
                            data.ErpAccountSalesOrgName = string.Concat(erpAccountSalesOrg.Name, "(", erpAccountSalesOrg.Code, ")");
                        else
                            data.ErpAccountSalesOrgName = erpAccountSalesOrg.Name;
                    }
                }
                else
                {
                    data.IsDeletedErpAccount = true;
                }

                return data;

            });
        });

        return model;
    }

    /// <summary>
    /// Prepare erpShipToAddress model
    /// </summary>
    /// <param name="model">ErpShipToAddress model</param>
    /// <param name="erpShipToAddress">ErpShipToAddress</param>
    /// <param name="excludeProperties">Whether to exclude populating of some properties of model</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the erpShipToAddress model
    /// </returns>
    public virtual async Task<ErpShipToAddressModel> PrepareErpShipToAddressModelAsync(ErpShipToAddressModel model, ErpShipToAddress erpShipToAddress, bool excludeProperties = false)
    {
        var address = new Address();
        var addressModel = new AddressModel();

        if (erpShipToAddress != null)
        {
            //fill in model values from the entity
            model ??= erpShipToAddress.ToModel<ErpShipToAddressModel>();
            var erpShipToAddressAccountMap = await _erpShipToAddressService.GetErpShipToAddressErpAccountMapByErpShipToAddressIdAsync(erpShipToAddress.Id);
            model.ErpAccountId = erpShipToAddressAccountMap?.ErpAccountId ?? 0;

            var erpAccount = await _erpAccountService.GetErpAccountByIdAsync(model.ErpAccountId);
            model.ErpAccount = erpAccount != null ? $"{erpAccount.AccountName} ({erpAccount.AccountNumber})" : "";
            model.CreatedOnUtc = await _dateTimeHelper.ConvertToUserTimeAsync(erpShipToAddress.CreatedOnUtc, DateTimeKind.Utc);
            model.UpdatedOnUtc = await _dateTimeHelper.ConvertToUserTimeAsync(erpShipToAddress.UpdatedOnUtc, DateTimeKind.Utc);
            model.LastShipToAddressSyncDate = await _dateTimeHelper.ConvertToUserTimeAsync(erpShipToAddress.LastShipToAddressSyncDate ?? DateTime.MinValue, DateTimeKind.Utc);
            //Address model field requirements add
            if (model.AddressId > 0)
            {
                address = await _addressService.GetAddressByIdAsync(model.AddressId);
                //prepare address model
                if (address != null)
                    addressModel = address.ToModel(addressModel);
            }
            else
            {
                addressModel = address.ToModel<AddressModel>();
            }
        }

        await _addressModelFactory.PrepareAddressModelAsync(addressModel, address);

        addressModel.FirstNameRequired = true;
        addressModel.EmailRequired = true;
        addressModel.CountryRequired = true;
        addressModel.CityRequired = true;
        addressModel.PhoneRequired = true;
        addressModel.ZipPostalCodeRequired = true;

        addressModel.CompanyRequired = _addressSettings.CompanyRequired;
        addressModel.CountyRequired = _addressSettings.CountyRequired;
        addressModel.StreetAddressRequired = _addressSettings.StreetAddressRequired;
        addressModel.StreetAddress2Required = _addressSettings.StreetAddress2Required;
        addressModel.FaxRequired = _addressSettings.FaxRequired;

        model.AddressModel = addressModel;
        model.IsActive = erpShipToAddress is null || erpShipToAddress.IsActive;

        return model;
    }

    #endregion

    #region Export
    public async Task<byte[]> ExportAllErpShipToAddressesToXlsxAsync(ErpShipToAddressSearchModel searchModel)
    {
        var sql = new StringBuilder(@"
                    SELECT 
                    shipto.Id,
                    shipto.ShipToCode,
                    shipto.ShipToName,
                    sAddress.Company,
                    sCountry.Name AS Country,
                    sStateProvince.Name AS StateProvince,
                    sAddress.City,
                    sAddress.Address1,
                    sAddress.Address2,
                    shipto.Suburb,
                    sAddress.ZipPostalCode,
                    sAddress.PhoneNumber,
                    shipto.DeliveryNotes,
                    shipto.EmailAddresses,
                    account.AccountNumber,
                    saleOrg.Code AS AccountSalesOrganisationCode,
                    shipto.IsActive
                FROM Erp_ShipToAddress shipto
                INNER JOIN Erp_ShiptoAddress_Erp_Account_Map cam
                    ON shipto.Id = cam.ErpShiptoAddressId
                INNER JOIN Erp_Account account
                    ON cam.ErpAccountId = account.Id
                LEFT JOIN Erp_Sales_Org saleOrg
                    ON account.ErpSalesOrgId = saleOrg.Id
                INNER JOIN Address sAddress
                    ON shipto.AddressId = sAddress.Id
                INNER JOIN Country sCountry
                    ON sAddress.CountryId = sCountry.Id
                INNER JOIN StateProvince sStateProvince
                    ON sAddress.StateProvinceId = sStateProvince.Id
                Where cam.ErpShipToAddressCreatedByTypeId = 10
                                                 ");

        // Append filters
        bool? showHidden = searchModel.ShowInActive == 0 ? null : (searchModel.ShowInActive == 2);
        if (showHidden.HasValue)
            sql.Append(showHidden.Value ? " AND shipto.IsActive = 0" : " AND shipto.IsActive = 1");

        if (!string.IsNullOrWhiteSpace(searchModel.SearchShipToCode))
            sql.Append(" AND shipto.ShipToCode LIKE '%' + @shipToCode + '%'");

        if (!string.IsNullOrWhiteSpace(searchModel.SearchShipToName))
            sql.Append(" AND shipto.ShipToName LIKE '%' + @shipToName + '%'");

        if (!string.IsNullOrWhiteSpace(searchModel.SearchEmailAddresses))
            sql.Append(" AND shipto.EmailAddresses LIKE '%' + @emailAddresses + '%'");

        if (searchModel.SearchErpAccountId > 0)
            sql.Append(" AND account.Id = @erpAccountId");

        if (!string.IsNullOrWhiteSpace(searchModel.SearchRepNumber))
            sql.Append(" AND shipto.RepNumber LIKE '%' + @repNumber + '%'");

        if (!string.IsNullOrWhiteSpace(searchModel.SearchRepFullName))
            sql.Append(" AND shipto.RepFullName LIKE '%' + @repFullName + '%'");

        if (!string.IsNullOrWhiteSpace(searchModel.SearchRepPhoneNumber))
            sql.Append(" AND shipto.RepPhoneNumber LIKE '%' + @repPhoneNumber + '%'");

        if (!string.IsNullOrWhiteSpace(searchModel.SearchRepEmail))
            sql.Append(" AND shipto.RepEmail LIKE '%' + @repEmail + '%'");

        if (searchModel.ErpSalesOrganisationId >0)
            sql.Append(" AND salesorg.Id = @erpSalesOrganisationId");

        sql.Append(" AND shipto.IsDeleted = 0");

        sql.Append("  ORDER BY account.Id");

        var parameters = new
        {
            shipToCode = searchModel.SearchShipToCode,
            shipToName = searchModel.SearchShipToName,
            emailAddresses = searchModel.SearchEmailAddresses,
            erpAccountId = searchModel.SearchErpAccountId,
            repNumber = searchModel.SearchRepNumber,
            repFullName = searchModel.SearchRepFullName,
            repPhoneNumber = searchModel.SearchRepPhoneNumber,
            repEmail = searchModel.SearchRepEmail,
            erpSalesOrganisationId = searchModel.ErpSalesOrganisationId,
        };

        // Pass parameters to the query
        var dataTable = await _b2BExportImportManager.GetXLWorkbookByQuery(sql.ToString(), parameters);

        using (var workbook = new XLWorkbook())
        {
            var worksheet = workbook.Worksheets.Add("Erp_ShipToAddress");

            // Load data into the worksheet
            worksheet.Cell(1, 1).InsertTable(dataTable);

            // Return the workbook as a byte array
            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                return stream.ToArray();
            }
        }
    }
    public async Task<byte[]> ExportSelectedErpShipToAddressesToXlsxAsync(string ids)
    {
        var sql = @"
                    SELECT 
                    shipto.Id,
                    shipto.ShipToCode,
                    shipto.ShipToName,
                    sAddress.Company,
                    sCountry.Name AS Country,
                    sStateProvince.Name AS StateProvince,
                    sAddress.City,
                    sAddress.Address1,
                    sAddress.Address2,
                    shipto.Suburb,
                    sAddress.ZipPostalCode,
                    sAddress.PhoneNumber,
                    shipto.DeliveryNotes,
                    shipto.EmailAddresses,
                    account.AccountNumber,
                    saleOrg.Code AS AccountSalesOrganisationCode,
                    shipto.IsActive
                FROM Erp_ShipToAddress shipto
                INNER JOIN Erp_ShiptoAddress_Erp_Account_Map cam
                    ON shipto.Id = cam.ErpShiptoAddressId
                INNER JOIN Erp_Account account
                    ON cam.ErpAccountId = account.Id
                LEFT JOIN Erp_Sales_Org saleOrg
                    ON account.ErpSalesOrgId = saleOrg.Id
                INNER JOIN Address sAddress
                    ON shipto.AddressId = sAddress.Id
                INNER JOIN Country sCountry
                    ON sAddress.CountryId = sCountry.Id
                INNER JOIN StateProvince sStateProvince
                    ON sAddress.StateProvinceId = sStateProvince.Id
                Where cam.ErpShipToAddressCreatedByTypeId = 10
                AND shipto.IsDeleted = 0
                AND shipto.Id IN @Ids
                 ORDER BY account.Id";


        if (!string.IsNullOrEmpty(ids))
        {
            sql = sql.Replace("@Ids", $"({ids})");
        }

        // Pass parameters to the query
        var dataTable = await _b2BExportImportManager.GetXLWorkbookByQuery(sql.ToString(), null);

        using (var workbook = new XLWorkbook())
        {
            var worksheet = workbook.Worksheets.Add("Erp_ShipToAddress");

            // Load data into the worksheet
            worksheet.Cell(1, 1).InsertTable(dataTable);

            // Return the workbook as a byte array
            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                return stream.ToArray();
            }
        }
    }

    #endregion

    #region Import

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

    protected virtual async Task<ErpShipToAddressExportImportModel> GetModelFromXlsxAsync(
   PropertyManager<ErpShipToAddressExportImportModel, Language> manager, IXLWorksheet worksheet, int iRow)
    {
        manager.ReadDefaultFromXlsx(worksheet, iRow);
        var model = new ErpShipToAddressExportImportModel();

        foreach (var property in manager.GetDefaultProperties)
        {
            switch (property.PropertyName)
            {
                case "ShipToCode":
                    model.ShipToCode = property.StringValue;
                    break;
                case "ShipToName":
                    model.ShipToName = property.StringValue;
                    break;
                case "Company":
                    model.Company = property.StringValue;
                    break;
                case "Country":
                    model.Country = property.StringValue;
                    break;
                case "StateProvince":
                    model.StateProvince = property.StringValue;
                    break;
                case "City":
                    model.City = property.StringValue;
                    break;
                case "Address1":
                    model.Address1 = property.StringValue;
                    break;
                case "Address2":
                    model.Address2 = property.StringValue;
                    break;
                case "Suburb":
                    model.Suburb = property.StringValue;
                    break;
                case "ZipPostalCode":
                    model.ZipPostalCode = property.StringValue;
                    break;
                case "PhoneNumber":
                    model.PhoneNumber = property.StringValue;
                    break;
                case "DeliveryNotes":
                    model.DeliveryNotes = property.StringValue;
                    break;
                case "EmailAddresses":
                    model.EmailAddresses = property.StringValue;
                    break;
                case "AccountNumber":
                    model.AccountNumber = property.StringValue;
                    break;
                case "AccountSalesOrganisationCode":
                    model.AccountSalesOrganisationCode = property.StringValue;
                    break;
                case "IsActive":
                    model.IsActive = property.StringValue;
                    break;
            }
        }
        return model;
    }

    public async Task ImportErpShipToAddressFromXlsxAsync(Stream stream)
    {
        var dataTable = new DataTable();
        using (var workbook = new XLWorkbook(stream))
        {
            var worksheet = workbook.Worksheets.FirstOrDefault();
            if (worksheet == null)
                throw new NopException("No workbook found");
            var properties = await GetPropertiesByExcelCellsAsync<ErpShipToAddressExportImportModel>(worksheet);

            // Pass the resolved list to the PropertyManager
            var manager = new PropertyManager<ErpShipToAddressExportImportModel, Language>(properties, _catalogSettings);

            var iRow = 2;

            dataTable.Columns.Add(new DataColumn("ShipToCode", typeof(string)));
            dataTable.Columns.Add(new DataColumn("ShipToName", typeof(string)));
            dataTable.Columns.Add(new DataColumn("Company", typeof(string)));
            dataTable.Columns.Add(new DataColumn("Country", typeof(string)));
            dataTable.Columns.Add(new DataColumn("StateProvince", typeof(string)));
            dataTable.Columns.Add(new DataColumn("City", typeof(string)));
            dataTable.Columns.Add(new DataColumn("Address1", typeof(string)));
            dataTable.Columns.Add(new DataColumn("Address2", typeof(string)));
            dataTable.Columns.Add(new DataColumn("Suburb", typeof(string)));
            dataTable.Columns.Add(new DataColumn("ZipPostalCode", typeof(string)));
            dataTable.Columns.Add(new DataColumn("PhoneNumber", typeof(string)));
            dataTable.Columns.Add(new DataColumn("DeliveryNotes", typeof(string)));
            dataTable.Columns.Add(new DataColumn("EmailAddresses", typeof(string)));
            dataTable.Columns.Add(new DataColumn("AccountNumber", typeof(string)));
            dataTable.Columns.Add(new DataColumn("AccountSalesOrganisationCode", typeof(string)));
            dataTable.Columns.Add(new DataColumn("IsActive", typeof(string)));

            while (true)
            {
                var allColumnsAreEmpty = manager.GetDefaultProperties
                .Select(property => worksheet.Cell(iRow, property.PropertyOrderPosition))
                .All(cell => cell == null || string.IsNullOrEmpty(cell.GetValue<string>()));

                if (allColumnsAreEmpty)
                    break;

                var model = await GetModelFromXlsxAsync(manager, worksheet, iRow);
                var row = dataTable.NewRow();
                row[dataTable.Columns.IndexOf("ShipToCode")] = model.ShipToCode;
                row[dataTable.Columns.IndexOf("ShipToName")] = model.ShipToName;
                row[dataTable.Columns.IndexOf("Company")] = model.Company;
                row[dataTable.Columns.IndexOf("Country")] = model.Country;
                row[dataTable.Columns.IndexOf("StateProvince")] = model.StateProvince;
                row[dataTable.Columns.IndexOf("City")] = model.City;
                row[dataTable.Columns.IndexOf("Address1")] = model.Address1;
                row[dataTable.Columns.IndexOf("Address2")] = model.Address2;
                row[dataTable.Columns.IndexOf("Suburb")] = model.Suburb;
                row[dataTable.Columns.IndexOf("ZipPostalCode")] = model.ZipPostalCode;
                row[dataTable.Columns.IndexOf("PhoneNumber")] = model.PhoneNumber;
                row[dataTable.Columns.IndexOf("DeliveryNotes")] = model.DeliveryNotes;
                row[dataTable.Columns.IndexOf("EmailAddresses")] = model.EmailAddresses;
                row[dataTable.Columns.IndexOf("AccountNumber")] = model.AccountNumber;
                row[dataTable.Columns.IndexOf("AccountSalesOrganisationCode")] = model.AccountSalesOrganisationCode;
                row[dataTable.Columns.IndexOf("IsActive")] = model.IsActive;

                dataTable.Rows.Add(row);

                iRow++;
            }

        }

        string connectionString = DataSettingsManager.LoadSettings().ConnectionString;

        using (var connection = new SqlConnection(connectionString))
        {
            await connection.OpenAsync();

            // Truncate staging table
            using (var truncateCmd = new SqlCommand("TRUNCATE TABLE [dbo].[ErpShipToAddressImport]", connection))
            {
                await truncateCmd.ExecuteNonQueryAsync();
            }

            // Insert each row
            foreach (DataRow row in dataTable.Rows)
            { 
                using (var insertCmd = new SqlCommand(@"
                    INSERT INTO [dbo].[ErpShipToAddressImport]
                    (ShipToCode,
                    ShipToName,
                    Company,
                    Country,
                    StateProvince,
                    City,
                    Address1,
                    Address2,
                    Suburb,
                    ZipPostalCode,
                    PhoneNumber,
                    DeliveryNotes,
                    EmailAddresses,
                    AccountNumber,
                    AccountSalesOrganisationCode,
                    IsActive)
                    VALUES (@ShipToCode,
                    @ShipToName,
                    @Company,
                    @Country,
                    @StateProvince,
                    @City,
                    @Address1,
                    @Address2,
                    @Suburb,
                    @ZipPostalCode,
                    @PhoneNumber,
                    @DeliveryNotes,
                    @EmailAddresses,
                    @AccountNumber,
                    @AccountSalesOrganisationCode,
                    @IsActive
                    )",
                    connection))
                {
                    insertCmd.Parameters.AddWithValue("@ShipToCode", row["ShipToCode"]);
                    insertCmd.Parameters.AddWithValue("@ShipToName", row["ShipToName"]);
                    insertCmd.Parameters.AddWithValue("@Company", row["Company"]);
                    insertCmd.Parameters.AddWithValue("@Country", row["Country"]);
                    insertCmd.Parameters.AddWithValue("@StateProvince", row["StateProvince"]);
                    insertCmd.Parameters.AddWithValue("@City", row["City"]);
                    insertCmd.Parameters.AddWithValue("@Address1", row["Address1"]);
                    insertCmd.Parameters.AddWithValue("@Address2", row["Address2"]);
                    insertCmd.Parameters.AddWithValue("@Suburb", row["Suburb"]);
                    insertCmd.Parameters.AddWithValue("@ZipPostalCode", row["ZipPostalCode"]);
                    insertCmd.Parameters.AddWithValue("@PhoneNumber", row["PhoneNumber"]);
                    insertCmd.Parameters.AddWithValue("@DeliveryNotes", row["DeliveryNotes"]);
                    insertCmd.Parameters.AddWithValue("@EmailAddresses", row["EmailAddresses"]);
                    insertCmd.Parameters.AddWithValue("@AccountNumber", row["AccountNumber"]);
                    insertCmd.Parameters.AddWithValue("@AccountSalesOrganisationCode", row["AccountSalesOrganisationCode"]);
                    insertCmd.Parameters.AddWithValue("@IsActive", row["IsActive"]);


                    await insertCmd.ExecuteNonQueryAsync();
                }
            }

            // Call the stored procedure
            using (var spCmd = new SqlCommand("[dbo].[ErpShipToAddressImportProcedure]", connection))
            {
                spCmd.CommandType = CommandType.StoredProcedure;
                spCmd.Parameters.AddWithValue("@CurrentUserId", ((await _workContext.GetCurrentCustomerAsync()).Id));

                await spCmd.ExecuteNonQueryAsync();
            }
        }
    }
    #endregion
}
