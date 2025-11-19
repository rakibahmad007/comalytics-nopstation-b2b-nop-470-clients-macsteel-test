using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Orders;
using Nop.Core.Events;
using Nop.Data;
using Nop.Services.Attributes;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Shipping;
using Nop.Services.Shipping.Date;
using Nop.Services.Stores;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpCustomerFunctionality;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpSpecificationAttributeService;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Services.Overriden;

public partial class OverridenShoppingCartService : ShoppingCartService
{
    #region Fields

    protected readonly IErpCustomerFunctionalityService _erpCustomerFunctionalityService;
    protected readonly ISettingService _settingService;
    private readonly ISpecificationAttributeService _specificationAttributeService;
    private readonly IErpSpecificationAttributeService _erpSpecificationAttributeService;
    private readonly IErpShipToAddressService _erpShipToAddressService;
    private readonly IErpSalesOrgService _erpSalesOrgService;
    private readonly B2BB2CFeaturesSettings _b2BB2CFeaturesSettings;

    #endregion

    #region Ctor

    public OverridenShoppingCartService(
        CatalogSettings catalogSettings,
        IAclService aclService,
        IActionContextAccessor actionContextAccessor,
        IAttributeParser<CheckoutAttribute, CheckoutAttributeValue> checkoutAttributeParser,
        IAttributeService<CheckoutAttribute, CheckoutAttributeValue> checkoutAttributeService,
        ICurrencyService currencyService,
        ICustomerService customerService,
        IDateRangeService dateRangeService,
        IDateTimeHelper dateTimeHelper,
        IEventPublisher eventPublisher,
        IGenericAttributeService genericAttributeService,
        ILocalizationService localizationService,
        IPermissionService permissionService,
        IPriceCalculationService priceCalculationService,
        IPriceFormatter priceFormatter,
        IProductAttributeParser productAttributeParser,
        IProductAttributeService productAttributeService,
        IProductService productService,
        IRepository<ShoppingCartItem> sciRepository,
        IShippingService shippingService,
        IShortTermCacheManager shortTermCacheManager,
        IStaticCacheManager staticCacheManager,
        IStoreContext storeContext,
        IStoreService storeService,
        IStoreMappingService storeMappingService,
        IUrlHelperFactory urlHelperFactory,
        IUrlRecordService urlRecordService,
        IWorkContext workContext,
        OrderSettings orderSettings,
        ShoppingCartSettings shoppingCartSettings,
        IErpCustomerFunctionalityService erpCustomerFunctionalityService,
        ISettingService settingService,
        ISpecificationAttributeService specificationAttributeService,
        IErpSpecificationAttributeService erpSpecificationAttributeService,
        IErpShipToAddressService erpShipToAddressService,
        B2BB2CFeaturesSettings b2BB2CFeaturesSettings,
        IErpSalesOrgService erpSalesOrgService) : base(
            catalogSettings,
            aclService,
            actionContextAccessor,
            checkoutAttributeParser,
            checkoutAttributeService,
            currencyService,
            customerService,
            dateRangeService,
            dateTimeHelper,
            eventPublisher,
            genericAttributeService,
            localizationService,
            permissionService,
            priceCalculationService,
            priceFormatter,
            productAttributeParser,
            productAttributeService,
            productService,
            sciRepository,
            shippingService,
            shortTermCacheManager,
            staticCacheManager,
            storeContext,
            storeService,
            storeMappingService,
            urlHelperFactory,
            urlRecordService,
            workContext,
            orderSettings,
            shoppingCartSettings
        )
    {
        _erpCustomerFunctionalityService = erpCustomerFunctionalityService;
        _settingService = settingService;
        _specificationAttributeService = specificationAttributeService;
        _erpSpecificationAttributeService = erpSpecificationAttributeService;
        _erpShipToAddressService = erpShipToAddressService;
        _b2BB2CFeaturesSettings = b2BB2CFeaturesSettings;
        _erpSalesOrgService = erpSalesOrgService;
    }

    #endregion

    #region Utilities

    private async Task<IList<string>> GetQuantityProductWarningsForB2CUserAsync(
        Product product,
        int quantity,
        int maximumQuantityCanBeAdded
    )
    {
        ArgumentNullException.ThrowIfNull(product);

        var warnings = new List<string>();

        if (maximumQuantityCanBeAdded < quantity)
        {
            if (maximumQuantityCanBeAdded <= 0)
            {
                var productAvailabilityRange =
                    await _dateRangeService.GetProductAvailabilityRangeByIdAsync(
                        product.ProductAvailabilityRangeId
                    );
                var warning =
                    productAvailabilityRange == null
                        ? await _localizationService.GetResourceAsync(
                            "Plugins.Payment.B2BCustomerAccount.B2C.ShoppingCart.OutOfStock"
                        )
                        : string.Format(
                            await _localizationService.GetResourceAsync(
                                "Plugins.Payment.B2BCustomerAccount.B2C.ShoppingCart.AvailabilityRange"
                            ),
                            await _localizationService.GetLocalizedAsync(
                                productAvailabilityRange,
                                range => range.Name
                            )
                        );
                warnings.Add(warning);
            }
            else
                warnings.Add(
                    string.Format(
                        await _localizationService.GetResourceAsync(
                            "Plugins.Payment.B2BCustomerAccount.B2C.ShoppingCart.QuantityExceedsStock"
                        ),
                        maximumQuantityCanBeAdded
                    )
                );
        }

        return warnings;
    }

