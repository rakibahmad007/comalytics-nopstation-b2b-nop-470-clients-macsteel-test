using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Services.Localization;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models.B2CMacsteelExpressShops;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Factories;
public class B2CMacsteelExpressShopFactory : IB2CMacsteelExpressShopFactory
{
    private readonly IB2CMacsteelExpressShopService _b2CMacsteelExpressShopService;
    private readonly ILocalizationService _localizationService;

    public B2CMacsteelExpressShopFactory(
        IB2CMacsteelExpressShopService b2CMacsteelExpressShopService,
        ILocalizationService localizationService)
    {
        _localizationService = localizationService;
        _b2CMacsteelExpressShopService = b2CMacsteelExpressShopService;
    }

    public async Task<B2CMacsteelExpressShopListModel> PrepareB2CMacsteelExpressShopListModelAsync(B2CMacsteelExpressShopSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        var overrideActive = searchModel.SearchActiveId == 0 ? null : (bool?)(searchModel.SearchActiveId == 1);

        var expressShops = await _b2CMacsteelExpressShopService.GetAllB2CMacsteelExpressShopsAsync(searchModel.SearchMacsteelExpressShopCode, searchModel.SearchMacsteelExpressShopName,
            pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

        var model = await new B2CMacsteelExpressShopListModel().PrepareToGridAsync(searchModel, expressShops, () =>
        {
            return expressShops.SelectAwait(async expressShop =>
            {
                var expressShopModel = expressShop.ToModel<B2CMacsteelExpressShopModel>();

                return expressShopModel;
            });
        });

        return model;
    }

    public async Task<B2CMacsteelExpressShopModel> PrepareB2CMacsteelExpressShopModelAsync(B2CMacsteelExpressShopModel model, B2CMacsteelExpressShop b2CMacsteelExpressShop)
    {
        if (b2CMacsteelExpressShop != null)
            model = model ??= await Task.Run(() => b2CMacsteelExpressShop.ToModel<B2CMacsteelExpressShopModel>());

        return model;
    }

    public async Task<B2CMacsteelExpressShopSearchModel> PrepareB2CMacsteelExpressShopSearchModelAsync(B2CMacsteelExpressShopSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        searchModel.AvailableActiveOptions.Add(new SelectListItem
        {
            Value = "0",
            Text = await _localizationService.GetResourceAsync("Plugins.Payments.B2BCustomerAccount.SearchActive.All"),
        });
        searchModel.AvailableActiveOptions.Add(new SelectListItem
        {
            Value = "1",
            Text = await _localizationService.GetResourceAsync("Plugins.Payments.B2BCustomerAccount.SearchActive.ActiveOnly"),
        });
        searchModel.AvailableActiveOptions.Add(new SelectListItem
        {
            Value = "2",
            Text = await _localizationService.GetResourceAsync("Plugins.Payments.B2BCustomerAccount.SearchActive.InactiveOnly"),
        });

        searchModel.SearchActiveId = 0;
        searchModel.SetGridPageSize();

        return searchModel;
    }
}
