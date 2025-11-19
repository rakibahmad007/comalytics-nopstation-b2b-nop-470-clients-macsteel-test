using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models.SpecialIncludeExcludes;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.SpecialIncludeExcludeService;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Factories;

public class SpecialIncludeExcludeModelFactory : ISpecialIncludeExcludeModelFactory
{
    #region fields

    private readonly IErpAccountService _erpAccountService;
    private readonly IErpSalesOrgService _erpSalesOrgService;
    private readonly ILocalizationService _localizationService;
    private readonly IB2BSpecialIncludeExcludeService _b2BSpecialIncludeExcludeService;
    private readonly IProductService _productService;

    #endregion

    #region Ctor

    public SpecialIncludeExcludeModelFactory(
        ILocalizationService localizationService,
        IB2BSpecialIncludeExcludeService b2BSpecialIncludeExcludeService,
        IProductService productService,
        IErpAccountService erpAccountService,
        IErpSalesOrgService erpSalesOrgService
    )
    {
        _localizationService = localizationService;
        _b2BSpecialIncludeExcludeService = b2BSpecialIncludeExcludeService;
        _productService = productService;
        _erpAccountService = erpAccountService;
        _erpSalesOrgService = erpSalesOrgService;
    }

    #endregion

    public async Task<SpecialIncludeExcludeSearchModel> PrepareSpecialIncludeExcludeSearchModelAsync(
        SpecialIncludeExcludeSearchModel model
    )
    {
        ArgumentNullException.ThrowIfNull(model);

        var salesOrgList = (await _erpSalesOrgService.GetAllErpSalesOrgAsync()).Where(so =>
            so.IsActive
        );

        model.AvailableSalesOrgs = salesOrgList
            .Select(so => new SelectListItem
            {
                Text = $"{so.Code} - {so.Code}",
                Value = $"{so.Id}",
            })
            .ToList();
        model.AvailableSalesOrgs.Insert(
            0,
            new SelectListItem { Text = "Select Sales Organization", Value = "-1" }
        );

        model.AvailablePublishedOptions.Add(
            new SelectListItem
            {
                Value = "0",
                Text = await _localizationService.GetResourceAsync(
                    "Admin.Catalog.Products.List.SearchPublished.All"
                ),
            }
        );
        model.AvailablePublishedOptions.Add(
            new SelectListItem
            {
                Value = "1",
                Text = await _localizationService.GetResourceAsync(
                    "Admin.Catalog.Products.List.SearchPublished.PublishedOnly"
                ),
            }
        );
        model.AvailablePublishedOptions.Add(
            new SelectListItem
            {
                Value = "2",
                Text = await _localizationService.GetResourceAsync(
                    "Admin.Catalog.Products.List.SearchPublished.UnpublishedOnly"
                ),
            }
        );

        model.AvailableStatuses.Add(
            new SelectListItem
            {
                Value = "0",
                Text = await _localizationService.GetResourceAsync(
                    "NopStation.Plugin.B2B.B2BB2CFeatures.SearchActive.All"
                ),
            }
        );
        model.AvailableStatuses.Add(
            new SelectListItem
            {
                Value = "1",
                Text = await _localizationService.GetResourceAsync(
                    "NopStation.Plugin.B2B.B2BB2CFeatures.SearchActive.ActiveOnly"
                ),
            }
        );
        model.AvailableStatuses.Add(
            new SelectListItem
            {
                Value = "2",
                Text = await _localizationService.GetResourceAsync(
                    "NopStation.Plugin.B2B.B2BB2CFeatures.SearchActive.InactiveOnly"
                ),
            }
        );

        model.SetGridPageSize();

        return model;
    }

    public async Task<SpecialIncludeExcludeListModel> PrepareSpecialIncludeExcludeListModelAsync(
        SpecialIncludeExcludeSearchModel model
    )
    {
        ArgumentNullException.ThrowIfNull(model);

        var specialIncludePagedList =
            await _b2BSpecialIncludeExcludeService.GetAllSpecialIncludeExcludesAsync(
                (SpecialType)model.Type,
                model.AccountName,
                model.AccountNumber,
                model.ErpSalesOrgId,
                GetStatus(model.IsActive),
                GetStatus(model.Published),
                model.Page,
                model.PageSize,
                false
            );

        return new SpecialIncludeExcludeListModel().PrepareToGrid(
            model,
            specialIncludePagedList,
            () => specialIncludePagedList
        );
    }

    private bool? GetStatus(string status)
    {
        switch (status)
        {
            case "0":
                return null;
            case "1":
                return true;
            case "2":
                return false;
            default:
                return null;
        }
    }

    public async Task<SpecialIncludeExcludeModel> PrepareSpecialIncludeExcludeModelAsync(
        SpecialIncludeExcludeModel model
    )
    {
        if (model.ErpAccountId > 0)
        {
            var b2bAccount = await _erpAccountService.GetErpAccountByIdAsync(
                model.ErpAccountId
            );
            model.ErpSalesOrgId = b2bAccount.ErpSalesOrgId;

            var salesOrg = await _erpSalesOrgService.GetErpSalesOrgByIdAsync(
                b2bAccount.ErpSalesOrgId
            );
            model.SalesOrgCode = salesOrg.Code;
            model.SalesOrgName = salesOrg.Name;

            model.AccountName = b2bAccount.AccountName;
            model.IsActive = model.IsActive;
        }
        else
        {
            model.IsActive = true;
        }

        if (model.ProductId > 0)
        {
            var product = await _productService.GetProductByIdAsync(model.ProductId);
            model.ProductName = product.Name;
        }

        return model;
    }
}
