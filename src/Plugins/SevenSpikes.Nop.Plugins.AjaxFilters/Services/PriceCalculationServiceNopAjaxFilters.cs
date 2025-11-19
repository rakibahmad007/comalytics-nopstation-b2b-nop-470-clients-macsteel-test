using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.Vendors;
using Nop.Data;
using Nop.Services.Customers;
using Nop.Services.Directory;
using SevenSpikes.Nop.Plugins.AjaxFilters.Models.PriceRangeFilterSlider;
using SevenSpikes.Nop.Services.Catalog;
using SevenSpikes.Nop.Services.Helpers;

namespace SevenSpikes.Nop.Plugins.AjaxFilters.Services;

public class PriceCalculationServiceNopAjaxFilters : IPriceCalculationServiceNopAjaxFilters
{
	private readonly IWorkContext _workContext;

	private readonly CatalogSettings _catalogSettings;

	private readonly TaxSettings _taxSettings;

	private readonly ICategoryService7Spikes _categoryService7Spikes;

	private readonly IAclHelper _aclHelper;

	private readonly IRepository<ProductCategory> _productCategoryRepository;

	private readonly IRepository<ProductManufacturer> _productManufacturerRepository;

	private readonly IRepository<Vendor> _vendorRepository;

	private readonly ITaxServiceNopAjaxFilters _taxServiceNopAjaxFilters;

	private readonly ICurrencyService _currencyService;

	private readonly IProductServiceNopAjaxFilters _productServiceNopAjaxFilters;

	private readonly IStoreHelper _storeHelper;

	private readonly ICustomerService _customerService;

	private readonly ICustomAclHelper _customAclHelper;

    public PriceCalculationServiceNopAjaxFilters(IWorkContext workContext, CatalogSettings catalogSettings, TaxSettings taxSettings, ICategoryService7Spikes categoryService7Spikes, IAclHelper aclHelper, IRepository<ProductCategory> productCategoryRepository, IRepository<ProductManufacturer> productManufacturerRepository, IRepository<Vendor> vendorRepository, ITaxServiceNopAjaxFilters taxServiceNopAjaxFilters, ICurrencyService currencyService, IProductServiceNopAjaxFilters productServiceNopAjaxFilters, IStoreHelper storeHelper, ICustomerService customerService, ICustomAclHelper customAclHelper)
    {
        _workContext = workContext;
        _catalogSettings = catalogSettings;
        _taxSettings = taxSettings;
        _categoryService7Spikes = categoryService7Spikes;
        _aclHelper = aclHelper;
        _vendorRepository = vendorRepository;
        _productCategoryRepository = productCategoryRepository;
        _productManufacturerRepository = productManufacturerRepository;
        _taxServiceNopAjaxFilters = taxServiceNopAjaxFilters;
        _currencyService = currencyService;
        _productServiceNopAjaxFilters = productServiceNopAjaxFilters;
        _storeHelper = storeHelper;
        _customerService = customerService;
        _customAclHelper = customAclHelper;
    }

    public async Task<PriceRangeFilterDto> GetPriceRangeFilterDtoAsync(int categoryId, int manufacturerId, int vendorId)
	{
		PriceRangeFilterDto model = new PriceRangeFilterDto();
		if (categoryId > 0)
		{
			SetDiscountAmountPercentageForCategory(categoryId, model);
		}
		TaxDisplayType val = await _workContext.GetTaxDisplayTypeAsync();
		if ((int)val != 0)
		{
			if ((int)val == 10)
			{
				model.TaxDisplayTypeIncludingTax = false;
			}
		}
		else
		{
			model.TaxDisplayTypeIncludingTax = true;
		}
		model.TaxPriceIncludeTax = _taxSettings.PricesIncludeTax;
		await SetMinMaxPricesAsync(categoryId, manufacturerId, vendorId, model);
		PriceRangeFilterDto priceRangeFilterDto = model;
		ICurrencyService currencyService = _currencyService;
		decimal minPrice = model.MinPrice;
		priceRangeFilterDto.MinPrice = await currencyService.ConvertFromPrimaryStoreCurrencyAsync(minPrice, await _workContext.GetWorkingCurrencyAsync());
		priceRangeFilterDto = model;
		currencyService = _currencyService;
		minPrice = model.MaxPrice;
		priceRangeFilterDto.MaxPrice = await currencyService.ConvertFromPrimaryStoreCurrencyAsync(minPrice, await _workContext.GetWorkingCurrencyAsync());
		return model;
	}

