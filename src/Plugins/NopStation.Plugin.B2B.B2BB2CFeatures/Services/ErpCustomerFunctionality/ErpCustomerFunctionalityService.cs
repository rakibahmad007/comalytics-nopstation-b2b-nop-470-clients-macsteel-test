using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Orders;
using Nop.Services.Security;
using NopStation.Plugin.B2B.B2BB2CFeatures.Model;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpSpecificationAttributeService;
using NopStation.Plugin.B2B.ERPIntegrationCore;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Infrastructure;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpCustomerFunctionality;

public class ErpCustomerFunctionalityService : IErpCustomerFunctionalityService
{
    #region Fields

    private readonly ICustomerService _customerService;
    private readonly IErpAccountService _erpAccountService;
    private readonly IErpOrderAdditionalDataService _erpOrderAdditionalDataService;
    private readonly IGenericAttributeService _genericAttributeService;
    private readonly IWorkContext _workContext;
    private readonly IStoreContext _storeContext;
    private readonly IOrderService _orderService;
    private readonly IErpNopUserService _erpNopUserService;
    private readonly IStaticCacheManager _staticCacheManager;
    private readonly B2BB2CFeaturesSettings _b2BB2CFeaturesSettings;
    private readonly IErpSpecificationAttributeService _erpSpecificationAttributeService;
    private readonly IErpSalesOrgService _erpSalesOrgService;
    private readonly ICategoryService _categoryService;
    private readonly IErpGroupPriceService _erpGroupPriceService;
    private readonly IErpSpecialPriceService _erpSpecialPriceService;
    private readonly IRepository<ErpCustomerConfiguration> _erpCustomerConfigurationRepository;
    private readonly IStaticCacheManager _cacheManager;
    private readonly IErpSalesRepService _erpSalesRepService;
    private readonly IPermissionService _permissionService;
    private readonly IProductAttributeService _productAttributeService;
    private readonly IErpWarehouseSalesOrgMapService _erpWarehouseSalesOrgMapService;
    private readonly IRepository<ErpWarehouseSalesOrgMap> _erpWarehouseSalesOrgMapRepository;
    private readonly IRepository<ErpSalesOrg> _erpSalesOrgRepository;

    #endregion

    #region Ctor

    public ErpCustomerFunctionalityService(ICustomerService customerService,
        IErpAccountService erpAccountService,
        IErpOrderAdditionalDataService erpOrderAdditionalDataService,
        IGenericAttributeService genericAttributeService,
        IWorkContext workContext,
        IStoreContext storeContext,
        IOrderService orderService,
        IErpNopUserService erpNopUserService,
        IStaticCacheManager staticCacheManager,
        B2BB2CFeaturesSettings b2BB2CFeaturesSettings,
        IErpSpecificationAttributeService erpSpecificationAttributeService,
        IErpSalesOrgService erpSalesOrgService,
        ICategoryService categoryService,
        IErpGroupPriceService erpGroupPriceService,
        IRepository<ErpCustomerConfiguration> erpCustomerConfigurationRepository,
        IStaticCacheManager cacheManager,
        IErpSpecialPriceService erpSpecialPriceService,
        IErpSalesRepService erpSalesRepService,
        IPermissionService permissionService,
        IProductAttributeService productAttributeService,
        IErpWarehouseSalesOrgMapService erpWarehouseSalesOrgMapService,
        IRepository<ErpWarehouseSalesOrgMap> erpWarehouseSalesOrgMapRepository,
        IRepository<ErpSalesOrg> erpSalesOrgRepository)
    {
        _customerService = customerService;
        _erpAccountService = erpAccountService;
        _erpOrderAdditionalDataService = erpOrderAdditionalDataService;
        _genericAttributeService = genericAttributeService;
        _workContext = workContext;
        _storeContext = storeContext;
        _orderService = orderService;
        _erpNopUserService = erpNopUserService;
        _staticCacheManager = staticCacheManager;
        _b2BB2CFeaturesSettings = b2BB2CFeaturesSettings;
        _erpSpecificationAttributeService = erpSpecificationAttributeService;
        _erpSalesOrgService = erpSalesOrgService;
        _categoryService = categoryService;
        _erpGroupPriceService = erpGroupPriceService;
        _erpCustomerConfigurationRepository = erpCustomerConfigurationRepository;
        _cacheManager = cacheManager;
        _erpSpecialPriceService = erpSpecialPriceService;
        _erpSalesRepService = erpSalesRepService;
        _permissionService = permissionService;
        _productAttributeService = productAttributeService;
        _erpWarehouseSalesOrgMapService = erpWarehouseSalesOrgMapService;
        _erpWarehouseSalesOrgMapRepository = erpWarehouseSalesOrgMapRepository;
        _erpSalesOrgRepository = erpSalesOrgRepository;
    }

    #endregion

    #region Util

    private async Task<(decimal, decimal)> GetGroupPriceAsync(ErpAccount erpAccount, int productId)
    {
        var priceGroupProductPricing = await _erpGroupPriceService
            .GetErpGroupPriceByErpPriceGroupCodeAndProductId(erpAccount.B2BPriceGroupCodeId ?? 0, productId);

        if (priceGroupProductPricing != null && priceGroupProductPricing.Id > 0)
        {
            return (priceGroupProductPricing.Price, decimal.Zero);
        }

        return (decimal.Zero, decimal.Zero);
    }

