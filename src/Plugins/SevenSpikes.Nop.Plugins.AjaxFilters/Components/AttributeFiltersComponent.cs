using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Stores;
using Nop.Services.Customers;
using Nop.Services.Media;
using Nop.Web.Framework.Components;
using Nop.Web.Infrastructure.Cache;
using Nop.Web.Models.Media;
using SevenSpikes.Nop.Core.Helpers;
using SevenSpikes.Nop.Framework.Components;
using SevenSpikes.Nop.Plugins.AjaxFilters.Domain;
using SevenSpikes.Nop.Plugins.AjaxFilters.Helpers;
using SevenSpikes.Nop.Plugins.AjaxFilters.Infrastructure.Cache;
using SevenSpikes.Nop.Plugins.AjaxFilters.Models.AttributeFilter;
using SevenSpikes.Nop.Plugins.AjaxFilters.Services;
using SevenSpikes.Nop.Services.Catalog;
using SevenSpikes.Nop.Services.Catalog.DTO;
using SevenSpikes.Nop.Services.Helpers;

namespace SevenSpikes.Nop.Plugins.AjaxFilters.Components;

[ViewComponent(Name = "NopAjaxFiltersAttributeFilters")]
public class AttributeFiltersComponent : Base7SpikesComponent
{
	private readonly IAclHelper _aclHelper;

	private readonly IConvertToDictionaryHelper _convertToDictionaryHelper;

	private readonly IProductAttributeServiceAjaxFilters _productAttributeServiceAjaxFilters;

	private readonly IProductAttributeService7Spikes _productAttributeService7Spikes;

	private readonly IPictureService _pictureService;

	private readonly ISearchQueryStringHelper _searchQueryStringHelper;

	private readonly IStaticCacheManager _staticCacheManager;

	private readonly IStoreContext _storeContext;

	private readonly IWebHelper _webHelper;

	private readonly IWorkContext _workContext;

	private readonly CatalogSettings _catalogSettings;

	private readonly MediaSettings _mediaSettings;

	private readonly NopAjaxFiltersSettings _nopAjaxFiltersSettings;

	private readonly ICustomerService _customerService;

	public AttributeFiltersComponent(IAclHelper aclHelper, IConvertToDictionaryHelper convertToDictionaryHelper, IProductAttributeServiceAjaxFilters productAttributeServiceAjaxFilters, IProductAttributeService7Spikes productAttributeService7Spikes, IPictureService pictureService, ISearchQueryStringHelper searchQueryStringHelper, IStaticCacheManager staticCacheManager, IStoreContext storeContext, IWebHelper webHelper, IWorkContext workContext, CatalogSettings catalogSettings, MediaSettings mediaSettings, NopAjaxFiltersSettings nopAjaxFiltersSettings, ICustomerService customerService)
	{
		_aclHelper = aclHelper;
		_convertToDictionaryHelper = convertToDictionaryHelper;
		_productAttributeServiceAjaxFilters = productAttributeServiceAjaxFilters;
		_productAttributeService7Spikes = productAttributeService7Spikes;
		_pictureService = pictureService;
		_searchQueryStringHelper = searchQueryStringHelper;
		_staticCacheManager = staticCacheManager;
		_storeContext = storeContext;
		_webHelper = webHelper;
		_workContext = workContext;
		_catalogSettings = catalogSettings;
		_mediaSettings = mediaSettings;
		_nopAjaxFiltersSettings = nopAjaxFiltersSettings;
		_customerService = customerService;
	}

	public async Task<IViewComponentResult> InvokeAsync(int categoryId, int manufacturerId, int vendorId)
	{
		AttributeFilterModel7Spikes attributeFilterModel7Spikes = await GetProductAttributeFilterInternalAsync(categoryId, manufacturerId, vendorId);
		if (attributeFilterModel7Spikes.AttributeFilterGroups.Count == 0)
		{
			return ((ViewComponent)(object)this).Content(string.Empty);
		}
		return ((NopViewComponent)this).View<AttributeFilterModel7Spikes>("AttributeFilter", attributeFilterModel7Spikes);
	}