	public async Task<decimal> CalculateBasePriceAsync(decimal price, PriceRangeFilterDto priceRangeModel, bool isFromPrice)
	{
		price = GetPriceWithoutDiscount(price, priceRangeModel);
		price = GetPriceWithoutTax(price, priceRangeModel);
		ICurrencyService currencyService = _currencyService;
		decimal num = price;
		price = await currencyService.ConvertToPrimaryStoreCurrencyAsync(num, await _workContext.GetWorkingCurrencyAsync());
		price = Math.Round(price, 2);
		return price;
	}

	private static decimal GetPriceWithoutTax(decimal price, PriceRangeFilterDto priceRangeModel)
	{
		decimal num = price;
		if (priceRangeModel.TaxPriceIncludeTax)
		{
			if (!priceRangeModel.TaxDisplayTypeIncludingTax)
			{
				num = CalculatePriceWithoutTax(price, priceRangeModel.TaxRatePercentage, increase: false);
			}
		}
		else if (priceRangeModel.TaxDisplayTypeIncludingTax)
		{
			num = CalculatePriceWithoutTax(num, priceRangeModel.TaxRatePercentage, increase: true);
		}
		return num;
	}

	private static decimal CalculatePriceWithoutTax(decimal price, decimal percent, bool increase)
	{
		decimal num = default(decimal);
		if (percent == 0m)
		{
			return price;
		}
		if (increase)
		{
			return price / (1m + percent / 100m);
		}
		return price * (100m + percent) / (100m + percent - percent);
	}

	private static decimal GetPriceWithoutDiscount(decimal price, PriceRangeFilterDto priceRangeModel)
	{
		if (priceRangeModel.MaxDiscountAmount == 0m && priceRangeModel.MaxDiscountPercentage == 0m)
		{
			return price;
		}
		decimal result = price;
		decimal num = (decimal)((float)price * 100f / (100f - (float)priceRangeModel.MaxDiscountPercentage));
		decimal num2 = price + priceRangeModel.MaxDiscountAmount;
		if (num > 0m && num > num2)
		{
			result = num;
		}
		else if (num2 > 0m)
		{
			result = num2;
		}
		return result;
	}

	private async Task SetMinMaxPricesAsync(int categoryId, int manufacturerId, int vendorId, PriceRangeFilterDto priceRangeModel)
	{
		IQueryable<Product> source = await PrepareMinMaxPriceProductVariantQueryAsync(categoryId, manufacturerId, vendorId);
		if (source.Any())
		{
			decimal? minPrice = source.Min((Expression<Func<Product, decimal?>>)((Product pv) => pv.Price));
			decimal? maxPrice = source.Max((Expression<Func<Product, decimal?>>)((Product pv) => pv.Price));
			Product productVariant = await AsyncIQueryableExtensions.FirstOrDefaultAsync<Product>(source.Take(1), (Expression<Func<Product, bool>>)null);
			SetMinPrice(priceRangeModel, minPrice);
			SetMaxPrice(priceRangeModel, maxPrice);
			priceRangeModel.TaxRatePercentage = await GetTaxRatePercentageAsync(productVariant);
			if (priceRangeModel.TaxRatePercentage > 0m)
			{
				SetTaxForMinMaxPrice(priceRangeModel);
			}
		}
	}

	private void SetTaxForMinMaxPrice(PriceRangeFilterDto priceRangeModel)
	{
		if (priceRangeModel.TaxPriceIncludeTax)
		{
			if (!priceRangeModel.TaxDisplayTypeIncludingTax)
			{
				priceRangeModel.MinPrice = CalculatePrice(priceRangeModel.MinPrice, priceRangeModel.TaxRatePercentage, increase: false);
				priceRangeModel.MaxPrice = CalculatePrice(priceRangeModel.MaxPrice, priceRangeModel.TaxRatePercentage, increase: false);
			}
		}
		else if (priceRangeModel.TaxDisplayTypeIncludingTax)
		{
			priceRangeModel.MinPrice = CalculatePrice(priceRangeModel.MinPrice, priceRangeModel.TaxRatePercentage, increase: true);
			priceRangeModel.MaxPrice = CalculatePrice(priceRangeModel.MaxPrice, priceRangeModel.TaxRatePercentage, increase: true);
		}
	}