    #endregion

    #region Methods

    // This method has been overridden for making the products call to bulk fetch
    public override async Task<IList<string>> GetShoppingCartWarningsAsync(IList<ShoppingCartItem> shoppingCart,
        string checkoutAttributesXml, bool validateCheckoutAttributes)
    {
        var warnings = new List<string>();

        if (shoppingCart.Count > _shoppingCartSettings.MaximumShoppingCartItems)
            warnings.Add(string.Format(await _localizationService.GetResourceAsync("ShoppingCart.MaximumShoppingCartItems"), _shoppingCartSettings.MaximumShoppingCartItems));

        var hasStandardProducts = false;
        var hasRecurringProducts = false;

        var productIds = shoppingCart.Select(x => x.ProductId).Distinct().ToArray();
        var products = await _productService.GetProductsByIdsAsync(productIds);

        foreach (var sci in shoppingCart)
        {
            var product = products.FirstOrDefault(x => x.Id == sci.ProductId);
            if (product == null)
            {
                warnings.Add(string.Format(await _localizationService.GetResourceAsync("ShoppingCart.CannotLoadProduct"), sci.ProductId));
                return warnings;
            }

            if (product.IsRecurring)
                hasRecurringProducts = true;
            else
                hasStandardProducts = true;
        }

        //don't mix standard and recurring products
        if (hasStandardProducts && hasRecurringProducts)
            warnings.Add(await _localizationService.GetResourceAsync("ShoppingCart.CannotMixStandardAndAutoshipProducts"));

        //recurring cart validation
        if (hasRecurringProducts)
        {
            var cyclesError = (await GetRecurringCycleInfoAsync(shoppingCart)).error;
            if (!string.IsNullOrEmpty(cyclesError))
            {
                warnings.Add(cyclesError);
                return warnings;
            }
        }

        //validate checkout attributes
        if (!validateCheckoutAttributes)
            return warnings;

        //selected attributes
        var attributes1 = await _checkoutAttributeParser.ParseAttributesAsync(checkoutAttributesXml);

        //existing checkout attributes
        var excludeShippableAttributes = !await ShoppingCartRequiresShippingAsync(shoppingCart);
        var store = await _storeContext.GetCurrentStoreAsync();
        var attributes2 = await _checkoutAttributeService.GetAllAttributesAsync(_staticCacheManager, _storeMappingService, store.Id, excludeShippableAttributes);

        //validate conditional attributes only (if specified)
        attributes2 = await attributes2.WhereAwait(async x =>
        {
            var conditionMet = await _checkoutAttributeParser.IsConditionMetAsync(x.ConditionAttributeXml, checkoutAttributesXml);
            return !conditionMet.HasValue || conditionMet.Value;
        }).ToListAsync();

        foreach (var a2 in attributes2)
        {
            if (!a2.IsRequired)
                continue;

            var found = false;
            //selected checkout attributes
            foreach (var a1 in attributes1)
            {
                if (a1.Id != a2.Id)
                    continue;

                var attributeValuesStr = _checkoutAttributeParser.ParseValues(checkoutAttributesXml, a1.Id);
                foreach (var str1 in attributeValuesStr)
                    if (!string.IsNullOrEmpty(str1.Trim()))
                    {
                        found = true;
                        break;
                    }
            }

            if (found)
                continue;

            //if not found
            warnings.Add(!string.IsNullOrEmpty(await _localizationService.GetLocalizedAsync(a2, a => a.TextPrompt))
                ? await _localizationService.GetLocalizedAsync(a2, a => a.TextPrompt)
                : string.Format(await _localizationService.GetResourceAsync("ShoppingCart.SelectAttribute"),
                    await _localizationService.GetLocalizedAsync(a2, a => a.Name)));
        }

        //now validation rules

        //minimum length
        foreach (var ca in attributes2)
        {
            string enteredText;
            int enteredTextLength;

            if (ca.ValidationMinLength.HasValue)
            {
                if (ca.AttributeControlType == AttributeControlType.TextBox ||
                    ca.AttributeControlType == AttributeControlType.MultilineTextbox)
                {
                    enteredText = _checkoutAttributeParser.ParseValues(checkoutAttributesXml, ca.Id).FirstOrDefault();
                    enteredTextLength = string.IsNullOrEmpty(enteredText) ? 0 : enteredText.Length;

                    if (ca.ValidationMinLength.Value > enteredTextLength)
                    {
                        warnings.Add(string.Format(await _localizationService.GetResourceAsync("ShoppingCart.TextboxMinimumLength"), await _localizationService.GetLocalizedAsync(ca, a => a.Name), ca.ValidationMinLength.Value));
                    }
                }
            }

            //maximum length
            if (!ca.ValidationMaxLength.HasValue)
                continue;

            if (ca.AttributeControlType != AttributeControlType.TextBox && ca.AttributeControlType != AttributeControlType.MultilineTextbox)
                continue;

            enteredText = _checkoutAttributeParser.ParseValues(checkoutAttributesXml, ca.Id).FirstOrDefault();
            enteredTextLength = string.IsNullOrEmpty(enteredText) ? 0 : enteredText.Length;

            if (ca.ValidationMaxLength.Value < enteredTextLength)
            {
                warnings.Add(string.Format(await _localizationService.GetResourceAsync("ShoppingCart.TextboxMaximumLength"), await _localizationService.GetLocalizedAsync(ca, a => a.Name), ca.ValidationMaxLength.Value));
            }
        }

        return warnings;
    }