	private async Task<AttributeFilterModel7Spikes> GetProductAttributeFilterInternalAsync(int categoryId, int manufacturerId, int vendorId)
	{
		SearchQueryStringParameters searchQueryStringParameters = _searchQueryStringHelper.GetQueryStringParameters(((ViewComponent)(object)this).Request.QueryString.Value);
		AttributeFilterModel7Spikes attributeFilterModel7Spikes = new AttributeFilterModel7Spikes();
		if (searchQueryStringParameters.IsOnSearchPage)
		{
			IList<ProductAttribute> productAttributes = new List<ProductAttribute>();
			ICustomerService customerService = _customerService;
			string customerRolesIds = string.Join(",", await customerService.GetCustomerRoleIdsAsync(await _workContext.GetCurrentCustomerAsync(), false));
			IStaticCacheManager staticCacheManager = _staticCacheManager;
			CacheKey nOP_AJAX_FILTERS_ATTRIBUTE_OPTION_IDS_KEY = NopAjaxFiltersModelCacheEventConsumer.NOP_AJAX_FILTERS_ATTRIBUTE_OPTION_IDS_KEY;
			object obj = searchQueryStringParameters.SearchCategoryId;
			object obj2 = searchQueryStringParameters.SearchManufacturerId;
			object obj3 = searchQueryStringParameters.SearchVendorId;
			object obj4 = ((BaseEntity)(await _workContext.GetWorkingLanguageAsync())).Id;
			object obj5 = customerRolesIds;
			Store val = await _storeContext.GetCurrentStoreAsync();
			CacheKey val2 = ((ICacheKeyService)staticCacheManager).PrepareKey(nOP_AJAX_FILTERS_ATTRIBUTE_OPTION_IDS_KEY, new object[12]
			{
				obj,
				obj2,
				obj3,
				obj4,
				obj5,
				((BaseEntity)val).Id,
				searchQueryStringParameters.Keyword,
				searchQueryStringParameters.PriceFrom,
				searchQueryStringParameters.PriceTo,
				searchQueryStringParameters.IncludeSubcategories,
				searchQueryStringParameters.SearchInProductDescriptions,
				searchQueryStringParameters.AdvancedSearch
			});
			IList<int> productAttributeMappingIds = await _staticCacheManager.GetAsync<IList<int>>(val2, (Func<Task<IList<int>>>)(async () => (IList<int>)null));
			if (productAttributeMappingIds != null && productAttributeMappingIds.Any())
			{
				productAttributes = await _productAttributeService7Spikes.GetAllProductAttributesByProductAttributeMappingIdsAsync(productAttributeMappingIds);
			}
			if (productAttributes.Count > 0)
			{
				attributeFilterModel7Spikes = await GetAttributeFilterModel7SpikesAsync(categoryId, manufacturerId, vendorId, productAttributes, isOnSearchPage: true, productAttributeMappingIds);
			}
		}
		else
		{
			IStaticCacheManager staticCacheManager = _staticCacheManager;
			CacheKey nOP_AJAX_FILTERS_ATTRIBUTE_OPTION_IDS_KEY = NopAjaxFiltersModelCacheEventConsumer.NOP_AJAX_FILTERS_ATTRIBUTE_FILTERS_MODEL_KEY;
			object obj5 = categoryId;
			object obj4 = manufacturerId;
			object obj3 = vendorId;
			object obj2 = ((BaseEntity)(await _workContext.GetWorkingLanguageAsync())).Id;
			object obj = await _aclHelper.GetAllowedCustomerRolesIdsAsync();
			Store val = await _storeContext.GetCurrentStoreAsync();
			CacheKey val3 = ((ICacheKeyService)staticCacheManager).PrepareKeyForDefaultCache(nOP_AJAX_FILTERS_ATTRIBUTE_OPTION_IDS_KEY, new object[6]
			{
				obj5,
				obj4,
				obj3,
				obj2,
				obj,
				((BaseEntity)val).Id
			});
			attributeFilterModel7Spikes = await _staticCacheManager.GetAsync<AttributeFilterModel7Spikes>(val3, (Func<Task<AttributeFilterModel7Spikes>>)async delegate
			{
				IList<ProductAttribute> list = new List<ProductAttribute>();
				if (categoryId > 0)
				{
					list = await _productAttributeService7Spikes.GetAllProductAttributesByCategoryIdAsync(categoryId, _catalogSettings.ShowProductsFromSubcategories);
				}
				else if (manufacturerId > 0)
				{
					list = await _productAttributeService7Spikes.GetAllProductAttributesByManufacturerIdAsync(manufacturerId);
				}
				else if (vendorId > 0)
				{
					list = await _productAttributeService7Spikes.GetAllProductAttributesByVendorIdAsync(vendorId);
				}
				AttributeFilterModel7Spikes result = new AttributeFilterModel7Spikes();
				if (list.Count > 0)
				{
					result = await GetAttributeFilterModel7SpikesAsync(categoryId, manufacturerId, vendorId, list);
				}
				return result;
			});
		}
		return attributeFilterModel7Spikes;
	}