	private decimal CalculatePrice(decimal? nulablePrice, decimal percent, bool increase)
	{
		decimal num = default(decimal);
		if (nulablePrice.HasValue)
		{
			num = nulablePrice.Value;
		}
		decimal num2 = default(decimal);
		if (percent == 0m)
		{
			return num;
		}
		if (increase)
		{
			return num * (1m + percent / 100m);
		}
		return num - num / (100m + percent) * percent;
	}

	private async Task<decimal> GetTaxRatePercentageAsync(Product productVariant)
	{
		Customer customer = await _workContext.GetCurrentCustomerAsync();
		if (customer != null)
		{
			if (customer.IsTaxExempt)
			{
				return default(decimal);
			}
			if ((await _customerService.GetCustomerRolesAsync(customer, false)).Any((CustomerRole cr) => cr.TaxExempt))
			{
				return default(decimal);
			}
		}
		return await _taxServiceNopAjaxFilters.GetTaxRateForProductAsync(productVariant, 0, customer);
	}

	private void SetMaxPrice(PriceRangeFilterDto priceRangeModel, decimal? maxPrice)
	{
		if (!maxPrice.HasValue)
		{
			priceRangeModel.MaxPrice = 0m;
			return;
		}
		maxPrice = ApplyDiscount(maxPrice.Value, priceRangeModel);
		priceRangeModel.MaxPrice = maxPrice.Value;
	}

	private void SetMinPrice(PriceRangeFilterDto priceRangeModel, decimal? minPrice)
	{
		if (!minPrice.HasValue)
		{
			priceRangeModel.MinPrice = 0m;
			return;
		}
		minPrice = ApplyDiscount(minPrice.Value, priceRangeModel);
		priceRangeModel.MinPrice = minPrice.Value;
	}

	private decimal ApplyDiscount(decimal price, PriceRangeFilterDto priceRangeModel)
	{
		decimal result = default(decimal);
		decimal num = price - (decimal)((float)price * (float)priceRangeModel.MaxDiscountPercentage / 100f);
		decimal num2 = price - priceRangeModel.MaxDiscountAmount;
		if (num > 0m && num < num2)
		{
			return num;
		}
		if (num2 > 0m)
		{
			return num2;
		}
		return result;
	}

	private async Task<IQueryable<Product>> PrepareMinMaxPriceProductVariantQueryAsync(int categoryId, int manufacturerId, int vendorId)
	{
		DateTime nowUtc = DateTime.UtcNow;
		IQueryable<Product> productsQuery = await _customAclHelper.GetAvailableProductsForCurrentErpCustomerAsync();
		productsQuery = await _storeHelper.GetProductsForCurrentStoreAsync(productsQuery);
		if (manufacturerId > 0)
		{
			return await PrepareMinMaxPriceProductVariantForManufacturerQueryAsync(manufacturerId, productsQuery, nowUtc);
		}
		if (vendorId > 0)
		{
			return await PrepareMinMaxPriceProductVariantForVendorQueryAsync(vendorId, productsQuery, nowUtc);
		}
		return await PrepareMinMaxPriceProductVariantForCategoryQueryAsync(categoryId, productsQuery, nowUtc);
	}