    private async Task<(decimal, decimal)> GetSpecialPriceAsync(ErpAccount erpAccount, int productId)
    {
        var productSpecialPricing = await _erpSpecialPriceService
            .GetErpSpecialPricesByErpAccountIdAndNopProductIdAsync(erpAccount.Id, productId);

        if (productSpecialPricing != null && productSpecialPricing.Id > 0)
        {
            return _b2BB2CFeaturesSettings.EnableOnlineSavings
                ? (productSpecialPricing.Price, productSpecialPricing.DiscountPerc)
                : (productSpecialPricing.Price, decimal.Zero);
        }

        return (decimal.Zero, decimal.Zero);
    }

    #endregion

    #region Methods

    public async Task<bool> IsHideAddToCartAsync()
    {
        var isHideAddToCart = _b2BB2CFeaturesSettings.IsShowLoginForPrice
            && !await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync());
        return isHideAddToCart;
    }

    public async Task ClearGenericAttributeOfB2BQuoteOrderAsync()
    {
        await _genericAttributeService.SaveAttributeAsync<int?>(
            await _workContext.GetCurrentCustomerAsync(),
            B2BB2CFeaturesDefaults.B2BConvertedQuoteB2BOrderId,
            null,
            (await _storeContext.GetCurrentStoreAsync()).Id
        );
    }

    public async Task ClearGenericAttributeOfB2CQuoteOrderAsync()
    {
        await _genericAttributeService.SaveAttributeAsync<int?>(
            await _workContext.GetCurrentCustomerAsync(),
            B2BB2CFeaturesDefaults.B2CConvertedQuoteB2COrderId,
            null,
            (await _storeContext.GetCurrentStoreAsync()).Id
        );
    }

    public async Task<bool> IsOriginalCustomerMultiAccountBuyer()
    {
        var salesReps = await _erpSalesRepService.GetErpSalesRepsByNopCustomerIdAsync(
            _workContext.OriginalCustomerIfImpersonated.Id,
            true
        );
        return salesReps.Any(salesRep => salesRep.SalesRepType == SalesRepType.MultiBuyers);
    }

    public async Task<(bool, bool)> IsTermsAndConditionAcceptRequiredAndIsFirstTimeAsync()
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (!await _customerService.IsRegisteredAsync(customer)
            || (_workContext.OriginalCustomerIfImpersonated != null
                && (await _customerService.IsAdminAsync(_workContext.OriginalCustomerIfImpersonated)
                    || !await IsOriginalCustomerMultiAccountBuyer())
            )
        )
        {
            return (false, false);
        }

        if (await IsCurrentCustomerInErpSalesRepRoleAsync())
        {
            return (false, false);
        }

        var dateOfTermsAndConditionChecked =
            await _genericAttributeService.GetAttributeAsync<DateTime?>(
                customer,
                ERPIntegrationCoreDefaults.CustomerDateOfTermsAndConditionCheckedAttributeName,
                defaultValue: null
            );

        if (!dateOfTermsAndConditionChecked.HasValue)
        {
            return (true, true);
        }

        if (
            dateOfTermsAndConditionChecked.Value.Date
            < _b2BB2CFeaturesSettings.LastDateTimeOfTCUpdate.Date
        )
        {
            return (true, false);
        }

        return (false, false);
    }

    public async Task<bool> CheckAndUpdateGenericAttributeOfB2BQuoteOrder(
        int erpOrderId,
        IList<ShoppingCartItem> currentShoppingCartItems
    )
    {
        var erpOrderAdditionalData =
            await _erpOrderAdditionalDataService.GetErpOrderAdditionalDataByIdAsync(erpOrderId);

        if (
            await CheckAndUpdateGenericAttributeOfERPQuoteOrder(
                erpOrderAdditionalData,
                currentShoppingCartItems
            )
        )
        {
            return true;
        }
        else
        {
            await ClearGenericAttributeOfB2BQuoteOrderAsync();
            return false;
        }
    }

    public async Task<bool> CheckAndUpdateGenericAttributeOfB2CQuoteOrder(
        int erpOrderId,
        IList<ShoppingCartItem> currentShoppingCartItems
    )
    {
        var erpOrderAdditionalData =
            await _erpOrderAdditionalDataService.GetErpOrderAdditionalDataByIdAsync(erpOrderId);
        if (
            await CheckAndUpdateGenericAttributeOfERPQuoteOrder(
                erpOrderAdditionalData,
                currentShoppingCartItems
            )
        )
        {
            return true;
        }
        else
        {
            await ClearGenericAttributeOfB2CQuoteOrderAsync();
            return false;
        }
    }

    public async Task<bool> CheckAndUpdateGenericAttributeOfERPQuoteOrder(
        ErpOrderAdditionalData b2BOrderPerAccount,
        IList<ShoppingCartItem> shoppingCartItems
    )
    {
        if (b2BOrderPerAccount == null)
            return false;

        if (!await _erpOrderAdditionalDataService.CheckQuoteOrderStatusAsync(b2BOrderPerAccount))
        {
            return false;
        }

        if (shoppingCartItems == null || !shoppingCartItems.Any())
        {
            return false;
        }

        var orderItems = await _orderService.GetOrderItemsAsync(b2BOrderPerAccount.NopOrderId);
        if (orderItems == null || !orderItems.Any())
        {
            return false;
        }

        var isQuoteItemExist = shoppingCartItems.Any(s =>
            orderItems.Any(o => o.ProductId == s.ProductId)
        );

        if (!isQuoteItemExist)
        {
            return false;
        }

        return true;
    }

    public async Task<ErpAccount> GetActiveErpAccountByCustomerAsync(Customer customer)
    {
        var key = _staticCacheManager.PrepareKeyForDefaultCache(
            ERPIntegrationCoreDefaults.ErpAccountByCustomerCacheKey,
            customer.Id,
            string.Join(",", await _customerService.GetCustomerRoleIdsAsync(customer))
        );

        return await _staticCacheManager.Get(
            key,
            async () =>
            {
                var erpNopUser = await GetActiveErpNopUserByCustomerAsync(customer);
                if (erpNopUser != null && !erpNopUser.IsDeleted && erpNopUser.IsActive)
                {
                    var erpAccount = await _erpAccountService.GetErpAccountByIdWithActiveAsync(
                        erpNopUser.ErpAccountId
                    );
                    if (erpAccount != null)
                        return erpAccount;
                }
                return null;
            }
        );
    }

    public async Task InsertErpCustomerConfigurationAsync(
        ErpCustomerConfiguration erpCustomerConfiguration
    )
    {
        ArgumentNullException.ThrowIfNull(erpCustomerConfiguration);

        await _erpCustomerConfigurationRepository.InsertAsync(erpCustomerConfiguration);
        await _cacheManager.RemoveByPrefixAsync(
            B2BB2CFeaturesDefaults.B2BCustomerConfigurationPrefixCacheKey
        );
    }

    public async Task UpdateErpCustomerConfigurationAsync(
        ErpCustomerConfiguration erpCustomerConfiguration
    )
    {
        ArgumentNullException.ThrowIfNull(erpCustomerConfiguration);

        await _erpCustomerConfigurationRepository.UpdateAsync(erpCustomerConfiguration);
        await _cacheManager.RemoveByPrefixAsync(
            B2BB2CFeaturesDefaults.B2BCustomerConfigurationPrefixCacheKey
        );
    }

    public async Task<ErpCustomerConfiguration> CreateErpCustomerConfigurationByNopCustomerAsync(
        Customer customer
    )
    {
        ArgumentNullException.ThrowIfNull(customer);

        var b2BAccount = await GetActiveErpAccountByCustomerAsync(customer);
        if (b2BAccount == null)
            throw new ArgumentNullException(nameof(b2BAccount));

        var erpCustomerConfiguration = new ErpCustomerConfiguration()
        {
            NopCustomerId = customer.Id,
            IsHidePricingNote = false,
            IsHideWeightInfo = false,
        };

        await InsertErpCustomerConfigurationAsync(erpCustomerConfiguration);
        return erpCustomerConfiguration;
    }

    public async Task<ErpCustomerConfiguration> CreateErpCustomerConfigurationByNopCustomerIdAsync(
        int nopCustomerId
    )
    {
        var customer = await _customerService.GetCustomerByIdAsync(nopCustomerId);
        return await CreateErpCustomerConfigurationByNopCustomerAsync(customer);
    }

    public async Task<ErpCustomerConfiguration> GetErpCustomerConfigurationByNopCustomerIdAsync(
        int nopCustomerId
    )
    {
        if (nopCustomerId < 1)
            return null;

        var cacheKey = new CacheKey(
            string.Format(
                B2BB2CFeaturesDefaults.B2BCustomerConfigurationByIdCacheKey,
                nopCustomerId
            )
        );

        var customerConfiguration = await _cacheManager.GetAsync(
            cacheKey,
            async () =>
            {
                var query = _erpCustomerConfigurationRepository.Table;
                query = query.Where(b => b.NopCustomerId == nopCustomerId);
                return await query.FirstOrDefaultAsync();
            }
        );

        customerConfiguration ??= await CreateErpCustomerConfigurationByNopCustomerIdAsync(nopCustomerId);

        return customerConfiguration;
    }

    public async Task<ErpCustomerConfiguration> GetErpCustomerConfigurationOfCurrentCustomerAsync()
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (customer == null || !await _customerService.IsRegisteredAsync(customer))
        {
            return null;
        }

        var erpAccount = await GetActiveErpAccountByCustomerAsync(customer);
        if (erpAccount == null)
        {
            return null;
        }

        return await GetErpCustomerConfigurationByNopCustomerIdAsync(customer.Id);
    }

    public async Task<bool> IsErpAccountBlockSalesOrderAsync(Customer customer)
    {
        if (await IsCustomerInB2BCustomerRoleAsync(customer))
        {
            var erpAccount = await _erpAccountService.GetActiveErpAccountByCustomerIdAsync(
                customer.Id
            );
            if (
                erpAccount == null
                || erpAccount.ErpAccountStatusType == ErpAccountStatusType.BlockOrder
            )
                return true;
        }
        return false;
    }

    public async Task<ErpNopUser> GetActiveErpNopUserByCustomerAsync(Customer customer)
    {
        if (customer == null)
            return null;

        return await _erpNopUserService.GetErpNopUserByCustomerIdAsync(customer.Id, showHidden: false);
    }

    public async Task<bool> IsConsideredAsB2BOrderByB2BUser(ErpNopUser b2BUser)
    {
        if (b2BUser == null || !b2BUser.IsActive)
        {
            return false;
        }
        var erpAccount = await _erpAccountService.GetErpAccountByIdAsync(b2BUser.ErpAccountId);
        if (erpAccount == null || erpAccount.IsDeleted || !erpAccount.IsActive)
        {
            return false;
        }

        return true;
    }

    public async Task<bool> IsConsideredAsB2COrderByB2CUser(ErpNopUser b2CUser)
    {
        if (b2CUser == null || !b2CUser.IsActive)
            return false;

        var b2BAccount = await _erpAccountService.GetErpAccountByIdWithActiveAsync(
            b2CUser.ErpAccountId
        );
        if (b2BAccount == null)
            return false;

        return true;
    }

    public async Task<bool> IsSalesOrderInvalidForCurrentCustomerAsync()
    {
        bool isQuoteOrder;
        var currCustomer = await _workContext.GetCurrentCustomerAsync();
        var store = await _storeContext.GetCurrentStoreAsync();
        var nopUser = await GetActiveErpNopUserByCustomerAsync(currCustomer);
        var b2bUser = nopUser?.ErpUserType == ErpUserType.B2BUser ? nopUser : null;
        if (b2bUser != null && b2bUser.Id > 0)
        {
            isQuoteOrder = await _genericAttributeService.GetAttributeAsync<bool>(
                currCustomer,
                B2BB2CFeaturesDefaults.B2BQouteOrderAttribute,
                store.Id
            );
            if (isQuoteOrder)
                return false;
        }

        var b2CUser = nopUser?.ErpUserType == ErpUserType.B2CUser ? nopUser : null;
        if (b2CUser != null && b2CUser.Id > 0)
        {
            isQuoteOrder = await _genericAttributeService.GetAttributeAsync<bool>(
                currCustomer,
                B2BB2CFeaturesDefaults.B2CQouteOrderAttribute,
                store.Id
            );
            if (isQuoteOrder)
                return false;
        }

        return await IsErpAccountBlockSalesOrderAsync(currCustomer);
    }

    public async Task<bool> CheckQuoteOrderStatusAsync(ErpOrderAdditionalData erpOrder)
    {
        if (erpOrder == null || erpOrder.ErpOrderType == ErpOrderType.B2BSalesOrder)
            return false;

        if (erpOrder.QuoteExpiryDate == null || string.IsNullOrEmpty(erpOrder.ERPOrderStatus))
            return false;

        if (erpOrder.QuoteExpiryDate.Value.Date < DateTime.UtcNow.Date)
            return false;

        if (erpOrder.QuoteSalesOrderId.HasValue && erpOrder.QuoteSalesOrderId.Value > 0)
            return false;

        return erpOrder.ERPOrderStatus == B2BB2CFeaturesDefaults.ErpOrderStatusApproved
            || erpOrder.ERPOrderStatus == B2BB2CFeaturesDefaults.ErpOrderStatusPendingApproval
            || erpOrder.ERPOrderStatus == nameof(OrderStatus.Complete);
    }

    public async Task<bool> CheckAllowAddressEdit(ErpAccount b2BAccount)
    {
        if (b2BAccount == null)
        {
            return false;
        }

        return b2BAccount.OverrideAddressEditOnCheckoutConfigSetting
            ? b2BAccount.AllowAccountsAddressEditOnCheckout
            : _b2BB2CFeaturesSettings.AllowAddressEditOnCheckoutForAll;
    }

    public async Task<bool> B2BDisableBuyButtonAsync(
        Product product,
        bool currentValue,
        int totalStockQuantity = 0
    )
    {
        var currentCustomer = await _workContext.GetCurrentCustomerAsync();
        var b2BAccount = await GetActiveErpAccountByCustomerAsync(currentCustomer);

        // If it is not a B2B account, then keep the current value
        if (b2BAccount == null)
            return currentValue;

        if (await _permissionService.AuthorizeAsync(ErpPermissionProvider.DisplayB2BPrices))
        {
            return false;
        }

        var erpUser = await GetActiveErpNopUserByCustomerAsync(currentCustomer);

        var isB2CUser = erpUser != null && erpUser.ErpUserType == ErpUserType.B2CUser;

        var isB2BBackOrderingAllowed = await CheckAllowBackOrderingByAccountAsync(b2BAccount);

        // If backordering is allowed and it is not B2C User or currentValue is already true, keep the current value
        if ((isB2BBackOrderingAllowed && !isB2CUser) || currentValue)
            return currentValue;

        var disable = false;

        switch (product.ManageInventoryMethod)
        {
            case ManageInventoryMethod.DontManageStock:
                // Do nothing
                break;

            case ManageInventoryMethod.ManageStock:
                if (product.BackorderMode == BackorderMode.NoBackorders && totalStockQuantity < 1)
                {
                    disable = true;
                }
                break;

            case ManageInventoryMethod.ManageStockByAttributes:
                var combinations =
                    await _productAttributeService.GetAllProductAttributeCombinationsAsync(product.Id);
                var combinationTotalStock = combinations?.Sum(a => a.StockQuantity);
                if (combinationTotalStock.HasValue && combinationTotalStock < 1)
                {
                    disable = true;
                }
                break;

            default:
                break;
        }

        return disable;
    }

    public async Task<(DateTime, DateTime)> GetMinimumAndMaximumDeliveryDateForShippingAddress()
    {
        var minDeliveryDate = DateTime.Now.AddDays(_b2BB2CFeaturesSettings.DeliveryDays);
        if (minDeliveryDate.Hour > _b2BB2CFeaturesSettings.CutoffTime)
            minDeliveryDate = minDeliveryDate.AddDays(1);
        var maxDeliveryDate = DateTime.Now.AddMonths(6);

        return (minDeliveryDate, maxDeliveryDate);
    }

    public async Task ClearCurrentCustomerYearlySavingsCacheAsync(int customerId)
    {
        var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(
            new CacheKey(string.Format(B2BB2CFeaturesDefaults.B2BUserCurrentYearSavingsByCustomerCacheKey, customerId))
        );

        await _staticCacheManager.RemoveAsync(cacheKey);
    }

    public async Task ClearCurrentCustomerAllTimeSavingsCacheAsync(int customerId)
    {
        var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(
            new CacheKey(string.Format(B2BB2CFeaturesDefaults.B2BUserAllTimeSavingsByCustomerCacheKey, customerId))
        );

        await _staticCacheManager.RemoveAsync(cacheKey);
    }

    #region Customer Role based check

    public async Task<bool> IsCurrentCustomerInErpSalesRepRoleAsync()
    {
        return await IsCustomerInB2BSalesRepRoleAsync(await _workContext.GetCurrentCustomerAsync());
    }

    public async Task<bool> IsCustomerInB2BCustomerRoleAsync(Customer customer)
    {
        return await _customerService.IsInCustomerRoleAsync(customer, ERPIntegrationCoreDefaults.B2BCustomerRoleSystemName);
    }

    public async Task<bool> IsCustomerInB2BCustomerRoleAsync(int customerId = 0)
    {
        var customer = await _customerService.GetCustomerByIdAsync(customerId);
        if (customerId == 0 || customer == null)
            return false;
        return await IsCustomerInB2BCustomerRoleAsync(customer);
    }

    public async Task<bool> IsCurrentCustomerInB2BCustomerRoleAsync()
    {
        return await IsCustomerInB2BCustomerRoleAsync(await _workContext.GetCurrentCustomerAsync());
    }

    public async Task<bool> IsCustomerInB2CCustomerRoleAsync(Customer customer)
    {
        return await _customerService.IsInCustomerRoleAsync(customer, ERPIntegrationCoreDefaults.B2CCustomerRoleSystemName);
    }

    public async Task<bool> IsCustomerInB2CCustomerRoleAsync(int customerId = 0)
    {
        var customer = await _customerService.GetCustomerByIdAsync(customerId);
        if (customerId == 0 || customer == null)
            return false;
        return await IsCustomerInB2CCustomerRoleAsync(customer);
    }

    public async Task<bool> IsCurrentCustomerInB2CCustomerRoleAsync()
    {
        return await IsCustomerInB2CCustomerRoleAsync(await _workContext.GetCurrentCustomerAsync());
    }


    public async Task<ErpFilterInfoModel> GetErpFilterInfoModel()
    {
        var model = new ErpFilterInfoModel();
        var erpAccount = await GetActiveErpAccountByCustomerAsync(
            await _workContext.GetCurrentCustomerAsync()
        );
        if (erpAccount == null)
        {
            return model;
        }

        model.IsErpAccount = true;
        model.ErpAccountId = erpAccount.Id;
        model.ErpSalesOrganisationId = erpAccount.ErpSalesOrgId;

        if (erpAccount.PreFilterFacets?.Trim() == null && erpAccount.SpecialIncludes?.Trim() == null)
        {
            model.ErpFilterFacetReturnNoProduct = true;
            return model;
        }
        else
        {
            if (erpAccount.PreFilterFacets?.Trim() != null)
            {
                var specIds =
                    await _erpSpecificationAttributeService.GetSpecificationAttributeOptionIdsByNames(
                        _b2BB2CFeaturesSettings.PreFilterFacetSpecificationAttributeId,
                        erpAccount.PreFilterFacets.Trim(),
                        erpAccount.Id
                    );

                model.UsePriceGroupPricing = _b2BB2CFeaturesSettings.UseProductGroupPrice;
                model.PriceGroupCodeId = erpAccount.B2BPriceGroupCodeId ?? 0;

                foreach (var specId in specIds)
                {
                    if (!model.PreFilterFacetSpecIds.Contains(specId))
                        model.PreFilterFacetSpecIds.Add(specId);
                }
            }
        }

        if (erpAccount.SpecialIncludes?.Trim() != null)
        {
            var specialIncludeSpecIds =
                await _erpSpecificationAttributeService.GetSpecificationAttributeOptionIdsByNames(
                    _b2BB2CFeaturesSettings.PreFilterFacetSpecificationAttributeId,
                    erpAccount.SpecialIncludes.Trim(),
                    erpAccount.Id
                );

            foreach (var spIncludId in specialIncludeSpecIds)
            {
                if (!model.PreFilterFacetSpecIds.Contains(spIncludId))
                    model.PreFilterFacetSpecIds.Add(spIncludId);
            }
        }

        if (!model.PreFilterFacetSpecIds.Any())
        {
            model.ErpFilterFacetReturnNoProduct = true;
            return model;
        }

        if (erpAccount.SpecialExcludes?.Trim() != null)
        {
            var specialExcludeSpecIds =
                await _erpSpecificationAttributeService.GetSpecificationAttributeOptionIdsForExcludeByNames(
                    _b2BB2CFeaturesSettings.PreFilterFacetSpecificationAttributeId,
                    erpAccount.SpecialExcludes.Trim()
                );
            foreach (var spExcludId in specialExcludeSpecIds)
            {
                if (!model.SpecialExcludeSpecIds.Contains(spExcludId))
                    model.SpecialExcludeSpecIds.Add(spExcludId);
            }
        }
        if (model.SpecialExcludeSpecIds != null && model.SpecialExcludeSpecIds.Any())
        {
            model.ExcludedProductIds =
                await _erpSpecificationAttributeService.GetProductIdBySpecificationAttributeOptionNames(
                    _b2BB2CFeaturesSettings.PreFilterFacetSpecificationAttributeId,
                    erpAccount.SpecialExcludes?.Trim(),
                    erpAccount.Id
                );
        }

        return model;
    }

    public async Task<(decimal, decimal)> GetErpProductPriceAndDiscountPriceByErpAccountAndProductAsync(
        ErpAccount b2BAccount,
        int productId
    )
    {
        var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(
            B2BB2CFeaturesDefaults.ErpProductPricingCommonCacheKey,
            b2BAccount.Id,
            _b2BB2CFeaturesSettings.UseProductGroupPrice,
            b2BAccount.B2BPriceGroupCodeId,
            productId
        );

        return await _staticCacheManager.GetAsync(
            cacheKey,
            async () =>
            {
                if (_b2BB2CFeaturesSettings.UseProductGroupPrice)
                {
                    var priceGroupProductPricing =
                        await _erpGroupPriceService.GetErpGroupPriceByErpPriceGroupCodeAndProductId(
                            b2BAccount.B2BPriceGroupCodeId ?? 0,
                            productId
                        );
                    if (priceGroupProductPricing != null && priceGroupProductPricing.Id > 0)
                    {
                        return (priceGroupProductPricing.Price, decimal.Zero);
                    }
                    else
                    {
                        return (decimal.Zero, decimal.Zero);
                    }
                }
                else
                {
                    var productPricing =
                        await _erpSpecialPriceService.GetErpSpecialPricesByErpAccountIdAndNopProductIdAsync(
                            b2BAccount.Id,
                            productId
                        );

                    if (productPricing != null && productPricing.Id > 0)
                    {
                        if (_b2BB2CFeaturesSettings.EnableOnlineSavings)
                            return (productPricing.Price, productPricing.DiscountPerc);
                        else
                            return (productPricing.Price, 0);
                    }
                    else
                    {
                        return (decimal.Zero, decimal.Zero);
                    }
                }
            }
        );
    }

    public async Task<bool> CheckAllowBackOrderingByAccountAsync(ErpAccount erpAccount)
    {
        if (erpAccount == null)
        {
            return false;
        }

        return erpAccount.OverrideBackOrderingConfigSetting ?
            erpAccount.AllowAccountsBackOrdering : _b2BB2CFeaturesSettings.AllowBackOrderingForAll;
    }

    public async Task<bool> IsTheProductFromSpecialCategoryAsync(Product product)
    {
        if (product == null)
            return false;

        var customerRoles = await _customerService.GetCustomerRolesAsync(
            await _workContext.GetCurrentCustomerAsync()
        );
        var customerRoleNames = await customerRoles.Select(r => r.Name).ToListAsync();
        var specialCatIdsToShow = (
            await _erpSalesOrgService.GetSalesOrganisationsByCodesAsync(customerRoleNames)
        ).Select(so => so.SpecialsCategoryId);

        if (specialCatIdsToShow.Any())
        {
            var productCategories = await _categoryService.GetProductCategoriesByProductIdAsync(
                product.Id
            );
            return productCategories?.Any(x => specialCatIdsToShow.Contains(x.CategoryId)) ?? false;
        }
        return false;
    }

    public async Task<Dictionary<int, bool>> AreTheProductsFromSpecialCategoryAsync(IList<Product> products)
    {
        var result = new Dictionary<int, bool>();
        if (products == null || !products.Any())
            return result;

        // 1. Get current customer roles and special category IDs
        var customer = await _workContext.GetCurrentCustomerAsync();
        var customerRoles = await _customerService.GetCustomerRolesAsync(customer);
        var customerRoleNames = customerRoles.Select(r => r.Name).ToList();
        var specialCatIdsToShow = (await _erpSalesOrgService.GetSalesOrganisationsByCodesAsync(customerRoleNames))
            .Select(so => so.SpecialsCategoryId)
            .ToList();

        if (specialCatIdsToShow.Count == 0)
        {
            // No special categories, all false
            foreach (var product in products)
                result[product.Id] = false;
            return result;
        }

        // 2. Get all product-category mappings in bulk
        var productIds = products.Select(p => p.Id).ToArray();
        var productCategoryIdsDict = await _categoryService.GetProductCategoryIdsAsync(productIds);

        // 3. For each product, check if any of its categories is in the specialCatIdsToShow
        foreach (var product in products)
        {
            var categoryIds = productCategoryIdsDict.TryGetValue(product.Id, out var ids) ? ids : Array.Empty<int>();
            result[product.Id] = categoryIds.Any(cid => specialCatIdsToShow.Contains(cid));
        }

        return result;
    }

    public async Task<string> GetPricingNoteAsync(ErpAccount erpAccount, Product product)
    {
        var erpSpecialPrice =
            await _erpSpecialPriceService.GetErpSpecialPricesByErpAccountIdAndNopProductIdAsync(
                erpAccount.Id,
                product.Id
            );

        if (erpSpecialPrice == null)
            return string.Empty;

        return  await _erpSpecialPriceService.GetProductPricingNoteByErpSpecialPriceAsync(
            erpSpecialPrice,
            _b2BB2CFeaturesSettings.UseProductGroupPrice,
            erpSpecialPrice.Price == _b2BB2CFeaturesSettings.ProductQuotePrice
        );
    }

    public async Task AddOrUpdateB2CUserSpecialRolesAsync(Customer nopCustomer, int oldSalesOrgId, int newSalesOrgId)
    {
        if (nopCustomer == null)
            return;

        var erpSalesOrgWarehouseMap = _erpWarehouseSalesOrgMapRepository.Table;
        var erpSalesOrgList = _erpSalesOrgRepository.Table;
        var b2CSalesOrgWarehousesList = erpSalesOrgWarehouseMap
            .Where(x => x.IsB2CWarehouse == true);
        var b2BSalesOrgWarehousesList = erpSalesOrgWarehouseMap
            .Where(x => x.IsB2CWarehouse == false);

        if (oldSalesOrgId > 0)
        {
            var oldB2CWarehouses = b2CSalesOrgWarehousesList
                .Where(x => x.ErpSalesOrgId == oldSalesOrgId)
                .ToList();

            foreach (var oldB2C in oldB2CWarehouses)
            {
                var b2bWarehouse = b2BSalesOrgWarehousesList
                    .FirstOrDefault(x =>
                        x.WarehouseCode == oldB2C.WarehouseCode
                    );

                if (b2bWarehouse == null)
                    continue;

                var oldSalesOrg = erpSalesOrgList
                    .FirstOrDefault(x => x.Id == b2bWarehouse.ErpSalesOrgId);

                if (oldSalesOrg == null)
                    continue;

                var role = await _customerService.GetCustomerRoleBySystemNameAsync(oldSalesOrg.Code);
                if (role == null)
                    continue;

                if (await _customerService.IsInCustomerRoleAsync(nopCustomer, role.SystemName))
                {
                    await _customerService.RemoveCustomerRoleMappingAsync(nopCustomer, role);
                }
            }
        }

        if (newSalesOrgId > 0)
        {
            var newB2CWarehouses = b2CSalesOrgWarehousesList
                .Where(x => x.ErpSalesOrgId == newSalesOrgId)
                .ToList();
            foreach (var newB2C in newB2CWarehouses)
            {
                var b2bWarehouse = b2BSalesOrgWarehousesList
                    .FirstOrDefault(x =>
                        x.WarehouseCode == newB2C.WarehouseCode
                    );

                if (b2bWarehouse == null)
                    continue;

                var newSalesOrg = erpSalesOrgList
                    .FirstOrDefault(x => x.Id == b2bWarehouse.ErpSalesOrgId);

                if (newSalesOrg == null)
                    continue;

                var newSpecialRole = await _customerService.GetCustomerRoleBySystemNameAsync(newSalesOrg?.Code ?? string.Empty);

                if (newSpecialRole != null && !await _customerService.IsInCustomerRoleAsync(nopCustomer, newSpecialRole.SystemName))
                {
                    await _customerService.AddCustomerRoleMappingAsync(
                        new CustomerCustomerRoleMapping
                        {
                            CustomerId = nopCustomer.Id,
                            CustomerRoleId = newSpecialRole.Id
                        }
                    );
                }
            }
        }
    }

    public async Task<bool> IsCustomerInB2BQuoteAssistantRoleAsync(int customerId = 0)
    {
        var customer = await _customerService.GetCustomerByIdAsync(customerId);
        if (customerId == 0 || customer == null)
            return false;
        return await IsCustomerInB2BQuoteAssistantRoleAsync(customer);
    }

    public async Task<bool> IsCustomerInB2BQuoteAssistantRoleAsync(Customer customer)
    {
        return await _customerService.IsInCustomerRoleAsync(customer, ERPIntegrationCoreDefaults.B2BQuoteAssistantRoleSystemName);
    }

    public async Task<bool> IsCurrentCustomerInB2BQuoteAssistantRoleAsync()
    {
        return await IsCustomerInB2BQuoteAssistantRoleAsync(await _workContext.GetCurrentCustomerAsync());
    }

    public async Task<bool> IsCustomerInB2BOrderAssistantRoleAsync(Customer customer)
    {
        return await _customerService.IsInCustomerRoleAsync(customer, ERPIntegrationCoreDefaults.B2BOrderAssistantRoleSystemName);
    }

    public async Task<bool> IsCustomerInB2BOrderAssistantRoleAsync(int customerId = 0)
    {
        var customer = await _customerService.GetCustomerByIdAsync(customerId);
        if (customerId == 0 || customer == null)
            return false;
        return await IsCustomerInB2BOrderAssistantRoleAsync(customer);
    }

    public async Task<bool> IsCurrentCustomerInB2BOrderAssistantRoleAsync()
    {
        return await IsCustomerInB2BOrderAssistantRoleAsync(await _workContext.GetCurrentCustomerAsync());
    }

    public async Task<bool> IsCustomerInB2BB2CAdminRoleAsync(Customer customer)
    {
        return await _customerService.IsInCustomerRoleAsync(customer, ERPIntegrationCoreDefaults.B2BB2CAdminRoleSystemName);
    }

    public async Task<bool> IsCustomerInB2BB2CAdminRoleAsync(int customerId = 0)
    {
        var customer = await _customerService.GetCustomerByIdAsync(customerId);
        if (customerId == 0 || customer == null)
            return false;
        return await IsCustomerInB2BB2CAdminRoleAsync(customer);
    }

    public async Task<bool> IsCurrentCustomerInB2BB2CAdminRoleAsync()
    {
        return await IsCustomerInB2BB2CAdminRoleAsync(await _workContext.GetCurrentCustomerAsync());
    }

    public async Task<bool> IsCustomerInB2BCustomerAccountingPersonnelRoleAsync(Customer customer)
    {
        return await _customerService.IsInCustomerRoleAsync(customer, ERPIntegrationCoreDefaults.B2BCustomerAccountingPersonnelRoleSystemName);
    }

    public async Task<bool> IsCustomerInB2BCustomerAccountingPersonnelRoleAsync(int customerId = 0)
    {
        var customer = await _customerService.GetCustomerByIdAsync(customerId);
        if (customerId == 0 || customer == null)
            return false;
        return await IsCustomerInB2BCustomerAccountingPersonnelRoleAsync(customer);
    }

    public async Task<bool> IsCurrentCustomerInB2BCustomerAccountingPersonnelRoleAsync()
    {
        return await IsCustomerInB2BCustomerAccountingPersonnelRoleAsync(await _workContext.GetCurrentCustomerAsync());
    }

    public async Task<bool> IsCustomerInB2BSalesRepRoleAsync(Customer customer)
    {
        return await _customerService.IsInCustomerRoleAsync(customer, ERPIntegrationCoreDefaults.B2BSalesRepRoleSystemName);
    }

    public async Task<bool> IsCustomerInB2BSalesRepRoleAsync(int customerId = 0)
    {
        var customer = await _customerService.GetCustomerByIdAsync(customerId);
        if (customerId == 0 || customer == null)
            return false;
        return await IsCustomerInB2BSalesRepRoleAsync(customer);
    }

    public async Task<bool> IsCurrentCustomerInB2BSalesRepRoleAsync()
    {
        return await IsCustomerInB2BSalesRepRoleAsync(await _workContext.GetCurrentCustomerAsync());
    }

    public async Task<bool> IsCustomerInQuickOrderUserRoleAsync(Customer customer)
    {
        return await _customerService.IsInCustomerRoleAsync(customer, ERPIntegrationCoreDefaults.QuickOrderUserRoleSystemName);
    }

    public async Task<bool> IsCustomerInQuickOrderUserRoleAsync(int customerId = 0)
    {
        var customer = await _customerService.GetCustomerByIdAsync(customerId);
        if (customerId == 0 || customer == null)
            return false;
        return await IsCustomerInQuickOrderUserRoleAsync(customer);
    }

    public async Task<bool> IsCurrentCustomerInQuickOrderUserRoleAsync()
    {
        return await IsCustomerInQuickOrderUserRoleAsync(await _workContext.GetCurrentCustomerAsync());
    }

    public async Task<bool> IsCurrentCustomerInAdministratorRoleAsync()
    {
        return await IsCurrentCustomerInAdministratorRoleAsync(await _workContext.GetCurrentCustomerAsync());
    }

    public async Task<bool> IsCurrentCustomerInAdministratorRoleAsync(Customer customer)
    {
        return await _customerService.IsAdminAsync(customer);
    }

    public async Task<(decimal, decimal)> GetErpProductPriceAndDiscountPercByErpAccountAndProduct(ErpAccount erpAccount, int productId)
    {
        if (_b2BB2CFeaturesSettings.UseProductCombinedPrice)
        {
            var specialPrice = await GetSpecialPriceAsync(erpAccount, productId);
            if (specialPrice.Item1 > 0)
                return specialPrice;

            return await GetGroupPriceAsync(erpAccount, productId);
        }

        if (_b2BB2CFeaturesSettings.UseProductGroupPrice)
        {
            return await GetGroupPriceAsync(erpAccount, productId);
        }

        return await GetSpecialPriceAsync(erpAccount, productId);
    }

    #endregion

    #endregion
}