    /// <summary>
    /// Validates a product for standard properties
    /// </summary>
    /// <param name="customer">Customer</param>
    /// <param name="shoppingCartType">Shopping cart type</param>
    /// <param name="product">Product</param>
    /// <param name="attributesXml">Attributes in XML format</param>
    /// <param name="customerEnteredPrice">Customer entered price</param>
    /// <param name="quantity">Quantity</param>
    /// <param name="shoppingCartItemId">Shopping cart identifier; pass 0 if it's a new item</param>
    /// <param name="storeId">Store identifier</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the warnings
    /// </returns>
    protected override async Task<IList<string>> GetStandardWarningsAsync(
        Customer customer,
        ShoppingCartType shoppingCartType,
        Product product,
        string attributesXml,
        decimal customerEnteredPrice,
        int quantity,
        int shoppingCartItemId,
        int storeId
    )
    {
        ArgumentNullException.ThrowIfNull(customer);

        ArgumentNullException.ThrowIfNull(product);

        var warnings = new List<string>();

        //deleted
        if (product.Deleted)
        {
            warnings.Add(
                await _localizationService.GetResourceAsync("ShoppingCart.ProductDeleted")
            );
            return warnings;
        }

        //published
        if (!product.Published)
        {
            warnings.Add(
                await _localizationService.GetResourceAsync("ShoppingCart.ProductUnpublished")
            );
        }

        //we can add only simple products
        if (product.ProductType != ProductType.SimpleProduct)
        {
            warnings.Add("This is not simple product");
        }

        //ACL
        if (!await _aclService.AuthorizeAsync(product, customer))
        {
            warnings.Add(
                await _localizationService.GetResourceAsync("ShoppingCart.ProductUnpublished")
            );
        }

        //Store mapping
        if (!await _storeMappingService.AuthorizeAsync(product, storeId))
        {
            warnings.Add(
                await _localizationService.GetResourceAsync("ShoppingCart.ProductUnpublished")
            );
        }

        //disabled "add to cart" button
        if (shoppingCartType == ShoppingCartType.ShoppingCart && product.DisableBuyButton)
        {
            warnings.Add(
                await _localizationService.GetResourceAsync("ShoppingCart.BuyingDisabled")
            );
        }

        //disabled "add to wishlist" button
        if (shoppingCartType == ShoppingCartType.Wishlist && product.DisableWishlistButton)
        {
            warnings.Add(
                await _localizationService.GetResourceAsync("ShoppingCart.WishlistDisabled")
            );
        }

        //call for price
        if (
            shoppingCartType == ShoppingCartType.ShoppingCart
            && product.CallForPrice
            &&
            //also check whether the current user is impersonated
            (
                !_orderSettings.AllowAdminsToBuyCallForPriceProducts
                || _workContext.OriginalCustomerIfImpersonated == null
            )
        )
        {
            warnings.Add(await _localizationService.GetResourceAsync("Products.CallForPrice"));
        }

        //customer entered price
        if (product.CustomerEntersPrice)
        {
            if (
                customerEnteredPrice < product.MinimumCustomerEnteredPrice
                || customerEnteredPrice > product.MaximumCustomerEnteredPrice
            )
            {
                var currentCurrency = await _workContext.GetWorkingCurrencyAsync();
                var minimumCustomerEnteredPrice =
                    await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(
                        product.MinimumCustomerEnteredPrice,
                        currentCurrency
                    );
                var maximumCustomerEnteredPrice =
                    await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(
                        product.MaximumCustomerEnteredPrice,
                        currentCurrency
                    );
                warnings.Add(
                    string.Format(
                        await _localizationService.GetResourceAsync(
                            "ShoppingCart.CustomerEnteredPrice.RangeError"
                        ),
                        await _priceFormatter.FormatPriceAsync(
                            minimumCustomerEnteredPrice,
                            false,
                            false
                        ),
                        await _priceFormatter.FormatPriceAsync(
                            maximumCustomerEnteredPrice,
                            false,
                            false
                        )
                    )
                );
            }
        }

        //quantity validation
        var hasQtyWarnings = false;
        if (quantity < product.OrderMinimumQuantity)
        {
            warnings.Add(
                string.Format(
                    await _localizationService.GetResourceAsync("ShoppingCart.MinimumQuantity"),
                    product.OrderMinimumQuantity
                )
            );
            hasQtyWarnings = true;
        }

        if (quantity > product.OrderMaximumQuantity)
        {
            warnings.Add(
                string.Format(
                    await _localizationService.GetResourceAsync("ShoppingCart.MaximumQuantity"),
                    product.OrderMaximumQuantity
                )
            );
            hasQtyWarnings = true;
        }

        var allowedQuantities = _productService.ParseAllowedQuantities(product);
        if (allowedQuantities.Length > 0 && !allowedQuantities.Contains(quantity))
        {
            warnings.Add(
                string.Format(
                    await _localizationService.GetResourceAsync("ShoppingCart.AllowedQuantities"),
                    string.Join(", ", allowedQuantities)
                )
            );
        }

        var validateOutOfStock =
            shoppingCartType == ShoppingCartType.ShoppingCart
            || !_shoppingCartSettings.AllowOutOfStockItemsToBeAddedToWishlist;

        #region Custom for B2B

        // Sales Rep Role should not be allowed to purchase
        //if(customer.CustomerRoles.Any(x => x.SystemName.Equals(B2BCustomerAccountDefaults.B2BSalesRepRoleSystemName)))
        if (await _erpCustomerFunctionalityService.IsCustomerInB2BSalesRepRoleAsync(customer))
        {
            warnings.Add(
                await _localizationService.GetResourceAsync(
                    "Plugins.Payments.B2BCustomerAccount.B2BSalesReps.IsNotAllowedToPurchase"
                )
            );
        }

        var b2BAccount = await _erpCustomerFunctionalityService.GetActiveErpAccountByCustomerAsync(
            customer
        );
        if (b2BAccount != null)
        {
            //load settings for current Store
            var b2BB2CFeaturesSettings =
                await _settingService.LoadSettingAsync<B2BB2CFeaturesSettings>(
                    (await _storeContext.GetCurrentStoreAsync()).Id
                );

            var productSpecificationAttributeIds = (
                await _specificationAttributeService.GetProductSpecificationAttributesAsync(
                    product.Id
                )
            ).Select(x => x.SpecificationAttributeOptionId);
            var specIds =
                await _erpSpecificationAttributeService.GetSpecificationAttributeOptionIdsByNames(
                    b2BB2CFeaturesSettings.PreFilterFacetSpecificationAttributeId,
                    b2BAccount.PreFilterFacets?.Trim(),
                    b2BAccount.Id
                );

            // Special Include
            var specialIncludeSpecIds =
                await _erpSpecificationAttributeService.GetSpecificationAttributeOptionIdsByNames(
                    b2BB2CFeaturesSettings.PreFilterFacetSpecificationAttributeId,
                    b2BAccount.SpecialIncludes?.Trim(),
                    b2BAccount.Id
                );
            if (specialIncludeSpecIds != null && specialIncludeSpecIds.Any())
            {
                foreach (var includeSpecId in specialIncludeSpecIds)
                {
                    if (specIds == null)
                        specIds = new List<int>();
                    if (!specIds.Contains(includeSpecId))
                        specIds.Add(includeSpecId);
                }
            }
            var commonSpecIds = specIds.Intersect(productSpecificationAttributeIds);

            if (!commonSpecIds.Any())
            {
                warnings.Add(
                    string.Format(
                        await _localizationService.GetResourceAsync(
                            "Plugins.Payments.B2BCustomerAccount.ProductIsNotAllowed"
                        )
                    )
                );
            }

            // Special Exclude
            var specialExcludeSpecIds =
                await _erpSpecificationAttributeService.GetSpecificationAttributeOptionIdsForExcludeByNames(
                    b2BB2CFeaturesSettings.PreFilterFacetSpecificationAttributeId,
                    b2BAccount.SpecialExcludes?.Trim()
                );
            if (specialExcludeSpecIds != null && specialExcludeSpecIds.Any())
            {
                if (
                    productSpecificationAttributeIds != null
                    && productSpecificationAttributeIds.Any()
                )
                {
                    var commonSpecificationIds = specialExcludeSpecIds.Intersect(
                        productSpecificationAttributeIds
                    );
                    if (commonSpecificationIds.Any())
                    {
                        warnings.Add(
                            string.Format(
                                await _localizationService.GetResourceAsync(
                                    "Plugins.Payments.B2BCustomerAccount.ProductIsNotAllowed"
                                )
                            )
                        );
                    }
                }
            }
        }

        if (validateOutOfStock && !hasQtyWarnings)
        {
            var erpNopUser = await _erpCustomerFunctionalityService.GetActiveErpNopUserByCustomerAsync(customer);
            if (erpNopUser != null && erpNopUser.ErpUserType == ErpUserType.B2BUser)
            {
                var isB2BBackOrderingAllowed = await _erpCustomerFunctionalityService.CheckAllowBackOrderingByAccountAsync(b2BAccount);
                var isSalesOrgSpecialProduct =
                    await _erpSalesOrgService.CheckAnyB2BSalesOrgProductsExistBySalesOrgIdAndProductsIdAsync(b2BAccount.ErpSalesOrgId, product.Id) ||
                    await _erpCustomerFunctionalityService.IsTheProductFromSpecialCategoryAsync(product);

                if (!isB2BBackOrderingAllowed || isSalesOrgSpecialProduct)
                {
                    switch (product.ManageInventoryMethod)
                    {
                        case ManageInventoryMethod.DontManageStock:
                            //do nothing
                            break;
                        case ManageInventoryMethod.ManageStock:

                            var maximumQuantityCanBeAdded = await _productService.GetTotalStockQuantityAsync(product);

                            warnings.AddRange(await GetQuantityProductWarningsAsync(product, quantity, maximumQuantityCanBeAdded));

                            if (warnings.Any())
                                return warnings;

                            //validate product quantity with non combinable product attributes
                            var productAttributeMappings =
                                await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(
                                    product.Id
                                );
                            if (productAttributeMappings?.Any() == true)
                            {
                                var onlyCombinableAttributes = productAttributeMappings.All(
                                    mapping => !mapping.IsNonCombinable()
                                );
                                if (!onlyCombinableAttributes)
                                {
                                    var cart = await GetShoppingCartAsync(
                                        customer,
                                        shoppingCartType,
                                        storeId
                                    );
                                    var totalAddedQuantity = cart.Where(item =>
                                            item.ProductId == product.Id
                                            && item.Id != shoppingCartItemId
                                        )
                                        .Sum(product => product.Quantity);

                                    totalAddedQuantity += quantity;

                                    //counting a product into bundles
                                    foreach (
                                        var bundle in cart.Where(x =>
                                            x.Id != shoppingCartItemId
                                            && !string.IsNullOrEmpty(x.AttributesXml)
                                        )
                                    )
                                    {
                                        var attributeValues =
                                            await _productAttributeParser.ParseProductAttributeValuesAsync(
                                                bundle.AttributesXml
                                            );
                                        foreach (var attributeValue in attributeValues)
                                        {
                                            if (
                                                attributeValue.AttributeValueType
                                                    == AttributeValueType.AssociatedToProduct
                                                && attributeValue.AssociatedProductId == product.Id
                                            )
                                                totalAddedQuantity +=
                                                    bundle.Quantity * attributeValue.Quantity;
                                        }
                                    }

                                    warnings.AddRange(
                                        await GetQuantityProductWarningsAsync(
                                            product,
                                            totalAddedQuantity,
                                            maximumQuantityCanBeAdded
                                        )
                                    );
                                }
                            }

                            if (warnings.Any())
                                return warnings;

                            //validate product quantity and product quantity into bundles
                            if (string.IsNullOrEmpty(attributesXml))
                            {
                                var cart = await GetShoppingCartAsync(
                                    customer,
                                    shoppingCartType,
                                    storeId
                                );
                                var totalQuantityInCart = cart.Where(item =>
                                        item.ProductId == product.Id
                                        && item.Id != shoppingCartItemId
                                        && string.IsNullOrEmpty(item.AttributesXml)
                                    )
                                    .Sum(product => product.Quantity);

                                totalQuantityInCart += quantity;

                                foreach (
                                    var bundle in cart.Where(x =>
                                        x.Id != shoppingCartItemId
                                        && !string.IsNullOrEmpty(x.AttributesXml)
                                    )
                                )
                                {
                                    var attributeValues =
                                        await _productAttributeParser.ParseProductAttributeValuesAsync(
                                            bundle.AttributesXml
                                        );
                                    foreach (var attributeValue in attributeValues)
                                    {
                                        if (
                                            attributeValue.AttributeValueType
                                                == AttributeValueType.AssociatedToProduct
                                            && attributeValue.AssociatedProductId == product.Id
                                        )
                                            totalQuantityInCart +=
                                                bundle.Quantity * attributeValue.Quantity;
                                    }
                                }

                                warnings.AddRange(
                                    await GetQuantityProductWarningsAsync(
                                        product,
                                        totalQuantityInCart,
                                        maximumQuantityCanBeAdded
                                    )
                                );
                            }

                            break;
                        case ManageInventoryMethod.ManageStockByAttributes:
                            var combination =
                                await _productAttributeParser.FindProductAttributeCombinationAsync(
                                    product,
                                    attributesXml
                                );
                            if (combination != null)
                            {
                                //combination exists
                                //let's check stock level
                                if (!combination.AllowOutOfStockOrders)
                                    warnings.AddRange(
                                        await GetQuantityProductWarningsAsync(
                                            product,
                                            quantity,
                                            combination.StockQuantity
                                        )
                                    );
                            }
                            else
                            {
                                //combination doesn't exist
                                if (product.AllowAddingOnlyExistingAttributeCombinations)
                                {
                                    //maybe, is it better  to display something like "No such product/combination" message?
                                    var productAvailabilityRange =
                                        await _dateRangeService.GetProductAvailabilityRangeByIdAsync(
                                            product.ProductAvailabilityRangeId
                                        );
                                    var warning =
                                        productAvailabilityRange == null
                                            ? await _localizationService.GetResourceAsync(
                                                "ShoppingCart.OutOfStock"
                                            )
                                            : string.Format(
                                                await _localizationService.GetResourceAsync(
                                                    "ShoppingCart.AvailabilityRange"
                                                ),
                                                await _localizationService.GetLocalizedAsync(
                                                    productAvailabilityRange,
                                                    range => range.Name
                                                )
                                            );
                                    warnings.Add(warning);
                                }
                            }

                            break;
                        default:
                            break;
                    }
                }
            }
            else
            {
                switch (product.ManageInventoryMethod)
                {
                    case ManageInventoryMethod.DontManageStock:
                        //do nothing
                        break;
                    case ManageInventoryMethod.ManageStock: // just resource strings will be changed
                        if (product.BackorderMode == BackorderMode.NoBackorders)
                        {
                            var maximumQuantityCanBeAdded =
                                await _productService.GetTotalStockQuantityAsync(product);

                            warnings.AddRange(
                                await GetQuantityProductWarningsForB2CUserAsync(
                                    product,
                                    quantity,
                                    maximumQuantityCanBeAdded
                                )
                            );

                            if (warnings.Any())
                                return warnings;

                            //validate product quantity with non combinable product attributes
                            var productAttributeMappings =
                                await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(
                                    product.Id
                                );
                            if (productAttributeMappings?.Any() == true)
                            {
                                var onlyCombinableAttributes = productAttributeMappings.All(
                                    mapping => !mapping.IsNonCombinable()
                                );
                                if (!onlyCombinableAttributes)
                                {
                                    var cart = await GetShoppingCartAsync(
                                        customer,
                                        shoppingCartType,
                                        storeId
                                    );
                                    var totalAddedQuantity = cart.Where(item =>
                                            item.ProductId == product.Id
                                            && item.Id != shoppingCartItemId
                                        )
                                        .Sum(product => product.Quantity);

                                    totalAddedQuantity += quantity;

                                    //counting a product into bundles
                                    foreach (
                                        var bundle in cart.Where(x =>
                                            x.Id != shoppingCartItemId
                                            && !string.IsNullOrEmpty(x.AttributesXml)
                                        )
                                    )
                                    {
                                        var attributeValues =
                                            await _productAttributeParser.ParseProductAttributeValuesAsync(
                                                bundle.AttributesXml
                                            );
                                        foreach (var attributeValue in attributeValues)
                                        {
                                            if (
                                                attributeValue.AttributeValueType
                                                    == AttributeValueType.AssociatedToProduct
                                                && attributeValue.AssociatedProductId == product.Id
                                            )
                                                totalAddedQuantity +=
                                                    bundle.Quantity * attributeValue.Quantity;
                                        }
                                    }

                                    warnings.AddRange(
                                        await GetQuantityProductWarningsForB2CUserAsync(
                                            product,
                                            totalAddedQuantity,
                                            maximumQuantityCanBeAdded
                                        )
                                    );
                                }
                            }

                            if (warnings.Any())
                                return warnings;

                            //validate product quantity and product quantity into bundles
                            if (string.IsNullOrEmpty(attributesXml))
                            {
                                var cart = await GetShoppingCartAsync(
                                    customer,
                                    shoppingCartType,
                                    storeId
                                );
                                var totalQuantityInCart = cart.Where(item =>
                                        item.ProductId == product.Id
                                        && item.Id != shoppingCartItemId
                                        && string.IsNullOrEmpty(item.AttributesXml)
                                    )
                                    .Sum(product => product.Quantity);

                                totalQuantityInCart += quantity;

                                foreach (
                                    var bundle in cart.Where(x =>
                                        x.Id != shoppingCartItemId
                                        && !string.IsNullOrEmpty(x.AttributesXml)
                                    )
                                )
                                {
                                    var attributeValues =
                                        await _productAttributeParser.ParseProductAttributeValuesAsync(
                                            bundle.AttributesXml
                                        );
                                    foreach (var attributeValue in attributeValues)
                                    {
                                        if (
                                            attributeValue.AttributeValueType
                                                == AttributeValueType.AssociatedToProduct
                                            && attributeValue.AssociatedProductId == product.Id
                                        )
                                            totalQuantityInCart +=
                                                bundle.Quantity * attributeValue.Quantity;
                                    }
                                }

                                warnings.AddRange(
                                    await GetQuantityProductWarningsForB2CUserAsync(
                                        product,
                                        totalQuantityInCart,
                                        maximumQuantityCanBeAdded
                                    )
                                );
                            }
                        }

                        break;
                    case ManageInventoryMethod.ManageStockByAttributes: //// just resource strings will be changed
                        var combination =
                            await _productAttributeParser.FindProductAttributeCombinationAsync(
                                product,
                                attributesXml
                            );
                        if (combination != null)
                        {
                            //combination exists
                            //let's check stock level
                            if (!combination.AllowOutOfStockOrders)
                                warnings.AddRange(
                                    await GetQuantityProductWarningsForB2CUserAsync(
                                        product,
                                        quantity,
                                        combination.StockQuantity
                                    )
                                );
                        }
                        else
                        {
                            //combination doesn't exist
                            if (product.AllowAddingOnlyExistingAttributeCombinations)
                            {
                                //maybe, is it better  to display something like "No such product/combination" message?
                                var productAvailabilityRange =
                                    await _dateRangeService.GetProductAvailabilityRangeByIdAsync(
                                        product.ProductAvailabilityRangeId
                                    );
                                var warning =
                                    productAvailabilityRange == null
                                        ? await _localizationService.GetResourceAsync(
                                            "Plugins.Payment.B2BCustomerAccount.B2C.ShoppingCart.OutOfStock"
                                        )
                                        : string.Format(
                                            await _localizationService.GetResourceAsync(
                                                "Plugins.Payment.B2BCustomerAccount.B2C.ShoppingCart.AvailabilityRange"
                                            ),
                                            await _localizationService.GetLocalizedAsync(
                                                productAvailabilityRange,
                                                range => range.Name
                                            )
                                        );
                                warnings.Add(warning);
                            }
                        }

                        break;
                    default:
                        break;
                }
            }
        }

        var nopUser = await _erpCustomerFunctionalityService.GetActiveErpNopUserByCustomerAsync(
            customer
        );
        if (shoppingCartType == ShoppingCartType.ShoppingCart && nopUser != null)
        {
            var b2CShiptoAddress = await _erpShipToAddressService.GetErpShipToAddressByIdAsync(
                nopUser.ErpShipToAddressId
            );
            if (b2CShiptoAddress == null)
                warnings.Add(
                    await _localizationService.GetResourceAsync(
                        "Plugins.Payment.B2BCustomerAccount.ShoppingCart.InvalidShipToAddress"
                    )
                );
        }

        #endregion

        //availability dates
        var availableStartDateError = false;
        if (product.AvailableStartDateTimeUtc.HasValue)
        {
            var availableStartDateTime = DateTime.SpecifyKind(
                product.AvailableStartDateTimeUtc.Value,
                DateTimeKind.Utc
            );
            if (availableStartDateTime.CompareTo(DateTime.UtcNow) > 0)
            {
                warnings.Add(
                    await _localizationService.GetResourceAsync("ShoppingCart.NotAvailable")
                );
                availableStartDateError = true;
            }
        }

        if (!product.AvailableEndDateTimeUtc.HasValue || availableStartDateError)
            return warnings;

        var availableEndDateTime = DateTime.SpecifyKind(
            product.AvailableEndDateTimeUtc.Value,
            DateTimeKind.Utc
        );
        if (availableEndDateTime.CompareTo(DateTime.UtcNow) < 0)
        {
            warnings.Add(await _localizationService.GetResourceAsync("ShoppingCart.NotAvailable"));
        }

        return warnings;
    }