	private async Task<AttributeFilterModel7Spikes> GetAttributeFilterModel7SpikesAsync(int categoryId, int manufacturerId, int vendorId, IList<ProductAttribute> productAttributesParameter, bool isOnSearchPage = false, IList<int> productAttributeMappingIds = null)
	{
		AttributeFilterModel7Spikes attributeFilterModel7Spikes = new AttributeFilterModel7Spikes
		{
			CategoryId = categoryId,
			ManufacturerId = manufacturerId,
			VendorId = vendorId
		};
		IList<ProductAttributeProductAttributeValueDTO> productAttributeProductAttributeValueDtos = null;
		if (isOnSearchPage && productAttributeMappingIds != null)
		{
			productAttributeProductAttributeValueDtos = await _productAttributeService7Spikes.GetProductAttributeProductAttributeValueDtosByProductAttributeMappingIdsAsync(productAttributeMappingIds);
		}
		Dictionary<int, AttributeFilterGroup> attributeFilterGroupsDictionary = new Dictionary<int, AttributeFilterGroup>();
		IList<ProductAttribute> list = new List<ProductAttribute>(productAttributesParameter);
		IList<string> activeProductAttributes = _nopAjaxFiltersSettings.ActiveProductAttributes;
		if (activeProductAttributes.Count > 0)
		{
			IList<KeyValuePair<int, ProductAttribute>> list2 = new List<KeyValuePair<int, ProductAttribute>>();
			IDictionary<int, int> dictionary = _convertToDictionaryHelper.CreateDictionaryFromSemicolonSeparatedPairs<int, int>(activeProductAttributes);
			foreach (ProductAttribute item in list)
			{
				int key = (dictionary.ContainsKey(((BaseEntity)item).Id) ? dictionary[((BaseEntity)item).Id] : int.MaxValue);
				list2.Add(new KeyValuePair<int, ProductAttribute>(key, item));
			}
			list = (from x in list2
				orderby x.Key
				select x.Value).ToList();
		}
		if (list.Count > _nopAjaxFiltersSettings.NumberOfAttributeFilters)
		{
			list = list.Take(_nopAjaxFiltersSettings.NumberOfAttributeFilters).ToList();
		}
		foreach (ProductAttribute productAttribute in list)
		{
			attributeFilterGroupsDictionary.TryGetValue(((BaseEntity)productAttribute).Id, out var attributeFilterGroup);
			if (attributeFilterGroup == null)
			{
				AttributeFilterGroup attributeFilterGroup2 = new AttributeFilterGroup
				{
					Id = ((BaseEntity)productAttribute).Id
				};
				AttributeFilterGroup attributeFilterGroup3 = attributeFilterGroup2;
				attributeFilterGroup3.Name = await base.LocalizationService.GetLocalizedAsync<ProductAttribute, string>(productAttribute, (Expression<Func<ProductAttribute, string>>)((ProductAttribute x) => x.Name), (int?)null, true, true);
				attributeFilterGroup = attributeFilterGroup2;
				attributeFilterModel7Spikes.AttributeFilterGroups.Add(attributeFilterGroup);
				attributeFilterGroupsDictionary.Add(attributeFilterGroup.Id, attributeFilterGroup);
			}
			if (isOnSearchPage)
			{
				if (productAttributeProductAttributeValueDtos != null)
				{
					AttributeFilterGroup attributeFilterGroup2 = attributeFilterGroup;
					attributeFilterGroup2.FilterItems = await GetAttributeFilterItemsForProductAttributeAsync(((BaseEntity)productAttribute).Id, productAttributeProductAttributeValueDtos);
				}
			}
			else
			{
				AttributeFilterGroup attributeFilterGroup2 = attributeFilterGroup;
				attributeFilterGroup2.FilterItems = await GetAttributeFilterItemsForEntityAndProductAttributeIdAsync(categoryId, manufacturerId, vendorId, ((BaseEntity)productAttribute).Id);
			}
			if (attributeFilterGroup.FilterItems.Count == 0)
			{
				attributeFilterModel7Spikes.AttributeFilterGroups.Remove(attributeFilterGroup);
			}
			attributeFilterGroup = null;
		}
		return attributeFilterModel7Spikes;
	}

	private async Task<IList<AttributeFilterItem>> GetAttributeFilterItemsForEntityAndProductAttributeIdAsync(int categoryId, int manufacturerId, int vendorId, int productAttributeId)
	{
		IList<ProductAttributeValue> productAttributeValuesLocalized = new List<ProductAttributeValue>();
		if (categoryId > 0)
		{
			productAttributeValuesLocalized = await _productAttributeService7Spikes.GetAllProductVariantAttributeValuesByProductAttributeIdAndCategoryIdAsync(productAttributeId, categoryId, _catalogSettings.ShowProductsFromSubcategories);
		}
		else if (manufacturerId > 0)
		{
			productAttributeValuesLocalized = await _productAttributeService7Spikes.GetAllProductVariantAttributeValuesByProductAttributeIdAndManufacturerIdAsync(productAttributeId, manufacturerId);
		}
		else if (vendorId > 0)
		{
			productAttributeValuesLocalized = await _productAttributeService7Spikes.GetAllProductVariantAttributeValuesByProductAttributeIdAndVendorIdAsync(productAttributeId, vendorId);
		}
		return await PrepareAttributeFilterItemsByProductAttributeValuesAsync(productAttributeValuesLocalized, productAttributeId);
	}