	private async Task<IQueryable<Product>> PrepareMinMaxPriceProductVariantForManufacturerQueryAsync(int manufacturerId, IQueryable<Product> availableProductsQuery, DateTime nowUtc)
	{
		bool includeFeaturedProducts = _catalogSettings.IncludeFeaturedProductsInNormalLists;
		IList<int> groupProductIds = await _productServiceNopAjaxFilters.GetAllGroupProductIdsInManufacturerAsync(manufacturerId);
		return from _003C_003Eh__TransparentIdentifier1 in Queryable.SelectMany(availableProductsQuery.GroupJoin((IEnumerable<ProductManufacturer>)_productManufacturerRepository.Table, (Expression<Func<Product, int>>)((Product p) => ((BaseEntity)p).Id), (Expression<Func<ProductManufacturer, int>>)((ProductManufacturer pm) => pm.ProductId), (Product p, IEnumerable<ProductManufacturer> p_pm) => new { p, p_pm }), _003C_003Eh__TransparentIdentifier0 => _003C_003Eh__TransparentIdentifier0.p_pm.DefaultIfEmpty(), (_003C_003Eh__TransparentIdentifier0,   pm) => new { _003C_003Eh__TransparentIdentifier0, pm })
			where ((_003C_003Eh__TransparentIdentifier1.pm != null && _003C_003Eh__TransparentIdentifier1.pm.ManufacturerId == manufacturerId && (_003C_003Eh__TransparentIdentifier1.pm.IsFeaturedProduct == includeFeaturedProducts || !_003C_003Eh__TransparentIdentifier1.pm.IsFeaturedProduct)) || (_003C_003Eh__TransparentIdentifier1.pm == null && groupProductIds.Contains(_003C_003Eh__TransparentIdentifier1._003C_003Eh__TransparentIdentifier0.p.ParentGroupedProductId))) && _003C_003Eh__TransparentIdentifier1._003C_003Eh__TransparentIdentifier0.p.Published && !_003C_003Eh__TransparentIdentifier1._003C_003Eh__TransparentIdentifier0.p.Deleted && (!_003C_003Eh__TransparentIdentifier1._003C_003Eh__TransparentIdentifier0.p.AvailableStartDateTimeUtc.HasValue || _003C_003Eh__TransparentIdentifier1._003C_003Eh__TransparentIdentifier0.p.AvailableStartDateTimeUtc <= nowUtc) && (!_003C_003Eh__TransparentIdentifier1._003C_003Eh__TransparentIdentifier0.p.AvailableEndDateTimeUtc.HasValue || _003C_003Eh__TransparentIdentifier1._003C_003Eh__TransparentIdentifier0.p.AvailableEndDateTimeUtc >= nowUtc)
			select _003C_003Eh__TransparentIdentifier1._003C_003Eh__TransparentIdentifier0.p;
	}

	private async Task<IQueryable<Product>> PrepareMinMaxPriceProductVariantForVendorQueryAsync(int vendorId, IQueryable<Product> availableProductsQuery, DateTime nowUtc)
	{
		IList<int> groupProductIds = await _productServiceNopAjaxFilters.GetAllGroupProductIdsInVendorAsync(vendorId);
		return from _003C_003Eh__TransparentIdentifier1 in Queryable.SelectMany(availableProductsQuery.GroupJoin((IEnumerable<Vendor>)_vendorRepository.Table, (Expression<Func<Product, int>>)((Product p) => p.VendorId), (Expression<Func<Vendor, int>>)((Vendor v) => ((BaseEntity)v).Id), (Product p, IEnumerable<Vendor> p_pv) => new { p, p_pv }), _003C_003Eh__TransparentIdentifier0 => _003C_003Eh__TransparentIdentifier0.p_pv.DefaultIfEmpty(), (_003C_003Eh__TransparentIdentifier0,   v) => new { _003C_003Eh__TransparentIdentifier0, v })
			where ((_003C_003Eh__TransparentIdentifier1.v != null && ((BaseEntity)_003C_003Eh__TransparentIdentifier1.v).Id == vendorId) || (_003C_003Eh__TransparentIdentifier1.v == null && groupProductIds.Contains(_003C_003Eh__TransparentIdentifier1._003C_003Eh__TransparentIdentifier0.p.ParentGroupedProductId))) && _003C_003Eh__TransparentIdentifier1._003C_003Eh__TransparentIdentifier0.p.Published && !_003C_003Eh__TransparentIdentifier1._003C_003Eh__TransparentIdentifier0.p.Deleted && (!_003C_003Eh__TransparentIdentifier1._003C_003Eh__TransparentIdentifier0.p.AvailableStartDateTimeUtc.HasValue || _003C_003Eh__TransparentIdentifier1._003C_003Eh__TransparentIdentifier0.p.AvailableStartDateTimeUtc <= nowUtc) && (!_003C_003Eh__TransparentIdentifier1._003C_003Eh__TransparentIdentifier0.p.AvailableEndDateTimeUtc.HasValue || _003C_003Eh__TransparentIdentifier1._003C_003Eh__TransparentIdentifier0.p.AvailableEndDateTimeUtc >= nowUtc)
			select _003C_003Eh__TransparentIdentifier1._003C_003Eh__TransparentIdentifier0.p;
	}

