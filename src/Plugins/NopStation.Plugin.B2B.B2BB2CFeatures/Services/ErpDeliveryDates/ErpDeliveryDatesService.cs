using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Data;
using Nop.Services.Logging;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpDeliveyDates;

public class ErpDeliveryDatesService : IErpDeliveryDatesService
{
    #region Fields

    private readonly IRepository<ErpDeliveryDates> _deliveryDatesRepository;
    private readonly IErpSalesOrgService _erpSalesOrgService;
    private readonly ILogger _logger;
    private readonly B2BB2CFeaturesSettings _b2BB2CFeaturesSettings;

    #endregion

    #region Ctor
    public ErpDeliveryDatesService(
        IRepository<ErpDeliveryDates> deliveryDatesRepository,
        ILogger logger,
        IErpSalesOrgService erpSalesOrgService,
        B2BB2CFeaturesSettings b2BB2CFeaturesSettings
    )
    {
        _deliveryDatesRepository = deliveryDatesRepository;
        _logger = logger;
        _erpSalesOrgService = erpSalesOrgService;
        _b2BB2CFeaturesSettings = b2BB2CFeaturesSettings;
    }

    #endregion

    #region Utilities

    private async Task<ErpDeliveryDateResponseModel> GetErpDeliveryDatesBySalesOrgOrPlantAndCityAsync(
        string salesOrg,
        string city
    )
    {
        if (string.IsNullOrWhiteSpace(salesOrg) || string.IsNullOrWhiteSpace(city))
            return null;

        var deliveryDates = await _deliveryDatesRepository.Table
            .WhereAwait(async x =>
                !x.Deleted && x.SalesOrgOrPlant.ToLower().Equals(salesOrg.ToLower()) && x.City.ToLower().Equals(city.ToLower())
            )
            .FirstOrDefaultAsync();
        if (deliveryDates == null)
            return null;
        var delDate1IsTomorrow = false;
        var delDate1IsGreaterThanTomorrow = false;
        var currentDate = DateTime.Now;
        if (currentDate.DayOfWeek == DayOfWeek.Friday)
        {
            delDate1IsTomorrow =
                deliveryDates.DelDate1 != null
                && (deliveryDates.DelDate1.Value.Date == currentDate.AddDays(3).Date);
            delDate1IsGreaterThanTomorrow =
                deliveryDates.DelDate1 != null
                && (deliveryDates.DelDate1.Value.Date > currentDate.AddDays(3).Date);
        }
        else if (currentDate.DayOfWeek == DayOfWeek.Saturday)
        {
            delDate1IsTomorrow =
                deliveryDates.DelDate1 != null
                && (deliveryDates.DelDate1.Value.Date == currentDate.AddDays(2).Date);
            delDate1IsGreaterThanTomorrow =
                deliveryDates.DelDate1 != null
                && (deliveryDates.DelDate1.Value.Date > currentDate.AddDays(2).Date);
        }
        else
        {
            delDate1IsTomorrow =
                deliveryDates.DelDate1 != null
                && (deliveryDates.DelDate1.Value.Date == currentDate.AddDays(1).Date);
            delDate1IsGreaterThanTomorrow =
                deliveryDates.DelDate1 != null
                && (deliveryDates.DelDate1.Value.Date > currentDate.AddDays(1).Date);
        }

        if (
            DateTime.TryParseExact(
                deliveryDates.CutOffTime,
                "HH:mm",
                null,
                System.Globalization.DateTimeStyles.None,
                out var cutOffTime
            )
        )
        {
            var deliveryDateResponseModel = new ErpDeliveryDateResponseModel();
            deliveryDateResponseModel.IsFullLoadRequired = deliveryDates.IsFullLoadRequired ?? false;
            deliveryDateResponseModel.DeliveryDates = new List<DeliveryDate>();

            if (
                deliveryDates.DelDate1 != null
                && (
                    (cutOffTime.TimeOfDay > currentDate.TimeOfDay && delDate1IsTomorrow)
                    || delDate1IsGreaterThanTomorrow
                )
            )
                deliveryDateResponseModel.DeliveryDates.Add(
                    new DeliveryDate { DelDate = deliveryDates.DelDate1?.ToString("dd/MM/yyyy") }
                );

            if (
                deliveryDates.DelDate2 != null
                && deliveryDates.DelDate2.Value.Date > currentDate.Date
            )
                deliveryDateResponseModel.DeliveryDates.Add(
                    new DeliveryDate { DelDate = deliveryDates.DelDate2?.ToString("dd/MM/yyyy") }
                );

            if (
                deliveryDates.DelDate3 != null
                && deliveryDates.DelDate3.Value.Date > currentDate.Date
            )
                deliveryDateResponseModel.DeliveryDates.Add(
                    new DeliveryDate { DelDate = deliveryDates.DelDate3?.ToString("dd/MM/yyyy") }
                );

            if (
                deliveryDates.DelDate4 != null
                && deliveryDates.DelDate4.Value.Date > currentDate.Date
            )
                deliveryDateResponseModel.DeliveryDates.Add(
                    new DeliveryDate { DelDate = deliveryDates.DelDate4?.ToString("dd/MM/yyyy") }
                );

            if (
                deliveryDates.DelDate5 != null
                && deliveryDates.DelDate5.Value.Date > currentDate.Date
            )
                deliveryDateResponseModel.DeliveryDates.Add(
                    new DeliveryDate { DelDate = deliveryDates.DelDate5?.ToString("dd/MM/yyyy") }
                );

            if (
                deliveryDates.DelDate6 != null
                && deliveryDates.DelDate6.Value.Date > currentDate.Date
            )
                deliveryDateResponseModel.DeliveryDates.Add(
                    new DeliveryDate { DelDate = deliveryDates.DelDate6?.ToString("dd/MM/yyyy") }
                );

            if (
                deliveryDates.DelDate7 != null
                && deliveryDates.DelDate7.Value.Date > currentDate.Date
            )
                deliveryDateResponseModel.DeliveryDates.Add(
                    new DeliveryDate { DelDate = deliveryDates.DelDate7?.ToString("dd/MM/yyyy") }
                );

            if (
                deliveryDates.DelDate8 != null
                && deliveryDates.DelDate8.Value.Date > currentDate.Date
            )
                deliveryDateResponseModel.DeliveryDates.Add(
                    new DeliveryDate { DelDate = deliveryDates.DelDate8?.ToString("dd/MM/yyyy") }
                );

            if (
                deliveryDates.DelDate9 != null
                && deliveryDates.DelDate9.Value.Date > currentDate.Date
            )
                deliveryDateResponseModel.DeliveryDates.Add(
                    new DeliveryDate { DelDate = deliveryDates.DelDate9?.ToString("dd/MM/yyyy") }
                );

            if (
                deliveryDates.DelDate10 != null
                && deliveryDates.DelDate10.Value.Date > currentDate.Date
            )
                deliveryDateResponseModel.DeliveryDates.Add(
                    new DeliveryDate { DelDate = deliveryDates.DelDate10?.ToString("dd/MM/yyyy") }
                );

            return deliveryDateResponseModel;
        }
        else
        {
            await _logger.ErrorAsync("Failed to parse the cut off time.");
            return null;
        }
    }