	private async Task<IList<AttributeFilterItem>> GetAttributeFilterItemsForProductAttributeAsync(int productAttributeId, IEnumerable<ProductAttributeProductAttributeValueDTO> productAttributeProductAttributeValueDtos)
	{
		IEnumerable<ProductAttributeValue> productAttributeValuesLocalized = from papav in productAttributeProductAttributeValueDtos
			where papav.ProductAttributeId == productAttributeId
			select papav.ProductAttributeValue;
		return await PrepareAttributeFilterItemsByProductAttributeValuesAsync(productAttributeValuesLocalized, productAttributeId);
	}

	private async Task<IList<AttributeFilterItem>> PrepareAttributeFilterItemsByProductAttributeValuesAsync(IEnumerable<ProductAttributeValue> productAttributeValuesLocalized, int productAttributeId)
	{
		IList<AttributeFilterItem> attributeFilterItems = new List<AttributeFilterItem>();
		foreach (ProductAttributeValue productVariantAttributeValue in productAttributeValuesLocalized)
		{
			string productVariantAttributeValueNameLocalized = productVariantAttributeValue.Name.Trim();
			AttributeFilterItem attributeFilterItem = attributeFilterItems.FirstOrDefault((AttributeFilterItem x) => x.Name.ToLower().Trim() == productVariantAttributeValueNameLocalized.ToLower());
			if (attributeFilterItem == null)
			{
				if (!string.IsNullOrEmpty(productVariantAttributeValue.ColorSquaresRgb))
				{
					attributeFilterItem = new AttributeFilterItem
					{
						ValueId = ((BaseEntity)productVariantAttributeValue).Id,
						Name = productVariantAttributeValueNameLocalized,
						ColorSquaresRgb = productVariantAttributeValue.ColorSquaresRgb.ToLower().Trim()
					};
				}
				else if (productVariantAttributeValue.ImageSquaresPictureId > 0)
				{
					IStaticCacheManager staticCacheManager = _staticCacheManager;
					CacheKey productAttributeImageSquarePictureModelKey = NopModelCacheDefaults.ProductAttributeImageSquarePictureModelKey;
					object obj = productVariantAttributeValue.ImageSquaresPictureId;
					object obj2 = _webHelper.IsCurrentConnectionSecured();
					Store val = await _storeContext.GetCurrentStoreAsync();
					CacheKey val2 = ((ICacheKeyService)staticCacheManager).PrepareKeyForDefaultCache(productAttributeImageSquarePictureModelKey, new object[3]
					{
						obj,
						obj2,
						((BaseEntity)val).Id
					});
					PictureModel val3 = await _staticCacheManager.GetAsync<PictureModel>(val2, (Func<Task<PictureModel>>)async delegate
					{
						Picture imageSquaresPicture = await _pictureService.GetPictureByIdAsync(productVariantAttributeValue.ImageSquaresPictureId);
						if (imageSquaresPicture != null)
						{
							PictureModel val4 = new PictureModel();
							PictureModel val5 = val4;
							val5.FullSizeImageUrl = await _pictureService.GetPictureUrlAsync(((BaseEntity)imageSquaresPicture).Id, 0, true, (string)null, (PictureType)1);
							PictureModel val6 = val4;
							val6.ImageUrl = await _pictureService.GetPictureUrlAsync(((BaseEntity)imageSquaresPicture).Id, _mediaSettings.ImageSquarePictureSize, true, (string)null, (PictureType)1);
							return val4;
						}
						return new PictureModel();
					});
					attributeFilterItem = new AttributeFilterItem
					{
						ValueId = ((BaseEntity)productVariantAttributeValue).Id,
						Name = productVariantAttributeValueNameLocalized,
						ImageSquaresUrl = val3.ImageUrl
					};
				}
				else
				{
					attributeFilterItem = new AttributeFilterItem
					{
						ValueId = ((BaseEntity)productVariantAttributeValue).Id,
						Name = productVariantAttributeValueNameLocalized
					};
				}
				attributeFilterItem.AttributeId = productAttributeId;
				attributeFilterItems.Add(attributeFilterItem);
			}
			else if (!string.IsNullOrEmpty(productVariantAttributeValue.ColorSquaresRgb))
			{
				attributeFilterItem.ColorSquaresRgb = productVariantAttributeValue.ColorSquaresRgb.ToLower().Trim();
			}
			attributeFilterItem.ProductVariantAttributeIds.Add(productVariantAttributeValue.ProductAttributeMappingId);
		}
		return await _productAttributeServiceAjaxFilters.GetSortedAttributeValuesBasedOnTheirPredefinedDisplayOrderAsync(attributeFilterItems);
	}
}