	private async Task<IQueryable<Product>> PrepareMinMaxPriceProductVariantForCategoryQueryAsync(int categoryId, IQueryable<Product> availableProductsQuery, DateTime nowUtc)
	{
		bool showProductsFromSubcategories = _catalogSettings.ShowProductsFromSubcategories;
		bool includeFeaturedProducts = _catalogSettings.IncludeFeaturedProductsInNormalLists;
		List<int> categoryIds = new List<int> { categoryId };
		if (showProductsFromSubcategories)
		{
			List<int> collection = await _categoryService7Spikes.GetCategoryIdsByParentCategoryAsync(categoryId);
			categoryIds.AddRange(collection);
		}
		IList<int> groupProductIds = await _productServiceNopAjaxFilters.GetAllGroupProductIdsInCategoriesAsync(categoryIds);
		return from _003C_003Eh__TransparentIdentifier1 in Queryable.SelectMany(availableProductsQuery.GroupJoin((IEnumerable<ProductCategory>)_productCategoryRepository.Table, (Expression<Func<Product, int>>)((Product p) => ((BaseEntity)p).Id), (Expression<Func<ProductCategory, int>>)((ProductCategory pc) => pc.ProductId), (Product p, IEnumerable<ProductCategory> p_pc) => new { p, p_pc }), _003C_003Eh__TransparentIdentifier0 => _003C_003Eh__TransparentIdentifier0.p_pc.DefaultIfEmpty(), (_003C_003Eh__TransparentIdentifier0,   pc) => new { _003C_003Eh__TransparentIdentifier0, pc })
			where ((_003C_003Eh__TransparentIdentifier1.pc != null && categoryIds.Contains(_003C_003Eh__TransparentIdentifier1.pc.CategoryId) && _003C_003Eh__TransparentIdentifier1._003C_003Eh__TransparentIdentifier0.p.ProductTypeId != 10 && (_003C_003Eh__TransparentIdentifier1.pc.IsFeaturedProduct == includeFeaturedProducts || !_003C_003Eh__TransparentIdentifier1.pc.IsFeaturedProduct)) || (_003C_003Eh__TransparentIdentifier1.pc == null && groupProductIds.Contains(_003C_003Eh__TransparentIdentifier1._003C_003Eh__TransparentIdentifier0.p.ParentGroupedProductId))) && _003C_003Eh__TransparentIdentifier1._003C_003Eh__TransparentIdentifier0.p.Published && !_003C_003Eh__TransparentIdentifier1._003C_003Eh__TransparentIdentifier0.p.Deleted && (!_003C_003Eh__TransparentIdentifier1._003C_003Eh__TransparentIdentifier0.p.AvailableStartDateTimeUtc.HasValue || _003C_003Eh__TransparentIdentifier1._003C_003Eh__TransparentIdentifier0.p.AvailableStartDateTimeUtc <= nowUtc) && (!_003C_003Eh__TransparentIdentifier1._003C_003Eh__TransparentIdentifier0.p.AvailableEndDateTimeUtc.HasValue || _003C_003Eh__TransparentIdentifier1._003C_003Eh__TransparentIdentifier0.p.AvailableEndDateTimeUtc >= nowUtc)
			select _003C_003Eh__TransparentIdentifier1._003C_003Eh__TransparentIdentifier0.p;
	}

	private void SetDiscountAmountPercentageForCategory(int categoryId, PriceRangeFilterDto priceRangeModel)
	{
		IEnumerable<Discount> allowedDiscountsForCategory = GetAllowedDiscountsForCategory(categoryId);
		decimal num = default(decimal);
		decimal num2 = default(decimal);
		foreach (Discount item in allowedDiscountsForCategory)
		{
			if (item.DiscountAmount > num)
			{
				num = item.DiscountAmount;
			}
			if (item.DiscountPercentage > num2)
			{
				num2 = item.DiscountPercentage;
			}
		}
		priceRangeModel.MaxDiscountAmount = num;
		priceRangeModel.MaxDiscountPercentage = num2;
	}

	private IEnumerable<Discount> GetAllowedDiscountsForCategory(int categoryId)
	{
		return new List<Discount>();
	}
}