    public override async Task<(decimal unitPrice, decimal discountAmount, List<Discount> appliedDiscounts)> GetUnitPriceAsync(ShoppingCartItem shoppingCartItem,
        bool includeDiscounts)
    {
        ArgumentNullException.ThrowIfNull(shoppingCartItem);

        var customer = await _customerService.GetCustomerByIdAsync(shoppingCartItem.CustomerId);
        var product = await _productService.GetProductByIdAsync(shoppingCartItem.ProductId);
        var store = await _storeService.GetStoreByIdAsync(shoppingCartItem.StoreId);

        #region B2B Custom

        var (unitPrice, discountAmount, appliedDiscounts) = await GetUnitPriceAsync(product,
            customer,
            store,
            shoppingCartItem.ShoppingCartType,
            shoppingCartItem.Quantity,
            shoppingCartItem.AttributesXml,
            shoppingCartItem.CustomerEnteredPrice,
            shoppingCartItem.RentalStartDateUtc,
            shoppingCartItem.RentalEndDateUtc,
            includeDiscounts);

        var erpAccount = await _erpCustomerFunctionalityService.GetActiveErpAccountByCustomerAsync(customer);

        if (erpAccount != null && unitPrice != _b2BB2CFeaturesSettings.ProductQuotePrice)
        {
            var currentItemB2BDiscountAmount = decimal.Zero;
            (var removeL, var discountPerc) = await _erpCustomerFunctionalityService.GetErpProductPriceAndDiscountPercByErpAccountAndProduct(erpAccount, product.Id);

            if (unitPrice > 0 && discountPerc > 0)
            {
                // we applying b2b per item discount on actual price, 
                // thus we are allowing to add regular nop discount applied to product
                var priceWithoutDiscount = unitPrice + discountAmount;
                currentItemB2BDiscountAmount = (priceWithoutDiscount * discountPerc) / 100;
                discountAmount += currentItemB2BDiscountAmount;

                unitPrice = unitPrice - currentItemB2BDiscountAmount;
            }
        }

        return (unitPrice, discountAmount, appliedDiscounts);

        #endregion
    }

