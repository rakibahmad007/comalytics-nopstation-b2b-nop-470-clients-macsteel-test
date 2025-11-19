using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExCSS;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models.ErpActivityLogs;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Factories;

/// <summary>
/// Represents the erp activity logs model factory implementation
/// </summary>
public partial class ErpActivityLogsModelFactory : IErpActivityLogsModelFactory
{
    #region Fields

    private readonly IErpActivityLogsService _erpActivityLogsService;
    private readonly ICustomerService _customerService;
    private readonly IDateTimeHelper _dateTimeHelper;
    private readonly ILocalizationService _localizationService;

    #endregion

    #region Ctor

    public ErpActivityLogsModelFactory(
        IErpActivityLogsService erpActivityLogsService,
        ICustomerService customerService,
        IDateTimeHelper dateTimeHelper,
        ILocalizationService localizationService
    )
    {
        _erpActivityLogsService = erpActivityLogsService;
        _customerService = customerService;
        _dateTimeHelper = dateTimeHelper;
        _localizationService = localizationService;
    }

    #endregion

    #region Utilities

    /// <summary>
    /// Prepare default item
    /// </summary>
    /// <param name="items">Available items</param>
    /// <param name="withSpecialDefaultItem">Whether to insert the first special item for the default value</param>
    /// <param name="defaultItemText">Default item text; pass null to use "All" text</param>
    /// <param name="defaultItemValue">Default item value; defaults 0</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    protected virtual async Task PrepareDefaultItemAsync(
        IList<SelectListItem> items,
        bool withSpecialDefaultItem,
        string defaultItemText = null,
        string defaultItemValue = "0"
    )
    {
        ArgumentNullException.ThrowIfNull(items);

        //whether to insert the first special item for the default value
        if (!withSpecialDefaultItem)
            return;

        //prepare item text
        defaultItemText ??= await _localizationService.GetResourceAsync("Admin.Common.All");

        //insert this default item at first
        items.Insert(0, new SelectListItem { Text = defaultItemText, Value = defaultItemValue });
    }

    public virtual async Task PrepareErpActivityLogsTypesAsync(
        IList<SelectListItem> items,
        bool withSpecialDefaultItem = true,
        string defaultItemText = null
    )
    {
        ArgumentNullException.ThrowIfNull(items);

        //prepare available erp activity log types
        var availableActivityTypes = Enum.GetValues(typeof(ErpActivityType))
            .Cast<ErpActivityType>()
            .Select(type => new SelectListItem { Value = $"{(int)type}", Text = $"{type}" })
            .ToList();
        foreach (var types in availableActivityTypes)
        {
            items.Add(types);
        }
        //insert special item for the default value
        await PrepareDefaultItemAsync(items, withSpecialDefaultItem, defaultItemText);
    }

    #endregion

    #region Methods

    /// <summary>
    /// Prepare erp activity logs search model
    /// </summary>
    /// <param name="searchModel">Erp Activity logs search model</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the erp activity logs search model
    /// </returns>
    public virtual async Task<ErpActivityLogsSearchModel> PrepareErpActivityLogsSearchModelAsync(
        ErpActivityLogsSearchModel searchModel
    )
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        //prepare available activity log types
        await PrepareErpActivityLogsTypesAsync(searchModel.ErpActivityLogsType);
        searchModel.AvailableEntityType = await PrepareEntityItemAsync(
            searchModel.AvailableEntityType
        );

        searchModel.SetGridPageSize();

        return searchModel;
    }

    private async Task<IList<SelectListItem>> PrepareEntityItemAsync(
        IList<SelectListItem> items,
        bool withSpecialDefaultItem = true,
        string defaultItemText = null
    )
    {
        ArgumentNullException.ThrowIfNull(items);

        //prepare available entity names
        var entityTypes = new[]
        {
            nameof(ErpAccount),
            nameof(ErpGroupPriceCode),
            nameof(ErpSalesOrg),
            nameof(ErpWarehouseSalesOrgMap),
            nameof(B2BSalesOrgPickupPoint),
            nameof(ErpShipToAddress),
            nameof(ErpNopUser),
            nameof(ErpUserRegistrationInfo),
            nameof(ErpOrderAdditionalData),
            nameof(ErpOrderItemAdditionalData),
            nameof(ErpInvoice),
            nameof(ErpSpecialPrice),
            nameof(ErpGroupPrice),
            nameof(ErpSalesRep),
            nameof(ErpSalesRepSalesOrgMap),
            nameof(ErpShiptoAddressErpAccountMap),
            nameof(ErpUserFavourite),
            nameof(ErpProductNotePerSalesOrg),
            nameof(ERPPriceListDownloadTrack),
            nameof(CustomerPassword),
            nameof(CustomerRole),
            nameof(GenericAttribute),
            //nameof(B2BSAPErrorMsgTranslation)
        };

        foreach (var entityType in entityTypes)
        {
            items.Add(new SelectListItem { Text = entityType, Value = entityType });
        }

        ///whether to insert the first special item for the default value
        if (!withSpecialDefaultItem)
            return items;

        //at now we use "0" as the default value
        const string value = "0";

        //prepare item text
        defaultItemText ??= await _localizationService.GetResourceAsync("Admin.Common.All");

        //insert this default item at first
        items.Insert(0, new SelectListItem { Text = defaultItemText, Value = value });

        return items;
    }

    /// <summary>
    /// Prepare paged erp activity logs list model
    /// </summary>
    /// <param name="searchModel">Erp Activity logs search model</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the erp activity logs list model
    /// </returns>
    public virtual async Task<ErpActivityLogsListModel> PrepareErpActivityLogsListModelAsync(
        ErpActivityLogsSearchModel searchModel
    )
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        //get parameters to filter log
        var startDateValue =
            searchModel.CreatedOnFrom == null
                ? null
                : (DateTime?)
                    _dateTimeHelper.ConvertToUtcTime(
                        searchModel.CreatedOnFrom.Value,
                        await _dateTimeHelper.GetCurrentTimeZoneAsync()
                    );
        var endDateValue =
            searchModel.CreatedOnTo == null
                ? null
                : (DateTime?)
                    _dateTimeHelper
                        .ConvertToUtcTime(
                            searchModel.CreatedOnTo.Value,
                            await _dateTimeHelper.GetCurrentTimeZoneAsync()
                        )
                        .AddDays(1);

        //get log
        var activityLog = await _erpActivityLogsService.GetAllErpActivitiesAsync(
            entityName: searchModel.EntityType,
            entityDescription: searchModel.EntityDescription,
            propertyName: searchModel.PropertyName,
            newValue: searchModel.NewValue,
            oldValue: searchModel.OldValue,
            createdOnFrom: startDateValue,
            createdOnTo: endDateValue,
            activityLogTypeId: searchModel.ErpActivityLogsTypeId,
            ipAddress: searchModel.IpAddress,
            pageIndex: searchModel.Page - 1,
            pageSize: searchModel.PageSize
        );

        if (activityLog is null)
            return new ErpActivityLogsListModel();

        //prepare list model
        var customerIds = activityLog
            .GroupBy(logItem => logItem.CustomerId)
            .Select(logItem => logItem.Key);
        var activityLogCustomers = await _customerService.GetCustomersByIdsAsync(
            customerIds.ToArray()
        );
        var model = await new ErpActivityLogsListModel().PrepareToGridAsync(
            searchModel,
            activityLog,
            () =>
            {
                return activityLog.SelectAwait(async logItem =>
                {
                    //fill in model values from the entity
                    var logItemModel = logItem.ToModel<ErpActivityLogsModel>();
                    logItemModel.ErpActivityLogTypeName = Enum.GetName(
                        typeof(ErpActivityType),
                        logItem.ErpActivityLogTypeId
                    );

                    logItemModel.CustomerEmail = activityLogCustomers
                        ?.FirstOrDefault(x => x.Id == logItem.CustomerId)
                        ?.Email;

                    //convert dates to the user time
                    logItemModel.CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(
                        logItem.CreatedOnUtc,
                        DateTimeKind.Utc
                    );

                    return logItemModel;
                });
            }
        );

        return model;
    }

    #endregion
}