    #endregion

    #region Methods

    public virtual async Task<IPagedList<ErpDeliveryDates>> SearchDeliveryDatesAsync(
        string salesOrg,
        string city,
        int pageIndex = 0,
        int pageSize = int.MaxValue
    )
    {
        salesOrg = (salesOrg ?? string.Empty).Trim();

        var query = _deliveryDatesRepository.Table;

        if (!string.IsNullOrEmpty(salesOrg))
            query = query.Where(qe => qe.SalesOrgOrPlant.Contains(salesOrg));

        if (!string.IsNullOrEmpty(city))
            query = query.Where(qe => qe.City.Contains(city));

        query = query.OrderByDescending(qe => qe.Id);

        var deliveryDatess = await query.ToPagedListAsync(pageIndex, pageSize);

        return deliveryDatess;
    }

    public virtual async Task<ErpDeliveryDateResponseModel> GetDeliveryDatesForAreaAndWarehouseAsync(
        ErpAccount erpAccount,
        string area,
        string warehouseCode
    )
    {
        if (erpAccount == null || string.IsNullOrEmpty(area))
            return null;

        var accountSalesOrg = await _erpSalesOrgService.GetErpSalesOrgByIdAsync(
            erpAccount.ErpSalesOrgId
        );

        try
        {
            return await GetErpDeliveryDatesBySalesOrgOrPlantAndCityAsync(warehouseCode, area);
        }
        catch (Exception ex)
        {
            if (_b2BB2CFeaturesSettings.EnableLogOnErpCall)
                await _logger.ErrorAsync(
                    $"B2B ERP Integration: Request failed, Error occured in GetDeliveryDatesForAreaAndWarehouse method for Account Number: {erpAccount.AccountNumber}"
                        + $" and plant code {warehouseCode}, Request URL:",
                    ex
                );
        }

        return null;
    }

    public virtual async Task<ErpDeliveryDateResponseModel> GetDeliveryDatesForSuburbOrCityAsync(
        ErpAccount erpAccount,
        string suburbOrCity
    )
    {
        if (erpAccount == null || string.IsNullOrEmpty(suburbOrCity))
            return null;

        var accountSalesOrg = await _erpSalesOrgService.GetErpSalesOrgByIdAsync(
            erpAccount.ErpSalesOrgId
        );

        try
        {
            var accSalesOrgCode = accountSalesOrg.Code;
            if (accSalesOrgCode.Equals("1032"))
                accSalesOrgCode = "1030";

            return await GetErpDeliveryDatesBySalesOrgOrPlantAndCityAsync(
                accSalesOrgCode,
                suburbOrCity
            );
        }
        catch (Exception ex)
        {
            if (_b2BB2CFeaturesSettings.EnableLogOnErpCall)
                await _logger.ErrorAsync(
                    $"B2B ERP Integration: Request failed, Error Occured while Get GetDeliveryDatesForSuburb for Account Number: {erpAccount.AccountNumber}"
                        + $" and sales org code {accountSalesOrg.Code}, Request URL: ",
                    ex
                );
        }

        return null;
    }

    #endregion
}