    /// <summary>
    /// Gets the shopping cart item sub total
    /// </summary>
    /// <param name="shoppingCartItem">The shopping cart item</param>
    /// <param name="includeDiscounts">A value indicating whether include discounts or not for price computation</param>
    /// <returns>Shopping cart item sub total. Applied discount amount. Applied discounts. Maximum discounted qty. Return not nullable value if discount cannot be applied to ALL items</returns>
    public override async Task<(decimal subTotal, decimal discountAmount, List<Discount> appliedDiscounts, int? maximumDiscountQty)> GetSubTotalAsync(ShoppingCartItem shoppingCartItem,
        bool includeDiscounts)
    {
        ArgumentNullException.ThrowIfNull(shoppingCartItem);

        decimal subTotal;
        int? maximumDiscountQty = null;

        //unit price
        var (unitPrice, discountAmount, appliedDiscounts) = await GetUnitPriceAsync(shoppingCartItem, includeDiscounts);

        var b2BAccount = await _erpCustomerFunctionalityService.GetActiveErpAccountByCustomerAsync(await _customerService.GetCustomerByIdAsync(shoppingCartItem.CustomerId));

        //discount
        if (appliedDiscounts.Any() || (b2BAccount != null && discountAmount > 0))
        {
            //we can properly use "MaximumDiscountedQuantity" property only for one discount (not cumulative ones)
            Discount oneAndOnlyDiscount = null;
            if (appliedDiscounts.Count == 1)
                oneAndOnlyDiscount = appliedDiscounts.First();

            if ((oneAndOnlyDiscount?.MaximumDiscountedQuantity.HasValue ?? false) &&
                shoppingCartItem.Quantity > oneAndOnlyDiscount.MaximumDiscountedQuantity.Value)
            {
                maximumDiscountQty = oneAndOnlyDiscount.MaximumDiscountedQuantity.Value;
                //we cannot apply discount for all shopping cart items
                var discountedQuantity = oneAndOnlyDiscount.MaximumDiscountedQuantity.Value;
                var discountedSubTotal = unitPrice * discountedQuantity;
                discountAmount *= discountedQuantity;

                var notDiscountedQuantity = shoppingCartItem.Quantity - discountedQuantity;
                var notDiscountedUnitPrice = (await GetUnitPriceAsync(shoppingCartItem, false)).unitPrice;
                var notDiscountedSubTotal = notDiscountedUnitPrice * notDiscountedQuantity;

                subTotal = discountedSubTotal + notDiscountedSubTotal;
            }
            else
            {
                //discount is applied to all items (quantity)
                //calculate discount amount for all items
                discountAmount *= shoppingCartItem.Quantity;

                subTotal = unitPrice * shoppingCartItem.Quantity;
            }
        }
        else
        {
            subTotal = unitPrice * shoppingCartItem.Quantity;
        }

        return (subTotal, discountAmount, appliedDiscounts, maximumDiscountQty);
    }

    #endregion
}
