using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Plugin.Pickup.PickupInStore.Areas.Admin.Models.B2BSalesOrgPickupPoints;
using Nop.Plugin.Pickup.PickupInStore.Services;
using Nop.Services.Localization;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace Nop.Plugin.Pickup.PickupInStore.Areas.Admin.Factories;

public class B2BSalesOrgPickupPointFactory : IB2BSalesOrgPickupPointFactory
{
    private readonly ILocalizationService _localizationService;
    private readonly IB2BSalesOrgPickupPointService _b2BSalesOrgPickupPointService;
    private readonly IStorePickupPointService _storePickupPointService;

    public B2BSalesOrgPickupPointFactory(
        ILocalizationService localizationService,
        IB2BSalesOrgPickupPointService b2BSalesOrgPickupPointService,
        IStorePickupPointService storePickupPointService
        )
    {
        _localizationService = localizationService;
        _b2BSalesOrgPickupPointService = b2BSalesOrgPickupPointService;
        _storePickupPointService = storePickupPointService;
    }

    #region B2BSalesOrgPickupPoint

    public async Task<B2BSalesOrgPickupPointListModel> PrepareB2BSalesOrgPickupPointListModel(B2BSalesOrgPickupPointSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        // get B2BSalesOrgPickupPoints
        var b2BSalesOrgPickupPoints = await _b2BSalesOrgPickupPointService.GetAllB2BSalesOrgPickupPointsAsync(searchModel.B2BSalesOrgId,
            pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

        //prepare list model
        var model = await new B2BSalesOrgPickupPointListModel().PrepareToGridAsync(searchModel, b2BSalesOrgPickupPoints, () =>
        {
            return b2BSalesOrgPickupPoints.SelectAwait(async saleOrgPickupPoint =>
            {
                var pickupPoint = await _storePickupPointService.GetStorePickupPointByIdAsync(saleOrgPickupPoint.NopPickupPointId);
                //fill in model values from the entity
                var salesOrgPickupPointModel = new B2BSalesOrgPickupPointModel
                {
                    Id = saleOrgPickupPoint.Id,
                    PickupPointId = saleOrgPickupPoint.NopPickupPointId,
                    PickupPointName = pickupPoint?.Name,
                    B2BSalesOrgId = saleOrgPickupPoint.B2BSalesOrgId,
                };
                return salesOrgPickupPointModel;
            });
        });

        return model;
    }

    #endregion

    public async Task PrepareB2BStorePickupPoints(IList<SelectListItem> items, bool withSpecialDefaultItem = false)
    {
        ArgumentNullException.ThrowIfNull(items);

        //prepare available StorePickupPoints
        var availableStorePickupPoints = await _storePickupPointService.GetAllStorePickupPointsAsync();
        foreach (var pickupPoint in availableStorePickupPoints)
        {
            items.Add(new SelectListItem { Value = pickupPoint.Id.ToString(), Text = pickupPoint.Name });
        }

        if (withSpecialDefaultItem)
            PrepareDefaultItem(items);
    }

    protected virtual async void PrepareDefaultItem(IList<SelectListItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        //at now we use "0" as the default value
        const string value = "0";

        //prepare item text
        var defaultItemText = await _localizationService.GetResourceAsync("Admin.Common.All");

        //insert this default item at first
        items.Insert(0, new SelectListItem { Text = defaultItemText, Value = value });
    }
}
